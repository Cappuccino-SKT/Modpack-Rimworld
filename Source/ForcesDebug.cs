using System;
using System.Globalization;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>Etapa 10d: "forces [war a b | hegwar a b | peace a b | raise power [inimigo] [n] | step [n] | arrive | daily [n]
    /// | call power | support n moral | treasury power valor | garrison facção n | log]". Sem argumento: estado.</summary>
    public static class EQForcesDebug
    {
        private static WorldObject_EQFront Front() => Find.WorldObjects.AllWorldObjects.OfType<WorldObject_EQFront>().OrderByDescending(f => f.openedTick).FirstOrDefault();

        private static WorldObject_EQArmy Commanded(WorldComponent_EQForces comp) => comp.Armies.FirstOrDefault(x => x.playerCommand);

        public static string Run(string[] a)
        {
            var comp = WorldComponent_EQForces.Current;
            if (comp == null) return "sem componente";
            string Arg(int i) => i < a.Length ? a[i] : null;
            Faction Fac(int i)
            {
                Faction f = EQDebug.FindFaction(Arg(i));
                if (f == null) throw new Exception("facção não encontrada: " + Arg(i));
                return EQUtil.Canonical(f);
            }
            string extra = "";
            switch (Arg(1))
            {
                case "war":
                {
                    Faction x = Fac(2), y = Fac(3);
                    string key = x.def.defName + "|" + y.def.defName;
                    if (!comp.forcedWars.Contains(key)) comp.forcedWars.Add(key);
                    extra = "guerra forçada " + x.Name + " × " + y.Name;
                    break;
                }
                case "hegwar":
                {
                    Faction x = Fac(2), y = Fac(3);
                    if (EQForces.DeclareWarHandler == null) { extra = "sem Hegemony"; break; }
                    string why = EQForces.DeclareWarHandler(x, y, "eqtest-war-" + x.loadID + "-" + y.loadID + "-" + Find.TickManager.TicksGame);
                    extra = why == null ? "guerra declarada no Hegemony: " + x.Name + " × " + y.Name : "Hegemony recusou: " + why;
                    break;
                }
                case "peace":
                    comp.forcedWars.RemoveAll(k => k == Fac(2).def.defName + "|" + Fac(3).def.defName || k == Fac(3).def.defName + "|" + Fac(2).def.defName);
                    extra = "guerra forçada removida";
                    break;
                case "raise":
                {
                    Faction p = Fac(2);
                    Faction e = Arg(3) != null && !int.TryParse(Arg(3), out _) ? Fac(3) : null;
                    int n = int.TryParse(Arg(a.Length - 1), out int v) && a.Length > 3 ? v : 0;
                    var army = comp.TryMobilize(p, e, out string why, n);
                    extra = army != null ? "levantado " + army.Label + " força=" + army.strength + " de " + army.home?.LabelCap + " contra " + army.target?.LabelCap + " (" + army.TilesLeft + " tiles)" : "recusado: " + why;
                    break;
                }
                case "step":
                {
                    int n = int.Parse(Arg(2) ?? "1");
                    var sb = new StringBuilder();
                    foreach (var army in comp.Armies.ToList())
                    {
                        string last = null;
                        for (int i = 0; i < n && !army.Destroyed; i++) last = army.Step();
                        sb.Append(army.Label + ": " + last + "; ");
                    }
                    extra = sb.Length == 0 ? "sem exércitos" : sb.ToString();
                    break;
                }
                case "arrive":
                {
                    var sb = new StringBuilder();
                    foreach (var army in comp.Armies.ToList())
                    {
                        if (!army.returning && army.target != null) army.Tile = army.target.Tile;
                        sb.Append(army.Label + ": " + army.Step() + "; ");
                    }
                    extra = sb.Length == 0 ? "sem exércitos" : sb.ToString();
                    break;
                }
                case "daily":
                {
                    int n = int.Parse(Arg(2) ?? "1");
                    string last = null;
                    for (int i = 0; i < n; i++) last = comp.Daily();
                    extra = "dias=" + n + " " + last;
                    break;
                }
                case "call":
                    extra = comp.CallToArms(Fac(2), ignoreCooldown: true);
                    break;
                case "support":
                    extra = comp.Support(WorldComponent_Allegiance.Current?.memberOf, float.Parse(Arg(2) ?? "10", CultureInfo.InvariantCulture),
                        float.Parse(Arg(3) ?? "0", CultureInfo.InvariantCulture), false);
                    break;
                case "treasury":
                    comp.SetTreasury(Fac(2), float.Parse(Arg(3), CultureInfo.InvariantCulture));
                    extra = "tesouro " + Fac(2).Name + " = " + comp.Treasury(Fac(2));
                    break;
                case "garrison":
                {
                    Faction f = Fac(2);
                    int n = int.Parse(Arg(3));
                    foreach (Settlement s in EQForces.SettlementsOf(f)) comp.SetGarrison(s, n);
                    extra = "guarnições de " + f.Name + " = " + n;
                    break;
                }
                case "log":
                    return string.Join("\n", comp.log);
                // Etapa 11c: comando do jogador
                case "offer":
                {
                    var army = comp.ArmiesOf(WorldComponent_Allegiance.Current?.memberOf).Where(x => !x.returning).OrderByDescending(x => x.raisedTick).FirstOrDefault();
                    extra = army == null ? "sem exército da patrona" : "pode_comandar=" + comp.CanCommand(army.Faction) + " " + comp.OfferCommand(army);
                    break;
                }
                case "posture":
                {
                    var army = Commanded(comp);
                    if (army == null) { extra = "sem exército sob comando"; break; }
                    army.posture = (EQPosture)Enum.Parse(typeof(EQPosture), Arg(2), true);
                    extra = "postura " + army.posture;
                    break;
                }
                case "pay":
                    extra = Commanded(comp)?.PaySoldo() ?? "sem exército sob comando";
                    break;
                case "target":
                {
                    var army = Commanded(comp);
                    if (army == null) { extra = "sem exército sob comando"; break; }
                    var opts = army.TargetOptions();
                    if (Arg(2) == null) { extra = "alvos: " + string.Join("; ", opts.Select((s, i) => i + "=" + s.LabelCap + " (" + s.Faction?.Name + ")")); break; }
                    extra = army.SetTarget(opts.ElementAtOrDefault(int.Parse(Arg(2))));
                    break;
                }
                case "retreat":
                    extra = Commanded(comp)?.Retreat() ?? "sem exército sob comando";
                    break;
                // Etapa 10e: frente de trincheira
                case "frontchance":
                    EQFronts.PatronFrontChance = float.Parse(Arg(2) ?? "0", CultureInfo.InvariantCulture);
                    extra = "chance de frente da patrona = " + EQFronts.PatronFrontChance;
                    break;
                // Etapa 10e, 2º bloco: frente de invasão do Hegemony (simula o evento InvasionArrived/Resolved)
                case "hegfrontchance":
                    EQFronts.HegemonyFrontChance = float.Parse(Arg(2) ?? "1", CultureInfo.InvariantCulture);
                    extra = "chance de frente do Hegemony = " + EQFronts.HegemonyFrontChance;
                    break;
                case "invasion":
                {
                    // forces invasion atacante defensor [n] | forces invasion resolve attacker|defender|none
                    if (Arg(2) == "resolve")
                    {
                        var hf = Front();
                        if (hf == null || !hf.IsHegemonyFront) { extra = "sem frente do Hegemony"; break; }
                        Faction w = Arg(3) == "attacker" ? hf.attacker : Arg(3) == "defender" ? hf.defender : null;
                        lock (EQFronts.PendingInvasions)
                            EQFronts.PendingInvasions.Add(new EQFronts.InvasionNote { arrived = false, attacker = hf.attacker, defender = hf.defender, winner = w, marker = hf.hegemonyMarker });
                        EQFronts.ProcessPending();
                        extra = EQFronts.LastInvasion;
                        break;
                    }
                    Faction x = Fac(2), y = Fac(3);
                    Settlement target = Find.WorldObjects.Settlements.Where(s => EQUtil.Canonical(s.Faction) == y).RandomElementWithFallback();
                    if (target == null) { extra = "defensor sem assentamento"; break; }
                    int size = int.TryParse(Arg(4), out int sz) ? sz : 25;
                    lock (EQFronts.PendingInvasions)
                        EQFronts.PendingInvasions.Add(new EQFronts.InvasionNote { arrived = true, attacker = x, defender = y, location = target, marker = 900000 + Find.TickManager.TicksGame % 100000, forceSize = size });
                    EQFronts.ProcessPending();
                    extra = EQFronts.LastInvasion;
                    break;
                }
                case "played":
                {
                    var army = Commanded(comp);
                    if (army == null) { extra = "sem exército sob comando"; break; }
                    army.playedBattle = Arg(2) != "off";
                    extra = "batalha jogada = " + army.playedBattle;
                    break;
                }
                case "front":
                {
                    var f = Front();
                    if (f == null) { extra = "sem frente"; break; }
                    switch (Arg(2))
                    {
                        case "side":
                            // forces front side attacker|defender|none (como a escolha do menu da caravana)
                            extra = f.ChooseSide(Arg(3) == "attacker" ? f.attacker : Arg(3) == "defender" ? f.defender : null)
                                    + " | gw_jogador=" + Faction.OfPlayer.GoodwillWith(f.attacker) + "/" + Faction.OfPlayer.GoodwillWith(f.defender)
                                    + " menu_atk=" + f.SideReport(f.attacker).Accepted + " menu_def=" + f.SideReport(f.defender).Accepted;
                            break;
                        case "visit":
                            f.GenerateFrontMap();
                            f.playerVisited = true;
                            extra = "mapa gerado: " + WorldObject_EQFront.LastMapReport + " soldados_último_lado=" + WorldObject_EQFront.LastSpawned;
                            break;
                        case "expire":
                            f.deadlineTick = Find.TickManager.TicksGame;
                            extra = "prazo vencido";
                            break;
                        case "kill":
                        {
                            // mata todos os combatentes de um lado (attacker|defender) para fechar a batalha
                            Faction side = Arg(3) == "defender" ? f.defender : f.attacker;
                            int n = 0;
                            foreach (Pawn p in f.Fighters(side).ToList()) { p.Kill(null); n++; }
                            extra = "mortos " + n + " de " + side?.Name;
                            break;
                        }
                        case "check":
                            extra = "check: " + f.Check(Find.TickManager.TicksGame);
                            break;
                    }
                    extra = (extra == "" ? "" : extra + " | ") + "frente=" + f.Label + " tile=" + f.Tile + " mapa=" + f.HasMap + " resolvida=" + f.resolved
                        + " forças=" + f.attackerStrength + "x" + f.defenderStrength
                        + (f.HasMap ? " de_pé=" + f.Fighters(f.attacker).Count() + "x" + f.Fighters(f.defender).Count() + " onda=" + f.waveSent + " avanço=" + f.assaultStarted : "")
                        + " hostis=" + f.attacker?.HostileTo(f.defender) + " visitou=" + f.playerVisited
                        + (f.IsHegemonyFront ? " hegemony=" + f.hegemonyMarker + " alvo=" + f.targetSettlement?.LabelCap + (f.hegemonyResult != null ? " invasão=" + f.hegemonyResult : "") : "")
                        + (f.sideChosen ? " lado=" + (f.supported?.Name ?? "fora") : "")
                        + (WorldObject_EQFront.LastReward != null ? " recompensa=" + WorldObject_EQFront.LastReward : "")
                        + (f.result != null ? " resultado=" + f.result : "");
                    break;
                }
                // Etapa 10d (resto): tensão, conselho de guerra, frentes de fronteira
                case "tension":
                {
                    // forces tension [a b [valor]] : mostra todas; ou define (cruza limiares como no dia)
                    if (Arg(2) != null)
                    {
                        Faction x = Fac(2), y = Fac(3);
                        float before = EQWarDiplomacy.Get(x, y);
                        if (Arg(4) != null)
                        {
                            float v = float.Parse(Arg(4), CultureInfo.InvariantCulture);
                            comp.tension[EQWarDiplomacy.Key(x, y)] = v;
                            extra = "tensão " + before.ToString("0") + " → " + v + " " + EQWarDiplomacy.Thresholds(comp, x, y, before, v);
                        }
                    }
                    extra += " tensões=[" + string.Join(", ", EQWarDiplomacy.PowerPairs().Select(p => p.Key.def.defName.Replace("EQ_Faction_", "") + "×" + p.Value.def.defName.Replace("EQ_Faction_", "")
                        + "=" + EQWarDiplomacy.Get(p.Key, p.Value).ToString("0") + "(+" + EQWarDiplomacy.Growth(p.Key, p.Value).ToString("0.0", CultureInfo.InvariantCulture) + "/d)")) + "]";
                    break;
                }
                case "tensionday":
                {
                    int n = int.TryParse(Arg(2), out int v) ? v : 1;
                    for (int i = 0; i < n; i++) EQWarDiplomacy.Daily(comp);
                    extra = n + " dia(s): " + EQWarDiplomacy.LastDaily;
                    break;
                }
                case "council":
                {
                    var l = Find.LetterStack.LettersListForReading.OfType<ChoiceLetter_EQTension>().FirstOrDefault(x => !x.resolved);
                    if (l == null) { extra = "sem conselho de guerra (último: " + EQWarDiplomacy.LastConsult + ")"; break; }
                    l.Choose(Arg(2) != "calm");
                    Find.LetterStack.RemoveLetter(l);
                    extra = "conselho: " + ChoiceLetter_EQTension.LastResult;
                    break;
                }
                case "border":
                {
                    if (Arg(2) == null) { extra = EQWarDiplomacy.BorderFronts(comp, true) ?? "nenhuma guerra com frente possível"; break; }
                    Faction x = Fac(2), y = Fac(3);
                    var tile = EQWarDiplomacy.FindBorderTile(x, y, out string how);
                    extra = "tile=" + tile + " (" + how + ") | " + (EQWarDiplomacy.BorderFronts(comp, true, x, y) ?? "sem frente (não estão em guerra?)");
                    break;
                }
                case "unpaid":
                {
                    var army = Commanded(comp);
                    if (army != null) army.paidUntilTick = 0;
                    extra = "soldo zerado";
                    break;
                }
            }
            var wars = EQForces.Wars();
            var cmd = Commanded(comp);
            var res = new StringBuilder();
            if (extra != "") res.Append(extra + " | ");
            res.Append("hegemony=" + (EQForces.WarsProvider != null) + " guerras=" + wars.Count + " [" + string.Join("; ", wars.Take(8).Select(w => w.Key.Name + "×" + w.Value.Name)) + "]");
            res.Append(" exércitos=" + comp.Armies.Count() + " [" + string.Join("; ", comp.Armies.Select(x => x.Label + " f=" + x.strength + " m=" + x.morale.ToString("0.00", CultureInfo.InvariantCulture)
                + " apoio=" + x.playerSupport + (x.returning ? " voltando" : " alvo=" + x.target?.LabelCap) + " tiles=" + x.TilesLeft)) + "]");
            res.Append(" tesouros=[" + string.Join(", ", EQAllegiance.Powers.Select(p => p.def.defName.Replace("EQ_Faction_", "") + "=" + comp.Treasury(p).ToString("0"))) + "]");
            if (cmd != null) res.Append(" comando=[" + cmd.Label + " postura=" + cmd.posture + " soldo_até_dia=" + (cmd.paidUntilTick / 60000f).ToString("0.0", CultureInfo.InvariantCulture) + " custo=" + cmd.SoldoCost + "]");
            if (ChoiceLetter_EQCommand.LastResult != null) res.Append(" carta_comando=" + ChoiceLetter_EQCommand.LastResult);
            if (WorldComponent_EQForces.LastBattle != null) res.Append(" última_batalha=" + WorldComponent_EQForces.LastBattle);
            if (EQPowerEvents.LastArmySupport != null) res.Append(" último_apoio=" + EQPowerEvents.LastArmySupport);
            return res.ToString();
        }
    }
}
