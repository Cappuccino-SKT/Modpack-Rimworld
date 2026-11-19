using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using EcosDaQueda;
using static EcosDaQueda.DebugTools.KitCatalog;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Parte "research" do catálogo do editor (página Pesquisa): abas, ramos, todos os projetos como estão no jogo
    /// (aba, posição, pré-requisitos, o que desbloqueiam) e tudo o que um projeto pode desbloquear (construções, plantas,
    /// pisos, itens fabricáveis e receitas sem produto, como cirurgias), com a pesquisa que exigem hoje.</summary>
    public static class ResearchCatalog
    {
        public static void Append(StringBuilder sb)
        {
            List<ResearchProjectDef> projects = DefDatabase<ResearchProjectDef>.AllDefsListForReading.OrderBy(r => r.defName).ToList();
            sb.Append("{\n\"tabs\": [\n");
            sb.Append(string.Join(",\n", DefDatabase<ResearchTabDef>.AllDefsListForReading.Select(t => "{" + string.Join(",",
                P("def", t.defName), P("label", t.LabelCap.ToString()), P("n", projects.Count(r => r.tab == t))) + "}")));
            sb.Append("\n],\n\"branches\": [\n");
            sb.Append(string.Join(",\n", DefDatabase<ResearchBranchDef>.AllDefsListForReading.OrderBy(b => b.order).Select(b => "{" + string.Join(",",
                P("def", b.defName), P("label", b.LabelCap.ToString()), P("tab", b.tab?.defName ?? ""), P("faction", b.faction?.defName ?? ""),
                P("free", b.free ? 1 : 0)) + "}")));
            sb.Append("\n],\n\"projects\": [\n");
            sb.Append(string.Join(",\n", projects.Select(r =>
            {
                var o = new List<string>
                {
                    P("def", r.defName), P("label", r.LabelCap.ToString()), P("desc", Desc(r.description)),
                    P("cost", r.baseCost), P("tech", r.techLevel.ToString()), P("tab", r.tab?.defName ?? ""),
                    P("x", r.ResearchViewX), P("y", r.ResearchViewY),
                    P("branch", EQResearch.Branch(r)?.defName ?? ""),
                    P("origTab", EQResearch.OriginalTab.TryGetValue(r, out ResearchTabDef ot) ? ot.defName : ""),
                    PL("prereqs", (r.prerequisites ?? new List<ResearchProjectDef>()).Select(p => p.defName)),
                    PL("hidden", (r.hiddenPrerequisites ?? new List<ResearchProjectDef>()).Select(p => p.defName)),
                    P("bench", r.requiredResearchBuilding?.defName ?? ""),
                    PL("facilities", (r.requiredResearchFacilities ?? new List<ThingDef>()).Select(f => f.defName)),
                    P("techprints", r.techprintCount),
                    PL("unlocks", Safe(() => r.UnlockedDefs.Select(d => d.defName).Distinct().ToList())),
                    P("mod", r.modContentPack?.Name ?? "?"), P("pkg", r.modContentPack?.PackageId ?? "?"),
                    P("editor", r.GetModExtension<ResearchEditorExt>() != null ? 1 : 0),
                };
                if (r.knowledgeCategory != null) o.Add(P("knowledge", r.knowledgeCategory.defName));
                return "{" + string.Join(",", o) + "}";
            })));

            // ---- desbloqueáveis
            var recipeUsers = new Dictionary<RecipeDef, List<string>>();
            List<string> Users(RecipeDef rec)
            {
                if (!recipeUsers.TryGetValue(rec, out var l))
                    recipeUsers[rec] = l = Safe(() => rec.AllRecipeUsers.Select(u => u.LabelCap.ToString()).Distinct().Take(4).ToList());
                return l;
            }
            IEnumerable<ResearchProjectDef> RecReq(RecipeDef rec) =>
                (rec.researchPrerequisite != null ? new[] { rec.researchPrerequisite } : new ResearchProjectDef[0]).Concat(rec.researchPrerequisites ?? new List<ResearchProjectDef>());
            var productOf = new Dictionary<ThingDef, List<RecipeDef>>();
            foreach (RecipeDef rec in DefDatabase<RecipeDef>.AllDefsListForReading)
                if (rec.products != null)
                    foreach (ThingDefCountClass pr in rec.products)
                        if (pr.thingDef != null)
                        {
                            if (!productOf.TryGetValue(pr.thingDef, out var l)) productOf[pr.thingDef] = l = new List<RecipeDef>();
                            l.Add(rec);
                        }

            var rows = new List<string>();
            string Row(Def d, string kind, string tex, string icon, string gclass, UnityEngine.Color color, string cat, IEnumerable<ResearchProjectDef> req,
                TechLevel tech, IEnumerable<string> benches, string extra = null)
            {
                var o = new List<string>
                {
                    P("def", d.defName), P("label", d.LabelCap.ToString()), P("kind", kind),
                    P("mod", d.modContentPack?.Name ?? "?"), P("modDir", d.modContentPack?.RootDir ?? ""),
                    P("tex", tex ?? ""), P("icon", icon ?? ""), P("gclass", gclass ?? ""), P("color", Hex(color)), P("cat", cat ?? ""),
                    P("tech", tech.ToString()), P("desc", Short(d.description)),
                    PL("research", req.Where(x => x != null).Select(x => x.defName).Distinct()),
                };
                if (benches != null) o.Add(PL("benches", benches));
                if (extra != null) o.Add(extra);
                return "{" + string.Join(",", o) + "}";
            }
            foreach (ThingDef t in DefDatabase<ThingDef>.AllDefsListForReading.OrderBy(d => d.defName))
            {
                try
                {
                    if (t.plant != null && t.plant.Sowable)
                        rows.Add(Row(t, "plant", t.graphicData?.texPath, t.uiIconPath, t.graphicData?.graphicClass?.Name, t.graphicData?.color ?? UnityEngine.Color.white,
                            "Planta", t.plant.sowResearchPrerequisites ?? new List<ResearchProjectDef>(), t.techLevel, null));
                    else if (t.designationCategory != null)
                        rows.Add(Row(t, "building", t.graphicData?.texPath, t.uiIconPath, t.graphicData?.graphicClass?.Name, t.graphicData?.color ?? UnityEngine.Color.white,
                            t.designationCategory.LabelCap.ToString(), t.researchPrerequisites ?? new List<ResearchProjectDef>(), t.techLevel, null));
                    else if (productOf.TryGetValue(t, out var recs) && !t.IsCorpse)
                        rows.Add(Row(t, "item", t.graphicData?.texPath, t.uiIconPath, t.graphicData?.graphicClass?.Name, t.graphicData?.color ?? UnityEngine.Color.white,
                            t.FirstThingCategory?.LabelCap.ToString() ?? "", recs.SelectMany(RecReq), t.techLevel,
                            recs.SelectMany(Users).Distinct().Take(4), P("recipes", recs.Count)));
                }
                catch (Exception ex) { Log.Warning("[EQ debug] catálogo de pesquisa, " + t.defName + ": " + ex.Message); }
            }
            foreach (TerrainDef t in DefDatabase<TerrainDef>.AllDefsListForReading.Where(d => d.designationCategory != null).OrderBy(d => d.defName))
                rows.Add(Row(t, "terrain", t.texturePath, t.uiIconPath, "", t.color, t.designationCategory.LabelCap.ToString(),
                    t.researchPrerequisites ?? new List<ResearchProjectDef>(), TechLevel.Undefined, null));
            foreach (RecipeDef rec in DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.products.NullOrEmpty()
                && (r.IsSurgery || r.researchPrerequisite != null || !r.researchPrerequisites.NullOrEmpty())).OrderBy(d => d.defName))
                rows.Add(Row(rec, "recipe", "", "", "", UnityEngine.Color.white, rec.IsSurgery ? "Cirurgia" : "Receita", RecReq(rec), TechLevel.Undefined, Users(rec)));
            sb.Append("\n],\n\"unlocks\": [\n").Append(string.Join(",\n", rows));

            sb.Append("\n],\n\"benches\": [\n");
            var facilities = new HashSet<ThingDef>(projects.SelectMany(r => r.requiredResearchFacilities ?? new List<ThingDef>()));
            sb.Append(string.Join(",\n", DefDatabase<ThingDef>.AllDefsListForReading
                .Where(t => (t.thingClass != null && typeof(Building_ResearchBench).IsAssignableFrom(t.thingClass)) || facilities.Contains(t))
                .OrderBy(t => t.defName).Select(t => "{" + string.Join(",", P("def", t.defName), P("label", t.LabelCap.ToString()),
                    P("facility", facilities.Contains(t) ? 1 : 0)) + "}")));
            sb.Append("\n],\n");
            var tags = DefDatabase<FactionDef>.AllDefsListForReading.Select(f => f.categoryTag)
                .Concat(projects.SelectMany(r => r.heldByFactionCategoryTags ?? new List<string>()))
                .Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x);
            sb.Append(PL("factionTags", tags)).Append("\n}");
        }

        /// <summary>Comando "pesquisa": confere as pesquisas do editor carregadas (cada desbloqueio exige o projeto, aba e ramo)
        /// e aplica um projeto de teste criado só na memória a uma construção, planta, piso, item e cirurgia vanilla.</summary>
        public static string Test()
        {
            var sb = new StringBuilder();
            int problemas = 0;
            foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                ResearchEditorExt ext = r.GetModExtension<ResearchEditorExt>();
                if (ext == null) continue;
                var falta = ext.unlocks.Where(u => !EQResearchUnlocks.Requires(u, r)).ToList();
                problemas += falta.Count;
                sb.AppendLine("  " + r.defName + " [" + r.LabelCap + "] aba " + r.tab?.defName + ", ramo " + EQResearch.Branch(r)?.defName
                    + ", pos " + r.ResearchViewX + "/" + r.ResearchViewY + ", pré: " + string.Join(",", (r.prerequisites ?? new List<ResearchProjectDef>()).Select(p => p.defName))
                    + " | desbloqueia " + ext.unlocks.Count + (falta.Count > 0 ? " | NÃO APLICADO: " + string.Join(", ", falta) : ""));
            }
            var proj = new ResearchProjectDef { defName = "EQ_R_TesteMemoria", label = "teste", baseCost = 100, techLevel = TechLevel.Industrial };
            var e = new ResearchEditorExt { unlocks = new List<string> { "Bed", "MealSimple", "Plant_Rice", "WoodPlankFloor", "Anesthetize", "EQ_NaoExiste" } };
            List<string> avisos = EQResearchUnlocks.Apply(proj, e);
            EQResearchUnlocks.ClearCaches();
            var unl = proj.UnlockedDefs.Select(d => d.defName).ToList();
            foreach (string u in e.unlocks.Take(5))
            {
                bool ok = EQResearchUnlocks.Requires(u, proj) && unl.Contains(u);
                if (!ok) problemas++;
                sb.AppendLine("  teste " + u + ": " + (ok ? "ok" : "FALHOU (exige=" + EQResearchUnlocks.Requires(u, proj) + ", na tela=" + unl.Contains(u) + ")"));
            }
            if (avisos.Count != 1) problemas++;
            sb.AppendLine("  avisos: " + string.Join("; ", avisos));
            sb.Append(problemas == 0 ? "  OK" : "  PROBLEMAS: " + problemas);
            return sb.ToString();
        }

        private static string Desc(string s)
        {
            s = s ?? "";
            int i = s.LastIndexOf("\n\nRamo: ", StringComparison.Ordinal);   // nota que o EQResearch acrescenta
            return Short(i >= 0 ? s.Substring(0, i) : s);
        }

        private static List<string> Safe(Func<List<string>> f)
        {
            try { return f(); } catch { return new List<string>(); }
        }
    }
}
