using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Ações no menu de debug do jogo (modo desenvolvedor), categoria "Ecos da Queda".</summary>
    public static class DebugActions
    {
        private const string Cat = "Ecos da Queda";

        private static void Say(string text)
        {
            Log.Message("[EQ Debug] " + text);
            Messages.Message(text.Split('\n')[0], MessageTypeDefOf.NeutralEvent, false);
        }

        /// <summary>Abre uma lista com as 4 potências e executa a ação na escolhida.</summary>
        private static void PickPower(Func<Faction, string> action, bool withNone = false)
        {
            var options = new List<DebugMenuOption>();
            if (withNone)
                options.Add(new DebugMenuOption("(nenhuma)", DebugMenuOptionMode.Action, () => Say(action(null))));
            foreach (Faction f in EQDebug.Powers)
            {
                Faction faction = f;
                options.Add(new DebugMenuOption(f.Name, DebugMenuOptionMode.Action, () => Say(action(faction))));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction(Cat, "Fama +50...", allowedGameStates = AllowedGameStates.Playing)]
        private static void AddFame() => PickPower(f => EQDebug.AddReputation(f, 50f, 0f));

        [DebugAction(Cat, "Infâmia +50...", allowedGameStates = AllowedGameStates.Playing)]
        private static void AddInfamy() => PickPower(f => EQDebug.AddReputation(f, 0f, 50f));

        [DebugAction(Cat, "Zerar reputação...", allowedGameStates = AllowedGameStates.Playing)]
        private static void ResetRep() => PickPower(f => { EQDebug.SetReputation(f, true, 0f); return EQDebug.SetReputation(f, false, 0f); });

        [DebugAction(Cat, "Forçar lealdade a...", allowedGameStates = AllowedGameStates.Playing)]
        private static void ForceJoin() => PickPower(f => EQDebug.Join(f), withNone: true);

        [DebugAction(Cat, "Lealdade: voltar ao Hegemony", allowedGameStates = AllowedGameStates.Playing)]
        private static void RestoreJoin() => Say(EQDebug.Join(null, restore: true));

        [DebugAction(Cat, "Vencer estipêndio agora", allowedGameStates = AllowedGameStates.Playing)]
        private static void DueStipend() => Say(EQDebug.MakeDue(false));

        [DebugAction(Cat, "Vencer dízimo agora", allowedGameStates = AllowedGameStates.Playing)]
        private static void DueTithe() => Say(EQDebug.MakeDue(true));

        [DebugAction(Cat, "Tornar hostil...", allowedGameStates = AllowedGameStates.Playing)]
        private static void MakeHostile() => PickPower(f => EQDebug.SetHostile(f, true));

        [DebugAction(Cat, "Tornar neutra...", allowedGameStates = AllowedGameStates.Playing)]
        private static void MakeNeutral() => PickPower(f => EQDebug.SetHostile(f, false));

        [DebugAction(Cat, "Forçar retaliação de...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Retaliate() => PickPower(f => EQDebug.Retaliate(f));

        [DebugAction(Cat, "Forçar presente de...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Gift() => PickPower(f => EQDebug.SendGift(f, false));

        [DebugAction(Cat, "Forçar tributo de...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Tribute() => PickPower(f => EQDebug.SendGift(f, true));

        [DebugAction(Cat, "Matar um membro de...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Kill() => PickPower(f => EQDebug.KillMember(f));

        [DebugAction(Cat, "Dar 1 Soro", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GiveSerum() => Say(EQDebug.Give("EQ_Soro", 1));

        [DebugAction(Cat, "Dar 2000 de prata", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GiveSilver() => Say(EQDebug.Give("Silver", 2000));

        [DebugAction(Cat, "Criar prisioneiro", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Prisoner() => Say(EQDebug.SpawnPrisoner());

        [DebugAction(Cat, "Zerar recarga dos serviços", allowedGameStates = AllowedGameStates.Playing)]
        private static void ResetCooldowns()
        {
            var svc = WorldComponent_Services.Current;
            var field = HarmonyLib.AccessTools.Field(typeof(WorldComponent_Services), "lastUsed");
            (field.GetValue(svc) as Dictionary<string, int>)?.Clear();
            Say("recargas zeradas");
        }

        [DebugAction(Cat, "Relatório no log (dump)", allowedGameStates = AllowedGameStates.Playing)]
        private static void Dump()
        {
            Log.Message("[EQ Debug] Relatório:\n" + EQDebug.Dump() + "\n" + EQDebug.Services() + "\n" + EQDebug.Letters());
            Messages.Message("Relatório do Ecos da Queda gravado no log.", MessageTypeDefOf.NeutralEvent, false);
        }
    }
}
