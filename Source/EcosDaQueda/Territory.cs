using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Fronteiras vivas (MUNDO.md, F1+): leitura do território do Hegemony. O módulo do Hegemony preenche os provedores.
    /// Fórmula do Hegemony (TerritorySolver): força = √prosperidade × 2^(−distância × 1,5 × multiplicador ÷ 2); o tile é
    /// reivindicado enquanto a força ≥ 10 (claimThreshold), até 40 tiles (claimRange).
    /// </summary>
    public static class EQTerritory
    {
        /// <summary>Todos os tiles reivindicados (tile, dono canônico; disputados ficam de fora).</summary>
        public static Func<List<KeyValuePair<PlanetTile, Faction>>> ClaimsProvider;
        /// <summary>Prosperidade do assentamento no Hegemony (−1 = sem estado).</summary>
        public static Func<Settlement, float> ProsperityProvider;

        public const float Threshold = 10f, DistancePower = 1.5f;

        /// <summary>Raio natural (tiles) de uma cidade com essa prosperidade e multiplicador de decaimento.</summary>
        public static float NaturalRadius(float prosperity, float decayMultiplier = 1f)
        {
            if (prosperity <= 0f) return 0f;
            double s = Math.Sqrt(prosperity) / Threshold;
            if (s <= 1.0) return 0f;
            return (float)(2.0 * Math.Log(s, 2.0) / (DistancePower * Math.Max(0.01f, decayMultiplier)));
        }

        public class FactionReport
        {
            public Faction faction;
            public int tiles, settlements;
            public float maxDist;
            public List<string> cities = new List<string>();
        }

        /// <summary>Por facção: tiles, cidades e a distância máxima de um tile dela até a cidade dela mais próxima.</summary>
        public static List<FactionReport> Report()
        {
            var res = new Dictionary<Faction, FactionReport>();
            List<KeyValuePair<PlanetTile, Faction>> claims = null;
            try { claims = ClaimsProvider?.Invoke(); } catch (Exception ex) { Log.Warning("[Ecos da Queda] território: " + ex.Message); }
            var bySettle = Find.WorldObjects.Settlements.Where(s => s.Faction != null && !s.Faction.IsPlayer)
                .GroupBy(s => EQUtil.Canonical(s.Faction)).ToDictionary(g => g.Key, g => g.ToList());
            FactionReport R(Faction f) => res.TryGetValue(f, out var r) ? r : res[f] = new FactionReport { faction = f };
            foreach (var c in claims ?? new List<KeyValuePair<PlanetTile, Faction>>())
            {
                Faction f = EQUtil.Canonical(c.Value);
                if (f == null) continue;
                var r = R(f);
                r.tiles++;
                if (bySettle.TryGetValue(f, out var ss) && ss.Count > 0)
                    r.maxDist = Math.Max(r.maxDist, ss.Min(s => Find.WorldGrid.ApproxDistanceInTiles(s.Tile, c.Key)));
            }
            foreach (var kv in bySettle)
            {
                var r = R(kv.Key);
                r.settlements = kv.Value.Count;
                foreach (Settlement s in kv.Value.Take(4))
                {
                    float p = ProsperityProvider?.Invoke(s) ?? -1f;
                    r.cities.Add(s.LabelCap + " p=" + p.ToString("0") + " r≈" + NaturalRadius(p).ToString("0.0"));
                }
            }
            return res.Values.OrderByDescending(r => r.tiles).ToList();
        }
    }
}
