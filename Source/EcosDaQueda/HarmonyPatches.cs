using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    [StaticConstructorOnStartup]
    public static class EcosDaQuedaStartup
    {
        static EcosDaQuedaStartup()
        {
            new Harmony("ecosdaqueda.core").PatchAll(typeof(EcosDaQuedaStartup).Assembly);
            EQWorldFactions.Apply();
            EQGameStart.Apply();
            EQArsenal.Apply();
            EQTrade.Apply();
            EQPsycasts.Apply();
            EQAndroids.Apply();
            EQVehicles.Apply();
            EQSanity.Apply();
            EQResearch.Apply();
            EQResearchUnlocks.ApplyAll();
            EQThirdPartyFixes.CleanBadRules();
        }
    }

    /// <summary>Toda mudança de boa vontade entre o jogador e uma potência vira Fama (+) ou Infâmia (-).</summary>
    [HarmonyPatch(typeof(Faction), nameof(Faction.TryAffectGoodwillWith))]
    public static class Patch_Faction_TryAffectGoodwillWith
    {
        /// <summary>Ligado enquanto o próprio mod muda a boa vontade e já registrou a reputação (evita contar em dobro).</summary>
        public static bool Suppress;

        public static void Postfix(Faction __instance, Faction other, int goodwillChange, bool __result)
        {
            if (Suppress)
                return;
            if (!__result || goodwillChange == 0 || __instance == null || other == null)
                return;
            Faction npc = __instance.IsPlayer ? other : other.IsPlayer ? __instance : null;
            npc = EQUtil.Canonical(npc);
            if (npc == null)
                return;
            var rep = WorldComponent_Reputation.Current;
            // Record decide: potência ganha Fama/Infâmia própria; facção menor só repercute nas potências.
            if (rep == null)
                return;
            if (goodwillChange > 0)
                rep.Record(npc, goodwillChange, 0f);
            else
                rep.Record(npc, 0f, -goodwillChange);
        }
    }

    /// <summary>O título de reputação com a facção do comerciante melhora ou piora os preços.</summary>
    [HarmonyPatch(typeof(TradeUtility), nameof(TradeUtility.GetPricePlayerBuy))]
    public static class Patch_TradeUtility_GetPricePlayerBuy
    {
        public static void Prefix(ref float priceGain_FactionBase) => priceGain_FactionBase += ReputationTrade.CurrentGain();
    }

    [HarmonyPatch(typeof(TradeUtility), nameof(TradeUtility.GetPricePlayerSell))]
    public static class Patch_TradeUtility_GetPricePlayerSell
    {
        public static void Prefix(ref float priceGain_FactionBase) => priceGain_FactionBase += ReputationTrade.CurrentGain();
    }

    public static class ReputationTrade
    {
        public static float CurrentGain()
        {
            if (!TradeSession.Active)
                return 0f;
            Faction f = EQUtil.Canonical(TradeSession.trader?.Faction);
            ReputationTier tier = WorldComponent_Reputation.Current?.TierFor(f);
            float gain = tier?.priceGain ?? 0f;
            var alg = WorldComponent_Allegiance.Current;
            if (alg != null && f != null && alg.memberOf == f)
                gain += alg.Rank?.priceGain ?? 0f;
            return gain;
        }
    }

    /// <summary>Matar membros de uma potência (mesmo hostis) gera Infâmia com ela.</summary>
    [HarmonyPatch(typeof(Faction), nameof(Faction.Notify_MemberDied))]
    public static class Patch_Faction_Notify_MemberDied
    {
        public static void Postfix(Faction __instance, Pawn member, DamageInfo? dinfo)
        {
            if (member == null || !dinfo.HasValue || dinfo.Value.Instigator?.Faction == null || !dinfo.Value.Instigator.Faction.IsPlayer)
                return;
            if (PawnGenerator.IsBeingGenerated(member))
                return;
            var rep = WorldComponent_Reputation.Current;
            var s = ReputationSettingsDef.Settings;
            if (rep == null || s == null || !WorldComponent_Reputation.Tracks(EQUtil.Canonical(__instance)))
                return;
            rep.Record(__instance, 0f, member.RaceProps.Humanlike ? s.infamyPerHumanKilled : s.infamyPerOtherKilled);
        }
    }

    /// <summary>Capturar membros de uma potência gera Infâmia com ela.</summary>
    [HarmonyPatch(typeof(Faction), nameof(Faction.Notify_MemberCaptured))]
    public static class Patch_Faction_Notify_MemberCaptured
    {
        public static void Postfix(Faction __instance, Pawn member, Faction violator)
        {
            if (violator == null || !violator.IsPlayer)
                return;
            var rep = WorldComponent_Reputation.Current;
            var s = ReputationSettingsDef.Settings;
            if (rep == null || s == null || !WorldComponent_Reputation.Tracks(EQUtil.Canonical(__instance)))
                return;
            rep.Record(__instance, 0f, s.infamyPerCapture);
        }
    }
}
