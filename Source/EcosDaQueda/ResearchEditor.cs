using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Pesquisa criada no editor de árvore (EcosDaQueda-Dev/kits, página Pesquisa), gerada em Defs/Pesquisa/EQ_Pesquisa.xml.
    /// <see cref="unlocks"/> são defNames de construções, plantas, pisos, itens (todas as receitas que fazem o item) ou
    /// receitas; na carga cada um passa a exigir este projeto (com <see cref="replace"/>, só ele: sai da pesquisa antiga).
    /// <see cref="branch"/> escolhe o ramo (licença); com <see cref="keepTab"/>, o projeto fica na aba da árvore do editor
    /// e na posição desenhada lá, em vez de ir para a aba do ramo.
    /// </summary>
    public class ResearchEditorExt : DefModExtension
    {
        public List<string> unlocks = new List<string>();
        public bool replace = true;
        public ResearchBranchDef branch;
        public bool keepTab = true;
        /// <summary>Com o SeedsPlease: cada planta desbloqueada ganha uma receita de semente que exige este projeto
        /// (extrair da colheita, ou propagar a partir de uma semente quando a colheita não dá semente).</summary>
        public bool seeds = true;
    }

    public static class EQResearchUnlocks
    {
        private static readonly AccessTools.FieldRef<ResearchProjectDef, List<Def>> fUnlocked =
            AccessTools.FieldRefAccess<ResearchProjectDef, List<Def>>("cachedUnlockedDefs");
        private static readonly AccessTools.FieldRef<ResearchProjectDef, float> fX = AccessTools.FieldRefAccess<ResearchProjectDef, float>("x");
        private static readonly AccessTools.FieldRef<ResearchProjectDef, float> fY = AccessTools.FieldRefAccess<ResearchProjectDef, float>("y");

        public static void ApplyAll()
        {
            int n = 0;
            foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                ResearchEditorExt ext = r.GetModExtension<ResearchEditorExt>();
                if (ext == null) continue;
                foreach (string w in Apply(r, ext)) Log.Warning("[Ecos da Queda] pesquisa " + r.defName + ": " + w);
                if (ext.keepTab)
                {
                    // posição desenhada no editor (o Clean Research Sort e o YART ainda podem reorganizar depois)
                    fX(r) = r.researchViewX;
                    fY(r) = r.researchViewY;
                }
                n++;
            }
            if (n > 0)
            {
                ClearCaches();
                // depois de todos os construtores estáticos: o SeedsPlease cria as sementes no dele
                LongEventHandler.ExecuteWhenFinished(EQSeedRecipes.Apply);
            }
        }

        public static void ClearCaches()
        {
            foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading) fUnlocked(r) = null;
        }

        /// <summary>Aplica os desbloqueios de um projeto; devolve os avisos (nomes que não existem).</summary>
        public static List<string> Apply(ResearchProjectDef proj, ResearchEditorExt ext)
        {
            var warn = new List<string>();
            foreach (string name in ext.unlocks)
            {
                ThingDef t = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                if (t != null)
                {
                    if (t.plant != null && t.plant.Sowable)
                        t.plant.sowResearchPrerequisites = Set(t.plant.sowResearchPrerequisites, proj, ext.replace);
                    else if (t.designationCategory != null)
                        t.researchPrerequisites = Set(t.researchPrerequisites, proj, ext.replace);
                    else
                    {
                        List<RecipeDef> recipes = RecipesFor(t);
                        if (recipes.Count == 0) warn.Add(name + " não é construção, planta, nem item fabricável");
                        foreach (RecipeDef rec in recipes) SetRecipe(rec, proj, ext.replace);
                    }
                    continue;
                }
                TerrainDef ter = DefDatabase<TerrainDef>.GetNamedSilentFail(name);
                if (ter != null) { ter.researchPrerequisites = Set(ter.researchPrerequisites, proj, ext.replace); continue; }
                RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(name);
                if (recipe != null) { SetRecipe(recipe, proj, ext.replace); continue; }
                warn.Add(name + " não existe (mod fora da lista?)");
            }
            return warn;
        }

        public static List<RecipeDef> RecipesFor(ThingDef t) =>
            DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.products != null && r.products.Any(p => p.thingDef == t)).ToList();

        private static List<ResearchProjectDef> Set(List<ResearchProjectDef> list, ResearchProjectDef p, bool replace)
        {
            if (replace || list == null) return new List<ResearchProjectDef> { p };
            if (!list.Contains(p)) list.Add(p);
            return list;
        }

        private static void SetRecipe(RecipeDef r, ResearchProjectDef p, bool replace)
        {
            // Na lista (não no campo único): a tela de pesquisa só lista cirurgias desbloqueadas pela lista.
            if (replace) { r.researchPrerequisite = null; r.researchPrerequisites = new List<ResearchProjectDef> { p }; return; }
            if (r.researchPrerequisite == p) return;
            r.researchPrerequisites = r.researchPrerequisites ?? new List<ResearchProjectDef>();
            if (!r.researchPrerequisites.Contains(p)) r.researchPrerequisites.Add(p);
        }

        /// <summary>O def passou a exigir o projeto? (para o teste)</summary>
        public static bool Requires(string name, ResearchProjectDef p)
        {
            ThingDef t = DefDatabase<ThingDef>.GetNamedSilentFail(name);
            if (t != null)
            {
                if (t.plant != null && t.plant.Sowable) return t.plant.sowResearchPrerequisites?.Contains(p) == true;
                if (t.designationCategory != null) return t.researchPrerequisites?.Contains(p) == true;
                List<RecipeDef> recs = RecipesFor(t);
                return recs.Count > 0 && recs.All(r => RecipeRequires(r, p));
            }
            TerrainDef ter = DefDatabase<TerrainDef>.GetNamedSilentFail(name);
            if (ter != null) return ter.researchPrerequisites?.Contains(p) == true;
            RecipeDef rec = DefDatabase<RecipeDef>.GetNamedSilentFail(name);
            return rec != null && RecipeRequires(rec, p);
        }

        private static bool RecipeRequires(RecipeDef r, ResearchProjectDef p) => r.researchPrerequisite == p || r.researchPrerequisites?.Contains(p) == true;
    }

    /// <summary>
    /// Receitas de semente por planta (SeedsPlease Lite Redux, dependência opcional: sem ele nada acontece).
    /// O SeedsPlease extrai todas as sementes numa receita só ("ExtractSeeds"); aqui cada planta das árvores do editor
    /// ganha "extrair sementes de X" (a colheita sai da receita geral) ou, se a colheita não dá semente (madeira,
    /// farinha, produto dividido entre plantas), "propagar semente de X" (1 semente vira 2). As duas exigem o projeto.
    /// </summary>
    public static class EQSeedRecipes
    {
        public const int PropagateYield = 2;
        public static readonly List<RecipeDef> Created = new List<RecipeDef>();

        public static bool IsSeed(ThingDef t) => t?.modExtensions?.Any(e => e.GetType().Name == "Seed") == true;

        public static void Apply()
        {
            RecipeDef gen = DefDatabase<RecipeDef>.GetNamedSilentFail("ExtractSeeds");
            if (gen == null || gen.recipeUsers.NullOrEmpty()) return;
            var taken = (Dictionary<Type, HashSet<ushort>>)AccessTools.Field(typeof(ShortHashGiver), "takenHashesPerDeftype").GetValue(null);
            if (!taken.TryGetValue(typeof(RecipeDef), out HashSet<ushort> hashes)) taken[typeof(RecipeDef)] = hashes = new HashSet<ushort>();
            var giveHash = AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash");
            int extractCount = Mathf.Max(1, (int)(gen.ingredients?.FirstOrDefault()?.GetBaseCount() ?? 8));
            var seen = new HashSet<ThingDef>();
            foreach (ResearchProjectDef proj in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                ResearchEditorExt ext = proj.GetModExtension<ResearchEditorExt>();
                if (ext == null || !ext.seeds) continue;
                foreach (string name in ext.unlocks)
                {
                    ThingDef plant = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                    ThingDef seed = plant?.blueprintDef;
                    if (plant?.plant == null || !IsSeed(seed) || !seen.Add(seed)) continue;
                    ThingDef produce = plant.plant.harvestedThingDef;
                    ThingDefCountClass bp = produce?.butcherProducts?.FirstOrDefault();
                    RecipeDef r;
                    if (bp != null && bp.thingDef == seed)
                    {
                        r = Make("EQ_ExtrairSemente_" + plant.defName, "extrair sementes de " + produce.label,
                            "Extrai sementes de " + plant.label + " de " + produce.label + ".", gen, produce, extractCount, seed, bp.count,
                            gen.workAmount, gen.workSkill, proj);
                        gen.fixedIngredientFilter?.SetAllow(produce, false);
                        gen.defaultIngredientFilter?.SetAllow(produce, false);
                        foreach (IngredientCount ic in gen.ingredients) ic.filter?.SetAllow(produce, false);
                    }
                    else
                        r = Make("EQ_PropagarSemente_" + plant.defName, "propagar " + seed.label,
                            "Multiplica uma semente de " + plant.label + " em mudas e estacas: 1 vira " + PropagateYield + ".", gen, seed, 1,
                            seed, PropagateYield, 1200f, SkillDefOf.Plants, proj);
                    giveHash.Invoke(null, new object[] { r, typeof(RecipeDef), hashes });
                    DefDatabase<RecipeDef>.Add(r);
                    Created.Add(r);
                }
            }
            var cache = AccessTools.Field(typeof(ThingDef), "allRecipesCached");
            foreach (ThingDef u in gen.recipeUsers) cache.SetValue(u, null);
            EQResearchUnlocks.ClearCaches();
        }

        private static RecipeDef Make(string defName, string label, string desc, RecipeDef gen, ThingDef ingredient, int count,
            ThingDef seed, int yield, float work, SkillDef skill, ResearchProjectDef proj)
        {
            var r = new RecipeDef
            {
                defName = defName, label = label, description = desc, jobString = gen.jobString,
                workAmount = work, workSpeedStat = gen.workSpeedStat, workSkill = skill,
                effectWorking = gen.effectWorking, soundWorking = gen.soundWorking,
                recipeUsers = new List<ThingDef>(gen.recipeUsers),
                products = new List<ThingDefCountClass> { new ThingDefCountClass(seed, yield) },
                researchPrerequisites = new List<ResearchProjectDef> { proj },
                modContentPack = proj.modContentPack,
                fixedIngredientFilter = new ThingFilter(),
            };
            var ic = new IngredientCount();
            ic.SetBaseCount(count);
            ic.filter.SetAllow(ingredient, true);
            r.ingredients = new List<IngredientCount> { ic };
            r.fixedIngredientFilter.SetAllow(ingredient, true);
            r.ResolveReferences();
            r.fixedIngredientFilter.SetAllow(ingredient, true);
            ic.filter.SetAllow(ingredient, true);
            r.defaultIngredientFilter?.SetAllow(ingredient, true);
            return r;
        }
    }
}
