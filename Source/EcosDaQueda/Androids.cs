using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// "Questão androide" (1.7/Defs/EQ_Androids.xml): a potência dona da licença (Forja) cobra Infâmia de quem fabrica
    /// androide sem o cargo que licencia, e de quem abriga androide desperto; os libertadores (Fora de Linha) gostam.
    /// As mudanças são de boa vontade: o sistema de reputação já converte em Fama/Infâmia e repercute nas potências.
    /// </summary>
    public class AndroidPolicyDef : Def
    {
        public FactionDef licensor;
        /// <summary>Índice do cargo (0 = primeiro) da potência que licencia a fabricação.</summary>
        public int licenseRank = 1;
        public int piracyGoodwill = -8;
        public string piracyText;
        public string licensedText;
        public int awakenedGoodwill = -10;
        public FactionDef liberator;
        public int liberatorGoodwill = 10;
        public string awakenedText;

        public static AndroidPolicyDef Current => DefDatabase<AndroidPolicyDef>.AllDefsListForReading.FirstOrDefault();
    }

    public static class EQAndroids
    {
        private static MethodInfo mIsAndroid;
        private static MethodInfo mIsAwakened;

        public static bool Active => mIsAwakened != null;

        public static void Apply()
        {
            Type utils = AccessTools.TypeByName("VREAndroids.Utils");
            if (utils == null)
                return;
            mIsAndroid = AccessTools.Method(utils, "IsAndroid", new[] { typeof(Pawn) });
            mIsAwakened = AccessTools.Method(utils, "IsAwakened", new[] { typeof(Pawn) });
        }

        public static bool IsAndroid(Pawn p) => Active && p?.genes != null && (bool)mIsAndroid.Invoke(null, new object[] { p });

        public static bool IsAwakenedAndroid(Pawn p) => IsAndroid(p) && (bool)mIsAwakened.Invoke(null, new object[] { p });

        /// <summary>O líder do jogador tem o cargo que licencia a fabricação?</summary>
        public static bool HasLicense(AndroidPolicyDef d) => HasLicense(d.licensor, d.licenseRank);

        /// <summary>Algum colono tem cargo da potência igual ou acima de rank?</summary>
        public static bool HasLicense(FactionDef licensor, int rank)
        {
            FactionAllegianceDef alg = FactionAllegianceDef.For(licensor);
            if (alg == null || rank < 0 || rank >= alg.ranks.Count)
                return false;
            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
                if (p.royalty != null && p.royalty.AllTitlesForReading.Any(t => alg.RankIndexOf(t.def) >= rank))
                    return true;
            return false;
        }

        public static string LicenseTitleLabel(AndroidPolicyDef d) => LicenseTitleLabel(d.licensor, d.licenseRank);

        public static string LicenseTitleLabel(FactionDef licensor, int rank)
        {
            FactionAllegianceDef alg = FactionAllegianceDef.For(licensor);
            return alg != null && rank >= 0 && rank < alg.ranks.Count ? alg.ranks[rank].title.label : "?";
        }

        private static void Goodwill(FactionDef def, int amount)
        {
            if (def == null || amount == 0)
                return;
            Faction f = EQUtil.MainFaction(def);
            if (f == null || f.defeated || f == Faction.OfPlayer)
                return;
            Faction.OfPlayer.TryAffectGoodwillWith(f, amount, canSendMessage: true, canSendHostilityLetter: true);
        }

        /// <summary>Androide fabricado na colônia. Devolve true se foi pirataria.</summary>
        public static bool OnFabricated(Thing station)
        {
            AndroidPolicyDef d = AndroidPolicyDef.Current;
            if (d == null || Current.ProgramState != ProgramState.Playing)
                return false;
            Faction lic = EQUtil.MainFaction(d.licensor);
            if (HasLicense(d))
            {
                if (!d.licensedText.NullOrEmpty() && lic != null)
                    Messages.Message(d.licensedText.Formatted(lic.Named("FACTION")), station, MessageTypeDefOf.NeutralEvent);
                return false;
            }
            if (!d.piracyText.NullOrEmpty() && lic != null)
                Messages.Message(d.piracyText.Formatted(lic.Named("FACTION"), LicenseTitleLabel(d).Named("TITLE")), station, MessageTypeDefOf.NegativeEvent);
            Goodwill(d.licensor, d.piracyGoodwill);
            return true;
        }

        /// <summary>Androide desperto passou a ser do jogador (despertou aqui, entrou já desperto ou saiu desperto da estação).</summary>
        public static void OnAwakenedInColony(Pawn p)
        {
            AndroidPolicyDef d = AndroidPolicyDef.Current;
            if (d == null || p == null || Current.ProgramState != ProgramState.Playing)
                return;
            Faction lic = EQUtil.MainFaction(d.licensor);
            if (!d.awakenedText.NullOrEmpty() && lic != null)
                Messages.Message(d.awakenedText.Formatted(p.Named("PAWN"), lic.Named("FACTION")), p, MessageTypeDefOf.NegativeEvent);
            Goodwill(d.licensor, d.awakenedGoodwill);
            Goodwill(d.liberator, d.liberatorGoodwill);
        }

        /// <summary>O projeto da estação gera androide já desperto (sem nenhum gene que some ao despertar)?</summary>
        public static bool ProjectIsAwakened(object project)
        {
            if (project == null)
                return false;
            var genes = Traverse.Create(project).Field("genes").GetValue() as IEnumerable;
            if (genes == null)
                return false;
            foreach (object g in genes)
            {
                FieldInfo f = g?.GetType().GetField("removeWhenAwakened");
                if (f != null && (bool)f.GetValue(g))
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    public static class Patch_VREA_FinishAndroidProject
    {
        public static bool Prepare() => AccessTools.TypeByName("VREAndroids.Building_AndroidCreationStation") != null;

        public static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("VREAndroids.Building_AndroidCreationStation"), "FinishAndroidProject");

        public static void Prefix(Building __instance, out bool __state)
        {
            __state = EQAndroids.ProjectIsAwakened(Traverse.Create(__instance).Field("curAndroidProject").GetValue());
        }

        public static void Postfix(Building __instance, bool __state)
        {
            EQAndroids.OnFabricated(__instance);
            if (!__state || __instance.Map == null)
                return;
            Pawn p = __instance.Position.GetThingList(__instance.Map).OfType<Pawn>()
                .FirstOrDefault(x => x.Faction == Faction.OfPlayer && x.ageTracker.AgeBiologicalTicks < 100 && EQAndroids.IsAwakenedAndroid(x));
            if (p != null)
                EQAndroids.OnAwakenedInColony(p);
        }
    }

    [HarmonyPatch]
    public static class Patch_VREA_Awaken
    {
        public static bool Prepare() => AccessTools.TypeByName("VREAndroids.Gene_SyntheticBody") != null;

        public static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("VREAndroids.Gene_SyntheticBody"), "Awaken");

        public static void Postfix(Gene __instance)
        {
            Pawn p = __instance.pawn;
            if (p?.Faction != null && p.Faction.IsPlayer)
                EQAndroids.OnAwakenedInColony(p);
        }
    }

    /// <summary>Androide já desperto entrando na colônia (recrutado, missão, refugiado).</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
    public static class Patch_Pawn_SetFaction_Android
    {
        public static void Prefix(Pawn __instance, out bool __state)
        {
            __state = __instance.Faction != null && __instance.Faction.IsPlayer;
        }

        public static void Postfix(Pawn __instance, Faction newFaction, bool __state)
        {
            if (__state || newFaction == null || !newFaction.IsPlayer || Current.ProgramState != ProgramState.Playing)
                return;
            if (EQAndroids.IsAwakenedAndroid(__instance))
                EQAndroids.OnAwakenedInColony(__instance);
        }
    }
}
