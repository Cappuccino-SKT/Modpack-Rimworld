using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Etapa 10d: tropas das potências como objetos do mundo.
    /// - Cada potência tem um tesouro de guerra (renda diária pelo território, manutenção por soldado).
    /// - Cada assentamento tem uma guarnição abstrata (número de soldados, se recompõe devagar).
    /// - Em guerra (Hegemony, ou guerra forçada pelo teste), a potência levanta exércitos que marcham pelo
    ///   mapa-múndi até o assentamento inimigo mais próximo e lutam de forma abstrata contra a guarnição.
    /// - Vitória: toma o assentamento pelo Hegemony (TryTransferSettlement) se houver guerra formal; senão saqueia.
    /// - O jogador vassalo recebe a chamada às armas (PowerEventDef warCall): emprestar colono, pagar o soldo ou recusar.
    /// O módulo do Hegemony preenche os provedores abaixo; sem ele só valem as guerras forçadas.
    /// </summary>
    public static class EQForces
    {
        /// <summary>Guerras em curso (pares de facções canônicas). Preenchido pelo módulo do Hegemony.</summary>
        public static Func<IEnumerable<KeyValuePair<Faction, Faction>>> WarsProvider;
        /// <summary>(assentamento, novo dono, id da operação) → null se tomou; senão o motivo da recusa.</summary>
        public static Func<Settlement, Faction, string, string> ConquestHandler;
        /// <summary>Tiles de território da facção (renda do tesouro).</summary>
        public static Func<Faction, int> TerritoryProvider;
        /// <summary>Declarar guerra pelo Hegemony (debug/eventos): null = ok; senão o motivo.</summary>
        public static Func<Faction, Faction, string, string> DeclareWarHandler;

        public const int SoldierCost = 20;          // prata do tesouro por soldado recrutado
        public const float UpkeepPerSoldier = 1.5f; // prata por soldado por dia
        public const int MaxArmiesPerPower = 2;
        public const int TicksPerTile = 7500;       // ~8 tiles por dia
        public const float MobilizeMtbDays = 4f;

        public static WorldObjectDef ArmyDef => DefDatabase<WorldObjectDef>.GetNamedSilentFail("EQ_Army");

        public static bool Valid(Faction f) => f != null && !f.defeated && !f.IsPlayer && !EQUtil.IsProxy(f);

        /// <summary>Guerras em curso: as do Hegemony + as forçadas (teste). Sem duplicatas, só facções válidas.</summary>
        public static List<KeyValuePair<Faction, Faction>> Wars()
        {
            var list = new List<KeyValuePair<Faction, Faction>>();
            void Add(Faction a, Faction b)
            {
                a = EQUtil.Canonical(a); b = EQUtil.Canonical(b);
                if (!Valid(a) || !Valid(b) || a == b) return;
                if (list.Any(p => p.Key == a && p.Value == b || p.Key == b && p.Value == a)) return;
                list.Add(new KeyValuePair<Faction, Faction>(a, b));
            }
            if (WarsProvider != null)
            {
                try { foreach (var p in WarsProvider()) Add(p.Key, p.Value); }
                catch (Exception ex) { Log.ErrorOnce("[Ecos da Queda] Falha ao ler guerras do Hegemony: " + ex, 0x3E0A11); }
            }
            var comp = WorldComponent_EQForces.Current;
            if (comp != null)
                foreach (var fw in comp.forcedWars)
                    Add(EQUtil.MainFaction(DefDatabase<FactionDef>.GetNamedSilentFail(fw.Split('|')[0])),
                        EQUtil.MainFaction(DefDatabase<FactionDef>.GetNamedSilentFail(fw.Split('|').ElementAtOrDefault(1) ?? "")));
            return list;
        }

        public static bool AtWar(Faction a, Faction b)
        {
            a = EQUtil.Canonical(a); b = EQUtil.Canonical(b);
            return Wars().Any(p => p.Key == a && p.Value == b || p.Key == b && p.Value == a);
        }

        public static IEnumerable<Faction> EnemiesOf(Faction f)
        {
            f = EQUtil.Canonical(f);
            foreach (var p in Wars())
            {
                if (p.Key == f) yield return p.Value;
                else if (p.Value == f) yield return p.Key;
            }
        }

        public static IEnumerable<Settlement> SettlementsOf(Faction f)
        {
            f = EQUtil.Canonical(f);
            return Find.WorldObjects.Settlements.Where(s => s.Faction != null && EQUtil.Canonical(s.Faction) == f);
        }

        public static float Distance(PlanetTile a, PlanetTile b) => Find.WorldGrid.ApproxDistanceInTiles(a, b);

        /// <summary>Par (base de partida, alvo) mais próximo entre as duas facções.</summary>
        public static bool TryFindFront(Faction power, Faction enemy, out Settlement home, out Settlement target)
        {
            home = null; target = null;
            float best = float.MaxValue;
            var mine = SettlementsOf(power).ToList();
            var theirs = SettlementsOf(enemy).ToList();
            foreach (Settlement h in mine)
                foreach (Settlement t in theirs)
                {
                    if (h.Tile.Layer != t.Tile.Layer) continue;
                    float d = Distance(h.Tile, t.Tile);
                    if (d < best) { best = d; home = h; target = t; }
                }
            return home != null;
        }

        public static string PlayerRole(WorldObject_EQArmy army)
        {
            Faction patron = WorldComponent_Allegiance.Current?.memberOf;
            if (army.playerSupport > 0) return "apoiado";
            if (patron == null) return null;
            if (army.Faction == patron) return "patrona";
            if (army.enemy == patron) return "inimiga";
            return null;
        }
    }

    /// <summary>Etapa 11c: postura do exército comandado pelo jogador.</summary>
    public enum EQPosture { Normal, Agressiva, Cautelosa }

    /// <summary>Exército de uma potência em marcha no mapa-múndi.</summary>
    public class WorldObject_EQArmy : WorldObject
    {
        public int armyId;
        public Settlement home;
        public Settlement target;
        public Faction enemy;
        public int strength;
        public int initialStrength;
        public float morale = 0.7f;
        /// <summary>Força vinda do jogador (colonos emprestados + soldo pago).</summary>
        public int playerSupport;
        public int playerColonists;
        public bool returning;
        public int raisedTick;
        public int lastStepTick;
        /// <summary>Etapa 11c: comandado pelo líder do jogador (cargos 5+): alvo, postura, soldo e recuo são dele.</summary>
        public bool playerCommand;
        public EQPosture posture;
        /// <summary>Soldo pago até este tique; depois disso a moral cai todo dia.</summary>
        public int paidUntilTick;
        /// <summary>Etapa 10e: frente de trincheira aberta por este exército (espera nela até a batalha acabar).</summary>
        public WorldObject_EQFront front;
        /// <summary>Etapa 10e: exército comandado decide no modo jogado (abre frente ao chegar).</summary>
        public bool playedBattle;

        public float AttackFactor => posture == EQPosture.Agressiva ? 1.25f : posture == EQPosture.Cautelosa ? 0.85f : 1f;
        public float LossFactor => posture == EQPosture.Agressiva ? 1.3f : posture == EQPosture.Cautelosa ? 0.6f : 1f;
        public int SoldoCost => Mathf.CeilToInt(strength * EQForces.UpkeepPerSoldier * WorldComponent_EQForces.SoldoDays);

        public override string Label => "Exército nº " + armyId + " (" + (Faction?.Name ?? "?") + ")" + (playerCommand ? " [seu comando]" : "");

        public Settlement Destination => returning ? home : target;

        public int TilesLeft => Destination == null ? 0 : (int)Math.Ceiling(EQForces.Distance(Tile, Destination.Tile));

        public override string GetInspectString()
        {
            string s = base.GetInspectString();
            if (!s.NullOrEmpty()) s += "\n";
            s += "Força: " + strength + " soldados   Moral: " + morale.ToStringPercent();
            if (Destination != null)
                s += "\n" + (returning ? "Voltando para " : "Marchando contra ") + Destination.LabelCap
                    + " (" + TilesLeft + " tiles, ~" + (TilesLeft * EQForces.TicksPerTile / 60000f).ToString("0.#") + " dias)";
            if (playerCommand)
            {
                int left = paidUntilTick - Find.TickManager.TicksGame;
                s += "\nSob o seu comando. Postura: " + posture.ToString().ToLower() + ". Soldo "
                    + (left > 0 ? "pago por mais " + (left / 60000f).ToString("0.0") + " dias" : "ATRASADO (a moral cai todo dia)") + ".";
            }
            if (playerSupport > 0)
                s += "\nApoio da sua colônia: " + playerSupport + " de força" + (playerColonists > 0 ? " (" + playerColonists + " colono(s))" : "");
            return s;
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            int now = Find.TickManager.TicksGame;
            if (now - lastStepTick >= EQForces.TicksPerTile)
            {
                lastStepTick = now;
                Step();
            }
        }

        /// <summary>Um passo de marcha (um tile); chegando, luta ou se recolhe.</summary>
        public string Step()
        {
            if (Destroyed) return "destruído";
            if (front != null && !front.Destroyed && !front.resolved) return "na frente";
            front = null;
            Settlement dest = Destination;
            if (!returning && (target == null || target.Destroyed || target.Faction == null || EQUtil.Canonical(target.Faction) != EQUtil.Canonical(enemy)
                               || !EQForces.AtWar(Faction, enemy)))
            {
                returning = true;
                dest = home;
                WorldComponent_EQForces.Current?.Log(Label + ": alvo perdido ou guerra encerrada; volta para casa.", EQForces.PlayerRole(this) != null);
            }
            if (dest == null || dest.Destroyed || dest.Faction == null || (returning && EQUtil.Canonical(dest.Faction) != EQUtil.Canonical(Faction)))
            {
                // Casa perdida: tenta outra base da potência; senão se dispersa.
                home = EQForces.SettlementsOf(Faction).OrderBy(s => EQForces.Distance(Tile, s.Tile)).FirstOrDefault();
                if (home == null) { Disband("sem base para onde voltar"); return "dispersou"; }
                returning = true;
                dest = home;
            }
            if (Tile == dest.Tile || EQForces.Distance(Tile, dest.Tile) <= 1.01f)
                return Arrive();
            var neighbors = new List<PlanetTile>();
            Find.WorldGrid.GetTileNeighbors(Tile, neighbors);
            PlanetTile best = Tile;
            float bestD = EQForces.Distance(Tile, dest.Tile);
            foreach (PlanetTile n in neighbors)
            {
                float d = EQForces.Distance(n, dest.Tile);
                if (d < bestD) { bestD = d; best = n; }
            }
            if (best == Tile) return Arrive(); // não há como chegar mais perto: chega assim mesmo
            Tile = best;
            return "marcha (" + TilesLeft + " tiles)";
        }

        public string Arrive()
        {
            var comp = WorldComponent_EQForces.Current;
            if (returning)
            {
                if (home != null && comp != null) comp.AddGarrison(home, strength);
                comp?.Log(Label + " voltou para " + (home?.LabelCap ?? "casa") + " com " + strength + " soldados.", EQForces.PlayerRole(this) != null);
                Destroy();
                return "voltou";
            }
            string role = EQForces.PlayerRole(this);
            bool open = playerCommand ? playedBattle : role == "patrona" && Rand.Chance(EQFronts.PatronFrontChance);
            if (open && EQFronts.Open(this) != null) return "frente aberta";
            return Battle();
        }

        /// <summary>Batalha abstrata contra a guarnição do alvo.</summary>
        public string Battle(bool? forcedWin = null, bool fromFront = false)
        {
            var comp = WorldComponent_EQForces.Current;
            if (comp == null || target == null) return "sem alvo";
            int garrison = comp.GarrisonOf(target);
            int before = strength;
            float att = strength * (0.5f + morale) * AttackFactor * Rand.Range(0.75f, 1.25f);
            float def = garrison * 1.2f * Rand.Range(0.75f, 1.25f);
            bool win = forcedWin ?? att > def;
            int attLoss = (int)Math.Round(Math.Min(1f, (win ? Rand.Range(0.1f, 0.3f) : Rand.Range(0.5f, 0.8f)) * LossFactor) * strength);
            int defLoss = (int)Math.Round(garrison * (win ? Rand.Range(0.6f, 0.9f) : Rand.Range(0.15f, 0.35f)));
            strength = Math.Max(0, strength - attLoss);
            comp.SetGarrison(target, garrison - defLoss);
            string where = target.LabelCap;
            string role = EQForces.PlayerRole(this);
            string result;
            if (win)
            {
                morale = Math.Min(1f, morale + 0.15f);
                string why = EQForces.ConquestHandler == null ? "sem Hegemony" : null;
                if (why == null)
                {
                    try { why = EQForces.ConquestHandler(target, Faction, "eqarmy-" + armyId + "-" + target.ID); }
                    catch (Exception ex) { why = ex.Message; }
                }
                if (why == null)
                {
                    comp.SetGarrison(target, strength / 2);
                    result = Faction.Name + " tomou " + where + " (" + before + " contra " + garrison + "; perdas " + attLoss + "/" + defLoss + ").";
                    strength -= strength / 2;
                }
                else
                    result = Faction.Name + " saqueou " + where + " (" + before + " contra " + garrison + ", " + why + "; perdas " + attLoss + "/" + defLoss + ").";
                comp.RecordBattle(this, true, result);
                returning = true;
            }
            else
            {
                morale = Math.Max(0f, morale - 0.25f);
                result = Faction.Name + " foi repelida em " + where + " (" + before + " contra " + garrison + "; perdas " + attLoss + "/" + defLoss + ").";
                comp.RecordBattle(this, false, result);
                returning = true;
            }
            if (strength <= 0) { playerCommand = false; Disband("aniquilado"); // a derrota já contou a falta
                return (win ? "vitória" : "derrota") + "; aniquilado"; }
            return (win ? "vitória: " : "derrota: ") + result;
        }

        public void Disband(string why)
        {
            if (playerCommand && !Destroyed)
                WorldComponent_EQForces.Current?.CommandFailed(this, "o " + Label + " se dispersou sob o seu comando (" + why + ")");
            WorldComponent_EQForces.Current?.Log(Label + " se dispersou (" + why + ").", EQForces.PlayerRole(this) != null);
            if (!Destroyed) Destroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref armyId, "armyId");
            Scribe_References.Look(ref home, "home");
            Scribe_References.Look(ref target, "target");
            Scribe_References.Look(ref enemy, "enemy");
            Scribe_Values.Look(ref strength, "strength");
            Scribe_Values.Look(ref initialStrength, "initialStrength");
            Scribe_Values.Look(ref morale, "morale", 0.7f);
            Scribe_Values.Look(ref playerSupport, "playerSupport");
            Scribe_Values.Look(ref playerColonists, "playerColonists");
            Scribe_Values.Look(ref returning, "returning");
            Scribe_Values.Look(ref raisedTick, "raisedTick");
            Scribe_Values.Look(ref lastStepTick, "lastStepTick");
            Scribe_Values.Look(ref playerCommand, "playerCommand");
            Scribe_Values.Look(ref posture, "posture");
            Scribe_Values.Look(ref paidUntilTick, "paidUntilTick");
            Scribe_References.Look(ref front, "front");
            Scribe_Values.Look(ref playedBattle, "playedBattle");
        }

        // ------------------------------------------------------------ Etapa 11c: comando do jogador

        /// <summary>Alvos possíveis: assentamentos dos inimigos em guerra com a potência, do mais perto ao mais longe.</summary>
        public List<Settlement> TargetOptions()
        {
            var list = new List<Settlement>();
            foreach (Faction e in EQForces.EnemiesOf(Faction))
                list.AddRange(EQForces.SettlementsOf(e).Where(s => s.Tile.Layer == Tile.Layer));
            return list.OrderBy(s => EQForces.Distance(Tile, s.Tile)).Take(8).ToList();
        }

        public string SetTarget(Settlement s)
        {
            if (s == null) return "sem alvo";
            target = s;
            enemy = EQUtil.Canonical(s.Faction);
            returning = false;
            return "alvo: " + s.LabelCap + " (" + TilesLeft + " tiles)";
        }

        public string PaySoldo()
        {
            Map map = Find.AnyPlayerHomeMap;
            int cost = SoldoCost;
            if (map == null) return "sem colônia";
            if (EQItems.CountOnMap(map, ThingDefOf.Silver) < cost) return "falta prata (" + cost + ")";
            EQItems.TakeFromMap(map, ThingDefOf.Silver, cost);
            int now = Find.TickManager.TicksGame;
            paidUntilTick = Math.Max(paidUntilTick, now) + WorldComponent_EQForces.SoldoDays * 60000;
            morale = Math.Min(1f, morale + 0.05f);
            Messages.Message("Soldo do " + Label + " pago (" + cost + " de prata, " + WorldComponent_EQForces.SoldoDays + " dias).", this, MessageTypeDefOf.PositiveEvent);
            return "pago " + cost;
        }

        /// <summary>Recuar: volta para casa; a potência anota (perde favor, sem falta).</summary>
        public string Retreat()
        {
            if (returning) return "já está voltando";
            if (front != null && !front.Destroyed && !front.resolved)
            {
                if (front.HasMap) return "a batalha na frente já começou";
                front.resolved = true;
                front.result = "o atacante recuou antes da batalha";
                front = null;
            }
            returning = true;
            WorldComponent_EQForces.Current?.CommandRetreated(this);
            return "recuando";
        }

        private static Texture2D icoAttack, icoPosture, icoPay, icoRetreat, icoPlayed;
        private static Texture2D Icon(ref Texture2D cache, string path)
        {
            if (cache == null) cache = ContentFinder<Texture2D>.Get(path, false) ?? BaseContent.BadTex;
            return cache;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos()) yield return g;
            if (!playerCommand) yield break;
            yield return new Command_Action
            {
                defaultLabel = "Escolher alvo",
                defaultDesc = "Escolha qual assentamento inimigo o exército ataca (os 8 mais próximos das facções em guerra com " + Faction?.Name + ").",
                icon = Icon(ref icoAttack, "UI/Commands/Attack"),
                action = () =>
                {
                    var opts = TargetOptions().Select(s => new FloatMenuOption(s.LabelCap + " (" + s.Faction?.Name + ", " + Mathf.CeilToInt(EQForces.Distance(Tile, s.Tile)) + " tiles)",
                        () => Messages.Message(SetTarget(s), this, MessageTypeDefOf.NeutralEvent))).ToList();
                    if (opts.Count == 0) opts.Add(new FloatMenuOption("Nenhum inimigo ao alcance", null));
                    Find.WindowStack.Add(new FloatMenu(opts));
                }
            };
            yield return new Command_Action
            {
                defaultLabel = "Postura: " + posture.ToString().ToLower(),
                defaultDesc = "Agressiva: ataca 25% mais forte e perde 30% mais gente. Cautelosa: ataca 15% mais fraco e perde 40% menos. Normal: nem uma coisa nem outra.",
                icon = Icon(ref icoPosture, "UI/Commands/Draft"),
                action = () =>
                {
                    var opts = new List<FloatMenuOption>();
                    foreach (EQPosture p in Enum.GetValues(typeof(EQPosture)))
                    {
                        EQPosture pp = p;
                        opts.Add(new FloatMenuOption(pp.ToString(), () => posture = pp));
                    }
                    Find.WindowStack.Add(new FloatMenu(opts));
                }
            };
            var pay = new Command_Action
            {
                defaultLabel = "Pagar soldo (" + SoldoCost + ")",
                defaultDesc = "Paga " + WorldComponent_EQForces.SoldoDays + " dias de soldo com a prata da colônia. Soldo atrasado derruba a moral todo dia; moral no chão é deserção (e uma falta para você).",
                icon = Icon(ref icoPay, "Things/Item/Resource/Silver/Silver_c"),
                action = () => Messages.Message(PaySoldo(), this, MessageTypeDefOf.NeutralEvent)
            };
            Map map = Find.AnyPlayerHomeMap;
            if (map == null || EQItems.CountOnMap(map, ThingDefOf.Silver) < SoldoCost) pay.Disable("Falta prata na colônia (" + SoldoCost + ")");
            yield return pay;
            yield return new Command_Toggle
            {
                defaultLabel = "Batalha jogada",
                defaultDesc = "Ligado: ao chegar ao alvo, o exército cava uma frente de trincheira no mapa-múndi e espera 3 dias; mande uma caravana para lutar no mapa. Desligado: a batalha é simulada na hora.",
                icon = Icon(ref icoPlayed, "UI/Commands/Attack"),
                isActive = () => playedBattle,
                toggleAction = () => playedBattle = !playedBattle
            };
            var back = new Command_Action
            {
                defaultLabel = "Recuar",
                defaultDesc = "Volta para casa sem lutar. A potência não pune com falta, mas tira favor: ninguém gosta de general que dá meia-volta.",
                icon = Icon(ref icoRetreat, "UI/Commands/ReleaseAnimals"),
                action = () => Messages.Message(Retreat(), this, MessageTypeDefOf.NeutralEvent)
            };
            if (returning) back.Disable("Já está voltando");
            yield return back;
        }
    }

    public class WorldComponent_EQForces : WorldComponent
    {
        /// <summary>Guarnição por assentamento (ID do objeto do mundo → soldados).</summary>
        public Dictionary<int, int> garrisons = new Dictionary<int, int>();
        /// <summary>Tesouro de guerra por potência (defName → prata abstrata).</summary>
        public Dictionary<string, float> treasury = new Dictionary<string, float>();
        /// <summary>Guerras forçadas pelo teste ("defA|defB").</summary>
        public List<string> forcedWars = new List<string>();
        public List<string> log = new List<string>();
        public int nextArmyId = 1;
        public int lastCallTick = -9999999;
        /// <summary>Guerras recém-começadas (avisadas pelo Hegemony fora do tique; processadas no próximo).</summary>
        public static readonly List<KeyValuePair<Faction, Faction>> PendingWarStarts = new List<KeyValuePair<Faction, Faction>>();

        public const float CallCooldownDays = 5f;
        /// <summary>Etapa 11c: comando a partir do 5º cargo (índice 4); dias de soldo por pagamento.</summary>
        public const int CommandMinRank = 4;
        public const int SoldoDays = 3;

        public WorldComponent_EQForces(World world) : base(world) { }

        public static WorldComponent_EQForces Current => Find.World?.GetComponent<WorldComponent_EQForces>();

        public IEnumerable<WorldObject_EQArmy> Armies => Find.WorldObjects.AllWorldObjects.OfType<WorldObject_EQArmy>();

        public IEnumerable<WorldObject_EQArmy> ArmiesOf(Faction f) => Armies.Where(a => EQUtil.Canonical(a.Faction) == EQUtil.Canonical(f));

        public static int GarrisonCap(Settlement s) => EQAllegiance.IsPower(EQUtil.Canonical(s.Faction)) ? 60 : 30;

        public int GarrisonOf(Settlement s)
        {
            if (s == null) return 0;
            if (!garrisons.TryGetValue(s.ID, out int n))
            {
                n = GarrisonCap(s) * 2 / 3 + Rand.RangeInclusive(0, 8);
                garrisons[s.ID] = n;
            }
            return n;
        }

        public void SetGarrison(Settlement s, int n) { if (s != null) garrisons[s.ID] = Math.Max(0, n); }

        public void AddGarrison(Settlement s, int n) => SetGarrison(s, GarrisonOf(s) + n);

        public float Treasury(Faction f) => f != null && treasury.TryGetValue(f.def.defName, out float v) ? v : 1500f;

        public void SetTreasury(Faction f, float v) { if (f != null) treasury[f.def.defName] = Math.Max(0f, v); }

        public void Log(string text, bool notify)
        {
            log.Add("[dia " + (Find.TickManager.TicksGame / 60000f).ToString("0.0") + "] " + text);
            if (log.Count > 30) log.RemoveAt(0);
            if (notify) Messages.Message(text, MessageTypeDefOf.NeutralEvent);
        }

        public override void WorldComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (now % 250 == 77 && PendingWarStarts.Count > 0)
            {
                var starts = PendingWarStarts.ToList();
                PendingWarStarts.Clear();
                foreach (var p in starts) OnWarStarted(p.Key, p.Value);
            }
            if (now % 250 == 88)
                EQFronts.ProcessPending();
            if (now % 250 == 99)
                foreach (Letter l in Find.LetterStack.LettersListForReading.ToList())
                    if (l is ChoiceLetter_EQCommand c && !c.resolved && c.deadline >= 0 && now >= c.deadline)
                    {
                        Messages.Message("Comando de campanha sem resposta: conta como recusa.", MessageTypeDefOf.NegativeEvent);
                        c.Refuse();
                        Find.LetterStack.RemoveLetter(c);
                    }
            if (now % 60000 == 7777)
                Daily();
        }

        /// <summary>Uma vez por dia: renda e manutenção, recomposição das guarnições, moral e mobilização.</summary>
        public string Daily()
        {
            var sb = new List<string>();
            foreach (Faction p in EQAllegiance.Powers)
            {
                if (p.defeated) continue;
                int tiles = 0;
                try { tiles = EQForces.TerritoryProvider?.Invoke(p) ?? 0; } catch { }
                float income = 150f + 2f * tiles;
                float upkeep = ArmiesOf(p).Where(a => !a.playerCommand).Sum(a => a.strength) * EQForces.UpkeepPerSoldier;
                float t = Treasury(p) + income - upkeep;
                if (t < 0)
                    foreach (var a in ArmiesOf(p).Where(x => !x.playerCommand)) a.morale = Math.Max(0f, a.morale - 0.1f);
                SetTreasury(p, Math.Min(t, 6000f));
                sb.Add(p.def.defName + " tesouro=" + Treasury(p).ToString("0"));
            }
            foreach (Settlement s in Find.WorldObjects.Settlements)
                if (garrisons.TryGetValue(s.ID, out int g) && g < GarrisonCap(s))
                    garrisons[s.ID] = Math.Min(GarrisonCap(s), g + 2);
            foreach (var a in Armies.ToList())
            {
                if (!a.returning) a.morale = Math.Max(0f, a.morale - (a.posture == EQPosture.Agressiva ? 0.03f : 0.02f));
                if (a.playerCommand && Find.TickManager.TicksGame > a.paidUntilTick)
                {
                    a.morale = Math.Max(0f, a.morale - 0.08f);
                    Messages.Message("Soldo atrasado no " + a.Label + ": moral " + a.morale.ToStringPercent() + ".", a, MessageTypeDefOf.NegativeEvent);
                }
                if (a.morale <= 0.05f) a.Disband("deserção: moral no chão");
            }
            foreach (Faction p in EQAllegiance.Powers)
                if (!p.defeated && Rand.MTBEventOccurs(EQForces.MobilizeMtbDays, 1f, 1f))
                    TryMobilize(p, null, out _);
            return string.Join(", ", sb);
        }

        private void OnWarStarted(Faction a, Faction b)
        {
            a = EQUtil.Canonical(a); b = EQUtil.Canonical(b);
            Faction patron = WorldComponent_Allegiance.Current?.memberOf;
            bool involved = patron != null && (a == patron || b == patron);
            Log("Guerra: " + a?.Name + " × " + b?.Name + "." + (involved ? " (patrona envolvida)" : ""), false);
            if (involved)
                Find.LetterStack.ReceiveLetter("Guerra: " + a.Name + " × " + b.Name,
                    "O Hegemony registra guerra entre " + a.Name + " e " + b.Name + ". Como membro de " + patron.Name
                    + ", espere ser chamado: as potências mobilizam exércitos que marcham pelo mapa-múndi, e quem jurou lealdade paga em gente ou em prata.",
                    LetterDefOf.ThreatSmall, LookTargets.Invalid, patron);
            foreach (Faction f in new[] { a, b })
                if (EQAllegiance.IsPower(f)) TryMobilize(f, f == a ? b : a, out _);
        }

        /// <summary>Levanta um exército contra um inimigo (ou o mais próximo em guerra). null = feito; senão o motivo.</summary>
        public WorldObject_EQArmy TryMobilize(Faction power, Faction enemy, out string reason, int forcedStrength = 0)
        {
            reason = null;
            power = EQUtil.Canonical(power);
            if (!EQForces.Valid(power) || !EQAllegiance.IsPower(power)) { reason = "não é potência"; return null; }
            if (EQForces.ArmyDef == null) { reason = "sem def EQ_Army"; return null; }
            if (ArmiesOf(power).Count() >= EQForces.MaxArmiesPerPower) { reason = "já tem " + EQForces.MaxArmiesPerPower + " exércitos"; return null; }
            var enemies = enemy != null ? new List<Faction> { EQUtil.Canonical(enemy) } : EQForces.EnemiesOf(power).ToList();
            if (enemies.Count == 0) { reason = "sem guerra"; return null; }
            if (enemy != null && !EQForces.AtWar(power, enemy)) { reason = "não está em guerra com " + enemy.Name; return null; }
            Settlement home = null, target = null;
            float best = float.MaxValue;
            foreach (Faction e in enemies)
                if (EQForces.TryFindFront(power, e, out Settlement h, out Settlement t) && EQForces.Distance(h.Tile, t.Tile) < best)
                {
                    best = EQForces.Distance(h.Tile, t.Tile);
                    home = h; target = t; enemy = e;
                }
            if (home == null) { reason = "sem frente (faltam assentamentos)"; return null; }
            float money = Treasury(power);
            int n = forcedStrength > 0 ? forcedStrength : Math.Min(60, (int)(money * 0.6f / EQForces.SoldierCost));
            if (n < 10) { reason = "tesouro baixo (" + money.ToString("0") + ")"; return null; }
            if (forcedStrength <= 0) SetTreasury(power, money - n * EQForces.SoldierCost);
            var army = (WorldObject_EQArmy)WorldObjectMaker.MakeWorldObject(EQForces.ArmyDef);
            army.SetFaction(power);
            army.Tile = home.Tile;
            army.home = home;
            army.target = target;
            army.enemy = enemy;
            army.strength = army.initialStrength = n;
            army.armyId = nextArmyId++;
            army.raisedTick = army.lastStepTick = Find.TickManager.TicksGame;
            Find.WorldObjects.Add(army);
            Faction patron = WorldComponent_Allegiance.Current?.memberOf;
            Log(power.Name + " levantou o " + army.Label + " (" + n + " soldados) em " + home.LabelCap + " contra " + target.LabelCap + " (" + enemy.Name + ").",
                patron != null && (power == patron || enemy == patron));
            if (patron == power && !power.HostileTo(Faction.OfPlayer))
                Log(CanCommand(power) ? "Comando: " + OfferCommand(army) : "Chamada às armas: " + CallToArms(power), false);
            return army;
        }

        /// <summary>Carta de chamada às armas da potência patrona (PowerEventDef com warCall).</summary>
        public string CallToArms(Faction power, bool ignoreCooldown = false)
        {
            int now = Find.TickManager.TicksGame;
            if (!ignoreCooldown && now - lastCallTick < CallCooldownDays * 60000) return "chamada recente";
            PowerEventDef d = DefDatabase<PowerEventDef>.AllDefsListForReading.FirstOrDefault(x => x.warCall && x.faction == power.def);
            var pe = WorldComponent_PowerEvents.Current;
            if (d == null || pe == null) return "sem chamada para " + power.def.defName;
            string why = pe.BlockReason(d, ignoreTime: true);
            if (why != null) return why;
            lastCallTick = now;
            pe.Fire(d);
            return "chamada enviada: " + d.defName;
        }

        /// <summary>Resposta à chamada às armas: reforça o exército da potência que está marchando.</summary>
        public string Support(Faction power, float strength, float morale, bool colonist)
        {
            var army = ArmiesOf(power).Where(a => !a.returning).OrderByDescending(a => a.raisedTick).FirstOrDefault();
            if (army == null)
            {
                // Sem exército em marcha: o reforço vai para a guarnição mais exposta.
                Settlement s = EQForces.SettlementsOf(power).FirstOrDefault();
                if (s != null) AddGarrison(s, (int)strength);
                return "sem exército em marcha; reforço foi para a guarnição";
            }
            army.strength += (int)strength;
            army.playerSupport += (int)strength;
            if (colonist) army.playerColonists++;
            army.morale = Math.Min(1f, army.morale + morale);
            return army.Label + " reforçado (+" + (int)strength + ", moral " + army.morale.ToStringPercent() + ")";
        }

        // ------------------------------------------------------------ Etapa 11c: comando do jogador

        /// <summary>O líder pode comandar exércitos desta potência: membro, cargo 5+, sem outro exército sob comando.</summary>
        public bool CanCommand(Faction power)
        {
            var alg = WorldComponent_Allegiance.Current;
            return alg?.memberOf != null && EQUtil.Canonical(power) == alg.memberOf && alg.RankIndex >= CommandMinRank
                   && alg.leader != null && !Armies.Any(a => a.playerCommand);
        }

        /// <summary>Tamanho do exército que a potência confia ao líder: 30 no 5º cargo, +10 por cargo acima.</summary>
        public static int CommandStrength(int rankIndex) => 30 + 10 * Math.Max(0, rankIndex - CommandMinRank);

        public string OfferCommand(WorldObject_EQArmy army)
        {
            if (army == null || army.Destroyed) return "sem exército";
            var alg = WorldComponent_Allegiance.Current;
            var letter = (ChoiceLetter_EQCommand)LetterMaker.MakeLetter("Comando de campanha: " + army.Faction.Name,
                army.Faction.Name + " levantou o " + army.Label + " contra " + army.target?.LabelCap + " (" + army.enemy?.Name + ").\n\n"
                + "Pelo cargo de " + alg?.leader?.LabelShort + ", a potência oferece o comando: o exército passa a ter "
                + CommandStrength(alg?.RankIndex ?? CommandMinRank) + " soldados, você escolhe o alvo e a postura no mapa-múndi e paga o soldo "
                + "depois dos dois primeiros dias (a potência adianta esses). Vitória rende muito favor e apaga uma falta; derrota ou deserção contam falta.\n\n"
                + "Recusar o comando também conta falta. A potência não esquece quem recusa uma bandeira.",
                DefDatabase<LetterDef>.GetNamed("EQ_CommandLetter"), army, army.Faction);
            letter.army = army;
            letter.deadline = Find.TickManager.TicksGame + 2 * 60000;
            Find.LetterStack.ReceiveLetter(letter);
            return "oferecido: " + army.Label;
        }

        public string AcceptCommand(WorldObject_EQArmy army)
        {
            if (army == null || army.Destroyed) return "o exército já não existe";
            var alg = WorldComponent_Allegiance.Current;
            army.playerCommand = true;
            army.strength = Math.Max(army.strength, CommandStrength(alg?.RankIndex ?? CommandMinRank));
            army.paidUntilTick = Find.TickManager.TicksGame + 2 * 60000;
            Log(alg?.leader?.LabelShort + " assumiu o comando do " + army.Label + ".", true);
            return "comando assumido: " + army.Label + " força=" + army.strength;
        }

        public void RefuseCommand(Faction power)
        {
            WorldComponent_Encargos.LoseFavor(power, 4);
            WorldComponent_PowerEvents.Current?.AddStrike(power);
            Log("Comando recusado: " + power?.Name + " anota uma falta.", true);
        }

        public void CommandFailed(WorldObject_EQArmy army, string text)
        {
            WorldComponent_Encargos.LoseFavor(army.Faction, 4);
            WorldComponent_PowerEvents.Current?.AddStrike(army.Faction);
            Find.LetterStack.ReceiveLetter("Comando fracassado", text.CapitalizeFirst() + ". A potência anota uma falta e tira favor.",
                LetterDefOf.NegativeEvent, LookTargets.Invalid, army.Faction);
        }

        public void CommandRetreated(WorldObject_EQArmy army)
        {
            WorldComponent_Encargos.LoseFavor(army.Faction, 2);
            Log("O " + army.Label + " recuou por ordem sua (favor -2).", true);
        }

        /// <summary>Resultado de uma batalha: carta se envolve o jogador; favor/Fama se ele apoiou.</summary>
        public void RecordBattle(WorldObject_EQArmy army, bool win, string text)
        {
            string role = EQForces.PlayerRole(army);
            Log(text, false);
            LastBattle = text;
            if (role == null) return;
            string extra = "";
            if (army.playerCommand)
            {
                bool took = text.Contains(" tomou ");
                if (win)
                {
                    int favor = 6 + (took ? 4 : 0);
                    EQFavor.Award(army.Faction, favor, "vitória do " + army.Label);
                    WorldComponent_Allegiance.ApplyOutcome(army.Faction, took ? 10f : 6f, 0f, 8);
                    bool cleared = WorldComponent_PowerEvents.Current?.ClearStrike(army.Faction) ?? false;
                    extra = "\n\nVitória sob o seu comando: +" + favor + " de favor" + (cleared ? " e uma falta apagada" : "") + ". O seu nome entra na crônica da potência, provavelmente escrito certo.";
                }
                else
                {
                    WorldComponent_Encargos.LoseFavor(army.Faction, 4);
                    WorldComponent_PowerEvents.Current?.AddStrike(army.Faction);
                    extra = "\n\nDerrota sob o seu comando: -4 de favor e uma falta. Os sobreviventes voltam para casa contando a versão deles.";
                }
            }
            else if (army.playerSupport > 0)
            {
                int favor = win ? 3 + army.playerColonists * 2 : 1;
                EQFavor.Award(army.Faction, favor, "campanha do " + army.Label);
                WorldComponent_Allegiance.ApplyOutcome(army.Faction, win ? 4f : 1f, 0f, win ? 5 : 0);
                extra = win
                    ? "\n\nO seu apoio entrou no relatório (com o nome escrito certo, desta vez): +" + favor + " de favor."
                    : "\n\nPerdemos, mas a potência anota quem apareceu: +" + favor + " de favor. Quem não apareceu também foi anotado.";
            }
            Find.LetterStack.ReceiveLetter((win ? "Vitória: " : "Derrota: ") + army.Label, text + extra,
                win == (role != "inimiga") ? LetterDefOf.PositiveEvent : LetterDefOf.NegativeEvent, army);
        }

        public static string LastBattle;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref garrisons, "garrisons", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref treasury, "treasury", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref forcedWars, "forcedWars", LookMode.Value);
            Scribe_Collections.Look(ref log, "log", LookMode.Value);
            Scribe_Values.Look(ref nextArmyId, "nextArmyId", 1);
            Scribe_Values.Look(ref lastCallTick, "lastCallTick", -9999999);
            if (garrisons == null) garrisons = new Dictionary<int, int>();
            if (treasury == null) treasury = new Dictionary<string, float>();
            if (forcedWars == null) forcedWars = new List<string>();
            if (log == null) log = new List<string>();
        }

        /// <summary>Resumo para a aba Reputação: guerras e exércitos da potência patrona.</summary>
        public string Describe(Faction power)
        {
            if (power == null) return null;
            var enemies = EQForces.EnemiesOf(power).Select(e => e.Name).ToList();
            if (enemies.Count == 0) return null;
            var armies = ArmiesOf(power).ToList();
            return "em guerra com " + string.Join(", ", enemies) + (armies.Count > 0 ? "; " + armies.Count + " exército(s) em campo (" + armies.Sum(a => a.strength) + " soldados)" : "");
        }
    }
}

