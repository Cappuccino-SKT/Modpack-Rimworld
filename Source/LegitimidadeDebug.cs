using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// N3 (NACAO.md › 3.6): "legit [show|set N|add N|day N|unrest|arrive|win|lose|bribe|merc flee|merc come|mercnation flee|come|
    /// kill|heir i|leader i|crown|check|deadline|react helice|forja yes|no|letters|raid|execute]".
    /// </summary>
    public static class EQLegitimidadeDebug
    {
        private static WorldComponent_EQLegitimacy C => WorldComponent_EQLegitimacy.Current;

        public static string Run(string[] a)
        {
            string sub = a.Length > 1 ? a[1] : "show";
            var c = C;
            if (c == null) return "sem WorldComponent_EQLegitimacy";
            string arg = a.Length > 2 ? a[2] : null;
            float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            switch (sub)
            {
                case "show": return Show();
                case "set": c.value = F(arg); EQCouncil.Dirty(); return Show();
                case "add": c.Add(F(arg), a.Length > 3 ? string.Join(" ", a.Skip(3)) : "teste"); return EQLegitimacy.LastReport + "\n" + Show();
                case "day":
                {
                    float days = F(arg);
                    float v = c.DailyDrift(out string detail);
                    c.Add(v * days, "dia a dia (teste, " + days + "d)");
                    return detail + " → " + EQLegitimacy.LastReport + "\n" + Show();
                }
                case "unrest": return c.StartRevolt("unrest", LegitimacyDef.Active.unrestPointsFactor, false) + "\n  carta: " + LetterOf<ChoiceLetter_EQUnrest>() + "\n" + Show();
                case "arrive": c.ArriveNow(); return EQLegitimacy.LastReport + "\n" + Show();
                case "win":
                {
                    var r = c.revolt;
                    if (r == null || !r.started) return "sem revolta em curso";
                    int n = 0;
                    foreach (Pawn p in r.revolters.Where(p => !p.Dead && p.Spawned).ToList()) { p.Kill(null); n++; }
                    c.CheckRevolt();
                    return "revoltosos mortos=" + n + "; " + EQLegitimacy.LastReport + "\n" + Show();
                }
                case "lose":
                {
                    var r = c.revolt;
                    if (r == null || !r.started) return "sem revolta em curso";
                    Room room = r.throne?.GetRoom();
                    Pawn p = r.revolters.FirstOrDefault(x => !WorldComponent_EQNation.OutOfFight(x, r.map));
                    if (room == null || p == null) return "sem sala do trono ou sem revoltoso";
                    IntVec3 cell = room.Cells.FirstOrDefault(x => x.Standable(r.map) && x.GetFirstPawn(r.map) == null);
                    p.Position = cell; p.Notify_Teleported();
                    r.throneTicks = (int)(NationDef.Active.throneHoldHours * GenDate.TicksPerHour) - 250;
                    c.CheckRevolt();
                    return "revoltoso na sala do trono; " + EQLegitimacy.LastReport + "\n" + Show();
                }
                case "bribe":
                {
                    if (c.revolt == null) return "sem revolta";
                    GiveSilver(c.revolt.map, EQNation.BribeCost(c.revolt.points));
                    return (c.Bribe() is string s ? s : "-") + "\n" + Show();
                }
                case "merc":
                case "mercnation":
                {
                    EQMercs.TestFlee = arg == "flee" ? true : arg == "come" ? (bool?)false : null;
                    if (sub == "merc")
                    {
                        if (c.revolt == null) return "sem revolta";
                        int cost = EQMercs.Cost(c.revolt.points);
                        GiveSilver(c.revolt.map, cost);
                        string err = c.HireMercs();
                        return (err ?? EQLegitimacy.LastReport) + " (custo " + cost + "); de novo: " + (c.HireMercs() ?? "aceitou (ERRO)") + "\n" + Show();
                    }
                    var n = WorldComponent_EQNation.Current;
                    int cost2 = EQMercs.Cost(n.points);
                    GiveSilver(n.map ?? Find.AnyPlayerHomeMap, cost2);
                    string err2 = n.HireMercs();
                    return (err2 ?? EQLegitimacy.LastReport) + " (custo " + cost2 + ", desconto " + c.mercDiscount + ")";
                }
                // Decretos: "legit decree list | <defName> | revoke <defName> | cooldown | stats"
                case "decree":
                {
                    if (arg == null || arg == "list")
                        return string.Join("\n  ", EQDecrees.All.Select(d => d.defName + (EQDecrees.IsActive(d) ? " [em vigor]" : "") + ": " + ((EQDecrees.IsActive(d) ? EQDecrees.CannotRevoke(d) : EQDecrees.CannotEnact(d)) ?? "pode"))) + "\n" + Show();
                    if (arg == "cooldown") { c.decreeTick = -999999; return "espera zerada\n" + Show(); }
                    if (arg == "stats")
                    {
                        Pawn p = EQLeader.AllFreeColonists().FirstOrDefault();
                        ThoughtDef th = DefDatabase<ThoughtDef>.GetNamedSilentFail("EQ_Decreto_Alistamento");
                        bool mood = p?.needs?.mood?.thoughts != null && th != null && th.Worker.CurrentState(p).Active;
                        return "exército×" + EQCouncil.ArmyFactor.ToString("0.00") + " revolta×" + EQCouncil.RevoltFactor.ToString("0.00") + " tributo×" + EQCouncil.TributeFactor.ToString("0.00")
                               + " boa_vontade×" + EQCouncil.GoodwillFactor.ToString("0.00") + " comércio+" + EQCouncil.Channel("tradePrice").ToString("0.00") + " pesquisa" + EQCouncil.Channel("research").ToString("+0.00;-0.00")
                               + " defesa_fronteira" + EQCouncil.Channel("borderDefense").ToString("+0.00;-0.00") + " | pensamento alistamento em " + p?.LabelShort + "=" + mood
                               + " | legitimidade/dia dos decretos=" + EQDecrees.LegitimacyDaily.ToString("+0.##;-0.##;0") + " peso da Infâmia×" + EQDecrees.InfamyFactor.ToString("0.0");
                    }
                    bool rev = arg == "revoke";
                    DecreeDef d = DefDatabase<DecreeDef>.GetNamedSilentFail(rev ? (a.Length > 3 ? a[3] : "") : arg);
                    if (d == null) return "decreto desconhecido";
                    string err = rev ? EQDecrees.Revoke(d) : EQDecrees.Enact(d);
                    return (err != null ? "RECUSADO: " + err : EQDecrees.LastReport) + "\n" + Show();
                }
                case "slots": EQDecrees.CheckSlots(); return (EQDecrees.LastReport ?? "-") + "\n" + Show();
                case "sindfriend":
                {
                    Faction f = EQMercs.Faction;
                    if (f == null) return "sem Sindicato";
                    int gw = f.GoodwillWith(Faction.OfPlayer);
                    if (gw < 10) f.ChangeGoodwill_Debug(Faction.OfPlayer, 10 - gw);
                    return "Sindicato hostil ao jogador=" + f.HostileTo(Faction.OfPlayer) + " boa vontade=" + f.GoodwillWith(Faction.OfPlayer);
                }
                case "mercs":
                {
                    var list = c.revolt?.mercs ?? WorldComponent_EQNation.Current?.mercs ?? new List<Pawn>();
                    Faction rf = EQNation.RevoltFaction;
                    return "mercenários: " + EQMercs.LastArrival + " | vivos=" + list.Count(p => !p.Dead) + " lord=" + list.FirstOrDefault()?.GetLord()?.LordJob?.GetType().Name
                           + " | Sindicato hostil ao jogador=" + EQMercs.Faction?.HostileTo(Faction.OfPlayer) + " hostil à revolta=" + (rf != null && EQMercs.Faction?.HostileTo(rf) == true);
                }
                case "kill":
                {
                    Pawn l = c.crowned ?? EQLeader.IdeoLeader;
                    if (l == null) return "sem líder";
                    l.Kill(null);
                    c.SuccessionCheck();
                    return "líder " + l.LabelShort + " morto; " + EQLegitimacy.LastReport + "\n" + Show();
                }
                case "heir":
                {
                    var cands = EQLeader.Candidates().Where(p => p != EQLeader.IdeoLeader).ToList();
                    int i = arg != null ? int.Parse(arg) : 0;
                    if (i >= cands.Count) return "candidato inválido (há " + cands.Count + ")";
                    WorldComponent_Allegiance.Current.successor = cands[i];
                    c.SuccessionCheck();
                    return "sucessor designado: " + cands[i].LabelShort + "\n" + Show();
                }
                case "leader":
                {
                    var cands = EQLeader.Candidates().Where(p => p != EQLeader.IdeoLeader).ToList();
                    int i = arg != null ? int.Parse(arg) : 0;
                    if (i >= cands.Count) return "candidato inválido (há " + cands.Count + ")";
                    bool ok = EQLeader.Assign(cands[i]);
                    c.SuccessionCheck();
                    return "novo líder " + cands[i].LabelShort + " ok=" + ok + "; " + EQLegitimacy.LastReport + "\n" + Show();
                }
                case "crown":
                {
                    Building_Throne th = EQLegitimacy.FindThrone();
                    if (th == null) return "sem trono";
                    Pawn p = EQLeader.IdeoLeader;
                    if (p != null && p.Spawned && p.Map == th.Map && p.GetRoom() != th.GetRoom())
                    {
                        IntVec3 cell = th.GetRoom().Cells.FirstOrDefault(x => x.Standable(th.Map) && x.GetFirstPawn(th.Map) == null);
                        if (cell.IsValid) { p.Position = cell; p.Notify_Teleported(); }
                    }
                    string why = EQLegitimacy.CanCrown(th);
                    string err = c.Crown(th);
                    return "botão: " + (why ?? "pode") + " | " + (err ?? EQLegitimacy.LastReport) + "\n" + Show();
                }
                case "check": c.SuccessionCheck(); return EQLegitimacy.LastReport + "\n" + Show();
                case "deadline": c.crownDeadline = Find.TickManager.TicksGame; c.SuccessionCheck(); return EQLegitimacy.LastReport + "\n  carta: " + LetterOf<ChoiceLetter_EQUnrest>() + "\n" + Show();
                case "react":
                {
                    if (arg == "forja" && a.Length > 3 && a[3] == "yes")
                    {
                        Map m = Find.AnyPlayerHomeMap;
                        Thing st = ThingMaker.MakeThing(ThingDefOf.Steel); st.stackCount = LegitimacyDef.Active.forjaSteel;
                        GenPlace.TryPlaceThing(st, m.Center, m, ThingPlaceMode.Near);
                    }
                    return EQLegitimacy.Answer(arg, a.Length > 3 && a[3] == "yes") + " | conselho: cerimônia×" + EQCouncil.CeremonyFactor.ToString("0.00") + "\n" + Show();
                }
                case "letters":
                    return "cartas de reação: " + string.Join(" || ", Find.LetterStack.LettersListForReading.OfType<ChoiceLetter_EQReaction>().Select(Options))
                           + "\n  outras: " + string.Join(" | ", Find.LetterStack.LettersListForReading.Where(l => !(l is ChoiceLetter_EQReaction)).Select(l => l.Label.ToString()).Reverse().Take(8));
                case "raid":
                {
                    Map m = Find.AnyPlayerHomeMap;
                    Faction f = Find.FactionManager.AllFactions.FirstOrDefault(x => !x.IsPlayer && x.HostileTo(Faction.OfPlayer) && x.def.humanlikeFaction && !x.Hidden && x != EQNation.RevoltFaction);
                    if (f == null) return "sem facção hostil";
                    var before = new HashSet<Pawn>(m.mapPawns.AllPawnsSpawned);
                    var parms = new IncidentParms { target = m, faction = f, points = 400f, forced = true, raidStrategy = RaidStrategyDefOf.ImmediateAttack, raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn };
                    bool ok = IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                    var raiders = m.mapPawns.AllPawnsSpawned.Where(p => !before.Contains(p) && p.Faction == f).ToList();
                    foreach (Pawn p in raiders) p.Kill(null);
                    return "raid de " + f.Name + " ok=" + ok + " mortos=" + raiders.Count + " (o lord some em seguida; ver 'legit' depois de esperar)";
                }
                case "execute":
                {
                    var council = WorldComponent_EQCouncil.Current;
                    Pawn victim = council?.seats.Values.FirstOrDefault(p => p != null && !p.Dead);
                    Pawn exe = EQLeader.IdeoLeader;
                    if (victim == null || exe == null) return "sem conselheiro ou sem líder";
                    ExecutionUtility.DoExecutionByCut(exe, victim);
                    return "executado " + victim.LabelShort + " (morto=" + victim.Dead + "); " + EQLegitimacy.LastReport + "\n" + Show();
                }
            }
            return "subcomando desconhecido: " + sub;
        }

        public static string Show()
        {
            var c = C;
            return c.Report() + "\n  canais do conselho: tributo×" + EQCouncil.TributeFactor.ToString("0.00") + " fronteira×" + EQCouncil.FrontierSpeedFactor.ToString("0.00")
                   + " cerimônia×" + EQCouncil.CeremonyFactor.ToString("0.00") + " | sucessão: " + (EQGov.Succession() ?? "-");
        }

        private static void GiveSilver(Map m, int n)
        {
            Thing s = ThingMaker.MakeThing(ThingDefOf.Silver);
            s.stackCount = n;
            GenPlace.TryPlaceThing(s, m.Center, m, ThingPlaceMode.Near);
        }

        private static string Options(ChoiceLetter l) =>
            l.Label + " [" + string.Join(" / ", l.Choices.Select(o => HarmonyLib.Traverse.Create(o).Field("text").GetValue<string>() + (o.disabled ? " (" + o.disabledReason + ")" : ""))) + "]";

        private static string LetterOf<T>() where T : ChoiceLetter
        {
            var l = Find.LetterStack.LettersListForReading.OfType<T>().LastOrDefault();
            return l == null ? "nenhuma" : Options(l);
        }
    }
}
