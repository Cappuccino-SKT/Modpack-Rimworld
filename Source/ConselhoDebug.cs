using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// N2 (NACAO.md › 3.4): "council [show|recruit [n]|sit &lt;cadeira&gt; [i]|empty &lt;cadeira&gt;|skill &lt;cadeira&gt; &lt;nível&gt;|cooldown|goodwill|down &lt;cadeira&gt;|thoughts]".
    /// N2b: "council meme add|remove &lt;MemeDef&gt; | meme clear | role &lt;PreceptDef&gt; [i] | mood &lt;leader|cadeira&gt; &lt;0-1&gt; | vote | drop &lt;cadeira|none&gt; | age &lt;i&gt; &lt;anos&gt; | stats | yield".
    /// </summary>
    public static class EQConselhoDebug
    {
        private static WorldComponent_EQCouncil C => WorldComponent_EQCouncil.Current;

        private static List<Pawn> Eligible() => EQLeader.AllFreeColonists().Distinct().Where(p => EQCouncil.CannotSit(p) == null).ToList();

        public static string Run(string[] a)
        {
            string sub = a.Length > 1 ? a[1] : "show";
            var c = C;
            CouncilDef d = CouncilDef.Active;
            if (c == null || d == null) return "sem conselho";
            string key = a.Length > 2 ? a[2] : null;
            EQCouncil.Dirty();
            switch (sub)
            {
                case "show": return Show();
                // N2b: governo pela ideologia
                case "meme":
                {
                    // council meme add|remove <defName> | council meme clear (tira todos os que têm governo)
                    Ideo ideo = EQLeader.PlayerIdeo;
                    if (ideo == null) return "sem ideologia";
                    if (key == "clear")
                    {
                        int n = ideo.memes.RemoveAll(m => DefDatabase<GovMemeDef>.AllDefsListForReading.Any(g => g.MemeDef == m));
                        EQCouncil.Dirty();
                        return n + " memes de governo tirados; cleanup=" + c.Cleanup(false) + "\n" + Show();
                    }
                    MemeDef meme = a.Length > 3 ? DefDatabase<MemeDef>.GetNamedSilentFail(a[3]) : null;
                    if (meme == null) return "meme não encontrado: " + (a.Length > 3 ? a[3] : "?");
                    if (key == "add" && !ideo.memes.Contains(meme)) ideo.memes.Add(meme);
                    else if (key == "remove") ideo.memes.Remove(meme);
                    EQCouncil.Dirty();
                    return "memes: " + string.Join(",", ideo.memes.Select(m => m.defName)) + "; cleanup=" + c.Cleanup(false) + "\n" + Show();
                }
                case "role":
                {
                    // council role <PreceptDef> [i]: cria a função na ideologia (se faltar) e dá ao candidato i (fora do conselho e sem função)
                    Ideo ideo = EQLeader.PlayerIdeo;
                    PreceptDef pd = DefDatabase<PreceptDef>.GetNamedSilentFail(key ?? "");
                    if (ideo == null || pd == null) return "função não encontrada: " + key;
                    Precept_Role r = EQCouncil.RoleOf(pd.defName);
                    if (r == null) { r = (Precept_Role)PreceptMaker.MakePrecept(pd); ideo.AddPrecept(r, true); }
                    List<Pawn> el = Eligible().Where(p => !c.seats.ContainsValue(p) && !ideo.PreceptsListForReading.OfType<Precept_Role>().Any(x => x.IsAssigned(p))).ToList();
                    int i = a.Length > 3 ? int.Parse(a[3]) : 0;
                    if (i < 0 || i >= el.Count) return "candidato inválido (há " + el.Count + ")";
                    foreach (Pawn old in r.ChosenPawns().ToList()) r.Unassign(old, false);
                    r.Assign(el[i], false);
                    EQCouncil.Dirty();
                    return "função " + r.LabelCap + " → " + el[i].LabelShort + "\n" + Show();
                }
                case "mood":
                {
                    // council mood <leader|cadeira> <0-1>
                    Pawn p = key == "leader" ? EQLeader.IdeoLeader : EQCouncil.HolderOf(key);
                    if (p?.needs?.mood == null) return "sem pessoa/humor";
                    p.needs.mood.CurLevelPercentage = float.Parse(a[3], System.Globalization.CultureInfo.InvariantCulture);
                    EQCouncil.Dirty();
                    return p.LabelShort + " humor=" + p.needs.mood.CurLevelPercentage.ToString("0.00") + "\n" + Show();
                }
                case "vote": return "voto: " + (EQGov.VoteBlock("jurar lealdade") ?? "liberado")
                                    + " | cerimônia: " + (EQAllegiance.Powers.FirstOrDefault() is Faction pw ? (EQCeremony.BlockReason(pw) ?? "liberada") : "-");
                case "drop":
                    c.droppedSeat = key == "none" ? null : key;
                    return "fechada=" + (c.droppedSeat ?? "-") + " cleanup=" + c.Cleanup(false) + "\n" + Show();
                case "age":
                {
                    // council age <i> <anos>: idade biológica do candidato i
                    List<Pawn> el = Eligible();
                    Pawn p = el[int.Parse(key)];
                    p.ageTracker.AgeBiologicalTicks = int.Parse(a[3]) * (long)GenDate.TicksPerYear;
                    EQCouncil.Dirty();
                    return p.LabelShort + " idade=" + p.ageTracker.AgeBiologicalYears;
                }
                case "stats":
                {
                    var all = EQLeader.AllFreeColonists().Distinct().ToList();
                    Pawn Top(SkillDef sk) => all.OrderByDescending(x => x.skills?.GetSkill(sk)?.Level ?? 0).FirstOrDefault();
                    Pawn p = Top(SkillDefOf.Intellectual), soc = Top(SkillDefOf.Social), cr = Top(SkillDefOf.Crafting);
                    Map map = Find.AnyPlayerHomeMap;
                    Building b = map?.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x.def.IsWorkTable);
                    if (b == null && map != null && DefDatabase<ThingDef>.GetNamedSilentFail("CraftingSpot") is ThingDef bench)
                    {
                        Thing t = ThingMaker.MakeThing(bench);
                        t.SetFaction(Faction.OfPlayer);
                        b = GenSpawn.Spawn(t, CellFinder.RandomClosewalkCellNear(map.Center, map, 6), map) as Building;
                    }
                    int up = 0, tries = 400;
                    if (cr != null)
                        for (int i = 0; i < tries; i++)
                            if (QualityUtility.GenerateQualityCreatedByPawn(cr, SkillDefOf.Crafting, false) >= QualityCategory.Excellent) up++;
                    return "pesquisa=" + (p == null ? "-" : p.GetStatValue(StatDefOf.ResearchSpeed).ToString("0.000"))
                           + " comércio=" + (soc == null ? "-" : soc.GetStatValue(StatDefOf.TradePriceImprovement).ToString("0.000"))
                           + " bancada=" + (b == null ? "-" : b.def.defName + " " + b.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor).ToString("0.000"))
                           + " qualidade_canal=" + EQCouncil.Channel("quality").ToString("0.00") + " excelente+=" + up + "/" + tries;
                }
                // N2c: marcos (preço, alcance, demarcação), veto de invasão, efeitos da fundação
                case "marker":
                {
                    var pr = EQPlayerFrontier.MarkerPrice();
                    return "marco: " + pr.count + "x " + pr.def.defName + " alcance=" + EQPlayerFrontier.MaxFromBorder.ToString("0.#") + " demarcação="
                           + EQPlayerFrontier.SetupDays.ToString("0.#") + "d raio_inicial+" + EQCouncil.Channel("startRadius").ToString("0.#")
                           + " vida=" + EQGov.ActiveDefs().Select(g => g.markerLifeDays).Where(x => x > 0f).DefaultIfEmpty(0f).Min().ToString("0") + "d";
                }
                case "veto":
                {
                    int n = 0, tries = 0;
                    foreach (Faction f in Find.FactionManager.AllFactionsVisible.Where(x => !x.IsPlayer)) { tries++; if (EQGov.InvasionVeto(f) != null) n++; }
                    return "veto de invasão: canal=" + EQCouncil.Channel("invasionVeto").ToString("0.00") + " vetadas hoje " + n + "/" + tries;
                }
                case "founding":
                {
                    var before = Find.FactionManager.AllFactionsVisible.Where(x => !x.IsPlayer && !x.temporary).ToDictionary(x => x, x => x.GoodwillWith(Faction.OfPlayer));
                    string r = EQGov.OnFounding(WorldComponent_EQNation.Current);
                    return "fundação: " + r + " | boa vontade: " + string.Join(", ", before.Where(kv => kv.Key.GoodwillWith(Faction.OfPlayer) != kv.Value).Take(6)
                               .Select(kv => kv.Key.def.defName + " " + (kv.Key.GoodwillWith(Faction.OfPlayer) - kv.Value).ToString("+0;-0")));
                }
                case "yield":
                {
                    var res = EQPlayerFrontier.ComputeYield();
                    string feats = "";
                    try
                    {
                        Faction me = EQUtil.Canonical(Faction.OfPlayer);
                        var mine = (EQTerritory.ClaimsProvider?.Invoke() ?? new List<KeyValuePair<RimWorld.Planet.PlanetTile, Faction>>())
                            .Where(c => EQUtil.Canonical(c.Value) == me).Select(c => c.Key).ToList();
                        int road = 0, river = 0, coast = 0, forest = 0;
                        foreach (var t in mine)
                        {
                            var tile = Find.WorldGrid[t];
                            if (tile is RimWorld.Planet.SurfaceTile st) { if (!st.Roads.NullOrEmpty()) road++; if (!st.Rivers.NullOrEmpty()) river++; }
                            if (Find.World.CoastDirectionAt(t).IsValid) coast++;
                            if (EQCouncil.IsForest(tile?.PrimaryBiome)) forest++;
                        }
                        feats = " [tiles " + mine.Count + ": estrada " + road + ", rio " + river + ", costa " + coast + ", floresta " + forest + ", bioma " + string.Join("/", mine.Select(t => Find.WorldGrid[t]?.PrimaryBiome?.defName).Distinct()) + "]";
                    }
                    catch (System.Exception e) { feats = " [" + e.Message + "]"; }
                    return (res.Count == 0 ? "sem rendimento" : string.Join(" | ", res.Select(kv => kv.Key.Label + ": " + kv.Value))) + feats;
                }
                case "recruit":
                {
                    Map map = Find.AnyPlayerHomeMap;
                    int n = a.Length > 2 ? int.Parse(a[2]) : 3;
                    for (int i = 0; i < n; i++)
                    {
                        Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer, forceGenerateNewPawn: true,
                            developmentalStages: DevelopmentalStage.Adult, mustBeCapableOfViolence: true));
                        if (p.ideo != null && EQLeader.PlayerIdeo != null) p.ideo.SetIdeo(EQLeader.PlayerIdeo);
                        GenSpawn.Spawn(p, CellFinder.RandomClosewalkCellNear(map.Center, map, 8), map);
                    }
                    return n + " colonos novos; " + Show();
                }
                case "sit":
                {
                    List<Pawn> el = Eligible().Where(p => !c.seats.ContainsValue(p)).ToList();
                    int i = a.Length > 3 ? int.Parse(a[3]) : 0;
                    if (i < 0 || i >= el.Count) return "candidato inválido (há " + el.Count + ")";
                    string err = c.Assign(key, el[i]);
                    return (err != null ? "RECUSADO: " + err : "sentou " + el[i].LabelShort + " em " + key) + "\n" + Show();
                }
                case "leader":
                {
                    if (EQLeader.IdeoLeader == null && EQLeader.Candidates().Count > 0) EQLeader.Assign(EQLeader.Candidates().Last());
                    Pawn l = EQLeader.IdeoLeader;
                    if (l == null) return "sem líder";
                    return "líder na cadeira " + key + ": " + (c.Assign(key, l) ?? "ACEITO (ERRO)");
                }
                case "empty": return (c.Assign(key, null) ?? "vazia") + "\n" + Show();
                case "skill":
                {
                    // council skill <cadeira> <nível> [leader]: "leader" muda a perícia do líder nas perícias da cadeira
                    Pawn p = a.Length > 4 && a[4] == "leader" ? EQLeader.IdeoLeader : EQCouncil.HolderOf(key);
                    if (p == null) return "cadeira vazia";
                    int lv = int.Parse(a[3]);
                    foreach (SkillDef s in EQCouncil.SeatOf(key).def.skills) { var r = p.skills.GetSkill(s); if (r != null) r.Level = lv; }
                    return p.LabelShort + " perícia " + lv + "\n" + Show();
                }
                case "cooldown":
                    foreach (string k in c.lastChange.Keys.ToList()) c.lastChange[k] = -999999;
                    return "esperas zeradas";
                case "down":
                {
                    Pawn p = EQCouncil.HolderOf(key);
                    if (p == null) return "cadeira vazia";
                    HealthUtility.DamageUntilDowned(p, allowBleedingWounds: false);
                    return p.LabelShort + " caído: " + (EQCouncil.Inactive(p, EQCouncil.SeatOf(key).def) ?? "valendo") + "\n" + Show();
                }
                case "goodwill":
                {
                    Faction f = EQAllegiance.Powers.FirstOrDefault(x => !x.HostileTo(Faction.OfPlayer)) ?? EQAllegiance.Powers.FirstOrDefault();
                    if (f == null) return "sem potência";
                    int before = f.GoodwillWith(Faction.OfPlayer);
                    f.TryAffectGoodwillWith(Faction.OfPlayer, 10, canSendMessage: false, canSendHostilityLetter: false);
                    return "boa vontade com " + f.Name + ": +10 pedido → " + (f.GoodwillWith(Faction.OfPlayer) - before) + " (fator " + EQCouncil.GoodwillFactor.ToString("0.00") + ")";
                }
                case "thoughts":
                {
                    ThoughtDef m = DefDatabase<ThoughtDef>.GetNamed("EQ_ConselhoMembro"), o = DefDatabase<ThoughtDef>.GetNamed("EQ_ConselhoPreterido");
                    return string.Join(" | ", EQLeader.AllFreeColonists().Distinct().Select(p =>
                    {
                        var list = new List<Thought>();
                        p.needs?.mood?.thoughts?.GetAllMoodThoughts(list);
                        return p.LabelShort + ":" + (list.Any(t => t.def == m) ? " membro" : "") + (list.Any(t => t.def == o) ? " preterido" : "");
                    }));
                }
            }
            return "subcomando desconhecido: " + sub;
        }

        public static string Show()
        {
            Pawn l = EQLeader.IdeoLeader;
            return EQCouncil.Report() + "\n  líder=" + (l?.LabelShort ?? "-") + " fundada=" + EQNation.Founded
                   + " candidatos=" + string.Join(", ", Eligible().Select(p => p.LabelShort + "(soc " + p.skills.GetSkill(SkillDefOf.Social).Level + ", int " + p.skills.GetSkill(SkillDefOf.Intellectual).Level + ")"));
        }
    }
}
