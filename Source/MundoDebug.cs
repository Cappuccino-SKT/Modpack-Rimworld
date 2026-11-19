using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Mundo fixo (10b/M1): "biomes [arquivo]" lista todos os biomas e quantos tiles cada um tem no mundo atual;
    /// "worldinfo" resume a grade e o Worldbuilder.</summary>
    public static class EQMundoDebug
    {
        public static string Biomes(string path)
        {
            var count = new Dictionary<BiomeDef, int>();
            var grid = Find.WorldGrid;
            if (grid != null)
                for (int i = 0; i < grid.TilesCount; i++)
                {
                    BiomeDef b = grid[i]?.PrimaryBiome;
                    if (b != null) count[b] = count.TryGetValue(b, out int c) ? c + 1 : 1;
                }
            var sb = new StringBuilder("defName\tlabel\tmod\timplemented\tcanBuildBase\tcanAutoChoose\tinVacuum\ttiles\n");
            var all = DefDatabase<BiomeDef>.AllDefsListForReading.OrderBy(b => b.modContentPack?.PackageId).ThenBy(b => b.defName).ToList();
            foreach (BiomeDef b in all)
            {
                bool vac = false;
                try { vac = (bool)(typeof(BiomeDef).GetField("inVacuum")?.GetValue(b) ?? false); } catch { }
                sb.AppendLine(string.Join("\t", b.defName, b.label, b.modContentPack?.PackageId ?? "?", b.implemented, b.canBuildBase, b.canAutoChoose, vac,
                    count.TryGetValue(b, out int n) ? n : 0));
            }
            if (!path.NullOrEmpty()) System.IO.File.WriteAllText(path, sb.ToString());
            var usable = all.Where(b => b.implemented && b.canBuildBase).ToList();
            return "biomas=" + all.Count + " implementados_habitáveis=" + usable.Count + " no_mundo=" + count.Count
                   + " faltando_no_mundo=" + usable.Count(b => !count.ContainsKey(b))
                   + " por_mod=[" + string.Join(", ", all.GroupBy(b => b.modContentPack?.PackageId ?? "?").Select(g => g.Key + "=" + g.Count())) + "]"
                   + (path.NullOrEmpty() ? "" : " arquivo=" + path);
        }

        /// <summary>F1: território do Hegemony por facção (tiles, cidades, distância máxima até a cidade, raio natural pela prosperidade).</summary>
        public static string Territory()
        {
            if (EQTerritory.ClaimsProvider == null) return "sem Hegemony";
            var rep = EQTerritory.Report();
            return "facções=" + rep.Count + " tiles_total=" + rep.Sum(r => r.tiles) + "\n" + string.Join("\n", rep.Take(14).Select(r =>
                "  " + (r.faction.def.defName.Replace("EQ_Faction_", "")) + ": tiles=" + r.tiles + " cidades=" + r.settlements
                + " dist_max=" + r.maxDist.ToString("0.0") + " tiles/cidade=" + (r.settlements > 0 ? (r.tiles / (float)r.settlements).ToString("0") : "-")
                + " [" + string.Join("; ", r.cities) + "]"));
        }

        /// <summary>F2: fronteira que cresce. Raio permitido, teto e multiplicador por cidade; cidades novas.</summary>
        public static string Frontier()
        {
            var c = WorldComponent_EQFrontier.Current;
            return c == null ? "sem componente" : c.Report();
        }

        /// <summary>F2: avança o crescimento n dias (um passo por dia, com as cidades novas), sem passar o tempo do jogo.</summary>
        public static string FrontierSkip(int days)
        {
            var c = WorldComponent_EQFrontier.Current;
            if (c == null || !c.Enabled) return "fronteira desligada";
            int before = Find.WorldObjects.Settlements.Count;
            for (int i = 0; i < days; i++)
            {
                c.AdvanceDays(1f);
                foreach (string k in c.lastNewCity.Keys.ToList()) c.lastNewCity[k] -= GenDate.TicksPerDay;
            }
            return "avançou " + days + " dias; assentamentos " + before + " → " + Find.WorldObjects.Settlements.Count;
        }

        /// <summary>M3: mundo fixo. Relatório da aplicação e as cidades por facção (nome, tile, lat/lon, bioma, estrada).</summary>
        public static string Fixed()
        {
            var w = Find.World;
            if (w == null) return "sem mundo";
            FixedWorldDef def = EQFixedWorld.ActiveDef();
            var sb = new StringBuilder();
            sb.Append("preset=" + (EQFixedWorld.CurrentPresetName() ?? "-") + " def=" + (def?.defName ?? "-") + " relatorio=" + EQFixedWorld.LastReport);
            sb.Append("\n  cidades=" + w.worldObjects.Settlements.Count + " na_agua=" + w.worldObjects.Settlements.Count(s => w.grid[s.Tile].WaterCovered));
            foreach (var g in w.worldObjects.Settlements.Where(s => s.Faction != null).GroupBy(s => s.Faction).OrderBy(g => g.Key.def.defName))
            {
                sb.Append("\n  " + g.Key.def.defName.Replace("EQ_Faction_", "") + " (" + g.Count() + "): ");
                sb.Append(string.Join("; ", g.Select(s =>
                {
                    var ll = w.grid.LongLatOf(s.Tile);
                    var t = w.grid[s.Tile];
                    bool road = t is SurfaceTile st && !st.Roads.NullOrEmpty();
                    return s.Name + " @" + s.Tile.tileId + " (" + ll.y.ToString("0.0") + "," + ll.x.ToString("0.0") + ") " + t.PrimaryBiome?.defName + (road ? " +estrada" : "");
                })));
            }
            return sb.ToString();
        }

        public static string Info()
        {
            var w = Find.World;
            if (w == null) return "sem mundo";
            string wb = "não";
            Type mgr = GenTypes.GetTypeInAnyAssembly("Worldbuilder.WorldPresetManager");
            if (mgr != null)
            {
                int presets = -1;
                string names = "";
                try
                {
                    var m = mgr.GetMethod("GetAllPresets", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    var list = (m?.Invoke(null, new object[] { false, true }) as System.Collections.IEnumerable)?.Cast<object>().ToList();
                    presets = list?.Count ?? -1;
                    names = string.Join("; ", (list ?? new List<object>()).Select(p => p.GetType().GetField("name")?.GetValue(p) as string));
                }
                catch (Exception ex) { names = "erro: " + ex.GetType().Name + " " + ex.Message; }
                wb = "sim, presets=" + presets + " [" + names + "]";
            }
            return "tiles=" + Find.WorldGrid.TilesCount + " cobertura=" + w.info.planetCoverage + " semente=" + w.info.seedString
                   + " assentamentos=" + Find.WorldObjects.Settlements.Count + " worldbuilder=" + wb;
        }
    }
}
