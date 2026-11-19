using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Fronteiras vivas, F3 (MUNDO.md): rendimento diário de cada tile do território para a cidade dona.</summary>
    public class TerritoryYieldDef : Def
    {
        /// <summary>Só no mundo fixo (como a fronteira que cresce, F2).</summary>
        public bool onlyFixedWorld = true;
        /// <summary>Por tile por dia, antes do perfil do bioma: comida, materiais (madeira/pedra/metal) e prosperidade.</summary>
        public float food = 1f, materials = 0.5f, prosperity = 10f;
        /// <summary>Peso do perfil de bioma do Hegemony (BiomeEffectDef): rendimento × (1 + campo × profileWeight).</summary>
        public float profileWeight = 2f;
        /// <summary>Componentes por tile por dia quando o perfil do bioma tem components &gt; 0 (zonas da Queda).</summary>
        public float components = 0.05f;
        /// <summary>Morro e montanha: pedra e metal extras por tile.</summary>
        public float hillsStone = 0.2f, mountainsMetal = 0.2f;
        /// <summary>Rio e estrada no tile.</summary>
        public float riverFood = 0.5f, roadProsperity = 5f;
        /// <summary>Tiles que contam por cidade (os mais perto primeiro).</summary>
        public int maxTilesPerSettlement = 60;

        public static TerritoryYieldDef Active => DefDatabase<TerritoryYieldDef>.AllDefsListForReading.FirstOrDefault();
    }

    /// <summary>Rendimento de um dia (somado por cidade).</summary>
    public class TileYield
    {
        public float food, wood, stone, metal, components, prosperity, happiness;
        public int tiles;

        public void Add(TileYield o)
        {
            food += o.food; wood += o.wood; stone += o.stone; metal += o.metal;
            components += o.components; prosperity += o.prosperity; happiness += o.happiness; tiles += o.tiles;
        }

        public override string ToString() =>
            "tiles=" + tiles + " comida=" + food.ToString("0.#") + " madeira=" + wood.ToString("0.#") + " pedra=" + stone.ToString("0.#")
            + " metal=" + metal.ToString("0.#") + " comp=" + components.ToString("0.##") + " prosp=" + prosperity.ToString("0") + " fel=" + happiness.ToString("0.##");
    }

    /// <summary>
    /// F3: o Hegemony só usa o tile da própria cidade (bioma, morro, rio, marco) e já dá à cidade os marcos e sítios do
    /// território (LandmarkHoldings). Aqui cada tile do território rende também pelo bioma: uma vez por dia, cada tile
    /// vai para a cidade mais próxima da mesma facção e soma comida, materiais e prosperidade (perfil de bioma do
    /// Hegemony, BiomeEffectDef: os do jogo e os nossos das zonas da Queda). Com a fronteira que cresce (F2), crescer rende.
    /// O módulo do Hegemony entrega (estoques por tipo, reflexão nos métodos internos de soma).
    /// </summary>
    public static class EQTerritoryYield
    {
        /// <summary>Perfil do bioma no Hegemony: food, wood, stone, metal, components, prosperity, happiness (null = sem perfil).</summary>
        public static Func<BiomeDef, float[]> BiomeProfileProvider;
        /// <summary>Entrega o rendimento ao estado da cidade no Hegemony.</summary>
        public static Action<Settlement, TileYield> Applier;
        /// <summary>Estoques da cidade para relatório (comida, materiais, prosperidade, felicidade).</summary>
        public static Func<Settlement, string> StockReport;

        /// <summary>Último rendimento por cidade (ID → rendimento), para o relatório.</summary>
        public static readonly Dictionary<int, TileYield> Last = new Dictionary<int, TileYield>();
        public static int LastTick = -1;

        public static bool Enabled
        {
            get
            {
                TerritoryYieldDef d = TerritoryYieldDef.Active;
                if (d == null || Applier == null || EQTerritory.ClaimsProvider == null) return false;
                return !d.onlyFixedWorld || WorldComponent_EQFrontier.Current?.active == true;
            }
        }

        public static TileYield YieldOf(PlanetTile t, TerritoryYieldDef d)
        {
            var y = new TileYield { tiles = 1 };
            Tile tile = Find.WorldGrid[t];
            if (tile == null || tile.WaterCovered) return null;
            float[] p = BiomeProfileProvider?.Invoke(tile.PrimaryBiome);
            float F(int i) => p == null ? 0f : p[i];
            float w = d.profileWeight;
            float mult(int i) => Math.Max(0f, 1f + F(i) * w);
            y.food = d.food * mult(0);
            y.wood = d.materials * 0.4f * mult(1);
            y.stone = d.materials * 0.3f * mult(2);
            y.metal = d.materials * 0.3f * mult(3);
            y.components = F(4) > 0f ? d.components * F(4) * 10f : 0f;
            y.prosperity = d.prosperity * mult(5);
            y.happiness = F(6) * 0.01f;
            if (tile.hilliness >= Hilliness.SmallHills) y.stone += d.hillsStone;
            if (tile.hilliness >= Hilliness.Mountainous) y.metal += d.mountainsMetal;
            if (tile is SurfaceTile st)
            {
                if (!st.Rivers.NullOrEmpty()) y.food += d.riverFood;
                if (!st.Roads.NullOrEmpty()) y.prosperity += d.roadProsperity;
            }
            return y;
        }

        /// <summary>Calcula o rendimento de cada cidade (sem entregar).</summary>
        public static Dictionary<Settlement, TileYield> Compute()
        {
            var res = new Dictionary<Settlement, TileYield>();
            TerritoryYieldDef d = TerritoryYieldDef.Active;
            if (d == null) return res;
            List<KeyValuePair<PlanetTile, Faction>> claims;
            try { claims = EQTerritory.ClaimsProvider?.Invoke(); } catch (Exception) { return res; }
            if (claims == null) return res;
            var cities = Find.WorldObjects.Settlements.Where(WorldComponent_EQFrontier.Managed)
                .GroupBy(s => EQUtil.Canonical(s.Faction)).ToDictionary(g => g.Key, g => g.ToList());
            var cityTiles = new HashSet<PlanetTile>(Find.WorldObjects.Settlements.Select(s => s.Tile));
            var perCity = new Dictionary<Settlement, List<KeyValuePair<float, PlanetTile>>>();
            foreach (var c in claims)
            {
                if (cityTiles.Contains(c.Key)) continue;
                Faction f = EQUtil.Canonical(c.Value);
                if (f == null || !cities.TryGetValue(f, out var list)) continue;
                Settlement best = null; float bd = float.MaxValue;
                foreach (Settlement s in list)
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
                    TileYield y = YieldOf(t.Value, d);
                    if (y != null) total.Add(y);
                }
                res[kv.Key] = total;
            }
            return res;
        }

        /// <summary>Um dia de rendimento: calcula e entrega.</summary>
        public static int ApplyDaily()
        {
            if (!Enabled) return 0;
            Last.Clear();
            LastTick = Find.TickManager.TicksGame;
            int n = 0;
            foreach (var kv in Compute())
            {
                Last[kv.Key.ID] = kv.Value;
                try { Applier(kv.Key, kv.Value); n++; }
                catch (Exception ex) { Log.ErrorOnce("[Ecos da Queda] Rendimento do território (F3): " + ex, 0x4E51F3); }
            }
            return n;
        }

        public static string Report()
        {
            var sb = new StringBuilder("ativo=" + Enabled + " último_dia_tick=" + LastTick);
            var comp = Compute();
            foreach (var g in comp.GroupBy(kv => EQUtil.Canonical(kv.Key.Faction)).OrderBy(g => g.Key.def.defName))
            {
                var sum = new TileYield();
                foreach (var kv in g) sum.Add(kv.Value);
                sb.Append("\n  " + g.Key.def.defName.Replace("EQ_Faction_", "") + " (" + g.Count() + " cidades): " + sum);
                foreach (var kv in g.OrderByDescending(x => x.Value.tiles).Take(2))
                    sb.Append("\n    " + kv.Key.Name + ": " + kv.Value + " | estoque: " + (StockReport?.Invoke(kv.Key) ?? "-"));
            }
            return sb.ToString();
        }
    }
}
