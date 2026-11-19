using System.Linq;
using RimWorld;
using Verse;


namespace EcosDaQueda.DebugTools
{
    /// <summary>Investigação de estrutura: "investigate [spawn &lt;def&gt; | designate | apply | research &lt;def&gt;]". Sem argumento: estado.</summary>
    public static class EQStructuresDebug
    {
        private static Thing last;

        public static string Run(string[] a)
        {
            string Arg(int i) => i < a.Length ? a[i] : null;
            Map map = EQDebug.Map;
            string extra = "";
            switch (Arg(1))
            {
                case "spawn":
                {
                    ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(Arg(2) ?? "AncientGenerator");
                    if (def == null) return "def não existe: " + Arg(2);
                    IntVec3 near = map.mapPawns.FreeColonistsSpawned.FirstOrDefault()?.Position ?? map.Center;
                    if (!CellFinder.TryFindRandomCellNear(near, map, 12, c => GenConstruct.CanPlaceBlueprintAt(def, c, Rot4.North, map).Accepted && c.Standable(map) && !c.Fogged(map), out IntVec3 cell))
                        return "sem lugar para " + def.defName;
                    foreach (Thing t in GenAdj.OccupiedRect(cell, Rot4.North, def.size).SelectMany(c => c.GetThingList(map)).Where(t => t.def.category == ThingCategory.Plant || t.def.category == ThingCategory.Item).ToList()) t.Destroy();
                    last = GenSpawn.Spawn(ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null), cell, map);
                    extra = "criado " + last.LabelCap + " em " + cell + " pode=" + Report(last);
                    break;
                }
                case "designate":
                {
                    if (last == null || last.Destroyed) return "sem estrutura";
                    var des = new Designator_EQInvestigate();
                    var r = des.CanDesignateThing(last);
                    if (r.Accepted) des.DesignateThing(last);
                    extra = "designar: " + (r.Accepted ? "ok" : "recusado " + r.Reason);
                    break;
                }
                case "apply":
                    if (last == null || last.Destroyed) return "sem estrutura";
                    var proj = EQStructures.Apply(last, null);
                    extra = "aplicado: " + (proj?.defName ?? "nada") + " -> " + EQStructures.LastResult;
                    break;
                case "research":
                {
                    // investigate research <projeto> <0|1> : zera ou conclui um projeto (para testar "nada a aprender")
                    ResearchProjectDef r = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(Arg(2));
                    if (r == null) return "projeto não existe";
                    if (Arg(3) == "1") Find.ResearchManager.FinishProject(r, false, null, false);
                    else Find.ResearchManager.ResetAllProgress();
                    extra = r.defName + " concluído=" + r.IsFinished;
                    break;
                }
            }
            var dd = EQStructures.Designation;
            int marked = dd == null ? -1 : map.designationManager.SpawnedDesignationsOfDef(dd).Count();
            ResearchProjectDef elec = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Electricity");
            string s = (extra == "" ? "" : extra + " | ") + "lições=" + EQStructures.LessonCount + " fontes=" + EQStructures.Sources.Count
                + " designações=" + marked + " última=" + (last == null ? "-" : last.def.defName + (last.Destroyed ? " (desmontada)" : " pode=" + Report(last)))
                + " Electricity=" + elec?.ProgressPercent.ToStringPercent() + " resultado=" + (EQStructures.LastResult ?? "-")
                + " reverso_na_lista=" + (Find.ReverseDesignatorDatabase.Get<Designator_EQInvestigate>() != null);
            return s;
        }

        private static string Report(Thing t)
        {
            var r = EQStructures.CanInvestigate(t);
            return r.Accepted ? "sim(" + EQStructures.PickProject(t)?.defName + " " + EQStructures.Fraction(t).ToStringPercent() + " trabalho=" + EQStructures.WorkFor(t) + ")" : "não(" + r.Reason + ")";
        }
    }
}
