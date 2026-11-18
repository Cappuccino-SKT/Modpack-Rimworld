using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Etapa 4c: "ritualoutcome &lt;def&gt; [escala]" aplica o resultado anexável de potência como se o ritual tivesse terminado.</summary>
    public static class EQRitualDebug
    {
        public static string Run(string defName, string scale)
        {
            var d = DefDatabase<RitualAttachableOutcomeEffectDef>.GetNamedSilentFail(defName ?? "");
            if (d == null)
                return "resultados EQ: " + string.Join(", ", DefDatabase<RitualAttachableOutcomeEffectDef>.AllDefs.Where(x => x.defName.StartsWith("EQ_")).Select(x => x.defName));
            Map map = Find.AnyPlayerHomeMap;
            IntVec3 spot = map.mapPawns.FreeColonistsSpawned.FirstOrDefault()?.Position ?? map.Center;
            string text = EQRitualOutcomes.Grant(d, map, spot, scale == null ? 1f : float.Parse(scale, System.Globalization.CultureInfo.InvariantCulture));
            return "aplicado " + d.defName + ": " + (text ?? "(nada: potência ausente)");
        }
    }
}
