using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Etapa 5f: o que cada evento sem dono dá (configurado no IncidentDef).</summary>
    public class WorldEventExtension : DefModExtension
    {
        /// <summary>Abrigo: quantos moradores saem e o que cada um traz.</summary>
        public IntRange pawnCount = new IntRange(1, 2);
        public PawnKindDef pawnKind;
        public List<ThingDefCountClass> carry = new List<ThingDefCountClass>();

        /// <summary>Comboio: pilhas sorteadas (peso = count no formato &lt;Def&gt;n&lt;/Def&gt;; quantidade de cada pilha em stackCount).</summary>
        public List<ThingDefCountClass> cargo = new List<ThingDefCountClass>();
        public IntRange stacks = new IntRange(4, 7);
        public IntRange stackCount = new IntRange(10, 40);
        /// <summary>Comboio: chance de a facção dona vir buscar a carga, e quem.</summary>
        public float claimChance = 0.5f;
        public FactionDef claimFaction;
        public float claimDelayDays = 1f;

        /// <summary>Recall: facção forçada no ataque.</summary>
        public FactionDef raidFaction;
    }

    /// <summary>Abrigo que se abre: moradores de um abrigo do Velho Mundo saem pela primeira vez e pedem para ficar.</summary>
    public class IncidentWorker_EQVaultOpens : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms) =>
            parms.target is Map map && CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out _);

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            var ext = def.GetModExtension<WorldEventExtension>() ?? new WorldEventExtension();
            if (!CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 cell))
                return false;
            var pawns = new List<Pawn>();
            int n = ext.pawnCount.RandomInRange;
            for (int i = 0; i < n; i++)
            {
                Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(ext.pawnKind ?? PawnKindDefOf.Colonist, Faction.OfPlayer,
                    forceGenerateNewPawn: true, mustBeCapableOfViolence: false, fixedIdeo: Faction.OfPlayer.ideos?.PrimaryIdeo));
                foreach (ThingDefCountClass c in ext.carry)
                {
                    if (c.thingDef == null) continue;
                    Thing t = ThingMaker.MakeThing(c.thingDef, GenStuff.DefaultStuffFor(c.thingDef));
                    t.stackCount = c.count;
                    p.inventory.innerContainer.TryAdd(t);
                }
                GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(cell, map, 4), map);
                pawns.Add(p);
            }
            SendStandardLetter(def.letterLabel, def.letterText.Formatted(n.Named("COUNT")), def.letterDef, parms, pawns);
            return true;
        }
    }

    /// <summary>Comboio perdido: a carga de um comboio que não chegou cai perto da colônia; às vezes o dono vem buscar.</summary>
    public class IncidentWorker_EQLostConvoy : IncidentWorker
    {
        /// <summary>Teste: o dono vem buscar a carga com certeza na próxima vez.</summary>
        public static bool ForceClaim;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            var ext = def.GetModExtension<WorldEventExtension>() ?? new WorldEventExtension();
            var pool = ext.cargo.Where(c => c.thingDef != null).ToList();
            if (pool.Count == 0) return false;
            var things = new List<Thing>();
            int stacks = ext.stacks.RandomInRange;
            for (int i = 0; i < stacks; i++)
            {
                ThingDef d = pool.RandomElementByWeight(c => c.count).thingDef;
                Thing t = ThingMaker.MakeThing(d, GenStuff.DefaultStuffFor(d));
                t.stackCount = d.stackLimit <= 1 ? 1 : System.Math.Min(d.stackLimit, ext.stackCount.RandomInRange);
                things.Add(t);
            }
            IntVec3 spot = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(spot, map, things, 110, canInstaDropDuringInit: false, leaveSlag: true, forbid: false);
            string text = def.letterText;
            Faction owner = ext.claimFaction == null ? null : Find.FactionManager.FirstFactionOfDef(ext.claimFaction);
            if (owner != null && !owner.defeated && owner.HostileTo(Faction.OfPlayer) && (ForceClaim || Rand.Chance(ext.claimChance)))
            {
                var raid = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                raid.faction = owner;
                raid.points = System.Math.Max(raid.points * 0.6f, 150f);
                raid.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                raid.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                raid.forced = true;
                int at = Find.TickManager.TicksGame + (int)(ext.claimDelayDays * 60000);
                ForceClaim = false;
                Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, raid), at));
                text += "\n\nO rádio de um dos caixotes ainda está ligado. Alguém do " + owner.Name + " está perguntando onde foi parar a carga.";
            }
            SendStandardLetter(def.letterLabel, text, def.letterDef, parms, new TargetInfo(spot, map));
            return true;
        }
    }

    /// <summary>Recall mecanoide: mechs com o lacre da Forja vencido aparecem para "recolher" o que acham que é da Forja.</summary>
    public class IncidentWorker_EQMechRecall : IncidentWorker_RaidEnemy
    {
        private FactionDef ForcedDef => def.GetModExtension<WorldEventExtension>()?.raidFaction;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            // Mesmo forçado (debug, fila), o recall nunca vem com menos que o mínimo do IncidentDef.
            parms.points = System.Math.Max(parms.points, def.minThreatPoints);
            return base.TryExecuteWorker(parms);
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            FactionDef fd = ForcedDef;
            parms.faction = fd == null ? Faction.OfMechanoids : Find.FactionManager.FirstFactionOfDef(fd);
            return parms.faction != null;
        }

        protected override string GetLetterLabel(IncidentParms parms) => def.letterLabel ?? base.GetLetterLabel(parms);

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns) => (def.letterText ?? "") + "\n\n" + base.GetLetterText(parms, pawns);
    }
}
