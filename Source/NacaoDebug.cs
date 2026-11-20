using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// N1 (NACAO.md): "nation [show|lock|classic|faith|throne|proclaim [i]|bribe|giveup|revolt|win|lose leader|lose throne|retry|invasion|found]".
    /// </summary>
    public static class EQNacaoDebug
    {
        private static Building_Throne lastThrone;

        private static WorldComponent_EQNation C => WorldComponent_EQNation.Current;

        public static string Run(string[] a)
        {
            string sub = a.Length > 1 ? a[1] : "show";
            var c = C;
            if (c == null) return "sem WorldComponent_EQNation";
            switch (sub)
            {
                case "show": return Show();
                case "lock":
                    EQNation.TestAssumeFounded = false;
                    c.founded = false; c.stage = NationStage.Settlement; c.failures = 0; c.retryTick = -1;
                    return "trava ligada (nação não fundada)\n" + Show();
                case "throne": return BuildThrone();
                case "proclaim":
                {
                    List<Pawn> cands = EQLeader.Candidates();
                    int i = a.Length > 2 ? int.Parse(a[2]) : 0;
                    string can = EQNation.CanProclaim(lastThrone);
                    if (can != null) return "RECUSADO: " + can;
                    if (i < 0 || i >= cands.Count) return "candidato inválido (há " + cands.Count + ")";
                    string err = c.Proclaim(cands[i], lastThrone);
                    return (err != null ? "FALHOU: " + err : EQNation.LastReport) + "\n" + Show() + "\n  carta: " + LetterOf();
                }
                case "bribe":
                {
                    Map m = c.map ?? Find.AnyPlayerHomeMap;
                    int cost = EQNation.BribeCost(c.points);
                    Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                    silver.stackCount = cost;
                    GenPlace.TryPlaceThing(silver, m.Center, m, ThingPlaceMode.Near);
                    string err = c.Bribe();
                    return (err != null ? "FALHOU: " + err : EQNation.LastReport) + "; de novo: " + (c.Bribe() ?? "aceitou (ERRO)");
                }
                case "giveup": c.GiveUp(); return EQNation.LastReport + "\n" + Show();
                case "revolt":
                    if (c.stage != NationStage.Proclaimed) return "sem proclamação pendente";
                    c.StartRevolt();
                    return EQNation.LastReport + "\n" + Show();
                case "clear":
                case "win":
                {
                    if (sub == "win" && c.stage != NationStage.Revolt) return "sem revolta";
                    int n = 0;
                    Faction rf = EQNation.RevoltFaction;
                    foreach (Pawn p in c.revolters.Concat(c.map?.mapPawns.SpawnedPawnsInFaction(rf) ?? new List<Pawn>()).Distinct().ToList())
                        if (!p.Dead && p.Spawned) { p.Kill(null); n++; }
                    if (sub == "clear") return "revoltosos que sobraram mortos=" + n;
                    c.CheckRevolt();
                    return "revoltosos mortos=" + n + "; " + EQNation.LastReport + "\n" + Show();
                }
                case "lose":
                {
                    if (c.stage != NationStage.Revolt) return "sem revolta";
                    if (a.Length > 2 && a[2] == "throne")
                    {
                        Room room = c.throne?.GetRoom();
                        Pawn p = c.revolters.FirstOrDefault(x => !WorldComponent_EQNation.OutOfFight(x, c.map));
                        if (room == null || p == null) return "sem sala do trono ou sem revoltoso";
                        IntVec3 cell = room.Cells.FirstOrDefault(x => x.Standable(c.map) && x.GetFirstPawn(c.map) == null);
                        p.Position = cell; p.Notify_Teleported();
                        c.throneTicks = (int)(NationDef.Active.throneHoldHours * GenDate.TicksPerHour) - 250;
                        c.CheckRevolt();
                        return "revoltoso " + p.LabelShort + " na sala do trono; " + EQNation.LastReport + "\n" + Show();
                    }
                    c.leader?.Kill(null);
                    c.CheckRevolt();
                    return "líder morto; " + EQNation.LastReport + "\n" + Show();
                }
                case "retry": c.retryTick = -1; return "espera zerada\n" + Show();
                case "classic": return EQNationFaith.MakePlayerClassic() + "\n" + Show();
                case "faith": return "fé: " + (EQNationFaith.LastReport ?? "-") + "\n" + Show();
                case "found": c.Victory(); return EQNation.LastReport + "\n" + Show();
                case "invasion":
                {
                    if (EQNation.InvasionVetoProbe == null) return "sem sonda do Hegemony";
                    Settlement home = Find.WorldObjects.Settlements.FirstOrDefault(s => s.Faction == Faction.OfPlayer && s.HasMap);
                    Faction att = EQAllegiance.Powers.FirstOrDefault() ?? Find.FactionManager.RandomEnemyFaction();
                    return "veto de invasão (" + att?.Name + " → " + home?.LabelCap + "): " + (EQNation.InvasionVetoProbe(att, home) ?? "nenhum");
                }
            }
            return "subcomando desconhecido: " + sub;
        }

        public static string Show()
        {
            var c = C;
            return c.Report() + "\n  adultos livres=" + EQNation.AdultFreeColonists() + " candidatos=" + EQLeader.Candidates().Count
                   + " proclamar: " + (EQNation.CanProclaim(lastThrone) ?? "pode")
                   + "\n  fronteira: " + (EQNation.Founded ? "liberada" : EQNation.NoFrontierReason)
                   + "\n  começo clássico neste cenário: " + EQNationFaith.ClassicStartApplies(Find.Scenario)
                   + "\n  ideologias: principal=" + Faction.OfPlayer.ideos?.PrimaryIdeo?.name + " | " + string.Join(", ", EQLeader.AllFreeColonists().Distinct().Select(p => p.LabelShort + "=" + p.Ideo?.name));
        }

        private static string LetterOf()
        {
            var l = Find.LetterStack.LettersListForReading.OfType<ChoiceLetter_EQProclamation>().LastOrDefault();
            return l == null ? "nenhuma" : l.Label + " | opções: " + string.Join(" / ", l.Choices.Select(o => HarmonyLib.Traverse.Create(o).Field("text").GetValue<string>() + (o.disabled ? " [" + o.disabledReason + "]" : "")));
        }

        /// <summary>Sala do trono 5×5 (paredes de aço, teto, trono de madeira) perto do centro do mapa.</summary>
        private static string BuildThrone()
        {
            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return "sem mapa";
            ThingDef throneDef = DefDatabase<ThingDef>.GetNamedSilentFail("Throne");
            if (throneDef == null) return "sem Royalty";
            IntVec3 origin = IntVec3.Invalid;
            foreach (IntVec3 c in GenRadial.RadialCellsAround(map.Center, 40f, true))
            {
                var rect = CellRect.CenteredOn(c, 3);
                if (!rect.InBounds(map)) continue;
                if (rect.Cells.All(x => x.Standable(map) && x.GetEdifice(map) == null && !x.Roofed(map) && x.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy)
                                         && x.GetFirstPawn(map) == null && !x.GetThingList(map).Any(t => t.def.category == ThingCategory.Item || t.def.category == ThingCategory.Building)))
                { origin = c; break; }
            }
            if (!origin.IsValid) return "sem lugar para a sala do trono";
            var outer = CellRect.CenteredOn(origin, 3);
            foreach (IntVec3 x in outer.Cells)
                foreach (Thing t in x.GetThingList(map).Where(t => t.def.category == ThingCategory.Plant).ToList()) t.Destroy();
            foreach (IntVec3 x in outer.EdgeCells)
            {
                Thing w = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Steel);
                w.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(w, x, map);
            }
            foreach (IntVec3 x in outer.ContractedBy(1).Cells) map.roofGrid.SetRoof(x, RoofDefOf.RoofConstructed);
            Thing th = ThingMaker.MakeThing(throneDef, ThingDefOf.WoodLog);
            th.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(th, origin, map, Rot4.South);
            map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
            lastThrone = th as Building_Throne;
            Room room = lastThrone?.GetRoom();
            return "trono em " + origin + " sala=" + (room?.Role?.defName ?? "-") + " válida=" + EQNation.IsThroneRoom(lastThrone)
                   + " botão: " + (EQNation.CanProclaim(lastThrone) ?? "pode");
        }
    }
}
