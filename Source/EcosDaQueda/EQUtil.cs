using RimWorld;
using Verse;

namespace EcosDaQueda
{
    public static class EQUtil
    {
        /// <summary>
        /// Facções "proxy" (ex.: expedições do Hegemony) são cópias ocultas e temporárias com o mesmo FactionDef.
        /// Elas nunca devem receber relações, ideologia ou reputação próprias.
        /// </summary>
        public static bool IsProxy(Faction f) => f != null && (f.temporary || f.Hidden);

        /// <summary>A facção principal (não proxy) de um FactionDef, preferindo a não derrotada.</summary>
        public static Faction MainFaction(FactionDef def)
        {
            if (def == null || Find.FactionManager == null)
                return null;
            Faction fallback = null;
            foreach (Faction f in Find.FactionManager.AllFactionsListForReading)
            {
                if (f.def != def || IsProxy(f))
                    continue;
                if (!f.defeated)
                    return f;
                fallback ??= f;
            }
            return fallback;
        }

        /// <summary>Converte uma facção (talvez proxy) na facção principal do mesmo def.</summary>
        public static Faction Canonical(Faction f)
        {
            if (f == null || !IsProxy(f))
                return f;
            return MainFaction(f.def) ?? f;
        }
    }
}
