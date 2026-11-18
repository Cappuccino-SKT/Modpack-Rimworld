using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Etapa 5a: condição extra para um evento de potência aparecer.</summary>
    public enum PowerEventCondition { None, PlayerHasMechs, PlayerHasXenotypes, PlayerResearching }

    /// <summary>Uma resposta possível ao evento: custo (prata, itens, itens por etiqueta, prisioneiro) e efeito.</summary>
    public class PowerEventOption
    {
        [MustTranslate] public string label;
        [MustTranslate] public string resultText;
        public int costSilver;
        public List<ThingDefCountClass> costThings = new List<ThingDefCountClass>();
        public string costTradeTag;
        public int costTradeTagCount;
        [MustTranslate] public string costTradeTagLabel;
        public bool handOverPrisoner;

        public float fame;
        public float infamy;
        public int goodwill;
        public int favor;
        /// <summary>Lembrança dada a todos os colonos livres do mapa.</summary>
        public ThoughtDef thought;
        /// <summary>Fração do progresso do projeto de pesquisa atual que se perde.</summary>
        public float researchLoss;
        public List<ThingDefCountClass> rewardThings = new List<ThingDefCountClass>();
    }

    /// <summary>
    /// Evento de uma potência (auditoria da Hélice, inspeção de garantia da Forja, turnê do Coro, chamada de artigos
    /// do Silêncio): carta de escolha que aparece por MTB quando a potência não é hostil e a condição vale.
    /// </summary>
    public class PowerEventDef : Def
    {
        public FactionDef faction;
        public float mtbDays = 30f;
        /// <summary>Intervalo mínimo entre duas vezes do mesmo evento.</summary>
        public float minIntervalDays = 30f;
        public PowerEventCondition condition;
        /// <summary>Só aparece para membros desta potência (com o cargo mínimo).</summary>
        public bool membersOnly;
        public int minRank;
        /// <summary>Fama mínima com a potência (0 = qualquer um; ela só cobra quem conhece).</summary>
        public float minFame;
        [MustTranslate] public string letterLabel;
        [MustTranslate] public string letterText;
        public List<PowerEventOption> options = new List<PowerEventOption>();
        public LetterDef letterDef;
    }

    public class WorldComponent_PowerEvents : WorldComponent
    {
        /// <summary>Dias mínimos entre dois eventos de potência quaisquer.</summary>
        public const float GlobalCooldownDays = 8f;
        private const int CheckInterval = 2500;
        /// <summary>Prazo para responder; depois disso conta como a última opção (recusa).</summary>
        public const int DeadlineTicks = 60000 * 3;

        private Dictionary<string, int> lastFired = new Dictionary<string, int>();
        private int lastAny = -9999999;

        public WorldComponent_PowerEvents(World world) : base(world) { }

        public static WorldComponent_PowerEvents Current => Find.World?.GetComponent<WorldComponent_PowerEvents>();

        public override void WorldComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (now % 250 == 0)
                CheckDeadlines(now);
            if (now % CheckInterval != 1234 % CheckInterval || now < 60000 * 3)
                return;
            if (now - lastAny < GlobalCooldownDays * 60000)
                return;
            foreach (PowerEventDef d in DefDatabase<PowerEventDef>.AllDefsListForReading.InRandomOrder())
            {
                if (BlockReason(d) == null && Rand.MTBEventOccurs(d.mtbDays, 60000f, CheckInterval))
                {
                    Fire(d);
                    return;
                }
            }
        }

        private static void CheckDeadlines(int now)
        {
            foreach (Letter l in Find.LetterStack.LettersListForReading.ToList())
            {
                if (l is ChoiceLetter_EQPowerEvent pe && !pe.resolved && pe.deadline >= 0 && now >= pe.deadline)
                {
                    pe.TimeOut();
                    pe.resolved = true;
                    Find.LetterStack.RemoveLetter(pe);
                }
            }
        }

        public static Faction FactionOf(PowerEventDef d) => d.faction == null ? null : Find.FactionManager.FirstFactionOfDef(d.faction);

        /// <summary>null se o evento pode aparecer agora; senão o motivo.</summary>
        public string BlockReason(PowerEventDef d, bool ignoreTime = false)
        {
            Faction f = FactionOf(d);
            if (f == null || f.defeated) return "potência ausente";
            if (f.HostileTo(Faction.OfPlayer)) return "potência hostil";
            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return "sem colônia";
            if (!ignoreTime && lastFired.TryGetValue(d.defName, out int last) && Find.TickManager.TicksGame - last < d.minIntervalDays * 60000)
                return "aconteceu há pouco";
            var alg = WorldComponent_Allegiance.Current;
            if (d.membersOnly && (alg?.memberOf != f || alg.RankIndex < d.minRank)) return "só para membros";
            if (d.minFame > 0 && (WorldComponent_Reputation.Current?.Get(f)?.fame ?? 0) < d.minFame) return "fama baixa";
            switch (d.condition)
            {
                case PowerEventCondition.PlayerHasMechs:
                    if (!map.mapPawns.PawnsInFaction(Faction.OfPlayer).Any(p => p.RaceProps.IsMechanoid)) return "sem mechs";
                    break;
                case PowerEventCondition.PlayerHasXenotypes:
                    if (!map.mapPawns.FreeColonistsSpawned.Any(p => p.genes?.Xenotype != null && p.genes.Xenotype != XenotypeDefOf.Baseliner || p.genes?.CustomXenotype != null)) return "sem xenos";
                    break;
                case PowerEventCondition.PlayerResearching:
                    if (Find.ResearchManager.GetProject() == null) return "sem pesquisa em andamento";
                    break;
            }
            return null;
        }

        public void Fire(PowerEventDef d)
        {
            Map map = Find.AnyPlayerHomeMap;
            Faction f = FactionOf(d);
            if (map == null || f == null) return;
            int now = Find.TickManager.TicksGame;
            lastFired[d.defName] = now;
            lastAny = now;
            var letter = (ChoiceLetter_EQPowerEvent)LetterMaker.MakeLetter(d.letterLabel, d.letterText,
                d.letterDef ?? DefDatabase<LetterDef>.GetNamed("EQ_PowerEventLetter"), f);
            letter.eventDef = d;
            letter.map = map;
            letter.deadline = now + DeadlineTicks;
            Find.LetterStack.ReceiveLetter(letter);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref lastFired, "lastFired", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref lastAny, "lastAny", -9999999);
            if (lastFired == null) lastFired = new Dictionary<string, int>();
        }
    }

    public class ChoiceLetter_EQPowerEvent : ChoiceLetter
    {
        public PowerEventDef eventDef;
        public Map map;
        public int deadline = -1;
        public bool resolved;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || eventDef == null)
                {
                    yield return Option_Close;
                    yield break;
                }
                foreach (PowerEventOption opt in eventDef.options)
                {
                    PowerEventOption o = opt;
                    var dia = new DiaOption(o.label);
                    string disabled = CanPay(o);
                    if (disabled != null)
                        dia.Disable(disabled);
                    dia.action = () =>
                    {
                        Choose(o);
                        Find.LetterStack.RemoveLetter(this);
                    };
                    dia.resolveTree = true;
                    yield return dia;
                }
                yield return Option_Postpone;
            }
        }

        /// <summary>Ignorar até o fim do prazo = a última opção (recusar). Chamado pelo WorldComponent.</summary>
        public void TimeOut()
        {
            PowerEventOption last = eventDef?.options.LastOrDefault();
            if (last != null && !resolved && CanPay(last) == null)
            {
                Messages.Message(eventDef.letterLabel + ": sem resposta, conta como recusa.", MessageTypeDefOf.NegativeEvent);
                Choose(last);
            }
        }

        public string CanPay(PowerEventOption o)
        {
            if (map == null || !Find.Maps.Contains(map)) return "Sem colônia";
            if (o.costSilver > 0 && EQItems.CountOnMap(map, ThingDefOf.Silver) < o.costSilver) return "Falta prata (" + o.costSilver + ")";
            foreach (ThingDefCountClass c in o.costThings)
                if (c.thingDef != null && EQItems.CountOnMap(map, c.thingDef) < c.count) return "Falta " + c.thingDef.label + " (" + c.count + ")";
            if (!o.costTradeTag.NullOrEmpty() && EQItems.CountTagged(map, o.costTradeTag) < o.costTradeTagCount)
                return "Falta " + (o.costTradeTagLabel ?? o.costTradeTag) + " (" + o.costTradeTagCount + ")";
            if (o.handOverPrisoner && map.mapPawns.PrisonersOfColonySpawned.FirstOrDefault() == null) return "Nenhum prisioneiro";
            if (o.researchLoss > 0 && Find.ResearchManager.GetProject() == null) return null;
            return null;
        }

        public void Choose(PowerEventOption o)
        {
            if (resolved) return;
            resolved = true;
            Faction f = relatedFaction;
            if (o.costSilver > 0) EQItems.TakeFromMap(map, ThingDefOf.Silver, o.costSilver);
            foreach (ThingDefCountClass c in o.costThings)
                if (c.thingDef != null) EQItems.TakeFromMap(map, c.thingDef, c.count);
            if (!o.costTradeTag.NullOrEmpty()) EQItems.TakeTagged(map, o.costTradeTag, o.costTradeTagCount);
            if (o.handOverPrisoner)
            {
                Pawn p = map.mapPawns.PrisonersOfColonySpawned.FirstOrDefault();
                if (p != null) EQItems.HandOver(p, f);
            }
            WorldComponent_Allegiance.ApplyOutcome(f, o.fame, o.infamy, o.goodwill);
            if (o.favor > 0) EQFavor.Award(f, o.favor, eventDef.letterLabel);
            if (o.thought != null)
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                    p.needs?.mood?.thoughts?.memories?.TryGainMemory(o.thought);
            if (o.researchLoss > 0) EQPowerEvents.LoseResearch(o.researchLoss);
            if (!o.rewardThings.NullOrEmpty())
            {
                var things = new List<Thing>();
                foreach (ThingDefCountClass c in o.rewardThings)
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
            if (!o.resultText.NullOrEmpty())
                Messages.Message(o.resultText, MessageTypeDefOf.NeutralEvent);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref eventDef, "eventDef");
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref resolved, "resolved");
            Scribe_Values.Look(ref deadline, "deadline", -1);
        }
    }

    public static class EQPowerEvents
    {
        /// <summary>Tira uma fração do progresso do projeto atual ("revisão por pares" do Silêncio).</summary>
        public static string LoseResearch(float fraction)
        {
            ResearchProjectDef proj = Find.ResearchManager.GetProject();
            if (proj == null) return null;
            var dict = Traverse.Create(Find.ResearchManager).Field("progress").GetValue<Dictionary<ResearchProjectDef, float>>();
            if (dict == null || !dict.TryGetValue(proj, out float cur)) return proj.label;
            dict[proj] = Math.Max(0f, cur - proj.CostApparent * fraction);
            Messages.Message("Revisão por pares: o projeto " + proj.LabelCap + " perdeu " + fraction.ToStringPercent() + " do progresso.", MessageTypeDefOf.NegativeEvent);
            return proj.label;
        }
    }
}
