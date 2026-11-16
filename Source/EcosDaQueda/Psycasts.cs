using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Caminhos psíquicos licenciados (Vanilla Psycasts Expanded). Cada cargo de uma potência pode licenciar um
    /// caminho (AllegianceRank.psycastPath). Para colonos do jogador o caminho só destrava com aquele cargo ou
    /// acima; a cerimônia de outorga destrava de graça; perder o cargo revoga (ensureLockRequirement do VPE).
    /// NPCs não são afetados. Tudo por reflexão: sem o VPE, nada acontece.
    /// </summary>
    public static class EQPsycasts
    {
        public struct License
        {
            public FactionAllegianceDef allegiance;
            public int rank;
        }

        private static Type pathType;
        private static Type abilitiesType;
        private static FieldInfo fUnlockedPaths;
        private static FieldInfo fPrevUnlockedPaths;
        private static FieldInfo fPoints;
        private static MethodInfo mCanPawnUnlock;
        private static MethodInfo mRecheckPaths;

        /// <summary>Caminho (PsycasterPathDef) → licença.</summary>
        public static readonly Dictionary<Def, License> Licenses = new Dictionary<Def, License>();

        public static bool Active => pathType != null;

        public static void Apply()
        {
            pathType = AccessTools.TypeByName("VanillaPsycastsExpanded.PsycasterPathDef");
            abilitiesType = AccessTools.TypeByName("VanillaPsycastsExpanded.Hediff_PsycastAbilities");
            if (pathType == null || abilitiesType == null)
            {
                pathType = null;
                return;
            }
            fUnlockedPaths = AccessTools.Field(abilitiesType, "unlockedPaths");
            fPrevUnlockedPaths = AccessTools.Field(abilitiesType, "previousUnlockedPaths");
            fPoints = AccessTools.Field(abilitiesType, "points");
            mCanPawnUnlock = AccessTools.Method(pathType, "CanPawnUnlock");
            mRecheckPaths = AccessTools.Method(AccessTools.TypeByName("VanillaPsycastsExpanded.PsycastUtility"), "RecheckPaths");

            foreach (FactionAllegianceDef alg in DefDatabase<FactionAllegianceDef>.AllDefsListForReading)
            {
                for (int i = 0; i < alg.ranks.Count; i++)
                {
                    AllegianceRank r = alg.ranks[i];
                    if (r.psycastPath.NullOrEmpty())
                        continue;
                    Def path = GenDefDatabase.GetDefSilentFail(pathType, r.psycastPath, false);
                    if (path == null)
                        continue; // caminho de mod ausente (ex.: Hemosage)
                    if (Licenses.ContainsKey(path))
                    {
                        Log.Warning("[EcosDaQueda] caminho psíquico licenciado duas vezes: " + r.psycastPath);
                        continue;
                    }
                    Licenses[path] = new License { allegiance = alg, rank = i };
                    // Licença sempre conferida (revoga ao perder o cargo) e não se contorna com psitreinador.
                    Traverse t = Traverse.Create(path);
                    t.Field("ensureLockRequirement").SetValue(true);
                    t.Field("ignoreLockRestrictionsForNeurotrainers").SetValue(false);
                    string reason = r.psycastLockedReason;
                    if (reason.NullOrEmpty())
                        reason = "licença do " + alg.faction.LabelCap + " (" + r.title.label + ")";
                    t.Field("lockedReason").SetValue(reason);
                }
            }
        }

        /// <summary>Verdadeiro se o colono do jogador não tem o cargo que licencia o caminho.</summary>
        public static bool LacksLicense(Def path, Pawn pawn)
        {
            if (pawn == null || !Licenses.TryGetValue(path, out License lic))
                return false;
            if (Current.ProgramState != ProgramState.Playing || pawn.Faction == null || !pawn.Faction.IsPlayer)
                return false;
            return !HasRank(pawn, lic);
        }

        public static bool HasRank(Pawn pawn, License lic)
        {
            if (pawn.royalty == null)
                return false;
            foreach (RoyalTitle t in pawn.royalty.AllTitlesForReading)
                if (lic.allegiance.RankIndexOf(t.def) >= lic.rank)
                    return true;
            return false;
        }

        public static Hediff AbilitiesHediff(Pawn pawn)
        {
            if (!Active || pawn?.health?.hediffSet == null)
                return null;
            return pawn.health.hediffSet.hediffs.FirstOrDefault(h => abilitiesType.IsInstanceOfType(h));
        }

        public static IList<Def> UnlockedPaths(Pawn pawn)
        {
            Hediff h = AbilitiesHediff(pawn);
            return h == null ? new List<Def>() : ((System.Collections.IEnumerable)fUnlockedPaths.GetValue(h)).Cast<Def>().ToList();
        }

        public static bool CanPawnUnlock(Def path, Pawn pawn) => (bool)mCanPawnUnlock.Invoke(path, new object[] { pawn });

        public static void Recheck(Pawn pawn)
        {
            if (mRecheckPaths != null && AbilitiesHediff(pawn) != null)
                mRecheckPaths.Invoke(null, new object[] { pawn });
        }

        /// <summary>Destrava de graça os caminhos licenciados pelos cargos que o colono tem. Devolve os novos.</summary>
        public static List<Def> GrantLicensed(Pawn pawn)
        {
            var granted = new List<Def>();
            Hediff h = AbilitiesHediff(pawn);
            if (h == null)
                return granted;
            Recheck(pawn); // devolve antes os que estavam revogados
            var unlocked = (System.Collections.IList)fUnlockedPaths.GetValue(h);
            var previous = (System.Collections.IList)fPrevUnlockedPaths.GetValue(h);
            foreach (KeyValuePair<Def, License> kv in Licenses)
            {
                if (unlocked.Contains(kv.Key) || (previous != null && previous.Contains(kv.Key)))
                    continue;
                if (!HasRank(pawn, kv.Value) || !CanPawnUnlock(kv.Key, pawn))
                    continue;
                unlocked.Add(kv.Key);
                granted.Add(kv.Key);
            }
            return granted;
        }

        public static string Describe(Pawn pawn)
        {
            if (!Active)
                return "VPE ausente";
            Hediff h = AbilitiesHediff(pawn);
            if (h == null)
                return "sem poderes psíquicos";
            string lic = string.Join(", ", Licenses.Keys.Select(p => p.defName + (CanPawnUnlock(p, pawn) ? "+" : "-")));
            return "caminhos=[" + string.Join(", ", UnlockedPaths(pawn).Select(p => p.defName)) + "] pontos=" + fPoints.GetValue(h)
                + " | licenças(+ pode/- não)=[" + lic + "]";
        }
    }

    /// <summary>Trava os caminhos licenciados para colonos sem o cargo.</summary>
    [HarmonyPatch]
    public static class Patch_VPE_CanPawnUnlock
    {
        public static bool Prepare() => AccessTools.TypeByName("VanillaPsycastsExpanded.PsycasterPathDef") != null;

        public static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("VanillaPsycastsExpanded.PsycasterPathDef"), "CanPawnUnlock");

        public static void Postfix(Def __instance, Pawn pawn, ref bool __result)
        {
            if (__result && EQPsycasts.LacksLicense(__instance, pawn))
                __result = false;
        }
    }

    /// <summary>Fim da cerimônia: o cargo novo destrava o caminho que ele licencia.</summary>
    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_Bestowing), nameof(RitualOutcomeEffectWorker_Bestowing.Apply))]
    public static class Patch_Bestowing_GrantLicensedPaths
    {
        public static void Postfix(LordJob_Ritual jobRitual)
        {
            if (!EQPsycasts.Active || !(jobRitual is LordJob_BestowingCeremony job))
                return;
            Pawn target = job.target;
            if (target == null || target.Dead || target.Faction == null || !target.Faction.IsPlayer)
                return;
            List<Def> granted = EQPsycasts.GrantLicensed(target);
            if (granted.Count > 0)
                Messages.Message(target.LabelShort + " recebeu licença para o caminho psíquico: "
                    + string.Join(", ", granted.Select(p => p.LabelCap.ToString())) + ". Use com afinação.",
                    target, MessageTypeDefOf.PositiveEvent);
        }
    }

    /// <summary>Mudou o cargo (promoção, renúncia, perda): o VPE confere de novo as licenças.</summary>
    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.SetTitle))]
    public static class Patch_RoyaltyTracker_SetTitle_RecheckPaths
    {
        public static void Postfix(Pawn_RoyaltyTracker __instance)
        {
            if (EQPsycasts.Active && Current.ProgramState == ProgramState.Playing)
                EQPsycasts.Recheck(__instance.pawn);
        }
    }
}
