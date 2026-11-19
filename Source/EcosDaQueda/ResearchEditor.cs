using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
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
            if (n > 0) ClearCaches();
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
}
