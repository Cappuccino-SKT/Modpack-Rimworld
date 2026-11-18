using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Etapa 5a: "events" lista os eventos de potência e o motivo de cada um não poder aparecer; "event &lt;def&gt;" força um.</summary>
    public static class EQPowerEventsDebug
    {
        public static string List()
        {
            var comp = WorldComponent_PowerEvents.Current;
            if (comp == null) return "sem componente";
            var sb = new StringBuilder();
            foreach (PowerEventDef d in DefDatabase<PowerEventDef>.AllDefsListForReading)
                sb.AppendLine(d.defName + ": " + (comp.BlockReason(d) ?? "pode aparecer") + " | mtb=" + d.mtbDays + "d opções=" + d.options.Count);
            return sb.ToString().TrimEnd();
        }

        public static string Fire(string defName)
        {
            var comp = WorldComponent_PowerEvents.Current;
            PowerEventDef d = DefDatabase<PowerEventDef>.GetNamedSilentFail(defName);
            if (comp == null || d == null) return "evento não encontrado: " + defName;
            string why = comp.BlockReason(d, ignoreTime: true);
            if (why != null) return "BLOQUEADO: " + why;
            comp.Fire(d);
            var l = Find.LetterStack.LettersListForReading.LastOrDefault() as ChoiceLetter_EQPowerEvent;
            if (l == null) return "carta não chegou";
            return "carta \"" + l.Label + "\": " + string.Join(" | ", d.options.Select((o, i) => i + "=" + o.label + (l.CanPay(o) is string s ? " [" + s + "]" : "")));
        }
    }
}
