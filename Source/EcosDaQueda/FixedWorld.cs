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
    /// <summary>Uma cidade fixa do mundo do Ecos (coordenadas do mundo novo: lat/lon da calota 25%, como em mundo/pintura.tsv).</summary>
    public class FixedSettlement
    {
        public FactionDef faction;
        public string name;
        public float lat, lon;
        public bool capital;
    }

    /// <summary>Mundo fixo (MUNDO.md, M3): cidades em lugares fixos quando o mundo vem do preset do Worldbuilder.</summary>
    public class FixedWorldDef : Def
    {
        /// <summary>Nome do preset do Worldbuilder que liga este mundo.</summary>
        public string presetName;
        /// <summary>Distância mínima (tiles) entre cidades.</summary>
        public int minSpacing = 4;
        /// <summary>Raio de busca (graus) em volta do ponto pedido quando o tile exato não serve.</summary>
        public float searchRadius = 6f;
        public bool regenerateLandmarks = true, regenerateMutators = true, regenerateRoads = true;
        /// <summary>Semente gravada no preset. O Worldbuilder sempre gera o mundo do preset com ela (pelo cenário ou
        /// pela tela de mundos); quando o mundo chega com ela, sorteamos outra para marcos, ruínas, estradas e mapas
        /// variarem por partida. Outra semente (ex.: a do executor de testes) fica como veio.</summary>
        public string fixedSeed;
        public List<FixedSettlement> settlements = new List<FixedSettlement>();
    }

    public static class EQFixedWorld
    {
        public static string LastReport = "não aplicado";
        /// <summary>Tiles das capitais (só nesta sessão; para o relatório).</summary>
        public static readonly List<string> Placed = new List<string>();

        private static Func<string> presetNameGetter;

        /// <summary>Nome do preset do Worldbuilder carregado agora (null sem Worldbuilder ou sem preset). Por reflexão.</summary>
        public static string CurrentPresetName()
        {
            if (presetNameGetter == null)
            {
                Type mgr = AccessTools.TypeByName("Worldbuilder.WorldPresetManager");
                var prop = mgr == null ? null : AccessTools.Property(mgr, "CurrentlyLoadedPreset");
                presetNameGetter = () =>
                {
                    object p = prop?.GetValue(null);
                    return p == null ? null : Traverse.Create(p).Field("name").GetValue<string>();
                };
            }
            try { return presetNameGetter(); } catch { return null; }
        }

        public static FixedWorldDef ActiveDef()
        {
            string preset = CurrentPresetName();
            if (preset.NullOrEmpty()) return null;
            return DefDatabase<FixedWorldDef>.AllDefsListForReading.FirstOrDefault(d => d.presetName == preset);
        }

        public static bool CanHoldSettlement(PlanetTile t, World world)
        {
            if (!t.Valid) return false;
            Tile tile = world.grid[t];
            if (tile == null || tile.PrimaryBiome == null || !tile.PrimaryBiome.canBuildBase || tile.WaterCovered) return false;
            if (tile.hilliness == Hilliness.Impassable) return false;
            return !world.worldObjects.AnyWorldObjectAt(t);
        }

        /// <summary>Tile livre mais perto do ponto pedido, longe das cidades já postas.</summary>
        public static PlanetTile FindTile(World world, PlanetLayer layer, float lat, float lon, float radius, int spacing, List<PlanetTile> taken)
        {
            WorldGrid grid = world.grid;
            PlanetTile best = PlanetTile.Invalid;
            float bestD = float.MaxValue;
            float cos = Mathf.Cos(lat * Mathf.Deg2Rad);
            for (int i = 0; i < layer.TilesCount; i++)
            {
                var t = new PlanetTile(i, layer);
                Vector2 ll = grid.LongLatOf(t);
                float dl = ll.y - lat, dn = (ll.x - lon) * cos;
                float d = dl * dl + dn * dn;
                if (d >= bestD || d > radius * radius) continue;
                if (!CanHoldSettlement(t, world)) continue;
                if (taken.Any(o => grid.ApproxDistanceInTiles(o, t) < spacing)) continue;
                best = t;
                bestD = d;
            }
            return best;
        }

        public static void Apply(World world, FixedWorldDef def)
        {
            var sb = new StringBuilder();
            PlanetLayer layer = world.grid.Surface;
            Placed.Clear();
            try
            {
                // 1) Cidades: as facções da lista perdem as sorteadas e ganham as fixas.
                var listed = new HashSet<FactionDef>(def.settlements.Select(s => s.faction).Where(f => f != null));
                int removed = 0;
                foreach (Settlement s in world.worldObjects.Settlements.Where(s => s.Faction != null && !s.Faction.IsPlayer && listed.Contains(s.Faction.def)).ToList())
                {
                    world.worldObjects.Remove(s);
                    removed++;
                }
                var taken = world.worldObjects.Settlements.Where(s => s.Tile.Layer == layer && CanHoldSettlementIgnoringSelf(s.Tile, world)).Select(s => s.Tile).ToList();
                int placed = 0, failed = 0;
                foreach (FixedSettlement fs in def.settlements)
                {
                    Faction f = fs.faction == null ? null : world.factionManager.FirstFactionOfDef(fs.faction);
                    if (f == null) { failed++; sb.Append(" sem_faccao:" + fs.faction?.defName); continue; }
                    PlanetTile t = FindTile(world, layer, fs.lat, fs.lon, def.searchRadius, def.minSpacing, taken);
                    if (!t.Valid) { failed++; sb.Append(" sem_tile:" + fs.name); continue; }
                    var settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.SetFaction(f);
                    settlement.Tile = t;
                    settlement.Name = fs.name;
                    world.worldObjects.Add(settlement);
                    taken.Add(t);
                    placed++;
                    Placed.Add(fs.name + "@" + t.tileId);
                }

                // 2) Cidades das outras facções (insetos…) que caíram na água ou num tile ruim depois do terreno do preset.
                int moved = 0;
                foreach (Settlement s in world.worldObjects.Settlements.Where(s => s.Faction != null && !s.Faction.IsPlayer && !listed.Contains(s.Faction.def)).ToList())
                {
                    if (CanHoldSettlementIgnoringSelf(s.Tile, world)) continue;
                    PlanetTile t = PlanetTile.Invalid;
                    for (int k = 0; k < 200 && !t.Valid; k++)
                    {
                        PlanetTile c = TileFinder.RandomSettlementTileFor(layer, s.Faction);
                        if (c.Valid && CanHoldSettlement(c, world) && !taken.Any(o => world.grid.ApproxDistanceInTiles(o, c) < def.minSpacing)) t = c;
                    }
                    if (!t.Valid) { world.worldObjects.Remove(s); continue; }
                    s.Tile = t;
                    taken.Add(t);
                    moved++;
                }
                sb.Insert(0, "removidas=" + removed + " postas=" + placed + " falhas=" + failed + " realocadas=" + moved);

                if (!def.fixedSeed.NullOrEmpty() && world.info.seedString == def.fixedSeed)
                {
                    Rand.PushState(Environment.TickCount ^ Guid.NewGuid().GetHashCode());
                    try { world.info.seedString = GenText.RandomSeedString(); }
                    finally { Rand.PopState(); }
                    sb.Append(" semente_nova=" + world.info.seedString);
                }
                int seed = GenText.StableStringHash(world.info.seedString);
                // 2b) Mutadores velhos: o preset troca o bioma mas não limpa os mutadores sorteados para o terreno
                // antigo (um "bosque" num bioma sem aquela planta quebra o rótulo). Limpa tudo e refaz.
                if (def.regenerateMutators)
                {
                    int stale = 0;
                    for (int i = 0; i < layer.TilesCount; i++)
                    {
                        Tile tile = world.grid[new PlanetTile(i, layer)];
                        if (tile.mutatorsNullable != null && tile.mutatorsNullable.Count > 0) stale++;
                        tile.mutatorsNullable = null;
                    }
                    world.landmarks.landmarks.Clear();
                    sb.Append(" mutadores_velhos=" + stale);
                }
                // 3) Marcos: o preset sai sem eles (variam por partida). Os tiles com cidade ficam de fora.
                if (def.regenerateLandmarks && ModsConfig.OdysseyActive)
                {
                    Rand.PushState(Gen.HashCombineInt(seed, 471293987));
                    try { new WorldGenStep_Landmarks().GenerateFresh(world.info.seedString, layer); }
                    finally { Rand.PopState(); }
                    sb.Append(" marcos=" + world.landmarks.landmarks.Count);
                }
                // 4) Mutadores de tile (costa, rio, montanha, cavernas…): o preset também os apaga.
                if (def.regenerateMutators)
                {
                    Rand.PushState(Gen.HashCombineInt(seed, 897213645));
                    try { WorldGenStep_Mutators.AddMutatorsFromTile(layer); }
                    finally { Rand.PopState(); }
                    int withMut = 0;
                    for (int i = 0; i < layer.TilesCount; i++) if (world.grid[new PlanetTile(i, layer)].Mutators.Any()) withMut++;
                    sb.Append(" tiles_com_mutador=" + withMut);
                }
                // 5) Estradas entre as cidades (o passo do jogo, agora com as cidades fixas).
                if (def.regenerateRoads)
                {
                    Rand.PushState(Gen.HashCombineInt(seed, 1538475135));
                    try { new WorldGenStep_Roads().GenerateFresh(world.info.seedString, layer); }
                    finally { Rand.PopState(); }
                    world.pathGrid.RecalculateAllLayersPathCosts();
                    int roads = 0;
                    for (int i = 0; i < layer.TilesCount; i++) if (world.grid[new PlanetTile(i, layer)] is SurfaceTile st && !st.Roads.NullOrEmpty()) roads++;
                    sb.Append(" tiles_com_estrada=" + roads);
                }
            }
            catch (Exception ex)
            {
                sb.Append(" EXCEÇÃO " + ex);
                Log.Error("[Ecos da Queda] Mundo fixo: " + ex);
            }
            LastReport = sb.ToString();
            Log.Message("[Ecos da Queda] Mundo fixo (" + def.defName + "): " + LastReport);
        }

        private static bool CanHoldSettlementIgnoringSelf(PlanetTile t, World world)
        {
            if (!t.Valid) return false;
            Tile tile = world.grid[t];
            return tile != null && tile.PrimaryBiome != null && tile.PrimaryBiome.canBuildBase && !tile.WaterCovered && tile.hilliness != Hilliness.Impassable;
        }
    }

    /// <summary>Depois do Worldbuilder aplicar o terreno do preset (postfix dele no mesmo método).</summary>
    [HarmonyPatch(typeof(World), nameof(World.FinalizeInit))]
    public static class Patch_World_FinalizeInit_FixedWorld
    {
        [HarmonyPriority(Priority.Last)]
        [HarmonyAfter("Worldbuilder", "ferny.worldbuilder", "com.ferny.worldbuilder")]
        public static void Postfix(World __instance, bool fromLoad)
        {
            if (fromLoad) return;
            FixedWorldDef def = EQFixedWorld.ActiveDef();
            if (def == null) return;
            EQFixedWorld.Apply(__instance, def);
        }
    }
}
