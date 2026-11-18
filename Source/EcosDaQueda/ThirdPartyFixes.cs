using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Vanilla Exploration Expanded: os mutadores de animais (predadores, presas, aves, venenosos, noturnos, santuário
    /// marinho e os "reduzidos") guardam em tmpAnimals a lista do primeiro mapa gerado na sessão (o worker é um só por
    /// mutador) e devolvem null quando o bioma não tem bicho do tipo, o que quebra a geração do mapa ("Error in GenStep"
    /// em GenStep_Animals) e o surgimento de animais no jogo. Aqui a lista é calculada pelo próprio método original uma vez
    /// por bioma e costa, e vazia em vez de null (sem o ErrorOnce do mod).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_VEE_AnimalMutators_GetAnimalKind
    {
        private static readonly string[] Workers = { "Predators", "Prey", "Aerie", "Venomous", "Nocturnal", "MarineSanctuary" };
        private static readonly Dictionary<(object, BiomeDef, bool), List<PawnKindDef>> cache = new Dictionary<(object, BiomeDef, bool), List<PawnKindDef>>();
        [ThreadStatic] internal static bool inside;

        private static IEnumerable<Type> Types => Workers.Select(w => AccessTools.TypeByName("VanillaExplorationExpanded.TileMutatorWorker_" + w)).Where(t => t != null);

        public static bool Prepare() => Types.Any();

        public static IEnumerable<MethodBase> TargetMethods()
            => Types.Select(t => AccessTools.DeclaredMethod(t, "GetAnimalKind")).Where(m => m != null && m.ReturnType == typeof(List<PawnKindDef>));

        private static (object, BiomeDef, bool) Key(object inst, PlanetTile tile) => (inst, Find.WorldGrid[tile].PrimaryBiome, tile.Tile.IsCoastal);

        public static bool Prefix(object __instance, PlanetTile tile, ref List<PawnKindDef> __result, out bool __state)
        {
            __state = false;
            if (cache.TryGetValue(Key(__instance, tile), out List<PawnKindDef> list))
            {
                __result = list;
                return false;
            }
            Traverse.Create(__instance).Field("tmpAnimals").SetValue(new List<PawnKindDef>());
            inside = true;
            __state = true;
            return true;
        }

        public static void Postfix(object __instance, PlanetTile tile, ref List<PawnKindDef> __result, bool __state)
        {
            if (!__state)
                return;
            List<PawnKindDef> list = Traverse.Create(__instance).Field("tmpAnimals").GetValue<List<PawnKindDef>>() ?? new List<PawnKindDef>();
            cache[Key(__instance, tile)] = list;
            __result = list;
        }

        public static void Finalizer(bool __state)
        {
            if (__state)
                inside = false;
        }
    }

    /// <summary>Cala o ErrorOnce dos mutadores de animais do VEE enquanto o cálculo acima roda (bioma sem bicho do tipo é normal).</summary>
    [HarmonyPatch(typeof(Log), nameof(Log.ErrorOnce))]
    public static class Patch_Log_ErrorOnce_VEEMutators
    {
        public static bool Prefix(string text) => !(Patch_VEE_AnimalMutators_GetAnimalKind.inside && text != null && text.StartsWith("TileMutatorWorker_"));
    }

    /// <summary>
    /// Hegemony (Faction Territories 2): FactionDispositions.LeaderFor compara a facção com Faction.OfPlayer, que dá
    /// "Could not find player faction" enquanto o mundo é gerado (a colônia ainda não existe). Como é chamado dentro de
    /// uma ordenação (prédios especiais das capitais), um mundo novo gerava ~7.400 erros e o log batia no limite de
    /// mensagens antes do jogo começar. Sem facção do jogador, nenhuma facção é dela: devolve o líder da própria facção.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_FT2_LeaderFor_NoPlayerFaction
    {
        private static MethodBase target;

        public static bool Prepare()
        {
            Type t = AccessTools.TypeByName("FactionTerritories2.FactionDispositions");
            target = t == null ? null : AccessTools.DeclaredMethod(t, "LeaderFor", new[] { typeof(Faction) });
            return target is MethodInfo m && m.IsStatic && m.ReturnType == typeof(Pawn);
        }

        public static MethodBase TargetMethod() => target;

        public static bool Prefix(Faction faction, ref Pawn __result)
        {
            if (Find.World?.factionManager?.OfPlayer != null)
                return true;
            __result = faction?.leader;
            return false;
        }
    }

    public static class EQThirdPartyFixes
    {
        private static readonly FieldInfo rulesStringsField = AccessTools.Field(typeof(Verse.Grammar.RulePack), "rulesStrings");
        private static readonly FieldInfo rulesResolvedField = AccessTools.Field(typeof(Verse.Grammar.RulePack), "rulesResolved");
        private static readonly FieldInfo rulePackField = AccessTools.Field(typeof(RulePackDef), "rulePack");
        private static readonly FieldInfo cachedRulesField = AccessTools.Field(typeof(RulePackDef), "cachedRules");
        private static readonly System.Text.RegularExpressions.Regex pattern =
            AccessTools.Field(typeof(Verse.Grammar.Rule_String), "Pattern")?.GetValue(null) as System.Text.RegularExpressions.Regex;

        /// <summary>Regras removidas (para o executor de testes listar).</summary>
        public static readonly List<string> Removed = new List<string>();
        public static int Repaired;

        /// <summary>
        /// Traduções oficiais PT-BR com regra de texto que o jogo não lê: a Anomaly pôs "subject_gerúndio->…" em
        /// BioferriteExtraction e a Odyssey tem regras assim em três RulePackDefs (estátua, nome e descrição de mapa).
        /// (entradas vazias/nulas na lista). A regra vira uma regra sem palavra-chave, e qualquer texto que a use quebra (ArgumentNullException em
        /// GrammarResolver.AddRule): tomos da Anomaly e o livro-mapa saíam "sem texto". Aqui saem as regras que o próprio
        /// leitor do jogo (Rule_String.Pattern) rejeita, nos RulePackDefs e nos projetos de pesquisa.
        /// </summary>
        public static void CleanBadRules()
        {
            if (rulesStringsField == null || rulesResolvedField == null || pattern == null)
                return;
            foreach (RulePackDef d in DefDatabase<RulePackDef>.AllDefsListForReading)
                if (Clean(rulePackField?.GetValue(d) as Verse.Grammar.RulePack, d.defName))
                    cachedRulesField?.SetValue(d, null);
            foreach (ResearchProjectDef r in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
                Clean(r.generalRules, r.defName);
            if ((Removed.Count > 0 || Repaired > 0) && Prefs.DevMode)
                Log.Message("[Ecos da Queda] regras de texto da tradução: " + Repaired + " consertada(s) (\">\" no lugar de \"->\"), " + Removed.Count + " removida(s): " + string.Join(" | ", Removed.Take(10)));
        }

        private static bool Clean(Verse.Grammar.RulePack pack, string owner)
        {
            if (pack == null || !(rulesStringsField.GetValue(pack) is List<string> list))
                return false;
            int n = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                string rule = list[i];
                if (!rule.NullOrEmpty() && pattern.IsMatch(rule))
                    continue;
                n++;
                // Odyssey PT-BR: "chave>texto" no lugar de "chave->texto".
                int gt = rule?.IndexOf('>') ?? -1;
                if (gt > 0 && !rule.Contains("->"))
                {
                    string fixedRule = rule.Substring(0, gt) + "->" + rule.Substring(gt + 1);
                    if (pattern.IsMatch(fixedRule))
                    {
                        list[i] = fixedRule;
                        Repaired++;
                        continue;
                    }
                }
                list.RemoveAt(i);
                Removed.Add(owner + ": " + (rule == null ? "(nula)" : rule.Length == 0 ? "(vazia)" : rule.Length > 60 ? rule.Substring(0, 60) : rule));
            }
            if (n > 0)
                rulesResolvedField.SetValue(pack, null);
            return n > 0;
        }
    }
}
