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
    /// Etapa 10d (resto): as potências declaram guerra por eventos nossos e brigam nas fronteiras.
    /// - Tensão por par de potências (0–100): cresce todo dia pela relação fixa (quanto mais ódio, mais rápido) e por
    ///   eventos (fim de frente: quem perdeu quer revanche). Em 50, incidente de fronteira (carta); em 75, se o jogador
    ///   tem cargo alto num dos lados, a patrona consulta (inflamar: +favor e +tensão; acalmar: -tensão, custa favor);
    ///   em 100, o lado mais rico declara guerra pelo Hegemony (ou guerra forçada sem ele) e a tensão zera.
    /// - Frentes de fronteira: em guerra, onde os territórios se encostam (Hegemony) ou entre as cidades mais próximas,
    ///   abre-se uma frente de trincheira (Fronts.cs, modo fronteira); quem perde perde guarnição e tesouro.
    /// </summary>
    public static class EQWarDiplomacy
    {
        public const float IncidentAt = 50f, ConsultAt = 75f, WarAt = 100f;
        public const float GrowthPerDayAtHate = 3f;   // a -100 de boa vontade fixa
        public const float BorderFrontChancePerDay = 0.25f;
        public const int MaxBorderFronts = 2;
        public const float BorderCooldownDays = 5f;
        /// <summary>Cargo mínimo (índice) para a patrona consultar o jogador.</summary>
        public const int ConsultRank = 2;

        /// <summary>Tiles da fronteira entre duas facções (território do Hegemony). Preenchido pelo módulo do Hegemony.</summary>
        public static Func<Faction, Faction, List<PlanetTile>> BorderProvider;

        public static string Key(Faction a, Faction b)
        {
            string x = a.def.defName, y = b.def.defName;
            return string.CompareOrdinal(x, y) < 0 ? x + "|" + y : y + "|" + x;
        }

        public static int FixedGoodwill(Faction a, Faction b)
        {
            FactionRelationEntry e = FactionRelationPresetDef.Find(a.def, b.def);
            return e?.goodwill ?? a.GoodwillWith(b);
        }

        public static float Growth(Faction a, Faction b) => Mathf.Max(0f, -FixedGoodwill(a, b)) / 100f * GrowthPerDayAtHate;

        public static IEnumerable<KeyValuePair<Faction, Faction>> PowerPairs()
        {
            var ps = EQAllegiance.Powers.Where(p => EQForces.Valid(p)).ToList();
            for (int i = 0; i < ps.Count; i++)
                for (int j = i + 1; j < ps.Count; j++)
                    yield return new KeyValuePair<Faction, Faction>(ps[i], ps[j]);
        }

        public static float Get(Faction a, Faction b)
        {
            var c = WorldComponent_EQForces.Current;
            return c != null && c.tension.TryGetValue(Key(a, b), out float v) ? v : 0f;
        }

        /// <summary>Soma tensão (eventos nossos). Não passa de WarAt; a checagem diária dispara os limiares.</summary>
        public static void Add(Faction a, Faction b, float amount, string why)
        {
            a = EQUtil.Canonical(a); b = EQUtil.Canonical(b);
            var c = WorldComponent_EQForces.Current;
            if (c == null || a == null || b == null || a == b || !EQAllegiance.IsPower(a) || !EQAllegiance.IsPower(b)) return;
            string k = Key(a, b);
            float v = Mathf.Clamp((c.tension.TryGetValue(k, out float t) ? t : 0f) + amount, 0f, WarAt);
            c.tension[k] = v;
            if (!why.NullOrEmpty()) c.Log("Tensão " + a.Name + " × " + b.Name + " " + (amount >= 0 ? "+" : "") + amount.ToString("0") + " (" + why + ") = " + v.ToString("0"), false);
        }

        public static string LastDaily;

        /// <summary>Uma vez por dia (WorldComponent_EQForces.Daily).</summary>
        public static string Daily(WorldComponent_EQForces c)
        {
            var sb = new List<string>();
            foreach (var p in PowerPairs())
            {
                Faction a = p.Key, b = p.Value;
                string k = Key(a, b);
                if (EQForces.AtWar(a, b)) { c.tension[k] = 0f; continue; }
                float g = Growth(a, b);
                if (g <= 0f) continue;
                Faction patron = EQAllegiance.PlayerOverlord;
                if (patron != null && (a == patron || b == patron)) g *= EQCouncil.PatronTensionFactor;
                float before = c.tension.TryGetValue(k, out float t) ? t : 0f;
                float now = Mathf.Min(WarAt, before + g * Rand.Range(0.6f, 1.4f));
                c.tension[k] = now;
                sb.Add(a.def.defName.Replace("EQ_Faction_", "") + "×" + b.def.defName.Replace("EQ_Faction_", "") + "=" + now.ToString("0"));
                sb.Add(Thresholds(c, a, b, before, now));
            }
            sb.Add(BorderFronts(c, false));
            LastDaily = string.Join(" ", sb.Where(s => !s.NullOrEmpty()));
            return LastDaily;
        }

        /// <summary>Dispara o que o salto de tensão cruzou.</summary>
        public static string Thresholds(WorldComponent_EQForces c, Faction a, Faction b, float before, float now)
        {
            string r = null;
            if (before < IncidentAt && now >= IncidentAt)
            {
                Find.LetterStack.ReceiveLetter("Incidente de fronteira: " + a.Name + " × " + b.Name,
                    Incident(a, b) + "\n\nNinguém declarou nada ainda. Mas os dois lados começaram a contar soldados.",
                    LetterDefOf.NeutralEvent, LookTargets.Invalid, a);
                c.Log("Incidente de fronteira: " + a.Name + " × " + b.Name + ".", false);
                r = "[incidente]";
            }
            if (before < ConsultAt && now >= ConsultAt)
                r = (r ?? "") + "[consulta:" + Consult(a, b) + "]";
            if (now >= WarAt)
                r = (r ?? "") + "[guerra:" + DeclareWar(c, a, b) + "]";
            return r;
        }

        private static string Incident(Faction a, Faction b)
        {
            string[] t =
            {
                "Uma patrulha de {A} atirou numa caravana de {B} \"por engano\". Duas vezes.",
                "{B} acusa {A} de mudar os marcos da fronteira durante a noite. {A} diz que os marcos andam sozinhos.",
                "Um emissário de {A} voltou de {B} sem a língua e com um recibo.",
                "{A} e {B} reivindicam o mesmo poço. O poço está seco, o que não ajudou a conversa.",
            };
            return t.RandomElement().Replace("{A}", a.Name).Replace("{B}", b.Name);
        }

        public static string LastConsult;

        /// <summary>A patrona (cargo alto) pergunta ao jogador se a guerra com o outro lado deve vir.</summary>
        public static string Consult(Faction a, Faction b)
        {
            var al = WorldComponent_Allegiance.Current;
            Faction patron = al?.memberOf;
            if (patron == null || (patron != a && patron != b) || al.RankIndex < ConsultRank || patron.HostileTo(Faction.OfPlayer)) return "sem consulta";
            Faction other = patron == a ? b : a;
            LetterDef def = DefDatabase<LetterDef>.GetNamedSilentFail("EQ_TensionLetter");
            if (def == null) return "sem def";
            var l = (ChoiceLetter_EQTension)LetterMaker.MakeLetter("Conselho de guerra: " + other.Name,
                patron.Name + " está a um passo da guerra com " + other.Name + ". Como " + al.RankLabel + ", a sua opinião foi pedida (e anotada).\n\n"
                + "Inflamar: a guerra vem mais cedo e a patrona lembra de quem a apoiou (favor). Acalmar: a tensão cai, mas os falcões da corte cobram em favor.",
                def, patron);
            l.other = other;
            l.deadline = Find.TickManager.TicksGame + 2 * 60000;
            Find.LetterStack.ReceiveLetter(l);
            LastConsult = "consulta de " + patron.def.defName;
            return "enviada";
        }

        /// <summary>O lado mais rico declara a guerra (Hegemony; sem ele, guerra forçada).</summary>
        public static string DeclareWar(WorldComponent_EQForces c, Faction a, Faction b)
        {
            Faction atk = c.Treasury(a) >= c.Treasury(b) ? a : b, def = atk == a ? b : a;
            c.tension[Key(a, b)] = 0f;
            string why = EQForces.DeclareWarHandler?.Invoke(atk, def, "eq-tension-" + atk.loadID + "-" + def.loadID + "-" + Find.TickManager.TicksGame);
            if (EQForces.DeclareWarHandler == null || why != null)
            {
                if (EQForces.DeclareWarHandler != null && why != null) { c.Log("Guerra recusada pelo Hegemony (" + atk.Name + " × " + def.Name + "): " + why, false); return "recusada: " + why; }
                string key = atk.def.defName + "|" + def.def.defName;
                if (!c.forcedWars.Contains(key)) c.forcedWars.Add(key);
            }
            Find.LetterStack.ReceiveLetter("Guerra declarada: " + atk.Name + " × " + def.Name,
                atk.Name + " declarou guerra a " + def.Name + ". Os incidentes de fronteira viraram motivo, e o motivo virou papel timbrado.\n\nOs exércitos se levantam nos próximos dias; onde os territórios se encostam, cavam-se trincheiras.",
                LetterDefOf.ThreatSmall, LookTargets.Invalid, atk);
            c.Log("Guerra declarada por tensão: " + atk.Name + " × " + def.Name + ".", true);
            if (EQAllegiance.IsPower(atk)) c.TryMobilize(atk, def, out _);
            return atk.def.defName + "→" + def.def.defName;
        }

        // ------------------------------------------------------------ frentes de fronteira

        public static string BorderFronts(WorldComponent_EQForces c, bool force, Faction onlyA = null, Faction onlyB = null)
        {
            if (EQFronts.FrontDef == null) return "sem def";
            int open = Find.WorldObjects.AllWorldObjects.OfType<WorldObject_EQFront>().Count(f => f.borderFront && !f.resolved);
            var r = new List<string>();
            foreach (var w in EQForces.Wars())
            {
                Faction a = w.Key, b = w.Value;
                if (onlyA != null && !(a == onlyA && b == onlyB || a == onlyB && b == onlyA)) continue;
                if (!EQAllegiance.IsPower(a) && !EQAllegiance.IsPower(b)) continue;
                if (open >= MaxBorderFronts) break;
                if (Find.WorldObjects.AllWorldObjects.OfType<WorldObject_EQFront>().Any(f => f.borderFront && !f.resolved && (f.attacker == a && f.defender == b || f.attacker == b && f.defender == a))) continue;
                if (!force && c.lastBorderFront.TryGetValue(Key(a, b), out int last) && Find.TickManager.TicksGame - last < BorderCooldownDays * 60000) continue;
                if (!force && !Rand.Chance(BorderFrontChancePerDay)) continue;
                string s = OpenBorderFront(c, a, b);
                r.Add(s);
                if (s.StartsWith("aberta")) open++;
            }
            return r.Count == 0 ? null : "fronteira: " + string.Join("; ", r);
        }

        /// <summary>Tile da fronteira: território do Hegemony encostado; sem ele, o meio do caminho entre as cidades mais próximas.</summary>
        public static PlanetTile FindBorderTile(Faction a, Faction b, out string how)
        {
            how = "cidades";
            if (!EQForces.TryFindFront(a, b, out Settlement ha, out Settlement tb)) { how = "sem cidades"; return PlanetTile.Invalid; }
            List<PlanetTile> border = null;
            try { border = BorderProvider?.Invoke(a, b); } catch (Exception ex) { Log.Warning("[Ecos da Queda] fronteira: " + ex.Message); }
            PlanetTile start;
            if (border != null && border.Count > 0)
            {
                how = "território (" + border.Count + " tiles de fronteira)";
                start = border.OrderBy(t => EQForces.Distance(t, ha.Tile) + EQForces.Distance(t, tb.Tile)).First();
            }
            else
            {
                // anda do assentamento de A em direção ao de B até a metade
                start = ha.Tile;
                float half = EQForces.Distance(ha.Tile, tb.Tile) / 2f;
                var tmp = new List<PlanetTile>();
                for (int i = 0; i < 200 && EQForces.Distance(start, ha.Tile) < half; i++)
                {
                    tmp.Clear();
                    Find.WorldGrid.GetTileNeighbors(start, tmp);
                    PlanetTile next = tmp.Where(n => !Find.World.Impassable(n)).OrderBy(n => EQForces.Distance(n, tb.Tile)).FirstOrDefault();
                    if (!next.Valid || EQForces.Distance(next, tb.Tile) >= EQForces.Distance(start, tb.Tile)) break;
                    start = next;
                }
            }
            return EQFronts.FindFrontTile(start);
        }

        public static string OpenBorderFront(WorldComponent_EQForces c, Faction a, Faction b)
        {
            PlanetTile tile = FindBorderTile(a, b, out string how);
            if (!tile.Valid) return "sem tile (" + how + ")";
            Faction atk = c.Treasury(a) >= c.Treasury(b) ? a : b, def = atk == a ? b : a;
            EQForces.TryFindFront(atk, def, out Settlement hA, out Settlement hD);
            var front = (WorldObject_EQFront)WorldObjectMaker.MakeWorldObject(EQFronts.FrontDef);
            front.Tile = tile;
            front.SetFaction(atk);
            front.borderFront = true;
            front.attacker = atk;
            front.defender = def;
            front.targetSettlement = hD;
            front.attackerSettlement = hA;
            front.attackerStrength = Mathf.Clamp(c.GarrisonOf(hA) / 2 + Rand.RangeInclusive(0, 6), 8, 40);
            front.defenderStrength = Mathf.Clamp(c.GarrisonOf(hD) / 2 + Rand.RangeInclusive(0, 6), 8, 40);
            front.openedTick = Find.TickManager.TicksGame;
            front.deadlineTick = front.openedTick + (int)(WorldObject_EQFront.OpenDays * 60000);
            front.wereHostile = atk.HostileTo(def);
            if (!front.wereHostile)
            {
                front.prevGoodwill = atk.GoodwillWith(def);
                EQFronts.SetGoodwill(atk, def, -100);
            }
            Find.WorldObjects.Add(front);
            Find.LetterStack.ReceiveLetter("Frente de fronteira: " + atk.Name + " × " + def.Name,
                "Onde as terras de " + atk.Name + " e " + def.Name + " se encostam, os dois lados cavaram trincheiras: "
                + front.attackerStrength + " contra " + front.defenderStrength + " soldados. Quem perder recua, perde gente da guarnição de "
                + (hD?.LabelCap ?? "?") + " ou " + (hA?.LabelCap ?? "?") + " e paga a conta.\n\nVá lutar por um lado, fique de fora ou só recolha o que sobrar. Em "
                + WorldObject_EQFront.OpenDays.ToString("0") + " dias, sem testemunhas, decide-se sozinha.",
                LetterDefOf.NeutralEvent, front);
            c.Log("Frente de fronteira aberta: " + front.Label + " (" + how + ").", false);
            return "aberta: " + front.Label + " " + front.attackerStrength + "x" + front.defenderStrength + " por " + how;
        }

        /// <summary>Resultado de uma frente de fronteira: quem perde perde guarnição e tesouro; revanche vira tensão depois da paz.</summary>
        public static string BorderOutcome(WorldObject_EQFront f, bool attackerWon)
        {
            var c = WorldComponent_EQForces.Current;
            if (c == null) return "";
            Faction win = attackerWon ? f.attacker : f.defender, lose = attackerWon ? f.defender : f.attacker;
            Settlement lostAt = attackerWon ? f.targetSettlement : f.attackerSettlement;
            int loss = Rand.RangeInclusive(8, 15);
            if (lostAt != null && !lostAt.Destroyed) c.SetGarrison(lostAt, c.GarrisonOf(lostAt) - loss);
            c.SetTreasury(win, c.Treasury(win) + 200f);
            c.SetTreasury(lose, c.Treasury(lose) - 150f);
            foreach (var army in c.ArmiesOf(win)) army.morale = Math.Min(1f, army.morale + 0.1f);
            foreach (var army in c.ArmiesOf(lose)) army.morale = Math.Max(0f, army.morale - 0.1f);
            c.lastBorderFront[Key(win, lose)] = Find.TickManager.TicksGame;
            string s = lose.Name + " recua: guarnição de " + (lostAt?.LabelCap ?? "?") + " -" + loss + ", tesouro -150; " + win.Name + " +200.";
            c.Log("Fim da frente de fronteira: " + s, false);
            return s;
        }
    }

    /// <summary>Conselho de guerra: inflamar (+favor, +tensão) ou acalmar (-tensão, -favor). Prazo de 2 dias = a corte decide sem você.</summary>
    public class ChoiceLetter_EQTension : ChoiceLetter
    {
        public Faction other;
        public int deadline = -1;
        public bool resolved;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || resolved) { yield return Option_Close; yield break; }
                yield return new DiaOption("Inflamar: \"que venha a guerra\" (+3 de favor, a guerra vem já)")
                    { resolveTree = true, action = () => { Choose(true); Find.LetterStack.RemoveLetter(this); } };
                yield return new DiaOption("Acalmar: \"ainda não\" (tensão cai 40, -2 de favor)")
                    { resolveTree = true, action = () => { Choose(false); Find.LetterStack.RemoveLetter(this); } };
                yield return Option_Postpone;
            }
        }

        public static string LastResult;

        public void Choose(bool inflame)
        {
            if (resolved) return;
            resolved = true;
            Faction patron = relatedFaction;
            var c = WorldComponent_EQForces.Current;
            if (patron == null || other == null || c == null) return;
            if (inflame)
            {
                EQFavor.Award(patron, 3, "conselho de guerra");
                float before = EQWarDiplomacy.Get(patron, other);
                EQWarDiplomacy.Add(patron, other, EQWarDiplomacy.WarAt, "o vassalo inflamou o conselho");
                LastResult = "inflamou: " + EQWarDiplomacy.Thresholds(c, patron, other, before, EQWarDiplomacy.Get(patron, other));
            }
            else
            {
                EQFavor.Award(patron, -2, "conselho de guerra");
                EQWarDiplomacy.Add(patron, other, -40f, "o vassalo acalmou o conselho");
                LastResult = "acalmou: tensão " + EQWarDiplomacy.Get(patron, other).ToString("0");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref other, "other");
            Scribe_Values.Look(ref deadline, "deadline", -1);
            Scribe_Values.Look(ref resolved, "resolved");
        }
    }
}
