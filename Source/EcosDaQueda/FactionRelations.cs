using System.Linq;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Uma relação fixa entre duas facções do mod.</summary>
    public class FactionRelationEntry
    {
        public FactionDef a;
        public FactionDef b;
        /// <summary>Boa vontade inicial (-100 a 100). &lt;= -75 vira hostil; &gt;= 75 vira aliado.</summary>
        public int goodwill;
        /// <summary>Nome da relação mostrado na aba Reputação (ex.: "ódio mortal").</summary>
        public string label;
        /// <summary>Inimigos para sempre: a hostilidade é reimposta e o Hegemony não deixa fazerem paz.</summary>
        public bool permanentEnemies;
    }

    /// <summary>Matriz de relações entre as potências (1.7/Defs/EQ_FactionRelations.xml).</summary>
    public class FactionRelationPresetDef : Def
    {
        public List<FactionRelationEntry> relations = new List<FactionRelationEntry>();

        public static FactionRelationEntry Find(FactionDef x, FactionDef y)
        {
            foreach (FactionRelationPresetDef preset in DefDatabase<FactionRelationPresetDef>.AllDefsListForReading)
                foreach (FactionRelationEntry e in preset.relations)
                    if ((e.a == x && e.b == y) || (e.a == y && e.b == x))
                        return e;
            return null;
        }

        /// <summary>A facção aparece em alguma relação fixa (potência ou facção menor).</summary>
        public static bool Involves(FactionDef x)
        {
            if (x == null) return false;
            foreach (FactionRelationPresetDef preset in DefDatabase<FactionRelationPresetDef>.AllDefsListForReading)
                foreach (FactionRelationEntry e in preset.relations)
                    if (e.a == x || e.b == x)
                        return true;
            return false;
        }

        public static bool ArePermanentEnemies(Faction x, Faction y)
        {
            if (x == null || y == null) return false;
            return Find(x.def, y.def)?.permanentEnemies ?? false;
        }
    }

    /// <summary>
    /// Aplica a matriz de relações uma vez por mundo (ao criar ou ao carregar um save antigo)
    /// e, diariamente, reimpõe a hostilidade entre inimigos permanentes.
    /// </summary>
    public class WorldComponent_FactionRelations : WorldComponent
    {
        private bool applied;
        /// <summary>loadID das facções que já receberam a boa vontade inicial com o jogador.</summary>
        private List<int> playerGoodwillApplied = new List<int>();
        private const int EnforceInterval = 60000; // 1 dia

        public WorldComponent_FactionRelations(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref applied, "applied", false);
            Scribe_Collections.Look(ref playerGoodwillApplied, "playerGoodwillApplied", LookMode.Value);
            if (playerGoodwillApplied == null) playerGoodwillApplied = new List<int>();
        }

        public override void FinalizeInit(bool fromLoad)
        {
            base.FinalizeInit(fromLoad);
            if (applied || world?.factionManager == null)
                return;
            ForEachPair((fa, fb, e) => SetRelation(fa, fb, e.goodwill));
            applied = true;
        }

        /// <summary>Boa vontade inicial com o jogador das facções menores (MinorFactionExtension.playerGoodwill).</summary>
        private void ApplyPlayerGoodwill()
        {
            Faction player = Faction.OfPlayerSilentFail;
            if (player == null) return;
            foreach (Faction f in world.factionManager.AllFactionsListForReading)
            {
                int? gw = f.def.GetModExtension<MinorFactionExtension>()?.playerGoodwill;
                if (gw == null || f.IsPlayer || EQUtil.IsProxy(f) || playerGoodwillApplied.Contains(f.loadID)) continue;
                playerGoodwillApplied.Add(f.loadID);
                if (!f.def.permanentEnemy)
                    SetRelation(player, f, gw.Value);
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % 250 == 1)
                ApplyPlayerGoodwill();
            if (Find.TickManager.TicksGame % EnforceInterval != 0)
                return;
            ForEachPair((fa, fb, e) =>
            {
                if (e.permanentEnemies && (fa.GoodwillWith(fb) > -75 || fa.RelationKindWith(fb) != FactionRelationKind.Hostile))
                    SetRelation(fa, fb, System.Math.Min(e.goodwill, -80));
            });
        }

        private void ForEachPair(System.Action<Faction, Faction, FactionRelationEntry> action)
        {
            foreach (FactionRelationPresetDef preset in DefDatabase<FactionRelationPresetDef>.AllDefsListForReading)
            {
                foreach (FactionRelationEntry e in preset.relations)
                {
                    if (e?.a == null || e.b == null)
                        continue;
                    foreach (Faction fa in world.factionManager.AllFactionsListForReading)
                    {
                        if (fa.def != e.a || fa.defeated || EQUtil.IsProxy(fa)) continue;
                        foreach (Faction fb in world.factionManager.AllFactionsListForReading)
                        {
                            if (fb.def != e.b || fb.defeated || fb == fa || EQUtil.IsProxy(fb)) continue;
                            action(fa, fb, e);
                        }
                    }
                }
            }
        }

        private static void SetRelation(Faction fa, Faction fb, int goodwill)
        {
            goodwill = System.Math.Max(-100, System.Math.Min(100, goodwill));
            FactionRelationKind kind = goodwill <= -75 ? FactionRelationKind.Hostile
                : goodwill >= 75 ? FactionRelationKind.Ally
                : FactionRelationKind.Neutral;
            Apply(fa.RelationWith(fb, allowNull: true), goodwill, kind);
            Apply(fb.RelationWith(fa, allowNull: true), goodwill, kind);
        }

        private static void Apply(FactionRelation rel, int goodwill, FactionRelationKind kind)
        {
            if (rel == null) return;
            rel.baseGoodwill = goodwill;
            rel.kind = kind;
        }
    }
    /// <summary>Opções de facção menor (modExtensions do FactionDef).</summary>
    public class MinorFactionExtension : DefModExtension
    {
        /// <summary>Sem assentamentos no mapa-múndi: remove o que o jogo cria ao gerar a facção.</summary>
        public bool noSettlements;
        /// <summary>Boa vontade inicial com o jogador (aplicada uma vez por mundo). Vazio = padrão do jogo.</summary>
        public int? playerGoodwill;
        /// <summary>
        /// Como noSettlements, mas só quando existe um VEF.Planet.MovingBaseDef para a facção (assentamento
        /// móvel). Sem o mod que dá a classe do assentamento móvel, a facção fica com assentamentos fixos.
        /// </summary>
        public bool noSettlementsIfMovingBase;
        /// <summary>Estoque do assentamento móvel (substitui o do VFE Medieval 2, que é medieval).</summary>
        public TraderKindDef movingBaseTrader;
    }

    /// <summary>
    /// O jogo dá um assentamento inicial a toda facção visível, mesmo com settlementGenerationWeight 0.
    /// Para facção marcada com noSettlements (células que ninguém sabe onde ficam), ele é removido na hora.
    /// </summary>
    [HarmonyLib.HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.NewGeneratedFaction), new[] { typeof(PlanetLayer), typeof(FactionGeneratorParms) })]
    public static class Patch_FactionGenerator_NewGeneratedFaction
    {
        public static void Postfix(Faction __result)
        {
            MinorFactionExtension ext = __result?.def?.GetModExtension<MinorFactionExtension>();
            if (ext == null || !(ext.noSettlements || (ext.noSettlementsIfMovingBase && EQWorldFactions.HasMovingBase(__result.def))))
                return;
            foreach (Settlement s in Find.WorldObjects.Settlements.Where(x => x.Faction == __result).ToList())
                s.Destroy();
        }
    }
    /// <summary>Quais facções existem no mundo (Defs/Factions/EQ_WorldFactions.xml).</summary>
    public class WorldFactionsDef : Def
    {
        /// <summary>Facções de terceiros que continuam sendo geradas.</summary>
        public List<FactionDef> keepFactions = new List<FactionDef>();
        /// <summary>Facções ocultas de terceiros que também saem (as visíveis saem todas, menos as de keepFactions).</summary>
        public List<FactionDef> removeHiddenFactions = new List<FactionDef>();
        /// <summary>Missões que dependem de uma facção removida (ex.: o Império): nunca são geradas.</summary>
        public List<QuestScriptDef> disabledQuests = new List<QuestScriptDef>();
        /// <summary>Eventos que dependem de uma facção removida: nunca disparam.</summary>
        public List<IncidentDef> disabledIncidents = new List<IncidentDef>();
    }

    /// <summary>
    /// O mundo só tem as nossas facções: toda facção humana visível que não é do mod (tribos, forasteiros,
    /// piratas do jogo e de outros mods) deixa de ser gerada e some da lista de criação do mundo.
    /// Facções ocultas e não humanas (mecanoides, insetos, entidades, anciões) ficam.
    /// Roda uma vez, depois que os defs carregam; saves antigos não mudam.
    /// </summary>
    public static class EQWorldFactions
    {
        public static void Apply()
        {
            // Facção toda móvel: além do assentamento inicial (removido no patch do FactionGenerator), o jogo
            // distribui os demais pelo peso; com assentamento móvel disponível, o peso vira 0.
            foreach (FactionDef d in DefDatabase<FactionDef>.AllDefsListForReading)
            {
                MinorFactionExtension ext = d.GetModExtension<MinorFactionExtension>();
                if (ext != null && (ext.noSettlements || (ext.noSettlementsIfMovingBase && HasMovingBase(d))))
                    d.settlementGenerationWeight = 0f;
            }

            WorldFactionsDef cfg = DefDatabase<WorldFactionsDef>.AllDefsListForReading.FirstOrDefault();
            if (cfg == null) return;
            var removed = new List<string>();
            foreach (FactionDef d in DefDatabase<FactionDef>.AllDefsListForReading)
            {
                bool ours = d.modContentPack != null && d.modContentPack.PackageId.StartsWith("ecosdaqueda");
                bool visibleHuman = !d.isPlayer && !d.hidden && d.humanlikeFaction;
                if (ours || cfg.keepFactions.Contains(d)) continue;
                if (!visibleHuman && !cfg.removeHiddenFactions.Contains(d)) continue;
                d.requiredCountAtGameStart = 0;
                d.startingCountAtWorldCreation = 0;
                d.maxConfigurableAtWorldCreation = 0;
                removed.Add(d.defName);
            }
            if (removed.Count > 0)
                Log.Message("[Ecos da Queda] Facções de terceiros fora do mundo (" + removed.Count + "): " + string.Join(", ", removed));

            foreach (QuestScriptDef q in cfg.disabledQuests)
                if (q != null) { q.rootSelectionWeight = 0f; DisabledQuests.Add(q); }
            foreach (IncidentDef inc in cfg.disabledIncidents)
                if (inc != null) { inc.baseChance = 0f; DisabledIncidents.Add(inc); }
        }

        public static readonly HashSet<QuestScriptDef> DisabledQuests = new HashSet<QuestScriptDef>();
        public static readonly HashSet<IncidentDef> DisabledIncidents = new HashSet<IncidentDef>();

        private static System.Type movingBaseDefType;

        /// <summary>Existe um VEF.Planet.MovingBaseDef (assentamento móvel) para esta facção?</summary>
        public static bool HasMovingBase(FactionDef def)
        {
            if (movingBaseDefType == null) movingBaseDefType = HarmonyLib.AccessTools.TypeByName("VEF.Planet.MovingBaseDef");
            if (movingBaseDefType == null) return false;
            foreach (Def d in GenDefDatabase.GetAllDefsInDatabaseForDef(movingBaseDefType))
                if (HarmonyLib.Traverse.Create(d).Field("baseFaction").GetValue<FactionDef>() == def)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// O assentamento móvel do VFE Medieval 2 (VFEMedieval.MerchantGuild) usa sempre o estoque medieval da
    /// guilda. Quando o dono é uma facção nossa com movingBaseTrader, usa esse estoque no lugar.
    /// Sem o VFE Medieval 2 o patch não é aplicado.
    /// </summary>
    [HarmonyLib.HarmonyPatch]
    public static class Patch_MerchantGuild_TraderKind
    {
        public static bool Prepare() => HarmonyLib.AccessTools.TypeByName("VFEMedieval.MerchantGuild") != null;

        public static System.Reflection.MethodBase TargetMethod() =>
            HarmonyLib.AccessTools.PropertyGetter(HarmonyLib.AccessTools.TypeByName("VFEMedieval.MerchantGuild"), "TraderKind");

        public static void Postfix(WorldObject __instance, ref TraderKindDef __result)
        {
            TraderKindDef own = __instance?.Faction?.def?.GetModExtension<MinorFactionExtension>()?.movingBaseTrader;
            if (own != null) __result = own;
        }
    }
    /// <summary>Missão desligada (WorldFactionsDef.disabledQuests) nunca pode rodar, nem forçada por evento.</summary>
    [HarmonyLib.HarmonyPatch]
    public static class Patch_QuestScriptDef_CanRun
    {
        public static IEnumerable<System.Reflection.MethodBase> TargetMethods() =>
            HarmonyLib.AccessTools.GetDeclaredMethods(typeof(QuestScriptDef)).Where(m => m.Name == "CanRun");

        public static bool Prefix(QuestScriptDef __instance, ref bool __result)
        {
            if (!EQWorldFactions.DisabledQuests.Contains(__instance)) return true;
            __result = false;
            return false;
        }
    }

    /// <summary>Evento desligado (WorldFactionsDef.disabledIncidents) nunca dispara.</summary>
    [HarmonyLib.HarmonyPatch]
    public static class Patch_IncidentWorker_Disabled
    {
        public static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            yield return HarmonyLib.AccessTools.Method(typeof(IncidentWorker), nameof(IncidentWorker.CanFireNow));
            yield return HarmonyLib.AccessTools.Method(typeof(IncidentWorker), nameof(IncidentWorker.TryExecute));
        }

        public static bool Prefix(IncidentWorker __instance, ref bool __result)
        {
            if (__instance?.def == null || !EQWorldFactions.DisabledIncidents.Contains(__instance.def)) return true;
            __result = false;
            return false;
        }
    }
}
