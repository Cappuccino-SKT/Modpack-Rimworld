using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Etapa 5d: "fungobi" (estado), "fungobi horde [pontos]" (dispara a horda), "fungobi ruin" (gera a ruína de um assentamento destruído ao lado).</summary>
    public static class EQFungobiDebug
    {
        public static string Run(string sub, string arg)
        {
            Faction f = EQFungobi.Faction;
            if (f == null) return "facção EQ_Faction_Fungobi não existe neste mundo";
            Map home = Find.AnyPlayerHomeMap;
            switch (sub)
            {
                case "horde":
                {
                    IncidentDef d = DefDatabase<IncidentDef>.GetNamed("EQ_Incidente_HordaFungobi");
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(d.category, home);
                    if (arg != null) parms.points = float.Parse(arg);
                    parms.forced = true;
                    bool ok = d.Worker.TryExecute(parms);
                    var pawns = home.mapPawns.SpawnedPawnsInFaction(f);
                    return "horda ok=" + ok + " pontos=" + parms.points.ToString("0") + " no mapa=" + pawns.Count + " " + string.Join(",", pawns.GroupBy(p => p.kindDef.defName).Select(g => g.Key + "x" + g.Count()))
                        + " xenos=" + string.Join(",", pawns.Select(p => p.genes?.Xenotype?.defName).Distinct()) + " hostil=" + f.HostileTo(Faction.OfPlayer)
                        + " lord=" + pawns.FirstOrDefault()?.GetLord()?.LordJob?.GetType().Name;
                }
                case "ruin":
                {
                    if (!TileFinder.TryFindPassableTileWithTraversalDistance(home.Tile, 2, 8, out PlanetTile tile, t => !Find.WorldObjects.AnyWorldObjectAt(t)))
                        return "sem tile livre perto";
                    var ds = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
                    ds.Tile = tile;
                    ds.SetFaction(Find.FactionManager.AllFactionsVisible.FirstOrDefault(x => !x.IsPlayer));
                    Find.WorldObjects.Add(ds);
                    EQFungobi.LastReport = null;
                    EQFungobi.ForceRuin = true;
                    Map m = GetOrGenerateMapUtility.GetOrGenerateMap(tile, new IntVec3(150, 1, 150), null);
                    int n = m.mapPawns.SpawnedPawnsInFaction(f).Count;
                    string rep = "ruína gerada: " + (EQFungobi.LastReport ?? "(sem moradores)") + " | no mapa=" + n;
                    Current.Game.DeinitAndRemoveMap(m, false);
                    ds.Destroy();
                    return rep;
                }
                default:
                    return "facção=" + f.Name + " oculta=" + f.Hidden + " hostil=" + f.HostileTo(Faction.OfPlayer) + " ideo=" + f.ideos?.PrimaryIdeo?.name;
            }
        }
    }
}
