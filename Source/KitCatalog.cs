using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using EcosDaQueda;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Comando "kitcatalog [arquivo]": grava em JSON tudo o que o editor de kits (EcosDaQueda-Dev/kits) precisa
    /// saber do jogo carregado: roupas, armas e itens (nome PT-BR, camadas, partes do corpo, materiais aceitos, textura),
    /// materiais (cor, para pintar o ícone), xenotipos e as nossas unidades. O editor e o extrator de ícones rodam
    /// fora do jogo a partir deste arquivo.</summary>
    public static class KitCatalog
    {
        public static string Export(string path)
        {
            // Padrão: EcosDaQueda-Dev/kits/kitcatalog.json (a pasta do mod de debug é Mods/EcosDaQuedaDebug).
            string root = LoadedModManager.RunningMods.FirstOrDefault(m => m.PackageId == "ecosdaqueda.debug")?.RootDir;
            path = path ?? Path.GetFullPath(Path.Combine(root ?? GenFilePaths.SaveDataFolderPath, "..", "..", "EcosDaQueda-Dev", "kits", "kitcatalog.json"));
            var sb = new StringBuilder();
            sb.Append("{\n\"items\": [\n");
            int n = 0;
            var stuffCats = new HashSet<string>();
            foreach (ThingDef t in DefDatabase<ThingDef>.AllDefs.Where(Wanted).OrderBy(d => d.defName))
            {
                if (n++ > 0) sb.Append(",\n");
                string kind = t.IsApparel ? "apparel" : t.IsRangedWeapon ? "ranged" : t.IsMeleeWeapon ? "melee" : "item";
                var o = new List<string>
                {
                    P("def", t.defName), P("label", t.LabelCap.ToString()), P("kind", kind),
                    P("mod", t.modContentPack?.Name ?? "?"), P("pkg", t.modContentPack?.PackageId ?? "?"),
                    P("modDir", t.modContentPack?.RootDir ?? ""), P("tech", t.techLevel.ToString()),
                    P("value", t.BaseMarketValue), P("cat", t.FirstThingCategory?.LabelCap.ToString() ?? ""),
                    P("tex", t.graphicData?.texPath ?? ""), P("gclass", t.graphicData?.graphicClass?.Name ?? ""),
                    P("icon", t.uiIconPath ?? ""), P("color", Hex(t.graphicData?.color ?? Color.white)),
                    P("stack", t.stackLimit), P("desc", Short(t.description)),
                };
                if (t.MadeFromStuff && t.stuffCategories != null)
                {
                    o.Add(PL("stuffCats", t.stuffCategories.Select(c => c.defName)));
                    o.Add(P("defaultStuff", GenStuff.DefaultStuffFor(t)?.defName ?? ""));
                    foreach (var c in t.stuffCategories) stuffCats.Add(c.defName);
                }
                if (t.IsApparel)
                {
                    o.Add(PL("layers", t.apparel.layers.Select(l => l.defName)));
                    o.Add(PL("parts", t.apparel.bodyPartGroups.Select(g => g.defName)));
                    // partes que a peça ocupa de fato no corpo humano (o jogo usa isto para decidir o que não cabe junto)
                    o.Add(PL("hits", t.apparel.GetInterferingBodyPartGroups(BodyDefOf.Human).Select(g => g.defName)));
                    o.Add(PL("tags", t.apparel.tags ?? new List<string>()));
                    o.Add(P("gender", t.apparel.gender.ToString()));
                    o.Add(P("wornTex", t.apparel.wornGraphicPath ?? ""));
                }
                else if (t.IsWeapon)
                {
                    o.Add(PL("tags", t.weaponTags ?? new List<string>()));
                    var (ammo, mag) = CEAmmo(t);
                    if (ammo.Count > 0) { o.Add(PL("ammo", ammo)); o.Add(P("mag", mag)); }
                }
                sb.Append("{").Append(string.Join(",", o)).Append("}");
            }
            sb.Append("\n],\n\"stuff\": [\n");
            n = 0;
            foreach (ThingDef s in DefDatabase<ThingDef>.AllDefs.Where(d => d.IsStuff && d.stuffProps.categories.Any(c => stuffCats.Contains(c.defName))).OrderBy(d => d.defName))
            {
                if (n++ > 0) sb.Append(",\n");
                sb.Append("{").Append(string.Join(",", P("def", s.defName), P("label", s.LabelCap.ToString()),
                    P("color", Hex(s.stuffProps.color)), PL("cats", s.stuffProps.categories.Select(c => c.defName)),
                    P("mod", s.modContentPack?.Name ?? "?"), P("value", s.BaseMarketValue))).Append("}");
            }
            sb.Append("\n],\n\"xenotypes\": [\n");
            sb.Append(string.Join(",\n", DefDatabase<XenotypeDef>.AllDefs.OrderBy(x => x.defName).Select(x =>
                "{" + string.Join(",", P("def", x.defName), P("label", x.LabelCap.ToString()), P("mod", x.modContentPack?.Name ?? "?")) + "}")));
            sb.Append("\n],\n\"pawnkinds\": [\n");
            sb.Append(string.Join(",\n", DefDatabase<PawnKindDef>.AllDefs.Where(k => k.defName.StartsWith("EQ_") && k.RaceProps?.Humanlike == true)
                .OrderBy(k => k.defName).Select(k => "{" + string.Join(",", P("def", k.defName), P("label", k.LabelCap.ToString()),
                    P("faction", k.defaultFactionDef?.defName ?? ""), P("factionLabel", k.defaultFactionDef?.LabelCap.ToString() ?? ""),
                    P("points", k.combatPower)) + "}")));
            sb.Append("\n],\n\"factions\": [\n");
            sb.Append(string.Join(",\n", DefDatabase<FactionDef>.AllDefs.Where(f => f.defName.StartsWith("EQ_"))
                .OrderBy(f => f.defName).Select(f => "{" + string.Join(",", P("def", f.defName), P("label", f.LabelCap.ToString())) + "}")));
            sb.Append("\n],\n\"research\": ");
            ResearchCatalog.Append(sb);
            sb.Append("\n}\n");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            return "catálogo de kits: " + DefDatabase<ThingDef>.AllDefs.Count(Wanted) + " itens -> " + path;
        }

        /// <summary>Comando "kittest &lt;KitCollectionDef&gt; &lt;PawnKind&gt; [n]": liga a coleção à unidade (só nesta sessão), gera n
        /// pawns pelo gerador normal (passa pelo patch) e confere se cada um saiu com as peças da variação sorteada.</summary>
        public static string Test(string colName, string kindName, int n)
        {
            var col = DefDatabase<KitCollectionDef>.GetNamedSilentFail(colName);
            if (col == null) return "coleção não encontrada: " + colName;
            var kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(kindName);
            if (kind == null) return "PawnKind não encontrado: " + kindName;
            EQKits.Bind(kind, col);
            float oldChance = col.chance;
            col.chance = 1f;
            Faction fac = kind.defaultFactionDef != null ? Find.FactionManager.FirstFactionOfDef(kind.defaultFactionDef) : null;
            var sb = new StringBuilder();
            var usadas = new Dictionary<string, int>();
            int problemas = 0;
            try
            {
                for (int i = 0; i < n; i++)
                {
                    EQKits.LastApplied = null;
                    Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, fac, forceGenerateNewPawn: true));
                    KitVariation v = EQKits.LastApplied;
                    if (v == null) { problemas++; sb.AppendLine("  " + p.LabelShort + ": kit NÃO aplicado"); p.Destroy(); continue; }
                    usadas[v.label] = usadas.TryGetValue(v.label, out int c) ? c + 1 : 1;
                    var worn = p.apparel.WornApparel.Select(a => a.def.defName).ToList();
                    var faltam = v.apparel.Where(it => it.chance >= 1f && DefDatabase<ThingDef>.GetNamedSilentFail(it.thing) != null && !worn.Contains(it.thing)).Select(it => it.thing).ToList();
                    string arma = p.equipment.Primary?.def.defName ?? "-";
                    string armaKit = v.weapons.FirstOrDefault()?.thing ?? "-";
                    var inv = p.inventory.innerContainer.GroupBy(t => t.def.defName).Select(g => g.Key + " x" + g.Sum(t => t.stackCount));
                    bool ok = faltam.Count == 0 && arma == armaKit;
                    if (!ok) problemas++;
                    if (i < 4 || !ok)
                        sb.AppendLine("  " + (ok ? "" : "PROBLEMA ") + p.LabelShort + " [" + v.label + "] veste: " + string.Join(", ", worn)
                            + (faltam.Count > 0 ? " | FALTAM: " + string.Join(", ", faltam) : "") + " | arma: " + arma + (arma != armaKit ? " (kit: " + armaKit + ")" : "")
                            + " | inventário: " + string.Join(", ", inv));
                    p.Destroy();
                }
            }
            finally { col.chance = oldChance; }
            sb.AppendLine("  variações sorteadas: " + string.Join(", ", usadas.Select(kv => kv.Key + " " + kv.Value)));
            sb.Append(problemas == 0 ? "  OK" : "  PROBLEMAS: " + problemas);
            return sb.ToString();
        }

        /// <summary>Comando "roster &lt;FactionDef&gt;": mostra os grupos da facção (quem pode vir em cada um, com peso),
        /// líderes e membro básico, e gera uma vez cada unidade do editor (EQ_U_*) para conferir se veio com kit.</summary>
        public static string Roster(string facName, string applyRoster = null)
        {
            FactionDef fd = DefDatabase<FactionDef>.GetNamedSilentFail(facName);
            if (fd == null) return "facção não encontrada: " + facName;
            var sb = new StringBuilder();
            if (applyRoster != null)
            {
                var r = DefDatabase<UnitRosterDef>.GetNamedSilentFail(applyRoster);
                if (r == null) return "elenco não encontrado: " + applyRoster;
                sb.AppendLine("  aplicado " + applyRoster + ": " + EQUnits.Apply(r));
            }
            int problemas = 0;
            sb.AppendLine("  líderes: " + string.Join(", ", (fd.fixedLeaderKinds ?? new List<PawnKindDef>()).Select(k => k.defName)) + " | membro básico: " + fd.basicMemberKind?.defName);
            foreach (PawnGroupMaker m in fd.pawnGroupMakers ?? new List<PawnGroupMaker>())
            {
                string Fmt(List<PawnGenOption> l) => l == null ? "" : string.Join(", ", l.Select(o => o.kind.defName + " " + o.selectionWeight));
                sb.AppendLine("  " + m.kindDef.defName + ": " + Fmt(m.options)
                    + (m.traders.NullOrEmpty() ? "" : " | mercadores: " + Fmt(m.traders)) + (m.guards.NullOrEmpty() ? "" : " | guardas: " + Fmt(m.guards)));
            }
            Faction fac = Find.FactionManager.FirstFactionOfDef(fd);
            foreach (PawnKindDef k in DefDatabase<PawnKindDef>.AllDefs.Where(k => k.defName.StartsWith("EQ_U_") && k.defaultFactionDef == fd))
            {
                EQKits.LastApplied = null;
                Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(k, fac, forceGenerateNewPawn: true));
                bool temKit = !EQKits.For(k).NullOrEmpty();
                bool ok = !temKit || EQKits.LastApplied != null;
                if (!ok) problemas++;
                sb.AppendLine("  " + (ok ? "" : "PROBLEMA ") + k.defName + " (" + k.label + ", " + k.combatPower + " pts): " + p.genes?.Xenotype?.defName
                    + (temKit ? " | kit [" + (EQKits.LastApplied?.label ?? "NÃO aplicado") + "]" : " | sem kit") + " | veste: " + string.Join(", ", p.apparel.WornApparel.Select(a => a.def.defName))
                    + " | arma: " + (p.equipment.Primary?.def.defName ?? "-"));
                p.Destroy();
            }
            sb.Append(problemas == 0 ? "  OK" : "  PROBLEMAS: " + problemas);
            return sb.ToString();
        }

        /// <summary>Munições que a arma aceita (Combat Extended: CompProperties_AmmoUser.ammoSet.ammoTypes[].ammo) e o
        /// tamanho do carregador, por reflexão (o CE é opcional).</summary>
        private static (List<string>, int) CEAmmo(ThingDef t)
        {
            var list = new List<string>();
            int mag = 0;
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (CompProperties cp in t.comps ?? new List<CompProperties>())
            {
                if (cp.GetType().Name != "CompProperties_AmmoUser") continue;
                mag = (cp.GetType().GetField("magazineSize", F)?.GetValue(cp) as int?) ?? 0;
                object set = cp.GetType().GetField("ammoSet", F)?.GetValue(cp);
                if (set?.GetType().GetField("ammoTypes", F)?.GetValue(set) is System.Collections.IEnumerable links)
                    foreach (object link in links)
                        if (link?.GetType().GetField("ammo", F)?.GetValue(link) is ThingDef a) list.Add(a.defName);
            }
            return (list, mag);
        }

        private static bool Wanted(ThingDef t)
        {
            if (t.category != ThingCategory.Item || t.IsCorpse || t.IsBlueprint || t.IsFrame || t.isUnfinishedThing) return false;
            if (t.IsApparel) return t.apparel.layers != null && t.apparel.bodyPartGroups != null;
            if (t.IsWeapon) return !t.destroyOnDrop || t.weaponTags != null;
            return !t.IsStuff || t.IsMedicine || t.IsNutritionGivingIngestible || t.IsDrug || t.thingCategories != null;
        }

        internal static string Short(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\r", "").Replace("\n", " ");
            return s.Length > 240 ? s.Substring(0, 240) + "…" : s;
        }

        internal static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);

        internal static string P(string k, string v) => "\"" + k + "\":" + Str(v);
        internal static string P(string k, float v) => "\"" + k + "\":" + v.ToString("0.##", CultureInfo.InvariantCulture);
        internal static string P(string k, int v) => "\"" + k + "\":" + v.ToString(CultureInfo.InvariantCulture);
        internal static string PL(string k, IEnumerable<string> v) => "\"" + k + "\":[" + string.Join(",", v.Select(Str)) + "]";

        internal static string Str(string s)
        {
            var sb = new StringBuilder("\"");
            foreach (char c in s ?? "")
            {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                else sb.Append(c);
            }
            return sb.Append('"').ToString();
        }
    }
}
