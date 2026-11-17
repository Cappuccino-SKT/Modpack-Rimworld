using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// Executor de testes: ativo só com -eqtest=arquivo. Lê os comandos do arquivo, carrega um save
    /// ou cria um jogo novo, executa tudo, grava o relatório (-eqout=arquivo, ou arquivo.result.txt)
    /// e fecha o jogo. Comandos: ver EcosDaQueda-Dev/TESTES.md.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class TestRunner
    {
        private enum Stage { Menu, Loading, Running, Done }

        public static readonly bool Active;
        private static readonly string scriptPath;
        private static readonly string outPath;
        private static readonly List<string> commands = new List<string>();
        private static int index;
        private static Stage stage = Stage.Menu;
        private static int framesInStage;
        private static int ticksToRun;
        private static readonly Stopwatch clock = Stopwatch.StartNew();
        private static float timeoutSeconds = 900f;
        private static int errorCount;
        private static bool inCommand;
        private static Func<bool> stopWhen;

        static TestRunner()
        {
            var harmony = new Harmony("ecosdaqueda.debug");
            if (!GenCommandLine.TryGetCommandLineArg("eqtest", out scriptPath) || scriptPath.NullOrEmpty())
                return;
            try
            {
                if (!GenCommandLine.TryGetCommandLineArg("eqout", out outPath) || outPath.NullOrEmpty())
                    outPath = scriptPath + ".result.txt";
                foreach (string raw in File.ReadAllLines(scriptPath))
                {
                    string line = raw.Trim();
                    if (line.Length > 0 && !line.StartsWith("#"))
                        commands.Add(line);
                }
                File.WriteAllText(outPath, "# teste: " + scriptPath + "\n# início: " + DateTime.Now + "\n");
                Active = true;
                EQLeader.SuppressDialogs = true;
                harmony.Patch(AccessTools.Method(typeof(Root_Entry), nameof(Root_Entry.Update)), postfix: new HarmonyMethod(typeof(TestRunner), nameof(Frame)));
                harmony.Patch(AccessTools.Method(typeof(Root_Play), nameof(Root_Play.Update)), postfix: new HarmonyMethod(typeof(TestRunner), nameof(Frame)));
                harmony.Patch(AccessTools.Method(typeof(Log), nameof(Log.Error), new[] { typeof(string) }), postfix: new HarmonyMethod(typeof(TestRunner), nameof(OnLogError)));
                Write("# " + commands.Count + " comandos; executor ativo");
            }
            catch (Exception ex)
            {
                Log.Error("[EQ Debug] Falha ao iniciar o executor de testes: " + ex);
            }
        }

        private static void Write(string text)
        {
            try { File.AppendAllText(outPath, text + "\n"); } catch { }
        }

        public static void OnLogError(string text)
        {
            if (!Active || text == null) return;
            // Ruído conhecido de terceiros durante a criação do mundo (Hegemony/Rimesis)
            if (text.StartsWith("Could not find player faction")) return;
            // Erro da tradução PT-BR do jogo ao gerar nomes de certos itens (mapas)
            if (text.StartsWith("Bad string pass when reading rule")) return;
            // Combat Extended sem placa de vídeo (modo -Headless)
            if (text.Contains("AsyncGPUReadback failed")) return;
            // Idem: sem texturas lidas da placa, o CE não tem a silhueta de árvores/pawns para calcular cobertura
            if (text.Contains("Combat Extended :: CropVertical") || text.Contains("was not cached. This is a bug - texture should have been collected")) return;
            if (text.StartsWith("Error in ParallelForEach") && text.Contains("Combat Extended")) return;
            errorCount++;
            if (errorCount <= 40)
            {
                string first = text.Split('\n')[0];
                Write("!! LOG ERROR" + (inCommand ? " (durante o comando)" : "") + ": " + (first.Length > 400 ? first.Substring(0, 400) : first));
                // Resto da mensagem (ex.: pilha de uma exceção incluída no texto), até 10 linhas.
                if (errorCount <= 5)
                {
                    string[] rest = text.Split((char)10);
                    for (int i = 1; i < rest.Length && i <= 10; i++) Write("     | " + rest[i].Trim());
                }
                // Pilha de quem chamou Log.Error (o Player.log corta mensagens depois do limite).
                if (errorCount <= 5)
                {
                    string[] st = Environment.StackTrace.Split((char)10);
                    for (int i = 3; i < st.Length && i < 21; i++) Write("     " + st[i].Trim());
                }
            }
        }

        public static void Frame()
        {
            if (!Active || stage == Stage.Done)
                return;
            try
            {
                if (clock.Elapsed.TotalSeconds > timeoutSeconds)
                {
                    Write("!! TIMEOUT após " + timeoutSeconds + "s no estágio " + stage + ", comando " + index + ": " + (index < commands.Count ? commands[index] : "-"));
                    Finish();
                    return;
                }
                framesInStage++;
                switch (stage)
                {
                    case Stage.Menu:
                        if (!GenScene.InEntryScene || LongEventHandler.ShouldWaitForEvent || framesInStage < 30)
                            return;
                        StartGame();
                        break;
                    case Stage.Loading:
                        if (Current.ProgramState != ProgramState.Playing || LongEventHandler.ShouldWaitForEvent || Find.AnyPlayerHomeMap == null && Find.CurrentMap == null)
                        {
                            framesInStage = 0;
                            return;
                        }
                        if (framesInStage < 20)
                            return;
                        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                        Write("# jogo pronto em " + clock.Elapsed.TotalSeconds.ToString("0") + "s, tick " + Find.TickManager.TicksGame);
                        SetStage(Stage.Running);
                        break;
                    case Stage.Running:
                        RunCommands();
                        break;
                }
            }
            catch (Exception ex)
            {
                Write("!! EXCEÇÃO no executor: " + ex);
                Finish();
            }
        }

        private static void SetStage(Stage s)
        {
            stage = s;
            framesInStage = 0;
        }

        /// <summary>Igual a Root_Play.SetupForQuickTestPlay, mas aceita "newgame [ScenarioDef] [StorytellerDef]".</summary>
        private static void SetupNewGame(string args)
        {
            string[] a = (args ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            ScenarioDef scen = a.Length > 0 ? DefDatabase<ScenarioDef>.GetNamed(a[0]) : ScenarioDefOf.Crashlanded;
            StorytellerDef teller = a.Length > 1 ? DefDatabase<StorytellerDef>.GetNamed(a[1]) : StorytellerDefOf.Cassandra;
            Current.ProgramState = ProgramState.Entry;
            Game.ClearCaches();
            Current.Game = new Game();
            Current.Game.InitData = new GameInitData();
            Current.Game.Scenario = scen.scenario;
            Find.Scenario.PreConfigure();
            Current.Game.storyteller = new Storyteller(teller, DifficultyDefOf.Rough);
            Current.Game.World = WorldGenerator.GenerateWorld(0.3f, GenText.RandomSeedString(), OverallRainfall.Normal, OverallTemperature.Normal, OverallPopulation.Normal, LandmarkDensity.Normal);
            Find.GameInitData.ChooseRandomStartingTile();
            Find.GameInitData.mapSize = 250;
            Find.Scenario.PostIdeoChosen();
        }

        private static void StartGame()
        {
            string first = index < commands.Count ? commands[index] : "";
            string[] p = first.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length > 0 && p[0] == "load" && p.Length > 1)
            {
                index++;
                Write("> " + first);
                GameDataSaveLoader.LoadGame(p[1].Trim());
            }
            else
            {
                if (p.Length > 0 && p[0] == "newgame")
                {
                    index++;
                    Write("> " + first);
                }
                else
                    Write("# (sem load/newgame no início: criando jogo novo)");
                // Mesmo caminho do jogo: prepara o jogo na cena de entrada e troca para a cena Play.
                SetupNewGame(p.Length > 1 && p[0] == "newgame" ? p[1] : null);
                Find.GameInitData.PrepForMapGen();
                Find.Scenario.PreMapGenerate();
                LongEventHandler.QueueLongEvent(null, "Play", "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
            }
            SetStage(Stage.Loading);
        }

        private static void RunCommands()
        {
            // "wait" em andamento: roda os ticks em blocos por quadro
            if (ticksToRun > 0)
            {
                var sw = Stopwatch.StartNew();
                while (ticksToRun > 0 && sw.ElapsedMilliseconds < 200)
                {
                    Find.TickManager.DoSingleTick();
                    ticksToRun--;
                    if (stopWhen != null && ticksToRun % 50 == 0 && stopWhen())
                        ticksToRun = 0;
                }
                if (ticksToRun == 0)
                    stopWhen = null;
                if (ticksToRun == 0)
                    Write("  agora tick " + Find.TickManager.TicksGame + " (dia " + (Find.TickManager.TicksGame / 60000f).ToString("0.0") + ")");
                return;
            }
            if (index >= commands.Count)
            {
                Finish();
                return;
            }
            string cmd = commands[index++];
            Write("> " + cmd);
            inCommand = true;
            try
            {
                string result = Execute(cmd);
                if (!result.NullOrEmpty())
                    Write(result.StartsWith("  ") ? result : "  " + result.Replace("\n", "\n  "));
            }
            catch (Exception ex)
            {
                Write("!! EXCEÇÃO no comando: " + (ex is System.Reflection.TargetInvocationException t && t.InnerException != null ? t.InnerException : ex));
            }
            inCommand = false;
        }

        private static string Execute(string cmd)
        {
            string[] a = cmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string Arg(int i) => i < a.Length ? a[i] : null;
            Faction Fac(int i)
            {
                Faction f = EQDebug.FindFaction(Arg(i));
                if (f == null) throw new Exception("facção não encontrada: " + Arg(i));
                return f;
            }
            switch (a[0].ToLowerInvariant())
            {
                case "wait":
                    ticksToRun = ParseTicks(Arg(1));
                    return "rodando " + ticksToRun + " ticks...";
                case "timeout":
                    timeoutSeconds = float.Parse(Arg(1));
                    return null;
                case "dump": return EQDebug.Dump();
                case "transexport": return TransExport.Export(Arg(1), Arg(2));
                case "deftext": return TransExport.DefText(Arg(1), Arg(2));
                case "genegen": return TransExport.GeneratedGenes(Arg(1));
                case "transerrors": return TransExport.TranslationErrors(Arg(1));
                case "language": return LanguageDatabase.activeLanguage?.folderName ?? "(nenhuma)";
                case "rep":
                    if (Arg(2) == "add") return EQDebug.AddReputation(Fac(1), float.Parse(Arg(3)), float.Parse(Arg(4) ?? "0"));
                    return EQDebug.SetReputation(Fac(1), Arg(2) == "fame" || Arg(2) == "fama", float.Parse(Arg(3)));
                case "goodwill": return EQDebug.Goodwill(Fac(1), int.Parse(Arg(2)));
                case "hostile": return EQDebug.SetHostile(Fac(1), true);
                case "peace": return EQDebug.SetHostile(Fac(1), false);
                case "movingbases": return EQDebug.MovingBases();
                case "factions": return EQDebug.ListFactions();
                case "research": return EQDebug.ResearchDump(Arg(1));
                case "tech": return EQDebug.Tech(Arg(1), Arg(2));
                case "faction": return EQDebug.DescribeFaction(Fac(1));
                case "raid": return EQDebug.Raid(Fac(1), float.Parse(Arg(2) ?? "0"), Arg(3) == "bichos");
                case "retaliate": return EQDebug.Retaliate(Fac(1));
                case "gift": return EQDebug.SendGift(Fac(1), false);
                case "tribute": return EQDebug.SendGift(Fac(1), true);
                case "join":
                    if (Arg(1) == "real") return EQDebug.Join(null, restore: true);
                    return EQDebug.Join(Arg(1) == "none" ? null : Fac(1));
                case "due": return EQDebug.MakeDue(Arg(1) == "tithe" || Arg(1) == "dizimo");
                case "allegiance": return EQDebug.DescribeAllegiance();
                case "ceremony":
                    if (Arg(1) == "await")
                    {
                        ticksToRun = ParseTicks(Arg(2) ?? "1d");
                        stopWhen = EQDebug.CeremonyReady;
                        return "esperando o emissário ficar pronto (até " + ticksToRun + " ticks)...";
                    }
                    if (Arg(1) == "start") return EQDebug.CeremonyStart();
                    if (Arg(1) == "diag") return EQDebug.CeremonyDiag(Fac(2));
                    if (Arg(1) == "status" || Arg(1) == null) return EQDebug.CeremonyStatus();
                    if (Arg(1) == "cancel") { EQCeremony.Cancel(); return EQDebug.DescribeAllegiance(); }
                    return EQDebug.Ceremony(Fac(1));
                case "quests": return EQDebug.Quests();
                case "quest": return EQDebug.QuestText(string.Join(" ", a.Skip(1)));
                case "accept": return EQDebug.AcceptQuest(string.Join(" ", a.Skip(1)));
                case "favor":
                    if (Arg(1) == "event") return EQDebug.FavorEvent(Arg(2), int.Parse(Arg(3) ?? "1"));
                    return EQDebug.Favor(int.Parse(Arg(1)));
                case "leader": return EQDebug.Leader(Arg(1), Arg(2));
                case "renounce": return EQDebug.Renounce();
                case "services": return EQDebug.Services();
                case "service": return EQDebug.UseService(Arg(1));
                case "give": return EQDebug.Give(Arg(1), int.Parse(Arg(2) ?? "1"));
                case "prisoner": return EQDebug.SpawnPrisoner();
                case "kill": return EQDebug.KillMember(Fac(1));
                case "serum": return EQDebug.UseSerum(int.Parse(Arg(1) ?? "1"));
                case "cecheck": return EQDebug.CECheck(Arg(1));
                case "weapontags": return EQDebug.WeaponTags(Arg(1));
                case "kit": return EQDebug.Kit(Arg(1), int.Parse(Arg(2) ?? "20"));
                case "tradeinfo": return EQDebug.TradeInfo();
                case "trader": return EQDebug.TraderStock(Arg(1), Arg(2));
                case "start": return EQDebug.StartInfo();
                case "modusage": return EQDebug.ModUsage(Arg(1));
                case "xenogen": return EQDebug.XenoGen(Arg(1), int.Parse(Arg(2) ?? "20"));
                case "pawngen": return EQDebug.PawnGen(Arg(1), int.Parse(Arg(2) ?? "20"));
                case "count":
                {
                    ThingDef cd = DefDatabase<ThingDef>.GetNamedSilentFail(Arg(1) ?? "");
                    if (cd == null) return "ThingDef não existe: " + Arg(1);
                    Map cm = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
                    var all = cm.listerThings.ThingsOfDef(cd);
                    return cd.defName + ": no mapa " + all.Sum(t => t.stackCount) + ", na área doméstica " + all.Where(t => cm.areaManager.Home[t.Position]).Sum(t => t.stackCount)
                        + " (+ em cápsulas a cair: " + cm.listerThings.ThingsInGroup(ThingRequestGroup.ActiveTransporter).Count + ")";
                }
                case "leather": return EQDebug.Leather(Arg(1));
                case "hive": return Arg(1) == "nature" ? EQDebug.Hive(Arg(2), true) : EQDebug.Hive(Arg(1), false);
                case "base": return EQDebug.Base(Fac(1));
                case "permit": return EQDebug.Permit(Arg(1), Arg(2));
                case "patron": return EQDebug.Patron(Arg(1));
                case "drone": return EQDebug.Drone(Arg(1));
                case "landmarks": return EQDebug.Landmarks();
                case "vehicle": return EQDebug.Vehicle(Arg(1), Arg(2));
                case "sanity": return EQDebug.Sanity(Arg(1), Arg(2));
                case "android": return EQDebug.Android(Arg(1) == "fab" && Arg(2) == "awake" ? "fabawake" : Arg(1));
                case "psy": return EQDebug.Psy(Arg(1), Arg(2), Arg(3));
                case "signature": return EQDebug.Signature(Fac(1), Arg(2) == "run");
                case "incident": return EQDebug.FireIncident(Arg(1));
                case "letters": return EQDebug.Letters();
                case "choose": return EQDebug.Choose(string.Join(" ", a.Skip(1).Take(a.Length - 2)), int.Parse(a[a.Length - 1]));
                case "save":
                    GameDataSaveLoader.SaveGame(Arg(1) ?? "EQTest");
                    return "salvo como " + (Arg(1) ?? "EQTest");
                case "quit":
                    index = commands.Count;
                    return null;
                default:
                    return "!! comando desconhecido: " + a[0];
            }
        }

        private static int ParseTicks(string s)
        {
            if (s.NullOrEmpty()) return 2500;
            s = s.ToLowerInvariant();
            if (s.EndsWith("d")) return (int)(float.Parse(s.TrimEnd('d'), System.Globalization.CultureInfo.InvariantCulture) * 60000);
            if (s.EndsWith("h")) return (int)(float.Parse(s.TrimEnd('h'), System.Globalization.CultureInfo.InvariantCulture) * 2500);
            return int.Parse(s);
        }

        private static void Finish()
        {
            if (stage == Stage.Done) return;
            stage = Stage.Done;
            Write("# fim: " + DateTime.Now + " | duração " + clock.Elapsed.TotalSeconds.ToString("0") + "s | erros no log: " + errorCount);
            Write("# RESULTADO: " + (errorCount == 0 ? "SEM ERROS" : errorCount + " ERRO(S)"));
            Root.Shutdown();
        }
    }
}
