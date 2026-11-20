using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    public class SeatRename { public string key; public string label; }
    public class SeatFactor { public string key; public float factor = 1f; }

    /// <summary>
    /// N2b (NACAO.md › 3.7, nacao/memes.md grupos 1 e 2): o que um meme da ideologia do jogador acrescenta ao governo.
    /// O meme vai por defName (texto), então memes de mods ausentes ficam inertes sem erro. Cadeiras com a mesma chave
    /// vindas de memes diferentes somam pilha (CouncilDef.seatStackBonus). Tudo vale com a nação fundada, menos o canal
    /// "revolt" dos efeitos fixos (vale na revolta da proclamação).
    /// </summary>
    public class GovMemeDef : Def
    {
        public string meme;
        /// <summary>Cadeiras que o meme acrescenta (manuais ou de função).</summary>
        public List<CouncilSeat> seats = new List<CouncilSeat>();
        /// <summary>Efeitos fixos (não dependem de cadeira).</summary>
        public List<GovEffect> effects = new List<GovEffect>();
        /// <summary>Multiplica todas as cadeiras (Coletivista 0,8; Individualista 1,3 depois de fechar uma).</summary>
        public float seatFactor = 1f;
        public List<SeatFactor> seatFactors = new List<SeatFactor>();
        public List<SeatRename> renames = new List<SeatRename>();
        /// <summary>Cadeiras (chaves) que passam a ser do líder (Sacerdócio exaltado).</summary>
        public List<string> leaderTakes = new List<string>();
        /// <summary>Deus-imperador: com a cadeira fixa vazia, o líder vale esta fração dela.</summary>
        public float leaderFillsEmpty;
        public bool dropOneFixedSeat, noPassedOver, noSwapCooldown, moodScaled, majorDecisionVote, fixedSeatsRequireTitle;
        public int fixedSeatMinAge;
        // ---- N2c (grupos 3 e 4)
        /// <summary>Cadeiras (chaves) que o meme tira, fixas ou de outros memes.</summary>
        public List<string> blockSeats = new List<string>();
        /// <summary>Efeitos multiplicados pelo número de cadeiras ocupadas e valendo (Progressista).</summary>
        public List<GovEffect> perSeat = new List<GovEffect>();
        /// <summary>Bacanal: bônus pelo humor médio do conselho.</summary>
        public bool councilMoodScaled;
        /// <summary>Pré-fabricados: o marco é pago em aço (metade do número).</summary>
        public bool markerSteel;
        /// <summary>Nômade: o marco some depois destes dias de pronto (0 = nunca).</summary>
        public float markerLifeDays;
        /// <summary>Na vitória da revolta (fundação): boa vontade com as facções listadas (vazio = todas as visíveis) e Fama com todas.</summary>
        public int foundingGoodwill;
        public List<string> foundingGoodwillFactions = new List<string>();
        public float foundingFame;
        /// <summary>Pacifista: a carta da proclamação oferece negociar (Social do líder).</summary>
        public bool negotiateRevolt;
        /// <summary>Conversão violenta: na vitória, até este número de revoltosos caídos se junta à colônia (mínimo 1).</summary>
        public int convertRevolters;
        // ---- N3 (sucessão coroada e legitimidade)
        /// <summary>República: troca de líder sem coroação.</summary>
        public bool successionNoCrown;
        /// <summary>Real, Lealista: o herdeiro designado assume já coroado.</summary>
        public bool successionHeirCrowned;
        /// <summary>Real: sem o herdeiro designado, a revolta da sucessão vem maior.</summary>
        public float successionNoHeirFactor = 1f;
        /// <summary>Deus-imperador: morte do líder traz a revolta sem prazo, deste tamanho, e o conselho cai.</summary>
        public bool successionRevoltNow, successionCouncilFalls;
        public float successionRevoltFactor = 1f;
        /// <summary>Corte de sangue: executar conselheiro dá legitimidade. Culpado: perder revolta dá (expiação).</summary>
        public bool bloodCourtLegitimacy, guiltyExpiation;
        /// <summary>Regra de sucessão (texto para a janela do conselho).</summary>
        [MustTranslate] public string succession;
        [MustTranslate] public string summary;

        [Unsaved] private MemeDef memeDef;
        [Unsaved] private bool resolved;

        public MemeDef MemeDef
        {
            get
            {
                if (!resolved) { memeDef = DefDatabase<MemeDef>.GetNamedSilentFail(meme ?? ""); resolved = true; }
                return memeDef;
            }
        }

        public string MemeLabel => MemeDef?.label ?? meme;
    }

    public static class EQGov
    {
        public static readonly string[] Triumvirs = { "VME_IdeoRole_LeaderSecond", "VME_IdeoRole_LeaderThird" };
        public const float VoteMood = 0.4f;

        /// <summary>Memes do governo em vigor (os da ideologia principal do jogador).</summary>
        public static List<GovMemeDef> ActiveDefs()
        {
            Ideo ideo = EQLeader.PlayerIdeo;
            if (ideo == null) return new List<GovMemeDef>();
            return DefDatabase<GovMemeDef>.AllDefsListForReading.Where(g => g.MemeDef != null && ideo.HasMeme(g.MemeDef)).ToList();
        }

        public static bool Rule(Func<GovMemeDef, bool> f) => ActiveDefs().Any(f);

        public static string LastFounding;

        /// <summary>Na vitória da revolta (com os memes da ideologia nesse momento): boa vontade, Fama, conversão de revoltosos.</summary>
        public static string OnFounding(WorldComponent_EQNation c)
        {
            var gov = ActiveDefs();
            var parts = new List<string>();
            foreach (GovMemeDef g in gov.Where(x => x.foundingGoodwill != 0))
            {
                var targets = Find.FactionManager.AllFactionsVisible.Where(f => !f.IsPlayer && !f.defeated && !f.temporary
                    && (g.foundingGoodwillFactions.Count == 0 || g.foundingGoodwillFactions.Contains(f.def.defName))).ToList();
                foreach (Faction f in targets) f.TryAffectGoodwillWith(Faction.OfPlayer, g.foundingGoodwill, canSendMessage: false, canSendHostilityLetter: false);
                parts.Add(g.MemeLabel + ": boa vontade " + g.foundingGoodwill.ToString("+0;-0") + " com " + targets.Count);
            }
            float fame = gov.Sum(x => x.foundingFame);
            var rep = WorldComponent_Reputation.Current;
            if (fame > 0f && rep != null)
            {
                int n = 0;
                foreach (Faction f in Find.FactionManager.AllFactionsVisible.Where(WorldComponent_Reputation.Tracks)) { rep.Record(f, fame, 0f); n++; }
                parts.Add("Fama +" + fame.ToString("0") + " com " + n);
            }
            int conv = gov.Count == 0 ? 0 : gov.Max(x => x.convertRevolters);
            if (conv > 0 && c?.map != null)
            {
                var downed = c.revolters.Where(p => p != null && !p.Dead && p.Spawned && p.Map == c.map && p.Downed && p.RaceProps.Humanlike).ToList();
                int take = Math.Min(downed.Count, Rand.RangeInclusive(1, conv));
                var got = downed.InRandomOrder().Take(take).ToList();
                foreach (Pawn p in got)
                {
                    RecruitUtility.Recruit(p, Faction.OfPlayer);
                    if (p.ideo != null && EQLeader.PlayerIdeo != null) p.ideo.SetIdeo(EQLeader.PlayerIdeo);
                }
                if (got.Count > 0)
                {
                    parts.Add("convertidos " + got.Count + " (" + string.Join(", ", got.Select(p => p.LabelShort)) + ")");
                    if (!EQLeader.SuppressDialogs)
                        Messages.Message(got.Count + " revoltosos caídos acordaram convencidos da causa. Voluntários, todos eles: " + string.Join(", ", got.Select(p => p.LabelShort)) + ".",
                            new LookTargets(got), MessageTypeDefOf.PositiveEvent, false);
                }
                else parts.Add("ninguém caído para converter");
            }
            LastFounding = parts.Count == 0 ? "nada" : string.Join("; ", parts);
            return LastFounding;
        }

        /// <summary>Isolacionista, Oráculo: chance (por dia e atacante) de uma invasão do Hegemony contra a nação não sair. null = sai.</summary>
        public static string InvasionVeto(Faction attacker)
        {
            if (!EQNation.Founded) return null;
            float ch = EQCouncil.Channel("invasionVeto");
            if (ch <= 0f) return null;
            int seed = Gen.HashCombineInt(Find.TickManager.TicksGame / GenDate.TicksPerDay, attacker?.loadID ?? 0);
            return Rand.ChanceSeeded(Mathf.Min(0.9f, ch), seed) ? "As fronteiras fechadas de " + Faction.OfPlayer.Name + " desanimaram a invasão (governo pela ideologia)." : null;
        }

        public static string Succession()
        {
            var s = ActiveDefs().Where(g => !g.succession.NullOrEmpty()).Select(g => g.MemeLabel + ": " + g.succession).ToList();
            return s.Count == 0 ? null : string.Join("; ", s);
        }

        /// <summary>República: decisões grandes (jurar lealdade, proclamar) precisam de 2 dos 3 triúnviros com humor acima de 40%. null = pode.</summary>
        public static string VoteBlock(string what)
        {
            if (!Rule(g => g.majorDecisionVote)) return null;
            var people = new List<Pawn> { EQLeader.IdeoLeader };
            foreach (string r in Triumvirs) people.Add(EQCouncil.RoleOf(r)?.ChosenPawns().FirstOrDefault());
            int yes = people.Count(p => p != null && !p.Dead && (p.needs?.mood?.CurLevelPercentage ?? 0f) > VoteMood);
            if (yes >= 2) return null;
            return "A república precisa de 2 dos 3 triúnviros de acordo (humor acima de " + (VoteMood * 100f).ToString("0") + "%) para " + what + ": "
                   + yes + " de 3. " + string.Join(", ", people.Select((p, i) => p == null ? (i == 0 ? "sem líder" : "sem " + EQCouncil.RoleLabel(Triumvirs[i - 1])) : p.LabelShort + " " + ((p.needs?.mood?.CurLevelPercentage ?? 0f) * 100f).ToString("0") + "%")) + ".";
        }
    }

    /// <summary>Canal do governo num atributo: multiplica (WorkTableWorkSpeedFactor, ResearchSpeed) ou soma (TradePriceImprovement).</summary>
    public class StatPart_EQGov : StatPart
    {
        public string channel;
        public bool additive;
        /// <summary>true: só construções do jogador; false: só colonos do jogador.</summary>
        public bool building;

        private bool Applies(StatRequest req, out float v)
        {
            v = 0f;
            Thing t = req.Thing;
            if (t == null || t.Faction == null || !t.Faction.IsPlayer || (building ? !(t is Building) : !(t is Pawn))) return false;
            if (Current.ProgramState != ProgramState.Playing || !EQNation.Founded) return false;
            v = EQCouncil.Channel(channel);
            return v != 0f;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!Applies(req, out float v)) return;
            if (additive) val += v; else val *= Mathf.Max(0.05f, 1f + v);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!Applies(req, out float v)) return null;
            return "Governo da nação: " + (additive ? (v >= 0f ? "+" : "") + v.ToStringPercent() : "x" + Mathf.Max(0.05f, 1f + v).ToStringPercent());
        }
    }

    [StaticConstructorOnStartup]
    public static class EQGovStartup
    {
        static EQGovStartup()
        {
            AddPart(StatDefOf.WorkTableWorkSpeedFactor, "workSpeed", false, true);
            AddPart(StatDefOf.ResearchSpeed, "research", false, false);
            AddPart(StatDefOf.TradePriceImprovement, "tradePrice", true, false);
        }

        private static void AddPart(StatDef stat, string ch, bool additive, bool building)
        {
            if (stat == null) return;
            if (stat.parts == null) stat.parts = new List<StatPart>();
            stat.parts.Add(new StatPart_EQGov { parentStat = stat, channel = ch, additive = additive, building = building });
        }
    }

    /// <summary>Canal "quality": chance de o item feito por um colono sair um nível acima (até obra-prima). Mestre de ofícios.</summary>
    [HarmonyPatch]
    public static class Patch_QualityUtility_CreatedByPawn_Gov
    {
        public static MethodBase TargetMethod() =>
            AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new[] { typeof(Pawn), typeof(SkillDef), typeof(bool) });

        public static void Postfix(Pawn pawn, ref QualityCategory __result)
        {
            if (pawn?.Faction == null || !pawn.Faction.IsPlayer || __result >= QualityCategory.Masterwork) return;
            float ch = EQCouncil.Channel("quality");
            if (ch > 0f && Rand.Chance(ch)) __result = (QualityCategory)((int)__result + 1);
        }
    }
}
