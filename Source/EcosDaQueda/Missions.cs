using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Missões com patrono (1.7/Defs/EQ_Missions.xml): concluir com sucesso uma missão da lista dá Fama à potência
    /// patrona e, se ela for a potência do líder, favor (favorSources PatronQuestCompleted). Os nomes das missões são
    /// texto (os mods de missão são opcionais).
    /// </summary>
    public class QuestPatronDef : Def
    {
        public FactionDef faction;
        public float fame = 3f;
        public List<string> quests = new List<string>();
        /// <summary>Missões que valem majorFame (o fim de cada cadeia).</summary>
        public List<string> majorQuests = new List<string>();
        public float majorFame = 6f;
        public string messageText;

        public bool Covers(QuestScriptDef q) => q != null && (quests.Contains(q.defName) || majorQuests.Contains(q.defName));

        public float FameFor(QuestScriptDef q) => majorQuests.Contains(q.defName) ? majorFame : fame;

        public static QuestPatronDef For(QuestScriptDef q) => DefDatabase<QuestPatronDef>.AllDefsListForReading.FirstOrDefault(d => d.Covers(q));
    }

    /// <summary>Fabricação licenciada de drones (Drone Factory): sem o cargo da potência, cada drone é pirataria.</summary>
    public class DronePolicyDef : Def
    {
        public FactionDef licensor;
        public int licenseRank = 1;
        public int piracyGoodwill = -3;
        public string piracyText;
        public string licensedText;

        public static DronePolicyDef Current => DefDatabase<DronePolicyDef>.AllDefsListForReading.FirstOrDefault();
    }

    public static class EQMissions
    {
        /// <summary>Contadores para teste.</summary>
        public static int PatronQuests, DronesPirated, DronesLicensed;

        public static void OnQuestSucceeded(Quest quest)
        {
            QuestPatronDef d = QuestPatronDef.For(quest?.root);
            if (d == null || Current.ProgramState != ProgramState.Playing)
                return;
            Faction f = EQUtil.MainFaction(d.faction);
            if (f == null || f.defeated)
                return;
            PatronQuests++;
            WorldComponent_Reputation.Current?.Record(f, d.FameFor(quest.root), 0f);
            if (!d.messageText.NullOrEmpty())
                Messages.Message(d.messageText.Formatted(f.Named("FACTION"), quest.name.Named("QUEST")), MessageTypeDefOf.PositiveEvent, historical: false);
            var alg = WorldComponent_Allegiance.Current;
            if (alg?.Def != null && alg.Def.faction == d.faction)
                EQFavor.Notify(FavorEventType.PatronQuestCompleted);
        }

        /// <summary>Drone saído do chocador do jogador. Devolve true se foi pirataria.</summary>
        public static bool OnDroneFabricated(GlobalTargetInfo where)
        {
            DronePolicyDef d = DronePolicyDef.Current;
            if (d == null || Current.ProgramState != ProgramState.Playing || Faction.OfPlayerSilentFail == null)
                return false;
            Faction lic = EQUtil.MainFaction(d.licensor);
            if (lic == null || lic.defeated)
                return false;
            if (EQAndroids.HasLicense(d.licensor, d.licenseRank))
            {
                DronesLicensed++;
                if (!d.licensedText.NullOrEmpty())
                    Messages.Message(d.licensedText.Formatted(lic.Named("FACTION")), new LookTargets(where), MessageTypeDefOf.NeutralEvent, historical: false);
                return false;
            }
            DronesPirated++;
            if (!d.piracyText.NullOrEmpty())
                Messages.Message(d.piracyText.Formatted(lic.Named("FACTION"), EQAndroids.LicenseTitleLabel(d.licensor, d.licenseRank).Named("TITLE")),
                    new LookTargets(where), MessageTypeDefOf.NegativeEvent, historical: false);
            if (d.piracyGoodwill != 0 && lic != Faction.OfPlayer)
                Faction.OfPlayer.TryAffectGoodwillWith(lic, d.piracyGoodwill, canSendMessage: true, canSendHostilityLetter: true);
            return true;
        }
    }

    [HarmonyPatch(typeof(Quest), nameof(Quest.End))]
    public static class Patch_Quest_End_Patron
    {
        public static void Prefix(Quest __instance, out bool __state)
        {
            __state = __instance.State == QuestState.Ongoing;
        }

        public static void Postfix(Quest __instance, QuestEndOutcome outcome, bool __state)
        {
            if (__state && outcome == QuestEndOutcome.Success && __instance.State == QuestState.EndedSuccess)
                EQMissions.OnQuestSucceeded(__instance);
        }
    }

    /// <summary>VQE Drone Factory: CompDroneHatcher.Hatch gera sempre um drone do jogador e destrói o item.</summary>
    [HarmonyPatch]
    public static class Patch_DroneHatcher_Hatch
    {
        private const string TypeName = "VanillaQuestsExpandedDroneFactory.CompDroneHatcher";

        public static bool Prepare() => AccessTools.TypeByName(TypeName) != null;

        public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName(TypeName), "Hatch");

        public static void Prefix(ThingComp __instance, out GlobalTargetInfo __state)
        {
            Thing t = __instance.parent;
            __state = t?.MapHeld != null ? new GlobalTargetInfo(t.PositionHeld, t.MapHeld) : GlobalTargetInfo.Invalid;
        }

        public static void Postfix(GlobalTargetInfo __state)
        {
            if (__state.IsValid)
                EQMissions.OnDroneFabricated(__state);
        }
    }
}
