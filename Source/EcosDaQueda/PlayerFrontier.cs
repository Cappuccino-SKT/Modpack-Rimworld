using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Fronteiras vivas, F4 (MUNDO.md): parâmetros da fronteira do jogador.</summary>
    public class PlayerFrontierDef : Def
    {
        /// <summary>Raio inicial da colônia (distância do Hegemony) e ritmo: dias por tile, colônia pobre e rica (riqueza do mapa).</summary>
        public float startRadius = 2f;
        public float daysPerTilePoor = 5f, daysPerTileRich = 3f;
        public float poorWealth = 20000f, richWealth = 150000f;
        /// <summary>Teto: raio natural do Hegemony pela riqueza × isto.</summary>
        public float capMultiplier = 1f;

        /// <summary>Marco de fronteira: raio em volta dele, raio depois de markerGrowDays, dias de demarcação até valer.</summary>
        public float markerRadius = 1.5f, markerRadiusGrown = 2.5f;
        public float markerGrowDays = 10f, markerSetupDays = 2f;
        /// <summary>Marcos por colônia; distância máxima da colônia; distância máxima até um tile do jogador (a borda).</summary>
        public int markersPerColony = 3;
        public float markerMaxDistance = 10f, markerMaxFromBorder = 1.5f;
        /// <summary>Prata tirada do mapa da colônia por marco.</summary>
        public int markerCostSilver = 150;

        /// <summary>Cidade nova: carta com o lugar sugerido quando a colônia chega ao teto; bônus se o jogador fundar perto.</summary>
        public float suggestCooldownDays = 30f, suggestValidDays = 60f, suggestNearTiles = 2f;
        public float newColonyBonusTiles = 1f;
        public int maxPlayerColonies = 4;

        /// <summary>Tributo do território: a cada intervalo, o rendimento acumulado vira itens (× factor) por drop pod.</summary>
        public float tributeIntervalDays = 5f, tributeFactor = 0.6f, prosperityToSilver = 0.3f;
        public int tributeMinTiles = 3;

        public static PlayerFrontierDef Active => DefDatabase<PlayerFrontierDef>.AllDefsListForReading.FirstOrDefault();
    }

    /// <summary>
    /// Marco de fronteira do jogador: uma estaca, uma placa e a convicção de que isso basta. Depois da demarcação vira
    /// uma fonte de território do jogador no Hegemony (módulo do Hegemony, postfix em TerritorySolver.Sources),
    /// com raio pequeno em volta: é o jeito de puxar a fronteira para um lado.
    /// </summary>
    public class WorldObject_EQFrontierMarker : WorldObject
    {
        public Settlement colony;
        public int placedTick;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref colony, "colony");
            Scribe_Values.Look(ref placedTick, "placedTick");
        }

        public float AgeDays => (Find.TickManager.TicksGame - placedTick) / (float)GenDate.TicksPerDay;

        public bool Active
        {
            get
            {
                PlayerFrontierDef d = PlayerFrontierDef.Active;
                return d != null && colony != null && !colony.Destroyed && colony.Faction == Faction.OfPlayer && AgeDays >= d.markerSetupDays;
            }
        }

        public float Radius
        {
            get
            {
                PlayerFrontierDef d = PlayerFrontierDef.Active;
                if (d == null) return 1.5f;
                return AgeDays >= d.markerSetupDays + d.markerGrowDays ? d.markerRadiusGrown : d.markerRadius;
            }
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder(base.GetInspectString());
            if (sb.Length > 0) sb.AppendLine();
            sb.Append("Colônia: " + (colony?.LabelCap ?? "nenhuma (marco abandonado)"));
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            if (d != null && AgeDays < d.markerSetupDays)
                sb.Append("\nEm demarcação: faltam " + (d.markerSetupDays - AgeDays).ToString("0.0") + " dias de estacas, cordas e discussões.");
            else
                sb.Append("\nReivindica " + Radius.ToString("0.0") + " tiles em volta.");
            return sb.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos()) yield return g;
            if (Faction != Faction.OfPlayer) yield break;
            yield return new Command_Action
            {
                defaultLabel = "Arrancar marco",
                defaultDesc = "Arranca a estaca e devolve a terra a quem passar por ela primeiro. A prata não volta.",
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct"),
                action = () =>
                {
                    if (!Destroyed) Destroy();
                }
            };
        }

        protected override void Tick()
        {
            base.Tick();
            // Colônia perdida: o marco cai junto (verificação barata, a cada ~1/4 dia).
            if (this.IsHashIntervalTick(15000) && (colony == null || colony.Destroyed || colony.Faction != Faction.OfPlayer))
                Destroy();
        }
    }

    /// <summary>Rendimento acumulado do território do jogador entre dois tributos (por colônia).</summary>
    public class PlayerYieldStock : IExposable
    {
        public int colonyId;
        public float food, wood, stone, metal, components, prosperity;
        public int days, tiles;

        public void ExposeData()
        {
            Scribe_Values.Look(ref colonyId, "colonyId");
            Scribe_Values.Look(ref food, "food");
            Scribe_Values.Look(ref wood, "wood");
            Scribe_Values.Look(ref stone, "stone");
            Scribe_Values.Look(ref metal, "metal");
            Scribe_Values.Look(ref components, "components");
            Scribe_Values.Look(ref prosperity, "prosperity");
            Scribe_Values.Look(ref days, "days");
            Scribe_Values.Look(ref tiles, "tiles");
        }

        public void Add(TileYield y)
        {
            food += y.food; wood += y.wood; stone += y.stone; metal += y.metal; components += y.components; prosperity += y.prosperity;
            tiles = y.tiles;
            days++;
        }

        public override string ToString() =>
            "dias=" + days + " tiles=" + tiles + " comida=" + food.ToString("0") + " madeira=" + wood.ToString("0") + " pedra=" + stone.ToString("0")
            + " metal=" + metal.ToString("0") + " comp=" + components.ToString("0.#") + " prosp=" + prosperity.ToString("0");
    }

    /// <summary>
    /// F4: a fronteira do jogador. O Hegemony não cria estado para colônias com mapa (usa a riqueza do mapa como
    /// prosperidade e multiplicador 1), então a regra do F2 não pegava o jogador. Aqui:
    /// - a colônia nasce com raio pequeno e cresce como as cidades das NPCs (riqueza no lugar da prosperidade);
    /// - marcos de fronteira (botão na colônia, no mapa-múndi) puxam o território numa direção;
    /// - colônia no teto: carta com um lugar sugerido para a próxima; fundar perto dá um empurrão na fronteira nova;
    /// - o território rende para a colônia mais próxima e, a cada poucos dias, chega um tributo em drop pod.
    /// O módulo do Hegemony aplica o raio (postfix em TerritorySolver.Sources) e acrescenta os marcos como fontes.
    /// </summary>
    public static class EQPlayerFrontier
    {
        public static bool Enabled => PlayerFrontierDef.Active != null && WorldComponent_EQFrontier.Current?.Enabled == true;

        public static bool IsColony(Settlement s) => s != null && !s.Destroyed && s.Faction == Faction.OfPlayer && s.HasMap;

        public static IEnumerable<Settlement> Colonies() => Find.WorldObjects.Settlements.Where(IsColony);

        public static IEnumerable<WorldObject_EQFrontierMarker> Markers() =>
            Find.WorldObjects.AllWorldObjects.OfType<WorldObject_EQFrontierMarker>().Where(m => !m.Destroyed);

        public static float Wealth(Settlement s) => s?.Map?.wealthWatcher?.WealthTotal ?? 0f;

        /// <summary>Teto do raio da colônia (raio natural do Hegemony pela riqueza × multiplicador).</summary>
        public static float Cap(Settlement s, float wealth)
        {
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            return EQTerritory.NaturalRadius(wealth) * (d?.capMultiplier ?? 1f);
        }

        public static float Allowed(WorldComponent_EQFrontier c, Settlement s)
        {
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            c.growth.TryGetValue(s.ID, out float g);
            return (d?.startRadius ?? 2f) + (float)Math.Floor(g);
        }

        /// <summary>Multiplicador para um raio desejado com essa prosperidade/riqueza (a força chega ao limiar no raio).</summary>
        public static float DecayForRadius(float prosperity, float radius, float original)
        {
            if (prosperity <= 0f || radius <= 0.05f) return original;
            double strength = Math.Sqrt(prosperity) / EQTerritory.Threshold;
            if (strength <= 1.0) return original;
            return (float)(2.0 * Math.Log(strength, 2.0) / (EQTerritory.DistancePower * radius));
        }

        /// <summary>Multiplicador da fonte da colônia no Hegemony (prosperidade = riqueza do mapa, como ele usa).</summary>
        public static float DecayForColony(Settlement s, float prosperity, float original)
        {
            WorldComponent_EQFrontier c = WorldComponent_EQFrontier.Current;
            if (!Enabled || c == null || !IsColony(s)) return original;
            float radius = Math.Min(Allowed(c, s), Cap(s, prosperity));
            return DecayForRadius(prosperity, radius, original);
        }

        /// <summary>Fontes extras do jogador (os marcos ativos): tile, riqueza da colônia, multiplicador.</summary>
        public static List<(PlanetTile tile, float prosperity, float decay)> MarkerSources()
        {
            var l = new List<(PlanetTile, float, float)>();
            if (!Enabled || !EQNation.Founded) return l;
            foreach (WorldObject_EQFrontierMarker m in Markers())
            {
                if (!m.Active) continue;
                float w = Wealth(m.colony);
                float decay = DecayForRadius(w, m.Radius, -1f);
                if (decay <= 0f) continue;
                l.Add((m.Tile, w, decay));
            }
            return l;
        }

        // ---------- Crescimento diário, cidade sugerida e tributo ----------

        public static void AdvanceDays(WorldComponent_EQFrontier c, float days)
        {
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            if (d == null || !EQNation.Founded) return; // N1: sem nação, a fronteira não cresce nem sugere cidade nova
            foreach (Settlement s in Colonies().ToList())
            {
                if (!c.growth.ContainsKey(s.ID))
                {
                    // Colônia nova: perto do lugar sugerido, começa com o empurrão prometido.
                    float bonus = 0f;
                    if (c.playerSuggestTile >= 0 && Find.TickManager.TicksGame - c.playerSuggestTick < d.suggestValidDays * GenDate.TicksPerDay)
                    {
                        var sug = new PlanetTile(c.playerSuggestTile, s.Tile.Layer);
                        if (Find.WorldGrid.ApproxDistanceInTiles(sug, s.Tile) <= d.suggestNearTiles)
                        {
                            bonus = d.newColonyBonusTiles;
                            c.playerSuggestTile = -1;
                            Messages.Message(s.LabelCap + " foi fundada onde os agrimensores mandaram. Eles já estão cobrando por isso: a fronteira nasce maior.",
                                s, MessageTypeDefOf.PositiveEvent, false);
                        }
                    }
                    c.growth[s.ID] = bonus;
                }
                float w = Wealth(s);
                if (w <= 0f) continue;
                float t = Mathf.Clamp01((w - d.poorWealth) / Math.Max(1f, d.richWealth - d.poorWealth));
                float daysPerTile = d.daysPerTilePoor + (d.daysPerTileRich - d.daysPerTilePoor) * t;
                float g = c.growth[s.ID];
                if (d.startRadius + g >= Cap(s, w) + 1f) continue;
                c.growth[s.ID] = g + days * EQCouncil.FrontierSpeedFactor / Math.Max(0.1f, daysPerTile);
            }
            TrySuggestColony(c, d);
        }

        private static void TrySuggestColony(WorldComponent_EQFrontier c, PlayerFrontierDef d)
        {
            int now = Find.TickManager.TicksGame;
            var cols = Colonies().ToList();
            if (cols.Count == 0 || cols.Count >= d.maxPlayerColonies) return;
            if (c.playerSuggestTick > 0 && now - c.playerSuggestTick < d.suggestCooldownDays * GenDate.TicksPerDay) return;
            Settlement full = cols.FirstOrDefault(s => Allowed(c, s) >= Cap(s, Wealth(s)) && Cap(s, Wealth(s)) > 0f);
            if (full == null) return;
            PlanetTile tile = SuggestSite(full);
            c.playerSuggestTick = now;
            if (!tile.Valid) return;
            c.playerSuggestTile = tile.tileId;
            c.log.Add("dia " + (now / (float)GenDate.TicksPerDay).ToString("0.0") + ": sugerido ao jogador o tile " + tile.tileId + " (a "
                      + Find.WorldGrid.ApproxDistanceInTiles(full.Tile, tile).ToString("0.0") + " de " + full.Name + ")");
            if (c.log.Count > 40) c.log.RemoveAt(0);
            Find.LetterStack.ReceiveLetter("Bordas longe demais",
                "Os agrimensores de " + full.LabelCap + " voltaram com a notícia: a fronteira chegou onde nenhum grito de socorro alcança. "
                + "Ou se funda uma colônia nova lá adiante, ou a borda fica de enfeite.\n\n"
                + "Eles marcaram um lugar (tile " + tile.tileId + "). Quem fundar a até " + d.suggestNearTiles.ToString("0") + " tiles dali nos próximos "
                + d.suggestValidDays.ToString("0") + " dias ganha uma fronteira que já nasce maior. Os agrimensores ganham uma comissão. Sempre ganham.",
                LetterDefOf.NeutralEvent, new LookTargets(new GlobalTargetInfo(tile)));
        }

        /// <summary>Lugar para a próxima colônia: na própria terra ou em terra de ninguém, à distância da regra do F2.</summary>
        public static PlanetTile SuggestSite(Settlement from)
        {
            FrontierDef fd = FrontierDef.Active;
            if (fd == null) return PlanetTile.Invalid;
            var claims = new List<KeyValuePair<PlanetTile, Faction>>();
            try { claims = EQTerritory.ClaimsProvider?.Invoke() ?? claims; } catch (Exception) { }
            var all = Find.WorldObjects.Settlements.Where(s => !s.Destroyed).ToList();
            return WorldComponent_EQFrontier.FindSite(from, fd, all, claims);
        }

        /// <summary>F3 para o jogador: rendimento de um dia por colônia (tiles do jogador para a colônia mais perto).</summary>
        public static Dictionary<Settlement, TileYield> ComputeYield()
        {
            var res = new Dictionary<Settlement, TileYield>();
            TerritoryYieldDef d = TerritoryYieldDef.Active;
            var cols = Colonies().ToList();
            if (d == null || cols.Count == 0) return res;
            List<KeyValuePair<PlanetTile, Faction>> claims;
            try { claims = EQTerritory.ClaimsProvider?.Invoke(); } catch (Exception) { return res; }
            if (claims == null) return res;
            Faction me = EQUtil.Canonical(Faction.OfPlayer);
            var cityTiles = new HashSet<PlanetTile>(Find.WorldObjects.Settlements.Select(s => s.Tile));
            var perCity = new Dictionary<Settlement, List<KeyValuePair<float, PlanetTile>>>();
            foreach (var c in claims)
            {
                if (cityTiles.Contains(c.Key) || EQUtil.Canonical(c.Value) != me) continue;
                Settlement best = null; float bd = float.MaxValue;
                foreach (Settlement s in cols)
                {
                    if (s.Tile.Layer != c.Key.Layer) continue;
                    float dist = Find.WorldGrid.ApproxDistanceInTiles(s.Tile, c.Key);
                    if (dist < bd) { bd = dist; best = s; }
                }
                if (best == null) continue;
                if (!perCity.TryGetValue(best, out var l)) perCity[best] = l = new List<KeyValuePair<float, PlanetTile>>();
                l.Add(new KeyValuePair<float, PlanetTile>(bd, c.Key));
            }
            foreach (var kv in perCity)
            {
                var total = new TileYield();
                foreach (var t in kv.Value.OrderBy(x => x.Key).Take(d.maxTilesPerSettlement))
                {
                    TileYield y = EQTerritoryYield.YieldOf(t.Value, d);
                    if (y != null) total.Add(y);
                }
                res[kv.Key] = total;
            }
            return res;
        }

        /// <summary>Um dia: acumula o rendimento de cada colônia; no intervalo, entrega o tributo.</summary>
        public static int YieldDaily(WorldComponent_EQFrontier c, bool forceDeliver = false)
        {
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            if (d == null || !EQTerritoryYield.Enabled || !EQNation.Founded) return 0;
            foreach (var kv in ComputeYield())
            {
                PlayerYieldStock st = c.playerYield.FirstOrDefault(x => x.colonyId == kv.Key.ID);
                if (st == null) c.playerYield.Add(st = new PlayerYieldStock { colonyId = kv.Key.ID });
                st.Add(kv.Value);
            }
            int delivered = 0;
            foreach (PlayerYieldStock st in c.playerYield.ToList())
            {
                Settlement s = Colonies().FirstOrDefault(x => x.ID == st.colonyId);
                if (s == null) { c.playerYield.Remove(st); continue; }
                if (!forceDeliver && st.days < d.tributeIntervalDays) continue;
                if (st.tiles >= d.tributeMinTiles && Deliver(s, st, d)) delivered++;
                c.playerYield.Remove(st);
            }
            return delivered;
        }

        private static ThingDef Named(string defName, ThingDef fallback) => DefDatabase<ThingDef>.GetNamedSilentFail(defName) ?? fallback;

        private static bool Deliver(Settlement s, PlayerYieldStock st, PlayerFrontierDef d)
        {
            Map map = s.Map;
            if (map == null) return false;
            var stacks = new List<(ThingDef def, int count)>
            {
                (Named("RawPotatoes", ThingDefOf.Pemmican), (int)(st.food * d.tributeFactor * EQCouncil.TributeFactor)),
                (ThingDefOf.WoodLog, (int)(st.wood * d.tributeFactor * EQCouncil.TributeFactor)),
                (Named("BlocksGranite", ThingDefOf.Steel), (int)(st.stone * d.tributeFactor * EQCouncil.TributeFactor)),
                (ThingDefOf.Steel, (int)(st.metal * d.tributeFactor * EQCouncil.TributeFactor)),
                (ThingDefOf.ComponentIndustrial, (int)(st.components * d.tributeFactor * EQCouncil.TributeFactor)),
                (ThingDefOf.Silver, (int)(st.prosperity * d.tributeFactor * EQCouncil.TributeFactor * d.prosperityToSilver)),
            };
            var things = new List<Thing>();
            var lines = new List<string>();
            foreach (var (def, count) in stacks)
            {
                if (def == null || count <= 0) continue;
                lines.Add("  - " + def.LabelCap + " x" + count);
                int left = count;
                while (left > 0)
                {
                    Thing t = ThingMaker.MakeThing(def);
                    t.stackCount = Math.Min(left, def.stackLimit);
                    left -= t.stackCount;
                    things.Add(t);
                }
            }
            if (things.Count == 0) return false;
            LastTribute = s.Name + ": " + st + " → " + string.Join(", ", lines.Select(l => l.Trim()));
            DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, things);
            Find.LetterStack.ReceiveLetter("Tributo do território",
                "Os " + st.tiles + " tiles em volta de " + s.LabelCap + " renderam o que a terra tinha para dar em " + st.days + " dias. "
                + "Ninguém sabe ao certo quem colheu, cortou e carregou; os pods chegaram, e é isso que importa.\n\n" + string.Join("\n", lines),
                LetterDefOf.PositiveEvent, new LookTargets(things));
            return true;
        }

        public static string LastTribute;

        // ---------- Marcos ----------

        public static int MarkersOf(Settlement colony) => Markers().Count(m => m.colony == colony);

        /// <summary>null = pode; senão o motivo.</summary>
        public static string CanPlaceMarker(Settlement colony, PlanetTile tile, bool free, List<KeyValuePair<PlanetTile, Faction>> claims = null)
        {
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            if (d == null || !Enabled) return "A fronteira viva só vale no mundo fixo do Ecos.";
            if (!EQNation.Founded) return EQNation.NoFrontierReason;
            if (!IsColony(colony)) return "Só uma colônia com mapa pode mandar agrimensores.";
            if (MarkersOf(colony) >= (d.markersPerColony + EQCouncil.ExtraMarkers)) return "Esta colônia já tem " + (d.markersPerColony + EQCouncil.ExtraMarkers) + " marcos. Mais estaca que gente.";
            if (!free && SilverOn(colony.Map) < d.markerCostSilver) return "Faltam " + d.markerCostSilver + " de prata na colônia (estaca é barata, agrimensor não).";
            if (!tile.Valid || tile.Layer != colony.Tile.Layer) return "Escolha um tile na mesma superfície da colônia.";
            Tile t = Find.WorldGrid[tile];
            if (t == null || t.WaterCovered || t.hilliness == Hilliness.Impassable || !Find.WorldPathGrid.Passable(tile)) return "Escolha um tile de terra transitável.";
            if (Find.WorldObjects.AnyWorldObjectAt(tile)) return "O tile já está ocupado.";
            if (Find.WorldGrid.ApproxDistanceInTiles(colony.Tile, tile) > d.markerMaxDistance)
                return "Longe demais da colônia (máximo " + d.markerMaxDistance.ToString("0") + " tiles).";
            try { claims = claims ?? EQTerritory.ClaimsProvider?.Invoke(); } catch (Exception) { }
            if (claims == null) return "Território indisponível (Hegemony).";
            Faction me = EQUtil.Canonical(Faction.OfPlayer);
            Faction owner = null;
            bool nearBorder = false;
            foreach (var c in claims)
            {
                if (c.Key == tile) owner = EQUtil.Canonical(c.Value);
                if (!nearBorder && EQUtil.Canonical(c.Value) == me && c.Key.Layer == tile.Layer
                    && Find.WorldGrid.ApproxDistanceInTiles(c.Key, tile) <= d.markerMaxFromBorder)
                    nearBorder = true;
            }
            foreach (WorldObject_EQFrontierMarker m in Markers())
                if (!nearBorder && m.Tile.Layer == tile.Layer && Find.WorldGrid.ApproxDistanceInTiles(m.Tile, tile) <= d.markerMaxFromBorder + 1f) nearBorder = true;
            if (owner != null && owner != me) return "Esse tile é de " + owner.Name + ". Para isso existem exércitos, não estacas.";
            if (!nearBorder) return "O marco precisa encostar na fronteira atual (até " + d.markerMaxFromBorder.ToString("0.#") + " tile de um tile seu).";
            return null;
        }

        public static int SilverOn(Map map) => map == null ? 0 : map.listerThings.ThingsOfDef(ThingDefOf.Silver).Where(t => !t.IsForbidden(Faction.OfPlayer)).Sum(t => t.stackCount);

        private static void TakeSilver(Map map, int amount)
        {
            foreach (Thing t in map.listerThings.ThingsOfDef(ThingDefOf.Silver).Where(x => !x.IsForbidden(Faction.OfPlayer)).ToList())
            {
                if (amount <= 0) break;
                int take = Math.Min(amount, t.stackCount);
                t.SplitOff(take).Destroy();
                amount -= take;
            }
        }

        public static WorldObject_EQFrontierMarker PlaceMarker(Settlement colony, PlanetTile tile, bool free)
        {
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            if (!free) TakeSilver(colony.Map, d.markerCostSilver);
            var m = (WorldObject_EQFrontierMarker)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("EQ_FrontierMarker"));
            m.SetFaction(Faction.OfPlayer);
            m.Tile = tile;
            m.colony = colony;
            m.placedTick = Find.TickManager.TicksGame;
            Find.WorldObjects.Add(m);
            return m;
        }

        /// <summary>Tile automático para o marco (testes): o válido mais longe da colônia, perto da borda.</summary>
        public static PlanetTile AutoMarkerTile(Settlement colony)
        {
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            List<KeyValuePair<PlanetTile, Faction>> claims = null;
            try { claims = EQTerritory.ClaimsProvider?.Invoke(); } catch (Exception) { }
            PlanetTile best = PlanetTile.Invalid; float bd = -1f;
            var seen = new HashSet<PlanetTile> { colony.Tile };
            var queue = new Queue<PlanetTile>();
            queue.Enqueue(colony.Tile);
            var tmp = new List<PlanetTile>();
            while (queue.Count > 0)
            {
                PlanetTile t = queue.Dequeue();
                float dist = Find.WorldGrid.ApproxDistanceInTiles(colony.Tile, t);
                if (dist > (d?.markerMaxDistance ?? 10f)) continue;
                if (dist > bd && CanPlaceMarker(colony, t, true, claims) == null) { bd = dist; best = t; }
                tmp.Clear();
                Find.WorldGrid.GetTileNeighbors(t, tmp);
                foreach (PlanetTile n in tmp) if (n.Layer == t.Layer && seen.Add(n)) queue.Enqueue(n);
            }
            return best;
        }

        public static Command PlaceCommand(Settlement colony)
        {
            PlayerFrontierDef d = PlayerFrontierDef.Active;
            var cmd = new Command_Action
            {
                defaultLabel = "Plantar marco de fronteira",
                defaultDesc = "Manda agrimensores fincarem uma estaca com placa num tile encostado na fronteira (até "
                              + d.markerMaxDistance.ToString("0") + " tiles da colônia). Depois de " + d.markerSetupDays.ToString("0")
                              + " dias de demarcação, o território cresce em volta do marco (" + d.markerRadius.ToString("0.#") + " tiles; "
                              + d.markerRadiusGrown.ToString("0.#") + " depois de " + d.markerGrowDays.ToString("0") + " dias). Custa "
                              + d.markerCostSilver + " de prata. Até " + (d.markersPerColony + EQCouncil.ExtraMarkers) + " por colônia.\n\nMarcos desta colônia: " + MarkersOf(colony) + ".",
                icon = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/SurveySiteExpanding", false) ?? BaseContent.BadTex,
                action = () =>
                {
                    Find.WorldTargeter.BeginTargeting(target =>
                    {
                        string why = CanPlaceMarker(colony, target.Tile, false);
                        if (why != null) { Messages.Message(why, MessageTypeDefOf.RejectInput, false); return false; }
                        PlaceMarker(colony, target.Tile, false);
                        Messages.Message("Os agrimensores partiram com estacas, cordas e opiniões. A demarcação leva " + d.markerSetupDays.ToString("0") + " dias.",
                            MessageTypeDefOf.TaskCompletion, false);
                        return true;
                    }, true, extraLabelGetter: target => CanPlaceMarker(colony, target.Tile, false) ?? "Plantar marco aqui");
                }
            };
            string block = !EQNation.Founded ? EQNation.NoFrontierReason
                : MarkersOf(colony) >= (d.markersPerColony + EQCouncil.ExtraMarkers) ? "Esta colônia já tem " + (d.markersPerColony + EQCouncil.ExtraMarkers) + " marcos."
                : SilverOn(colony.Map) < d.markerCostSilver ? "Faltam " + d.markerCostSilver + " de prata na colônia." : null;
            if (block != null) cmd.Disable(block);
            return cmd;
        }

        public static string Report(WorldComponent_EQFrontier c)
        {
            var sb = new StringBuilder("jogador: ativo=" + Enabled);
            foreach (Settlement s in Colonies())
            {
                float w = Wealth(s);
                sb.Append("\n  " + s.Name + " riqueza=" + w.ToString("0") + " R=" + Allowed(c, s).ToString("0.0") + " teto=" + Cap(s, w).ToString("0.0")
                          + " m=" + DecayForColony(s, w, 1f).ToString("0.00") + " marcos=" + MarkersOf(s));
            }
            foreach (WorldObject_EQFrontierMarker m in Markers())
                sb.Append("\n  marco tile " + m.Tile.tileId + " de " + m.colony?.Name + " idade=" + m.AgeDays.ToString("0.0") + " ativo=" + m.Active + " raio=" + m.Radius.ToString("0.0"));
            sb.Append("\n  sugestão: tile=" + c.playerSuggestTile + " tick=" + c.playerSuggestTick);
            foreach (PlayerYieldStock st in c.playerYield) sb.Append("\n  acumulado colônia " + st.colonyId + ": " + st);
            if (LastTribute != null) sb.Append("\n  último tributo: " + LastTribute);
            return sb.ToString();
        }
    }

    /// <summary>F4: botão "Plantar marco de fronteira" nas colônias do jogador (mapa-múndi), só no mundo fixo.</summary>
    [HarmonyPatch(typeof(Settlement), nameof(Settlement.GetGizmos))]
    public static class Patch_Settlement_GetGizmos_FrontierMarker
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Settlement __instance)
        {
            foreach (Gizmo g in __result) yield return g;
            if (EQPlayerFrontier.IsColony(__instance) && EQPlayerFrontier.Enabled)
                yield return EQPlayerFrontier.PlaceCommand(__instance);
        }
    }
}
