using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Etapa 6a: "narrator [pick n | canfire incidente | points]". Sem argumento: estado.</summary>
    public static class EQNarratorDebug
    {
        public static string Run(string[] a)
        {
            string Arg(int i) => i < a.Length ? a[i] : null;
            var e = EQNarrator.Ext;
            Map map = EQDebug.Map;
            string extra = "";
            switch (Arg(1))
            {
                case "pick":
                {
                    if (e == null) return "narrador sem extensão: " + Find.Storyteller?.def?.defName;
                    int n = int.TryParse(Arg(2), out int v) ? v : 200;
                    var worker = (IncidentWorker_RaidEnemy)IncidentDefOf.RaidEnemy.Worker;
                    var count = new Dictionary<string, int>();
                    for (int i = 0; i < n; i++)
                    {
                        var parms = new IncidentParms { target = map, points = 1000f, raidStrategy = RaidStrategyDefOf.ImmediateAttack };
                        if (EQNarrator.TryPick(worker, parms, e, out Faction f))
                        {
                            string k = f.def.defName.Replace("EQ_Faction_", "");
                            count[k] = count.TryGetValue(k, out int c) ? c + 1 : 1;
                        }
                    }
                    extra = "sorteios(" + n + "): " + string.Join(", ", count.OrderByDescending(kv => kv.Value).Select(kv => kv.Key + "=" + kv.Value)) + " | último: " + EQNarrator.LastPick;
                    break;
                }
                case "canfire":
                {
                    IncidentDef d = DefDatabase<IncidentDef>.GetNamedSilentFail(Arg(2));
                    if (d == null) return "incidente não existe";
                    var parms = StorytellerUtility.DefaultParmsNow(d.category, map);
                    extra = d.defName + " pode=" + d.Worker.CanFireNow(parms);
                    break;
                }
            }
            string s = (extra == "" ? "" : extra + " | ") + "narrador=" + Find.Storyteller?.def?.defName + " extensão=" + (e != null);
            if (e != null)
            {
                string why = EQNarrator.UnlockReason(e);
                s += " fim_de_jogo=" + (why == null ? "segurado" : "liberado (" + why + ")")
                     + " pontos_agora=" + StorytellerUtility.DefaultThreatPointsNow(map).ToString("0", CultureInfo.InvariantCulture) + " teto=" + e.heldPointsCap
                     + " bloqueados=" + EQNarrator.Held(e).Count + " membro=" + WorldComponent_Allegiance.Current?.memberOf?.def.defName + " cargo=" + WorldComponent_Allegiance.Current?.RankIndex
                     + " pesos=[" + string.Join(", ", Find.FactionManager.AllFactionsListForReading.Where(f => !f.IsPlayer && !f.Hidden && f.HostileTo(Faction.OfPlayer))
                         .Select(f => f.def.defName.Replace("EQ_Faction_", "") + "=" + EQNarrator.Weight(f, e, 1000f).ToString("0.0", CultureInfo.InvariantCulture))) + "]";
            }
            return s;
        }
    }
}
