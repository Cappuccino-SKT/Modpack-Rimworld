using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using HarmonyLib;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// Exporta, em inglês, todo texto traduzível (DefInjected) dos mods ativos, no formato de arquivo de tradução
    /// do jogo. Usa a mesma travessia do limpador de traduções do jogo (DefInjectionUtility).
    /// Saída: &lt;pasta&gt;/&lt;packageId&gt;/DefInjected/&lt;TipoDeDef&gt;/&lt;arquivo&gt;.xml
    /// </summary>
    public static class TransExport
    {
        private static readonly FieldInfo FileNameField = AccessTools.Field(typeof(Def), "fileName");

        public static string Export(string outDir, string filter)
        {
            if (!Path.IsPathRooted(outDir)) outDir = Path.Combine(Environment.CurrentDirectory, outDir); // relativo à pasta do jogo
            var only = filter.NullOrEmpty() || filter == "all" ? null : new HashSet<string>(filter.ToLowerInvariant().Split(','));
            var report = new StringBuilder();
            foreach (ModContentPack pack in LoadedModManager.RunningMods)
            {
                if (pack.IsOfficialMod) continue;
                string id = pack.PackageId.ToLowerInvariant();
                if (id.StartsWith("ecosdaqueda")) continue;
                if (only != null && !only.Contains(id)) continue;
                // arquivo -> lista de (caminho, valor)
                var files = new Dictionary<string, List<string>>();
                int count = 0;
                foreach (Type defType in GenDefDatabase.AllDefTypesWithDatabases())
                {
                    foreach (Def def in GenDefDatabase.GetAllDefsInDatabaseForDef(defType).ToList())
                    {
                        if (def.modContentPack != pack || def.generated) continue;
                        var seen = new HashSet<string>();
                        var lines = new List<string>();
                        DefInjectionUtility.PossibleDefInjectionTraverser act = (suggestedPath, normalizedPath, isCollection, currentValue, currentValueCollection, translationAllowed, fullListTranslationAllowed, fieldInfo, d) =>
                        {
                            if (!translationAllowed || !seen.Add(suggestedPath)) return;
                            if (suggestedPath.IndexOfAny(new[] { '<', '>' }) >= 0) return; // campo gerado pelo compilador (k__BackingField): não dá para traduzir
                            if (!isCollection)
                            {
                                if (!DefInjectionUtility.ShouldCheckMissingInjection(currentValue, fieldInfo, d)) return;
                                lines.Add("  <" + suggestedPath + ">" + Esc(currentValue) + "</" + suggestedPath + ">");
                            }
                            else
                            {
                                var vals = currentValueCollection?.ToList();
                                if (vals == null || vals.Count == 0 || !fullListTranslationAllowed) return;
                                if (!vals.Any(v => DefInjectionUtility.ShouldCheckMissingInjection(v, fieldInfo, d))) return;
                                var sb = new StringBuilder("  <" + suggestedPath + ">\n");
                                foreach (string v in vals) sb.Append("    <li>" + Esc(v) + "</li>\n");
                                sb.Append("  </" + suggestedPath + ">");
                                lines.Add(sb.ToString());
                            }
                        };
                        try { DefInjectionUtilityPublic.ForDef(def, act); }
                        catch (Exception ex) { report.AppendLine("  !! " + def.defName + ": " + ex.Message); continue; }
                        if (lines.Count == 0) continue;
                        string file = FileNameField?.GetValue(def) as string;
                        if (file.NullOrEmpty()) file = defType.Name + ".xml";
                        // o carregador de idioma acha a pasta pelo nome do tipo; tipos de mods (fora de Verse/RimWorld) precisam do nome completo
                        string ns = defType.Namespace ?? "";
                        string typeDir = ns == "Verse" || ns == "RimWorld" || ns.StartsWith("RimWorld.") || ns.StartsWith("Verse.") ? defType.Name : defType.FullName;
                        string key = typeDir + "/" + Path.GetFileName(file);
                        if (!files.TryGetValue(key, out var list)) files[key] = list = new List<string>();
                        list.Add("");
                        list.Add("  <!-- " + def.defName + " -->");
                        list.AddRange(lines);
                        count += lines.Count;
                    }
                }
                if (count == 0) continue;
                foreach (var kv in files)
                {
                    string path = Path.Combine(outDir, id, "DefInjected", kv.Key.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<LanguageData>\n" + string.Join("\n", kv.Value) + "\n\n</LanguageData>\n", new UTF8Encoding(false));
                }
                report.AppendLine(id + "\t" + pack.RootDir + "\t" + count + " textos em " + files.Count + " arquivos");
            }
            return report.ToString();
        }

        /// <summary>Mostra label e descrição de um def (para conferir se a tradução foi aplicada).</summary>
        public static string DefText(string typeName, string defName)
        {
            // Aceita nome curto ou completo; com nomes repetidos (ex.: AbilityDef do jogo e do VEF), usa o tipo que tem o def.
            var types = GenDefDatabase.AllDefTypesWithDatabases().Where(x => x.Name == typeName || x.FullName == typeName).ToList();
            if (types.Count == 0) return "tipo de def não encontrado: " + typeName;
            Type t = types[0];
            Def d = null;
            foreach (var cand in types)
            {
                d = GenDefDatabase.GetDefSilentFail(cand, defName);
                if (d != null) { t = cand; break; }
            }
            if (d == null) return "def não encontrado: " + defName;
            string desc = d.description ?? "";
            string inj = "";
            var pkg = LanguageDatabase.activeLanguage?.defInjections.FirstOrDefault(p => p.defType == t);
            if (pkg != null && pkg.injections.TryGetValue(defName + ".label", out var di))
                inj = " [injeção: " + (di.injected ? "aplicada" : "NÃO aplicada") + ", '" + di.injection + "', de " + di.fileSource + "]";
            else
                inj = " [sem injeção carregada]";
            return d.label + " | " + (desc.Length > 80 ? desc.Substring(0, 80) + "..." : desc) + inj + " | mod: " + d.modContentPack?.PackageId;
        }

        /// <summary>Rótulos de genes gerados por modelos (defName contém o filtro), para ver se o modelo traduzido chegou neles.</summary>
        public static string GeneratedGenes(string filter)
        {
            var list = DefDatabase<GeneDef>.AllDefs.Where(g => g.generated && g.defName.IndexOf(filter ?? "", StringComparison.OrdinalIgnoreCase) >= 0).Take(5);
            return string.Join("\n", list.Select(g => g.defName + " = " + g.label));
        }

        /// <summary>Erros de carga/injeção da tradução ativa (filtra por texto, ex.: um packageId ou nome de arquivo).</summary>
        public static string TranslationErrors(string filter)
        {
            var lang = LanguageDatabase.activeLanguage;
            var sb = new StringBuilder();
            int n = 0;
            foreach (var pkg in lang.defInjections)
            {
                foreach (string e in pkg.loadErrors)
                    if (filter.NullOrEmpty() || e.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                        if (n++ < 40) sb.AppendLine("  load: " + e);
                foreach (var inj in pkg.injections.Values)
                    if (!inj.injected && (filter.NullOrEmpty() || (inj.fileSource ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || inj.path.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
                        if (n++ < 40) sb.AppendLine("  não injetado: " + pkg.defType.Name + " " + inj.path + " (" + inj.fileSource + ")");
            }
            foreach (string e in lang.loadErrors)
                if (filter.NullOrEmpty() || e.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    if (n++ < 60) sb.AppendLine("  lang: " + e);
            return n + " problemas\n" + sb;
        }

        private static string Esc(string s) => SecurityElement.Escape(s).Replace("\r", "").Replace("\n", "\\n");
    }

    /// <summary>Acesso à travessia privada de um def só (ForEachPossibleDefInjectionInDef).</summary>
    internal static class DefInjectionUtilityPublic
    {
        private static readonly MethodInfo M = AccessTools.Method(typeof(DefInjectionUtility), "ForEachPossibleDefInjectionInDef");
        public static void ForDef(Def def, DefInjectionUtility.PossibleDefInjectionTraverser act) => M.Invoke(null, new object[] { def, act });
    }
}
