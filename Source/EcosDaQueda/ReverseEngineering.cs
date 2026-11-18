using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Ajustes da engenharia reversa (DefModExtension na receita EQ_EngenhariaReversa).</summary>
    public class ReverseEngineeringExtension : DefModExtension
    {
        /// <summary>Fração do custo do projeto por item: de minFraction (barato) a maxFraction (valor >= valueForMax).</summary>
        public float minFraction = 0.15f;
        public float maxFraction = 0.30f;
        public float valueForMax = 1500f;
        /// <summary>Item gasto rende menos: a fração é multiplicada pela vida (com este piso).</summary>
        public float minHitPointsFactor = 0.5f;
        /// <summary>Sem os pré-requisitos, o projeto para aqui (fração do custo) e fica liberado na bancada.</summary>
        public float capWithoutPrereqs = 0.99f;
        /// <summary>Itens mais baratos que isto (por unidade), material e comestíveis não ensinam nada.</summary>
        public float minMarketValue = 25f;
        /// <summary>Infâmia na potência dona quando o projeto é patente dela e o jogador não tem a licença.</summary>
        public float infamy = 2f;
        public string text;
        public string capText;
        public string piracyText;
    }

    /// <summary>
    /// Bloco 3c: engenharia reversa. Desmontar na bancada própria um item (ou prédio minificado) fabricado por um projeto
    /// não concluído dá 15–30% do custo do projeto, conforme o valor e o estado do item. Sem os pré-requisitos, o progresso
    /// para em 99% e o projeto fica liberado para terminar na bancada. Patente de potência sem licença dá Infâmia.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class EQReverse
    {
        /// <summary>Item → projetos que o fabricam (receitas e pré-requisitos de construção).</summary>
        public static readonly Dictionary<ThingDef, List<ResearchProjectDef>> Sources = new Dictionary<ThingDef, List<ResearchProjectDef>>();

        public static RecipeDef Recipe => DefDatabase<RecipeDef>.GetNamedSilentFail("EQ_EngenhariaReversa");

        public static ReverseEngineeringExtension Settings => Recipe?.GetModExtension<ReverseEngineeringExtension>() ?? new ReverseEngineeringExtension();

        static EQReverse()
        {
            float minValue = Settings.minMarketValue;
            void Add(ThingDef d, ResearchProjectDef r)
            {
                if (d == null || r == null || r.knowledgeCategory != null || r.baseCost <= 0f)
                    return;
                if (d.IsStuff || d.IsIngestible || d.BaseMarketValue < minValue)
                    return;
                if (!Sources.TryGetValue(d, out List<ResearchProjectDef> l))
                    Sources[d] = l = new List<ResearchProjectDef>();
                if (!l.Contains(r))
                    l.Add(r);
            }
            foreach (RecipeDef rec in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (rec.products == null || rec.products.Count == 0 || rec.IsSurgery)
                    continue;
                List<ResearchProjectDef> pre = EQLicenses.RecipePrereqs(rec).ToList();
                if (pre.Count == 0)
                    continue;
                foreach (ThingDefCountClass p in rec.products)
                    if (p.thingDef != null && p.thingDef.category == ThingCategory.Item && !p.thingDef.IsCorpse)
                        foreach (ResearchProjectDef r in pre)
                            Add(p.thingDef, r);
            }
            foreach (ThingDef d in DefDatabase<ThingDef>.AllDefsListForReading)
                if (d.category == ThingCategory.Building && d.Minifiable && d.researchPrerequisites != null)
                    foreach (ResearchProjectDef r in d.researchPrerequisites)
                        Add(d, r);
        }

        public static bool HasSources(ThingDef d) => d != null && Sources.ContainsKey(d);

        private static bool Waiting(ResearchProjectDef r, ReverseEngineeringExtension s)
            => !r.PrerequisitesCompleted && r.ProgressReal >= r.Cost * s.capWithoutPrereqs - 0.5f;

        /// <summary>Projeto que o item ensina (null = nada a aprender). Prefere o travado, o de nível mais alto e o mais adiantado.</summary>
        public static ResearchProjectDef PickProject(Thing t)
        {
            t = t?.GetInnerIfMinified();
            if (t == null || Current.ProgramState != ProgramState.Playing || !Sources.TryGetValue(t.def, out List<ResearchProjectDef> list))
                return null;
            ReverseEngineeringExtension s = Settings;
            ResearchProjectDef best = null;
            int bestKey = int.MinValue;
            float bestProg = -1f;
            foreach (ResearchProjectDef r in list)
            {
                if (r.IsFinished || Waiting(r, s))
                    continue;
                int key = (EQResearch.HasAccess(r) ? 0 : 100) + (int)r.techLevel;
                float prog = r.ProgressPercent;
                if (key > bestKey || (key == bestKey && prog > bestProg))
                {
                    best = r;
                    bestKey = key;
                    bestProg = prog;
                }
            }
            return best;
        }

        /// <summary>Fração do custo que o item rende.</summary>
        public static float Fraction(Thing t)
        {
            t = t.GetInnerIfMinified();
            ReverseEngineeringExtension s = Settings;
            float f = Mathf.Lerp(s.minFraction, s.maxFraction, Mathf.Clamp01(t.MarketValue / Mathf.Max(1f, s.valueForMax)));
            if (t.def.useHitPoints && t.MaxHitPoints > 0)
                f *= Mathf.Max(s.minHitPointsFactor, (float)t.HitPoints / t.MaxHitPoints);
            return f;
        }

        /// <summary>Aplica a engenharia reversa do item. Devolve o projeto que avançou (ou null).</summary>
        public static ResearchProjectDef Apply(Thing ingredient, Map map)
        {
            Thing t = ingredient?.GetInnerIfMinified();
            ResearchProjectDef r = PickProject(t);
            if (r == null)
                return null;
            ReverseEngineeringExtension s = Settings;
            ResearchManager rm = Find.ResearchManager;
            float before = rm.GetProgress(r);
            float amount = Fraction(t) * r.Cost;
            bool capped = false;
            if (!r.PrerequisitesCompleted)
            {
                // FinishProject concluiria os pré-requisitos de graça: para antes e libera o resto na bancada.
                float cap = r.Cost * s.capWithoutPrereqs;
                if (before + amount >= cap)
                {
                    amount = Mathf.Max(0f, cap - before);
                    capped = true;
                    if (EQResearch.Branch(r) != null && !EQResearch.HasAccess(r))
                        WorldComponent_EQResearch.Current?.Grant(r);
                }
            }
            rm.AddProgress(r, amount);
            LookTargets look = map != null && ingredient.Spawned ? new LookTargets(ingredient.Position, map) : LookTargets.Invalid;
            if (!s.text.NullOrEmpty())
                Messages.Message(s.text.Formatted(t.LabelNoCount.Named("ITEM"), r.LabelCap.Named("PROJECT"),
                    (amount / r.Cost).ToStringPercent().Named("GAIN"), r.ProgressPercent.ToStringPercent().Named("TOTAL")), look, MessageTypeDefOf.PositiveEvent, false);
            if (capped && !s.capText.NullOrEmpty())
                Messages.Message(s.capText.Formatted(r.LabelCap.Named("PROJECT")), look, MessageTypeDefOf.NeutralEvent, false);
            ResearchBranchDef b = EQLicenses.OwnedBranch(r);
            if (b != null && s.infamy > 0f && !EQLicenses.IsLicensed(r))
            {
                Faction owner = EQUtil.MainFaction(b.faction);
                if (owner != null && !owner.defeated && owner != Faction.OfPlayer)
                {
                    WorldComponent_Reputation.Current?.Record(owner, 0f, s.infamy);
                    if (!s.piracyText.NullOrEmpty())
                        Messages.Message(s.piracyText.Formatted(owner.Named("FACTION"), t.LabelNoCount.Named("ITEM"), s.infamy.ToString("0.#").Named("INFAMY")),
                            look, MessageTypeDefOf.NegativeEvent, false);
                }
            }
            return r;
        }
    }

    public class RecipeWorker_EQReverseEngineering : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            EQReverse.Apply(ingredient, map);
            base.ConsumeIngredient(ingredient, recipe, map);
        }
    }

    /// <summary>Itens que não ensinam nada (projeto já concluído, sem projeto, ou parado sem os pré-requisitos).</summary>
    public class SpecialThingFilterWorker_EQNothingToLearn : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t) => EQReverse.PickProject(t) == null;

        public override bool CanEverMatch(ThingDef def) => true;

        public override bool AlwaysMatches(ThingDef def) => !EQReverse.HasSources(def) && def != ThingDefOf.MinifiedThing;
    }
}
