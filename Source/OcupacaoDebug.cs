using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// F5 (MUNDO.md › Resultado do F5): "occupation [show | auto | win &lt;a&gt; &lt;b&gt; | front &lt;a&gt; &lt;b&gt; | player [a|auto] | playerwin [a|auto] | retake | tiles &lt;a&gt; &lt;b&gt; | age &lt;dias&gt; | clear]".
    /// auto: o primeiro par de facções com fronteira encostada; win: a vence b na fronteira (direto); front: abre e resolve uma
    /// frente de fronteira de verdade (o atacante vence); player: a vence o jogador; tiles: conta os tiles de cada um.
    /// </summary>
    public static class EQOcupacaoDebug
    {
        private static Faction Fac(string s)
        {
            if (s == "player") return Faction.OfPlayer;
            Faction f = EQDebug.FindFaction(s);
            if (f == null) throw new Exception("facção não encontrada: " + s);
            return EQUtil.Canonical(f);
        }

        public static int Tiles(Faction f)
        {
            List<KeyValuePair<PlanetTile, Faction>> claims = null;
            try { claims = EQTerritory.ClaimsProvider?.Invoke(); } catch (Exception) { }
            Faction c = EQUtil.Canonical(f);
            return claims?.Count(x => EQUtil.Canonical(x.Value) == c) ?? -1;
        }

        private static string Pair(Faction a, Faction b) =>
            (a.IsPlayer ? "jogador" : a.def.defName.Replace("EQ_Faction_", "")) + "=" + Tiles(a) + " " + (b.IsPlayer ? "jogador" : b.def.defName.Replace("EQ_Faction_", "")) + "=" + Tiles(b);

        public static string Run(string[] a)
        {
            string Arg(int i) => i < a.Length ? a[i] : null;
            string sub = Arg(1) ?? "show";
            switch (sub)
            {
                case "show": return EQOccupation.Report();
                case "auto":
                {
                    var facs = Find.FactionManager.AllFactionsVisible.Where(f => !f.IsPlayer && !f.defeated && !f.temporary)
                        .OrderByDescending(f => EQAllegiance.IsPower(f) ? 1 : 0).ToList();
                    foreach (Faction x in facs)
                        foreach (Faction y in facs)
                        {
                            if (x == y) continue;
                            List<PlanetTile> border = null;
                            try { border = EQWarDiplomacy.BorderProvider?.Invoke(x, y); } catch (Exception) { }
                            if (border == null || border.Count == 0) continue;
                            return "par " + x.Name + " × " + y.Name + " (" + border.Count + " tiles de fronteira) | antes: " + Pair(x, y) + " | " + Win(x, y);
                        }
                    // Sem território encostado: as duas potências com cidades mais perto (a frente fica no meio do caminho).
                    var powers = facs.Where(EQAllegiance.IsPower).ToList();
                    Faction bx = null, by = null; float bd = float.MaxValue;
                    foreach (Settlement sx in Find.WorldObjects.Settlements.Where(s => powers.Contains(s.Faction)))
                        foreach (Settlement sy in Find.WorldObjects.Settlements.Where(s => powers.Contains(s.Faction) && s.Faction != sx.Faction && s.Tile.Layer == sx.Tile.Layer))
                        {
                            float dd = Find.WorldGrid.ApproxDistanceInTiles(sx.Tile, sy.Tile);
                            if (dd < bd) { bd = dd; bx = sx.Faction; by = sy.Faction; }
                        }
                    if (bx == null) return "nenhum par de facções";
                    return "sem fronteira encostada; par mais perto " + bx.Name + " × " + by.Name + " (" + bd.ToString("0") + " tiles) | antes: " + Pair(bx, by) + " | " + Win(bx, by);
                }
                case "win": return Win(Fac(Arg(2)), Fac(Arg(3)));
                case "player":
                case "playerwin":
                {
                    // "player auto": a facção da cidade de NPC mais perto da colônia vence o jogador; "playerwin": o jogador vence ela
                    Faction x;
                    if (Arg(2) == null || Arg(2) == "auto")
                    {
                        Settlement home = Find.WorldObjects.Settlements.FirstOrDefault(s => s.Faction == Faction.OfPlayer);
                        if (home == null) return "sem colônia";
                        x = Find.WorldObjects.Settlements.Where(s => s.Faction != null && !s.Faction.IsPlayer && s.Tile.Layer == home.Tile.Layer)
                            .OrderBy(s => Find.WorldGrid.ApproxDistanceInTiles(s.Tile, home.Tile)).FirstOrDefault()?.Faction;
                        if (x == null) return "sem vizinho";
                    }
                    else x = Fac(Arg(2));
                    if (sub == "playerwin") return "antes: " + Pair(x, Faction.OfPlayer) + " | " + Win(Faction.OfPlayer, x);
                    return "antes: " + Pair(x, Faction.OfPlayer) + " defesa=" + EQCouncil.Channel("borderDefense").ToString("0.00") + " | " + Win(x, Faction.OfPlayer);
                }
                case "retake":
                {
                    // o perdedor do posto mais recente vence uma frente ali
                    var post = EQOccupation.Posts().OrderByDescending(x => x.placedTick).FirstOrDefault();
                    if (post == null) return "sem posto";
                    return "retomada: " + EQOccupation.OnBorderWin(post.Tile, post.loser, post.Faction);
                }
                case "front":
                {
                    var comp = WorldComponent_EQForces.Current;
                    Faction x = Fac(Arg(2)), y = Fac(Arg(3));
                    string open = EQWarDiplomacy.OpenBorderFront(comp, x, y);
                    var front = Find.WorldObjects.AllWorldObjects.OfType<WorldObject_EQFront>().LastOrDefault(f => f.borderFront && !f.resolved);
                    if (front == null) return "frente não abriu: " + open;
                    string r = front.Resolve(true, "teste");
                    return "frente: " + open + " | resultado: " + r;
                }
                case "tiles": return "tiles: " + Pair(Fac(Arg(2)), Fac(Arg(3))) + "\n" + EQOccupation.Report();
                case "age":
                {
                    int ticks = (int)(float.Parse(Arg(2), System.Globalization.CultureInfo.InvariantCulture) * GenDate.TicksPerDay);
                    foreach (var p in EQOccupation.Posts()) p.placedTick -= ticks;
                    return "postos envelhecidos\n" + EQOccupation.Report();
                }
                case "clear":
                {
                    int n = 0;
                    foreach (var p in EQOccupation.Posts().ToList()) { p.Destroy(); n++; }
                    return n + " postos removidos";
                }
            }
            return "subcomando desconhecido: " + sub;
        }

        /// <summary>"tradergroup &lt;facção&gt;": gera (sem pôr no mapa) a caravana de comércio da facção no tile da colônia.</summary>
        public static string TraderGroup(string name)
        {
            Faction f = Fac(name);
            Settlement home = Find.WorldObjects.Settlements.FirstOrDefault(s => s.Faction == Faction.OfPlayer);
            TraderKindDef tk = f.def.caravanTraderKinds.FirstOrDefault();
            var parms = new PawnGroupMakerParms { groupKind = PawnGroupKindDefOf.Trader, faction = f, points = 800f, tile = home?.Tile ?? PlanetTile.Invalid, traderKind = tk };
            List<Pawn> pawns = PawnGroupMakerUtility.GeneratePawns(parms).ToList();
            string r = f.def.defName + " (" + (tk?.defName ?? "sem tipo") + "): " + pawns.Count + " pawns, comerciantes=" + pawns.Count(p => p.TraderKind != null)
                       + " [" + string.Join(",", pawns.GroupBy(p => p.kindDef.defName).Select(g => g.Key + "x" + g.Count())) + "]";
            foreach (Pawn p in pawns) p.Destroy();
            return r;
        }

        private static string Win(Faction win, Faction lose)
        {
            PlanetTile tile = EQWarDiplomacy.FindBorderTile(win, lose, out string how);
            if (!tile.Valid) return "sem tile de fronteira (" + how + ")";
            List<KeyValuePair<PlanetTile, Faction>> claims = null;
            try { claims = EQTerritory.ClaimsProvider?.Invoke(); } catch (Exception) { }
            PlanetTile pt = EQOccupation.FindTile(tile, lose);
            string before = pt.Valid ? " antes em volta do posto: " + EQOccupation.Around(pt, 3f, claims) : "";
            return "frente em " + tile + " (" + how + ")" + before + ": " + EQOccupation.OnBorderWin(tile, win, lose);
        }
    }
}
