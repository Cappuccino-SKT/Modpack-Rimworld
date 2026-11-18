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

        /// <summary>Etapa 11a: faltas por potência, colonos emprestados e cargo atual.</summary>
        public static string Obligations(bool forceReturn = false)
        {
            var comp = WorldComponent_PowerEvents.Current;
            var alg = WorldComponent_Allegiance.Current;
            if (comp == null) return "sem componente";
            if (forceReturn) foreach (var l in comp.lent) l.returnTick = Find.TickManager.TicksGame;
            return "membro=" + (alg?.memberOf?.Name ?? "-") + " cargo=" + (alg?.memberOf != null ? (alg.RankIndex + 1) + " (" + alg.RankLabel + ")" : "-")
                + " faltas=" + string.Join(",", comp.strikes.Select(kv => kv.Key + ":" + kv.Value))
                + " emprestados=" + string.Join(",", comp.lent.Select(l => l.pawn?.LabelShort + "->" + l.faction?.def.defName + " volta no dia " + (l.returnTick / 60000f).ToString("0.0")))
                + " colonos_no_mapa=" + (Find.AnyPlayerHomeMap?.mapPawns.FreeColonistsSpawnedCount ?? 0)
                + " ultima_volta=" + (WorldComponent_PowerEvents.LastReturnInfo ?? "-")
                + " [" + string.Join(",", (Find.AnyPlayerHomeMap?.mapPawns.FreeColonistsSpawned ?? new System.Collections.Generic.List<Pawn>()).Select(p => p.LabelShort)) + "]";
        }

        /// <summary>Etapa 5b: "exposure [add N | drop]" — Exposição perante a Forja, relação com Forja e Fora de Linha;
        /// drop gera um técnico da Forja no mapa, mata e conta os dados de diagnóstico que caíram (chance forçada a 100%).</summary>
        public static string Exposure(string sub, string arg)
        {
            var comp = WorldComponent_Exposure.Current;
            if (comp == null) return "sem componente";
            Faction forja = WorldComponent_Exposure.Forja;
            Faction fdl = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamedSilentFail("EQ_Faction_ForaDeLinha"));
            Map map = Find.AnyPlayerHomeMap;
            string extra = "";
            if (sub == "add") comp.Add(float.Parse(arg, System.Globalization.CultureInfo.InvariantCulture), "teste");
            else if (sub == "drop")
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("EQ_Forja_Tecnico");
                float old = Patch_Pawn_Kill_Diagnostico.Chance;
                Patch_Pawn_Kill_Diagnostico.Chance = 1f;
                try
                {
                    Pawn p = PawnGenerator.GeneratePawn(kind, forja);
                    GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(map.Center, map, 10), map);
                    p.Kill(null);
                }
                finally { Patch_Pawn_Kill_Diagnostico.Chance = old; }
                ThingDef d = DefDatabase<ThingDef>.GetNamed("EQ_DadosDiagnostico");
                extra = " dados_no_mapa=" + map.listerThings.ThingsOfDef(d).Sum(t => t.stackCount);
            }
            return "exposicao=" + comp.exposure.ToString("0.#") + " forja=" + forja?.PlayerGoodwill + (forja != null && forja.HostileTo(Faction.OfPlayer) ? "(hostil)" : "")
                + " foraDeLinha=" + fdl?.PlayerGoodwill + (fdl != null && fdl.HostileTo(Faction.OfPlayer) ? "(hostil)" : "") + extra;
        }

        /// <summary>Etapa 5e: a facção está em trégua? E o sorteio de ataques ainda a aceita?</summary>
        public static string Truce(Faction f)
        {
            var comp = WorldComponent_PowerEvents.Current;
            var worker = IncidentDefOf.RaidEnemy.Worker;
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, Find.AnyPlayerHomeMap);
            bool canRaid = HarmonyLib.Traverse.Create(worker).Method("FactionCanBeGroupSource", new[] { typeof(Faction), typeof(IncidentParms), typeof(bool) }).GetValue<bool>(f, parms, false);
            return f.Name + ": tregua=" + (comp?.InTruce(f) ?? false) + " pode_atacar=" + canRaid;
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
