using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Ramo da árvore de pesquisa (Etapa 3). Cada projeto cai em um ramo: lista explícita (projects) de qualquer ramo,
    /// depois mods (packageId, menos exceptProjects), depois techLevels, senão o ramo free (bancada).
    /// </summary>
    public class ResearchBranchDef : Def
    {
        public ResearchTabDef tab;
        public FactionDef faction;
        /// <summary>Nome com preposição e artigo ("do Silêncio", "da Hélice").</summary>
        public string labelOf;
        public bool free;
        public int order;
        public List<string> projects = new List<string>();
        public List<string> mods = new List<string>();
        public List<string> exceptProjects = new List<string>();
        public List<TechLevel> techLevels = new List<TechLevel>();

        // ---- Bloco 3b: licenças (só ramos com faction) ----
        /// <summary>Cargo mínimo (índice, 0 = primeiro) que licencia cada nível tecnológico. Sem entrada: ver EQLicenses.DefaultRank.</summary>
        public List<LicenseRankByTech> licenseRanks = new List<LicenseRankByTech>();
        /// <summary>Exceções por projeto.</summary>
        public List<LicenseRankOverride> rankOverrides = new List<LicenseRankOverride>();
        /// <summary>Compra com favor: 1 de favor a cada costPerFavor pontos de pesquisa (mínimo minFavor), mais favorPerRankAbove por cargo acima do atual.</summary>
        public float costPerFavor = 400f;
        public int minFavor = 2;
        public int maxFavor = 24;
        public int favorPerRankAbove = 2;
        /// <summary>Projetos cuja pirataria já é tratada noutro sistema (androides, drones, blindados).</summary>
        public List<string> piracyExempt = new List<string>();
        public int piracyGoodwill = -4;
        public float piracyCooldownHours = 24f;
        /// <summary>{FACTION_name}, {ITEM}, {PROJECT}, {TITLE}.</summary>
        public string piracyText;
    }

    public class LicenseRankByTech
    {
        public TechLevel techLevel;
        public int rank;
    }

    public class LicenseRankOverride
    {
        public string project;
        public int rank;
    }

    public static class EQResearch
    {
        public static readonly Dictionary<ResearchProjectDef, ResearchBranchDef> BranchOf = new Dictionary<ResearchProjectDef, ResearchBranchDef>();
        public static ResearchBranchDef FreeBranch;

        private static readonly AccessTools.FieldRef<ResearchProjectDef, float> fX = AccessTools.FieldRefAccess<ResearchProjectDef, float>("x");
        private static readonly AccessTools.FieldRef<ResearchProjectDef, float> fY = AccessTools.FieldRefAccess<ResearchProjectDef, float>("y");

        private const int MaxRows = 14;
        private const float RowStep = 0.75f;

        public static void Apply()
        {
            List<ResearchBranchDef> branches = DefDatabase<ResearchBranchDef>.AllDefsListForReading.OrderBy(b => b.order).ToList();
            FreeBranch = branches.FirstOrDefault(b => b.free);
            if (FreeBranch == null)
                return;
            var explicitMap = new Dictionary<string, ResearchBranchDef>();
            foreach (ResearchBranchDef b in branches)
                foreach (string p in b.projects)
                    explicitMap[p] = b;

            foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                if (!explicitMap.TryGetValue(r.defName, out ResearchBranchDef b))
                {
                    string pkg = r.modContentPack?.PackageId?.ToLowerInvariant();
                    b = branches.FirstOrDefault(x => pkg != null && x.mods.Contains(pkg) && !x.exceptProjects.Contains(r.defName))
                        ?? branches.FirstOrDefault(x => x.techLevels.Contains(r.techLevel))
                        ?? FreeBranch;
                }
                BranchOf[r] = b;
                // Pesquisa da Anomalia (conhecimento) fica na aba própria; o resto vai para a aba do ramo.
                if (r.knowledgeCategory == null && b.tab != null)
                    r.tab = b.tab;
                r.description = (r.description ?? "").TrimEnd() + "\n\n" + BranchNote(b, r);
            }
            Layout();
        }

        private static string BranchNote(ResearchBranchDef b, ResearchProjectDef r)
        {
            if (b.free)
                return "Ramo: Bancada (pesquisa livre).";
            if (b.faction == null)
                return "Ramo: " + b.label + ". Só com sucata desmontada ou dados antigos.";
            return "Ramo: " + b.label + ". Licença: cargo de " + EQLicenses.RequiredTitleLabel(r) + " " + (b.labelOf ?? b.label)
                + " (ou compra com favor, sendo membro); sem licença, só por sucata ou dados antigos, e fabricar é pirataria.";
        }

        /// <summary>Posição inicial por aba: coluna pelo nível tecnológico e pré-requisitos da mesma aba, ordem pelo baricentro.
        /// O Clean Research Sort e o YART refazem depois; isto só garante um ponto de partida sem sobreposição.</summary>
        private static void Layout()
        {
            foreach (IGrouping<ResearchTabDef, ResearchProjectDef> group in DefDatabase<ResearchProjectDef>.AllDefsListForReading
                .Where(r => r.knowledgeCategory == null && BranchOf.TryGetValue(r, out ResearchBranchDef b) && b.tab == r.tab).GroupBy(r => r.tab))
            {
                var set = new HashSet<ResearchProjectDef>(group);
                var col = new Dictionary<ResearchProjectDef, int>();
                int Tech(ResearchProjectDef r) => r.techLevel == TechLevel.Undefined ? (int)TechLevel.Industrial : (int)r.techLevel;
                IEnumerable<ResearchProjectDef> Prereqs(ResearchProjectDef r) =>
                    (r.prerequisites ?? Enumerable.Empty<ResearchProjectDef>()).Concat(r.hiddenPrerequisites ?? Enumerable.Empty<ResearchProjectDef>()).Where(set.Contains);
                int baseCol = 0;
                foreach (IGrouping<int, ResearchProjectDef> level in group.GroupBy(Tech).OrderBy(g => g.Key))
                {
                    var visiting = new HashSet<ResearchProjectDef>();
                    int Col(ResearchProjectDef r)
                    {
                        if (col.TryGetValue(r, out int c)) return c;
                        if (!visiting.Add(r)) return baseCol;
                        int v = baseCol;
                        foreach (ResearchProjectDef p in Prereqs(r))
                            v = Math.Max(v, Col(p) + 1);
                        col[r] = v;
                        return v;
                    }
                    foreach (ResearchProjectDef r in level.OrderBy(x => x.defName)) Col(r);
                    baseCol = col.Values.Max() + 1;
                }
                var y = new Dictionary<ResearchProjectDef, float>();
                float physX = 0f;
                foreach (IGrouping<int, ResearchProjectDef> column in col.Keys.GroupBy(r => col[r]).OrderBy(g => g.Key))
                {
                    List<ResearchProjectDef> ordered = column.OrderBy(r =>
                    {
                        List<float> ys = Prereqs(r).Where(y.ContainsKey).Select(p => y[p]).ToList();
                        return ys.Count > 0 ? ys.Average() : 999f;
                    }).ThenBy(r => r.modContentPack?.PackageId).ThenBy(r => r.defName).ToList();
                    for (int i = 0; i < ordered.Count; i++)
                    {
                        if (i > 0 && i % MaxRows == 0) physX += 1f;
                        ResearchProjectDef r = ordered[i];
                        y[r] = (i % MaxRows) * RowStep;
                        r.researchViewX = physX;
                        r.researchViewY = y[r];
                        fX(r) = physX;
                        fY(r) = y[r];
                    }
                    physX += 1f;
                }
            }
        }

        public static ResearchBranchDef Branch(ResearchProjectDef r) => r != null && BranchOf.TryGetValue(r, out ResearchBranchDef b) ? b : null;

        /// <summary>O jogador pode pesquisar o projeto? (fora do jogo, sempre.)</summary>
        public static bool HasAccess(ResearchProjectDef r)
        {
            ResearchBranchDef b = Branch(r);
            if (b == null || b.free || r.IsFinished)
                return true;
            WorldComponent_EQResearch comp = WorldComponent_EQResearch.Current;
            if (comp == null)
                return true;
            return comp.IsGranted(r) || EQLicenses.IsLicensed(r);
        }

        /// <summary>Motivo da trava (null = sem trava nossa).</summary>
        public static string LockReason(ResearchProjectDef r)
        {
            if (HasAccess(r))
                return null;
            ResearchBranchDef b = Branch(r);
            if (b.faction == null)
                return "Tecnologia do Velho Mundo: exige sucata ou dados antigos";
            return "Patente " + (b.labelOf ?? "de " + b.label) + ": exige o cargo de " + EQLicenses.RequiredTitleLabel(r)
                + " (membros compram com favor), sucata ou dados antigos";
        }
    }

    /// <summary>Acesso do jogador aos projetos fora da bancada.</summary>
    public class WorldComponent_EQResearch : WorldComponent
    {
        private HashSet<ResearchProjectDef> granted = new HashSet<ResearchProjectDef>();
        /// <summary>Licenças compradas com favor (valem enquanto o jogador for membro da potência dona).</summary>
        private HashSet<ResearchProjectDef> purchased = new HashSet<ResearchProjectDef>();
        /// <summary>Última pirataria cobrada por potência (defName → tick).</summary>
        private Dictionary<string, int> lastPiracy = new Dictionary<string, int>();

        private static World cachedWorld;
        private static WorldComponent_EQResearch cached;

        public WorldComponent_EQResearch(World world) : base(world)
        {
        }

        public static WorldComponent_EQResearch Current
        {
            get
            {
                World w = Find.World;
                if (w == null) return null;
                if (w != cachedWorld)
                {
                    cachedWorld = w;
                    cached = w.GetComponent<WorldComponent_EQResearch>();
                }
                return cached;
            }
        }

        public bool IsGranted(ResearchProjectDef r) => granted.Contains(r);

        public void Grant(ResearchProjectDef r) => granted.Add(r);

        public void Revoke(ResearchProjectDef r) => granted.Remove(r);

        public IEnumerable<ResearchProjectDef> Granted => granted;

        public bool IsPurchased(ResearchProjectDef r) => purchased.Contains(r);

        public void Purchase(ResearchProjectDef r) => purchased.Add(r);

        public IEnumerable<ResearchProjectDef> Purchased => purchased;

        public int LastPiracyTick(FactionDef f) => f != null && lastPiracy.TryGetValue(f.defName, out int t) ? t : -999999999;

        public void SetLastPiracy(FactionDef f, int tick) => lastPiracy[f.defName] = tick;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref granted, "granted", LookMode.Def);
            if (granted == null) granted = new HashSet<ResearchProjectDef>();
            granted.Remove(null);
            Scribe_Collections.Look(ref purchased, "purchased", LookMode.Def);
            if (purchased == null) purchased = new HashSet<ResearchProjectDef>();
            purchased.Remove(null);
            Scribe_Collections.Look(ref lastPiracy, "lastPiracy", LookMode.Value, LookMode.Value);
            if (lastPiracy == null) lastPiracy = new Dictionary<string, int>();
        }
    }

    [HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.CanStartNow), MethodType.Getter)]
    public static class Patch_ResearchProjectDef_CanStartNow
    {
        public static void Postfix(ResearchProjectDef __instance, ref bool __result)
        {
            if (__result && !EQResearch.HasAccess(__instance))
                __result = false;
        }
    }

    /// <summary>Projeto atual sem acesso (save antigo, acesso perdido): para a pesquisa e avisa.</summary>
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ResearchPerformed))]
    public static class Patch_ResearchManager_ResearchPerformed
    {
        public static bool Prefix(ResearchManager __instance)
        {
            ResearchProjectDef proj = __instance.GetProject();
            if (proj == null || EQResearch.HasAccess(proj))
                return true;
            __instance.StopProject(proj);
            Messages.Message("Pesquisa interrompida: " + proj.LabelCap + ". " + EQResearch.LockReason(proj) + ".", MessageTypeDefOf.RejectInput, false);
            return false;
        }
    }

    /// <summary>Tela de pesquisa vanilla: mostra o motivo da nossa trava (sem ele o jogo acusa "locked but no reasons").</summary>
    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawStartButton")]
    public static class Patch_MainTabWindow_Research_DrawStartButton
    {
        private static readonly AccessTools.FieldRef<MainTabWindow_Research, ResearchProjectDef> fSelected =
            AccessTools.FieldRefAccess<MainTabWindow_Research, ResearchProjectDef>("selectedProject");

        public static bool Prefix(MainTabWindow_Research __instance, Rect startButRect)
        {
            ResearchProjectDef p = fSelected(__instance);
            if (p == null || p.IsFinished || Find.ResearchManager.IsCurrentProject(p))
                return true;
            string reason = EQResearch.LockReason(p);
            if (reason == null)
                return true;
            var reasons = new List<string>();
            if (!p.PrerequisitesCompleted) reasons.Add("PrerequisitesNotCompleted".Translate());
            if (!p.TechprintRequirementMet) reasons.Add("InsufficientTechprintsApplied".Translate(p.TechprintsApplied, p.TechprintCount));
            if (p.knowledgeCategory == null && !p.PlayerHasAnyAppropriateResearchBench) reasons.Add("MissingRequiredResearchFacilities".Translate());
            reasons.Add(reason);
            string text = "Locked".Translate() + ":\n" + reasons.ToLineList("  ");
            Widgets.DrawHighlight(startButRect);
            Rect r = startButRect.ContractedBy(4f);
            Vector2 size = Text.CalcSize(text);
            if (size.x > r.width || size.y > r.height)
            {
                TooltipHandler.TipRegion(startButRect, text);
                text = "Locked".Translate();
                Text.Anchor = TextAnchor.MiddleCenter;
            }
            Widgets.Label(r, text);
            Text.Anchor = TextAnchor.UpperLeft;
            return false;
        }
    }

    /// <summary>Tela vanilla: esconde abas vazias (as dos mods, cujos projetos foram para as nossas) e abre na Bancada.</summary>
    [HarmonyPatch(typeof(MainTabWindow_Research), nameof(MainTabWindow_Research.PostOpen))]
    public static class Patch_MainTabWindow_Research_PostOpen
    {
        public static void Postfix(MainTabWindow_Research __instance)
        {
            if (EQResearch.FreeBranch == null)
                return;
            var tabs = Traverse.Create(__instance).Field("tabs").GetValue() as System.Collections.IList;
            if (tabs == null)
                return;
            var used = new HashSet<ResearchTabDef>(DefDatabase<ResearchProjectDef>.AllDefsListForReading.Select(r => r.tab));
            for (int i = tabs.Count - 1; i >= 0; i--)
            {
                ResearchTabDef def = Traverse.Create(tabs[i]).Field("def").GetValue<ResearchTabDef>();
                if (def != null && !used.Contains(def))
                    tabs.RemoveAt(i);
            }
            if (__instance.CurTab == null || !used.Contains(__instance.CurTab))
                __instance.CurTab = EQResearch.FreeBranch.tab;
        }
    }

    /// <summary>YART: acrescenta o motivo da nossa trava à lista dele.</summary>
    [HarmonyPatch]
    public static class Patch_YART_GetLockedReasons
    {
        private static Type nodeType;

        public static bool Prepare()
        {
            nodeType = AccessTools.TypeByName("YART.ResearchNode");
            return nodeType != null && AccessTools.Method(nodeType, "GetLockedReasons") != null;
        }

        public static MethodBase TargetMethod() => AccessTools.Method(nodeType, "GetLockedReasons");

        public static void Postfix(object __instance, List<string> outReasons)
        {
            ResearchProjectDef def = Traverse.Create(__instance).Property("Def").GetValue<ResearchProjectDef>();
            if (def == null || def.IsFinished)
                return;
            string reason = EQResearch.LockReason(def);
            if (reason != null && !outReasons.Contains(reason))
                outReasons.Add(reason);
        }
    }
}
