using System;
using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Etapa 11b: "encargo [offer | report | end | progress &lt;tipo&gt; &lt;n&gt; [qualidade] | skip &lt;dias&gt;]".
    /// Sem argumento: estado (encargo, período, andamento, faltas, carta pendente).</summary>
    public static class EQEncargosDebug
    {
        public static string Run(string sub, string a2, string a3, string a4)
        {
            var comp = WorldComponent_Encargos.Current;
            var alg = WorldComponent_Allegiance.Current;
            if (comp == null) return "sem componente";
            string extra = "";
            switch (sub)
            {
                case "offer":
                {
                    EncargoDef d = EncargoDef.For(alg?.memberOf);
                    if (d == null) return "sem encargo para a potência atual";
                    string block = comp.OfferBlock(d, ignoreTime: true);
                    if (block != null) return "bloqueado: " + block;
                    comp.Offer(d);
                    extra = "oferta enviada: " + d.defName;
                    break;
                }
                case "report":
                    extra = "relatório: " + comp.Report();
                    break;
                case "end":
                    comp.End("teste", penalize: a2 == "penalize");
                    extra = "encerrado";
                    break;
                case "progress":
                {
                    var type = (FavorEventType)Enum.Parse(typeof(FavorEventType), a2, true);
                    var q = a4 == null ? QualityCategory.Legendary : (QualityCategory)Enum.Parse(typeof(QualityCategory), a4, true);
                    EQFavor.Notify(type, int.Parse(a3 ?? "1"), q);
                    extra = "notificado " + type + " x" + (a3 ?? "1");
                    break;
                }
                case "skip":
                    if (comp.active != null) comp.active.periodStart -= (int)(float.Parse(a2 ?? "1", System.Globalization.CultureInfo.InvariantCulture) * 60000);
                    extra = "período adiantado";
                    break;
            }
            var st = comp.active;
            int strikes = WorldComponent_PowerEvents.Current?.StrikesWith(alg?.memberOf) ?? 0;
            bool pending = comp.OfferPending;
            return (extra == "" ? "" : extra + " | ")
                + "membro=" + (alg?.memberOf?.Name ?? "-") + " cargo=" + (alg?.memberOf != null ? (alg.RankIndex + 1).ToString() : "-")
                + " favor=" + (alg?.Favor ?? 0) + " faltas=" + strikes + " oferta_pendente=" + pending
                + (st == null ? " encargo=nenhum"
                    : " encargo=" + st.def.defName + " periodos=" + st.periods + " cumpridos=" + st.successes + " descumpridos=" + st.fails
                      + " fim_do_periodo_dia=" + (st.PeriodEnd / 60000f).ToString("0.0") + " andamento=[" + comp.ProgressLine() + "]");
        }
    }
}
