using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    [StaticConstructorOnStartup]
    public static class ReputationTextures
    {
        public static readonly Texture2D FameBar = SolidColorMaterials.NewSolidColorTexture(new Color(0.85f, 0.72f, 0.25f));
        public static readonly Texture2D InfamyBar = SolidColorMaterials.NewSolidColorTexture(new Color(0.75f, 0.18f, 0.15f));
        public static readonly Texture2D BarBg = SolidColorMaterials.NewSolidColorTexture(new Color(0.12f, 0.12f, 0.12f));
    }

    /// <summary>Aba "Reputação": Fama, Infâmia, título e relações de cada potência.</summary>
    public class MainTabWindow_Reputation : MainTabWindow
    {
        private Vector2 scroll;
        private const float RowHeight = 140f;

        public override Vector2 RequestedTabSize => new Vector2(860f, 740f);

        public override void DoWindowContents(Rect inRect)
        {
            var s = ReputationSettingsDef.Settings;
            var rep = WorldComponent_Reputation.Current;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), "Reputação com as Potências");
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(new Rect(inRect.x, inRect.y + 34f, inRect.width, 20f),
                "Fama vem de ajudar (presentes, missões, comércio). Infâmia vem de ferir (ataques, mortes, capturas). Inimigos de uma potência gostam de quem a fere.");
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            if (s == null || rep == null)
                return;

            // Painel de lealdade
            Rect allegRect = new Rect(inRect.x, inRect.y + 58f, inRect.width, 112f);
            DrawAllegiance(allegRect);

            Rect outRect = new Rect(inRect.x, allegRect.yMax + 6f, inRect.width, inRect.yMax - allegRect.yMax - 6f);
            Rect view = new Rect(0f, 0f, outRect.width - 16f, s.factions.Count * RowHeight);
            Widgets.BeginScrollView(outRect, ref scroll, view);
            float y = 0f;
            foreach (FactionDef def in s.factions)
            {
                Faction f = EQUtil.MainFaction(def);
                if (f != null)
                    DrawRow(new Rect(0f, y, view.width, RowHeight - 8f), f, rep.Get(f), s);
                y += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private static void DrawAllegiance(Rect r)
        {
            Widgets.DrawMenuSection(r);
            Rect inner = r.ContractedBy(8f);
            var alg = WorldComponent_Allegiance.Current;
            if (alg == null)
                return;
            Pawn leader = EQLeader.IdeoLeader;
            if (alg.memberOf == null)
            {
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(inner.x, inner.y, inner.width - 280f, 24f),
                    "<b>Lealdade:</b> " + (alg.pendingPower != null ? "cerimônia de submissão solicitada com " + alg.pendingPower.Name + "." : "o assentamento não pertence a nenhuma potência."));
                Widgets.Label(new Rect(inner.x, inner.y + 24f, inner.width - 280f, 24f),
                    "<b>Líder:</b> " + (leader != null ? leader.Name.ToStringFull : "nenhum (defina o líder da sua ideologia)"));
                if (leader == null && Widgets.ButtonText(new Rect(inner.xMax - 270f, inner.y, 130f, 28f), "Escolher líder"))
                    Find.WindowStack.Add(new Dialog_EQChooseLeader(forced: false));
                if (alg.pendingPower != null && Widgets.ButtonText(new Rect(inner.xMax - 130f, inner.y, 130f, 28f), "Cancelar pedido"))
                    EQCeremony.Cancel();
                GUI.color = Color.gray;
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(inner.x, inner.y + 50f, inner.width, inner.height - 50f),
                    alg.pendingPower != null
                        ? "Aceite a missão da cerimônia (aba Missões): um emissário virá de nave presidi-la com o líder e recolher o preço."
                        : "Para se submeter a uma potência, use o botão 'Cerimônia' dela abaixo. A submissão tem um preço e é selada numa cerimônia com um emissário. Só o líder do assentamento (o líder da sua ideologia) recebe cargo e favor; sair depois conta como traição.");
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                return;
            }

            FactionAllegianceDef d = alg.Def;
            int rank = alg.RankIndex;
            if (Widgets.ButtonText(new Rect(inner.xMax - 130f, inner.y, 130f, 22f), "Serviços"))
                WorldComponent_Services.OpenMenu();
            if (Widgets.ButtonText(new Rect(inner.xMax - 130f, inner.y + 24f, 130f, 22f), "Licenças"))
                Find.WindowStack.Add(new Dialog_EQLicenses());
            if (Widgets.ButtonText(new Rect(inner.xMax - 130f, inner.y + 48f, 130f, 22f), "Sucessor"))
                Find.WindowStack.Add(new Dialog_EQChooseLeader(forced: false, successorMode: true));
            Rect promo = new Rect(inner.xMax - 266f, inner.y, 130f, 22f);
            if (Widgets.ButtonText(promo, alg.refusePromotion ? "Aceitar promoções" : "Recusar promoções"))
                alg.SetRefusePromotion(!alg.refusePromotion);
            TooltipHandler.TipRegion(promo, alg.refusePromotion
                ? "Promoções recusadas: a potência não marca cerimônias de promoção e o líder fica no cargo atual (com as obrigações dele). O favor continua acumulando."
                : "Recusar promoções: o líder fica no cargo atual e nas obrigações dele; o favor continua acumulando e pode comprar licenças.");
            Rect third = new Rect(inner.xMax - 130f, inner.y + 72f, 130f, 22f);
            if (leader == null)
            {
                if (Widgets.ButtonText(third, "Escolher líder"))
                    Find.WindowStack.Add(new Dialog_EQChooseLeader(forced: false));
            }
            else if (!(EQAllegiance.HasVassalage && alg.hegLinked) && Widgets.ButtonText(third, "Romper"))
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Romper o juramento com " + alg.memberOf.Name + "? O cargo é cassado e isso conta como traição (Infâmia).", () => alg.Renounce(), destructive: true));
            Rect iconRect = new Rect(inner.x, inner.y + 4f, 64f, 64f);
            GUI.color = alg.memberOf.Color;
            GUI.DrawTexture(iconRect, alg.memberOf.def.FactionIcon);
            GUI.color = Color.white;
            float x = iconRect.xMax + 10f;
            float w = inner.width - 350f;
            Text.Font = GameFont.Small;
            string encargo = WorldComponent_Encargos.Current?.Describe();
            string war = WorldComponent_EQForces.Current?.Describe(alg.memberOf);
            Rect lealRect = new Rect(x, inner.y, w, 24f);
            Widgets.Label(lealRect, "<b>Lealdade:</b> " + alg.RankLabel + " de " + alg.memberOf.Name
                + (encargo != null ? "   ·   <b>Encargo:</b> " + encargo : "")
                + (war != null ? "   ·   <b>Em guerra</b>" : ""));
            if (encargo != null || war != null)
                TooltipHandler.TipRegion(lealRect, (encargo != null ? "Andamento: " + WorldComponent_Encargos.Current.ProgressLine() : "")
                    + (encargo != null && war != null ? "\n\n" : "")
                    + (war != null ? alg.memberOf.Name + ": " + war + "." : ""));
            if (d == null || rank < 0)
                return;
            string next = rank + 1 < d.ranks.Count && d.ranks[rank + 1].title != null
                ? " / " + d.ranks[rank + 1].title.favorCost + " para " + d.RankLabel(rank + 1, leader)
                : " (cargo máximo)";
            Widgets.Label(new Rect(x, inner.y + 22f, w, 24f),
                "<b>Líder:</b> " + (leader != null ? leader.Name.ToStringFull : "<color=#ff6666>nenhum</color>")
                + "   ·   <b>" + d.FavorLabel.CapitalizeFirst() + ":</b> " + alg.Favor + next
                + "   ·   <b>Sucessor:</b> " + (alg.successor != null ? alg.successor.LabelShort : "não definido"));
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.8f, 0.8f, 0.6f);
            Widgets.Label(new Rect(x, inner.y + 46f, w, inner.height - 46f),
                WorldComponent_Allegiance.DescribeRank(d, rank).Replace("\n", "   ·   "));
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            TooltipHandler.TipRegion(new Rect(inner.x, inner.y, w + 74f, inner.height), (d.ranks[rank].title?.description ?? "") + "\n\n" + FavorHelp(d));
        }

        private static string FavorHelp(FactionAllegianceDef d)
        {
            var lines = new System.Collections.Generic.List<string> { "Como ganhar " + d.FavorLabel + ":" };
            foreach (FavorSource s in d.favorSources)
                lines.Add("  +" + s.amount + ": " + s.label + (s.per > 1 ? " (a cada " + s.per + ")" : ""));
            foreach (TitheOption o in d.titheOptions)
                if (o.favor > 0)
                    lines.Add("  +" + o.favor + ": " + d.titheLabel + " - " + o.label.Replace("{COUNT}", "N"));
            lines.Add("  Missões da potência e cerimônias bem assistidas também dão " + d.FavorLabel + ".");
            return string.Join("\n", lines);
        }

        private static void DrawCeremonyButton(Rect rect, Faction f)
        {
            FactionAllegianceDef d = FactionAllegianceDef.For(f.def);
            var alg = WorldComponent_Allegiance.Current;
            if (d == null || alg == null || alg.memberOf != null)
                return;
            string block = EQCeremony.BlockReason(f);
            string tip = d.ceremonyLabel + "\n\n" + (d.ceremonyText ?? "") + "\n\nPreço: " + d.PriceText() + (block != null ? "\n\n<color=#ff6666>" + block + "</color>" : "");
            TooltipHandler.TipRegion(rect, tip);
            if (!Widgets.ButtonText(rect, "Cerimônia", active: block == null))
                return;
            if (block != null)
            {
                Messages.Message(block, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                d.ceremonyLabel + " - " + f.Name + "\n\n" + (d.ceremonyText ?? "") + "\n\nPreço, recolhido pelo emissário ao fim da cerimônia: " + d.PriceText()
                + "\n\nAo concluir, o assentamento passa a pertencer a " + f.Name + (EQAllegiance.HasVassalage ? " e vira vassalo dela no Hegemony (com tributo)" : "") + ". Solicitar a cerimônia?",
                () =>
                {
                    string err = EQCeremony.Request(f);
                    if (err != null) Messages.Message(err, MessageTypeDefOf.RejectInput, historical: false);
                }));
        }

        private static void DrawRow(Rect r, Faction f, ReputationRecord rec, ReputationSettingsDef s)
        {
            Widgets.DrawMenuSection(r);
            Rect inner = r.ContractedBy(8f);

            // Ícone
            Rect iconRect = new Rect(inner.x, inner.y + 8f, 72f, 72f);
            GUI.color = f.Color;
            GUI.DrawTexture(iconRect, f.def.FactionIcon);
            GUI.color = Color.white;

            float x = iconRect.xMax + 12f;
            float w = inner.xMax - x;

            // Nome, título, relação
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(x, inner.y, w * 0.55f, 30f), f.Name);
            Text.Font = GameFont.Small;
            ReputationTier tier = s.Tier(rec.fame, rec.infamy);
            DrawCeremonyButton(new Rect(inner.x - 4f, inner.y + 84f, 80f, 26f), f);
            Rect titleRect = new Rect(x + w * 0.55f, inner.y + 4f, w * 0.45f, 24f);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(titleRect, "<b>" + (tier?.label ?? "?") + "</b>");
            Text.Anchor = TextAnchor.UpperLeft;
            if (tier != null)
                TooltipHandler.TipRegion(titleRect, tier.description + "\n\n" + Effects(tier));

            int gw = f.GoodwillWith(Faction.OfPlayer);
            GUI.color = Color.gray;
            Widgets.Label(new Rect(x, inner.y + 30f, w, 22f),
                "Relação com você: " + gw.ToStringWithSign() + " (" + f.RelationKindWith(Faction.OfPlayer).GetLabel() + ")" + Relations(f));
            GUI.color = Color.white;

            // Barras
            float max = s.thresholds.Count > 0 ? s.thresholds[s.thresholds.Count - 1] * 2f : 300f;
            DrawBar(new Rect(x, inner.y + 56f, w, 18f), "Fama", rec.fame, max, ReputationTextures.FameBar);
            DrawBar(new Rect(x, inner.y + 78f, w, 18f), "Infâmia", rec.infamy, max, ReputationTextures.InfamyBar);

            // Efeitos do título
            if (tier != null)
            {
                Text.Font = GameFont.Tiny;
                GUI.color = new Color(0.8f, 0.8f, 0.6f);
                Widgets.Label(new Rect(x, inner.y + 100f, w, 20f), Effects(tier).Replace("\n", "   ·   "));
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
            }
        }

        private static string Effects(ReputationTier t)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (t.priceGain != 0f)
                parts.Add("Preços: " + (t.priceGain > 0 ? "+" : "") + Mathf.RoundToInt(t.priceGain * 100f) + "%");
            if (t.giftChance > 0f) parts.Add("Envia presentes");
            if (t.tributeChance > 0f) parts.Add("Paga tributo para evitar problemas");
            if (t.raidChance > 0f) parts.Add("Se hostil: caçadores de recompensa");
            if (parts.Count == 0) parts.Add("Sem efeitos");
            return string.Join("\n", parts);
        }

        private static string Relations(Faction f)
        {
            string text = "";
            foreach (Faction other in Find.FactionManager.AllFactionsListForReading)
            {
                if (other == f || !WorldComponent_Reputation.Tracks(other)) continue;
                FactionRelationEntry e = FactionRelationPresetDef.Find(f.def, other.def);
                if (e == null || e.label.NullOrEmpty()) continue;
                text += (text.Length == 0 ? "   ·   " : ", ") + e.label + " com " + other.Name;
            }
            return text;
        }

        private static void DrawBar(Rect r, string label, float value, float max, Texture2D tex)
        {
            Rect labelRect = new Rect(r.x, r.y, 70f, r.height);
            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, label);
            Rect bar = new Rect(r.x + 70f, r.y + 2f, r.width - 130f, r.height - 4f);
            Widgets.FillableBar(bar, Mathf.Clamp01(value / max), tex, ReputationTextures.BarBg, true);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(new Rect(bar.xMax, r.y, 60f, r.height), Mathf.RoundToInt(value).ToString());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }
    }
}
