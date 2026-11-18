using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Bloco 5g (parte de jogo): nas missões com patrona (EQ_Missions.xml), quem pede é o líder da potência patrona em
    /// vez de "qualquer líder de facção". Vale enquanto a patrona não for hostil ao jogador e tiver líder; senão fica o
    /// sorteio original do mod. As variantes sem pedinte (sinal de rádio interceptado) continuam possíveis.
    /// Três caminhos: VQE Ancients (QuestNode_Site.FindAsker), Drone Factory (sorteio de facção dentro do RunInt das
    /// raízes) e The Generator (QuestNode_GetPawn do jogo com storeAs "asker").
    /// </summary>
    public static class EQQuestAskers
    {
        /// <summary>Contagem para o executor de testes.</summary>
        public static int Applied;
        public static string LastReport = "-";

        /// <summary>A potência patrona da missão em geração, se puder pedir (não hostil, com líder).</summary>
        public static Faction Patron()
        {
            QuestScriptDef root = QuestGen.Root;
            QuestPatronDef d = QuestPatronDef.For(root);
            if (d == null)
                return null;
            Faction f = EQUtil.MainFaction(d.faction);
            if (f == null || f.defeated || f.leader == null || f.leader.Dead || f.HostileTo(Faction.OfPlayer))
                return null;
            return f;
        }

        public static void Note(Faction f, string via)
        {
            Applied++;
            LastReport = (QuestGen.Root?.defName ?? "?") + " -> " + f.def.defName + " (" + via + ")";
        }

        private static Faction Prefer(IEnumerable<Faction> candidates)
        {
            Faction p = Patron();
            if (p != null && candidates.Contains(p))
            {
                Note(p, "sorteio de facção");
                return p;
            }
            return null;
        }

        /// <summary>Substitui GenCollection.RandomElement&lt;Faction&gt; nas raízes do Drone Factory.</summary>
        public static Faction RandomElement(IEnumerable<Faction> source) => Prefer(source) ?? source.RandomElement();

        /// <summary>Substitui GenCollection.RandomElementWithFallback&lt;Faction&gt; nas raízes do Drone Factory.</summary>
        public static Faction RandomElementWithFallback(IEnumerable<Faction> source, Faction fallback) => Prefer(source) ?? source.RandomElementWithFallback(fallback);
    }

    /// <summary>VQE Ancients: FindAsker sorteia (50%) um líder qualquer; se sorteou alguém, vira o líder da patrona.</summary>
    [HarmonyPatch]
    public static class Patch_VQEAncients_FindAsker
    {
        public static bool Prepare() => TargetMethod() != null;

        public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("VanillaQuestsExpandedAncients.QuestNode_Site"), "FindAsker");

        public static void Postfix(ref Pawn __result)
        {
            if (__result == null)
                return;
            Faction p = EQQuestAskers.Patron();
            if (p == null)
                return;
            __result = p.leader;
            EQQuestAskers.Note(p, "FindAsker");
        }
    }

    /// <summary>VQE Drone Factory: troca o sorteio de facção do RunInt das raízes pelo nosso (prefere a patrona se ela estiver entre as candidatas).</summary>
    [HarmonyPatch]
    public static class Patch_VQEDrones_AskerFaction
    {
        private static readonly string[] Roots = { "VanillaQuestsExpandedDroneFactory.QuestNode_Root_DroneScrapyard", "VanillaQuestsExpandedDroneFactory.QuestNode_Root_DroneFactory" };

        public static bool Prepare() => TargetMethods().Any();

        public static IEnumerable<MethodBase> TargetMethods()
            => Roots.Select(AccessTools.TypeByName).Where(t => t != null).Select(t => AccessTools.DeclaredMethod(t, "RunInt")).Where(m => m != null);

        /// <summary>Chamada a GenCollection.&lt;nome&gt;&lt;Faction&gt;(IEnumerable&lt;Faction&gt;, ...) com <paramref name="args"/> parâmetros.</summary>
        private static bool IsFactionPick(CodeInstruction ci, string name, int args)
        {
            if (!(ci.operand is MethodInfo m) || m.DeclaringType != typeof(GenCollection) || m.Name != name || !m.IsGenericMethod)
                return false;
            ParameterInfo[] ps = m.GetParameters();
            return m.GetGenericArguments()[0] == typeof(Faction) && ps.Length == args && ps[0].ParameterType == typeof(IEnumerable<Faction>);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo ours = AccessTools.Method(typeof(EQQuestAskers), nameof(EQQuestAskers.RandomElement));
            MethodInfo oursFb = AccessTools.Method(typeof(EQQuestAskers), nameof(EQQuestAskers.RandomElementWithFallback));
            foreach (CodeInstruction ci in instructions)
            {
                if (IsFactionPick(ci, nameof(GenCollection.RandomElement), 1))
                    yield return new CodeInstruction(OpCodes.Call, ours).MoveLabelsFrom(ci).MoveBlocksFrom(ci);
                else if (IsFactionPick(ci, nameof(GenCollection.RandomElementWithFallback), 2))
                    yield return new CodeInstruction(OpCodes.Call, oursFb).MoveLabelsFrom(ci).MoveBlocksFrom(ci);
                else
                    yield return ci;
            }
        }
    }

    /// <summary>The Generator (e qualquer missão patrona que use o nó do jogo): QuestNode_GetPawn com storeAs "asker" vira o líder da patrona.</summary>
    public static class EQGetPawnAsker
    {
        public static bool TrySet(QuestNode_GetPawn node, Slate slate)
        {
            if (slate == null || node.storeAs.GetValue(slate) != "asker")
                return false;
            Faction p = EQQuestAskers.Patron();
            if (p == null)
                return false;
            slate.Set("asker", p.leader);
            EQQuestAskers.Note(p, "QuestNode_GetPawn");
            return true;
        }
    }

    [HarmonyPatch(typeof(QuestNode_GetPawn), "RunInt")]
    public static class Patch_QuestNode_GetPawn_RunInt_Asker
    {
        public static bool Prefix(QuestNode_GetPawn __instance) => !EQGetPawnAsker.TrySet(__instance, QuestGen.slate);
    }

    [HarmonyPatch(typeof(QuestNode_GetPawn), "TestRunInt")]
    public static class Patch_QuestNode_GetPawn_TestRunInt_Asker
    {
        public static bool Prefix(QuestNode_GetPawn __instance, Slate slate, ref bool __result)
        {
            if (!EQGetPawnAsker.TrySet(__instance, slate))
                return true;
            __result = true;
            return false;
        }
    }
}
