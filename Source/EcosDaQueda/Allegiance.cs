using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    // ===================================================================== Defs

    /// <summary>Um posto dentro de uma potência.</summary>
    public class AllegianceRank
    {
        public string label;
        [MustTranslate] public string description;
        public float fameRequired;
        /// <summary>Somado à melhora de preço com a própria potência.</summary>
        public float priceGain;
        /// <summary>Entregue a cada stipendIntervalDays.</summary>
        public List<ThingDefCountClass> stipend = new List<ThingDefCountClass>();
    }

    public enum TitheOptionType { Silver, Things, Prisoner, Colonist, Refuse }

    /// <summary>Uma opção da carta de dízimo.</summary>
    public class TitheOption
    {
        public string label;
        public TitheOptionType type;
        public ThingDef thingDef;
        /// <summary>Quantidade no posto 1; multiplicada por (posto) — posto 2 = 2×, etc.</summary>
        public int count;
        /// <summary>Colonist: prefere crianças.</summary>
        public bool preferChildren;
        public float fame;
        public float infamy;
        public int goodwill;
        /// <summary>Mensagem após escolher.</summary>
        public string resultText;
    }

    /// <summary>Configuração de lealdade de uma potência (1.7/Defs/EQ_Allegiance.xml).</summary>
    public class FactionAllegianceDef : Def
    {
        public FactionDef faction;
        public List<AllegianceRank> ranks = new List<AllegianceRank>();
        public float stipendIntervalDays = 10f;

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

        public int RankIndex(float fame)
        {
            int idx = 0;
            for (int i = 0; i < ranks.Count; i++)
                if (fame >= ranks[i].fameRequired) idx = i;
            return idx;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string e in base.ConfigErrors()) yield return e;
            if (faction == null) yield return "FactionAllegianceDef sem faction.";
            if (ranks.NullOrEmpty()) yield return "FactionAllegianceDef sem ranks.";
        }
    }

    // ===================================================================== Estado

    /// <summary>
    /// Ponte para o sistema de lealdade de outro mod. O módulo do Hegemony preenche
    /// PlayerOverlordProvider com "a quem o jogador jurou fealdade".
    /// </summary>
    public static class EQAllegiance
    {
        public static Func<Faction> PlayerOverlordProvider;

        public static Faction PlayerOverlord
        {
            get
            {
                try { return EQUtil.Canonical(PlayerOverlordProvider?.Invoke()); }
                catch { return null; }
            }
        }
    }

    /// <summary>Membro de qual potência o jogador é, posto, estipêndio e dízimos.</summary>
    public class WorldComponent_Allegiance : WorldComponent
    {
        public Faction memberOf;
        public int joinedTick = -1;
        public int lastRank = -1;
        public int lastStipendTick = -1;
        public int lastTitheTick = -1;
        public int pendingTitheUntil = -1;   // >= 0 = existe dízimo aguardando resposta

        private const int CheckInterval = 2500; // 1 hora de jogo

        public WorldComponent_Allegiance(World world) : base(world)
        {
        }

        public static WorldComponent_Allegiance Current => Find.World?.GetComponent<WorldComponent_Allegiance>();

        public FactionAllegianceDef Def => memberOf == null ? null : FactionAllegianceDef.For(memberOf.def);

        public int RankIndex
        {
            get
            {
                var d = Def;
                var rep = WorldComponent_Reputation.Current;
                if (d == null || rep == null) return -1;
                return d.RankIndex(rep.Get(memberOf).fame);
            }
        }

        public AllegianceRank Rank => RankIndex >= 0 ? Def.ranks[RankIndex] : null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref memberOf, "memberOf");
            Scribe_Values.Look(ref joinedTick, "joinedTick", -1);
            Scribe_Values.Look(ref lastRank, "lastRank", -1);
            Scribe_Values.Look(ref lastStipendTick, "lastStipendTick", -1);
            Scribe_Values.Look(ref lastTitheTick, "lastTitheTick", -1);
            Scribe_Values.Look(ref pendingTitheUntil, "pendingTitheUntil", -1);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            int now = Find.TickManager.TicksGame;
            if (now % CheckInterval != 0)
                return;

            UpdateMembership(now);
            if (memberOf == null)
                return;

            var d = Def;
            Map map = Find.AnyPlayerHomeMap;
            if (d == null || map == null)
                return;

            // Promoção
            int rank = RankIndex;
            if (rank > lastRank && lastRank >= 0)
                SendRankLetter(d, rank, promoted: true);
            lastRank = Math.Max(lastRank, rank);

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
                SendTithe(d, map, rank);
            }
        }

        private void UpdateMembership(int now)
        {
            Faction overlord = EQAllegiance.PlayerOverlord;
            if (overlord != null && FactionAllegianceDef.For(overlord.def) == null)
                overlord = null; // jurou lealdade a uma facção que não é uma das potências
            if (overlord == memberOf)
                return;

            Faction old = memberOf;
            memberOf = overlord;
            pendingTitheUntil = -1;

            if (old != null)
            {
                var oldDef = FactionAllegianceDef.For(old.def);
                WorldComponent_Reputation.Current?.Record(old, 0f, oldDef?.leaveInfamy ?? 0f);
                Find.LetterStack.ReceiveLetter("Desligamento: " + old.Name,
                    "Você não é mais membro de " + old.Name + ". Seu crachá foi recolhido, seu nome riscado dos registros e sua foto virou alvo de dardos na sala de descanso.",
                    LetterDefOf.NegativeEvent, null, old);
            }

            if (memberOf != null)
            {
                joinedTick = now;
                lastStipendTick = now;
                lastTitheTick = now;
                lastRank = RankIndex;
                SendRankLetter(Def, lastRank, promoted: false);
            }
            else
            {
                lastRank = -1;
            }
        }

        private void SendRankLetter(FactionAllegianceDef d, int rank, bool promoted)
        {
            if (d == null || rank < 0) return;
            AllegianceRank r = d.ranks[rank];
            string label = (promoted ? "Promoção: " : "Juramento: ") + r.label;
            string text = (promoted
                    ? memberOf.Name + " reconhece seus serviços. Seu novo posto é " + r.label + "."
                    : "Você jurou lealdade a " + memberOf.Name + ". Seu posto inicial é " + r.label + ".")
                + "\n\n" + r.description + "\n\n" + DescribeRank(d, rank);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, null, memberOf);
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
            if (rank + 1 < d.ranks.Count)
                lines.Add("Próximo posto: " + d.ranks[rank + 1].label + " (Fama " + d.ranks[rank + 1].fameRequired + ")");
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
                memberOf.Name + " enviou seu estipêndio de " + r.label + ". Assine o recibo, por favor.",
                LetterDefOf.PositiveEvent, new LookTargets(cell, map), memberOf);
        }

        private void SendTithe(FactionAllegianceDef d, Map map, int rank)
        {
            var letter = (ChoiceLetter_EQTithe)LetterMaker.MakeLetter(d.titheLabel, d.titheText, DefDatabase<LetterDef>.GetNamed("EQ_TitheLetter"), memberOf);
            letter.allegianceDef = d;
            letter.map = map;
            letter.multiplier = rank + 1;
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
