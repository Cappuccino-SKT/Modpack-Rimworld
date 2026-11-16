using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Regras do início de jogo (Defs/Misc/EQ_StartRules.xml).</summary>
    public class StartRulesDef : Def
    {
        /// <summary>Na criação do jogo só aparecem os xenotipos do cenário (ou os padrões do mundo).</summary>
        public bool restrictStartingXenotypes = true;
        /// <summary>Cenários de fora do mod não aparecem na lista.</summary>
        public bool hideOtherScenarios = true;
        /// <summary>Narradores de fora do mod não aparecem na lista.</summary>
        public bool hideOtherStorytellers = true;
        /// <summary>Com o modo desenvolvedor ligado nada é escondido (o editor de xenotipo é a ferramenta de criação do mod).</summary>
        public bool devModeShowsAll = true;

        public static StartRulesDef Rules => DefDatabase<StartRulesDef>.AllDefsListForReading.FirstOrDefault();
    }

    /// <summary>Num ScenarioDef: xenotipos que os colonos iniciais podem ter (peso = chance no sorteio).</summary>
    public class ScenarioXenotypesExtension : DefModExtension
    {
        public List<XenotypeChance> xenotypeChances = new List<XenotypeChance>();
    }

    /// <summary>Num StorytellerDef nosso: copia o ritmo (comps e curvas) de outro narrador, já com o que os
    /// mods acrescentaram a ele por patch.</summary>
    public class StorytellerCopyExtension : DefModExtension
    {
        public StorytellerDef copyFrom;
    }

    public static class EQGameStart
    {
        private static readonly HashSet<Scenario> hiddenScenarios = new HashSet<Scenario>();
        private static Dictionary<string, XenotypeDef> xenoByLabel;

        private static bool Ours(Def d) =>
            d.modContentPack != null && d.modContentPack.PackageId.ToLowerInvariant().StartsWith("ecosdaqueda");

        private static bool ShowAll(StartRulesDef r) => r == null || (r.devModeShowsAll && Prefs.DevMode);

        public static void Apply()
        {
            StartRulesDef r = StartRulesDef.Rules;
            if (r == null)
                return;
            foreach (StorytellerDef s in DefDatabase<StorytellerDef>.AllDefsListForReading)
            {
                StorytellerCopyExtension ext = s.GetModExtension<StorytellerCopyExtension>();
                if (ext?.copyFrom == null)
                    continue;
                StorytellerDef o = ext.copyFrom;
                if (s.comps.NullOrEmpty()) s.comps = o.comps;
                s.populationIntentFactorFromPopCurve = s.populationIntentFactorFromPopCurve ?? o.populationIntentFactorFromPopCurve;
                s.populationIntentFactorFromPopAdaptDaysCurve = s.populationIntentFactorFromPopAdaptDaysCurve ?? o.populationIntentFactorFromPopAdaptDaysCurve;
                s.pointsFactorFromDaysPassed = s.pointsFactorFromDaysPassed ?? o.pointsFactorFromDaysPassed;
                s.pointsFactorFromAdaptDays = s.pointsFactorFromAdaptDays ?? o.pointsFactorFromAdaptDays;
                s.adaptDaysLossFromColonistLostByPostPopulation = s.adaptDaysLossFromColonistLostByPostPopulation ?? o.adaptDaysLossFromColonistLostByPostPopulation;
                s.adaptDaysLossFromColonistViolentlyDownedByPopulation = s.adaptDaysLossFromColonistViolentlyDownedByPopulation ?? o.adaptDaysLossFromColonistViolentlyDownedByPopulation;
                s.adaptDaysGrowthRateCurve = s.adaptDaysGrowthRateCurve ?? o.adaptDaysGrowthRateCurve;
                s.adaptDaysMin = o.adaptDaysMin;
                s.adaptDaysMax = o.adaptDaysMax;
                s.adaptDaysGameStartGraceDays = o.adaptDaysGameStartGraceDays;
            }
            if (r.hideOtherStorytellers && DefDatabase<StorytellerDef>.AllDefsListForReading.Any(s => Ours(s) && s.listVisible))
                foreach (StorytellerDef s in DefDatabase<StorytellerDef>.AllDefsListForReading)
                    if (!Ours(s))
                        s.listVisible = false;
            if (r.hideOtherScenarios && DefDatabase<ScenarioDef>.AllDefsListForReading.Any(Ours))
                foreach (ScenarioDef s in DefDatabase<ScenarioDef>.AllDefsListForReading)
                    if (!Ours(s) && s.scenario != null)
                        hiddenScenarios.Add(s.scenario);
        }

        public static bool ScenarioVisible(Scenario s) => ShowAll(StartRulesDef.Rules) || !hiddenScenarios.Contains(s);

        // ------------------------------------------------------------ xenotipos dos colonos iniciais

        /// <summary>Xenotipos do cenário em uso; sem lista própria, os padrões do mundo.</summary>
        public static List<XenotypeChance> StarterChances()
        {
            Scenario sc = Current.Game?.Scenario;
            if (sc != null)
                foreach (ScenarioDef d in DefDatabase<ScenarioDef>.AllDefsListForReading)
                    if (d.scenario == sc || (d.scenario != null && d.scenario.name == sc.name))
                    {
                        ScenarioXenotypesExtension ext = d.GetModExtension<ScenarioXenotypesExtension>();
                        if (ext != null && ext.xenotypeChances.Count > 0)
                            return ext.xenotypeChances;
                    }
            return DefDatabase<WorldXenotypesDef>.AllDefsListForReading.FirstOrDefault()?.baselinerReplacements
                ?? new List<XenotypeChance>();
        }

        public static bool AllowedForStart(XenotypeDef x) => StarterChances().Any(c => c.xenotype == x);

        public static XenotypeDef RandomStarter() =>
            StarterChances().TryRandomElementByWeight(c => c.chance, out XenotypeChance c2) ? c2.xenotype : null;

        public static bool RestrictStarters
        {
            get
            {
                StartRulesDef r = StartRulesDef.Rules;
                return r != null && r.restrictStartingXenotypes;
            }
        }

        /// <summary>Tira do menu de xenotipos da criação do jogo (tela do jogo e Prepare Carefully) tudo o que o
        /// cenário não permite: xenos de fora, elites, xenos personalizados e o editor.</summary>
        public static void FilterXenotypeMenu(List<FloatMenuOption> options, bool test = false)
        {
            StartRulesDef r = StartRulesDef.Rules;
            if (options == null || options.Count < 5 || r == null || !r.restrictStartingXenotypes || (!test && ShowAll(r)))
                return;
            if (!test && Current.ProgramState != ProgramState.Entry)
                return;
            if (xenoByLabel == null)
            {
                xenoByLabel = new Dictionary<string, XenotypeDef>();
                foreach (XenotypeDef x in DefDatabase<XenotypeDef>.AllDefsListForReading)
                    xenoByLabel[x.LabelCap.ToString()] = x;
            }
            if (options.Count(o => o.Label != null && xenoByLabel.ContainsKey(o.Label)) < 5)
                return;
            string editor = "XenotypeEditor".Translate().ToString();
            var custom = new HashSet<string>(CharacterCardUtility.CustomXenotypesForReading.Select(c => c.name.ToLowerInvariant()));
            options.RemoveAll(o =>
            {
                string l = o.Label ?? "";
                if (l.StartsWith(editor) || custom.Contains(l.ToLowerInvariant()))
                    return true;
                return xenoByLabel.TryGetValue(l, out XenotypeDef x) && !AllowedForStart(x);
            });
        }
    }

    [HarmonyPatch(typeof(FloatMenu), MethodType.Constructor, new[] { typeof(List<FloatMenuOption>) })]
    public static class Patch_FloatMenu_Ctor
    {
        public static void Prefix(List<FloatMenuOption> options)
        {
            try { EQGameStart.FilterXenotypeMenu(options); }
            catch (System.Exception e) { Log.ErrorOnce("[Ecos da Queda] filtro de xenotipos: " + e, 91823771); }
        }
    }

    [HarmonyPatch(typeof(ScenarioLister), nameof(ScenarioLister.ScenariosInCategory))]
    public static class Patch_ScenarioLister_ScenariosInCategory
    {
        public static IEnumerable<Scenario> Postfix(IEnumerable<Scenario> __result)
        {
            foreach (Scenario s in __result)
                if (EQGameStart.ScenarioVisible(s))
                    yield return s;
        }
    }

    [HarmonyPatch(typeof(ScenarioLister), nameof(ScenarioLister.AllScenarios))]
    public static class Patch_ScenarioLister_AllScenarios
    {
        public static IEnumerable<Scenario> Postfix(IEnumerable<Scenario> __result)
        {
            foreach (Scenario s in __result)
                if (EQGameStart.ScenarioVisible(s))
                    yield return s;
        }
    }

    /// <summary>Parte de cenário: o jogador já começa com história junto a uma facção (boa vontade, Fama, Infâmia).</summary>
    public class ScenPart_EQReputation : ScenPart
    {
        public FactionDef factionDef;
        /// <summary>Somado à boa vontade inicial (o mod converte em Fama/Infâmia como qualquer mudança).</summary>
        public int goodwill;
        public float fame;
        public float infamy;
        public string note;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref factionDef, "factionDef");
            Scribe_Values.Look(ref goodwill, "goodwill");
            Scribe_Values.Look(ref fame, "fame");
            Scribe_Values.Look(ref infamy, "infamy");
            Scribe_Values.Look(ref note, "note");
        }

        public override string Summary(Scenario scen)
        {
            if (!note.NullOrEmpty())
                return note;
            return factionDef == null ? "" : "Você começa com história junto a " + factionDef.label + ".";
        }

        public override void PostGameStart()
        {
            Faction f = factionDef == null ? null : Find.FactionManager.FirstFactionOfDef(factionDef);
            if (f == null || f.IsPlayer)
                return;
            if (goodwill != 0)
                f.TryAffectGoodwillWith(Faction.OfPlayer, goodwill, canSendMessage: false, canSendHostilityLetter: false);
            if (fame > 0f || infamy > 0f)
                WorldComponent_Reputation.Current?.Record(f, fame, infamy);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (factionDef == null)
                yield return "ScenPart_EQReputation sem factionDef";
        }
    }
}
