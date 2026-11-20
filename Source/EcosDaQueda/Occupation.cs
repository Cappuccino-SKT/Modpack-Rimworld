using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>F5 (MUNDO.md › Resultado do F5): parâmetros dos postos de ocupação. Valores propostos; ajustar no XML.</summary>
    public class OccupationDef : Def
    {
        /// <summary>Raio do posto ao ser plantado e depois de growDays dias.</summary>
        public float radius = 1.5f, radiusGrown = 2.5f, growDays = 10f;
        /// <summary>Força: prosperidade do posto = maior prosperidade das cidades envolvidas × isto (mínimo minProsperity).</summary>
        public float strengthFactor = 2f, minProsperity = 10000f;
        /// <summary>Procura do tile: o tile do perdedor mais perto da frente, até searchRadius tiles dela (a frente pode ser
        /// o meio do caminho entre as cidades quando os territórios ainda não se encostam).</summary>
        public float searchRadius = 12f;
        /// <summary>Vencer uma frente perto (até retakeDistance) de um posto do perdedor derruba o posto.</summary>
        public float retakeDistance = 3f;
        /// <summary>Postos por facção; o mais antigo cai quando passa disso.</summary>
        public int maxPerFaction = 4;
        /// <summary>Teto do canal "borderDefense" (General, Baluarte): o posto contra o jogador perde até isso do raio.</summary>
        public float maxPlayerDefense = 0.6f;
        /// <summary>O Hegemony mede distância por custo de caminho (bioma, morro), não por tiles: o raio do posto vira custo × isto.</summary>
        public float pathCostPerTile = 2f;

        public static OccupationDef Active => DefDatabase<OccupationDef>.AllDefsListForReading.FirstOrDefault();
    }

    /// <summary>Posto de ocupação: fonte de território do vencedor de uma frente de fronteira, na terra que era do perdedor.</summary>
    public class WorldObject_EQOccupation : WorldObject
    {
        public Faction loser;
        public int placedTick;
        public float prosperity;

        public float AgeDays => (Find.TickManager.TicksGame - placedTick) / (float)GenDate.TicksPerDay;

        public float Radius
        {
            get
            {
                OccupationDef d = OccupationDef.Active;
                if (d == null) return 1.5f;
                float r = AgeDays >= d.growDays ? d.radiusGrown : d.radius;
                // A fronteira do jogador cai mais devagar com General e Baluarte (canal borderDefense).
                if (loser != null && loser.IsPlayer)
                    r *= 1f - Mathf.Clamp(EQCouncil.Channel("borderDefense"), -0.5f, d.maxPlayerDefense); // negativo (Fronteiras abertas) cai mais rápido
                return Mathf.Max(1f, r);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref loser, "loser");
            Scribe_Values.Look(ref placedTick, "placedTick");
            Scribe_Values.Look(ref prosperity, "prosperity");
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (Find.TickManager.TicksGame % 2500 >= delta) return;
            if (Faction == null || Faction.defeated || (Faction.IsPlayer && !EQNation.Founded)) Destroy();
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder(base.GetInspectString());
            if (sb.Length > 0) sb.Append("\n");
            sb.Append("Ocupado por " + (Faction?.Name ?? "?") + ", tomado de " + (loser?.Name ?? "?") + " há " + AgeDays.ToString("0") + " dias. Reivindica "
                      + Radius.ToString("0.#") + " tiles em volta.");
            sb.Append("\nCai se " + (loser?.Name ?? "o dono anterior") + " vencer uma frente de fronteira aqui perto.");
            return sb.ToString();
        }
    }

    public static class EQOccupation
    {
        public static WorldObjectDef PostDef => DefDatabase<WorldObjectDef>.GetNamedSilentFail("EQ_OccupationPost");
        public static string LastReport;

        public static IEnumerable<WorldObject_EQOccupation> Posts() =>
            Find.WorldObjects?.AllWorldObjects.OfType<WorldObject_EQOccupation>() ?? Enumerable.Empty<WorldObject_EQOccupation>();

        private static float ProsperityOf(Faction f, PlanetTile near)
        {
            Settlement s = Find.WorldObjects.Settlements.Where(x => x.Faction == f && x.Tile.Layer == near.Layer)
                .OrderBy(x => Find.WorldGrid.ApproxDistanceInTiles(x.Tile, near)).FirstOrDefault();
            if (s == null) return 0f;
            if (f.IsPlayer) return s.Map != null ? s.Map.wealthWatcher.WealthTotal : 0f;
            return Math.Max(0f, EQTerritory.ProsperityProvider?.Invoke(s) ?? 0f);
        }

        /// <summary>Tile para o posto: do perdedor, perto da frente, sem cidade nem água.</summary>
        public static PlanetTile FindTile(PlanetTile front, Faction lose)
        {
            OccupationDef d = OccupationDef.Active;
            if (d == null || !front.Valid) return PlanetTile.Invalid;
            List<KeyValuePair<PlanetTile, Faction>> claims = null;
            try { claims = EQTerritory.ClaimsProvider?.Invoke(); } catch (Exception) { }
            Faction canonLose = EQUtil.Canonical(lose);
            var taken = new HashSet<PlanetTile>(Find.WorldObjects.AllWorldObjects.Where(o => !(o is WorldObject_EQFront)).Select(o => o.Tile));
            bool Ok(PlanetTile t)
            {
                Tile tile = Find.WorldGrid[t];
                return tile != null && !tile.WaterCovered && tile.hilliness != Hilliness.Impassable && !taken.Contains(t);
            }
            if (claims != null)
            {
                var best = claims.Where(c => c.Key.Layer == front.Layer && EQUtil.Canonical(c.Value) == canonLose && Ok(c.Key))
                    .Select(c => (t: c.Key, dist: Find.WorldGrid.ApproxDistanceInTiles(c.Key, front)))
                    .Where(x => x.dist <= d.searchRadius)
                    .OrderBy(x => x.dist).ToList();
                if (best.Count > 0) return best[0].t;
            }
            return Ok(front) ? front : PlanetTile.Invalid;
        }

        /// <summary>Vencedor de uma frente de fronteira: derruba postos do vencedor-antigo-perdedor por perto e planta o seu.</summary>
        public static string OnBorderWin(PlanetTile front, Faction win, Faction lose)
        {
            OccupationDef d = OccupationDef.Active;
            if (d == null || PostDef == null || win == null || lose == null || !front.Valid) return "sem ocupação";
            if (win.IsPlayer && !EQNation.Founded) return "sem nação, sem ocupação";
            var parts = new List<string>();
            // Retomada: o perdedor desta frente perde os postos que tinha aqui perto.
            foreach (WorldObject_EQOccupation p in Posts().Where(p => p.Faction == lose && p.Tile.Layer == front.Layer
                         && Find.WorldGrid.ApproxDistanceInTiles(p.Tile, front) <= d.retakeDistance).ToList())
            {
                p.Destroy();
                parts.Add("posto de " + lose.Name + " retomado");
            }
            if (win.IsPlayer) EQLegitimacy.Event(LegitimacyDef.Active?.borderWin ?? 0f, "frente de fronteira vencida contra " + lose.Name);
            else if (lose.IsPlayer) EQLegitimacy.Event(LegitimacyDef.Active?.borderLoss ?? 0f, "frente de fronteira perdida para " + win.Name);
            PlanetTile tile = FindTile(front, lose);
            if (!tile.Valid) { parts.Add("sem tile para o posto"); return LastReport = string.Join("; ", parts); }
            float pr = Math.Max(d.minProsperity, Math.Max(ProsperityOf(win, tile), ProsperityOf(lose, tile)) * d.strengthFactor);
            var post = (WorldObject_EQOccupation)WorldObjectMaker.MakeWorldObject(PostDef);
            post.Tile = tile;
            post.SetFaction(win);
            post.loser = lose;
            post.placedTick = Find.TickManager.TicksGame;
            post.prosperity = pr;
            Find.WorldObjects.Add(post);
            parts.Add(win.Name + " ocupa o tile " + tile + " de " + lose.Name + " (força " + Mathf.Sqrt(pr).ToString("0") + ")");
            // Teto por facção: o mais antigo é abandonado.
            var mine = Posts().Where(p => p.Faction == win).OrderBy(p => p.placedTick).ToList();
            while (mine.Count > d.maxPerFaction)
            {
                mine[0].Destroy();
                mine.RemoveAt(0);
                parts.Add("posto mais antigo de " + win.Name + " abandonado");
            }
            if (lose.IsPlayer && !EQLeader.SuppressDialogs)
                Find.LetterStack.ReceiveLetter("Fronteira ocupada",
                    win.Name + " venceu a frente e plantou um posto de ocupação em terra de " + Faction.OfPlayer.Name + ". As cercas mudaram de lugar durante a noite; os mapas oficiais vão demorar mais."
                    + "\n\nVencer uma frente de fronteira aqui perto derruba o posto. O General e memes como o Baluarte fazem a fronteira cair mais devagar.",
                    LetterDefOf.NegativeEvent, post);
            else if (win.IsPlayer && !EQLeader.SuppressDialogs)
                Messages.Message(Faction.OfPlayer.Name + " plantou um posto de ocupação em terra de " + lose.Name + ".", post, MessageTypeDefOf.PositiveEvent, false);
            return LastReport = string.Join("; ", parts);
        }

        /// <summary>Fontes de território dos postos: tile, dono, prosperidade, multiplicador de decaimento.</summary>
        public static List<(PlanetTile tile, Faction owner, float prosperity, float decay)> Sources()
        {
            var l = new List<(PlanetTile, Faction, float, float)>();
            foreach (WorldObject_EQOccupation p in Posts())
            {
                Faction f = p.Faction;
                if (f == null || f.defeated) continue;
                if (f.IsPlayer && !EQNation.Founded) continue;
                float decay = EQPlayerFrontier.DecayForRadius(p.prosperity, p.Radius * (OccupationDef.Active?.pathCostPerTile ?? 1f), -1f);
                if (decay <= 0f) continue;
                l.Add((p.Tile, f, p.prosperity, decay));
            }
            return l;
        }

        /// <summary>Donos dos tiles até dist tiles do centro (para os testes): "Coro 5, Forja 3, ninguém 4".</summary>
        public static string Around(PlanetTile center, float dist, List<KeyValuePair<PlanetTile, Faction>> claims)
        {
            var owners = new Dictionary<PlanetTile, Faction>();
            if (claims != null) foreach (var c in claims) owners[c.Key] = c.Value;
            var counts = new Dictionary<string, int>();
            var seen = new HashSet<PlanetTile> { center };
            var frontier = new List<PlanetTile> { center };
            var nb = new List<PlanetTile>();
            while (frontier.Count > 0)
            {
                var next = new List<PlanetTile>();
                foreach (PlanetTile t in frontier)
                {
                    string k = owners.TryGetValue(t, out Faction f) && f != null ? (f.IsPlayer ? "jogador" : f.def.defName.Replace("EQ_Faction_", "")) : "ninguém";
                    counts.TryGetValue(k, out int n);
                    counts[k] = n + 1;
                    nb.Clear();
                    Find.WorldGrid.GetTileNeighbors(t, nb);
                    foreach (PlanetTile x in nb)
                        if (x.Layer == center.Layer && seen.Add(x) && Find.WorldGrid.ApproxDistanceInTiles(x, center) <= dist) next.Add(x);
                }
                frontier = next;
            }
            return string.Join(",", counts.OrderByDescending(kv => kv.Value).Select(kv => kv.Key + " " + kv.Value));
        }

        public static string Report()
        {
            var posts = Posts().ToList();
            if (posts.Count == 0) return "postos de ocupação: nenhum";
            List<KeyValuePair<PlanetTile, Faction>> claims = null;
            try { claims = EQTerritory.ClaimsProvider?.Invoke(); } catch (Exception) { }
            return "postos de ocupação: " + string.Join(" | ", posts.Select(p =>
            {
                int own = claims?.Count(c => EQUtil.Canonical(c.Value) == EQUtil.Canonical(p.Faction) && c.Key.Layer == p.Tile.Layer
                                             && Find.WorldGrid.ApproxDistanceInTiles(c.Key, p.Tile) <= p.Radius + 0.5f) ?? -1;
                return p.Faction?.def.defName.Replace("EQ_Faction_", "") + "←" + p.loser?.def.defName.Replace("EQ_Faction_", "") + " tile=" + p.Tile
                       + " raio=" + p.Radius.ToString("0.#") + " idade=" + p.AgeDays.ToString("0.#") + "d força=" + Mathf.Sqrt(p.prosperity).ToString("0") + " tiles_do_dono_em_volta=" + own
                       + " donos_até_3=" + Around(p.Tile, 3f, claims);
            }));
        }
    }
}
