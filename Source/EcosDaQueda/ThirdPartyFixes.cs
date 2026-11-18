using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Vanilla Exploration Expanded: TileMutatorWorker_Predators guarda a lista de predadores do primeiro mapa gerado
    /// na sessão (o worker é um só por mutador) e devolve null quando esse bioma não tem predador comum, o que quebra a
    /// geração do mapa ("Error in GenStep" em GenStep_Animals). Aqui a lista é calculada por bioma (e costa), e vazia
    /// em vez de null.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_VEE_Predators_GetAnimalKind
    {
        private static readonly Dictionary<(Type, BiomeDef, bool), List<PawnKindDef>> cache = new Dictionary<(Type, BiomeDef, bool), List<PawnKindDef>>();

        public static bool Prepare() => AccessTools.TypeByName("VanillaExplorationExpanded.TileMutatorWorker_Predators") != null;

        public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("VanillaExplorationExpanded.TileMutatorWorker_Predators"), "GetAnimalKind");

        public static bool Prefix(object __instance, PlanetTile tile, ref List<PawnKindDef> __result)
        {
            BiomeDef biome = Find.WorldGrid[tile].PrimaryBiome;
            bool coastal = tile.Tile.IsCoastal;
            var key = (__instance.GetType(), biome, coastal);
            if (!cache.TryGetValue(key, out List<PawnKindDef> list))
            {
                float min = Traverse.Create(__instance).Property("MinAnimalCommonality").GetValue<float>();
                list = new List<PawnKindDef>();
                foreach (PawnKindDef a in biome.AllWildAnimals)
                    if (a.RaceProps.predator && (biome.CommonalityOfAnimal(a) > min || (coastal && biome.CommonalityOfCoastalAnimal(a) > min)))
                        list.Add(a);
                cache[key] = list;
            }
            __result = list;
            return false;
        }
    }
}
