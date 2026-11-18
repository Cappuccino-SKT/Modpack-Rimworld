using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Bloco 4b: zonas da Queda (biomas do Alpha Biomes). O recurso exclusivo de cada zona, analisado na bancada de
    /// engenharia reversa, rende parte de um projeto do ramo ligado à zona (o projeto atual, se for do ramo; senão o mais
    /// adiantado). Mesmas regras da engenharia reversa: teto sem pré-requisitos e Infâmia por patente sem licença.
    /// </summary>
    public class ZoneSampleExtension : DefModExtension
    {
        public BiomeDef biome;
        public ResearchBranchDef branch;
        /// <summary>Fração do custo do projeto por análise, limitada a maxPoints.</summary>
        public float fraction = 0.12f;
        public float maxPoints = 600f;
        public float infamy = 1f;
        public string text;
        public string nothingText;
        public string piracyText;
    }

    public static class EQZones
    {
        public static IEnumerable<RecipeDef> Recipes => DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.HasModExtension<ZoneSampleExtension>());

        public static IEnumerable<ResearchProjectDef> Pool(ZoneSampleExtension z)
        {
            float cap = EQReverse.Settings.capWithoutPrereqs;
            foreach (KeyValuePair<ResearchProjectDef, ResearchBranchDef> kv in EQResearch.BranchOf)
            {
                ResearchProjectDef r = kv.Key;
                if (kv.Value != z.branch || r.IsFinished || r.baseCost <= 0f || r.knowledgeCategory != null)
                    continue;
                if (!r.PrerequisitesCompleted && r.ProgressReal >= r.Cost * cap - 0.5f)
                    continue; // parado no teto
                yield return r;
            }
        }

        /// <summary>O projeto atual, se for do ramo; senão o de pré-requisitos prontos mais adiantado (empate: menor nível).</summary>
        public static ResearchProjectDef Pick(ZoneSampleExtension z)
        {
            List<ResearchProjectDef> pool = Pool(z).ToList();
            if (pool.Count == 0)
                return null;
            ResearchProjectDef cur = Find.ResearchManager.GetProject();
            if (cur != null && pool.Contains(cur))
                return cur;
            return pool.OrderByDescending(r => r.PrerequisitesCompleted).ThenByDescending(r => r.ProgressPercent)
                .ThenBy(r => (int)r.techLevel).ThenBy(r => r.baseCost).First();
        }

        public static ResearchProjectDef Apply(RecipeDef recipe, Pawn doer, Map map)
        {
            ZoneSampleExtension z = recipe.GetModExtension<ZoneSampleExtension>();
            if (z == null || z.branch == null)
                return null;
            LookTargets look = doer != null && doer.Spawned ? new LookTargets(doer) : LookTargets.Invalid;
            string item = recipe.ingredients.FirstOrDefault()?.filter?.AnyAllowedDef?.label ?? recipe.label;
            ResearchProjectDef r = Pick(z);
            if (r == null)
            {
                if (!z.nothingText.NullOrEmpty())
                    Messages.Message(z.nothingText.Formatted(z.branch.label.Named("BRANCH")), look, MessageTypeDefOf.NeutralEvent, false);
                return null;
            }
            float amount = Mathf.Min(z.fraction * r.Cost, z.maxPoints);
            float cap = EQReverse.Settings.capWithoutPrereqs;
            bool capped = EQResearchGain.AddCapped(r, ref amount, cap);
            if (capped && !EQResearch.HasAccess(r))
                WorldComponent_EQResearch.Current?.Grant(r);
            if (!z.text.NullOrEmpty())
                Messages.Message(z.text.Formatted(item.Named("ITEM"), (doer?.LabelShort ?? "?").Named("PAWN"), r.LabelCap.Named("PROJECT"),
                    (amount / r.Cost).ToStringPercent().Named("GAIN"), r.ProgressPercent.ToStringPercent().Named("TOTAL")), look, MessageTypeDefOf.PositiveEvent, false);
            if (capped && !EQReverse.Settings.capText.NullOrEmpty())
                Messages.Message(EQReverse.Settings.capText.Formatted(r.LabelCap.Named("PROJECT")), look, MessageTypeDefOf.NeutralEvent, false);
            EQResearchGain.Piracy(r, z.infamy, z.piracyText, item, look);
            return r;
        }
    }

    public class RecipeWorker_EQZoneSample : RecipeWorker
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            EQZones.Apply(recipe, billDoer, billDoer?.Map);
        }
    }
}
