using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// Etapa 10a: medição de desempenho. "perf start" instrumenta (Harmony, prefixo + sufixo com relógio) o tique
    /// do jogo e os tiques de mundo, mapa e de todos os componentes (WorldComponent, MapComponent, GameComponent,
    /// de qualquer mod) e zera os contadores; "perf report [arquivo]" imprime ms por tique de cada um.
    /// Os itens são aninhados (ex.: componentes do mundo estão dentro de World.WorldTick): a % é sobre o tique inteiro.
    /// </summary>
    public static class EQPerf
    {
        private class Stat { public long ticks; public long calls; }

        private static readonly Dictionary<MethodBase, Stat> stats = new Dictionary<MethodBase, Stat>();
        private static readonly Dictionary<MethodBase, string> labels = new Dictionary<MethodBase, string>();
        private static bool patched;
        private static int startTick;
        private static readonly Stopwatch wall = new Stopwatch();
        private static MethodBase total;

        public static string Start()
        {
            int n = 0;
            if (!patched)
            {
                var h = new Harmony("ecosdaqueda.debug.perf");
                var pre = new HarmonyMethod(typeof(EQPerf), nameof(Pre)) { priority = Priority.First };
                var post = new HarmonyMethod(typeof(EQPerf), nameof(Post)) { priority = Priority.Last };
                total = AccessTools.Method(typeof(TickManager), nameof(TickManager.DoSingleTick));
                var targets = new List<(MethodBase m, string label)>
                {
                    (total, "TOTAL (TickManager.DoSingleTick)"),
                    (AccessTools.Method(typeof(World), "WorldTick"), "mundo: World.WorldTick (inteiro)"),
                    (AccessTools.Method(typeof(WorldPawns), "WorldPawnsTick"), "mundo: pawns do mundo"),
                    (AccessTools.Method(typeof(WorldObjectsHolder), "WorldObjectsHolderTick"), "mundo: objetos (bases, caravanas, expedições)"),
                    (AccessTools.Method(typeof(FactionManager), "FactionManagerTick"), "mundo: facções"),
                    (AccessTools.Method(typeof(Map), "MapPreTick"), "mapa: MapPreTick"),
                    (AccessTools.Method(typeof(Map), "MapPostTick"), "mapa: MapPostTick"),
                    (AccessTools.Method(typeof(TickList), "Tick"), "mapa: coisas (TickList, pawns e prédios)"),
                    (AccessTools.Method(typeof(Storyteller), "StorytellerTick"), "narrador"),
                };
                foreach ((Type baseType, string name, string prefix) in new[]
                {
                    (typeof(WorldComponent), "WorldComponentTick", "comp. mundo"),
                    (typeof(MapComponent), "MapComponentTick", "comp. mapa"),
                    (typeof(GameComponent), "GameComponentTick", "comp. jogo"),
                })
                {
                    foreach (Type t in GenTypes.AllSubclassesNonAbstract(baseType))
                    {
                        MethodInfo m = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                        if (m != null && m.GetMethodBody() != null)
                            targets.Add((m, prefix + ": " + t.FullName + " [" + t.Assembly.GetName().Name + "]"));
                    }
                }
                foreach (var (m, label) in targets)
                {
                    if (m == null || labels.ContainsKey(m)) continue;
                    try
                    {
                        h.Patch(m, prefix: pre, postfix: post);
                        labels[m] = label;
                        n++;
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("[EQ Perf] não deu para medir " + label + ": " + ex.Message);
                    }
                }
                patched = true;
            }
            stats.Clear();
            startTick = Find.TickManager.TicksGame;
            wall.Restart();
            return "medindo " + labels.Count + " métodos" + (n > 0 ? " (instalados agora: " + n + ")" : "") + " a partir do tique " + startTick;
        }

        public static void Pre(out long __state) => __state = Stopwatch.GetTimestamp();

        public static void Post(MethodBase __originalMethod, long __state)
        {
            if (!stats.TryGetValue(__originalMethod, out Stat s))
                stats[__originalMethod] = s = new Stat();
            s.ticks += Stopwatch.GetTimestamp() - __state;
            s.calls++;
        }

        public static string Report(string path)
        {
            if (!patched) return "use 'perf start' antes";
            int ticks = Find.TickManager.TicksGame - startTick;
            if (ticks <= 0) return "nenhum tique desde o 'perf start'";
            double Ms(Stat s) => s.ticks * 1000.0 / Stopwatch.Frequency;
            stats.TryGetValue(total, out Stat tot);
            double totalMs = tot != null ? Ms(tot) : 0;
            var rows = stats.Select(kv => (label: labels.TryGetValue(kv.Key, out string l) ? l : kv.Key.Name, ms: Ms(kv.Value), calls: kv.Value.calls))
                .OrderByDescending(r => r.ms).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("tiques=" + ticks + " (" + (ticks / 60000f).ToString("0.##") + " dias) ms/tique=" + (totalMs / ticks).ToString("0.000")
                + " tiques/s_sem_desenho=" + (totalMs > 0 ? (ticks * 1000 / totalMs).ToString("0") : "?")
                + " relogio=" + wall.Elapsed.TotalSeconds.ToString("0") + "s mapas=" + Find.Maps.Count
                + " pawns_mundo=" + Find.WorldPawns.AllPawnsAliveOrDead.Count() + " objetos_mundo=" + Find.WorldObjects.AllWorldObjects.Count
                + " pawns_mapa=" + (Find.CurrentMap?.mapPawns.AllPawnsSpawnedCount ?? 0));
            foreach (var r in rows.Take(25))
                sb.AppendLine("  " + (r.ms / ticks).ToString("0.0000") + " ms/tique  " + (totalMs > 0 ? (r.ms * 100 / totalMs).ToString("0.0") : "?") + "%  chamadas=" + r.calls + "  " + r.label);
            string file = path ?? System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "perf.tsv");
            System.IO.File.WriteAllLines(file, new[] { "ms_por_tique\tporcentagem\tchamadas\tmetodo" }
                .Concat(rows.Select(r => (r.ms / ticks).ToString("0.00000") + "\t" + (totalMs > 0 ? (r.ms * 100 / totalMs).ToString("0.00") : "") + "\t" + r.calls + "\t" + r.label)));
            sb.Append("-> " + file);
            return sb.ToString();
        }
    }
}
