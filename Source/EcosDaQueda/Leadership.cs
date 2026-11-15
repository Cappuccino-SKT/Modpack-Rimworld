using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace EcosDaQueda
{
    // ===================================================================== Líder

    /// <summary>O líder do assentamento é o líder da ideologia principal do jogador.</summary>
    public static class EQLeader
    {
        /// <summary>O executor de testes desliga as janelas.</summary>
        public static bool SuppressDialogs;

        public static Ideo PlayerIdeo => Faction.OfPlayer?.ideos?.PrimaryIdeo;

        public static Precept_RoleSingle LeaderRole =>
            PlayerIdeo?.PreceptsListForReading.OfType<Precept_RoleSingle>().FirstOrDefault(r => r.def.leaderRole);

        public static Pawn IdeoLeader
        {
            get
            {
                Pawn p = LeaderRole?.ChosenPawnSingle();
                return p != null && !p.Dead && !p.Destroyed && p.Faction == Faction.OfPlayer && p.IsFreeNonSlaveColonist ? p : null;
            }
        }

        public static IEnumerable<Pawn> AllFreeColonists()
        {
            foreach (Map map in Find.Maps)
                foreach (Pawn p in map.mapPawns.FreeColonists)
                    yield return p;
            foreach (Caravan c in Find.WorldObjects.Caravans)
                if (c.IsPlayerControlled)
                    foreach (Pawn p in c.PawnsListForReading)
                        if (p.IsFreeColonist)
                            yield return p;
        }

        public static bool IsValidCandidate(Pawn p, bool strict)
        {
            if (p == null || p.Dead || p.Destroyed || p.Faction != Faction.OfPlayer || !p.IsFreeNonSlaveColonist || p.IsQuestLodger())
                return false;
            if (!p.RaceProps.Humanlike || !DevelopmentalStageExtensions.Adult(p.DevelopmentalStage) || p.royalty == null)
                return false;
            Precept_RoleSingle role = LeaderRole;
            if (role == null || p.Ideo != PlayerIdeo)
                return false;
            return !strict || role.RequirementsMet(p);
        }

        /// <summary>Quem pode ser líder: adultos livres que seguem a ideologia e cumprem as exigências do cargo de líder dela.</summary>
        public static List<Pawn> Candidates() => AllFreeColonists().Distinct().Where(p => IsValidCandidate(p, true)).ToList();

        public static bool Assign(Pawn p)
        {
            Precept_RoleSingle role = LeaderRole;
            if (role == null || !IsValidCandidate(p, strict: true))
                return false;
            role.Assign(p, addThoughts: true);
            return role.ChosenPawnSingle() == p;
        }

        public static void TryOpenChooseDialog()
        {
            if (SuppressDialogs || Find.WindowStack == null || Find.WindowStack.IsOpen<Dialog_EQChooseLeader>())
                return;
            if (Candidates().Count == 0)
                return;
            Find.WindowStack.Add(new Dialog_EQChooseLeader(forced: true));
        }
    }

    /// <summary>Escolha do líder (forçada quando o assentamento pertence a uma potência e está sem líder) ou do sucessor.</summary>
    public class Dialog_EQChooseLeader : Window
    {
        private readonly bool forced;
        private readonly bool successorMode;
        private Vector2 scroll;

        public override Vector2 InitialSize => new Vector2(520f, 560f);

        public Dialog_EQChooseLeader(bool forced, bool successorMode = false)
        {
            this.forced = forced;
            this.successorMode = successorMode;
            forcePause = true;
            absorbInputAroundWindow = true;
            doCloseX = !forced;
            closeOnCancel = !forced;
            closeOnAccept = false;
            closeOnClickedOutside = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var alg = WorldComponent_Allegiance.Current;
            Faction power = alg?.ActivePower;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), successorMode ? "Escolher o sucessor" : "Escolher o líder do assentamento");
            Text.Font = GameFont.Small;
            string intro = successorMode
                ? "Se o líder morrer ou for perdido, o sucessor assume a liderança da ideologia e herda o cargo e o favor" + (power != null ? " perante " + power.Name : "") + "."
                : "O assentamento está sem líder" + (power != null ? " e " + power.Name + " exige um responsável" : "") + ". O escolhido vira o líder da ideologia e herda o cargo e o favor do antecessor.";
            float h = Text.CalcHeight(intro, inRect.width);
            Widgets.Label(new Rect(inRect.x, inRect.y + 38f, inRect.width, h), intro);

            List<Pawn> list = EQLeader.Candidates();
            if (successorMode)
                list = list.Where(p => p != alg?.leader).ToList();
            Rect outRect = new Rect(inRect.x, inRect.y + 46f + h, inRect.width, inRect.height - 46f - h - (forced ? 0f : 40f));
            Rect view = new Rect(0f, 0f, outRect.width - 16f, list.Count * 44f);
            Widgets.BeginScrollView(outRect, ref scroll, view);
            float y = 0f;
            foreach (Pawn p in list)
            {
                Rect row = new Rect(0f, y, view.width, 40f);
                string label = p.Name?.ToStringFull ?? p.LabelShort;
                if (successorMode && alg?.successor == p) label += "  (sucessor atual)";
                if (Widgets.ButtonText(row, label))
                {
                    if (successorMode)
                    {
                        alg.successor = p;
                        Messages.Message(p.LabelShort + " é o sucessor designado.", p, MessageTypeDefOf.NeutralEvent);
                    }
                    else if (EQLeader.Assign(p))
                        alg?.LeaderTick();
                    Close();
                }
                y += 44f;
            }
            Widgets.EndScrollView();
            if (list.Count == 0)
                Widgets.Label(outRect, "Ninguém elegível: é preciso um adulto livre que siga a ideologia do assentamento e cumpra as exigências do cargo de líder dela.");
            if (!forced && Widgets.ButtonText(new Rect(inRect.xMax - 120f, inRect.yMax - 34f, 120f, 34f), "Fechar"))
                Close();
            if (forced && list.Count == 0)
                Close();
        }
    }

    // ===================================================================== Favor

    public static class EQFavor
    {
        private static int allowDepth;

        /// <summary>Dentro deste escopo o mod pode dar favor diretamente (fora dele, só o líder de um membro recebe).</summary>
        public static bool Allowed => allowDepth > 0;

        public static void Run(Action action)
        {
            allowDepth++;
            try { action(); }
            finally { allowDepth--; }
        }

        /// <summary>Dá favor ao líder perante a potência a que o assentamento pertence.</summary>
        public static bool Award(Faction power, int amount, string reason)
        {
            var alg = WorldComponent_Allegiance.Current;
            if (alg == null || power == null || amount == 0 || alg.memberOf != power || alg.leader?.royalty == null)
                return false;
            Run(() => alg.leader.royalty.GainFavor(power, amount));
            string label = FactionAllegianceDef.For(power.def)?.FavorLabel ?? "favor";
            Messages.Message(alg.leader.LabelShort + ": " + amount.ToStringWithSign() + " " + label + (reason.NullOrEmpty() ? "" : " (" + reason + ")"),
                alg.leader, amount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NegativeEvent);
            return true;
        }

        /// <summary>Um evento temático aconteceu; se a potência do jogador o valoriza, conta para o favor.</summary>
        public static void Notify(FavorEventType type, int count = 1, QualityCategory quality = QualityCategory.Legendary)
        {
            var alg = WorldComponent_Allegiance.Current;
            FactionAllegianceDef d = alg?.Def;
            if (d == null || count <= 0 || alg.leader == null)
                return;
            foreach (FavorSource src in d.favorSources)
            {
                if (src.type != type || src.amount == 0)
                    continue;
                if (type == FavorEventType.QualityCrafted && quality < src.minQuality)
                    continue;
                string key = d.defName + "/" + type + "/" + src.minQuality;
                alg.favorCounters.TryGetValue(key, out int have);
                have += count;
                int per = Math.Max(1, src.per);
                int awards = have / per;
                alg.favorCounters[key] = have % per;
                if (awards > 0)
                    Award(alg.memberOf, awards * src.amount, src.label);
            }
        }
    }

    // ===================================================================== Cerimônia

    public static class EQCeremony
    {
        public static bool HasPrice(FactionAllegianceDef d, Map map, out string missing)
        {
            var lacks = new List<string>();
            if (map == null)
                lacks.Add("colônia");
            else
            {
                if (d.joinPrisoner && !map.mapPawns.PrisonersOfColonySpawned.Any())
                    lacks.Add("1 prisioneiro");
                foreach (ThingDefCountClass c in d.joinThings)
                    if (c?.thingDef != null && EQItems.CountOnMap(map, c.thingDef) < c.count)
                        lacks.Add(c.count + "x " + c.thingDef.label);
            }
            missing = lacks.Count == 0 ? null : string.Join(", ", lacks);
            return missing == null;
        }

        public static void Pay(FactionAllegianceDef d, Map map, Faction power, Pawn leader)
        {
            foreach (ThingDefCountClass c in d.joinThings)
                if (c?.thingDef != null)
                    EQItems.TakeFromMap(map, c.thingDef, c.count);
            if (d.joinPrisoner)
            {
                Pawn prisoner = map.mapPawns.PrisonersOfColonySpawned.FirstOrDefault();
                if (prisoner != null) EQItems.HandOver(prisoner, power);
            }
            if (d.joinLeaderHediff != null && leader != null && !leader.Dead)
            {
                Hediff h = leader.health.GetOrAddHediff(d.joinLeaderHediff);
                if (h != null) h.Severity = Math.Max(h.Severity, d.joinLeaderHediffSeverity);
            }
        }

        /// <summary>Motivo pelo qual a cerimônia de submissão não pode ser solicitada agora, ou null.</summary>
        public static string BlockReason(Faction power)
        {
            var alg = WorldComponent_Allegiance.Current;
            FactionAllegianceDef d = power == null ? null : FactionAllegianceDef.For(power.def);
            if (alg == null || d == null) return "Indisponível";
            if (!ModsConfig.RoyaltyActive) return "Exige a DLC Royalty";
            if (alg.memberOf != null) return "O assentamento já pertence a " + alg.memberOf.Name;
            if (alg.pendingPower != null) return "Já há uma cerimônia solicitada com " + alg.pendingPower.Name;
            if (power.HostileTo(Faction.OfPlayer)) return "Potência hostil";
            Pawn leader = EQLeader.IdeoLeader;
            if (leader == null) return "O assentamento não tem líder (o líder da sua ideologia)";
            if (leader.royalty == null) return "O líder não pode receber cargos";
            if (leader.MapHeld == null || !leader.MapHeld.IsPlayerHome) return "O líder precisa estar na colônia";
            if (!HasPrice(d, leader.MapHeld, out string missing)) return "Falta o preço: " + missing;
            string heg = EQAllegiance.SubmitBlock(power);
            if (heg != null) return heg;
            return null;
        }

        /// <summary>Solicita a cerimônia de submissão: o líder recebe o favor do 1º cargo e a potência agenda o emissário.</summary>
        public static string Request(Faction power)
        {
            string block = BlockReason(power);
            if (block != null) return block;
            var alg = WorldComponent_Allegiance.Current;
            FactionAllegianceDef d = FactionAllegianceDef.For(power.def);
            alg.leader = EQLeader.IdeoLeader;
            alg.pendingPower = power;
            alg.lastCeremonyQuestTick = -999999;
            int need = d.FirstTitle.favorCost - alg.leader.royalty.GetFavor(power);
            if (need > 0)
                EQFavor.Run(() => alg.leader.royalty.GainFavor(power, need));
            else
                RoyalTitleUtility.GenerateBestowingCeremonyQuest(alg.leader, power);
            if (RoyalTitleUtility.GetCurrentBestowingCeremonyQuest(alg.leader, power) == null)
            {
                Cancel();
                return "A potência não conseguiu agendar a cerimônia agora.";
            }
            return null;
        }

        /// <summary>Desiste da cerimônia de submissão pendente.</summary>
        public static void Cancel()
        {
            var alg = WorldComponent_Allegiance.Current;
            Faction power = alg?.pendingPower;
            if (power == null) return;
            if (alg.leader?.royalty != null)
            {
                WorldComponent_Allegiance.EndCeremonyQuests(alg.leader, power);
                alg.leader.royalty.SetFavor(power, 0, notifyOnFavorChanged: false);
            }
            alg.pendingPower = null;
            alg.cachedFavor = 0;
        }
    }

    /// <summary>
    /// Cerimônia com emissário (submissão e promoções). Mesma estrutura da cerimônia de outorga da Royalty,
    /// com os peões da potência (FactionAllegianceDef.emissaryKind/escortKind) e textos temáticos.
    /// </summary>
    public class QuestNode_Root_EQCeremony : QuestNode
    {
        private static bool TryGetTarget(Slate slate, out Pawn pawn, out Faction power, out FactionAllegianceDef d)
        {
            d = null;
            slate.TryGet("bestowingFaction", out power);
            if (!slate.TryGet("titleHolder", out pawn) || pawn == null || power == null || pawn.Faction == null || !pawn.Faction.IsPlayer)
                return false;
            d = FactionAllegianceDef.For(power.def);
            return d != null && d.emissaryKind != null && RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(pawn, power);
        }

        protected override bool TestRunInt(Slate slate)
        {
            if (!TryGetTarget(slate, out Pawn pawn, out Faction power, out FactionAllegianceDef d) || power.HostileTo(Faction.OfPlayer))
                return false;
            return pawn.MapHeld != null && pawn.MapHeld.IsPlayerHome;
        }

        protected override void RunInt()
        {
            if (!ModLister.CheckRoyalty("Bestowing ceremony"))
                return;
            Quest quest = QuestGen.quest;
            Slate slate = QuestGen.slate;
            if (!TryGetTarget(slate, out Pawn pawn, out Faction power, out FactionAllegianceDef d))
                return;
            RoyalTitleDef newTitle = pawn.royalty.GetTitleAwardedWhenUpdating(power, pawn.royalty.GetFavor(power));
            bool submission = pawn.royalty.GetCurrentTitle(power) == null;

            string tag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("Bestowing");
            string sigExpired = QuestGenUtility.QuestTagSignal(tag, "CeremonyExpired");
            string sigFailed = QuestGenUtility.QuestTagSignal(tag, "CeremonyFailed");
            string sigDone = QuestGenUtility.QuestTagSignal(tag, "CeremonyDone");
            string sigAttacked = QuestGenUtility.QuestTagSignal(tag, "BeingAttacked");
            string sigFleeing = QuestGenUtility.QuestTagSignal(tag, "Fleeing");
            string sigTitleChanged = QuestGenUtility.QuestTagSignal(tag, "TitleAwardedWhenUpdatingChanged");

            Thing shuttle = QuestGen_Shuttle.GenerateShuttle(power);
            Pawn emissary = quest.GetPawn(new QuestGen_Pawns.GetPawnParms
            {
                tile = pawn.Tile,
                mustBeOfKind = d.emissaryKind,
                canGeneratePawn = true,
                mustBeOfFaction = power,
                mustBeWorldPawn = true,
                ifWorldPawnThenMustBeFree = true,
                redressPawn = true
            });
            QuestUtility.AddQuestTag(ref shuttle.questTags, tag);
            QuestUtility.AddQuestTag(ref pawn.questTags, tag);

            var contents = new List<Pawn> { emissary };
            slate.Set("shuttleContents", contents);
            slate.Set("shuttle", shuttle);
            slate.Set("target", pawn);
            slate.Set("bestower", emissary);
            slate.Set("bestowingFaction", power);
            var defenders = new List<Pawn>();
            PawnKindDef escortKind = d.escortKind ?? power.def.basicMemberKind;
            for (int i = 0; i < d.escortCount; i++)
            {
                Pawn guard = quest.GeneratePawn_NewTemp(escortKind, power, pawn.Tile);
                contents.Add(guard);
                defenders.Add(guard);
            }
            quest.EnsureNotDowned(contents);
            slate.Set("defenders", defenders);
            shuttle.TryGetComp<CompShuttle>().requiredPawns = contents;
            TransportShip ship = quest.GenerateTransportShip(TransportShipDefOf.Ship_Shuttle, contents, shuttle).transportShip;
            quest.AddShipJob_Arrive(ship, null, pawn, null, ShipJobStartMode.Instant, power);
            quest.AddShipJob(ship, ShipJobDefOf.Unload);
            quest.AddShipJob_WaitForever(ship, leaveImmediatelyWhenSatisfied: true, showGizmos: false, contents.Cast<Thing>().ToList()).sendAwayIfAnyDespawnedDownedOrDead = new List<Thing> { emissary };
            QuestUtility.AddQuestTag(ref ship.questTags, tag);
            quest.FactionGoodwillChange(power, -5, QuestGenUtility.HardcodedSignalWithQuestID("defenders.Killed"), canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.QuestPawnLost);

            var ceremony = new QuestPart_BestowingCeremony();
            ceremony.inSignal = slate.Get<string>("inSignal");
            ceremony.pawns.Add(emissary);
            ceremony.mapOfPawn = pawn;
            ceremony.faction = emissary.Faction;
            ceremony.bestower = emissary;
            ceremony.target = pawn;
            ceremony.shuttle = shuttle;
            ceremony.questTag = tag;
            quest.AddPart(ceremony);

            var escort = new QuestPart_EscortPawn();
            escort.inSignal = slate.Get<string>("inSignal");
            escort.escortee = emissary;
            escort.pawns.AddRange(defenders);
            escort.mapOfPawn = pawn;
            escort.faction = emissary.Faction;
            escort.shuttle = shuttle;
            escort.questTag = tag;
            escort.leavingDangerMessage = "MessageBestowingDanger".Translate();
            quest.AddPart(escort);

            string sigShuttleKilled = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.Killed");
            quest.FactionGoodwillChange(power, 0, sigShuttleKilled, canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.ShuttleDestroyed, QuestPart.SignalListenMode.OngoingOnly, ensureMakesHostile: true);
            quest.End(QuestEndOutcome.Fail, 0, null, sigShuttleKilled, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

            quest.AddPart(new QuestPart_RequirementsToAcceptPawnOnColonyMap { pawn = pawn });
            quest.AddPart(new QuestPart_RequirementsToAcceptNoDanger { mapPawn = pawn, dangerTo = power });
            quest.AddPart(new QuestPart_RequirementsToAcceptNoOngoingBestowingCeremony());

            string sigRecruited = QuestGenUtility.HardcodedSignalWithQuestID("shuttleContents.Recruited");
            string sigHostile = QuestGenUtility.HardcodedSignalWithQuestID("bestowingFaction.BecameHostileToPlayer");
            quest.Signal(sigRecruited, () =>
            {
                quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
            });
            quest.Bestowing_TargetChangedTitle(pawn, emissary, newTitle, sigTitleChanged);
            quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("target.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("bestower.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("bestower.LeftBehind"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("shuttle.LeftBehind"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Fail, 0, null, sigExpired);
            quest.End(QuestEndOutcome.Fail, 0, null, sigHostile, QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Fail, 0, null, sigFailed, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Fail, 0, null, sigAttacked, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Fail, 0, null, sigFleeing, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Success, 0, null, sigDone);

            QuestPart_Choice choice = quest.RewardChoice();
            choice.choices.Add(new QuestPart_Choice.Choice
            {
                rewards =
                {
                    new Reward_BestowingCeremony
                    {
                        targetPawnName = pawn.NameShortColored.Resolve(),
                        titleName = newTitle.GetLabelCapFor(pawn),
                        awardingFaction = power,
                        givePsylink = false,
                        royalTitle = newTitle
                    }
                }
            });

            string name = submission ? d.ceremonyLabel : d.promotionLabel;
            string text = (submission ? d.ceremonyText : d.promotionText) ?? "";
            string price = submission ? "Preço da submissão, recolhido pelo emissário ao fim da cerimônia: " + d.PriceText() + "." : "A promoção não tem custo além do favor já acumulado.";
            var nameRules = new List<Rule>();
            nameRules.AddRange(GrammarUtility.RulesForPawn("pawn", pawn));
            nameRules.Add(new Rule_String("newTitle", newTitle.GetLabelCapFor(pawn)));
            nameRules.Add(new Rule_String("ceremonyName", name));
            QuestGen.AddQuestNameRules(nameRules);
            var descRules = new List<Rule>();
            descRules.AddRange(GrammarUtility.RulesForFaction("faction", power));
            descRules.AddRange(GrammarUtility.RulesForPawn("pawn", pawn));
            descRules.Add(new Rule_String("newTitle", newTitle.GetLabelFor(pawn)));
            descRules.Add(new Rule_String("ceremonyText", text));
            descRules.Add(new Rule_String("priceText", price));
            QuestGen.AddQuestDescriptionRules(descRules);

            var alg = WorldComponent_Allegiance.Current;
            if (alg != null) alg.lastCeremonyQuestTick = Find.TickManager.TicksGame;
        }
    }

    // ===================================================================== Patches

    /// <summary>As potências usam a nossa cerimônia em vez da outorga do Império.</summary>
    [HarmonyPatch(typeof(RoyalTitleUtility), nameof(RoyalTitleUtility.GenerateBestowingCeremonyQuest))]
    public static class Patch_RoyalTitleUtility_GenerateBestowingCeremonyQuest
    {
        public static bool Prefix(Pawn pawn, Faction faction)
        {
            if (!EQAllegiance.IsPower(faction))
                return true;
            if (pawn == null || pawn.Dead)
                return false;
            QuestScriptDef script = DefDatabase<QuestScriptDef>.GetNamedSilentFail("EQ_Ceremony");
            if (script == null)
                return false;
            var slate = new Slate();
            slate.Set("titleHolder", pawn);
            slate.Set("bestowingFaction", faction);
            if (pawn.MapHeld != null && script.root.TestRun(slate.DeepCopy()))
            {
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(script, slate);
                QuestUtility.SendLetterQuestAvailable(quest);
            }
            return false;
        }
    }

    /// <summary>Favor com uma potência pertence só ao líder de um assentamento membro.</summary>
    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.GainFavor))]
    public static class Patch_Pawn_RoyaltyTracker_GainFavor
    {
        public static bool Prefix(Pawn_RoyaltyTracker __instance, Faction faction, int amount)
        {
            if (EQFavor.Allowed || !EQAllegiance.IsPower(faction))
                return true;
            Pawn pawn = __instance.pawn;
            if (pawn?.Faction == null || !pawn.Faction.IsPlayer)
                return true;
            var alg = WorldComponent_Allegiance.Current;
            if (alg == null || alg.memberOf != faction || alg.leader?.royalty == null)
                return false;
            if (pawn == alg.leader)
                return true;
            EQFavor.Run(() => alg.leader.royalty.GainFavor(faction, amount));
            return false;
        }
    }

    /// <summary>Cobra o preço da submissão ao fim da cerimônia e dispensa o amplificador psíquico do Império.</summary>
    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_Bestowing), nameof(RitualOutcomeEffectWorker_Bestowing.Apply))]
    public static class Patch_RitualOutcomeEffectWorker_Bestowing_Apply
    {
        public static bool Prefix(LordJob_Ritual jobRitual)
        {
            if (!(jobRitual is LordJob_BestowingCeremony job) || job.bestower == null || job.target == null)
                return true;
            Faction power = job.bestower.Faction;
            FactionAllegianceDef d = power == null ? null : FactionAllegianceDef.For(power.def);
            if (d == null)
                return true;
            Pawn target = job.target;
            if (target.royalty != null && target.royalty.GetCurrentTitle(power) == null)
            {
                Map map = target.MapHeld;
                if (!EQCeremony.HasPrice(d, map, out string missing))
                {
                    Find.LetterStack.ReceiveLetter(d.ceremonyLabel + ": recusada",
                        "O emissário de " + power.Name + " conferiu a lista e fechou a prancheta. Faltou o preço combinado: " + missing + ".\n\nA cerimônia foi encerrada sem juramento. Você pode solicitar outra.",
                        LetterDefOf.NegativeEvent, target, power);
                    EQCeremony.Cancel();
                    return false;
                }
                EQCeremony.Pay(d, map, power, target);
            }
            ThingOwner<Thing> inv = job.bestower.inventory?.innerContainer;
            if (inv != null && !inv.Any(t => t.def == ThingDefOf.PsychicAmplifier))
                inv.TryAdd(ThingMaker.MakeThing(ThingDefOf.PsychicAmplifier), 1);
            return true;
        }
    }

    // ---------------------------------------------------------------- fontes temáticas de favor

    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
    public static class Patch_ResearchManager_FinishProject
    {
        public static void Postfix() => EQFavor.Notify(FavorEventType.ResearchFinished);
    }

    [HarmonyPatch(typeof(CompQuality), nameof(CompQuality.SetQuality))]
    public static class Patch_CompQuality_SetQuality
    {
        public static void Postfix(QualityCategory q, ArtGenerationContext? source)
        {
            if (source == ArtGenerationContext.Colony && Current.ProgramState == ProgramState.Playing)
                EQFavor.Notify(FavorEventType.QualityCrafted, 1, q);
        }
    }

    [HarmonyPatch(typeof(LordJob_Ritual), nameof(LordJob_Ritual.ApplyOutcome))]
    public static class Patch_LordJob_Ritual_ApplyOutcome
    {
        public static void Postfix(LordJob_Ritual __instance, float progress, bool cancelled)
        {
            if (cancelled || progress < 1f || __instance is LordJob_BestowingCeremony)
                return;
            if (__instance.lord?.faction == null || !__instance.lord.faction.IsPlayer)
                return;
            EQFavor.Notify(FavorEventType.RitualFinished);
        }
    }

    [HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.ImplantXenogermItem))]
    public static class Patch_GeneUtility_ImplantXenogermItem
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn?.Faction != null && pawn.Faction.IsPlayer)
                EQFavor.Notify(FavorEventType.XenogermImplanted);
        }
    }

    [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
    public static class Patch_TradeDeal_TryExecute
    {
        public static void Prefix(TradeDeal __instance, out int __state)
        {
            __state = Math.Abs(__instance.CurrencyTradeable?.CountToTransfer ?? 0);
        }

        public static void Postfix(ref bool actuallyTraded, int __state)
        {
            if (!actuallyTraded || __state <= 0 || !TradeSession.Active)
                return;
            var alg = WorldComponent_Allegiance.Current;
            if (alg?.memberOf != null && EQUtil.Canonical(TradeSession.trader?.Faction) == alg.memberOf)
                EQFavor.Notify(FavorEventType.TradeSilver, __state);
        }
    }
}
