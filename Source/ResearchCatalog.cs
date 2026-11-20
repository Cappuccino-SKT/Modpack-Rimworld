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
                    P("eqHidden", EQHiddenResearch.Hidden.Contains(r) ? 1 : 0),
                    P("eqAbsorbedBy", EQAbsorbedResearch.By.TryGetValue(r, out ResearchProjectDef absBy) ? absBy.defName : ""),
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
            // torreta: munição do canhão dela (Combat Extended), para a árvore Defesa liberar a munição junto com a torreta
            string TurretAmmo(ThingDef t)
            {
                ThingDef gun = t.building?.turretGunDef;
                if (gun == null) return null;
                var ammo = KitCatalog.CEAmmo(gun).Item1;
                return ammo.Count > 0 ? PL("ammo", ammo) : null;
            }
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
                            "Planta", t.plant.sowResearchPrerequisites ?? new List<ResearchProjectDef>(), t.techLevel, null, PlantInfo(t)));
                    else if (t.designationCategory != null)
                        rows.Add(Row(t, "building", t.graphicData?.texPath, t.uiIconPath, t.graphicData?.graphicClass?.Name, t.graphicData?.color ?? UnityEngine.Color.white,
                            t.designationCategory.LabelCap.ToString(), t.researchPrerequisites ?? new List<ResearchProjectDef>(), t.techLevel, null,
                            TurretAmmo(t)));
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

        /// <summary>Dados de cultivo de uma planta semeável (para classificar no editor de pesquisa).</summary>
        private static string PlantInfo(ThingDef t)
        {
            PlantProperties pp = t.plant;
            ThingDef h = pp.harvestedThingDef;
            var o = new List<string>
            {
                P("harvest", h?.defName ?? ""), P("harvestLabel", h?.LabelCap.ToString() ?? ""), P("yield", pp.harvestYield),
                P("days", pp.growDays), P("tree", pp.IsTree ? 1 : 0), P("purpose", pp.purpose.ToString()),
                P("fert", pp.fertilityMin), P("skill", pp.sowMinSkill), P("glow", pp.growMinGlow), P("cave", pp.cavePlant ? 1 : 0),
                PL("sowTags", pp.sowTags ?? new List<string>()), P("value", h?.BaseMarketValue ?? 0f),
            };
            if (h != null)
            {
                o.Add(PL("hcats", (h.thingCategories ?? new List<ThingCategoryDef>()).Select(c => c.defName)));
                o.Add(PL("stuff", (h.stuffProps?.categories ?? new List<StuffCategoryDef>()).Select(c => c.defName)));
                o.Add(P("food", h.ingestible != null ? h.ingestible.foodType.ToString() : ""));
                o.Add(P("pref", h.ingestible != null ? h.ingestible.preferability.ToString() : ""));
                o.Add(P("drug", h.IsDrug ? 1 : 0));
                o.Add(P("medicine", h.IsMedicine ? 1 : 0));
                ThingDefCountClass bp = h.butcherProducts?.FirstOrDefault();
                if (bp != null && bp.thingDef == t.blueprintDef) o.Add(P("seedYield", bp.count));
                o.Add(P("ingestible", h.IsIngestible ? 1 : 0));
            }
            // semente do SeedsPlease (a planta só fica disponível com a semente no mapa: blueprintDef = semente)
            if (t.blueprintDef != null && t.blueprintDef.modExtensions?.Any(e => e.GetType().Name == "Seed") == true)
            {
                o.Add(P("seed", t.blueprintDef.defName));
                o.Add(P("seedLabel", t.blueprintDef.LabelCap.ToString()));
            }
            return "\"plant\":{" + string.Join(",", o) + "}";
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
            // sementes (SeedsPlease): cada planta com semente tem receita própria que exige o projeto e aparece na tela
            RecipeDef gen = DefDatabase<RecipeDef>.GetNamedSilentFail("ExtractSeeds");
            if (gen != null)
            {
                int ext = 0, prop = 0, semFalha = 0;
                foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
                {
                    ResearchEditorExt x = r.GetModExtension<ResearchEditorExt>();
                    if (x == null || !x.seeds) continue;
                    foreach (string u in x.unlocks)
                    {
                        ThingDef plant = DefDatabase<ThingDef>.GetNamedSilentFail(u);
                        ThingDef seed = plant?.blueprintDef;
                        if (plant?.plant == null || !EQSeedRecipes.IsSeed(seed)) continue;
                        RecipeDef re = DefDatabase<RecipeDef>.GetNamedSilentFail("EQ_ExtrairSemente_" + u);
                        RecipeDef rp = DefDatabase<RecipeDef>.GetNamedSilentFail("EQ_PropagarSemente_" + u);
                        RecipeDef rec = re ?? rp;
                        string erro = null;
                        if (rec == null) erro = "sem receita";
                        else if (rec.researchPrerequisites?.Contains(r) != true) erro = "receita não exige " + r.defName;
                        else if (!r.UnlockedDefs.Contains(seed)) erro = "semente não aparece na tela da pesquisa";
                        else if (rec.AllRecipeUsers.Count() == 0) erro = "receita sem bancada";
                        else if (re != null && gen.fixedIngredientFilter.Allows(plant.plant.harvestedThingDef)) erro = "colheita ainda na receita geral";
                        if (erro != null) { semFalha++; problemas++; sb.AppendLine("  semente " + u + ": " + erro); }
                        else if (re != null) ext++; else prop++;
                    }
                }
                sb.AppendLine("  sementes: " + ext + " por extração, " + prop + " por propagação, " + semFalha + " com problema; receitas criadas " + EQSeedRecipes.Created.Count);
                RecipeDef amostra = EQSeedRecipes.Created.FirstOrDefault();
                if (amostra != null) sb.AppendLine("  ex.: " + amostra.defName + " [" + amostra.label + "] " + amostra.ingredients[0].Summary + " -> "
                    + amostra.products[0].Label + " em " + string.Join(",", amostra.AllRecipeUsers.Select(t => t.defName)));
            }
            // pesquisas originais vazias: escondidas e fora dos pré-requisitos de quem continua visível
            var hid = EQHiddenResearch.Hidden;
            foreach (ResearchProjectDef h in hid)
                if (h.UnlockedDefs.Count > 0) { problemas++; sb.AppendLine("  escondida mas libera algo: " + h.defName); }
            foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(r => !hid.Contains(r)))
            {
                var ruim = (r.prerequisites ?? new List<ResearchProjectDef>()).Concat(r.hiddenPrerequisites ?? new List<ResearchProjectDef>()).Where(hid.Contains).ToList();
                if (ruim.Count > 0) { problemas++; sb.AppendLine("  " + r.defName + " ainda exige escondida: " + string.Join(",", ruim.Select(x => x.defName))); }
            }
            var win = new MainTabWindow_Research();
            int naTela = win.VisibleResearchProjects.Count(hid.Contains);
            if (naTela > 0) { problemas++; sb.AppendLine("  escondidas ainda na tela: " + naTela); }
            sb.AppendLine("  escondidas (" + hid.Count + "): " + string.Join(", ", hid.Select(h => h.defName)));
            var yart = HarmonyLib.AccessTools.Method(HarmonyLib.AccessTools.TypeByName("YART.Compat.WorldTechLevelCompat"), "ShouldShow");
            if (yart != null)
            {
                int yartMostra = hid.Count(h => (bool)yart.Invoke(null, new object[] { h }));
                if (yartMostra > 0) problemas++;
                sb.AppendLine("  YART mostra escondidas: " + yartMostra);
            }
            // coerência planta x produto: item liberado por uma árvore cujo ingrediente vem de planta que a cadeia dele não libera
            var sowBy = new Dictionary<ThingDef, List<ThingDef>>();
            foreach (ThingDef pl in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.plant != null && x.plant.Sowable && x.plant.harvestedThingDef != null))
            {
                if (!sowBy.TryGetValue(pl.plant.harvestedThingDef, out var l)) sowBy[pl.plant.harvestedThingDef] = l = new List<ThingDef>();
                l.Add(pl);
            }
            int incoer = 0;
            foreach (ResearchProjectDef pj in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(x => x.HasModExtension<ResearchEditorExt>()))
            {
                var anc = new HashSet<ResearchProjectDef>();
                var st = new Stack<ResearchProjectDef>(); st.Push(pj);
                while (st.Count > 0) { var q = st.Pop(); if (!anc.Add(q)) continue; foreach (var z in q.prerequisites ?? new List<ResearchProjectDef>()) st.Push(z); }
                foreach (string u in pj.GetModExtension<ResearchEditorExt>().unlocks)
                {
                    ThingDef it = DefDatabase<ThingDef>.GetNamedSilentFail(u);
                    if (it == null || it.plant != null || it.designationCategory != null) continue;
                    var falhas = new List<string>();
                    List<RecipeDef> recsIt = EQResearchUnlocks.RecipesFor(it);
                    int recOk = 0;
                    foreach (RecipeDef rec in recsIt)
                    {
                        int antes = falhas.Count;
                        foreach (IngredientCount ing in rec.ingredients ?? new List<IngredientCount>())
                        {
                            var allowed = ing.filter?.AllowedThingDefs?.ToList() ?? new List<ThingDef>();
                            if (allowed.Count == 0 || allowed.Count > 3) continue;   // ingrediente genérico (qualquer carne, qualquer vegetal)
                            // livre: não vem de planta, vem de árvore (lenha nativa), ou a planta não exige nada / exige só a cadeia do item
                            bool algumLivre = allowed.Any(a => !sowBy.ContainsKey(a) || sowBy[a].Any(pl => pl.plant.IsTree
                                || pl.plant.sowResearchPrerequisites.NullOrEmpty() || pl.plant.sowResearchPrerequisites.All(anc.Contains)));
                            if (algumLivre) continue;
                            falhas.Add(string.Join("/", allowed.Select(a => a.defName))
                                + " (planta em " + string.Join(",", allowed.SelectMany(a => sowBy[a]).SelectMany(pl => pl.plant.sowResearchPrerequisites).Distinct().Select(x => x.defName)) + ")");
                        }
                        if (falhas.Count == antes) recOk++;
                    }
                    // basta uma receita do item funcionar com o que a cadeia libera
                    if (recsIt.Count > 0 && recOk == 0 && falhas.Count > 0)
                    {
                        incoer++;
                        if (incoer <= 30) sb.AppendLine("  coerência: " + pj.defName + " libera " + u + ", que usa " + string.Join("; ", falhas.Distinct()));
                    }
                }
            }
            sb.AppendLine("  coerência planta x produto: " + incoer + " caso(s)");
            // pesquisas absorvidas: escondidas, ninguém mais exige a original
            var abs = EQAbsorbedResearch.By;
            int exigeAbs = 0;
            foreach (ThingDef t in DefDatabase<ThingDef>.AllDefsListForReading)
                if ((t.researchPrerequisites?.Any(abs.ContainsKey) ?? false) || (t.plant?.sowResearchPrerequisites?.Any(abs.ContainsKey) ?? false)) { exigeAbs++; if (exigeAbs <= 5) sb.AppendLine("  " + t.defName + " ainda exige absorvida"); }
            foreach (TerrainDef t in DefDatabase<TerrainDef>.AllDefsListForReading)
                if (t.researchPrerequisites?.Any(abs.ContainsKey) ?? false) { exigeAbs++; if (exigeAbs <= 5) sb.AppendLine("  " + t.defName + " ainda exige absorvida"); }
            foreach (RecipeDef rec in DefDatabase<RecipeDef>.AllDefsListForReading)
                if ((rec.researchPrerequisite != null && abs.ContainsKey(rec.researchPrerequisite)) || (rec.researchPrerequisites?.Any(abs.ContainsKey) ?? false)) { exigeAbs++; if (exigeAbs <= 5) sb.AppendLine("  " + rec.defName + " ainda exige absorvida"); }
            foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(r => !abs.ContainsKey(r)))
                if ((r.prerequisites ?? new List<ResearchProjectDef>()).Concat(r.hiddenPrerequisites ?? new List<ResearchProjectDef>()).Any(abs.ContainsKey)) { exigeAbs++; sb.AppendLine("  " + r.defName + " ainda exige absorvida"); }
            int absVisivel = abs.Keys.Count(o => !hid.Contains(o));
            if (exigeAbs + absVisivel > 0) problemas++;
            sb.AppendLine("  absorvidas: " + abs.Count + ", ainda exigidas " + exigeAbs + ", visíveis " + absVisivel);
            foreach (var grp in abs.GroupBy(kv => kv.Value))
                sb.AppendLine("    " + grp.Key.defName + " (" + grp.Key.UnlockedDefs.Count + " desbloqueios) <- " + string.Join(", ", grp.Select(kv => kv.Key.defName)));
            var proj = new ResearchProjectDef { defName = "EQ_R_TesteMemoria", label = "teste", baseCost = 100, techLevel = TechLevel.Industrial };
            var e = new ResearchEditorExt { unlocks = new List<string> { "Bed", "MealSimple", "Plant_Rice", "WoodPlankFloor", "Anesthetize", "EQ_NaoExiste" } };
            // o teste mexe em defs de verdade (cama, refeição simples, arroz...): guarda e restaura no fim, senão roteiros
            // seguintes no mesmo lote veem esses itens travados pelo projeto de teste
            var tPre = DefDatabase<ThingDef>.AllDefsListForReading.ToDictionary(t => t, t => t.researchPrerequisites);
            var tSow = DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.plant != null).ToDictionary(t => t, t => t.plant.sowResearchPrerequisites);
            var terPre = DefDatabase<TerrainDef>.AllDefsListForReading.ToDictionary(t => t, t => t.researchPrerequisites);
            var rPre = DefDatabase<RecipeDef>.AllDefsListForReading.ToDictionary(r => r, r => (r.researchPrerequisite, r.researchPrerequisites));
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
            foreach (var kv in tPre) kv.Key.researchPrerequisites = kv.Value;
            foreach (var kv in tSow) kv.Key.plant.sowResearchPrerequisites = kv.Value;
            foreach (var kv in terPre) kv.Key.researchPrerequisites = kv.Value;
            foreach (var kv in rPre) { kv.Key.researchPrerequisite = kv.Value.Item1; kv.Key.researchPrerequisites = kv.Value.Item2; }
            EQResearchUnlocks.ClearCaches();
            bool restaurado = !EQResearchUnlocks.Requires("MealSimple", proj) && !EQResearchUnlocks.Requires("Bed", proj);
            if (!restaurado) problemas++;
            sb.AppendLine("  teste restaurado: " + (restaurado ? "ok" : "FALHOU"));
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
