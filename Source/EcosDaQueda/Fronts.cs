using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace EcosDaQueda
{
    /// <summary>
    /// Etapa 10e: frente de trincheira. Quando um exército chega ao alvo e a batalha é "jogada" (exército sob o comando
    /// do jogador no modo jogado, ou exército da patrona por sorte), abre-se uma frente no mapa-múndi por 3 dias.
    /// Sem visita: resolve de forma abstrata no fim do prazo. Com uma caravana do jogador: gera o mapa com duas linhas
    /// de trincheira (Trenches; sacos de areia sem ele), os soldados dos dois lados (pontos pela força) e uma onda de
    /// reforço; depois de meio dia o atacante avança. O lado que ficar sem combatentes perde; o resultado vale como a
    /// batalha do exército (conquista pelo Hegemony, favor, faltas). O jogador apoia um lado, fica de fora ou saqueia.
    /// </summary>
    public class WorldObject_EQFront : MapParent
    {
        public WorldObject_EQArmy army;
        public Faction attacker;
        public Faction defender;
        public Settlement targetSettlement;
        public int attackerStrength;
        public int defenderStrength;
        public int openedTick;
        public int deadlineTick;
        public int mapGeneratedTick = -1;
        public bool waveSent;
        public bool assaultStarted;
        public bool resolved;
        public bool playerVisited;
        /// <summary>Relação entre os dois lados antes da frente (para devolver se não eram hostis).</summary>
        public bool wereHostile = true;
        public int prevGoodwill;
        public string result;
        /// <summary>Etapa 10e (2º bloco): frente de uma invasão do próprio Hegemony (ID do marcador dele; -1 = exército nosso).</summary>
        public int hegemonyMarker = -1;
        /// <summary>Resultado da invasão no Hegemony, quando chega (ele decide o assentamento; a frente é a escaramuça ao lado).</summary>
        public string hegemonyResult;
        /// <summary>Lado que o jogador apoia (null = de fora); sideChosen = já escolheu na primeira entrada.</summary>
        public Faction supported;
        public bool sideChosen;
        /// <summary>Etapa 10d (resto): frente de fronteira entre territórios em guerra (sem exército nem invasão).</summary>
        public bool borderFront;
        public Settlement attackerSettlement;

        public const float OpenDays = 3f;
        public const float HegemonyOpenDays = 4f;
        public const float MapTimeoutDays = 2f;
        public const float PointsPerSoldier = 40f;
        public const float MaxPointsPerSide = 3000f;

        public override string Label => "Frente: " + (attacker?.Name ?? "?") + " × " + (defender?.Name ?? "?");

        public bool IsHegemonyFront => hegemonyMarker >= 0;

        public Faction Opponent(Faction side) => side == attacker ? defender : side == defender ? attacker : null;

        public override string GetInspectString()
        {
            string s = base.GetInspectString();
            if (!s.NullOrEmpty()) s += "\n";
            if (resolved) return s + "Batalha decidida: " + result;
            s += "Atacante: " + attacker?.Name + " (" + attackerStrength + " soldados)\nDefensor: " + defender?.Name + " (" + defenderStrength + " soldados)";
            if (IsHegemonyFront) s += "\nInvasão do Hegemony contra " + (targetSettlement?.LabelCap ?? "?") + (hegemonyResult != null ? ": " + hegemonyResult : "");
            if (sideChosen) s += "\nVocê: " + (supported != null ? "apoia " + supported.Name : "de fora");
            if (!HasMap)
                s += "\nSe ninguém for lá, a batalha se decide sozinha em " + ((deadlineTick - Find.TickManager.TicksGame) / 60000f).ToString("0.0") + " dias.";
            return s;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan)) yield return o;
            for (int side = -1; side <= 1; side++)
            {
                int sd = side;
                Faction f = sd < 0 ? null : sd == 0 ? attacker : defender;
                if (sideChosen && f != supported) continue; // depois da primeira entrada, o lado fica
                string label = f == null ? "Entrar na frente sem tomar partido" : "Entrar na frente apoiando " + f.Name;
                foreach (FloatMenuOption o in CaravanArrivalActionUtility.GetFloatMenuOptions(
                             () => SideReport(f), () => new CaravanArrivalAction_EQFront(this, sd), label, caravan, Tile, this))
                    yield return o;
            }
        }

        public FloatMenuAcceptanceReport SideReport(Faction f)
        {
            if (resolved && !HasMap) return FloatMenuAcceptanceReport.WithFailReason("a batalha acabou");
            if (f != null && !sideChosen && f.HostileTo(Faction.OfPlayer)) return FloatMenuAcceptanceReport.WithFailReason(f.Name + " é hostil a você");
            return FloatMenuAcceptanceReport.WasAccepted;
        }

        /// <summary>Tomar partido: Fama com o apoiado na hora, Infâmia e hostilidade do outro lado (para lutar no mapa).</summary>
        public string ChooseSide(Faction side)
        {
            if (sideChosen) return "lado já escolhido: " + (supported?.Name ?? "de fora");
            sideChosen = true;
            supported = side;
            if (side == null) return "de fora";
            Faction other = Opponent(side);
            WorldComponent_Allegiance.ApplyOutcome(side, 2f, 0f, 0);
            if (other != null && !other.defeated)
            {
                int gw = Faction.OfPlayer.GoodwillWith(other);
                int delta = Math.Min(-25, -80 - gw); // hostil (abaixo de -75) para o mapa; quem já era hostil perde 25
                WorldComponent_Allegiance.ApplyOutcome(other, 0f, 3f, delta);
            }
            Messages.Message("Você tomou o partido de " + side.Name + (other != null ? "; " + other.Name + " não vai esquecer." : "."), MessageTypeDefOf.NeutralEvent);
            return "apoia " + side.Name + "; " + other?.Name + " agora " + (other != null && other.HostileTo(Faction.OfPlayer) ? "hostil" : "não hostil");
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            // A batalha continua mesmo sem o jogador no mapa (fecha por exaustão em até MapTimeoutDays).
            alsoRemoveWorldObject = resolved;
            return resolved && !Map.mapPawns.AnyPawnBlockingMapRemoval;
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (resolved)
            {
                if (!HasMap && !Destroyed) Destroy(); // com mapa, a remoção dele já destrói o objeto
                return;
            }
            int now = Find.TickManager.TicksGame;
            if (!HasMap)
            {
                if (now >= deadlineTick || IsHegemonyFront && hegemonyResult != null) ResolveAbstract();
                return;
            }
            Check(now);
        }

        /// <summary>Mapa aberto: onda de reforço, avanço do atacante, fim da batalha.</summary>
        public string Check(int now)
        {
            if (resolved || !HasMap) return "nada";
            float hours = (now - mapGeneratedTick) / 2500f;
            if (!waveSent && hours >= 6f)
            {
                waveSent = true;
                SpawnSide(attacker, attackerStrength * 0.4f, true, wave: true);
                SpawnSide(defender, defenderStrength * 0.4f, false, wave: true);
                Messages.Message(Label + ": chegou a segunda onda dos dois lados.", MessageTypeDefOf.ThreatSmall);
            }
            if (!assaultStarted && hours >= 12f)
            {
                assaultStarted = true;
                var pawns = Fighters(attacker).ToList();
                if (pawns.Count > 0)
                {
                    foreach (Pawn p in pawns) p.GetLord()?.Notify_PawnLost(p, PawnLostCondition.ForcedByPlayerAction);
                    LordMaker.MakeNewLord(attacker, new LordJob_AssaultColony(attacker, canKidnap: false, canTimeoutOrFlee: false, canSteal: false), Map, pawns);
                    Messages.Message(Label + ": " + attacker.Name + " sai das trincheiras e avança.", MessageTypeDefOf.ThreatSmall);
                }
            }
            if (hours < 1f) return "começando";
            int a = Fighters(attacker).Count(), d = Fighters(defender).Count();
            if (a == 0 || d == 0)
                return Resolve(a > 0, "no campo (" + a + " × " + d + " de pé)");
            if (now - mapGeneratedTick >= MapTimeoutDays * 60000)
                return Resolve(a * 1.0f >= d * 1.2f, "por exaustão (" + a + " × " + d + " de pé)");
            return "lutando " + a + " × " + d;
        }

        public IEnumerable<Pawn> Fighters(Faction f)
        {
            if (!HasMap) yield break;
            foreach (Pawn p in Map.mapPawns.AllPawnsSpawned)
                if (p.Faction == f && !p.Downed && !p.Dead && !p.IsPrisoner && (p.RaceProps.Humanlike || p.RaceProps.IsMechanoid))
                    yield return p;
        }

        public string ResolveAbstract()
        {
            if (resolved) return "já resolvida";
            if (borderFront)
            {
                bool atkWon = Rand.Chance(Mathf.Clamp((float)attackerStrength / Mathf.Max(1, attackerStrength + defenderStrength), 0.15f, 0.85f));
                result = "sem testemunhas: " + (atkWon ? attacker?.Name : defender?.Name) + " venceu. " + EQWarDiplomacy.BorderOutcome(this, atkWon);
            }
            else if (IsHegemonyFront)
                result = "sem testemunhas: " + (hegemonyResult ?? "a invasão não deu notícia");
            else if (army != null && !army.Destroyed)
            {
                army.front = null;
                string r = army.Battle();
                result = "sem testemunhas: " + r;
            }
            else result = "o exército sumiu antes da batalha";
            resolved = true;
            RestoreRelation();
            return result;
        }

        public string Resolve(bool attackerWon, string how)
        {
            if (resolved) return "já resolvida";
            resolved = true;
            string r;
            if (borderFront)
                r = EQWarDiplomacy.BorderOutcome(this, attackerWon);
            else if (IsHegemonyFront)
                r = "A escaramuça não decide a cidade: " + (hegemonyResult ?? "a invasão do Hegemony segue diante de " + targetSettlement?.LabelCap + ".");
            else
            {
                r = "o exército sumiu";
                if (army != null && !army.Destroyed)
                {
                    army.front = null;
                    r = army.Battle(forcedWin: attackerWon, fromFront: true);
                }
            }
            result = (attackerWon ? attacker?.Name : defender?.Name) + " venceu " + how + ". " + r;
            string extra = playerVisited ? Reward(attackerWon) : "";
            Find.LetterStack.ReceiveLetter("Fim da frente: " + (attackerWon ? attacker?.Name : defender?.Name) + " venceu",
                Label + ": " + result + extra + "\n\nO que ficou no chão (armas, armaduras, corpos) é de quem chegar primeiro.",
                LetterDefOf.NeutralEvent, HasMap ? new LookTargets(Map.Center, Map) : LookTargets.Invalid);
            RestoreRelation();
            return result;
        }

        public static string LastReward;

        /// <summary>Recompensa de quem tomou partido: vitória do apoiado = Fama, boa vontade (e favor, se for a patrona).</summary>
        private string Reward(bool attackerWon)
        {
            Faction winner = attackerWon ? attacker : defender;
            if (supported == null) { LastReward = "de fora"; return "\n\nVocê ficou de fora: ninguém agradece, ninguém cobra."; }
            bool won = supported == winner;
            WorldComponent_Allegiance.ApplyOutcome(supported, won ? 6f : 2f, 0f, won ? 12 : 4);
            string s = "\n\nVocê lutou por " + supported.Name + (won ? ", e ganhou: a gratidão vem com Fama e boa vontade." : ", e perdeu: ao menos viram você lá.");
            Faction patron = WorldComponent_Allegiance.Current?.memberOf;
            if (patron != null && patron == supported)
            {
                int favor = won ? 5 : 1;
                EQFavor.Award(patron, favor, "frente de trincheira");
                s += " +" + favor + " de favor com " + patron.Name + ".";
            }
            else if (patron != null && patron == Opponent(supported))
            {
                WorldComponent_Allegiance.ApplyOutcome(patron, 0f, 4f, -15);
                s += " " + patron.Name + ", a quem você jurou, viu de que lado você atirava.";
            }
            LastReward = (won ? "venceu" : "perdeu") + " com " + supported.Name;
            return s;
        }

        private void RestoreRelation()
        {
            if (wereHostile || attacker == null || defender == null || attacker.defeated || defender.defeated) return;
            if (EQForces.AtWar(attacker, defender)) return; // a guerra continua: ficam hostis
            EQFronts.SetGoodwill(attacker, defender, prevGoodwill);
        }

        // ------------------------------------------------------------ mapa

        public Map GenerateFrontMap()
        {
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(Tile, new IntVec3(150, 1, 150), def);
            if (mapGeneratedTick < 0)
            {
                mapGeneratedTick = Find.TickManager.TicksGame;
                BuildLine(map, attacker, -1);
                BuildLine(map, defender, +1);
                SpawnSide(attacker, attackerStrength * 0.6f, true, wave: false);
                SpawnSide(defender, defenderStrength * 0.6f, false, wave: false);
            }
            return map;
        }

        public static string LastMapReport;

        private int LineX(Map map, int side) => map.Center.x + side * 15;

        /// <summary>Linha de trincheira (Trenches; sacos de areia sem ele) e arame na frente, construídas já prontas.</summary>
        private void BuildLine(Map map, Faction f, int side)
        {
            SiegeTrenchDef s = EQSieges.For(f) ?? DefDatabase<SiegeTrenchDef>.AllDefsListForReading.FirstOrDefault(d => d.trench != null);
            ThingDef trench = s?.trench ?? ThingDefOf.Sandbags;
            ThingDef stuff = s?.trench != null ? s.trenchStuff : GenStuff.DefaultStuffFor(ThingDefOf.Sandbags);
            int x = LineX(map, side);
            int built = 0, wires = 0;
            for (int z = map.Center.z - 22; z <= map.Center.z + 22; z++)
            {
                if (TrySpawn(map, trench, stuff, new IntVec3(x, 0, z), f)) built++;
                if (s?.wire != null && (z - map.Center.z + 100) % 6 < 4 && TrySpawn(map, s.wire, s.wireStuff, new IntVec3(x - side * 4, 0, z), f)) wires++;
            }
            LastMapReport = (LastMapReport == null || side < 0 ? "" : LastMapReport + "; ") + f?.def.defName + ": " + trench.defName + "=" + built + " arame=" + wires;
        }

        private static bool TrySpawn(Map map, ThingDef def, ThingDef stuff, IntVec3 c, Faction f)
        {
            if (def == null || !c.InBounds(map) || c.Impassable(map) || c.GetEdifice(map) != null || c.GetFirstItem(map) != null && def.passability == Traversability.Impassable) return false;
            if (def.MadeFromStuff && stuff == null) stuff = GenStuff.DefaultStuffFor(def);
            if (!c.GetTerrain(map).affordances.Contains(def.terrainAffordanceNeeded ?? TerrainAffordanceDefOf.Light)) return false;
            foreach (Thing t in c.GetThingList(map).ToList())
                if (t.def.category == ThingCategory.Plant || t.def.category == ThingCategory.Filth) t.Destroy();
            Thing thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? stuff : null);
            if (thing.def.CanHaveFaction) thing.SetFaction(f);
            GenSpawn.Spawn(thing, c, map, Rot4.North);
            return true;
        }

        public static int LastSpawned;

        /// <summary>Gera os combatentes de um lado (grupo de combate da facção) atrás da linha dele, defendendo a trincheira.</summary>
        private void SpawnSide(Faction f, float soldiers, bool isAttacker, bool wave)
        {
            Map map = Map;
            if (map == null || f == null || soldiers < 1f) return;
            float points = Mathf.Clamp(soldiers * PointsPerSoldier, 200f, MaxPointsPerSide);
            var parms = new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                faction = f,
                points = points,
                tile = Tile,
                generateFightersOnly = true,
                dontUseSingleUseRocketLaunchers = true,
            };
            List<Pawn> pawns;
            try { pawns = PawnGroupMakerUtility.GeneratePawns(parms).ToList(); }
            catch (Exception ex) { Log.Warning("[Ecos da Queda] frente: falha ao gerar soldados de " + f.Name + ": " + ex.Message); return; }
            int side = isAttacker ? -1 : 1;
            int lineX = LineX(map, side);
            int backX = wave ? (side < 0 ? 2 : map.Size.x - 3) : lineX + side * 4;
            foreach (Pawn p in pawns)
            {
                IntVec3 near = new IntVec3(backX, 0, map.Center.z + Rand.RangeInclusive(-18, 18));
                if (!CellFinder.TryFindRandomCellNear(near, map, 6, c => c.Standable(map) && !c.Fogged(map), out IntVec3 cell))
                    cell = CellFinder.RandomClosewalkCellNear(near, map, 10);
                GenSpawn.Spawn(p, cell, map);
            }
            LastSpawned = pawns.Count;
            if (pawns.Count > 0)
            {
                LordJob job = isAttacker && assaultStarted
                    ? (LordJob)new LordJob_AssaultColony(f, canKidnap: false, canTimeoutOrFlee: false, canSteal: false)
                    : new LordJob_DefendPoint(new IntVec3(lineX - side, 0, map.Center.z), wanderRadius: 4f, defendRadius: 22f, addFleeToil: false);
                LordMaker.MakeNewLord(f, job, map, pawns);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref army, "army");
            Scribe_References.Look(ref attacker, "attacker");
            Scribe_References.Look(ref defender, "defender");
            Scribe_References.Look(ref targetSettlement, "targetSettlement");
            Scribe_Values.Look(ref attackerStrength, "attackerStrength");
            Scribe_Values.Look(ref defenderStrength, "defenderStrength");
            Scribe_Values.Look(ref openedTick, "openedTick");
            Scribe_Values.Look(ref deadlineTick, "deadlineTick");
            Scribe_Values.Look(ref mapGeneratedTick, "mapGeneratedTick", -1);
            Scribe_Values.Look(ref waveSent, "waveSent");
            Scribe_Values.Look(ref assaultStarted, "assaultStarted");
            Scribe_Values.Look(ref resolved, "resolved");
            Scribe_Values.Look(ref playerVisited, "playerVisited");
            Scribe_Values.Look(ref wereHostile, "wereHostile", true);
            Scribe_Values.Look(ref prevGoodwill, "prevGoodwill");
            Scribe_Values.Look(ref result, "result");
            Scribe_Values.Look(ref hegemonyMarker, "hegemonyMarker", -1);
            Scribe_Values.Look(ref hegemonyResult, "hegemonyResult");
            Scribe_References.Look(ref supported, "supported");
            Scribe_Values.Look(ref sideChosen, "sideChosen");
            Scribe_Values.Look(ref borderFront, "borderFront");
            Scribe_References.Look(ref attackerSettlement, "attackerSettlement");
        }
    }

    public class CaravanArrivalAction_EQFront : CaravanArrivalAction
    {
        private WorldObject_EQFront front;
        /// <summary>-1 de fora, 0 atacante, 1 defensor.</summary>
        private int side = -1;

        public CaravanArrivalAction_EQFront() { }
        public CaravanArrivalAction_EQFront(WorldObject_EQFront front, int side = -1) { this.front = front; this.side = side; }

        public override string Label => "Entrar na frente";
        public override string ReportString => "indo para a " + (front?.Label ?? "frente");

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
        {
            if (front == null || front.Destroyed || front.Tile != destinationTile) return false;
            return true;
        }

        public override void Arrived(Caravan caravan)
        {
            bool fresh = !front.HasMap;
            if (!front.sideChosen) front.ChooseSide(side < 0 ? null : side == 0 ? front.attacker : front.defender);
            Map map = front.GenerateFrontMap();
            front.playerVisited = true;
            CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
            if (fresh)
                Find.LetterStack.ReceiveLetter(front.Label,
                    "A caravana chegou a uma frente de trincheira: " + front.attacker?.Name + " de um lado, " + front.defender?.Name + " do outro, e muita lama no meio.\n\n"
                    + (front.supported != null ? "Você veio lutar por " + front.supported.Name + ": quem vencer lembra de quem estava na lama ao lado. " : "Você veio sem bandeira: fique de fora e espere o fim para recolher o que sobrar. ")
                    + "A segunda onda chega em umas 6 horas; em 12, o atacante sai das trincheiras.",
                    LetterDefOf.ThreatBig, new LookTargets(map.Center, map));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref front, "front");
            Scribe_Values.Look(ref side, "side", -1);
        }
    }

    public static class EQFronts
    {
        public static WorldObjectDef FrontDef => DefDatabase<WorldObjectDef>.GetNamedSilentFail("EQ_TrenchFront");

        /// <summary>Chance de um exército da patrona (não comandado) abrir frente em vez de lutar na hora. Teste: forces frontchance.</summary>
        public static float PatronFrontChance = 0.35f;

        /// <summary>Tile livre e transitável perto do alvo (até 3 anéis), sem outro objeto do mundo.</summary>
        public static PlanetTile FindFrontTile(PlanetTile near)
        {
            var seen = new HashSet<PlanetTile> { near };
            var ring = new List<PlanetTile> { near };
            var tmp = new List<PlanetTile>();
            for (int r = 0; r < 3; r++)
            {
                var next = new List<PlanetTile>();
                foreach (PlanetTile t in ring)
                {
                    tmp.Clear();
                    Find.WorldGrid.GetTileNeighbors(t, tmp);
                    foreach (PlanetTile n in tmp)
                    {
                        if (!seen.Add(n)) continue;
                        next.Add(n);
                        if (!Find.World.Impassable(n) && !Find.WorldObjects.AnyWorldObjectAt(n)) return n;
                    }
                }
                ring = next;
            }
            return PlanetTile.Invalid;
        }

        /// <summary>Boa vontade entre duas facções NPC (a relação vem dela; SetRelationDirect não vale para quem usa boa vontade).</summary>
        public static void SetGoodwill(Faction a, Faction b, int target)
        {
            int delta = target - a.GoodwillWith(b);
            if (delta != 0) a.TryAffectGoodwillWith(b, delta, canSendMessage: false, canSendHostilityLetter: false);
        }

        // ------------------------------------------------------------ invasões do Hegemony (2º bloco)

        /// <summary>Invasão do Hegemony que chegou (Arrived) ou acabou (Resolved), avisada fora do tique e processada no próximo.</summary>
        public class InvasionNote
        {
            public bool arrived;
            public Faction attacker, defender, winner;
            public MapParent location;
            public int marker, forceSize;
        }

        public static readonly List<InvasionNote> PendingInvasions = new List<InvasionNote>();

        /// <summary>Chance de uma invasão do Hegemony entre NPCs abrir frente; no máximo MaxHegemonyFronts abertas. Teste: forces hegfrontchance.</summary>
        public static float HegemonyFrontChance = 0.5f;
        public const int MaxHegemonyFronts = 2;
        public static string LastInvasion;

        public static void ProcessPending()
        {
            List<InvasionNote> notes;
            lock (PendingInvasions)
            {
                if (PendingInvasions.Count == 0) return;
                notes = PendingInvasions.ToList();
                PendingInvasions.Clear();
            }
            foreach (InvasionNote n in notes)
            {
                try { LastInvasion = n.arrived ? OpenForInvasion(n, false) : OnInvasionResolved(n.marker, n.winner); }
                catch (Exception ex) { Log.Warning("[Ecos da Queda] frente do Hegemony: " + ex.Message); }
            }
        }

        /// <summary>Por que não abre frente para esta invasão (null = pode).</summary>
        public static string HegemonyFrontBlock(Faction atk, Faction def, MapParent loc)
        {
            if (FrontDef == null) return "sem def";
            if (atk == null || def == null || atk == def) return "lados inválidos";
            if (atk.IsPlayer || def.IsPlayer || loc == null || loc.Faction == Faction.OfPlayer) return "envolve o jogador";
            if (atk.defeated || def.defeated) return "facção derrotada";
            if (!(Involved(atk) || Involved(def))) return "nenhum lado é nosso";
            var open = Find.WorldObjects.AllWorldObjects.OfType<WorldObject_EQFront>().Where(f => f.IsHegemonyFront && !f.resolved).ToList();
            if (open.Count >= MaxHegemonyFronts) return "frentes demais abertas";
            if (open.Any(f => f.targetSettlement != null && f.targetSettlement == loc)) return "já há frente diante deste alvo";
            return null;
        }

        private static bool Involved(Faction f) => EQAllegiance.IsPower(f) || FactionRelationPresetDef.Involves(f.def);

        public static string OpenForInvasion(InvasionNote n, bool force)
        {
            Faction atk = EQUtil.Canonical(n.attacker), def = EQUtil.Canonical(n.defender);
            string block = HegemonyFrontBlock(atk, def, n.location);
            if (block != null) return "sem frente: " + block;
            if (!force && !Rand.Chance(HegemonyFrontChance)) return "sem frente: sorte";
            PlanetTile tile = FindFrontTile(n.location.Tile);
            if (!tile.Valid) return "sem frente: sem tile livre";
            var comp = WorldComponent_EQForces.Current;
            var front = (WorldObject_EQFront)WorldObjectMaker.MakeWorldObject(FrontDef);
            front.Tile = tile;
            front.SetFaction(atk);
            front.hegemonyMarker = Math.Max(0, n.marker);
            front.attacker = atk;
            front.defender = def;
            front.targetSettlement = n.location as Settlement;
            front.attackerStrength = Mathf.Clamp(n.forceSize > 0 ? n.forceSize : 20, 8, 80);
            front.defenderStrength = Mathf.Clamp(front.targetSettlement != null && comp != null ? comp.GarrisonOf(front.targetSettlement) : 20, 8, 80);
            front.openedTick = Find.TickManager.TicksGame;
            front.deadlineTick = front.openedTick + (int)(WorldObject_EQFront.HegemonyOpenDays * 60000);
            front.wereHostile = atk.HostileTo(def);
            if (!front.wereHostile)
            {
                front.prevGoodwill = atk.GoodwillWith(def);
                SetGoodwill(atk, def, -100);
            }
            Find.WorldObjects.Add(front);
            Find.LetterStack.ReceiveLetter("Frente de trincheira: " + n.location.LabelCap,
                atk.Name + " chegou a " + n.location.LabelCap + " (" + def.Name + ") com " + front.attackerStrength + " soldados, e os dois lados cavaram trincheiras na lama ao lado.\n\n"
                + "A cidade se decide entre eles. Mas a escaramuça na linha de frente está aberta a quem chegar: lute por um lado (Fama e boa vontade de quem vencer; o outro lado não esquece), "
                + "fique de fora ou só recolha o que sobrar. Sem testemunhas, a frente fecha quando a invasão acabar.",
                LetterDefOf.NeutralEvent, front);
            comp?.Log("Frente aberta (invasão do Hegemony): " + front.Label + " diante de " + n.location.LabelCap + ".", false);
            return "frente aberta: " + front.Label + " força=" + front.attackerStrength + "x" + front.defenderStrength;
        }

        public static string OnInvasionResolved(int marker, Faction winner)
        {
            WorldObject_EQFront front = Find.WorldObjects.AllWorldObjects.OfType<WorldObject_EQFront>()
                .FirstOrDefault(f => f.IsHegemonyFront && !f.resolved && f.hegemonyMarker == Math.Max(0, marker));
            if (front == null) return "invasão resolvida sem frente";
            winner = EQUtil.Canonical(winner);
            front.hegemonyResult = winner == null ? "a invasão recuou sem vencedor claro."
                : winner == front.attacker ? front.attacker.Name + " tomou a cidade." : front.defender?.Name + " segurou a cidade.";
            if (front.HasMap)
                Messages.Message(front.Label + ": " + front.hegemonyResult + " A escaramuça continua.", MessageTypeDefOf.NeutralEvent);
            return "invasão resolvida: " + front.hegemonyResult + (front.HasMap ? " (mapa aberto, segue)" : " (fecha no próximo tique)");
        }

        /// <summary>Abre a frente para o exército que chegou ao alvo. null se não deu (luta na hora).</summary>
        public static WorldObject_EQFront Open(WorldObject_EQArmy army)
        {
            if (FrontDef == null || army?.target == null) return null;
            PlanetTile tile = FindFrontTile(army.target.Tile);
            if (!tile.Valid) return null;
            var comp = WorldComponent_EQForces.Current;
            var front = (WorldObject_EQFront)WorldObjectMaker.MakeWorldObject(FrontDef);
            front.Tile = tile;
            front.SetFaction(army.Faction);
            front.army = army;
            front.attacker = EQUtil.Canonical(army.Faction);
            front.defender = EQUtil.Canonical(army.enemy);
            front.targetSettlement = army.target;
            front.attackerStrength = army.strength;
            front.defenderStrength = comp?.GarrisonOf(army.target) ?? 30;
            front.openedTick = Find.TickManager.TicksGame;
            front.deadlineTick = front.openedTick + (int)(WorldObject_EQFront.OpenDays * 60000);
            front.wereHostile = front.attacker.HostileTo(front.defender);
            if (!front.wereHostile)
            {
                front.prevGoodwill = front.attacker.GoodwillWith(front.defender);
                SetGoodwill(front.attacker, front.defender, -100);
            }
            Find.WorldObjects.Add(front);
            army.front = front;
            army.Tile = tile;
            Find.LetterStack.ReceiveLetter("Frente de trincheira: " + army.target.LabelCap,
                army.Label + " cavou trincheiras diante de " + army.target.LabelCap + " (" + front.defender?.Name + "). "
                + front.attackerStrength + " soldados contra " + front.defenderStrength + " da guarnição.\n\n"
                + "Mande uma caravana para lutar ao lado de quem quiser (ou de ninguém) e recolher o que sobrar. Em "
                + WorldObject_EQFront.OpenDays.ToString("0") + " dias, sem testemunhas, a batalha se decide sozinha.",
                LetterDefOf.ThreatSmall, front);
            comp?.Log("Frente aberta: " + front.Label + " diante de " + army.target.LabelCap + ".", false);
            return front;
        }
    }
}
