using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Ponte para a sanidade do Vanilla Anomaly Expanded - Insanity (bloco I). Tudo por reflexão:
    /// sem o mod, Active é false e nada acontece.
    /// </summary>
    public static class EQSanity
    {
        private static bool init;
        private static NeedDef needDef;
        private static MethodInfo mGain;            // Need_Sanity.GainSanity(float, string, bool)
        private static FieldInfo fRecords;          // Need_Sanity.records

        private static void Init()
        {
            if (init) return;
            init = true;
            needDef = DefDatabase<NeedDef>.GetNamedSilentFail("VAEI_Sanity");
            Type need = AccessTools.TypeByName("VAEInsanity.Need_Sanity");
            if (need != null)
                mGain = AccessTools.Method(need, "GainSanity", new[] { typeof(float), typeof(string), typeof(bool) });
            if (need != null)
                fRecords = AccessTools.Field(need, "records");
            if (needDef != null && mGain == null)
                Log.Warning("[Ecos da Queda] Insanity ativo, mas Need_Sanity.GainSanity não foi encontrado.");
        }

        public static bool Active
        {
            get { Init(); return needDef != null && mGain != null; }
        }

        public static Need Need(Pawn p)
        {
            if (!Active || p?.needs == null) return null;
            return p.needs.TryGetNeed(needDef);
        }

        /// <summary>Nível de sanidade (0–1), ou null se o pawn não tem a necessidade.</summary>
        public static float? Level(Pawn p) => Need(p)?.CurLevel;

        /// <summary>Muda a sanidade pelo caminho do mod (registro, mensagem, loucura ao chegar a 0).</summary>
        public static bool Gain(Pawn p, float value, string reason, bool message = true)
        {
            Need n = Need(p);
            if (n == null) return false;
            mGain.Invoke(n, new object[] { value, reason, message });
            return true;
        }

        /// <summary>
        /// Perda contínua: muda o nível a cada chamada e soma num único registro com o mesmo motivo.
        /// (O SanityGainContinuously do mod só atualiza o registro, sem mexer no nível.)
        /// </summary>
        public static void GainContinuously(Pawn p, float value, string reason)
        {
            Need n = Need(p);
            if (n == null || value == 0f) return;
            object last = null;
            if (fRecords?.GetValue(n) is System.Collections.IList list && list.Count > 0)
                last = list[list.Count - 1];
            if (last != null && Traverse.Create(last).Field("reason").GetValue<string>() == reason)
            {
                float before = n.CurLevel;
                mGain.Invoke(n, new object[] { value, null, false });
                Traverse t = Traverse.Create(last);
                t.Field("effect").SetValue(t.Field("effect").GetValue<float>() + (n.CurLevel - before));
                t.Field("ticksOccured").SetValue(Find.TickManager.TicksGame);
            }
            else
                mGain.Invoke(n, new object[] { value, reason, false });
        }

        /// <summary>Lealdade atual do jogador, se ela cobra sanidade na pesquisa.</summary>
        public static FactionAllegianceDef ResearchPower
        {
            get
            {
                var alg = WorldComponent_Allegiance.Current;
                FactionAllegianceDef d = alg?.Def;
                if (d == null || (d.researchSanityDrainPerDay <= 0f && d.researchSpeedPerLostSanity <= 0f))
                    return null;
                return d;
            }
        }

        public static bool AppliesTo(Pawn p) =>
            p != null && p.Faction == Faction.OfPlayer && p.RaceProps.Humanlike && Need(p) != null;

        /// <summary>Colono com mais Intelectual (o consulente do Arquivo).</summary>
        public static Pawn BestScholar(Map map) =>
            map?.mapPawns.FreeColonistsSpawned.Where(p => Need(p) != null && !p.Dead)
                .OrderByDescending(p => p.skills?.GetSkill(SkillDefOf.Intellectual)?.Level ?? 0).FirstOrDefault();

        /// <summary>Colono com a menor sanidade, se estiver abaixo de max.</summary>
        public static Pawn MostShaken(Map map, float max = 0.95f) =>
            map?.mapPawns.FreeColonistsSpawned.Where(p => Need(p) != null && !p.Dead && Need(p).CurLevel < max)
                .OrderBy(p => Need(p).CurLevel).FirstOrDefault();

        /// <summary>Põe a parte "Método do Silêncio" na velocidade de pesquisa (só com o Insanity).</summary>
        public static void Apply()
        {
            if (!Active) return;
            StatDef stat = StatDefOf.ResearchSpeed;
            if (stat.parts == null) stat.parts = new System.Collections.Generic.List<StatPart>();
            if (stat.parts.Any(sp => sp is StatPart_EQSanityResearch)) return;
            stat.parts.Add(new StatPart_EQSanityResearch { parentStat = stat });
        }
    }

    /// <summary>
    /// Pesquisa para a potência (Silêncio): quem perdeu sanidade pesquisa mais rápido.
    /// fator = 1 + researchSpeedPerLostSanity × (1 − sanidade).
    /// </summary>
    public class StatPart_EQSanityResearch : StatPart
    {
        private static bool TryFactor(StatRequest req, out float factor, out float level, out FactionAllegianceDef d)
        {
            factor = 1f;
            level = 1f;
            d = EQSanity.ResearchPower;
            if (d == null || d.researchSpeedPerLostSanity <= 0f || !(req.Thing is Pawn p) || !EQSanity.AppliesTo(p))
                return false;
            level = EQSanity.Level(p) ?? 1f;
            factor = 1f + d.researchSpeedPerLostSanity * (1f - level);
            return factor != 1f;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (TryFactor(req, out float f, out _, out _))
                val *= f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!TryFactor(req, out float f, out float level, out FactionAllegianceDef d))
                return null;
            return d.researchSanityLabel + " (sanidade " + level.ToStringPercent() + "): x" + f.ToStringPercent();
        }
    }

    /// <summary>Pesquisar como membro do Silêncio cobra sanidade, proporcional ao que se pesquisou.</summary>
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ResearchPerformed))]
    public static class Patch_ResearchPerformed_Sanity
    {
        // Prefix: o método reescreve o próprio parâmetro amount (× 0,00825 etc.), então um postfix veria o valor já reduzido.
        public static void Prefix(float amount, Pawn researcher)
        {
            if (amount <= 0f || !EQSanity.Active) return;
            FactionAllegianceDef d = EQSanity.ResearchPower;
            if (d == null || d.researchSanityDrainPerDay <= 0f || !EQSanity.AppliesTo(researcher)) return;
            // amount = velocidade × ticks; a 100% de velocidade, um dia inteiro custa researchSanityDrainPerDay.
            EQSanity.GainContinuously(researcher, -d.researchSanityDrainPerDay * amount / GenDate.TicksPerDay, d.researchSanityLabel);
        }
    }
}
