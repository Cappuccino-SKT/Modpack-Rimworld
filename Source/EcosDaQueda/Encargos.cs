using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    // Etapa 11b: encargos. A partir de um cargo (minRank), a potência entrega ao membro algo dela para administrar
    // (cota da Forja, filial da Hélice, sala de leitura do Silêncio, temporada da corte do Coro). Fica salvo, tem metas
    // por período e um relatório no fim de cada período: cumprir dá favor e Fama; metade ou mais, quase nada; menos
    // que metade conta uma falta (as mesmas das obrigações da 11a: 3 faltas = rebaixamento). Entregas são tiradas da
    // área de casa no dia do relatório. Recusar o encargo ou renunciar a ele também conta uma falta.

    public enum EncargoGoalType { DeliverThing, DeliverTag, TradeSilver, QualityCrafted, ResearchFinished, RitualFinished }

    public class EncargoGoal
    {
        public EncargoGoalType type;
        [MustTranslate] public string label;
        public ThingDef thing;
        public string tradeTag;
        public int count = 1;
        public QualityCategory minQuality = QualityCategory.Good;
        /// <summary>Quanto a meta cresce por cargo acima do mínimo (0,5 = +50% por cargo).</summary>
        public float perRank = 0.5f;

        public bool IsDelivery => type == EncargoGoalType.DeliverThing || type == EncargoGoalType.DeliverTag;

        public int Required(int rankAbove) => Math.Max(1, (int)Math.Ceiling(count * (1f + perRank * Math.Max(0, rankAbove))));

        public int Available(Map map)
        {
            if (map == null) return 0;
            if (type == EncargoGoalType.DeliverThing) return EQItems.CountOnMap(map, thing);
            if (type == EncargoGoalType.DeliverTag) return EQItems.CountTagged(map, tradeTag);
            return 0;
        }

        public void Take(Map map, int n)
        {
            if (map == null || n <= 0) return;
            if (type == EncargoGoalType.DeliverThing) EQItems.TakeFromMap(map, thing, n);
            else if (type == EncargoGoalType.DeliverTag) EQItems.TakeTagged(map, tradeTag, n);
        }
    }

    public class EncargoDef : Def
    {
        public FactionDef faction;
        /// <summary>Cargo mínimo (0 = 1º cargo). Abaixo disso o encargo acaba sozinho.</summary>
        public int minRank = 2;
        public float periodDays = 15f;
        public float offerCooldownDays = 10f;
        public float offerDeadlineDays = 3f;
        [MustTranslate] public string offerLabel;
        [MustTranslate] public string offerText;
        [MustTranslate] public string acceptLabel;
        [MustTranslate] public string refuseLabel;
        [MustTranslate] public string renounceLabel;
        public int refuseFavorLoss = 3;
        public List<EncargoGoal> goals = new List<EncargoGoal>();
        public int successFavor = 6;
        public float successFame = 5f;
        public List<ThingDefCountClass> rewardThings = new List<ThingDefCountClass>();
        public int partialFavor = 1;
        public int failFavorLoss = 4;
        [MustTranslate] public string successText;
        [MustTranslate] public string partialText;
        [MustTranslate] public string failText;

        public static EncargoDef For(Faction f) => f == null ? null : DefDatabase<EncargoDef>.AllDefsListForReading.FirstOrDefault(d => d.faction == f.def);
    }

    /// <summary>O encargo em andamento (só um: o jogador é membro de uma potência).</summary>
    public class EncargoState : IExposable
    {
        public EncargoDef def;
        public int startTick;
        public int periodStart;
        public int periods;
        public int successes;
        public int fails;
        public bool reminded;
        public List<int> progress = new List<int>();

        public int PeriodEnd => periodStart + (int)(def.periodDays * 60000);

        public void ResetPeriod(int now)
        {
            periodStart = now;
            reminded = false;
            progress = def.goals.Select(_ => 0).ToList();
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref startTick, "startTick");
            Scribe_Values.Look(ref periodStart, "periodStart");
            Scribe_Values.Look(ref periods, "periods");
            Scribe_Values.Look(ref successes, "successes");
            Scribe_Values.Look(ref fails, "fails");
            Scribe_Values.Look(ref reminded, "reminded");
            Scribe_Collections.Look(ref progress, "progress", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && def != null)
            {
                if (progress == null) progress = new List<int>();
                while (progress.Count < def.goals.Count) progress.Add(0);
            }
        }
    }

    public class WorldComponent_Encargos : WorldComponent
    {
        public EncargoState active;
        public Dictionary<string, int> lastOffer = new Dictionary<string, int>();
        public const float ReminderDays = 2f;

        public WorldComponent_Encargos(World world) : base(world) { }

        public static WorldComponent_Encargos Current => Find.World?.GetComponent<WorldComponent_Encargos>();

        public Faction ActiveFaction => active?.def?.faction == null ? null : Find.FactionManager.FirstFactionOfDef(active.def.faction);

        /// <summary>Cargo acima do mínimo do encargo (as metas crescem com ele).</summary>
        public int RankAbove => Math.Max(0, (WorldComponent_Allegiance.Current?.RankIndex ?? 0) - (active?.def.minRank ?? 0));

        public override void WorldComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (now % 250 != 77) return;
            CheckOfferDeadlines(now);
            if (active != null)
            {
                if (active.def == null) { active = null; return; }
                string why = InvalidReason(active.def);
                if (why != null) { End(why, penalize: false); return; }
                if (!active.reminded && now >= active.PeriodEnd - (int)(ReminderDays * 60000))
                {
                    active.reminded = true;
                    Messages.Message(active.def.LabelCap + ": relatório em " + ReminderDays.ToString("0") + " dias. " + ProgressLine(), MessageTypeDefOf.CautionInput);
                }
                if (now >= active.PeriodEnd) Report();
                return;
            }
            if (now % 2500 != 77 % 2500 || now < 60000 * 3 || OfferPending) return;
            var alg = WorldComponent_Allegiance.Current;
            EncargoDef d = EncargoDef.For(alg?.memberOf);
            if (d != null && OfferBlock(d) == null) Offer(d);
        }

        /// <summary>null se o membro pode ter o encargo agora.</summary>
        public static string InvalidReason(EncargoDef d)
        {
            var alg = WorldComponent_Allegiance.Current;
            Faction f = Find.FactionManager.FirstFactionOfDef(d.faction);
            if (f == null || f.defeated) return "a potência deixou de existir";
            if (alg?.memberOf != f) return "o assentamento deixou de ser membro";
            if (alg.RankIndex < d.minRank) return "o cargo ficou abaixo do exigido";
            if (Find.AnyPlayerHomeMap == null) return "sem colônia";
            return null;
        }

        public string OfferBlock(EncargoDef d, bool ignoreTime = false)
        {
            if (active != null) return "já há um encargo";
            string why = InvalidReason(d);
            if (why != null) return why;
            if (!ignoreTime && lastOffer.TryGetValue(d.defName, out int last) && Find.TickManager.TicksGame - last < d.offerCooldownDays * 60000)
                return "oferecido há pouco";
            return null;
        }

        public bool OfferPending => Find.LetterStack.LettersListForReading.Any(l => l is ChoiceLetter_EQEncargo e && !e.resolved && !e.isReport);

        public void Offer(EncargoDef d)
        {
            Faction f = Find.FactionManager.FirstFactionOfDef(d.faction);
            int now = Find.TickManager.TicksGame;
            lastOffer[d.defName] = now;
            var letter = (ChoiceLetter_EQEncargo)LetterMaker.MakeLetter(d.offerLabel, d.offerText + "\n\n" + GoalsText(d, Math.Max(0, (WorldComponent_Allegiance.Current?.RankIndex ?? 0) - d.minRank), null)
                + "\n\nPrazo para responder: " + d.offerDeadlineDays.ToString("0") + " dias. Sem resposta, conta como recusa.",
                DefDatabase<LetterDef>.GetNamed("EQ_EncargoLetter"), f);
            letter.encargo = d;
            letter.deadline = now + (int)(d.offerDeadlineDays * 60000);
            Find.LetterStack.ReceiveLetter(letter);
        }

        private static void CheckOfferDeadlines(int now)
        {
            foreach (Letter l in Find.LetterStack.LettersListForReading.ToList())
            {
                if (l is ChoiceLetter_EQEncargo e && !e.resolved && e.deadline >= 0 && now >= e.deadline)
                {
                    Messages.Message(e.encargo?.offerLabel + ": sem resposta, conta como recusa.", MessageTypeDefOf.NegativeEvent);
                    e.Refuse();
                    Find.LetterStack.RemoveLetter(e);
                }
            }
        }

        public void Accept(EncargoDef d)
        {
            int now = Find.TickManager.TicksGame;
            active = new EncargoState { def = d, startTick = now };
            active.ResetPeriod(now);
            Messages.Message(d.LabelCap + " aceito: primeiro relatório em " + d.periodDays.ToString("0") + " dias.", MessageTypeDefOf.PositiveEvent);
        }

        /// <summary>Um evento contável aconteceu (chamado por EQFavor.Notify).</summary>
        public void Notify(FavorEventType type, int count, QualityCategory quality)
        {
            if (active?.def == null || count <= 0) return;
            for (int i = 0; i < active.def.goals.Count; i++)
            {
                EncargoGoal g = active.def.goals[i];
                bool match = g.type == EncargoGoalType.TradeSilver && type == FavorEventType.TradeSilver
                    || g.type == EncargoGoalType.ResearchFinished && type == FavorEventType.ResearchFinished
                    || g.type == EncargoGoalType.RitualFinished && type == FavorEventType.RitualFinished
                    || g.type == EncargoGoalType.QualityCrafted && type == FavorEventType.QualityCrafted && quality >= g.minQuality;
                if (match) active.progress[i] += count;
            }
        }

        public string ProgressLine()
        {
            if (active?.def == null) return "";
            Map map = Find.AnyPlayerHomeMap;
            var parts = new List<string>();
            for (int i = 0; i < active.def.goals.Count; i++)
            {
                EncargoGoal g = active.def.goals[i];
                int req = g.Required(RankAbove);
                int have = g.IsDelivery ? g.Available(map) : active.progress[i];
                parts.Add(g.label + " " + Math.Min(have, req) + "/" + req + (g.IsDelivery ? " (no armazém)" : ""));
            }
            return string.Join("; ", parts);
        }

        public static string GoalsText(EncargoDef d, int rankAbove, EncargoState st)
        {
            var sb = new StringBuilder("Metas a cada " + d.periodDays.ToString("0") + " dias:");
            foreach (EncargoGoal g in d.goals)
                sb.Append("\n  - " + g.label + ": " + g.Required(rankAbove) + (g.IsDelivery ? " (entregues no dia do relatório, tirados da área de casa)" : ""));
            sb.Append("\nCumprir tudo: +" + d.successFavor + " de favor, +" + d.successFame.ToString("0") + " de Fama e uma falta apagada.");
            sb.Append(" Metade ou mais: +" + d.partialFavor + ". Menos que metade: -" + d.failFavorLoss + " e uma falta (3 faltas = rebaixamento).");
            return sb.ToString();
        }

        /// <summary>Fim do período: entrega o que houver, avalia e manda o relatório. Devolve o resultado (teste).</summary>
        public string Report()
        {
            if (active?.def == null) return "sem encargo";
            EncargoDef d = active.def;
            Faction f = ActiveFaction;
            Map map = Find.AnyPlayerHomeMap;
            int above = RankAbove;
            var sb = new StringBuilder();
            float total = 0f;
            for (int i = 0; i < d.goals.Count; i++)
            {
                EncargoGoal g = d.goals[i];
                int req = g.Required(above);
                int got;
                if (g.IsDelivery)
                {
                    got = Math.Min(req, g.Available(map));
                    g.Take(map, got);
                }
                else got = Math.Min(req, active.progress[i]);
                total += (float)got / req;
                sb.AppendLine("  - " + g.label + ": " + got + " de " + req + (got >= req ? "" : "  <color=#ff8866>(faltou)</color>"));
            }
            float frac = d.goals.Count == 0 ? 1f : total / d.goals.Count;
            active.periods++;
            string verdict, flavor;
            LetterDef ldef;
            if (frac >= 0.999f)
            {
                active.successes++;
                verdict = "cumprido";
                flavor = d.successText;
                ldef = LetterDefOf.PositiveEvent;
                if (d.successFame > 0) WorldComponent_Allegiance.ApplyOutcome(f, d.successFame, 0f, 0);
                EQFavor.Award(f, d.successFavor, d.label);
                WorldComponent_PowerEvents.Current?.ClearStrike(f);
                DropRewards(d, map, f);
            }
            else if (frac >= 0.5f)
            {
                verdict = "cumprido em parte (" + frac.ToStringPercent() + ")";
                flavor = d.partialText;
                ldef = LetterDefOf.NeutralEvent;
                EQFavor.Award(f, d.partialFavor, d.label);
            }
            else
            {
                active.fails++;
                verdict = "descumprido (" + frac.ToStringPercent() + ")";
                flavor = d.failText;
                ldef = LetterDefOf.NegativeEvent;
                LoseFavor(f, d.failFavorLoss);
                WorldComponent_PowerEvents.Current?.AddStrike(f);
            }
            int now = Find.TickManager.TicksGame;
            active.ResetPeriod(now);
            string text = flavor + "\n\nRelatório do período " + active.periods + ": " + verdict + ".\n" + sb.ToString().TrimEnd()
                + "\n\nHistórico: " + active.successes + " cumpridos, " + active.fails + " descumpridos.\n\n" + GoalsText(d, RankAbove, active);
            var letter = (ChoiceLetter_EQEncargo)LetterMaker.MakeLetter("Relatório: " + d.label, text, DefDatabase<LetterDef>.GetNamed("EQ_EncargoLetter"), f);
            letter.encargo = d;
            letter.isReport = true;
            Find.LetterStack.ReceiveLetter(letter);
            return verdict;
        }

        private static void DropRewards(EncargoDef d, Map map, Faction f)
        {
            if (map == null || d.rewardThings.NullOrEmpty()) return;
            var things = new List<Thing>();
            foreach (ThingDefCountClass c in d.rewardThings)
            {
                if (c.thingDef == null) continue;
                int left = c.count;
                while (left > 0)
                {
                    Thing t = ThingMaker.MakeThing(c.thingDef, GenStuff.DefaultStuffFor(c.thingDef));
                    t.stackCount = Math.Min(left, c.thingDef.stackLimit);
                    left -= t.stackCount;
                    things.Add(t);
                }
            }
            DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, things, faction: f);
        }

        public static void LoseFavor(Faction f, int amount)
        {
            Pawn leader = WorldComponent_Allegiance.Current?.leader;
            if (amount > 0 && leader?.royalty != null && leader.royalty.GetFavor(f) > 0)
                leader.royalty.TryRemoveFavor(f, Math.Min(amount, leader.royalty.GetFavor(f)));
        }

        /// <summary>Encerra o encargo. Com penalize (renúncia), conta uma falta e tira favor.</summary>
        public void End(string reason, bool penalize)
        {
            if (active?.def == null) { active = null; return; }
            EncargoDef d = active.def;
            Faction f = ActiveFaction;
            active = null;
            if (f != null) lastOffer[d.defName] = Find.TickManager.TicksGame;
            if (penalize)
            {
                LoseFavor(f, d.refuseFavorLoss);
                WorldComponent_PowerEvents.Current?.AddStrike(f);
            }
            Messages.Message(d.LabelCap + " encerrado: " + reason + ".", penalize ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.NeutralEvent);
        }

        public string Describe()
        {
            if (active?.def == null) return null;
            int left = active.PeriodEnd - Find.TickManager.TicksGame;
            return active.def.LabelCap + " (relatório em " + (left / 60000f).ToString("0.0") + " dias)";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref active, "active");
            Scribe_Collections.Look(ref lastOffer, "lastOffer", LookMode.Value, LookMode.Value);
            if (lastOffer == null) lastOffer = new Dictionary<string, int>();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && active?.def == null) active = null;
        }
    }

    /// <summary>Carta do encargo: oferta (aceitar/recusar, com prazo) ou relatório (continuar/renunciar).</summary>
    public class ChoiceLetter_EQEncargo : ChoiceLetter
    {
        public EncargoDef encargo;
        public bool isReport;
        public int deadline = -1;
        public bool resolved;

        public override bool CanDismissWithRightClick => isReport;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || encargo == null || resolved)
                {
                    yield return Option_Close;
                    yield break;
                }
                var comp = WorldComponent_Encargos.Current;
                if (isReport)
                {
                    var keep = new DiaOption("Continuar no encargo") { resolveTree = true };
                    keep.action = () => { resolved = true; Find.LetterStack.RemoveLetter(this); };
                    yield return keep;
                    var quit = new DiaOption(encargo.renounceLabel ?? "Renunciar ao encargo (uma falta)") { resolveTree = true };
                    if (comp?.active?.def != encargo) quit.Disable("o encargo já acabou");
                    quit.action = () => { resolved = true; comp?.End("renúncia", penalize: true); Find.LetterStack.RemoveLetter(this); };
                    yield return quit;
                    yield break;
                }
                var accept = new DiaOption(encargo.acceptLabel ?? "Aceitar") { resolveTree = true };
                string block = comp?.OfferBlock(encargo, ignoreTime: true);
                if (block != null) accept.Disable(block);
                accept.action = () => { resolved = true; comp?.Accept(encargo); Find.LetterStack.RemoveLetter(this); };
                yield return accept;
                var refuse = new DiaOption(encargo.refuseLabel ?? "Recusar (uma falta)") { resolveTree = true };
                refuse.action = () => { Refuse(); Find.LetterStack.RemoveLetter(this); };
                yield return refuse;
                yield return Option_Postpone;
            }
        }

        public void Refuse()
        {
            if (resolved) return;
            resolved = true;
            Faction f = relatedFaction;
            WorldComponent_Encargos.LoseFavor(f, encargo.refuseFavorLoss);
            WorldComponent_PowerEvents.Current?.AddStrike(f);
            Messages.Message(encargo.LabelCap + ": recusado.", MessageTypeDefOf.NegativeEvent);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref encargo, "encargo");
            Scribe_Values.Look(ref isReport, "isReport");
            Scribe_Values.Look(ref deadline, "deadline", -1);
            Scribe_Values.Look(ref resolved, "resolved");
        }
    }
}