namespace EcosDaQueda
{
    /// <summary>Etapa 11c: oferta de comando de um exército da potência (aceitar, recusar = falta; prazo de 2 dias = recusa).</summary>
    public class ChoiceLetter_EQCommand : ChoiceLetter
    {
        public WorldObject_EQArmy army;
        public int deadline = -1;
        public bool resolved;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || resolved)
                {
                    yield return Option_Close;
                    yield break;
                }
                var comp = WorldComponent_EQForces.Current;
                var accept = new DiaOption("Assumir o comando") { resolveTree = true };
                if (army == null || army.Destroyed) accept.Disable("o exército já não existe");
                else if (comp != null && comp.Armies.Any(a => a.playerCommand)) accept.Disable("já há um exército sob o seu comando");
                accept.action = () => { resolved = true; LastResult = comp?.AcceptCommand(army); Find.LetterStack.RemoveLetter(this); };
                yield return accept;
                var refuse = new DiaOption("Recusar o comando (uma falta)") { resolveTree = true };
                refuse.action = () => { Refuse(); Find.LetterStack.RemoveLetter(this); };
                yield return refuse;
                yield return Option_Postpone;
            }
        }

        public static string LastResult;

        public void Refuse()
        {
            if (resolved) return;
            resolved = true;
            WorldComponent_EQForces.Current?.RefuseCommand(relatedFaction);
            LastResult = "recusado";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref army, "army");
            Scribe_Values.Look(ref deadline, "deadline", -1);
            Scribe_Values.Look(ref resolved, "resolved");
        }
    }
}
