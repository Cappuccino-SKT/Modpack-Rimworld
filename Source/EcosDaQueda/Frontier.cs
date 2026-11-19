using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Fronteiras vivas, F2 (MUNDO.md): parâmetros do crescimento do território e da cidade nova.</summary>
    public class FrontierDef : Def
    {
        /// <summary>Só no mundo fixo (preset do Worldbuilder com FixedWorldDef). No mundo aleatório o Hegemony fica como é.</summary>
        public bool onlyFixedWorld = true;
        /// <summary>Raio permitido (distância do Hegemony, ≈ tiles em terreno plano) de uma cidade nova.</summary>
        public float startRadius = 1.5f;
        /// <summary>Dias por tile de crescimento: cidade pobre (prosperidade ≤ poorProsperity) e rica (≥ richProsperity).</summary>
        public float daysPerTilePoor = 5f, daysPerTileRich = 3f;
        public float poorProsperity = 10000f, richProsperity = 50000f;
        /// <summary>O teto é o raio natural do Hegemony (pela prosperidade) × isto, na capital.</summary>
        public float capitalCapMultiplier = 1.3f;
        /// <summary>Multiplicador do teto por facção (ex.: terreno ruim em volta das cidades).</summary>
        public List<FrontierFactionEntry> factions = new List<FrontierFactionEntry>();
        /// <summary>Cidade nova: a esta distância (tiles) da cidade que bateu no teto, com folga de newCitySlack.</summary>
        public float newCityDistance = 6f, newCitySlack = 3f;
        /// <summary>Distância mínima (tiles) da cidade nova até qualquer outro assentamento.</summary>
        public float newCityMinSpacing = 4f;
        /// <summary>Intervalo mínimo entre cidades novas da mesma facção.</summary>
        public float newCityCooldownDays = 20f;
        /// <summary>Máximo de cidades: potências e as outras facções da lista do mundo fixo.</summary>
        public int maxCitiesPower = 6, maxCitiesMinor = 2;

        public static FrontierDef Active => DefDatabase<FrontierDef>.AllDefsListForReading.FirstOrDefault();

        public FrontierFactionEntry For(FactionDef f) => f == null ? null : factions.FirstOrDefault(e => e.faction == f);
    }

    public class FrontierFactionEntry
    {
        public FactionDef faction;
        public float capMultiplier = 1f;
        /// <summary>Sem cidade nova (ex.: facção que não se espalha).</summary>
        public bool noNewCities;
    }

    /// <summary>
    /// F2: o território do Hegemony nasce pequeno e cresce. Cada cidade tem um raio permitido R (começa em startRadius,
    /// cresce 1 tile a cada 3–5 dias, mais rápido com prosperidade) e um teto C (raio natural do Hegemony × facção × capital).
    /// O módulo do Hegemony troca o multiplicador de decaimento da cidade (TerritorySolver.SettlementDecay) por
    /// m = 2·log2(√P ÷ 10) ÷ (1,5 · min(R, C)): a força chega ao limiar exatamente nesse raio. R sobe em degraus de 1 tile,
    /// então o Hegemony só recalcula quando a fronteira muda de fato (o multiplicador entra na assinatura dele).
    /// Quando uma cidade chega ao teto, a facção funda outra a ~6 tiles (intervalo e limite de cidades no Def).
    /// </summary>
    public class WorldComponent_EQFrontier : WorldComponent
    {
        /// <summary>Ligado no mundo fixo (FixedWorld.Apply). Salvo: depois de carregar, o preset pode não estar mais em memória.</summary>
        public bool active;
        /// <summary>Progresso do crescimento por cidade (ID do objeto do mundo → tiles além do inicial, fracionário).</summary>
        public Dictionary<int, float> growth = new Dictionary<int, float>();
        /// <summary>Capitais (ID do objeto do mundo), postas pelo mundo fixo.</summary>
        public List<int> capitals = new List<int>();
        /// <summary>Última cidade nova por facção (defName → tique).</summary>
        public Dictionary<string, int> lastNewCity = new Dictionary<string, int>();
        public List<string> log = new List<string>();

        public WorldComponent_EQFrontier(World world) : base(world) { }

        public static WorldComponent_EQFrontier Current => Find.World?.GetComponent<WorldComponent_EQFrontier>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref active, "active");
            Scribe_Collections.Look(ref growth, "growth", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref capitals, "capitals", LookMode.Value);
            Scribe_Collections.Look(ref lastNewCity, "lastNewCity", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref log, "log", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                growth = growth ?? new Dictionary<int, float>();
                capitals = capitals ?? new List<int>();
                lastNewCity = lastNewCity ?? new Dictionary<string, int>();
                log = log ?? new List<string>();
            }
        }

        public override void WorldComponentTick()
        {
            if (!Enabled) return;
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 1234) AdvanceDays(1f);
        }

        public bool Enabled
        {
            get
            {
                FrontierDef d = FrontierDef.Active;
                return d != null && (active || !d.onlyFixedWorld) && EQTerritory.ProsperityProvider != null;
            }
        }

        public static bool Managed(Settlement s) => s != null && !s.Destroyed && s.Faction != null && !s.Faction.IsPlayer;

        /// <summary>Avança o crescimento de todas as cidades e tenta as cidades novas (um passo por dia).</summary>
        public void AdvanceDays(float days)
        {
            FrontierDef d = FrontierDef.Active;
            if (d == null) return;
            foreach (Settlement s in Find.WorldObjects.Settlements.Where(Managed).ToList())
            {
                float p = EQTerritory.ProsperityProvider?.Invoke(s) ?? -1f;
                if (p <= 0f) continue;
                float t = Mathf01((p - d.poorProsperity) / Math.Max(1f, d.richProsperity - d.poorProsperity));
                float daysPerTile = d.daysPerTilePoor + (d.daysPerTileRich - d.daysPerTilePoor) * t;
                growth.TryGetValue(s.ID, out float g);
                // Parado no teto (não acumula além: senão uma cidade empobrecida "pularia" de volta).
                if (d.startRadius + g >= Cap(s, p, 1f) + 1f) continue;
                growth[s.ID] = g + days / Math.Max(0.1f, daysPerTile);
            }
            TryNewCities(d);
        }

        private static float Mathf01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        public bool IsCapital(Settlement s) => s != null && capitals.Contains(s.ID);

        /// <summary>Teto do raio: raio natural do Hegemony (com o multiplicador original dos prédios) × facção × capital.</summary>
        public float Cap(Settlement s, float prosperity, float originalDecay)
        {
            FrontierDef d = FrontierDef.Active;
            float cap = EQTerritory.NaturalRadius(prosperity, originalDecay);
            float mult = d?.For(s.Faction?.def)?.capMultiplier ?? 1f;
            if (IsCapital(s)) mult *= d?.capitalCapMultiplier ?? 1f;
            return cap * mult;
        }

        /// <summary>Raio permitido agora (degraus de 1 tile).</summary>
        public float Allowed(Settlement s)
        {
            FrontierDef d = FrontierDef.Active;
            growth.TryGetValue(s.ID, out float g);
            return (d?.startRadius ?? 1.5f) + (float)Math.Floor(g);
        }

        /// <summary>Multiplicador de decaimento que o Hegemony usa para esta cidade (o original, se não for nosso caso).</summary>
        public float DecayFor(Settlement s, float prosperity, float originalDecay)
        {
            if (!Enabled || !Managed(s) || prosperity <= 0f) return originalDecay;
            double strength = Math.Sqrt(prosperity) / EQTerritory.Threshold;
            if (strength <= 1.0) return originalDecay;
            float radius = Math.Min(Allowed(s), Cap(s, prosperity, originalDecay));
            if (radius <= 0.05f) return originalDecay;
            return (float)(2.0 * Math.Log(strength, 2.0) / (EQTerritory.DistancePower * radius));
        }

        /// <summary>Raio efetivo (para relatório).</summary>
        public float Effective(Settlement s, float prosperity) => Math.Min(Allowed(s), Cap(s, prosperity, 1f));

        // ---------- Cidade nova ----------

        private int MaxCities(Faction f, FrontierDef d) => EQAllegiance.IsPower(f) ? d.maxCitiesPower : d.maxCitiesMinor;

        private bool Expands(Faction f, FrontierDef d)
        {
            if (f == null || f.IsPlayer || f.defeated) return false;
            if (d.For(f.def)?.noNewCities == true) return false;
            FixedWorldDef fw = EQFixedWorld.ActiveDefAnyway();
            return fw != null && fw.settlements.Any(x => x.faction == f.def);
        }

        public void TryNewCities(FrontierDef d)
        {
            int now = Find.TickManager.TicksGame;
            var all = Find.WorldObjects.Settlements.Where(s => !s.Destroyed).ToList();
            List<KeyValuePair<PlanetTile, Faction>> claims = null;
            foreach (var group in all.Where(Managed).GroupBy(s => s.Faction).ToList())
            {
                Faction f = group.Key;
                if (!Expands(f, d)) continue;
                if (group.Count() >= MaxCities(f, d)) continue;
                if (lastNewCity.TryGetValue(f.def.defName, out int last) && now - last < d.newCityCooldownDays * GenDate.TicksPerDay) continue;
                Settlement full = group.FirstOrDefault(s =>
                {
                    float p = EQTerritory.ProsperityProvider?.Invoke(s) ?? -1f;
                    return p > 0f && Allowed(s) >= Cap(s, p, 1f);
                });
                if (full == null) continue;
                if (claims == null)
                {
                    claims = new List<KeyValuePair<PlanetTile, Faction>>();
                    try { claims = EQTerritory.ClaimsProvider?.Invoke() ?? claims; } catch (Exception) { }
                }
                PlanetTile tile = FindSite(full, d, all, claims);
                if (!tile.Valid) { lastNewCity[f.def.defName] = now - (int)(d.newCityCooldownDays * GenDate.TicksPerDay) + GenDate.TicksPerDay * 5; continue; }
                Settlement made = Found(f, tile);
                if (made == null) continue;
                all.Add(made);
                lastNewCity[f.def.defName] = now;
                string note = "dia " + (now / (float)GenDate.TicksPerDay).ToString("0.0") + ": " + f.Name + " fundou " + made.Name + " (tile " + tile.tileId + ", a "
                              + Find.WorldGrid.ApproxDistanceInTiles(full.Tile, tile).ToString("0.0") + " de " + full.Name + ")";
                log.Add(note);
                if (log.Count > 40) log.RemoveAt(0);
                Log.Message("[Ecos da Queda] Fronteira: " + note);
                Messages.Message(f.Name + " fundou uma cidade nova: " + made.Name + ". A fronteira avança.", made, MessageTypeDefOf.NeutralEvent, false);
            }
        }

        private static PlanetTile FindSite(Settlement from, FrontierDef d, List<Settlement> all, List<KeyValuePair<PlanetTile, Faction>> claims)
        {
            World w = Find.World;
            Faction f = EQUtil.Canonical(from.Faction);
            var owner = new Dictionary<PlanetTile, Faction>();
            foreach (var c in claims) owner[c.Key] = EQUtil.Canonical(c.Value);
            PlanetTile best = PlanetTile.Invalid;
            float bestScore = float.MinValue;
            var seen = new HashSet<PlanetTile>();
            var queue = new Queue<PlanetTile>();
            var tmp = new List<PlanetTile>();
            queue.Enqueue(from.Tile);
            seen.Add(from.Tile);
            float maxD = d.newCityDistance + d.newCitySlack;
            while (queue.Count > 0)
            {
                PlanetTile t = queue.Dequeue();
                float dist = w.grid.ApproxDistanceInTiles(from.Tile, t);
                if (dist > maxD + 1f) continue;
                if (dist >= d.newCityDistance - 0.5f && dist <= maxD && EQFixedWorld.CanHoldSettlement(t, w)
                    && !all.Any(o => o.Tile.Layer == t.Layer && w.grid.ApproxDistanceInTiles(o.Tile, t) < d.newCityMinSpacing))
                {
                    owner.TryGetValue(t, out Faction o2);
                    // Na própria terra ou em terra de ninguém; nunca dentro do território de outra facção.
                    if (o2 == null || o2 == f)
                    {
                        float score = (o2 == f ? 10f : 0f) - Math.Abs(dist - d.newCityDistance) * 2f
                                      + (w.grid[t] is SurfaceTile st && !st.Roads.NullOrEmpty() ? 2f : 0f)
                                      + Rand.ValueSeeded(t.tileId ^ from.ID);
                        if (score > bestScore) { bestScore = score; best = t; }
                    }
                }
                tmp.Clear();
                w.grid.GetTileNeighbors(t, tmp);
                foreach (PlanetTile n in tmp)
                    if (n.Layer == t.Layer && seen.Add(n)) queue.Enqueue(n);
            }
            return best;
        }

        private static Settlement Found(Faction f, PlanetTile tile)
        {
            try
            {
                var s = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                s.SetFaction(f);
                s.Tile = tile;
                s.Name = SettlementNameGenerator.GenerateSettlementName(s, f.def.settlementNameMaker);
                Find.WorldObjects.Add(s);
                return s;
            }
            catch (Exception ex)
            {
                Log.Warning("[Ecos da Queda] Fronteira: falha ao fundar cidade de " + f?.Name + ": " + ex.Message);
                return null;
            }
        }

        /// <summary>Relatório para o executor de testes.</summary>
        public string Report()
        {
            var sb = new StringBuilder();
            sb.Append("ativo=" + Enabled + " capitais=" + capitals.Count);
            foreach (var g in Find.WorldObjects.Settlements.Where(Managed).GroupBy(s => EQUtil.Canonical(s.Faction)).OrderBy(g => g.Key.def.defName))
            {
                sb.Append("\n  " + g.Key.def.defName.Replace("EQ_Faction_", "") + " (" + g.Count() + "):");
                foreach (Settlement s in g.Take(8))
                {
                    float p = EQTerritory.ProsperityProvider?.Invoke(s) ?? -1f;
                    sb.Append(" " + s.Name + (IsCapital(s) ? "*" : "") + " R=" + Allowed(s).ToString("0.0") + " teto=" + Cap(s, p, 1f).ToString("0.0")
                              + " m=" + DecayFor(s, p, 1f).ToString("0.00") + ";");
                }
            }
            if (log.Count > 0) sb.Append("\n  cidades novas: " + string.Join(" | ", log));
            return sb.ToString();
        }
    }
}
