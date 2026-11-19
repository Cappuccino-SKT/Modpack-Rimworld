using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EcosDaQueda
{
    /// <summary>Uma estrutura que ensina pesquisa ao ser investigada (nomes em texto: some sem o mod/DLC).</summary>
    public class StructureLesson
    {
        public string building;
        public List<string> research = new List<string>();
        /// <summary>Fração fixa do custo do projeto (-1 = pelo valor, como a engenharia reversa).</summary>
        public float fraction = -1f;
    }

    /// <summary>Investigação de estrutura: ajustes e o mapa estrutura → pesquisa das ruínas do Velho Mundo.</summary>
    public class StructureInvestigationDef : Def
    {
        public List<StructureLesson> lessons = new List<StructureLesson>();
        /// <summary>Trabalho (pontos de pesquisa do colono) por célula da estrutura, com piso e teto.</summary>
        public float workPerCell = 600f;
        public float minWork = 800f;
        public float maxWork = 6000f;
        /// <summary>Estruturas antigas (faixa sem fração própria).</summary>
        public float defaultFraction = 0.25f;
        public string text;
        public string nothingText;
    }

    /// <summary>
    /// Investigar estrutura: como a engenharia reversa (3c), mas no próprio prédio, sem bancada e sem pesquisa prévia.
    /// O colono (trabalho de pesquisa) estuda a estrutura no lugar e a desmonta; rende parte do projeto que a constrói
    /// (pré-requisitos do prédio) ou, nas ruínas do Velho Mundo, o que ela ensina (lista em EQ_Structures.xml:
    /// gerador antigo → energia elétrica etc.). Mesmo teto de 99% sem pré-requisitos, liberação do projeto e pirataria.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class EQStructures
    {
        public static readonly Dictionary<ThingDef, List<ResearchProjectDef>> Sources = new Dictionary<ThingDef, List<ResearchProjectDef>>();
        public static readonly Dictionary<ThingDef, float> FixedFraction = new Dictionary<ThingDef, float>();

        public static StructureInvestigationDef Settings => DefDatabase<StructureInvestigationDef>.GetNamedSilentFail("EQ_InvestigacaoEstrutura");
        public static DesignationDef Designation => DefDatabase<DesignationDef>.GetNamedSilentFail("EQ_InvestigarEstrutura");
        public static JobDef Job => DefDatabase<JobDef>.GetNamedSilentFail("EQ_InvestigarEstrutura");

        public static int LessonCount;
        public static string LastResult;

        static EQStructures()
        {
            void Add(ThingDef d, ResearchProjectDef r)
            {
                if (d == null || r == null || r.knowledgeCategory != null || r.baseCost <= 0f) return;
                if (!Sources.TryGetValue(d, out List<ResearchProjectDef> l)) Sources[d] = l = new List<ResearchProjectDef>();
                if (!l.Contains(r)) l.Add(r);
            }
            foreach (ThingDef d in DefDatabase<ThingDef>.AllDefsListForReading)
                if (d.category == ThingCategory.Building && !d.IsFrame && !d.isUnfinishedThing && d.researchPrerequisites != null && d.building != null)
                    foreach (ResearchProjectDef r in d.researchPrerequisites)
                        Add(d, r);
            StructureInvestigationDef s = Settings;
            if (s == null) return;
            foreach (StructureLesson l in s.lessons)
            {
                ThingDef d = DefDatabase<ThingDef>.GetNamedSilentFail(l.building);
                if (d == null) continue;
                bool any = false;
                foreach (string rn in l.research)
                {
                    ResearchProjectDef r = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(rn);
                    if (r == null) continue;
                    Add(d, r);
                    any = true;
                }
                if (!any) continue;
                LessonCount++;
                FixedFraction[d] = l.fraction >= 0f ? l.fraction : s.defaultFraction;
            }
        }

        /// <summary>Projeto que a estrutura ensina agora (null = nada). Mesma preferência da engenharia reversa.</summary>
        public static ResearchProjectDef PickProject(Thing t)
        {
            if (t == null || Current.ProgramState != ProgramState.Playing || !Sources.TryGetValue(t.def, out List<ResearchProjectDef> list)) return null;
            float cap = EQReverse.Settings.capWithoutPrereqs;
            ResearchProjectDef best = null;
            int bestKey = int.MinValue;
            float bestProg = -1f;
            foreach (ResearchProjectDef r in list)
            {
                if (r.IsFinished || !r.PrerequisitesCompleted && r.ProgressReal >= r.Cost * cap - 0.5f) continue;
                // trancado primeiro; depois o que já tem os pré-requisitos e o mais básico (o gerador ensina energia antes de baterias)
                int key = (EQResearch.HasAccess(r) ? 0 : 100) + (r.PrerequisitesCompleted ? 50 : 0) - (int)r.techLevel;
                float prog = r.ProgressPercent;
                if (key > bestKey || key == bestKey && prog > bestProg) { best = r; bestKey = key; bestProg = prog; }
            }
            return best;
        }

        public static float Fraction(Thing t)
        {
            if (FixedFraction.TryGetValue(t.def, out float f)) return f;
            ReverseEngineeringExtension s = EQReverse.Settings;
            f = Mathf.Lerp(s.minFraction, s.maxFraction, Mathf.Clamp01(t.MarketValue / Mathf.Max(1f, s.valueForMax)));
            if (t.def.useHitPoints && t.MaxHitPoints > 0) f *= Mathf.Max(s.minHitPointsFactor, (float)t.HitPoints / t.MaxHitPoints);
            return f;
        }

        public static float WorkFor(Thing t)
        {
            StructureInvestigationDef s = Settings;
            if (s == null) return 1000f;
            return Mathf.Clamp(s.workPerCell * t.def.size.Area, s.minWork, s.maxWork);
        }

        public static AcceptanceReport CanInvestigate(Thing t)
        {
            if (!(t is Building b) || b.def.category != ThingCategory.Building || b.def.IsFrame) return false;
            if (!Sources.ContainsKey(b.def)) return false;
            if (b.Faction != null && b.Faction != Faction.OfPlayer) return "Só estruturas sem dono ou suas (reivindique antes).";
            if (PickProject(b) == null) return "Nada a aprender aqui (projeto concluído ou parado sem os pré-requisitos).";
            return true;
        }

        /// <summary>Fim da investigação: pesquisa ganha, estrutura desmontada (sobra o material de desmontar).</summary>
        public static ResearchProjectDef Apply(Thing t, Pawn by)
        {
            ResearchProjectDef r = PickProject(t);
            Map map = t.MapHeld;
            LookTargets look = t.Spawned ? new LookTargets(t.Position, map) : LookTargets.Invalid;
            string label = t.LabelNoCount;
            if (r == null)
            {
                LastResult = "nada";
                if (!Settings?.nothingText.NullOrEmpty() ?? false) Messages.Message(Settings.nothingText.Formatted(label.Named("ITEM")), look, MessageTypeDefOf.NeutralEvent, false);
            }
            else
            {
                ReverseEngineeringExtension s = EQReverse.Settings;
                float amount = Fraction(t) * r.Cost;
                bool capped = EQResearchGain.AddCapped(r, ref amount, s.capWithoutPrereqs);
                if (capped && EQResearch.Branch(r) != null && !EQResearch.HasAccess(r))
                    WorldComponent_EQResearch.Current?.Grant(r);
                string text = Settings?.text ?? s.text;
                if (!text.NullOrEmpty())
                    Messages.Message(text.Formatted(label.Named("ITEM"), r.LabelCap.Named("PROJECT"),
                        (amount / r.Cost).ToStringPercent().Named("GAIN"), r.ProgressPercent.ToStringPercent().Named("TOTAL")), look, MessageTypeDefOf.PositiveEvent, false);
                if (capped && !s.capText.NullOrEmpty())
                    Messages.Message(s.capText.Formatted(r.LabelCap.Named("PROJECT")), look, MessageTypeDefOf.NeutralEvent, false);
                EQResearchGain.Piracy(r, s.infamy, s.piracyText, label, look);
                LastResult = r.defName + " +" + (amount / r.Cost).ToStringPercent() + " (total " + r.ProgressPercent.ToStringPercent() + ")";
            }
            map?.designationManager.RemoveAllDesignationsOn(t);
            if (!t.Destroyed) t.Destroy(t.def.building?.IsDeconstructible == true ? DestroyMode.Deconstruct : DestroyMode.Vanish);
            return r;
        }
    }

    public class Designator_EQInvestigate : Designator
    {
        protected override DesignationDef Designation => EQStructures.Designation;

        public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Orders;

        public Designator_EQInvestigate()
        {
            defaultLabel = "investigar estrutura";
            defaultDesc = "Manda um pesquisador estudar a estrutura no lugar e desmontá-la para entender como funciona. Rende parte da pesquisa que a constrói; ruínas do Velho Mundo ensinam o que elas eram (um gerador antigo ensina energia elétrica). Patente de potência sem licença dá Infâmia.";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct");
            defaultIconColor = new Color(0.55f, 0.85f, 1f);
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
            soundSucceeded = SoundDefOf.Designate_Deconstruct;
            showReverseDesignatorDisabledReason = false;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map) || !DebugSettings.godMode && c.Fogged(Map)) return false;
            return Top(c) != null;
        }

        private Thing Top(IntVec3 c) => Map.thingGrid.ThingsAt(c).OrderByDescending(t => t.def.altitudeLayer).FirstOrDefault(t => CanDesignateThing(t).Accepted);

        public override void DesignateSingleCell(IntVec3 c) { Thing t = Top(c); if (t != null) DesignateThing(t); }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (Designation == null || Map.designationManager.DesignationOn(t, Designation) != null) return false;
            return EQStructures.CanInvestigate(t);
        }

        public override void DesignateThing(Thing t)
        {
            Map.designationManager.RemoveAllDesignationsOn(t);
            Map.designationManager.AddDesignation(new Designation(t, Designation));
        }

        public override void SelectedUpdate() => GenUI.RenderMouseoverBracket();
    }

    public class WorkGiver_EQInvestigate : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (Designation d in pawn.Map.designationManager.SpawnedDesignationsOfDef(EQStructures.Designation))
                yield return d.target.Thing;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
            => EQStructures.Designation == null || !pawn.Map.designationManager.AnySpawnedDesignationOfDef(EQStructures.Designation);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
            => pawn.Map.designationManager.DesignationOn(t, EQStructures.Designation) != null && pawn.CanReserve(t, 1, -1, null, forced);

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(EQStructures.Job, t);
    }

    public class JobDriver_EQInvestigate : JobDriver
    {
        private float workLeft = -1f;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnThingMissingDesignation(TargetIndex.A, EQStructures.Designation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil study = ToilMaker.MakeToil("EQInvestigate");
            study.initAction = () => { if (workLeft < 0f) workLeft = EQStructures.WorkFor(job.targetA.Thing); };
            study.tickIntervalAction = delta =>
            {
                workLeft -= pawn.GetStatValue(StatDefOf.ResearchSpeed) * delta;
                pawn.skills?.Learn(SkillDefOf.Intellectual, 0.08f * delta);
                if (workLeft <= 0f)
                {
                    EQStructures.Apply(job.targetA.Thing, pawn);
                    ReadyForNextToil();
                }
            };
            study.defaultCompleteMode = ToilCompleteMode.Never;
            study.WithProgressBar(TargetIndex.A, () => 1f - workLeft / Mathf.Max(1f, EQStructures.WorkFor(job.targetA.Thing)));
            study.WithEffect(EffecterDefOf.Research, TargetIndex.A);
            study.activeSkill = () => SkillDefOf.Intellectual;
            yield return study;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workLeft, "workLeft", -1f);
        }
    }

    /// <summary>O botão "investigar estrutura" aparece ao selecionar a estrutura (como desconstruir).</summary>
    [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
    public static class Patch_ReverseDesignatorDatabase_InitDesignators
    {
        public static void Postfix(List<Designator> ___desList)
        {
            if (EQStructures.Designation != null) ___desList?.Add(new Designator_EQInvestigate());
        }
    }
}
