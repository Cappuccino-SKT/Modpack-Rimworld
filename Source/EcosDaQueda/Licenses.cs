using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Bloco 3b: licenças de tecnologia das potências. Cada projeto de um ramo com dona (ResearchBranchDef.faction) é
    /// licenciado por um cargo dela (por nível tecnológico, com exceções no XML); membros compram licenças acima do
    /// cargo com favor. Licença dá acesso à pesquisa e dispensa techprints. Projeto conhecido sem licença (perdeu o
    /// cargo, rompeu, sucata, dados antigos) continua conhecido, mas fabricar ou construir com ele é pirataria.
    /// </summary>
    public static class EQLicenses
    {
        public enum Source { None, Title, Purchased }

        public static ResearchBranchDef OwnedBranch(ResearchProjectDef r)
        {
            ResearchBranchDef b = EQResearch.Branch(r);
            return b != null && !b.free && b.faction != null ? b : null;
        }

        public static int DefaultRank(TechLevel t)
        {
            switch (t)
            {
                case TechLevel.Animal:
                case TechLevel.Neolithic:
                case TechLevel.Medieval: return 0;
                case TechLevel.Industrial: return 1;
                case TechLevel.Spacer: return 3;
                case TechLevel.Ultra:
                case TechLevel.Archotech: return 5;
                default: return 2; // Undefined (Anomaly)
            }
        }

        /// <summary>Índice do cargo que licencia o projeto (-1 = não é de potência).</summary>
        public static int RequiredRank(ResearchProjectDef r)
        {
            ResearchBranchDef b = OwnedBranch(r);
            if (b == null)
                return -1;
            LicenseRankOverride o = b.rankOverrides.FirstOrDefault(x => x.project == r.defName);
            int rank = o != null ? o.rank : (b.licenseRanks.FirstOrDefault(x => x.techLevel == r.techLevel)?.rank ?? DefaultRank(r.techLevel));
            FactionAllegianceDef alg = FactionAllegianceDef.For(b.faction);
            if (alg != null && alg.ranks.Count > 0)
                rank = Mathf.Clamp(rank, 0, alg.ranks.Count - 1);
            return rank;
        }

        public static string RequiredTitleLabel(ResearchProjectDef r)
        {
            ResearchBranchDef b = OwnedBranch(r);
            return b == null ? "?" : EQAndroids.LicenseTitleLabel(b.faction, RequiredRank(r));
        }

        /// <summary>O jogador é membro da potência dona do ramo?</summary>
        public static bool IsMemberOf(FactionDef f)
        {
            WorldComponent_Allegiance alg = WorldComponent_Allegiance.Current;
            return alg?.memberOf != null && f != null && alg.memberOf.def == f;
        }

        public static Source LicenseSource(ResearchProjectDef r)
        {
            ResearchBranchDef b = OwnedBranch(r);
            if (b == null || Current.ProgramState != ProgramState.Playing)
                return Source.None;
            if (EQAndroids.HasLicense(b.faction, RequiredRank(r)))
                return Source.Title;
            WorldComponent_EQResearch comp = WorldComponent_EQResearch.Current;
            if (comp != null && comp.IsPurchased(r) && IsMemberOf(b.faction))
                return Source.Purchased;
            return Source.None;
        }

        public static bool IsLicensed(ResearchProjectDef r) => LicenseSource(r) != Source.None;

        public static int FavorPrice(ResearchProjectDef r)
        {
            ResearchBranchDef b = OwnedBranch(r);
            if (b == null)
                return 0;
            float cost = r.knowledgeCategory != null ? r.knowledgeCost * 300f : r.baseCost;
            int price = Mathf.Clamp(Mathf.CeilToInt(cost / Mathf.Max(1f, b.costPerFavor)), b.minFavor, Mathf.Max(b.minFavor, b.maxFavor));
            WorldComponent_Allegiance alg = WorldComponent_Allegiance.Current;
            int cur = alg != null && IsMemberOf(b.faction) ? alg.RankIndex : -1;
            return price + b.favorPerRankAbove * Mathf.Max(0, RequiredRank(r) - cur);
        }

        /// <summary>Motivo de não poder comprar (null = pode).</summary>
        public static string CannotBuyReason(ResearchProjectDef r)
        {
            ResearchBranchDef b = OwnedBranch(r);
            if (b == null)
                return "não é patente de nenhuma potência";
            if (!IsMemberOf(b.faction))
                return "só membros " + (b.labelOf ?? b.label) + " compram licenças";
            if (IsLicensed(r))
                return "já licenciado";
            WorldComponent_Allegiance alg = WorldComponent_Allegiance.Current;
            if (alg.leader?.royalty == null)
                return "sem líder para pagar o favor";
            int price = FavorPrice(r);
            if (alg.Favor < price)
                return "favor insuficiente (" + alg.Favor + "/" + price + ")";
            return null;
        }

        public static bool TryBuy(ResearchProjectDef r, out string result)
        {
            result = CannotBuyReason(r);
            if (result != null)
                return false;
            WorldComponent_Allegiance alg = WorldComponent_Allegiance.Current;
            int price = FavorPrice(r);
            if (!alg.leader.royalty.TryRemoveFavor(alg.memberOf, price))
            {
                result = "o favor não pôde ser cobrado";
                return false;
            }
            WorldComponent_EQResearch.Current.Purchase(r);
            result = "Licença comprada: " + r.LabelCap + " (" + price + " " + alg.Def.FavorLabel + ").";
            return true;
        }

        // ------------------------------------------------------------------ pirataria

        /// <summary>Algo foi fabricado ou construído na colônia com estes pré-requisitos. Devolve a potência lesada (ou null).</summary>
        public static FactionDef OnMade(IEnumerable<ResearchProjectDef> prereqs, string itemLabel, GlobalTargetInfo target)
        {
            if (prereqs == null || Current.ProgramState != ProgramState.Playing)
                return null;
            WorldComponent_EQResearch comp = WorldComponent_EQResearch.Current;
            if (comp == null)
                return null;
            foreach (ResearchProjectDef r in prereqs)
            {
                ResearchBranchDef b = r == null ? null : OwnedBranch(r);
                if (b == null || b.piracyExempt.Contains(r.defName) || IsLicensed(r))
                    continue;
                Faction lic = EQUtil.MainFaction(b.faction);
                if (lic == null || lic.defeated || lic == Faction.OfPlayer)
                    continue;
                int now = Find.TickManager.TicksGame;
                if (now - comp.LastPiracyTick(b.faction) < b.piracyCooldownHours * GenDate.TicksPerHour)
                    return b.faction; // já cobrado hoje: conta, mas não pune de novo
                comp.SetLastPiracy(b.faction, now);
                if (!b.piracyText.NullOrEmpty())
                    Messages.Message(b.piracyText.Formatted(lic.Named("FACTION"), itemLabel.Named("ITEM"), r.LabelCap.Named("PROJECT"),
                        RequiredTitleLabel(r).Named("TITLE")), target.IsValid ? new LookTargets(target) : LookTargets.Invalid, MessageTypeDefOf.NegativeEvent);
                if (b.piracyGoodwill != 0)
                    Faction.OfPlayer.TryAffectGoodwillWith(lic, b.piracyGoodwill, canSendMessage: true, canSendHostilityLetter: true);
                return b.faction;
            }
            return null;
        }

        public static IEnumerable<ResearchProjectDef> RecipePrereqs(RecipeDef recipe)
        {
            if (recipe == null)
                yield break;
            if (recipe.researchPrerequisite != null)
                yield return recipe.researchPrerequisite;
            if (recipe.researchPrerequisites != null)
                foreach (ResearchProjectDef r in recipe.researchPrerequisites)
                    yield return r;
        }
    }

    /// <summary>Sem o Império ninguém vende techprints: o acesso (licença, sucata, dados) dispensa.</summary>
    [HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.TechprintRequirementMet), MethodType.Getter)]
    public static class Patch_ResearchProjectDef_TechprintRequirementMet
    {
        public static void Postfix(ResearchProjectDef __instance, ref bool __result)
        {
            if (!__result && EQResearch.Branch(__instance) != null && EQResearch.HasAccess(__instance))
                __result = true;
        }
    }

    [HarmonyPatch(typeof(Bill_Production), nameof(Bill_Production.Notify_IterationCompleted))]
    public static class Patch_Bill_Production_Piracy
    {
        public static void Postfix(Bill_Production __instance, Pawn billDoer)
        {
            if (billDoer?.Faction == null || !billDoer.Faction.IsPlayer)
                return;
            RecipeDef rec = __instance.recipe;
            string label = rec.ProducedThingDef?.label ?? rec.label;
            EQLicenses.OnMade(EQLicenses.RecipePrereqs(rec), label, new GlobalTargetInfo(billDoer));
        }
    }

    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public static class Patch_Frame_Piracy
    {
        public static void Prefix(Frame __instance, Pawn worker)
        {
            if (worker?.Faction == null || !worker.Faction.IsPlayer)
                return;
            BuildableDef built = __instance.def.entityDefToBuild;
            if (built?.researchPrerequisites == null)
                return;
            EQLicenses.OnMade(built.researchPrerequisites, built.label, new GlobalTargetInfo(__instance.Position, __instance.Map));
        }
    }

    /// <summary>Licenças da potência do jogador: cargo exigido, situação e compra com favor.</summary>
    public class Dialog_EQLicenses : Window
    {
        private Vector2 scroll;
        private bool onlyBuyable;
        private string filter = "";

        public override Vector2 InitialSize => new Vector2(820f, 680f);

        public Dialog_EQLicenses()
        {
            doCloseX = true;
            doCloseButton = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            WorldComponent_Allegiance alg = WorldComponent_Allegiance.Current;
            Faction power = alg?.memberOf;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), "Licenças de tecnologia");
            Text.Font = GameFont.Small;
            if (power == null)
            {
                Widgets.Label(new Rect(inRect.x, inRect.y + 40f, inRect.width, 60f), "O assentamento não pertence a nenhuma potência.");
                return;
            }
            ResearchBranchDef branch = DefDatabase<ResearchBranchDef>.AllDefsListForReading.FirstOrDefault(b => b.faction == power.def);
            FactionAllegianceDef d = alg.Def;
            Widgets.Label(new Rect(inRect.x, inRect.y + 36f, inRect.width, 24f),
                "<b>" + power.Name + "</b>   ·   cargo: " + alg.RankLabel + "   ·   " + d.FavorLabel + ": " + alg.Favor);
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(inRect.x, inRect.y + 58f, inRect.width, 32f),
                "O cargo licencia de graça os projetos do ramo até o nível dele; os de cima se compram com favor (valem enquanto você for membro). "
                + "A licença abre a pesquisa e dispensa techprints. Fabricar ou construir com projeto sem licença é pirataria.");
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(inRect.x, inRect.y + 92f, 220f, 24f), "Só os que dá para comprar", ref onlyBuyable);
            filter = Widgets.TextField(new Rect(inRect.x + 240f, inRect.y + 92f, 220f, 24f), filter);
            if (branch == null)
                return;

            List<ResearchProjectDef> list = EQResearch.BranchOf.Where(kv => kv.Value == branch).Select(kv => kv.Key)
                .Where(r => filter.NullOrEmpty() || r.label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(r => !onlyBuyable || EQLicenses.CannotBuyReason(r) == null)
                .OrderBy(EQLicenses.RequiredRank).ThenBy(r => r.label).ToList();
            Rect outRect = new Rect(inRect.x, inRect.y + 122f, inRect.width, inRect.height - 122f - 50f);
            Rect view = new Rect(0f, 0f, outRect.width - 16f, list.Count * 30f);
            Widgets.BeginScrollView(outRect, ref scroll, view);
            float y = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                ResearchProjectDef r = list[i];
                Rect row = new Rect(0f, y, view.width, 28f);
                if (i % 2 == 0) Widgets.DrawLightHighlight(row);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(row.x + 4f, row.y, 270f, row.height), r.LabelCap.Truncate(266f));
                Widgets.Label(new Rect(row.x + 278f, row.y, 160f, row.height), EQLicenses.RequiredTitleLabel(r).CapitalizeFirst().Truncate(156f));
                EQLicenses.Source src = EQLicenses.LicenseSource(r);
                string state = src == EQLicenses.Source.Title ? "<color=#88cc88>licença do cargo</color>"
                    : src == EQLicenses.Source.Purchased ? "<color=#88cc88>licença comprada</color>"
                    : r.IsFinished ? "<color=#ff7777>conhecido, sem licença</color>"
                    : "sem licença";
                if (r.IsFinished && src != EQLicenses.Source.None) state += " · concluído";
                Widgets.Label(new Rect(row.x + 442f, row.y, 190f, row.height), state);
                Text.Anchor = TextAnchor.UpperLeft;
                if (src == EQLicenses.Source.None)
                {
                    int price = EQLicenses.FavorPrice(r);
                    string why = EQLicenses.CannotBuyReason(r);
                    Rect btn = new Rect(row.xMax - 150f, row.y + 2f, 146f, 24f);
                    if (why == null)
                    {
                        if (Widgets.ButtonText(btn, "Comprar (" + price + ")"))
                        {
                            ResearchProjectDef target = r;
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                                "Comprar a licença de " + r.LabelCap + " por " + price + " " + d.FavorLabel + "?",
                                () => { EQLicenses.TryBuy(target, out string res); Messages.Message(res, MessageTypeDefOf.PositiveEvent, false); }));
                        }
                    }
                    else
                    {
                        GUI.color = Color.gray;
                        Text.Anchor = TextAnchor.MiddleRight;
                        Widgets.Label(btn, price + " " + d.FavorLabel);
                        Text.Anchor = TextAnchor.UpperLeft;
                        GUI.color = Color.white;
                        TooltipHandler.TipRegion(btn, why);
                    }
                }
                TooltipHandler.TipRegion(new Rect(row.x, row.y, 640f, row.height), r.description);
                y += 30f;
            }
            Widgets.EndScrollView();
        }
    }
}
