using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Bloco 4b: trincheiras (mod Trenches) nos cercos. Além dos sacos de areia e da artilharia do jogo, a facção do cerco
    /// cava linhas de trincheira entre o acampamento e a colônia (e, se configurado, arame farpado com passagens).
    /// Os materiais entram na conta do cerco (LordToil_Siege.Init soma o custo das plantas e manda por cápsula).
    /// </summary>
    public class SiegeTrenchDef : Def
    {
        public List<FactionDef> factions = new List<FactionDef>();
        public ThingDef trench;
        public ThingDef trenchStuff;
        public IntRange lines = new IntRange(1, 2);
        public IntRange length = new IntRange(5, 9);
        /// <summary>Distância da primeira linha ao centro do cerco, rumo à colônia.</summary>
        public IntRange distance = new IntRange(5, 7);
        /// <summary>Espaço entre linhas.</summary>
        public int lineGap = 3;
        public ThingDef wire;
        public ThingDef wireStuff;
        public float wireChance;
        /// <summary>Arame em trechos deste tamanho, separados por passagens de 2 casas (para o próprio assalto passar).</summary>
        public int wireSegment = 3;
    }

    public static class EQSieges
    {
        /// <summary>Último relatório (para o executor de testes).</summary>
        public static string LastReport = "-";

        public static SiegeTrenchDef For(Faction f)
            => f == null ? null : DefDatabase<SiegeTrenchDef>.AllDefsListForReading.FirstOrDefault(d => d.trench != null && d.factions.Contains(f.def));

        /// <summary>Ponto da colônia para onde as trincheiras olham: média dos colonos no mapa, senão o centro.</summary>
        public static IntVec3 Target(Map map)
        {
            List<Pawn> cols = map.mapPawns.FreeColonistsSpawned;
            if (cols.Count == 0)
                return map.Center;
            return new IntVec3(Mathf.RoundToInt((float)cols.Average(p => p.Position.x)), 0, Mathf.RoundToInt((float)cols.Average(p => p.Position.z)));
        }

        private static bool CanPlace(ThingDef def, IntVec3 c, Map map, ThingDef stuff, Faction faction)
        {
            if (!c.InBounds(map))
                return false;
            return GenConstruct.CanPlaceBlueprintAt_NewTemp(def, c, Rot4.North, map, false, null, null, stuff, true, true, true,
                t => (t is Blueprint || t is Frame) && t.Faction != null && t.Faction != faction && t.Faction.HostileTo(faction)).Accepted;
        }

        /// <summary>Plantas de trincheira (e arame) do cerco de <paramref name="faction"/> em torno de <paramref name="center"/>.</summary>
        public static IEnumerable<Blueprint_Build> Place(IntVec3 center, Map map, Faction faction)
        {
            SiegeTrenchDef s = For(faction);
            if (s == null || map == null)
                yield break;
            IntVec3 target = Target(map);
            int dx = target.x - center.x, dz = target.z - center.z;
            // Linha perpendicular à direção da colônia, alinhada aos eixos (a trincheira só liga em cruz).
            bool towardX = Mathf.Abs(dx) >= Mathf.Abs(dz);
            IntVec3 forward = towardX ? new IntVec3(dx >= 0 ? 1 : -1, 0, 0) : new IntVec3(0, 0, dz >= 0 ? 1 : -1);
            if (dx == 0 && dz == 0)
                forward = IntVec3.North;
            IntVec3 side = new IntVec3(forward.z, 0, forward.x);
            int nLines = s.lines.RandomInRange;
            int dist = s.distance.RandomInRange;
            int trenches = 0, wires = 0;
            for (int i = 0; i < nLines; i++)
            {
                int len = s.length.RandomInRange;
                IntVec3 mid = center + forward * (dist + i * s.lineGap) + side * Rand.RangeInclusive(-2, 2);
                for (int k = -len / 2; k < len - len / 2; k++)
                {
                    IntVec3 c = mid + side * k;
                    if (!CanPlace(s.trench, c, map, s.trenchStuff, faction))
                        continue;
                    trenches++;
                    yield return GenConstruct.PlaceBlueprintForBuild(s.trench, c, map, Rot4.North, faction, s.trenchStuff);
                }
            }
            if (s.wire != null && Rand.Chance(s.wireChance))
            {
                int len = s.length.max + 4;
                IntVec3 mid = center + forward * (dist + (nLines - 1) * s.lineGap + 3);
                for (int k = -len / 2; k < len - len / 2; k++)
                {
                    int pos = k + len / 2;
                    if (pos % (s.wireSegment + 2) >= s.wireSegment)
                        continue; // passagem
                    IntVec3 c = mid + side * k;
                    if (!CanPlace(s.wire, c, map, s.wireStuff, faction))
                        continue;
                    wires++;
                    yield return GenConstruct.PlaceBlueprintForBuild(s.wire, c, map, Rot4.North, faction, s.wireStuff);
                }
            }
            LastReport = s.defName + " linhas=" + nLines + " trincheiras=" + trenches + " arame=" + wires + " rumo=" + (towardX ? (forward.x > 0 ? "L" : "O") : (forward.z > 0 ? "N" : "S"));
        }
    }

    /// <summary>
    /// Cerco com mechs (Forja): o jogo pode escolher um mech como construtor e SetAsBuilder quebra (mech não tem perícias),
    /// repetindo o erro a cada tique do lorde. Só quem tem perícias e ajustes de trabalho constrói.
    /// </summary>
    [HarmonyPatch(typeof(LordToil_Siege), "CanBeBuilder")]
    public static class Patch_LordToil_Siege_CanBeBuilder
    {
        public static bool Prefix(Pawn p, ref bool __result)
        {
            if (p?.skills != null && p.workSettings != null)
                return true;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(SiegeBlueprintPlacer), nameof(SiegeBlueprintPlacer.PlaceBlueprints))]
    public static class Patch_SiegeBlueprintPlacer_PlaceBlueprints
    {
        public static void Postfix(ref IEnumerable<Blueprint_Build> __result, IntVec3 placeCenter, Map map, Faction placeFaction)
        {
            if (EQSieges.For(placeFaction) != null)
                __result = Append(__result, placeCenter, map, placeFaction);
        }

        private static IEnumerable<Blueprint_Build> Append(IEnumerable<Blueprint_Build> original, IntVec3 center, Map map, Faction faction)
        {
            foreach (Blueprint_Build b in original)
                yield return b;
            List<Blueprint_Build> extra = new List<Blueprint_Build>();
            try
            {
                extra.AddRange(EQSieges.Place(center, map, faction));
            }
            catch (System.Exception e)
            {
                Log.Warning("[Ecos da Queda] trincheiras do cerco: " + e);
            }
            foreach (Blueprint_Build b in extra)
                yield return b;
        }
    }
}
