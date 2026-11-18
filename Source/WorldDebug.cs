using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Bloco 4b: zonas da Queda (amostras do Alpha Biomes) e trincheiras nos cercos.</summary>
    public static class EQWorldDebug
    {
        private static Map Map => Find.CurrentMap ?? Find.Maps.FirstOrDefault();

        /// <summary>zone list | zone &lt;sufixo ou defName&gt;</summary>
        public static string Zone(string arg)
        {
            var sb = new StringBuilder();
            ThingDef bench = DefDatabase<ThingDef>.GetNamedSilentFail("EQ_BancadaEngenhariaReversa");
            if (arg == null || arg == "list")
            {
                foreach (RecipeDef r in EQZones.Recipes)
                {
                    var z = r.GetModExtension<ZoneSampleExtension>();
                    var ing = r.ingredients.FirstOrDefault();
                    sb.AppendLine(r.defName + ": bioma=" + (z.biome?.defName ?? "NULO") + " recurso=" + (ing?.filter?.AnyAllowedDef?.defName ?? "NULO") + "x" + (ing?.GetBaseCount() ?? 0)
                        + " ramo=" + (z.branch?.defName ?? "NULO") + " projetos=" + EQZones.Pool(z).Count() + " na_bancada=" + (bench?.AllRecipes?.Contains(r) ?? false));
                }
                sb.Append("zonas=" + EQZones.Recipes.Count());
                return sb.ToString();
            }
            RecipeDef rec = DefDatabase<RecipeDef>.GetNamedSilentFail(arg) ?? DefDatabase<RecipeDef>.GetNamedSilentFail("EQ_AmostraZona_" + arg);
            if (rec == null || !rec.HasModExtension<ZoneSampleExtension>())
                return "receita de zona não encontrada: " + arg;
            var ext = rec.GetModExtension<ZoneSampleExtension>();
            Pawn col = PawnsFinder.AllMaps_FreeColonists.FirstOrDefault();
            ResearchProjectDef proj = EQZones.Pick(ext);
            float before = proj?.ProgressPercent ?? 0f;
            var owner = proj == null ? null : EQLicenses.OwnedBranch(proj);
            Faction of = owner == null ? null : EQUtil.MainFaction(owner.faction);
            float infBefore = of == null ? 0f : WorldComponent_Reputation.Current.Get(of).infamy;
            bool accBefore = proj != null && EQResearch.HasAccess(proj);
            // Mesmo caminho do jogo: o trabalhador da receita ao fim da iteração.
            rec.Worker.Notify_IterationCompleted(col, new List<Thing>());
            sb.Append("receita=" + rec.defName + " ramo=" + ext.branch?.defName + " projeto=" + (proj?.defName ?? "-"));
            if (proj != null)
                sb.Append(" nivel=" + proj.techLevel + " custo=" + proj.Cost.ToString("0") + " progresso=" + before.ToStringPercent() + "->" + proj.ProgressPercent.ToStringPercent()
                    + " prereqs_ok=" + proj.PrerequisitesCompleted + " acesso=" + accBefore + "->" + EQResearch.HasAccess(proj) + " licenca=" + EQLicenses.IsLicensed(proj)
                    + (of == null ? "" : " infamia_" + owner.faction.defName + "=" + infBefore.ToString("0.#") + "->" + WorldComponent_Reputation.Current.Get(of).infamy.ToString("0.#")));
            return sb.ToString();
        }

        /// <summary>siege &lt;facção&gt; place: plantas do cerco num ponto longe dos colonos (desfeitas depois). siege &lt;facção&gt; real: ataque com estratégia de cerco. siege &lt;facção&gt; count: o que a facção tem no mapa.</summary>
        public static string Siege(Faction f, string mode)
        {
            Map map = Map;
            if (f == null || map == null)
                return "facção ou mapa nulos";
            var sb = new StringBuilder();
            SiegeTrenchDef s = EQSieges.For(f);
            sb.Append("faccao=" + f.def.defName + " regra=" + (s?.defName ?? "-") + " ");
            switch (mode ?? "place")
            {
                case "place":
                {
                    IntVec3 target = EQSieges.Target(map);
                    IntVec3 center = CellFinder.RandomClosewalkCellNear(new IntVec3(target.x < map.Size.x / 2 ? map.Size.x - 30 : 30, 0, target.z), map, 6);
                    EQSieges.LastReport = "-";
                    List<Blueprint_Build> bps = SiegeBlueprintPlacer.PlaceBlueprints(center, map, f, 300f).ToList();
                    sb.Append("centro=" + center + " alvo=" + target + " plantas=" + bps.Count + " por_def=" + string.Join(",", bps.GroupBy(b => b.def.entityDefToBuild.defName).Select(g => g.Key + ":" + g.Count()))
                        + " trincheira_mais_perto=" + (bps.Where(b => b.def.entityDefToBuild.defName.StartsWith("Tr_Trench")).Select(b => b.Position.DistanceTo(center)).DefaultIfEmpty(-1f).Min().ToString("0.#"))
                        + " relatorio=" + EQSieges.LastReport);
                    foreach (var b in bps) if (!b.Destroyed) b.Destroy(DestroyMode.Cancel);
                    break;
                }
                case "real":
                {
                    if (!f.HostileTo(Faction.OfPlayer)) EQDebug.SetHostile(f, true);
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                    parms.faction = f;
                    parms.forced = true;
                    parms.points = Math.Max(parms.points, 1500f);
                    parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                    parms.raidStrategy = DefDatabase<RaidStrategyDef>.GetNamed("Siege");
                    EQSieges.LastReport = "-";
                    bool ok;
                    try { ok = IncidentDefOf.RaidEnemy.Worker.TryExecute(parms); }
                    catch (Exception ex) { return "EXCEÇÃO " + ex.GetType().Name + ": " + ex.Message; }
                    sb.Append("executou=" + ok + " pontos=" + parms.points.ToString("0") + " chegaram=" + map.mapPawns.AllPawnsSpawned.Count(p => p.Faction == f));
                    break;
                }
                case "count":
                {
                    var things = map.listerThings.AllThings.Where(t => t.Faction == f && (t is Blueprint_Build || t is Frame || t.def.defName.StartsWith("Tr_"))).ToList();
                    sb.Append("plantas=" + things.Count(t => t is Blueprint_Build) + " obras=" + things.Count(t => t is Frame) + " prontas_tr=" + things.Count(t => !(t is Blueprint) && !(t is Frame))
                        + " por_def=" + string.Join(",", things.GroupBy(t => (t is Blueprint_Build bb ? "planta:" + bb.def.entityDefToBuild.defName : t is Frame fr ? "obra:" + fr.def.entityDefToBuild.defName : t.def.defName)).Select(g => g.Key + ":" + g.Count()))
                        + " relatorio=" + EQSieges.LastReport + " lord=" + (map.lordManager.lords.FirstOrDefault(l => l.faction == f)?.CurLordToil?.GetType().Name ?? "-"));
                    break;
                }
                default: return "uso: siege <facção> place|real|count";
            }
            return sb.ToString();
        }

        /// <summary>asker &lt;QuestScriptDef&gt; [n]: gera n missões, conta quem pede (patrona, outra facção, ninguém) pelo texto e as remove.</summary>
        public static string Asker(string def, int n)
        {
            QuestScriptDef script = DefDatabase<QuestScriptDef>.GetNamedSilentFail(def);
            if (script == null) return "missão não encontrada: " + def;
            QuestPatronDef pd = QuestPatronDef.For(script);
            Faction patron = pd == null ? null : EQUtil.MainFaction(pd.faction);
            Map map = Map;
            int dePatrona = 0, deOutra = 0, semPedinte = 0, falhou = 0;
            int before = EQQuestAskers.Applied;
            string exemplo = null;
            for (int i = 0; i < n; i++)
            {
                var slate = new RimWorld.QuestGen.Slate();
                slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(map));
                slate.Set("map", map);
                Quest q = null;
                try { q = QuestUtility.GenerateQuestAndMakeAvailable(script, slate); }
                catch (Exception ex) { falhou++; exemplo = exemplo ?? ("EXCEÇÃO " + ex.GetType().Name + ": " + ex.Message); continue; }
                if (q == null) { falhou++; continue; }
                string d = q.description.ToString();
                if (patron != null && d.Contains(patron.Name)) dePatrona++;
                else if (Find.FactionManager.AllFactionsListForReading.Any(f => !f.IsPlayer && !f.Hidden && f.Name != null && d.Contains(f.Name))) { deOutra++; exemplo = exemplo ?? d.Truncate(200); }
                else semPedinte++;
                Find.QuestManager.Remove(q);
            }
            return "missao=" + def + " patrona=" + (patron?.def.defName ?? "-") + " hostil=" + (patron?.HostileTo(Faction.OfPlayer) ?? false) + " geradas=" + n
                + " da_patrona=" + dePatrona + " de_outra=" + deOutra + " sem_pedinte=" + semPedinte + " falhas=" + falhou
                + " trocas=" + (EQQuestAskers.Applied - before) + " ultima=" + EQQuestAskers.LastReport + (exemplo == null ? "" : " exemplo=" + exemplo.Replace("\n", " "));
        }
    }
}
