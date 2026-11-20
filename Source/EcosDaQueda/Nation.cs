using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace EcosDaQueda
{
    /// <summary>N1 (NACAO.md): parâmetros da fundação da nação. Valores propostos; ajustar no XML.</summary>
    public class NationDef : Def
    {
        public int minAdults = 3;
        public float revoltDelayMinDays = 1f, revoltDelayMaxDays = 2f;
        public float revoltPointsFactor = 1.3f, revoltMinPoints = 200f, revoltMaxPoints = 8000f;
        public float failGrowth = 0.2f;
        public float retryDays = 15f;
        public float throneHoldHours = 6f;
        public float bribeMinCut = 0.30f, bribeMaxCut = 0.40f;
        public float bribeSilverPerPoint = 0.6f;
        public FactionDef revoltFaction;
        /// <summary>N1b: ideologia do jogador clássica até a fundação (só cenários do Ecos, salvo classicStartAllScenarios).</summary>
        public bool classicStart = true;
        public bool classicStartAllScenarios = false;
        /// <summary>N1b: a fé nova já começa com a primeira reforma liberada (para acertar os preceitos sorteados).</summary>
        public bool freeReformOnFounding = true;

        public static NationDef Active => DefDatabase<NationDef>.AllDefsListForReading.FirstOrDefault();
    }

    public enum NationStage { Settlement, Proclaimed, Revolt, Founded }

    /// <summary>
    /// Antes da fundação o jogador é um "assentamento comum": sem território no Hegemony, sem fronteira (F4), sem
    /// submissão. Vale em qualquer mundo com o Hegemony. Quem já é vassalo (save antigo) conta como fundado.
    /// </summary>
    public static class EQNation
    {
        /// <summary>Ligado pela integração com o Hegemony (sem ele não há o que travar).</summary>
        public static bool HegemonyPresent;

        /// <summary>O executor de testes considera a nação fundada (os roteiros antigos da cerimônia); "nation lock" desliga.</summary>
        public static bool TestAssumeFounded;

        /// <summary>Teste: motivo do veto do Hegemony a uma invasão de conquista (atacante, colônia), ou null.</summary>
        public static Func<Faction, MapParent, string> InvasionVetoProbe;

        public const string NotNationReason = "A potência não aceita a vassalagem de um sítio; volte quando tiver um trono e um país em volta dele.";
        public const string NoFrontierReason = "Sem nação, sem fronteira: proclame um líder no trono e vença a revolta dos vizinhos.";

        public static bool Applies => HegemonyPresent && NationDef.Active != null;

        public static bool Founded
        {
            get
            {
                if (!Applies || TestAssumeFounded) return true;
                var c = WorldComponent_EQNation.Current;
                return c == null || c.IsFounded;
            }
        }

        public static Faction RevoltFaction
        {
            get
            {
                FactionDef def = NationDef.Active?.revoltFaction;
                if (def == null || Find.FactionManager == null) return null;
                Faction f = Find.FactionManager.FirstFactionOfDef(def);
                if (f != null) return f;
                // Save antigo ou mundo sem a facção: cria na hora.
                try
                {
                    f = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(def));
                    Find.FactionManager.Add(f);
                }
                catch (Exception ex) { Log.Error("[Ecos da Queda] Não consegui criar a facção da revolta: " + ex); }
                return Find.FactionManager.FirstFactionOfDef(def);
            }
        }

        public static int AdultFreeColonists() =>
            EQLeader.AllFreeColonists().Distinct().Count(p => p.RaceProps.Humanlike && DevelopmentalStageExtensions.Adult(p.DevelopmentalStage) && !p.IsQuestLodger());

        public static bool IsThroneRoom(Building_Throne throne)
        {
            if (throne == null || !throne.Spawned) return false;
            Room r = throne.GetRoom();
            return r != null && r.Role == RoomRoleDefOf.ThroneRoom && RoomRoleWorker_ThroneRoom.Validate(r) == null;
        }

        /// <summary>null = pode proclamar; senão o motivo.</summary>
        public static string CanProclaim(Building_Throne throne)
        {
            var c = WorldComponent_EQNation.Current;
            NationDef d = NationDef.Active;
            if (!Applies || c == null || d == null) return "Indisponível";
            if (Founded) return "A nação já foi fundada.";
            if (!ModsConfig.RoyaltyActive) return "Exige a DLC Royalty.";
            if (c.stage == NationStage.Proclaimed || c.stage == NationStage.Revolt) return "A revolta já está a caminho. Agora é segurar o trono.";
            int now = Find.TickManager.TicksGame;
            if (now < c.retryTick) return "Os vizinhos ainda comemoram a última vitória. Nova tentativa em " + ((c.retryTick - now) / (float)GenDate.TicksPerDay).ToString("0.#") + " dias.";
            if (throne == null || !throne.Spawned) return "Precisa de um trono.";
            if (!IsThroneRoom(throne)) return "O trono precisa estar numa sala do trono de verdade (fechada e com teto).";
            if (throne.Map?.IsPlayerHome != true) return "O trono precisa estar numa colônia.";
            int adults = AdultFreeColonists();
            if (adults < d.minAdults) return "Um país precisa de súditos: faltam " + (d.minAdults - adults) + " adultos livres (mínimo " + d.minAdults + ").";
            if (EQLeader.LeaderRole == null) return "A ideologia da colônia não tem o cargo de líder.";
            if (EQLeader.Candidates().Count == 0) return "Ninguém pode ser líder: é preciso um adulto livre que siga a ideologia e cumpra as exigências do cargo de líder.";
            return null;
        }

        public static float RevoltPoints(Map map, int failures)
        {
            NationDef d = NationDef.Active;
            float baseP = map != null ? StorytellerUtility.DefaultThreatPointsNow(map) : 500f;
            float p = baseP * d.revoltPointsFactor * Mathf.Pow(1f + d.failGrowth, failures) * EQCouncil.RevoltFactor;
            return Mathf.Clamp(p, d.revoltMinPoints, d.revoltMaxPoints);
        }

        public static int BribeCost(float points) => Mathf.CeilToInt(points * NationDef.Active.bribeSilverPerPoint / 10f) * 10;

        /// <summary>Último relatório (para o executor de testes).</summary>
        public static string LastReport;

        public static Command ProclaimCommand(Building_Throne throne)
        {
            var cmd = new Command_Action
            {
                defaultLabel = "Proclamar líder",
                defaultDesc = "Diz em voz alta, do trono, que esta colônia agora é um país e quem manda nele. O escolhido vira o líder da ideologia."
                              + "\n\nOs povos da região não vão gostar: uma revolta chega em 1 a 2 dias. Vencer funda a nação (território, fronteira, "
                              + "e a possibilidade de jurar lealdade a uma potência).",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/ThroneSpeech", false) ?? BaseContent.BadTex,
                action = () =>
                {
                    List<Pawn> cands = EQLeader.Candidates();
                    var opts = cands.Select(p => new FloatMenuOption((p.Name?.ToStringFull ?? p.LabelShort), () =>
                    {
                        string err = WorldComponent_EQNation.Current?.Proclaim(p, throne);
                        if (err != null) Messages.Message(err, MessageTypeDefOf.RejectInput, false);
                    })).ToList();
                    if (opts.Count > 0) Find.WindowStack.Add(new FloatMenu(opts, "Quem será proclamado?"));
                }
            };
            string why = CanProclaim(throne);
            if (why != null) cmd.Disable(why);
            return cmd;
        }
    }

    /// <summary>Estado da nação do jogador: proclamação, revolta, fundação.</summary>
    public class WorldComponent_EQNation : WorldComponent
    {
        public bool founded;
        public NationStage stage = NationStage.Settlement;
        public Pawn leader;
        public Building_Throne throne;
        public Map map;
        public int revoltTick = -1;
        public float points;
        public bool bribed;
        public int failures;
        public int retryTick = -1;
        public int throneTicks;
        public int revoltStartTick = -1;
        public List<Pawn> revolters = new List<Pawn>();
        public string lastOutcome;
        public bool faithPending;

        public WorldComponent_EQNation(World world) : base(world) { }

        public static WorldComponent_EQNation Current => Find.World?.GetComponent<WorldComponent_EQNation>();

        /// <summary>Fundada, ou já vassala de uma potência (save antigo, ou vassalagem por fora).</summary>
        public bool IsFounded
        {
            get
            {
                if (founded) return true;
                if (WorldComponent_Allegiance.Current?.memberOf != null || EQAllegiance.PlayerOverlord != null)
                {
                    founded = true;
                    stage = NationStage.Founded;
                    return true;
                }
                return false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref founded, "founded");
            Scribe_Values.Look(ref stage, "stage", NationStage.Settlement);
            Scribe_References.Look(ref leader, "leader");
            Scribe_References.Look(ref throne, "throne");
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref revoltTick, "revoltTick", -1);
            Scribe_Values.Look(ref points, "points");
            Scribe_Values.Look(ref bribed, "bribed");
            Scribe_Values.Look(ref failures, "failures");
            Scribe_Values.Look(ref retryTick, "retryTick", -1);
            Scribe_Values.Look(ref throneTicks, "throneTicks");
            Scribe_Values.Look(ref revoltStartTick, "revoltStartTick", -1);
            Scribe_Collections.Look(ref revolters, "revolters", LookMode.Reference);
            Scribe_Values.Look(ref lastOutcome, "lastOutcome");
            Scribe_Values.Look(ref faithPending, "faithPending");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (revolters == null) revolters = new List<Pawn>();
                revolters.RemoveAll(p => p == null);
            }
        }

        // ------------------------------------------------------------ proclamação

        public string Proclaim(Pawn p, Building_Throne th)
        {
            string why = EQNation.CanProclaim(th);
            if (why != null) return why;
            if (!EQLeader.Candidates().Contains(p)) return p.LabelShort + " não pode ser líder da ideologia.";
            if (EQLeader.IdeoLeader != p && !EQLeader.Assign(p)) return "Não consegui fazer de " + p.LabelShort + " o líder da ideologia.";
            WorldComponent_Allegiance.Current?.LeaderTick();
            NationDef d = NationDef.Active;
            leader = p;
            throne = th;
            map = th?.Map ?? p.MapHeld ?? Find.AnyPlayerHomeMap;
            stage = NationStage.Proclaimed;
            bribed = false;
            throneTicks = 0;
            revolters.Clear();
            points = EQNation.RevoltPoints(map, failures);
            revoltTick = Find.TickManager.TicksGame + (int)(Rand.Range(d.revoltDelayMinDays, d.revoltDelayMaxDays) * GenDate.TicksPerDay);
            SendWarning();
            EQNation.LastReport = "proclamado " + p.LabelShort + "; revolta em " + ((revoltTick - Find.TickManager.TicksGame) / (float)GenDate.TicksPerDay).ToString("0.0") + " dias com " + points.ToString("0") + " pontos";
            return null;
        }

        public int ApproxPeople => Mathf.Max(3, Mathf.RoundToInt(points / 85f));

        private void SendWarning()
        {
            string text = leader.LabelShort + " subiu ao trono e disse a palavra \"nação\" em voz alta. A notícia chegou aos vizinhos antes do arauto."
                          + "\n\nOs povos da região (agricultores, andarilhos, um ou outro veterano e um síndico com um abaixo-assinado) se reúnem para impedir "
                          + "que alguém mande na região. Chegam em 1 a 2 dias: cerca de " + ApproxPeople + " pessoas com tochas, foices e opiniões."
                          + "\n\nVencer a revolta funda a nação. Se " + leader.LabelShort + " morrer ou for capturado, ou se eles ficarem "
                          + NationDef.Active.throneHoldHours.ToString("0") + " horas na sala do trono, a proclamação falha.";
            var letter = (ChoiceLetter_EQProclamation)LetterMaker.MakeLetter("Proclamação: " + leader.LabelShort, text,
                DefDatabase<LetterDef>.GetNamed("EQ_ProclamationLetter"), new LookTargets(leader));
            Find.LetterStack.ReceiveLetter(letter);
        }

        /// <summary>Suborno: a vontade popular tem tabela de preços. null = feito.</summary>
        public string Bribe()
        {
            if (stage != NationStage.Proclaimed) return "Não há revolta para comprar.";
            if (bribed) return "Os que se vendiam já foram comprados.";
            int cost = EQNation.BribeCost(points);
            Map m = map ?? Find.AnyPlayerHomeMap;
            if (EQPlayerFrontier.SilverOn(m) < cost) return "Faltam " + cost + " de prata na colônia.";
            TakeSilver(m, cost);
            NationDef d = NationDef.Active;
            float cut = Rand.Range(d.bribeMinCut, d.bribeMaxCut);
            points *= 1f - cut;
            bribed = true;
            EQNation.LastReport = "suborno de " + cost + " de prata: -" + (cut * 100f).ToString("0") + "% (" + points.ToString("0") + " pontos)";
            Messages.Message("Parte dos vizinhos descobriu que o seu país tem bons argumentos (" + cost + " de prata). A revolta vem " + (cut * 100f).ToString("0") + "% menor.",
                MessageTypeDefOf.NeutralEvent, false);
            return null;
        }

        private static void TakeSilver(Map m, int amount)
        {
            foreach (Thing t in m.listerThings.ThingsOfDef(ThingDefOf.Silver).Where(t => !t.IsForbidden(Faction.OfPlayer)).ToList())
            {
                if (amount <= 0) break;
                int take = Math.Min(amount, t.stackCount);
                t.SplitOff(take).Destroy();
                amount -= take;
            }
        }

        public void GiveUp()
        {
            if (stage != NationStage.Proclaimed) return;
            stage = NationStage.Settlement;
            revoltTick = -1;
            lastOutcome = "desistiu";
            EQNation.LastReport = "proclamação cancelada";
            Messages.Message("A proclamação foi \"adiada por tempo indeterminado\". " + (leader?.LabelShort ?? "O líder") + " continua líder da ideologia; o país fica para outro dia.",
                MessageTypeDefOf.NeutralEvent, false);
        }

        // ------------------------------------------------------------ revolta

        public override void WorldComponentTick()
        {
            if (!EQNation.Applies) return;
            int now = Find.TickManager.TicksGame;
            if (now % 250 != 0) return;
            if (faithPending && now % 1000 == 0 && !EQLeader.SuppressDialogs && Find.WindowStack != null) EQNationFaith.OpenChooser(this);
            if (stage == NationStage.Proclaimed && now >= revoltTick) StartRevolt();
            else if (stage == NationStage.Revolt) CheckRevolt();
        }

        public void StartRevolt()
        {
            Map m = map != null && Find.Maps.Contains(map) ? map : Find.AnyPlayerHomeMap;
            Faction f = EQNation.RevoltFaction;
            if (m == null || f == null)
            {
                Log.Warning("[Ecos da Queda] Revolta sem mapa ou sem facção; a proclamação fica para depois.");
                stage = NationStage.Settlement;
                return;
            }
            map = m;
            var before = new HashSet<Pawn>(m.mapPawns.SpawnedPawnsInFaction(f));
            var parms = new IncidentParms
            {
                target = m,
                faction = f,
                points = points,
                forced = true,
                raidStrategy = RaidStrategyDefOf.ImmediateAttack,
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
                customLetterLabel = "Revolta: os povos da região",
                customLetterText = "Os vizinhos chegaram para desfazer a proclamação de " + (leader?.LabelShort ?? "seu líder")
                                   + ". O síndico traz o abaixo-assinado; o resto traz o resto.\n\nVença a revolta para fundar a nação.",
                customLetterDef = LetterDefOf.ThreatBig,
            };
            bool ok = false;
            try { ok = IncidentDefOf.RaidEnemy.Worker.TryExecute(parms); }
            catch (Exception ex) { Log.Warning("[Ecos da Queda] Raid da revolta falhou: " + ex.Message); }
            revolters = m.mapPawns.SpawnedPawnsInFaction(f).Where(p => !before.Contains(p)).ToList();
            if (!ok || revolters.Count == 0)
            {
                // Plano B: gera e põe na borda.
                var gp = new PawnGroupMakerParms { groupKind = PawnGroupKindDefOf.Combat, faction = f, points = points, tile = m.Tile };
                List<Pawn> pawns = PawnGroupMakerUtility.GeneratePawns(gp).ToList();
                if (pawns.Count > 0 && RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entry, m, CellFinder.EdgeRoadChance_Hostile))
                {
                    foreach (Pawn p in pawns) GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(entry, m, 6), m);
                    LordMaker.MakeNewLord(f, new LordJob_AssaultColony(f, canKidnap: true, canTimeoutOrFlee: true), m, pawns);
                    Find.LetterStack.ReceiveLetter(parms.customLetterLabel, parms.customLetterText, LetterDefOf.ThreatBig, new LookTargets(pawns));
                    revolters = pawns;
                }
            }
            stage = NationStage.Revolt;
            revoltStartTick = Find.TickManager.TicksGame;
            throneTicks = 0;
            EQNation.LastReport = "revolta com " + revolters.Count + " pessoas (" + points.ToString("0") + " pontos): "
                                  + string.Join(",", revolters.GroupBy(p => p.kindDef.defName).Select(g => g.Key + "x" + g.Count()));
            if (revolters.Count == 0) Victory();
        }

        public static bool OutOfFight(Pawn p, Map m) =>
            p == null || p.Dead || p.Destroyed || p.Downed || !p.Spawned || p.Map != m || p.IsPrisonerOfColony || p.Faction == null || !p.Faction.HostileTo(Faction.OfPlayer);

        public bool LeaderLost => leader == null || leader.Dead || leader.Destroyed || leader.Faction != Faction.OfPlayer || leader.IsPrisoner;

        public void CheckRevolt()
        {
            if (LeaderLost) { Defeat(leader != null && leader.Dead ? "o líder morreu" : "o líder foi capturado"); return; }
            Room room = throne != null && throne.Spawned ? throne.GetRoom() : null;
            if (room != null && revolters.Any(p => !OutOfFight(p, map) && p.GetRoom() == room))
            {
                throneTicks += 250;
                if (throneTicks >= NationDef.Active.throneHoldHours * GenDate.TicksPerHour) { Defeat("os revoltosos tomaram a sala do trono"); return; }
            }
            if (revolters.All(p => OutOfFight(p, map))) Victory();
        }

        public void Victory()
        {
            stage = NationStage.Founded;
            founded = true;
            lastOutcome = "vitória";
            Faction pf = Faction.OfPlayer;
            EQNation.LastReport = "nação fundada: " + pf.Name;
            Find.LetterStack.ReceiveLetter("Nação fundada",
                "Os vizinhos voltaram para casa (os que puderam). O abaixo-assinado foi arquivado em local seguro: debaixo do trono."
                + "\n\n" + pf.Name + " agora é uma nação, com " + (leader?.LabelShort ?? "o líder proclamado") + " no trono. "
                + "A colônia passa a ter território e fronteira, e as potências aceitam ouvir propostas de vassalagem (aba Reputação).",
                LetterDefOf.PositiveEvent, leader != null && leader.Spawned ? new LookTargets(leader) : LookTargets.Invalid);
            if (!EQLeader.SuppressDialogs && Find.WindowStack != null)
                Find.WindowStack.Add(new Dialog_NamePlayerFaction());
            EQNationFaith.BeginFounding(this);
        }

        public void Defeat(string why)
        {
            NationDef d = NationDef.Active;
            stage = NationStage.Settlement;
            failures++;
            retryTick = Find.TickManager.TicksGame + (int)(d.retryDays * GenDate.TicksPerDay);
            lastOutcome = "derrota: " + why;
            EQNation.LastReport = "proclamação falhou (" + why + "); nova tentativa em " + d.retryDays.ToString("0") + " dias, revolta +" + (d.failGrowth * 100f).ToString("0") + "%";
            // Quem sobrou volta para casa, contando vantagem.
            Faction f = EQNation.RevoltFaction;
            var left = revolters.Where(p => !OutOfFight(p, map)).ToList();
            if (left.Count > 0 && map != null && f != null)
            {
                foreach (Pawn p in left) p.GetLord()?.RemovePawn(p);
                LordMaker.MakeNewLord(f, new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Walk, canDig: false), map, left);
            }
            Find.LetterStack.ReceiveLetter("A proclamação falhou",
                "A região não reconhece o seu país: " + why + ". Os vizinhos voltam para casa com a sensação de dever cumprido e um abaixo-assinado mais comprido."
                + "\n\nNova proclamação só daqui a " + d.retryDays.ToString("0") + " dias, e a próxima revolta vem " + (d.failGrowth * 100f).ToString("0") + "% maior (agora eles têm experiência).",
                LetterDefOf.NegativeEvent);
        }

        public string Report()
        {
            int now = Find.TickManager.TicksGame;
            return "nação: aplica=" + EQNation.Applies + " fundada=" + EQNation.Founded + " (comp=" + founded + ", testes=" + EQNation.TestAssumeFounded + ")"
                   + " etapa=" + stage + " falhas=" + failures
                   + (retryTick > now ? " espera=" + ((retryTick - now) / (float)GenDate.TicksPerDay).ToString("0.0") + "d" : "")
                   + (stage == NationStage.Proclaimed ? " revolta_em=" + ((revoltTick - now) / (float)GenDate.TicksPerDay).ToString("0.00") + "d pontos=" + points.ToString("0") + " suborno=" + bribed : "")
                   + (stage == NationStage.Revolt ? " revoltosos=" + revolters.Count(p => !OutOfFight(p, map)) + "/" + revolters.Count + " trono_ocupado=" + throneTicks + "t" : "")
                   + " líder=" + (leader?.LabelShort ?? "-") + " último=" + (lastOutcome ?? "-")
                   + " ideologia=" + (EQNationFaith.PlayerIdeo?.name ?? "-") + " clássica=" + EQNationFaith.PlayerIdeoClassic
                   + " fluida=" + (EQNationFaith.PlayerIdeo?.Fluid == true) + " memes=" + (EQNationFaith.PlayerIdeo?.memes.Count ?? 0)
                   + (faithPending ? " fé_pendente" : "");
        }
    }

    /// <summary>Carta da proclamação: esperar, comprar parte dos vizinhos, desistir.</summary>
    public class ChoiceLetter_EQProclamation : ChoiceLetter
    {
        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                var c = WorldComponent_EQNation.Current;
                if (ArchivedOnly || c == null || c.stage != NationStage.Proclaimed)
                {
                    yield return Option_Close;
                    yield break;
                }
                var wait = new DiaOption("Esperar a revolta") { resolveTree = true };
                wait.action = () => Find.LetterStack.RemoveLetter(this);
                yield return wait;
                int cost = EQNation.BribeCost(c.points);
                var bribe = new DiaOption("Comprar parte dos vizinhos (" + cost + " de prata: a revolta vem 30–40% menor)") { resolveTree = true };
                if (c.bribed) bribe.Disable("já comprados");
                else if (EQPlayerFrontier.SilverOn(c.map ?? Find.AnyPlayerHomeMap) < cost) bribe.Disable("falta prata");
                bribe.action = () => { c.Bribe(); };
                yield return bribe;
                var give = new DiaOption("Desistir da proclamação") { resolveTree = true };
                give.action = () => { c.GiveUp(); Find.LetterStack.RemoveLetter(this); };
                yield return give;
                yield return Option_Postpone;
            }
        }
    }

    [HarmonyPatch(typeof(Building_Throne), nameof(Building_Throne.GetGizmos))]
    public static class Patch_Building_Throne_GetGizmos_Proclaim
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_Throne __instance)
        {
            foreach (Gizmo g in __result) yield return g;
            if (EQNation.Applies && !EQNation.Founded && __instance.Faction == Faction.OfPlayer)
                yield return EQNation.ProclaimCommand(__instance);
        }
    }
}

namespace EcosDaQueda
{
    /// <summary>A fé das nossas facções ocultas (Povos da Região, Fungobi) não vai para andarilhos nem conversões:
    /// o jogo base sorteia a ideologia de quem chega entre todas, inclusive as escondidas.</summary>
    [HarmonyPatch(typeof(IdeoUtility), nameof(IdeoUtility.IdeoChangeToWeight))]
    public static class Patch_IdeoUtility_IdeoChangeToWeight_HiddenEQ
    {
        public static void Postfix(Ideo ideo, ref float __result)
        {
            if (__result <= 0f || ideo == null || Find.FactionManager == null) return;
            foreach (Faction f in Find.FactionManager.AllFactionsListForReading)
                if (f.def.hiddenIdeo && f.def.defName.StartsWith("EQ_") && f.ideos?.PrimaryIdeo == ideo)
                {
                    __result = 0f;
                    return;
                }
        }
    }
}
