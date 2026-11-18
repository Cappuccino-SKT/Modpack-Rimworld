using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace EcosDaQueda
{
    /// <summary>
    /// Etapa 5d: os Fungobi (zumbis do mundo) numa facção oculta e sempre hostil (EQ_Faction_Fungobi), sem ataques
    /// comuns. Aparecem na "horda Fungobi" (incidente) e morando nas ruínas de assentamentos destruídos.
    /// </summary>
    public static class EQFungobi
    {
        public static Faction Faction => Find.FactionManager?.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamedSilentFail("EQ_Faction_Fungobi"));

        /// <summary>Último relatório (para o executor de testes).</summary>
        public static string LastReport;

        /// <summary>Forçar moradores na próxima geração de ruína (teste).</summary>
        public static bool ForceRuin;

        public static List<Pawn> Generate(Faction f, float points, Map map)
        {
            var parms = new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                faction = f,
                points = points,
                tile = map.Tile,
                inhabitants = true,
            };
            return PawnGroupMakerUtility.GeneratePawns(parms).ToList();
        }

        /// <summary>Ruína de assentamento destruído: os mortos da guerra levantam como Fungobi e ficam guardando o lugar.</summary>
        public static void OnMapGenerated(Map map)
        {
            if (map == null || !(map.Parent is DestroyedSettlement)) return;
            Faction f = Faction;
            if (f == null) return;
            if (!ForceRuin && !Rand.Chance(0.7f)) return;
            ForceRuin = false;
            float points = Rand.Range(250f, 700f);
            List<Pawn> pawns = Generate(f, points, map);
            if (pawns.Count == 0) return;
            IntVec3 center = CellFinder.RandomClosewalkCellNear(map.Center, map, 10);
            foreach (Pawn p in pawns)
            {
                IntVec3 cell = CellFinder.RandomClosewalkCellNear(center, map, 12);
                GenSpawn.Spawn(p, cell, map);
            }
            LordMaker.MakeNewLord(f, new LordJob_DefendPoint(center, 18f, 30f), map, pawns);
            LastReport = "ruína com " + pawns.Count + " Fungobi (" + points.ToString("0") + " pontos): " + string.Join(",", pawns.GroupBy(p => p.kindDef.defName).Select(g => g.Key + "x" + g.Count()));
        }
    }

    /// <summary>Horda Fungobi: ataque da facção oculta, sempre a pé pela borda, sem fuga.</summary>
    public class IncidentWorker_EQFungobiHorde : IncidentWorker_RaidEnemy
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Faction f = EQFungobi.Faction;
            return f != null && parms.target is Map && base.CanFireNowSub(parms);
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            parms.faction = EQFungobi.Faction;
            return parms.faction != null;
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        public override void ResolveRaidArriveMode(IncidentParms parms)
        {
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
        }

        protected override string GetLetterLabel(IncidentParms parms) => def.letterLabel ?? base.GetLetterLabel(parms);

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = def.letterText ?? base.GetLetterText(parms, pawns);
            return text + "\n\n" + pawns.Count + " Fungobi a caminho.";
        }
    }

    [HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
    public static class Patch_MapGenerator_GenerateMap_Fungobi
    {
        public static void Postfix(Map __result)
        {
            try
            {
                EQFungobi.OnMapGenerated(__result);
            }
            catch (System.Exception e)
            {
                Log.Warning("[Ecos da Queda] Fungobi na ruína: " + e);
            }
        }
    }
}
