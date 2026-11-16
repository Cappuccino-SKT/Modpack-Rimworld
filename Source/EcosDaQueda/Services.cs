using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Um serviço que uma potência oferece aos seus membros (1.7/Defs/EQ_Services.xml).
    /// Custo: prata e/ou uma entidade contida / um prisioneiro. Efeitos combináveis.
    /// </summary>
    public class FactionServiceDef : Def
    {
        public FactionDef faction;
        /// <summary>Posto mínimo (0 = primeiro posto).</summary>
        public int minRank;
        /// <summary>Favor dado ao líder ao usar o serviço.</summary>
        public int favor;
        public float cooldownDays = 10f;
        public int order;

        // Custos
        public int silver;
        public bool requiresHeldEntity;
        public bool requiresPrisoner;
        /// <summary>Itens cobrados do mapa (além da prata). Item de mod ausente é ignorado.</summary>
        public List<ThingDefCountClass> costThings = new List<ThingDefCountClass>();

        // Efeitos
        public List<ThingDefCountClass> things = new List<ThingDefCountClass>();
        public bool callAid;
        public float aidPointsFactor = 1f;
        public float researchPoints;
        public ThoughtDef colonistThought;
        public float fame;
        public string resultText;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string e in base.ConfigErrors()) yield return e;
            if (faction == null) yield return "FactionServiceDef sem faction.";
        }
    }

    /// <summary>Guarda os tempos de recarga dos serviços.</summary>
    public class WorldComponent_Services : WorldComponent
    {
        private Dictionary<string, int> lastUsed = new Dictionary<string, int>();

        public WorldComponent_Services(World world) : base(world)
        {
        }

        public static WorldComponent_Services Current => Find.World?.GetComponent<WorldComponent_Services>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref lastUsed, "lastUsed", LookMode.Value, LookMode.Value);
            if (lastUsed == null) lastUsed = new Dictionary<string, int>();
        }

        public static IEnumerable<FactionServiceDef> ServicesOf(FactionDef faction) =>
            DefDatabase<FactionServiceDef>.AllDefsListForReading.Where(s => s.faction == faction).OrderBy(s => s.order);

        public int CooldownTicksLeft(FactionServiceDef s)
        {
            if (!lastUsed.TryGetValue(s.defName, out int tick)) return 0;
            return Math.Max(0, tick + (int)(s.cooldownDays * GenDate.TicksPerDay) - Find.TickManager.TicksGame);
        }

        /// <summary>Motivo pelo qual o serviço não pode ser usado agora, ou null.</summary>
        public string BlockReason(FactionServiceDef s, Map map)
        {
            var alg = WorldComponent_Allegiance.Current;
            if (alg?.memberOf == null || alg.memberOf.def != s.faction)
                return "Só para membros";
            if (alg.memberOf.HostileTo(Faction.OfPlayer))
                return "Potência hostil";
            if (alg.RankIndex < s.minRank)
                return "Exige o cargo " + alg.Def.RankLabel(Math.Min(s.minRank, alg.Def.ranks.Count - 1));
            int cd = CooldownTicksLeft(s);
            if (cd > 0)
                return "Disponível em " + cd.ToStringTicksToPeriod();
            if (map == null)
                return "Sem colônia";
            if (s.silver > 0 && EQItems.CountOnMap(map, ThingDefOf.Silver) < s.silver)
                return "Falta prata (" + s.silver + ")";
            foreach (ThingDefCountClass c in s.costThings)
                if (c.thingDef != null && EQItems.CountOnMap(map, c.thingDef) < c.count)
                    return "Falta " + c.thingDef.label + " (" + c.count + ")";
            if (s.requiresHeldEntity && HeldEntity(map) == null)
                return "Nenhuma entidade contida";
            if (s.requiresPrisoner && !map.mapPawns.PrisonersOfColonySpawned.Any())
                return "Nenhum prisioneiro";
            if (s.researchPoints > 0f && Find.ResearchManager.GetProject() == null)
                return "Nenhuma pesquisa em andamento";
            return null;
        }

        public static string CostLabel(FactionServiceDef s)
        {
            var parts = new List<string>();
            if (s.silver > 0) parts.Add(s.silver + " de prata");
            foreach (ThingDefCountClass c in s.costThings)
                if (c.thingDef != null) parts.Add(c.count + " " + c.thingDef.label);
            return parts.Count == 0 ? "" : " (" + string.Join(", ", parts) + ")";
        }

        public static Pawn HeldEntity(Map map)
        {
            foreach (Building b in map.listerBuildings.allBuildingsColonist)
                if (b is Building_HoldingPlatform p && p.HeldPawn != null && !p.HeldPawn.RaceProps.Humanlike)
                    return p.HeldPawn;
            return null;
        }

        public void Use(FactionServiceDef s, Map map)
        {
            if (BlockReason(s, map) != null)
                return;
            Faction f = WorldComponent_Allegiance.Current.memberOf;

            // Custos
            if (s.silver > 0)
                EQItems.TakeFromMap(map, ThingDefOf.Silver, s.silver);
            foreach (ThingDefCountClass c in s.costThings)
                if (c.thingDef != null)
                    EQItems.TakeFromMap(map, c.thingDef, c.count);
            if (s.requiresHeldEntity)
            {
                Pawn entity = HeldEntity(map);
                entity?.Destroy();
            }
            if (s.requiresPrisoner)
                EQItems.HandOver(map.mapPawns.PrisonersOfColonySpawned.First(), f);

            // Efeitos
            if (!s.things.NullOrEmpty())
            {
                var made = new List<Thing>();
                foreach (ThingDefCountClass c in s.things)
                {
                    if (c?.thingDef == null) continue;
                    int left = c.count;
                    while (left > 0)
                    {
                        Thing t = ThingMaker.MakeThing(c.thingDef, GenStuff.DefaultStuffFor(c.thingDef));
                        t.stackCount = Math.Min(left, c.thingDef.stackLimit);
                        left -= t.stackCount;
                        made.Add(t);
                    }
                }
                if (made.Count > 0)
                    DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, made, faction: f);
            }
            if (s.callAid)
            {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                parms.faction = f;
                parms.forced = true;
                parms.points = Math.Max(300f, StorytellerUtility.DefaultThreatPointsNow(map) * s.aidPointsFactor);
                parms.raidArrivalModeForQuickMilitaryAid = true;
                IncidentDefOf.RaidFriendly.Worker.TryExecute(parms);
            }
            if (s.researchPoints > 0f)
            {
                ResearchProjectDef proj = Find.ResearchManager.GetProject();
                if (proj != null)
                    Find.ResearchManager.AddProgress(proj, s.researchPoints);
            }
            if (s.colonistThought != null)
            {
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                    p.needs?.mood?.thoughts?.memories?.TryGainMemory(s.colonistThought);
            }
            if (s.fame > 0f)
                WorldComponent_Reputation.Current?.Record(f, s.fame, 0f);
            if (s.favor > 0)
                EQFavor.Award(f, s.favor, s.label);

            lastUsed[s.defName] = Find.TickManager.TicksGame;
            Messages.Message(s.resultText.NullOrEmpty() ? s.LabelCap + ": concluído." : s.resultText, MessageTypeDefOf.PositiveEvent);
        }

        /// <summary>Menu com os serviços da potência do jogador.</summary>
        public static void OpenMenu()
        {
            var alg = WorldComponent_Allegiance.Current;
            var svc = Current;
            if (alg?.memberOf == null || svc == null) return;
            Map map = Find.AnyPlayerHomeMap;
            var options = new List<FloatMenuOption>();
            foreach (FactionServiceDef s in ServicesOf(alg.memberOf.def))
            {
                FactionServiceDef service = s;
                string cost = CostLabel(s);
                string block = svc.BlockReason(s, map);
                var opt = block == null
                    ? new FloatMenuOption(s.LabelCap + cost, () => svc.Use(service, map))
                    : new FloatMenuOption(s.LabelCap + cost + " — " + block, null);
                opt.tooltip = new TipSignal(s.description);
                options.Add(opt);
            }
            if (options.Count == 0)
                options.Add(new FloatMenuOption("Nenhum serviço disponível", null));
            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

    // ===================================================================== O Soro

    public class CompProperties_UseEffectSerum : CompProperties_UseEffect
    {
        /// <summary>Xenotipos de elite sorteados no sucesso.</summary>
        public List<XenotypeDef> xenotypes = new List<XenotypeDef>();
        /// <summary>Xenotipos sorteados quando dá errado.</summary>
        public List<XenotypeDef> failXenotypes = new List<XenotypeDef>();
        public float failChance = 0.25f;
        public string successText = "{0} agora é {1}.";
        public string failText = "{0} agora é {1}.";

        public CompProperties_UseEffectSerum()
        {
            compClass = typeof(CompUseEffect_Serum);
        }
    }

    /// <summary>O Soro do Concílio: reescreve o xenotipo de quem toma. Às vezes para pior.</summary>
    public class CompUseEffect_Serum : CompUseEffect
    {
        private CompProperties_UseEffectSerum Props => (CompProperties_UseEffectSerum)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy?.genes == null)
                return;
            bool fail = Rand.Chance(Props.failChance) && !Props.failXenotypes.NullOrEmpty();
            List<XenotypeDef> pool = (fail ? Props.failXenotypes : Props.xenotypes)
                .Where(x => x != null && x != usedBy.genes.Xenotype).ToList();
            if (pool.Count == 0)
                return;
            XenotypeDef chosen = pool.RandomElement();

            // Remove os xenogenes antigos e aplica o novo xenotipo
            foreach (Gene g in usedBy.genes.Xenogenes.ToList())
                usedBy.genes.RemoveGene(g);
            // Xeno hereditário (ex.: Lote Descontinuado) reescreve a linhagem: sai o padrão antigo,
            // ficam só os genes de cor de pele/cabelo se o novo não trouxer os seus.
            if (chosen.inheritable)
                EQXenotypeUtil.ClearEndogenesKeepingColors(usedBy, chosen);
            usedBy.genes.SetXenotype(chosen);
            EQFavor.Notify(FavorEventType.SerumUsed);

            string text = string.Format(fail ? Props.failText : Props.successText, usedBy.LabelShort, chosen.label);
            Find.LetterStack.ReceiveLetter(fail ? "O Soro: efeito colateral" : "O Soro: sucesso", text,
                fail ? LetterDefOf.NegativeEvent : LetterDefOf.PositiveEvent, usedBy);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (p.genes == null || !p.RaceProps.Humanlike)
                return "Não tem genes para reescrever";
            return base.CanBeUsedBy(p);
        }
    }
}
