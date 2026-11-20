using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// N1b (NACAO.md › 3.1 e 3.3): a ideologia do jogador fica no modo clássico até a fundação da nação; na vitória da
    /// revolta vira fluida e o jogador escolhe os primeiros memes (estrutura + 1, regras da ideologia fluida do jogo).
    ///
    /// Só a ideologia do jogador é clássica (<c>Ideo.classicMode</c>); o modo clássico global
    /// (<c>IdeoManager.classicMode</c>, lido em ~80 pontos do jogo) continua desligado, então as facções NPC seguem com
    /// as ideologias delas. É o mesmo estado que a tela de escolha de ideologia monta ao abrir.
    /// </summary>
    public static class EQNationFaith
    {
        public static Ideo PlayerIdeo => Faction.OfPlayer?.ideos?.PrimaryIdeo;

        public static bool PlayerIdeoClassic => PlayerIdeo?.classicMode == true;

        /// <summary>Último relatório (executor de testes).</summary>
        public static string LastReport;

        /// <summary>O cenário começa com a ideologia clássica? (Hegemony ativo, Ideology ativa, regra do NationDef.)</summary>
        public static bool ClassicStartApplies(Scenario scen)
        {
            NationDef d = NationDef.Active;
            if (d == null || !d.classicStart || !EQNation.Applies || !ModsConfig.IdeologyActive) return false;
            if (d.classicStartAllScenarios) return true;
            ScenarioDef sd = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.scenario == scen);
            return sd?.modContentPack != null && sd.modContentPack.PackageId.ToLowerInvariant().StartsWith("ecosdaqueda");
        }

        /// <summary>Troca a ideologia do jogador por uma clássica nova (testes e jogos já começados). Converte os colonos.</summary>
        public static string MakePlayerClassic()
        {
            Ideo old = PlayerIdeo;
            if (old == null) return "sem ideologia do jogador";
            if (old.classicMode) return "já é clássica";
            Ideo ideo = IdeoGenerator.GenerateClassicIdeo(old.culture, new IdeoGenerationParms(Faction.OfPlayer.def), noExpansionIdeo: false);
            Find.IdeoManager.Add(ideo);
            Faction.OfPlayer.ideos.SetPrimary(ideo);
            int n = 0;
            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
            {
                if (p.ideo == null || p.Ideo == ideo) continue;
                p.ideo.SetIdeo(ideo);
                n++;
            }
            Faction.OfPlayer.ideos.RecalculateIdeosBasedOnPlayerPawns();
            if (!Find.FactionManager.AllFactions.Any(f => f.ideos?.Has(old) == true) && !PawnsFinder.AllMapsWorldAndTemporary_Alive.Any(p => p.Ideo == old))
                Find.IdeoManager.Remove(old);
            return "ideologia clássica " + ideo.name + " (" + n + " convertidos)";
        }

        /// <summary>Na fundação: a ideologia clássica vira fluida e abre a escolha dos memes. Sem janelas (testes): escolhe sozinho.</summary>
        public static void BeginFounding(WorldComponent_EQNation c)
        {
            Ideo ideo = PlayerIdeo;
            if (ideo == null || !ideo.classicMode || !ModsConfig.IdeologyActive) return;
            ideo.classicMode = false;
            ideo.Fluid = true;
            // A clássica não tem lugar de origem; a descrição (regerada pela janela e por Finish) precisa dele.
            try { ideo.foundation.RandomizePlace(); } catch (Exception ex) { Log.Warning("[Ecos da Queda] Fé da nação (lugar): " + ex.Message); }
            c.faithPending = true;
            LastReport = "fé pendente: " + ideo.name + " fluida, sem memes";
            if (EQLeader.SuppressDialogs || Find.WindowStack == null) Finish(c, AutoPick(ideo));
            else OpenChooser(c);
        }

        public static void OpenChooser(WorldComponent_EQNation c)
        {
            Ideo ideo = PlayerIdeo;
            if (ideo == null || Find.WindowStack.IsOpen<Dialog_ChooseMemes>()) return;
            MemeCategory cat = ideo.memes.Any(m => m.category == MemeCategory.Structure) ? MemeCategory.Normal : MemeCategory.Structure;
            Find.WindowStack.Add(new Dialog_ChooseMemes(ideo, cat, initialSelection: true, () => Finish(c, null)));
        }

        /// <summary>Mesmo caminho da janela (Dialog_ChooseMemes.DoAcceptChanges), com memes sorteados. Devolve o que escolheu.</summary>
        public static string AutoPick(Ideo ideo)
        {
            FactionDef forFaction = IdeoUIUtility.FactionForRandomization(ideo);
            List<MemeDef> memes = IdeoUtility.RandomizeStructureMeme(new List<MemeDef>(ideo.memes), forFaction);
            memes = IdeoUtility.RandomizeNormalMemes(1, memes, forFaction, forNewFluidIdeo: true);
            ideo.memes.Clear();
            ideo.memes.AddRange(memes);
            ideo.SortMemesInDisplayOrder();
            ideo.foundation.RandomizePrecepts(init: true, new IdeoGenerationParms(forFaction));
            if (ideo.foundation is IdeoFoundation_Deity deity) deity.GenerateDeities();
            ideo.foundation.RandomizeStyles();
            return string.Join(", ", memes.Select(m => m.defName));
        }

        /// <summary>Fim da escolha: nome, título e ícone da fé nova, o líder proclamado de volta na função, reforma livre.</summary>
        public static void Finish(WorldComponent_EQNation c, string picked)
        {
            Ideo ideo = PlayerIdeo;
            if (ideo == null) return;
            if (!ideo.memes.Any(m => m.category == MemeCategory.Structure) || !ideo.memes.Any(m => m.category == MemeCategory.Normal))
                return; // a janela foi fechada sem terminar; o tick reabre
            c.faithPending = false;
            try
            {
                ideo.foundation.GenerateTextSymbols();
                ideo.foundation.GenerateLeaderTitle();
                ideo.foundation.RandomizeIcon();
                ideo.RegenerateDescription(force: true);
                ideo.style.RecalculateAvailableStyleItems();
                ideo.RecachePrecepts();
            }
            catch (Exception ex) { Log.Error("[Ecos da Queda] Fé da nação: " + ex); }

            // RandomizePrecepts refaz as funções: o líder proclamado volta ao cargo.
            Pawn leader = c.leader;
            bool back = leader != null && !leader.Dead && EQLeader.IdeoLeader != leader && EQLeader.Assign(leader);
            WorldComponent_Allegiance.Current?.LeaderTick();

            NationDef d = NationDef.Active;
            bool reform = d != null && d.freeReformOnFounding && ideo.development != null;
            if (reform) ideo.development.points = ideo.development.NextReformationDevelopmentPoints;

            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep)
                if (p.Ideo == ideo) p.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty();

            LastReport = "fé fundada: " + ideo.name + " memes=" + string.Join(", ", ideo.memes.Select(m => m.defName))
                         + (picked != null ? " (sorteados)" : "") + " fluida=" + ideo.Fluid + " clássica=" + ideo.classicMode
                         + " líder=" + (EQLeader.IdeoLeader?.LabelShort ?? "-") + (back ? " (devolvido)" : "")
                         + " pontos=" + (ideo.development?.points ?? 0) + "/" + (ideo.development?.NextReformationDevelopmentPoints ?? 0);

            Find.LetterStack.ReceiveLetter("A fé da nação",
                "Um país precisa de algo em que acreditar, de preferência por escrito. A colônia agora segue " + ideo.name.Colorize(ideo.TextColor)
                + ", uma ideologia fluida: ganha força com conversões e rituais, e muda nas reformas."
                + (reform ? "\n\nComo a tinta ainda está fresca, a primeira reforma já está liberada (aba Ideologia): é a hora de acertar os preceitos antes que alguém os decore." : ""),
                LetterDefOf.PositiveEvent);
        }
    }

    /// <summary>Cenários do Ecos (com o Hegemony): pula a escolha de ideologia e começa com a clássica só para o jogador.</summary>
    [HarmonyPatch(typeof(Page_ChooseIdeoPreset), nameof(Page_ChooseIdeoPreset.PostOpen))]
    public static class Patch_Page_ChooseIdeoPreset_PostOpen_Classic
    {
        public static void Postfix(Page_ChooseIdeoPreset __instance)
        {
            try
            {
                if (Find.Storyteller?.def?.tutorialMode == true || !EQNationFaith.ClassicStartApplies(Find.Scenario)) return;
                // PostOpen já deu a ideologia clássica ao jogador, gerou as das NPC e deixou o modo clássico global desligado.
                if (!EQNationFaith.PlayerIdeoClassic) return;
                Find.IdeoManager.RemoveUnusedStartingIdeos();
                Find.Scenario.PostIdeoChosen();
                if (__instance.next != null) Find.WindowStack.Add(__instance.next);
                __instance.nextAct?.Invoke();
                TutorSystem.Notify_Event("PageClosed");
                TutorSystem.Notify_Event("GoToNextPage");
                __instance.Close();
            }
            catch (Exception ex) { Log.Error("[Ecos da Queda] Início com ideologia clássica: " + ex); }
        }
    }
}
