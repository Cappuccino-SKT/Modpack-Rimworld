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
            Rect allegRect = new Rect(inRect.x, inRect.y + 58f, inRect.width, 96f);
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
            if (alg?.memberOf == null)
            {
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(inner.x, inner.y, inner.width, 24f), "<b>Lealdade:</b> você não pertence a nenhuma potência.");
                GUI.color = Color.gray;
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(inner.x, inner.y + 26f, inner.width, inner.height - 26f),
                    EQAllegiance.PlayerOverlordProvider == null
                        ? "Para entrar numa potência é preciso o Hegemony ativo."
                        : "Para entrar numa potência, jure lealdade a ela pelo Hegemony (diplomacia da facção › tornar-se vassalo). Você receberá um posto, um estipêndio periódico e obrigações (dízimos). Subir de posto exige Fama com ela. Sair depois conta como traição.");
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                return;
            }

            FactionAllegianceDef d = alg.Def;
            int rank = alg.RankIndex;
            Rect btn = new Rect(inner.xMax - 130f, inner.y, 130f, 28f);
            if (Widgets.ButtonText(btn, "Serviços"))
                WorldComponent_Services.OpenMenu();
            Rect iconRect = new Rect(inner.x, inner.y + 4f, 64f, 64f);
            GUI.color = alg.memberOf.Color;
            GUI.DrawTexture(iconRect, alg.memberOf.def.FactionIcon);
            GUI.color = Color.white;
            float x = iconRect.xMax + 10f;
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(x, inner.y, inner.width - 214f, 24f),
                "<b>Lealdade:</b> " + (rank >= 0 ? d.ranks[rank].label : "?") + " de " + alg.memberOf.Name);
            if (d != null && rank >= 0)
            {
                Text.Font = GameFont.Tiny;
                GUI.color = new Color(0.8f, 0.8f, 0.6f);
                Widgets.Label(new Rect(x, inner.y + 24f, inner.width - 74f, inner.height - 24f),
                    WorldComponent_Allegiance.DescribeRank(d, rank).Replace("\n", "   ·   "));
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                TooltipHandler.TipRegion(r, d.ranks[rank].description);
            }
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
