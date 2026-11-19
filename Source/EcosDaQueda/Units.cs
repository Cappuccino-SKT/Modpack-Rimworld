using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Elenco de unidades de uma facção montado no editor de kits (EcosDaQueda-Dev/kits, aba Unidades), gerado em
    /// Defs/Kits/EQ_Unidades.xml junto com os PawnKindDefs (EQ_U_*). Na carga, as unidades entram nos grupos da
    /// facção (combate, pacífico, assentamento, guarda e mercador de caravana) com o peso escolhido. Com
    /// <see cref="replaceExisting"/>, cada grupo que tiver alguma unidade do editor perde as unidades antigas
    /// (grupos sem nenhuma unidade do editor ficam como estão, para a facção nunca ficar sem gente).
    /// </summary>
    public class UnitRosterDef : Def
    {
        public FactionDef faction;
        public bool replaceExisting;
        public List<UnitGroupEntry> entries = new List<UnitGroupEntry>();
        public List<PawnKindDef> leaders = new List<PawnKindDef>();
        public PawnKindDef basicMember;
        /// <summary>false = não entra na carga (só pelo comando de teste "roster").</summary>
        public bool applyOnStartup = true;
    }

    public class UnitGroupEntry
    {
        public PawnKindDef kind;
        /// <summary>Combat, Peaceful, Settlement, TraderGuard ou Trader.</summary>
        public string group;
        public float weight = 1f;
    }

    [StaticConstructorOnStartup]
    public static class EQUnits
    {
        static EQUnits()
        {
            foreach (UnitRosterDef r in DefDatabase<UnitRosterDef>.AllDefsListForReading)
            {
                if (!r.applyOnStartup) continue;
                try { Apply(r); }
                catch (System.Exception ex) { Log.Error("[Ecos da Queda] unidades do editor (" + r.defName + "): " + ex); }
            }
        }

        public static string Apply(UnitRosterDef r)
        {
            FactionDef f = r.faction;
            if (f == null) return "sem facção";
            var log = new List<string>();
            if (!r.leaders.NullOrEmpty())
            {
                if (r.replaceExisting || f.fixedLeaderKinds == null) f.fixedLeaderKinds = new List<PawnKindDef>();
                foreach (PawnKindDef k in r.leaders) if (!f.fixedLeaderKinds.Contains(k)) f.fixedLeaderKinds.Add(k);
            }
            if (r.basicMember != null) f.basicMemberKind = r.basicMember;
            if (f.pawnGroupMakers == null) return "facção sem grupos";
            foreach (var byGroup in r.entries.Where(e => e.kind != null).GroupBy(e => e.group))
            {
                PawnGroupKindDef gk = byGroup.Key == "TraderGuard" ? PawnGroupKindDefOf.Trader : DefDatabase<PawnGroupKindDef>.GetNamedSilentFail(byGroup.Key);
                if (gk == null) { log.Add("grupo desconhecido: " + byGroup.Key); continue; }
                foreach (PawnGroupMaker m in f.pawnGroupMakers.Where(m => m.kindDef == gk))
                {
                    List<PawnGenOption> list = byGroup.Key == "TraderGuard" ? m.guards : byGroup.Key == "Trader" ? m.traders : m.options;
                    if (list == null) continue;
                    if (r.replaceExisting) list.Clear();
                    foreach (UnitGroupEntry e in byGroup)
                    {
                        list.RemoveAll(o => o.kind == e.kind);
                        list.Add(new PawnGenOption { kind = e.kind, selectionWeight = e.weight });
                    }
                }
                log.Add(byGroup.Key + ": " + byGroup.Count());
            }
            return string.Join(", ", log);
        }
    }
}
