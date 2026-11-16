using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    // ===================================================================== Defs

    /// <summary>Benefícios de um cargo (o cargo em si é um RoyalTitleDef: nome, descrição e custo em favor).</summary>
    public class AllegianceRank
    {
        public RoyalTitleDef title;
        /// <summary>Somado à melhora de preço com a própria potência.</summary>
        public float priceGain;
        /// <summary>Entregue a cada stipendIntervalDays.</summary>
        public List<ThingDefCountClass> stipend = new List<ThingDefCountClass>();
        /// <summary>defName de um caminho do Vanilla Psycasts Expanded que este cargo licencia (ver Psycasts.cs).</summary>
        public string psycastPath;
        /// <summary>Texto de trava do caminho (opcional; padrão: "licença da potência (cargo)").</summary>
        public string psycastLockedReason;
    }

    public enum TitheOptionType { Silver, Things, Prisoner, Colonist, Refuse }

    /// <summary>Uma opção da carta de dízimo.</summary>
    public class TitheOption
    {
        public string label;
        public TitheOptionType type;
        public ThingDef thingDef;
        /// <summary>Quantidade base; multiplicada conforme o cargo (ver WorldComponent_Allegiance.TitheMultiplier).</summary>
        public int count;
        /// <summary>Colonist: prefere crianças.</summary>
        public bool preferChildren;
        public float fame;
        public float infamy;
        public int goodwill;
        /// <summary>Favor dado ao líder ao escolher esta opção.</summary>
        public int favor;
        /// <summary>Mensagem após escolher.</summary>
        public string resultText;
    }

    public enum FavorEventType { ResearchFinished, SerumUsed, XenogermImplanted, TradeSilver, QualityCrafted, RitualFinished }

    /// <summary>Uma forma temática de ganhar favor: a cada "per" ocorrências do evento, o líder ganha "amount".</summary>
    public class FavorSource
    {
        public FavorEventType type;
        public int amount = 1;
        public int per = 1;
        /// <summary>QualityCrafted: qualidade mínima.</summary>
        public QualityCategory minQuality = QualityCategory.Masterwork;
        public string label;
    }

    /// <summary>Configuração de lealdade de uma potência (1.7/Defs/EQ_Allegiance.xml).</summary>
    public class FactionAllegianceDef : Def
    {
        public FactionDef faction;
        /// <summary>Um por cargo, na ordem dos títulos (do mais baixo ao mais alto).</summary>
        public List<AllegianceRank> ranks = new List<AllegianceRank>();
        public float stipendIntervalDays = 10f;

        // Cerimônias (submissão = 1º cargo; promoção = demais)
        public string ceremonyLabel = "Cerimônia de submissão";
        public string ceremonyText;
        public string promotionLabel = "Cerimônia de promoção";
        public string promotionText;
        public PawnKindDef emissaryKind;
        public PawnKindDef escortKind;
        public int escortCount = 4;
        /// <summary>Preço da submissão, cobrado ao fim da cerimônia.</summary>
        public List<ThingDefCountClass> joinThings = new List<ThingDefCountClass>();
        public bool joinPrisoner;
        public HediffDef joinLeaderHediff;
        public float joinLeaderHediffSeverity = 0.3f;
        /// <summary>Texto extra do preço (ex.: o que acontece com o líder).</summary>
        public string joinPriceNote;

        /// <summary>Só a potência psíquica (Coro) dá nível psíquico pelos cargos (maxPsylinkLevel dos títulos).</summary>
        public bool grantsPsylink;
        /// <summary>Implante que o líder recebe na cerimônia de submissão (Forja: vínculo mecanoide). No cérebro.</summary>
        public HediffDef ceremonyHediff;
        public string ceremonyHediffText;

        public List<FavorSource> favorSources = new List<FavorSource>();

        public string titheLabel;
        public string titheText;
        public float titheIntervalDays = 15f;
        public float titheTimeoutDays = 2f;
        public List<TitheOption> titheOptions = new List<TitheOption>();
        /// <summary>Penalidade ao ignorar a carta até expirar.</summary>
        public float ignoreInfamy = 15f;
        public int ignoreGoodwill = -15;

        public float leaveInfamy = 50f;

        /// <summary>Quando hostil e a reputação pede retaliação: chance de usar um destes incidentes em vez de caçadores.</summary>
        public List<IncidentDef> hostileIncidents = new List<IncidentDef>();
        public float hostileIncidentChance = 0.5f;
        public string hostileIncidentLabel;
        public string hostileIncidentText;

        public static FactionAllegianceDef For(FactionDef def) =>
            def == null ? null : DefDatabase<FactionAllegianceDef>.AllDefsListForReading.FirstOrDefault(a => a.faction == def);

        /// <summary>A potência dona de um cargo (pela tag do título em royalTitleTags da facção).</summary>
        public static FactionAllegianceDef ForTitle(RoyalTitleDef title)
        {
            if (title == null)
                return null;
            foreach (FactionAllegianceDef a in DefDatabase<FactionAllegianceDef>.AllDefsListForReading)
                if (a.ranks.Any(r => r.title == title))
                    return a;
            return null;
        }

        public string FavorLabel => faction?.royalFavorLabel ?? "favor";

        public RoyalTitleDef FirstTitle => ranks.Count > 0 ? ranks[0].title : null;

        public int RankIndexOf(RoyalTitleDef title)
        {
            if (title == null) return -1;
            return ranks.FindIndex(r => r.title == title);
        }

        public string RankLabel(int index, Pawn forPawn = null)
        {
            if (index < 0 || index >= ranks.Count || ranks[index].title == null) return "?";
            RoyalTitleDef t = ranks[index].title;
            return forPawn != null ? t.GetLabelCapFor(forPawn) : t.GetLabelCapForBothGenders();
        }

        public string PriceText()
        {
            var parts = new List<string>();
            if (joinPrisoner) parts.Add("1 prisioneiro");
            foreach (ThingDefCountClass c in joinThings)
                if (c?.thingDef != null) parts.Add(c.count + "x " + c.thingDef.label);
            string text = parts.Count == 0 ? "nada" : string.Join(", ", parts);
            if (!joinPriceNote.NullOrEmpty()) text += ". " + joinPriceNote;
            return text;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string e in base.ConfigErrors()) yield return e;
            if (faction == null) yield return "FactionAllegianceDef sem faction.";
            if (ranks.NullOrEmpty()) yield return "FactionAllegianceDef sem ranks.";
            else if (ranks.Any(r => r.title == null)) yield return "FactionAllegianceDef com rank sem title.";
            if (emissaryKind == null) yield return "FactionAllegianceDef sem emissaryKind.";
        }
    }

    // ===================================================================== Estado

    /// <summary>
    /// Ponte para o sistema de vassalagem de outro mod. O módulo do Hegemony preenche os delegados.
    /// </summary>
    public static class EQAllegiance
    {
        /// <summary>A quem o jogador jurou fealdade (null = ninguém). Null no delegado = sem Hegemony.</summary>
        public static Func<Faction> PlayerOverlordProvider;
        /// <summary>Faz o juramento do jogador à potência; devolve o motivo da falha ou null.</summary>
        public static Func<Faction, string> SubmitHandler;
        /// <summary>Motivo pelo qual o juramento seria recusado, ou null.</summary>
        public static Func<Faction, string> SubmitBlockHandler;
        /// <summary>Ligado enquanto o próprio mod faz o juramento (o juramento direto pelo Hegemony é bloqueado para as potências).</summary>
        public static bool AllowSubmit;

        public static bool HasVassalage => PlayerOverlordProvider != null;

        public static Faction PlayerOverlord
        {
            get
            {
                try { return EQUtil.Canonical(PlayerOverlordProvider?.Invoke()); }
                catch { return null; }
            }
        }

        public static string Submit(Faction f)
        {
            if (SubmitHandler == null) return null;
            try { return SubmitHandler(f); }
            catch (Exception ex) { return ex.Message; }
        }

        public static string SubmitBlock(Faction f)
        {
            if (SubmitBlockHandler == null) return null;
            try { return SubmitBlockHandler(f); }
            catch (Exception ex) { return ex.Message; }
        }

        public static bool IsPower(Faction f) => f != null && FactionAllegianceDef.For(f.def) != null;

        public static IEnumerable<Faction> Powers
        {
            get
            {
                foreach (FactionAllegianceDef d in DefDatabase<FactionAllegianceDef>.AllDefsListForReading)
                {
                    Faction f = EQUtil.MainFaction(d.faction);
                    if (f != null) yield return f;
                }
            }
        }
    }

    /// <summary>
    /// Membro de qual potência o assentamento é, quem é o líder (dono do cargo e do favor),
    /// sucessor, cerimônia pendente, estipêndio e dízimos.
    /// </summary>
    public class WorldComponent_Allegiance : WorldComponent
    {
        public Faction memberOf;
        /// <summary>Potência com cerimônia de submissão solicitada e ainda não concluída.</summary>
        public Faction pendingPower;
        /// <summary>O líder do assentamento = líder da ideologia do jogador. Só ele tem cargo e favor.</summary>
        public Pawn leader;
        public Pawn successor;
        /// <summary>Cópia do cargo/favor do líder, para sobreviver à perda do peão.</summary>
        public RoyalTitleDef cachedTitle;
        public int cachedFavor;
        /// <summary>O Hegemony confirmou a vassalagem (se sumir depois, o jogador saiu).</summary>
        public bool hegLinked;
        public bool needsLeaderChoice;
        public int lastCeremonyQuestTick = -999999;
        public Dictionary<string, int> favorCounters = new Dictionary<string, int>();

        public int joinedTick = -1;
        public int lastRank = -1;
        public int lastStipendTick = -1;
        public int lastTitheTick = -1;
        public int pendingTitheUntil = -1;   // >= 0 = existe dízimo aguardando resposta

        private const int LeaderInterval = 250;
        private const int CheckInterval = 2500; // 1 hora de jogo
        private const int CeremonyRetryTicks = 3 * GenDate.TicksPerDay;

        public WorldComponent_Allegiance(World world) : base(world)
        {
        }

        public static WorldComponent_Allegiance Current => Find.World?.GetComponent<WorldComponent_Allegiance>();

        public FactionAllegianceDef Def => memberOf == null ? null : FactionAllegianceDef.For(memberOf.def);

        /// <summary>Potência com a qual o líder tem (ou está para ter) cargo.</summary>
        public Faction ActivePower => memberOf ?? pendingPower;

        public RoyalTitleDef CurrentTitle
        {
            get
            {
                if (memberOf == null) return null;
                return (leader?.royalty != null ? leader.royalty.GetCurrentTitle(memberOf) : null) ?? cachedTitle;
            }
        }

        public int Favor
        {
            get
            {
                Faction p = ActivePower;
                if (p == null) return 0;
                return leader?.royalty != null ? leader.royalty.GetFavor(p) : cachedFavor;
            }
        }

        public int RankIndex
        {
            get
            {
                var d = Def;
                if (d == null) return -1;
                int i = d.RankIndexOf(CurrentTitle);
                return i < 0 ? 0 : i;
            }
        }

        public AllegianceRank Rank => RankIndex >= 0 ? Def.ranks[RankIndex] : null;

        public string RankLabel => Def?.RankLabel(RankIndex, leader) ?? "?";

        /// <summary>Multiplicador das quantidades do dízimo: 1, 1, 2, 2, 3, 3, 4…</summary>
        public int TitheMultiplier => 1 + Math.Max(0, RankIndex) / 2;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref memberOf, "memberOf");
            Scribe_References.Look(ref pendingPower, "pendingPower");
            Scribe_References.Look(ref leader, "leader");
            Scribe_References.Look(ref successor, "successor");
            Scribe_Defs.Look(ref cachedTitle, "cachedTitle");
            Scribe_Values.Look(ref cachedFavor, "cachedFavor");
            Scribe_Values.Look(ref hegLinked, "hegLinked");
            Scribe_Values.Look(ref lastCeremonyQuestTick, "lastCeremonyQuestTick", -999999);
            Scribe_Collections.Look(ref favorCounters, "favorCounters", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref joinedTick, "joinedTick", -1);
            Scribe_Values.Look(ref lastRank, "lastRank", -1);
            Scribe_Values.Look(ref lastStipendTick, "lastStipendTick", -1);
            Scribe_Values.Look(ref lastTitheTick, "lastTitheTick", -1);
            Scribe_Values.Look(ref pendingTitheUntil, "pendingTitheUntil", -1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && favorCounters == null)
                favorCounters = new Dictionary<string, int>();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            int now = Find.TickManager.TicksGame;
            if (now % LeaderInterval == 0)
                LeaderTick();
            if (now % CheckInterval != 0)
                return;

            UpdateMembership(now);
            foreach (Faction p in EQAllegiance.Powers)
                p.allowRoyalFavorRewards = p == memberOf;
            RetryCeremonyQuest(now);
            if (memberOf == null)
                return;

            var d = Def;
            Map map = Find.AnyPlayerHomeMap;
            if (d == null || map == null)
                return;

            int rank = RankIndex;
            lastRank = rank;

            // Estipêndio
            if (now - lastStipendTick >= d.stipendIntervalDays * GenDate.TicksPerDay)
            {
                lastStipendTick = now;
                SendStipend(d, map, rank);
            }

            // Dízimo pendente expirou = recusa
            if (pendingTitheUntil >= 0 && now >= pendingTitheUntil)
            {
                pendingTitheUntil = -1;
                ApplyOutcome(memberOf, 0f, d.ignoreInfamy, d.ignoreGoodwill);
                Messages.Message(memberOf.Name + " anotou sua \"ausência de resposta\" ao dízimo. Isso consta no seu registro.", MessageTypeDefOf.NegativeEvent);
            }

            // Novo dízimo
            if (pendingTitheUntil < 0 && !d.titheOptions.NullOrEmpty() && now - lastTitheTick >= d.titheIntervalDays * GenDate.TicksPerDay)
            {
                lastTitheTick = now;
                SendTithe(d, map);
            }
        }

        // ------------------------------------------------------------ líder

        /// <summary>Acompanha o líder da ideologia: transferência, sucessão e escolha forçada.</summary>
        public void LeaderTick()
        {
            Faction power = ActivePower;
            Pawn cur = EQLeader.IdeoLeader;
            if (successor != null && (successor == cur || !EQLeader.IsValidCandidate(successor, strict: true)))
                successor = null;

            if (cur != null)
            {
                needsLeaderChoice = false;
                if (cur != leader)
                {
                    Pawn old = leader;
                    leader = cur;
                    if (power != null)
                        TransferTo(old, cur, power);
                }
                CacheLeaderState();
                return;
            }

            if (power == null)
            {
                leader = null;
                return;
            }

            // Sem líder e ligado a uma potência: o sucessor assume, ou o jogador é obrigado a escolher.
            if (successor != null && EQLeader.Assign(successor))
            {
                Pawn heir = successor;
                successor = null;
                Find.LetterStack.ReceiveLetter("Sucessão: " + heir.LabelShort,
                    heir.LabelShort + " era o sucessor designado e assume a liderança do assentamento perante " + power.Name + ", herdando o cargo e o favor do antecessor.",
                    LetterDefOf.NeutralEvent, heir, power);
                LeaderTick();
                return;
            }
            needsLeaderChoice = true;
            EQLeader.TryOpenChooseDialog();
        }

        private void CacheLeaderState()
        {
            Faction power = ActivePower;
            if (power == null || leader?.royalty == null)
            {
                if (power == null) { cachedTitle = null; cachedFavor = 0; }
                return;
            }
            cachedTitle = leader.royalty.GetCurrentTitle(power);
            cachedFavor = leader.royalty.GetFavor(power);
        }

        /// <summary>Passa cargo e favor do líder antigo (talvez morto ou perdido) para o novo.</summary>
        private void TransferTo(Pawn old, Pawn cur, Faction power)
        {
            RoyalTitleDef title = cachedTitle;
            int favor = cachedFavor;
            if (old?.royalty != null)
            {
                title = old.royalty.GetCurrentTitle(power) ?? title;
                favor = Math.Max(favor, old.royalty.GetFavor(power));
                try
                {
                    EndCeremonyQuests(old, power);
                    if (old.royalty.GetCurrentTitle(power) != null)
                        old.royalty.SetTitle(power, null, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
                    old.royalty.SetFavor(power, 0, notifyOnFavorChanged: false);
                }
                catch (Exception ex)
                {
                    Log.Warning("[Ecos da Queda] Falha ao retirar o cargo do líder antigo: " + ex.Message);
                }
            }
            if (cur.royalty == null || (title == null && favor <= 0))
                return;
            if (title != null && cur.royalty.GetCurrentTitle(power) != title)
                cur.royalty.SetTitle(power, title, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
            cur.royalty.SetFavor(power, favor, notifyOnFavorChanged: false);
            cachedTitle = title;
            cachedFavor = favor;
            lastCeremonyQuestTick = -999999;
            if (old != null)
                Find.LetterStack.ReceiveLetter("Transferência de liderança: " + cur.LabelShort,
                    cur.LabelShort + " agora lidera o assentamento perante " + power.Name
                    + (title != null ? " e assume o cargo de " + title.GetLabelCapFor(cur) : "") + ", com " + favor + " " + (FactionAllegianceDef.For(power.def)?.FavorLabel ?? "favor") + "."
                    + (old.Dead ? "" : "\n\n" + old.LabelShort + " volta a ser um membro comum. A papelada já foi atualizada."),
                    LetterDefOf.NeutralEvent, cur, power);
        }

        /// <summary>Encerra a cerimônia marcada para o peão com a potência, aceita ou não.</summary>
        public static void EndCeremonyQuests(Pawn pawn, Faction power)
        {
            RoyalTitleUtility.EndExistingBestowingCeremonyQuest(pawn, power);
            Quest q = RoyalTitleUtility.GetCurrentBestowingCeremonyQuest(pawn, power);
            if (q != null && q.State == QuestState.Ongoing)
                q.End(QuestEndOutcome.Fail, sendLetter: false);
        }

        /// <summary>Se o líder já tem favor para o próximo cargo e não há cerimônia marcada, pede outra (a cada 3 dias).</summary>
        private void RetryCeremonyQuest(int now)
        {
            Faction power = ActivePower;
            if (power == null || leader?.royalty == null || power.HostileTo(Faction.OfPlayer))
                return;
            if (now - lastCeremonyQuestTick < CeremonyRetryTicks)
                return;
            if (!RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(leader, power))
                return;
            RoyalTitleUtility.GenerateBestowingCeremonyQuest(leader, power);
        }

        // ------------------------------------------------------------ entrada e saída

        private void UpdateMembership(int now)
        {
            Faction overlord = EQAllegiance.PlayerOverlord;
            if (overlord != null && !EQAllegiance.IsPower(overlord))
                overlord = null; // jurou lealdade a uma facção que não é uma das potências

            if (memberOf != null)
            {
                // Membro sem cargo (save anterior ao sistema de cargos): o líder recebe o 1º cargo.
                if (leader?.royalty != null && cachedTitle == null && leader.royalty.GetCurrentTitle(memberOf) == null)
                {
                    RoyalTitleDef first = Def?.FirstTitle;
                    if (first != null)
                    {
                        leader.royalty.SetTitle(memberOf, first, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
                        CacheLeaderState();
                        Find.LetterStack.ReceiveLetter("Cargo: " + RankLabel,
                            memberOf.Name + " atualizou os registros: " + leader.LabelShort + ", líder do assentamento, passa a responder por ele com o cargo de " + RankLabel + ".",
                            LetterDefOf.NeutralEvent, leader, memberOf);
                    }
                }
                if (EQAllegiance.HasVassalage)
                {
                    if (overlord == memberOf)
                        hegLinked = true;
                    else if (hegLinked)
                        Leave(now);
                }
                return;
            }

            // Cerimônia de submissão concluída: o líder tem um cargo numa potência.
            if (leader?.royalty != null)
            {
                foreach (Faction p in EQAllegiance.Powers)
                {
                    if (leader.royalty.GetCurrentTitle(p) != null)
                    {
                        Join(p, now);
                        return;
                    }
                }
            }

            // Vassalo pelo Hegemony sem cerimônia (saves antigos): recebe o 1º cargo.
            if (overlord != null && pendingPower == null)
            {
                if (leader?.royalty == null)
                {
                    pendingPower = overlord;
                    return;
                }
                RoyalTitleDef first = FactionAllegianceDef.For(overlord.def)?.FirstTitle;
                if (first != null)
                {
                    leader.royalty.SetTitle(overlord, first, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
                    Join(overlord, now);
                }
            }
            else if (overlord != null && pendingPower == overlord && leader?.royalty != null && leader.royalty.GetFavor(overlord) == 0)
            {
                pendingPower = null; // líder escolhido; na próxima hora recebe o cargo
            }
        }

        private void Join(Faction power, int now)
        {
            memberOf = power;
            pendingPower = null;
            pendingTitheUntil = -1;
            joinedTick = now;
            lastStipendTick = now;
            lastTitheTick = now;
            CacheLeaderState();
            lastRank = RankIndex;

            string extra = "";
            if (EQAllegiance.HasVassalage)
            {
                if (EQAllegiance.PlayerOverlord == power)
                    hegLinked = true;
                else
                {
                    string err = EQAllegiance.Submit(power);
                    hegLinked = err == null;
                    if (err != null)
                        extra = "\n\nO juramento de vassalagem (Hegemony) não pôde ser registrado: " + err;
                }
            }

            FactionAllegianceDef d = Def;
            Find.LetterStack.ReceiveLetter("Juramento: " + power.Name,
                "O assentamento agora pertence a " + power.Name + ". " + (leader?.LabelShort ?? "O líder") + " responde por ele com o cargo de " + RankLabel + "."
                + "\n\n" + (d.ranks[lastRank].title?.description ?? "") + "\n\n" + DescribeRank(d, lastRank) + extra,
                LetterDefOf.PositiveEvent, leader, power);
        }

        private void Leave(int now)
        {
            Faction old = memberOf;
            var oldDef = FactionAllegianceDef.For(old.def);
            if (leader?.royalty != null)
            {
                try
                {
                    EndCeremonyQuests(leader, old);
                    if (leader.royalty.GetCurrentTitle(old) != null)
                        leader.royalty.SetTitle(old, null, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
                    leader.royalty.SetFavor(old, 0, notifyOnFavorChanged: false);
                }
                catch (Exception ex)
                {
                    Log.Warning("[Ecos da Queda] Falha ao retirar o cargo: " + ex.Message);
                }
            }
            memberOf = null;
            pendingPower = null;
            pendingTitheUntil = -1;
            hegLinked = false;
            cachedTitle = null;
            cachedFavor = 0;
            lastRank = -1;
            old.allowRoyalFavorRewards = false;
            WorldComponent_Reputation.Current?.Record(old, 0f, oldDef?.leaveInfamy ?? 0f);
            Find.LetterStack.ReceiveLetter("Desligamento: " + old.Name,
                "O assentamento não pertence mais a " + old.Name + ". O cargo foi cassado, o crachá recolhido, o nome riscado dos registros e a foto do líder virou alvo de dardos na sala de descanso.",
                LetterDefOf.NegativeEvent, null, old);
        }

        /// <summary>Rompe o juramento por conta própria (sem Hegemony). Com Hegemony, a saída é a independência por lá.</summary>
        public string Renounce()
        {
            if (memberOf == null) return "Você não pertence a nenhuma potência.";
            if (EQAllegiance.HasVassalage && hegLinked)
                return "Você é vassalo pelo Hegemony: declare independência pela diplomacia do Hegemony.";
            Leave(Find.TickManager.TicksGame);
            return null;
        }

        public static string DescribeRank(FactionAllegianceDef d, int rank)
        {
            AllegianceRank r = d.ranks[rank];
            var lines = new List<string>();
            if (r.priceGain != 0f)
                lines.Add("Preços com a potência: " + (r.priceGain > 0 ? "+" : "") + (int)Math.Round(r.priceGain * 100f) + "%");
            if (!r.stipend.NullOrEmpty())
                lines.Add("Estipêndio a cada " + d.stipendIntervalDays + " dias: " + string.Join(", ", r.stipend.Select(s => s.count + "x " + s.thingDef?.label)));
            if (!d.titheOptions.NullOrEmpty())
                lines.Add("Obrigação: " + d.titheLabel + " a cada " + d.titheIntervalDays + " dias");
            if (rank + 1 < d.ranks.Count && d.ranks[rank + 1].title != null)
                lines.Add("Próximo cargo: " + d.RankLabel(rank + 1) + " (" + d.ranks[rank + 1].title.favorCost + " " + d.FavorLabel + ")");
            return string.Join("\n", lines);
        }

        private void SendStipend(FactionAllegianceDef d, Map map, int rank)
        {
            AllegianceRank r = d.ranks[rank];
            var things = new List<Thing>();
            foreach (ThingDefCountClass s in r.stipend)
            {
                if (s?.thingDef == null || s.count <= 0) continue;
                int left = s.count;
                while (left > 0)
                {
                    Thing t = ThingMaker.MakeThing(s.thingDef, GenStuff.DefaultStuffFor(s.thingDef));
                    t.stackCount = Math.Min(left, s.thingDef.stackLimit);
                    left -= t.stackCount;
                    things.Add(t);
                }
            }
            if (things.Count == 0) return;
            IntVec3 cell = DropCellFinder.TradeDropSpot(map);
            DropPodUtility.DropThingsNear(cell, map, things, faction: memberOf);
            Find.LetterStack.ReceiveLetter("Estipêndio: " + memberOf.Name,
                memberOf.Name + " enviou o estipêndio de " + RankLabel + ". Assine o recibo, por favor.",
                LetterDefOf.PositiveEvent, new LookTargets(cell, map), memberOf);
        }

        private void SendTithe(FactionAllegianceDef d, Map map)
        {
            var letter = (ChoiceLetter_EQTithe)LetterMaker.MakeLetter(d.titheLabel, d.titheText, DefDatabase<LetterDef>.GetNamed("EQ_TitheLetter"), memberOf);
            letter.allegianceDef = d;
            letter.map = map;
            letter.multiplier = TitheMultiplier;
            letter.StartTimeout((int)(d.titheTimeoutDays * GenDate.TicksPerDay));
            pendingTitheUntil = letter.disappearAtTick;
            Find.LetterStack.ReceiveLetter(letter);
        }

        public void ResolveTithe() => pendingTitheUntil = -1;

        public static void ApplyOutcome(Faction f, float fame, float infamy, int goodwill)
        {
            WorldComponent_Reputation.Current?.Record(f, fame, infamy);
            if (goodwill != 0)
            {
                Patch_Faction_TryAffectGoodwillWith.Suppress = true;
                try { Faction.OfPlayer.TryAffectGoodwillWith(f, goodwill, canSendMessage: true, canSendHostilityLetter: true); }
                finally { Patch_Faction_TryAffectGoodwillWith.Suppress = false; }
            }
        }
    }

    // ===================================================================== Carta de dízimo

    public class ChoiceLetter_EQTithe : ChoiceLetter
    {
        public FactionAllegianceDef allegianceDef;
        public Map map;
        public int multiplier = 1;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || allegianceDef == null)
                {
                    yield return Option_Close;
                    yield break;
                }
                foreach (TitheOption opt in allegianceDef.titheOptions)
                {
                    TitheOption o = opt;
                    int count = Math.Max(1, o.count * multiplier);
                    string label = o.label.Formatted(count.Named("COUNT"));
                    var dia = new DiaOption(label);
                    string disabled = CanPay(o, count, out Pawn pawn);
                    if (disabled != null)
                        dia.Disable(disabled);
                    else if (pawn != null)
                        dia = new DiaOption(label + ": " + pawn.LabelShort);
                    dia.action = () =>
                    {
                        Pay(o, count);
                        WorldComponent_Allegiance.ApplyOutcome(relatedFaction, o.fame, o.infamy, o.goodwill);
                        if (o.favor > 0)
                            EQFavor.Award(relatedFaction, o.favor, allegianceDef.titheLabel);
                        WorldComponent_Allegiance.Current?.ResolveTithe();
                        if (!o.resultText.NullOrEmpty())
                            Messages.Message(o.resultText, MessageTypeDefOf.NeutralEvent);
                        Find.LetterStack.RemoveLetter(this);
                    };
                    dia.resolveTree = true;
                    yield return dia;
                }
                yield return Option_Postpone;
            }
        }

        private string CanPay(TitheOption o, int count, out Pawn pawn)
        {
            pawn = null;
            if (map == null || !Find.Maps.Contains(map))
                return "Sem colônia";
            switch (o.type)
            {
                case TitheOptionType.Silver:
                    return EQItems.CountOnMap(map, ThingDefOf.Silver) >= count ? null : "Falta prata (" + count + ")";
                case TitheOptionType.Things:
                    return o.thingDef != null && EQItems.CountOnMap(map, o.thingDef) >= count ? null : "Falta " + o.thingDef?.label + " (" + count + ")";
                case TitheOptionType.Prisoner:
                    pawn = map.mapPawns.PrisonersOfColonySpawned.FirstOrDefault();
                    return pawn != null ? null : "Nenhum prisioneiro";
                case TitheOptionType.Colonist:
                    pawn = ChooseColonist(o);
                    return pawn != null ? null : "Ninguém adequado";
                default:
                    return null;
            }
        }

        private Pawn ChooseColonist(TitheOption o)
        {
            var colonists = map.mapPawns.FreeColonistsSpawned.Where(p => !p.IsQuestLodger()).ToList();
            if (colonists.Count < 2) return null; // nunca leva o último colono
            if (o.preferChildren)
            {
                Pawn child = colonists.Where(p => DevelopmentalStageExtensions.Child(p.DevelopmentalStage))
                    .OrderByDescending(p => p.GetStatValue(StatDefOf.PsychicSensitivity)).FirstOrDefault();
                if (child != null) return child;
            }
            return colonists.OrderByDescending(p => p.GetStatValue(StatDefOf.PsychicSensitivity)).FirstOrDefault();
        }

        private void Pay(TitheOption o, int count)
        {
            switch (o.type)
            {
                case TitheOptionType.Silver:
                    EQItems.TakeFromMap(map, ThingDefOf.Silver, count);
                    break;
                case TitheOptionType.Things:
                    EQItems.TakeFromMap(map, o.thingDef, count);
                    break;
                case TitheOptionType.Prisoner:
                case TitheOptionType.Colonist:
                    CanPay(o, count, out Pawn pawn);
                    if (pawn != null) EQItems.HandOver(pawn, relatedFaction);
                    break;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref allegianceDef, "allegianceDef");
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref multiplier, "multiplier", 1);
        }
    }

    // ===================================================================== Itens e pessoas

    public static class EQItems
    {
        private static IEnumerable<Thing> Available(Map map, ThingDef def) =>
            map.listerThings.ThingsOfDef(def).Where(t => t.Spawned && !t.IsForbidden(Faction.OfPlayer) && map.areaManager.Home[t.Position]);

        public static int CountOnMap(Map map, ThingDef def) => def == null ? 0 : Available(map, def).Sum(t => t.stackCount);

        public static void TakeFromMap(Map map, ThingDef def, int count)
        {
            foreach (Thing t in Available(map, def).ToList())
            {
                if (count <= 0) break;
                int take = Math.Min(count, t.stackCount);
                count -= take;
                if (take >= t.stackCount) t.Destroy();
                else t.SplitOff(take).Destroy();
            }
        }

        /// <summary>A pessoa é levada pela potência (sai do mapa e passa a pertencer a ela).</summary>
        public static void HandOver(Pawn pawn, Faction to)
        {
            if (pawn.guest != null && pawn.IsPrisoner)
                pawn.guest.SetGuestStatus(null);
            if (pawn.Spawned) pawn.DeSpawn();
            if (to != null && pawn.Faction != to) pawn.SetFaction(to);
            if (!pawn.IsWorldPawn())
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
        }
    }
}
