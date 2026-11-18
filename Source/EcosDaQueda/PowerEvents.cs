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
        /// <summary>Etapa 11a: empresta um colono à potência por N dias (sai do mapa e volta sozinho).</summary>
        public float lendColonistDays;
        /// <summary>Perícia preferida no colono emprestado (o melhor nela vai).</summary>
        public SkillDef lendSkill;

        public float fame;
        public float infamy;
        public int goodwill;
        public int favor;
        /// <summary>Favor tirado do líder (obrigação descumprida).</summary>
        public int favorLoss;
        /// <summary>Etapa 11a: conta uma falta com a potência; 3 faltas = rebaixamento de um cargo.</summary>
        public bool strike;
        /// <summary>Lembrança dada a todos os colonos livres do mapa.</summary>
        public ThoughtDef thought;
        /// <summary>Fração do progresso do projeto de pesquisa atual que se perde.</summary>
        public float researchLoss;
        public List<ThingDefCountClass> rewardThings = new List<ThingDefCountClass>();

        /// <summary>Etapa 5b: muda a Exposição perante a Forja (ajudar os Fora de Linha aumenta).</summary>
        public float exposure;
        /// <summary>Efeito numa segunda facção (ex.: sabotagem fere a Forja; trair a célula fere os Fora de Linha).</summary>
        public FactionDef otherFaction;
        public int otherGoodwill;
        public float otherFame;
        public float otherInfamy;

        /// <summary>Etapa 5e: trégua — a facção da carta não é sorteada para ataques por N dias (serve até para inimigo permanente).</summary>
        public float truceDays;
        /// <summary>Etapa 5c: chance de um contra-ataque de outra facção (se hostil) N dias depois.</summary>
        public FactionDef retaliationFaction;
        public float retaliationChance;
        public float retaliationDelayDays = 2f;
        public float retaliationPointsFactor = 0.8f;
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
        /// <summary>Etapa 11a: obrigação de cargo (só membros, faixa minRank..maxRank, cadência própria).</summary>
        public bool obligation;
        public int maxRank = 99;
        /// <summary>Prazo para responder (ignorar = última opção).</summary>
        public float deadlineDays = 3f;
        /// <summary>Fama mínima com a potência (0 = qualquer um; ela só cobra quem conhece).</summary>
        public float minFame;
        /// <summary>Etapa 5b: só aparece com a Exposição nesta faixa, e com boa vontade mínima com a facção.</summary>
        public float minExposure = -1f;
        public float maxExposure = 1000f;
        public int minGoodwill = -100;
        /// <summary>Etapa 5c: a carta pode vir de facção hostil (ex.: trégua oferecida pelos Não Catalogados).</summary>
        public bool allowHostile;
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

        /// <summary>Dias mínimos entre duas obrigações de cargo.</summary>
        public const float ObligationCooldownDays = 6f;
        public const int StrikesToDemote = 3;

        private Dictionary<string, int> lastFired = new Dictionary<string, int>();
        private int lastAny = -9999999;
        private int lastObligation = -9999999;
        /// <summary>Faltas por potência (defName da facção).</summary>
        public Dictionary<string, int> strikes = new Dictionary<string, int>();
        public List<LentPawn> lent = new List<LentPawn>();
        /// <summary>Etapa 5e: tréguas ativas (defName da facção → tique em que acaba).</summary>
        public Dictionary<string, int> truces = new Dictionary<string, int>();

        public bool InTruce(Faction f) => f != null && truces.TryGetValue(f.def.defName, out int until) && Find.TickManager.TicksGame < until;

        public WorldComponent_PowerEvents(World world) : base(world) { }

        public static WorldComponent_PowerEvents Current => Find.World?.GetComponent<WorldComponent_PowerEvents>();

        public override void WorldComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (now % 250 == 0)
            {
                CheckDeadlines(now);
                ReturnLent(now);
            }
            if (now % CheckInterval != 1234 % CheckInterval || now < 60000 * 3)
                return;
            bool eventFree = now - lastAny >= GlobalCooldownDays * 60000;
            bool obligationFree = now - lastObligation >= ObligationCooldownDays * 60000;
            foreach (PowerEventDef d in DefDatabase<PowerEventDef>.AllDefsListForReading.InRandomOrder())
            {
                if (!(d.obligation ? obligationFree : eventFree)) continue;
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

        /// <summary>Diagnóstico da última volta (executor de testes).</summary>
        public static string LastReturnInfo;

        public int StrikesWith(Faction f) => f != null && strikes.TryGetValue(f.def.defName, out int n) ? n : 0;

        /// <summary>Obrigação descumprida: uma falta; ao chegar a StrikesToDemote, rebaixa o líder um cargo (nunca expulsa).</summary>
        public string AddStrike(Faction f)
        {
            if (f == null) return null;
            int n = StrikesWith(f) + 1;
            var alg = WorldComponent_Allegiance.Current;
            if (n < StrikesToDemote || alg?.memberOf != f)
            {
                strikes[f.def.defName] = n;
                Messages.Message(f.Name + ": obrigação descumprida (" + n + " de " + StrikesToDemote + " faltas antes do rebaixamento).", MessageTypeDefOf.NegativeEvent);
                return "falta " + n;
            }
            strikes[f.def.defName] = 0;
            return EQPowerEvents.Demote(alg, f);
        }

        public void Lend(Pawn p, Faction f, float days)
        {
            Map map = p.MapHeld;
            if (p.Spawned) p.DeSpawn();
            if (!p.IsWorldPawn()) Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.KeepForever);
            lent.Add(new LentPawn { pawn = p, faction = f, returnTick = Find.TickManager.TicksGame + (int)(days * 60000), map = map });
            Messages.Message(p.LabelShort + " foi servir " + f.Name + " por " + days.ToString("0.#") + " dias.", MessageTypeDefOf.NeutralEvent);
        }

        private void ReturnLent(int now)
        {
            for (int i = lent.Count - 1; i >= 0; i--)
            {
                LentPawn l = lent[i];
                if (now < l.returnTick) continue;
                Map map = l.map != null && Find.Maps.Contains(l.map) ? l.map : Find.AnyPlayerHomeMap;
                if (map == null) continue;
                lent.RemoveAt(i);
                LastReturnInfo = l.pawn == null ? "pawn nulo" : l.pawn.LabelShort + " morto=" + l.pawn.Dead + " destruido=" + l.pawn.Destroyed + " descartado=" + l.pawn.Discarded + " mundo=" + l.pawn.IsWorldPawn() + " spawnado=" + l.pawn.Spawned + " faccao=" + l.pawn.Faction?.Name;
                if (l.pawn == null || l.pawn.Dead || l.pawn.Destroyed) continue;
                if (Find.WorldPawns.Contains(l.pawn)) Find.WorldPawns.RemovePawn(l.pawn);
                // Fora do mapa o jogo (ou um mod) pode passar o pawn para a potência que o hospedou: volta como colono.
                if (l.pawn.Faction != Faction.OfPlayer)
                {
                    l.pawn.SetFaction(Faction.OfPlayer);
                    foreach (Faction other in Find.FactionManager.AllFactions)
                        if (other.leader == l.pawn) other.TryGenerateNewLeader();
                }
                if (!CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 cell))
                    cell = CellFinder.RandomEdgeCell(map);
                GenSpawn.Spawn(l.pawn, cell, map);
                Messages.Message(l.pawn.LabelShort + " voltou do serviço para " + (l.faction?.Name ?? "a potência") + ".", l.pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

        public static Faction FactionOf(PowerEventDef d) => d.faction == null ? null : Find.FactionManager.FirstFactionOfDef(d.faction);

        /// <summary>null se o evento pode aparecer agora; senão o motivo.</summary>
        public string BlockReason(PowerEventDef d, bool ignoreTime = false)
        {
            Faction f = FactionOf(d);
            if (f == null || f.defeated) return "potência ausente";
            if (f.HostileTo(Faction.OfPlayer) && !d.allowHostile) return "potência hostil";
            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return "sem colônia";
            if (!ignoreTime && lastFired.TryGetValue(d.defName, out int last) && Find.TickManager.TicksGame - last < d.minIntervalDays * 60000)
                return "aconteceu há pouco";
            var alg = WorldComponent_Allegiance.Current;
            if ((d.membersOnly || d.obligation) && (alg?.memberOf != f || alg.RankIndex < d.minRank)) return "só para membros (cargo " + (d.minRank + 1) + "+)";
            if (d.obligation && alg.RankIndex > d.maxRank) return "cargo acima da faixa";
            if (d.minFame > 0 && (WorldComponent_Reputation.Current?.Get(f)?.fame ?? 0) < d.minFame) return "fama baixa";
            float exp = WorldComponent_Exposure.Current?.exposure ?? 0f;
            if (exp < d.minExposure) return "Exposição baixa (" + exp.ToString("0") + ")";
            if (exp > d.maxExposure) return "Exposição alta (" + exp.ToString("0") + ")";
            if (f.PlayerGoodwill < d.minGoodwill) return "boa vontade baixa";
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
            if (d.obligation) lastObligation = now; else lastAny = now;
            var letter = (ChoiceLetter_EQPowerEvent)LetterMaker.MakeLetter(d.letterLabel, d.letterText,
                d.letterDef ?? DefDatabase<LetterDef>.GetNamed("EQ_PowerEventLetter"), f);
            letter.eventDef = d;
            letter.map = map;
            letter.deadline = now + (int)(d.deadlineDays * 60000);
            Find.LetterStack.ReceiveLetter(letter);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref lastFired, "lastFired", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref lastAny, "lastAny", -9999999);
            Scribe_Values.Look(ref lastObligation, "lastObligation", -9999999);
            Scribe_Collections.Look(ref strikes, "strikes", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref lent, "lent", LookMode.Deep);
            Scribe_Collections.Look(ref truces, "truces", LookMode.Value, LookMode.Value);
            if (truces == null) truces = new Dictionary<string, int>();
            if (lastFired == null) lastFired = new Dictionary<string, int>();
            if (strikes == null) strikes = new Dictionary<string, int>();
            if (lent == null) lent = new List<LentPawn>();
            if (Scribe.mode == LoadSaveMode.PostLoadInit) lent.RemoveAll(l => l?.pawn == null);
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
            if (o.lendColonistDays > 0 && EQPowerEvents.ChooseLent(map, o.lendSkill) == null) return "Ninguém disponível (nunca vai o líder nem o último colono)";
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
            if (o.lendColonistDays > 0)
            {
                Pawn lentPawn = EQPowerEvents.ChooseLent(map, o.lendSkill);
                if (lentPawn != null) WorldComponent_PowerEvents.Current?.Lend(lentPawn, f, o.lendColonistDays);
            }
            WorldComponent_Allegiance.ApplyOutcome(f, o.fame, o.infamy, o.goodwill);
            if (o.favorLoss > 0)
            {
                Pawn leader = WorldComponent_Allegiance.Current?.leader;
                if (leader?.royalty != null && leader.royalty.GetFavor(f) > 0)
                    leader.royalty.TryRemoveFavor(f, System.Math.Min(o.favorLoss, leader.royalty.GetFavor(f)));
            }
            if (o.strike) WorldComponent_PowerEvents.Current?.AddStrike(f);
            if (o.exposure != 0) WorldComponent_Exposure.Current?.Add(o.exposure, eventDef.letterLabel);
            if (o.truceDays > 0 && f != null && WorldComponent_PowerEvents.Current != null)
            {
                WorldComponent_PowerEvents.Current.truces[f.def.defName] = Find.TickManager.TicksGame + (int)(o.truceDays * 60000);
                Messages.Message("Trégua com " + f.Name + " por " + o.truceDays.ToString("0") + " dias: sem ataques dela.", MessageTypeDefOf.PositiveEvent);
            }
            if (o.retaliationFaction != null && (EQPowerEvents.ForceRetaliation || Rand.Chance(o.retaliationChance)))
            {
                EQPowerEvents.ForceRetaliation = false;
                Faction rf = Find.FactionManager.FirstFactionOfDef(o.retaliationFaction);
                if (rf != null && !rf.defeated && rf.HostileTo(Faction.OfPlayer))
                {
                    var raid = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                    raid.faction = rf;
                    raid.forced = true;
                    raid.points = System.Math.Max(raid.points * o.retaliationPointsFactor, 200f);
                    raid.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                    raid.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                    Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, raid),
                        Find.TickManager.TicksGame + (int)(o.retaliationDelayDays * 60000)));
                    EQPowerEvents.LastRetaliation = rf.Name + " em " + o.retaliationDelayDays + " dias";
                }
            }
            if (o.otherFaction != null)
            {
                Faction other = Find.FactionManager.FirstFactionOfDef(o.otherFaction);
                if (other != null && !other.defeated)
                    WorldComponent_Allegiance.ApplyOutcome(other, o.otherFame, o.otherInfamy, o.otherGoodwill);
            }
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

    /// <summary>Colono emprestado a uma potência (fica entre os pawns do mundo até voltar).</summary>
    public class LentPawn : IExposable
    {
        public Pawn pawn;
        public Faction faction;
        public int returnTick;
        public Map map;

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref returnTick, "returnTick");
            Scribe_References.Look(ref map, "map");
        }
    }

    public static class EQPowerEvents
    {
        /// <summary>Teste: o próximo contra-ataque sorteado acontece com certeza.</summary>
        public static bool ForceRetaliation;
        public static string LastRetaliation;

        /// <summary>Quem vai servir: colono livre, nunca o líder nem o último; o melhor na perícia pedida.</summary>
        public static Pawn ChooseLent(Map map, SkillDef skill)
        {
            Pawn leader = WorldComponent_Allegiance.Current?.leader;
            var list = map.mapPawns.FreeColonistsSpawned.Where(p => p != leader && !p.IsQuestLodger() && !p.Downed).ToList();
            if (map.mapPawns.FreeColonistsSpawnedCount < 2 || list.Count == 0) return null;
            return skill == null ? list.RandomElement() : list.OrderByDescending(p => p.skills?.GetSkill(skill)?.Level ?? 0).First();
        }

        /// <summary>Rebaixa o líder um cargo na potência (no 1º cargo só adverte: rebaixamento é a punição máxima).</summary>
        public static string Demote(WorldComponent_Allegiance alg, Faction f)
        {
            int rank = alg.RankIndex;
            var d = alg.Def;
            if (d == null || alg.leader?.royalty == null) return null;
            if (rank <= 0)
            {
                Find.LetterStack.ReceiveLetter("Advertência: " + f.Name, f.Name + " registrou mais três obrigações descumpridas. Abaixo do primeiro cargo não há para onde descer, então a advertência vai para a sua ficha.", LetterDefOf.NegativeEvent, alg.leader, f);
                return "advertência (já no 1º cargo)";
            }
            RoyalTitleDef lower = d.ranks[rank - 1].title;
            alg.leader.royalty.SetTitle(f, lower, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
            Find.LetterStack.ReceiveLetter("Rebaixamento: " + f.Name,
                f.Name + " cansou de esperar. Depois de três obrigações descumpridas, " + alg.leader.LabelShort + " desce para o cargo de " + d.RankLabel(rank - 1, alg.leader) + ". O que o cargo anterior licenciava deixa de valer.",
                LetterDefOf.NegativeEvent, alg.leader, f);
            return "rebaixado para o cargo " + rank;
        }

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

namespace EcosDaQueda
{
    /// <summary>Etapa 5e: facção em trégua não é sorteada como origem de ataque.</summary>
    [HarmonyLib.HarmonyPatch(typeof(RimWorld.IncidentWorker_RaidEnemy), "FactionCanBeGroupSource")]
    public static class Patch_RaidEnemy_FactionCanBeGroupSource_Truce
    {
        public static void Postfix(RimWorld.Faction f, ref bool __result)
        {
            if (__result && WorldComponent_PowerEvents.Current != null && WorldComponent_PowerEvents.Current.InTruce(f))
                __result = false;
        }
    }
}
