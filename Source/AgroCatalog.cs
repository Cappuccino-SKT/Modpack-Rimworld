using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// Comando "agro": levanta tudo o que serve para cultivar (fora as plantas): canteiros, luz de cultivo, irrigação,
    /// solos, equipamentos e implantes que mudam plantio/colheita, mecanoides e drones agrícolas, receitas ligadas.
    /// Grava EcosDaQueda-Dev/tests/out/agro.tsv (tipo, def, rótulo, mod, motivo, pesquisa atual, categoria).
    /// </summary>
    public static class AgroCatalog
    {
        private static readonly Regex CompRe = new Regex("sprinkl|irrig|fertil|compost|plant|grow|harvest|crop|soil|farm|seed|sow|hive|apiar|bee|greenhouse|weed|pest", RegexOptions.IgnoreCase);
        private static readonly Regex StatRe = new Regex("plant|harvest|sow|grow|crop|farm", RegexOptions.IgnoreCase);

        private static bool PlantStat(StatDef s) => s != null && StatRe.IsMatch(s.defName);

        private static IEnumerable<string> StatHits(IEnumerable<StatModifier> mods) =>
            (mods ?? Enumerable.Empty<StatModifier>()).Where(m => PlantStat(m.stat)).Select(m => m.stat.defName + (m.value >= 0 ? "+" : "") + m.value.ToString("0.##"));

        public static string Export()
        {
            string root = LoadedModManager.RunningMods.FirstOrDefault(m => m.PackageId == "ecosdaqueda.debug")?.RootDir;
            string path = Path.GetFullPath(Path.Combine(root ?? GenFilePaths.SaveDataFolderPath, "..", "..", "EcosDaQueda-Dev", "tests", "out", "agro.tsv"));
            var rows = new List<string>();
            void Add(string kind, Def d, string why, IEnumerable<ResearchProjectDef> req, string cat)
            {
                rows.Add(string.Join("\t", kind, d.defName, d.LabelCap.ToString(), d.modContentPack?.Name ?? "?", why,
                    string.Join(",", (req ?? Enumerable.Empty<ResearchProjectDef>()).Where(r => r != null).Select(r => r.defName).Distinct()), cat ?? ""));
            }

            // hediffs que mexem com plantio (implantes, genes etc.) -> receitas que os instalam
            var plantHediffs = new Dictionary<HediffDef, string>();
            foreach (HediffDef h in DefDatabase<HediffDef>.AllDefsListForReading)
            {
                var hits = (h.stages ?? new List<HediffStage>()).SelectMany(s => StatHits(s.statOffsets).Concat(StatHits(s.statFactors))).Distinct().ToList();
                if (hits.Count > 0) plantHediffs[h] = string.Join(" ", hits);
            }

            foreach (ThingDef t in DefDatabase<ThingDef>.AllDefsListForReading.OrderBy(d => d.defName))
            {
                try
                {
                    if (t.plant != null || t.IsCorpse || t.isUnfinishedThing) continue;
                    var why = new List<string>();
                    if (t.thingClass != null && typeof(Building_PlantGrower).IsAssignableFrom(t.thingClass)) why.Add("canteiro");
                    else if (t.building?.sowTag != null) why.Add("canteiro(" + t.building.sowTag + ")");
                    if (t.fertility > 0f && t.category == ThingCategory.Building) why.Add("fertilidade " + t.fertility.ToString("0.##"));
                    var glow = t.GetCompProperties<CompProperties_Glower>();
                    if (glow != null && glow.overlightRadius > 0f) why.Add("luz de cultivo");
                    foreach (CompProperties cp in t.comps ?? new List<CompProperties>())
                    {
                        string n = (cp.compClass?.Name ?? "") + " " + cp.GetType().Name;
                        if (CompRe.IsMatch(n)) why.Add("comp " + (cp.compClass?.Name ?? cp.GetType().Name));
                    }
                    var st = StatHits(t.equippedStatOffsets).Concat(t.IsApparel ? StatHits(t.statBases?.Where(s => s.stat != StatDefOf.MarketValue)) : Enumerable.Empty<string>()).ToList();
                    if (st.Count > 0) why.Add("equipado: " + string.Join(" ", st));
                    if (t.race != null && t.race.mechEnabledWorkTypes != null && t.race.mechEnabledWorkTypes.Any(w => w == WorkTypeDefOf.Growing || w.defName == "PlantCutting"))
                        why.Add("mecanoide agrícola");
                    if (why.Count == 0) continue;
                    string kind = t.category == ThingCategory.Building ? "building" : t.race != null ? "pawn" : "item";
                    IEnumerable<ResearchProjectDef> req = t.researchPrerequisites;
                    if (kind != "building")
                        req = DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.products != null && r.products.Any(p => p.thingDef == t))
                            .SelectMany(r => (r.researchPrerequisites ?? new List<ResearchProjectDef>()).Append(r.researchPrerequisite));
                    Add(kind, t, string.Join("; ", why.Distinct()), req, t.designationCategory?.defName ?? t.FirstThingCategory?.defName);
                }
                catch (Exception ex) { rows.Add("erro\t" + t.defName + "\t" + ex.Message); }
            }
            foreach (TerrainDef t in DefDatabase<TerrainDef>.AllDefsListForReading.Where(x => x.designationCategory != null && x.fertility > 0f))
                Add("terrain", t, "solo fértil " + t.fertility.ToString("0.##"), t.researchPrerequisites, t.designationCategory.defName);
            foreach (RecipeDef r in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (r.addsHediff != null && plantHediffs.TryGetValue(r.addsHediff, out string w))
                    Add("recipe", r, "instala " + r.addsHediff.defName + ": " + w, (r.researchPrerequisites ?? new List<ResearchProjectDef>()).Append(r.researchPrerequisite), "cirurgia");
            }
            foreach (var kv in plantHediffs)
                rows.Add(string.Join("\t", "hediff", kv.Key.defName, kv.Key.LabelCap.ToString(), kv.Key.modContentPack?.Name ?? "?", kv.Value, "", ""));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, string.Join("\n", rows) + "\n", new UTF8Encoding(false));
            return "  agro: " + rows.Count + " linhas -> " + path;
        }
    
        private static readonly Regex AnimRe = new Regex("animal|pen\b|penmarker|egg|hatch|incubat|milk|shear|feed|nest|trough|hay|kibble|pasture|tame|train|breed|pet\b|leash|kennel|stable|livestock|ranch|barding|saddle|graz|herd|mount|ride|caravan|wool|fertil", RegexOptions.IgnoreCase);

        /// <summary>Comando "animais": tudo o que serve para criar, alimentar, treinar e equipar animais. Grava tests/out/animais.tsv.</summary>
        public static string Animals()
        {
            string root = LoadedModManager.RunningMods.FirstOrDefault(m => m.PackageId == "ecosdaqueda.debug")?.RootDir;
            string path = Path.GetFullPath(Path.Combine(root ?? GenFilePaths.SaveDataFolderPath, "..", "..", "EcosDaQueda-Dev", "tests", "out", "animais.tsv"));
            var rows = new List<string>();
            void Add(string kind, Def d, string why, IEnumerable<ResearchProjectDef> req, string cat)
            {
                rows.Add(string.Join("	", kind, d.defName, d.LabelCap.ToString(), d.modContentPack?.Name ?? "?", why,
                    string.Join(",", (req ?? Enumerable.Empty<ResearchProjectDef>()).Where(r => r != null).Select(r => r.defName).Distinct()), cat ?? ""));
            }
            IEnumerable<ResearchProjectDef> Rec(ThingDef t) => DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.products != null && r.products.Any(p => p.thingDef == t))
                .SelectMany(r => (r.researchPrerequisites ?? new List<ResearchProjectDef>()).Append(r.researchPrerequisite));
            foreach (ThingDef t in DefDatabase<ThingDef>.AllDefsListForReading.OrderBy(d => d.defName))
            {
                try
                {
                    if (t.plant != null || t.IsCorpse || t.isUnfinishedThing || t.race != null) continue;
                    var why = new List<string>();
                    if (t.IsBed && t.building != null && !t.building.bed_humanlike) why.Add("cama de animal");
                    foreach (CompProperties cp in t.comps ?? new List<CompProperties>())
                    {
                        string n = cp.compClass?.Name ?? cp.GetType().Name;
                        if (AnimRe.IsMatch(n) && !n.Contains("Fertilizer")) why.Add("comp " + n);
                    }
                    if (t.thingClass != null && AnimRe.IsMatch(t.thingClass.Name)) why.Add("classe " + t.thingClass.Name);
                    if (t.ingestible != null && (t.ingestible.foodType & FoodTypeFlags.Kibble) != 0 && t.ingestible.foodType == FoodTypeFlags.Kibble) why.Add("ração");
                    if (t.IsApparel && Regex.IsMatch(t.defName, "animal|pet|barding|saddle|horse|dog", RegexOptions.IgnoreCase)) why.Add("equipamento de animal");
                    if (t.category == ThingCategory.Building && t.building != null && t.building.isMealSource == false && Regex.IsMatch(t.defName, "trough|feeder|hopper|pen|nest|hatch|incub|kennel|stable|petbed|animal", RegexOptions.IgnoreCase)) why.Add("nome");
                    if (why.Count == 0) continue;
                    string kind = t.category == ThingCategory.Building ? "building" : "item";
                    Add(kind, t, string.Join("; ", why.Distinct()), kind == "building" ? t.researchPrerequisites : Rec(t), t.designationCategory?.defName ?? t.FirstThingCategory?.defName);
                }
                catch (Exception ex) { rows.Add("erro	" + t.defName + "	" + ex.Message); }
            }
            // receitas feitas em animais (cirurgias, administrar) e receitas que fazem ração
            var animalRaces = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.race != null && t.race.Animal));
            foreach (RecipeDef r in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                var users = r.AllRecipeUsers.Where(animalRaces.Contains).ToList();
                if (users.Count > 0 && !r.AllRecipeUsers.Any(u => u.race != null && u.race.Humanlike))
                    Add("recipe", r, "em animais (" + users.Count + ")", (r.researchPrerequisites ?? new List<ResearchProjectDef>()).Append(r.researchPrerequisite), r.IsSurgery ? "cirurgia" : "receita");
            }
            // projetos de pesquisa que já desbloqueiam algo da lista
            var defs = new HashSet<string>(rows.Select(l => l.Split('	')[1]));
            foreach (ResearchProjectDef p in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                var hit = p.UnlockedDefs.Where(d => defs.Contains(d.defName)).Select(d => d.defName).ToList();
                if (hit.Count > 0) rows.Add(string.Join("	", "project", p.defName, p.LabelCap.ToString(), p.modContentPack?.Name ?? "?", string.Join(",", hit), p.techLevel.ToString(), p.tab?.defName));
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, string.Join("\n", rows) + "\n", new UTF8Encoding(false));
            return "  animais: " + rows.Count + " linhas -> " + path;
        }
    
        /// <summary>Comando "inicio": para cada cenário do mod, as pesquisas que já começam concluídas (tags da facção do
        /// jogador, partes do cenário) e as dadas por memes. Grava tests/out/inicio.tsv (cenário, origem, projeto).</summary>
        public static string Start()
        {
            string root = LoadedModManager.RunningMods.FirstOrDefault(m => m.PackageId == "ecosdaqueda.debug")?.RootDir;
            string path = Path.GetFullPath(Path.Combine(root ?? GenFilePaths.SaveDataFolderPath, "..", "..", "EcosDaQueda-Dev", "tests", "out", "inicio.tsv"));
            var rows = new List<string>();
            var resumo = new StringBuilder();
            foreach (ScenarioDef sc in DefDatabase<ScenarioDef>.AllDefsListForReading.Where(d => d.defName.StartsWith("EQ_")))
            {
                FactionDef fac = (FactionDef)HarmonyLib.AccessTools.Field(typeof(ScenPart_PlayerFaction), "factionDef").GetValue(sc.scenario.AllParts.OfType<ScenPart_PlayerFaction>().FirstOrDefault() ?? (object)new ScenPart_PlayerFaction());
                var got = new HashSet<ResearchProjectDef>();
                if (fac?.startingResearchTags != null)
                    foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
                        if (r.tags != null && r.tags.Any(t => fac.startingResearchTags.Contains(t))) { got.Add(r); rows.Add(sc.defName + "\tfacção " + fac.defName + "\t" + r.defName + "\t" + r.LabelCap); }
                foreach (ScenPart_StartingResearch sp in sc.scenario.AllParts.OfType<ScenPart_StartingResearch>())
                {
                    var proj = (ResearchProjectDef)HarmonyLib.AccessTools.Field(typeof(ScenPart_StartingResearch), "project").GetValue(sp);
                    if (proj != null && got.Add(proj)) rows.Add(sc.defName + "\tcenário\t" + proj.defName + "\t" + proj.LabelCap);
                }
                resumo.AppendLine("  " + sc.defName + " (" + fac?.defName + "): " + got.Count + " pesquisas no início");
            }
            foreach (MemeDef m in DefDatabase<MemeDef>.AllDefsListForReading.Where(m => !m.startingResearchProjects.NullOrEmpty()))
                foreach (ResearchProjectDef r in m.startingResearchProjects)
                    rows.Add("(meme)\tmeme " + m.defName + "\t" + r.defName + "\t" + r.LabelCap);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, string.Join("\n", rows) + "\n", new UTF8Encoding(false));
            // jogo atual: pesquisas concluídas, kit livre disponível, Fundamentos travado
            if (Current.Game != null)
            {
                var feitas = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(r => r.IsFinished).Select(r => r.defName).ToList();
                resumo.AppendLine("  concluídas agora: " + feitas.Count + (feitas.Count > 0 ? " (" + string.Join(",", feitas) + ")" : ""));
                string kitFile = Path.GetFullPath(Path.Combine(root ?? "", "..", "..", "EcosDaQueda-Dev", "kits", "pesquisa", "Fundamentos.json"));
                var kit = new List<string>();
                if (File.Exists(kitFile))
                {
                    string txt = File.ReadAllText(kitFile);
                    Match km = Regex.Match(txt, @"""kit"":\s*\[(.*?)\]", RegexOptions.Singleline);
                    if (km.Success) kit = Regex.Matches(km.Groups[1].Value, "\"([^\"]+)\"").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
                }
                int kitOk = 0;
                foreach (string k in kit)
                {
                    bool? ok = Available(k);
                    if (ok == true) kitOk++;
                    else
                    {
                        resumo.AppendLine("  KIT INDISPONÍVEL: " + k + (ok == null ? " (não existe)" : ""));
                        ThingDef kt = DefDatabase<ThingDef>.GetNamedSilentFail(k);
                        if (kt != null)
                            foreach (RecipeDef r in DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.products != null && r.products.Any(p => p.thingDef == kt)))
                                resumo.AppendLine("    receita " + r.defName + " pré=" + string.Join(",", new[] { r.researchPrerequisite }.Concat(r.researchPrerequisites ?? new List<ResearchProjectDef>()).Where(x => x != null).Select(x => x.defName))
                                    + " memes=" + string.Join(",", (r.memePrerequisitesAny ?? new List<MemeDef>()).Select(m => m.defName)) + " users=" + string.Join(",", r.AllRecipeUsers.Select(u => u.defName)));
                    }
                }
                resumo.AppendLine("  kit livre: " + kitOk + "/" + kit.Count);
                int trav = 0, livre = 0;
                foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(r => r.defName.StartsWith("EQ_R_Fundamentos_")))
                    foreach (string u in r.GetModExtension<ResearchEditorExt>()?.unlocks ?? new List<string>())
                    {
                        if (Available(u) == true) { livre++; resumo.AppendLine("  FUNDAMENTOS LIVRE: " + u); } else trav++;
                    }
                resumo.AppendLine("  Fundamentos travados: " + trav + ", livres por engano: " + livre);
            }
            return resumo.ToString() + "  inicio: " + rows.Count + " linhas -> " + path;
        }

        /// <summary>Disponível agora? Construção/piso: pesquisa concluída; item: alguma receita disponível; null se não existe.</summary>
        private static bool? Available(string name)
        {
            ThingDef t = DefDatabase<ThingDef>.GetNamedSilentFail(name);
            if (t != null)
            {
                if (t.plant != null) return t.plant.sowResearchPrerequisites.NullOrEmpty() || t.plant.sowResearchPrerequisites.All(r => r.IsFinished);
                if (t.designationCategory != null) return t.IsResearchFinished;
                var recs = DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.products != null && r.products.Any(p => p.thingDef == t)).ToList();
                return recs.Count == 0 || recs.Any(r => r.AvailableNow);
            }
            TerrainDef ter = DefDatabase<TerrainDef>.GetNamedSilentFail(name);
            if (ter != null) return ter.IsResearchFinished;
            return null;
        }
    }
}
