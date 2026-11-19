using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Etapa 6a: o que o narrador próprio sabe dos nossos sistemas (extensão no StorytellerDef).</summary>
    public class EQStorytellerExtension : DefModExtension
    {
        /// <summary>Chance de o narrador escolher o atacante (no resto, o sorteio do jogo).</summary>
        public float pickChance = 0.75f;
        /// <summary>Peso extra por Infâmia: potência hostil pesa 1 + infâmia / infamyPerWeight.</summary>
        public float infamyPerWeight = 40f;
        /// <summary>Facções do começo de jogo (Sindicato): peso earlyWeight no dia 0, caindo até 1 em earlyDays.</summary>
        public List<FactionDef> earlyFactions = new List<FactionDef>();
        public float earlyWeight = 5f;
        public float earlyDays = 20f;
        /// <summary>Fim de jogo segurado: teto de pontos de ameaça e incidentes bloqueados até o jogador ter cargo
        /// (índice unlockRank numa potência), pesquisa do nível unlockTech ou passar de unlockDays.</summary>
        public float heldPointsCap = 2500f;
        public int unlockRank = 1;
        public TechLevel unlockTech = TechLevel.Spacer;
        public float unlockDays = 150f;
        public List<string> heldIncidents = new List<string>();
    }

    public static class EQNarrator
    {
        public static EQStorytellerExtension Ext => Find.Storyteller?.def?.GetModExtension<EQStorytellerExtension>();

        public static string LastPick;
        private static HashSet<IncidentDef> held;

        public static HashSet<IncidentDef> Held(EQStorytellerExtension e)
        {
            if (held == null)
                held = new HashSet<IncidentDef>(e.heldIncidents.Select(n => DefDatabase<IncidentDef>.GetNamedSilentFail(n)).Where(d => d != null));
            return held;
        }

        /// <summary>Por que o fim de jogo já está liberado (null = ainda segurado).</summary>
        public static string UnlockReason(EQStorytellerExtension e)
        {
            var al = WorldComponent_Allegiance.Current;
            if (al?.memberOf != null && al.RankIndex >= e.unlockRank) return "cargo " + al.RankLabel + " em " + al.memberOf.Name;
            ResearchProjectDef r = DefDatabase<ResearchProjectDef>.AllDefsListForReading.FirstOrDefault(p => p.techLevel >= e.unlockTech && p.IsFinished && p.knowledgeCategory == null);
            if (r != null) return "pesquisa " + r.LabelCap;
            if (GenDate.DaysPassedSinceSettleFloat >= e.unlockDays) return "dia " + e.unlockDays.ToString("0");
            return null;
        }

        public static bool Holding(out EQStorytellerExtension e)
        {
            e = Ext;
            return e != null && UnlockReason(e) == null;
        }

        /// <summary>Peso do narrador: frequência de ataque do próprio jogo (pelos pontos) × Infâmia × começo de jogo.</summary>
        public static float Weight(Faction f, EQStorytellerExtension e, float points = -1f)
        {
            float w = points > 0f ? f.def.RaidCommonalityFromPoints(points) : 1f;
            if (w <= 0f) return 0f;
            ReputationRecord rec = EQAllegiance.IsPower(EQUtil.Canonical(f)) ? WorldComponent_Reputation.Current?.Get(EQUtil.Canonical(f)) : null;
            if (rec != null) w *= 1f + rec.infamy / Mathf.Max(1f, e.infamyPerWeight);
            if (e.earlyFactions.Contains(f.def))
                w *= Mathf.Lerp(e.earlyWeight, 1f, Mathf.Clamp01(GenDate.DaysPassedSinceSettleFloat / Mathf.Max(1f, e.earlyDays)));
            return w;
        }

        private static readonly System.Reflection.MethodInfo canBeSource = AccessTools.Method(typeof(IncidentWorker_RaidEnemy), "FactionCanBeGroupSource",
            new[] { typeof(Faction), typeof(IncidentParms), typeof(bool) });

        /// <summary>Atacantes possíveis do ataque (hostis, que o jogo aceitaria, com pontos para um grupo de combate).</summary>
        public static List<Faction> Candidates(IncidentWorker_RaidEnemy worker, IncidentParms parms)
        {
            var list = new List<Faction>();
            foreach (Faction f in Find.FactionManager.AllFactionsListForReading)
            {
                if (f.IsPlayer || f.defeated || f.deactivated || !f.HostileTo(Faction.OfPlayer) || f.def.pawnGroupMakers.NullOrEmpty()) continue;
                try { if (canBeSource != null && !(bool)canBeSource.Invoke(worker, new object[] { f, parms, false })) continue; }
                catch { continue; }
                if (parms.points < f.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)) continue;
                list.Add(f);
            }
            return list;
        }

        public static bool TryPick(IncidentWorker_RaidEnemy worker, IncidentParms parms, EQStorytellerExtension e, out Faction pick)
        {
            pick = null;
            List<Faction> c = Candidates(worker, parms);
            float pts = parms.points;
            if (c.Count == 0 || c.Sum(f => Weight(f, e, pts)) <= 0f || !c.TryRandomElementByWeight(f => Weight(f, e, pts), out pick)) return false;
            LastPick = pick.Name + " entre " + c.Count + " (" + string.Join(", ", c.Select(f => f.def.defName.Replace("EQ_Faction_", "") + "=" + Weight(f, e, pts).ToString("0.00"))) + ")";
            return true;
        }
    }

    /// <summary>O narrador escolhe quem ataca: Infâmia com potências hostis e o Sindicato no começo.</summary>
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
    public static class Patch_RaidEnemy_TryResolveRaidFaction_Narrator
    {
        public static void Prefix(IncidentWorker_RaidEnemy __instance, IncidentParms parms)
        {
            if (parms == null || parms.faction != null || parms.forced) return;
            EQStorytellerExtension e = EQNarrator.Ext;
            if (e == null || !Rand.Chance(e.pickChance)) return;
            try { if (EQNarrator.TryPick(__instance, parms, e, out Faction f)) parms.faction = f; }
            catch (Exception ex) { Log.Warning("[Ecos da Queda] narrador: " + ex.Message); }
        }
    }

    /// <summary>Fim de jogo segurado: teto de pontos de ameaça.</summary>
    [HarmonyPatch(typeof(StorytellerUtility), nameof(StorytellerUtility.DefaultThreatPointsNow))]
    public static class Patch_StorytellerUtility_DefaultThreatPointsNow_Narrator
    {
        public static void Postfix(ref float __result)
        {
            if (EQNarrator.Holding(out EQStorytellerExtension e) && __result > e.heldPointsCap) __result = e.heldPointsCap;
        }
    }

    /// <summary>Fim de jogo segurado: incidentes grandes da lista não disparam.</summary>
    [HarmonyPatch(typeof(IncidentWorker), nameof(IncidentWorker.CanFireNow))]
    public static class Patch_IncidentWorker_CanFireNow_Narrator
    {
        public static void Postfix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        {
            if (!__result || parms != null && parms.forced) return;
            EQStorytellerExtension e = EQNarrator.Ext;
            if (e == null || e.heldIncidents.Count == 0 || !EQNarrator.Held(e).Contains(__instance.def)) return;
            if (EQNarrator.UnlockReason(e) == null) __result = false;
        }
    }
}
