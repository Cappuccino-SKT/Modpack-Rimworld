using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Decretos da nação (NACAO.md › 3.6 item 5): leis com prós e contras. Efeitos por canal do conselho (EQ_Conselho.xml),
    /// legitimidade por dia, peso da Infâmia na legitimidade e um pensamento nos colonos. Vagas e custo em EQ_Legitimidade.xml.
    /// </summary>
    public class DecreeDef : Def
    {
        public List<GovEffect> effects = new List<GovEffect>();
        public float legitimacyDaily;
        /// <summary>Multiplica a perda diária de legitimidade pela Infâmia (Ministério da Verdade 0,5).</summary>
        public float infamyFactor = 1f;
        public ThoughtDef thought;
        [MustTranslate] public string pros, cons;
    }

    public static class EQDecrees
    {
        public static List<DecreeDef> All => DefDatabase<DecreeDef>.AllDefsListForReading;

        public static List<DecreeDef> Active
        {
            get
            {
                var c = WorldComponent_EQLegitimacy.Current;
                if (!EQLegitimacy.On || c == null) return new List<DecreeDef>();
                return c.decrees.Select(n => DefDatabase<DecreeDef>.GetNamedSilentFail(n)).Where(d => d != null).ToList();
            }
        }

        public static bool IsActive(DecreeDef d) => Active.Contains(d);

        public static int Slots
        {
            get
            {
                LegitimacyDef d = LegitimacyDef.Active;
                var c = WorldComponent_EQLegitimacy.Current;
                if (d == null || c == null || !EQLegitimacy.On) return 0;
                return c.value >= d.decreeSecondSlotAt ? 2 : 1;
            }
        }

        public static float CooldownLeftDays
        {
            get
            {
                LegitimacyDef d = LegitimacyDef.Active;
                var c = WorldComponent_EQLegitimacy.Current;
                if (d == null || c == null) return 0f;
                return Mathf.Max(0f, d.decreeCooldownDays - (Find.TickManager.TicksGame - c.decreeTick) / (float)GenDate.TicksPerDay);
            }
        }

        /// <summary>null = pode decretar; senão o motivo.</summary>
        public static string CannotEnact(DecreeDef dec)
        {
            var c = WorldComponent_EQLegitimacy.Current;
            if (!EQLegitimacy.On || c == null) return "Sem nação, sem decreto.";
            if (IsActive(dec)) return "Já está em vigor.";
            if (c.decrees.Count >= Slots)
                return Slots < 2 ? "Só cabe um decreto (dois com legitimidade " + LegitimacyDef.Active.decreeSecondSlotAt.ToString("0") + " ou mais). Revogue o atual primeiro." : "As duas vagas estão ocupadas.";
            float cd = CooldownLeftDays;
            if (cd > 0f) return "A última lei ainda está fresca: espere " + cd.ToString("0.#") + " dias.";
            return EQGov.VoteBlock("decretar uma lei");
        }

        public static string CannotRevoke(DecreeDef dec)
        {
            if (!IsActive(dec)) return "Não está em vigor.";
            float cd = CooldownLeftDays;
            if (cd > 0f) return "A última lei ainda está fresca: espere " + cd.ToString("0.#") + " dias.";
            return null;
        }

        public static string LastReport;

        public static string Enact(DecreeDef dec)
        {
            string why = CannotEnact(dec);
            if (why != null) return why;
            var c = WorldComponent_EQLegitimacy.Current;
            c.decrees.Add(dec.defName);
            c.decreeTick = Find.TickManager.TicksGame;
            c.Add(-LegitimacyDef.Active.decreeChangeCost, "decreto: " + dec.label);
            EQCouncil.Dirty();
            Messages.Message("Decreto: " + dec.LabelCap + ". Afixado na porta da sala do trono, com carimbo.", MessageTypeDefOf.NeutralEvent, false);
            LastReport = "decretado " + dec.defName;
            return null;
        }

        public static string Revoke(DecreeDef dec, string reason = null)
        {
            if (reason == null)
            {
                string why = CannotRevoke(dec);
                if (why != null) return why;
            }
            var c = WorldComponent_EQLegitimacy.Current;
            c.decrees.Remove(dec.defName);
            if (reason == null) c.decreeTick = Find.TickManager.TicksGame;
            EQCouncil.Dirty();
            Messages.Message("Decreto revogado: " + dec.LabelCap + (reason != null ? " (" + reason + ")" : "") + ".", MessageTypeDefOf.NeutralEvent, false);
            LastReport = "revogado " + dec.defName + (reason != null ? " (" + reason + ")" : "");
            return null;
        }

        /// <summary>Legitimidade caiu abaixo da 2ª vaga: o decreto mais novo cai.</summary>
        public static void CheckSlots()
        {
            var c = WorldComponent_EQLegitimacy.Current;
            if (c == null) return;
            while (c.decrees.Count > Slots && c.decrees.Count > 0)
            {
                DecreeDef d = DefDatabase<DecreeDef>.GetNamedSilentFail(c.decrees.Last());
                if (d == null) { c.decrees.RemoveAt(c.decrees.Count - 1); continue; }
                Revoke(d, "a legitimidade não sustenta duas leis");
            }
        }

        public static IEnumerable<KeyValuePair<string, float>> Channels()
        {
            foreach (DecreeDef d in Active)
                foreach (GovEffect fx in d.effects)
                    yield return new KeyValuePair<string, float>(fx.Key, fx.amount);
        }

        public static float LegitimacyDaily => Active.Sum(d => d.legitimacyDaily);
        public static float InfamyFactor => Active.Aggregate(1f, (f, d) => f * d.infamyFactor);
    }

    /// <summary>Pensamento de um decreto em vigor (todos os colonos livres do jogador).</summary>
    public class ThoughtWorker_EQDecree : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.Faction == null || !p.Faction.IsPlayer || !p.IsFreeColonist) return ThoughtState.Inactive;
            return EQDecrees.Active.Any(d => d.thought == def) ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive;
        }
    }
}
