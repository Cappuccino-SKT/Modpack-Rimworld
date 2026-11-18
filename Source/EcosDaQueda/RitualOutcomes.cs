using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Etapa 4c: o que um resultado anexável de ritual "de potência" dá (fama, favor, boa vontade, itens).</summary>
    public class EQRitualOutcomeExtension : DefModExtension
    {
        public FactionDef faction;
        public float fame = 2f;
        public int favor;
        public int goodwill;
        public List<ThingDefCountClass> items = new List<ThingDefCountClass>();
    }

    /// <summary>
    /// Resultado anexável (o bônus escolhido ao criar o ritual da ideologia): uma potência fica sabendo do ritual e
    /// recompensa a colônia. Ritual excelente dá tudo; bom dá metade (arredondado para cima).
    /// </summary>
    public class RitualAttachableOutcomeEffectWorker_EQPower : RitualAttachableOutcomeEffectWorker
    {
        public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
        {
            List<RitualOutcomePossibility> chances = jobRitual.Ritual.outcomeEffect.def.outcomeChances;
            int best = chances.Max(c => c.positivityIndex);
            float scale = outcome.positivityIndex >= best ? 1f : 0.5f;
            extraOutcomeDesc = EQRitualOutcomes.Grant(def, jobRitual.Map, jobRitual.Spot, scale);
        }
    }

    public static class EQRitualOutcomes
    {
        /// <summary>Aplica o efeito (também usado pelo executor de testes). Devolve o texto para a carta do ritual.</summary>
        public static string Grant(RitualAttachableOutcomeEffectDef def, Map map, IntVec3 spot, float scale)
        {
            var ext = def.GetModExtension<EQRitualOutcomeExtension>();
            Faction f = ext?.faction == null ? null : Find.FactionManager.FirstFactionOfDef(ext.faction);
            if (f == null || f.defeated) return null;
            float fame = ext.fame * scale;
            int goodwill = UnityEngine.Mathf.CeilToInt(ext.goodwill * scale);
            WorldComponent_Allegiance.ApplyOutcome(f, fame, 0f, f.HostileTo(Faction.OfPlayer) ? 0 : goodwill);
            int favor = UnityEngine.Mathf.CeilToInt(ext.favor * scale);
            if (favor > 0) EQFavor.Award(f, favor, def.label);
            if (!ext.items.NullOrEmpty() && map != null && !f.HostileTo(Faction.OfPlayer))
            {
                var things = new List<Thing>();
                foreach (ThingDefCountClass c in ext.items)
                {
                    if (c.thingDef == null) continue;
                    Thing t = ThingMaker.MakeThing(c.thingDef, GenStuff.DefaultStuffFor(c.thingDef));
                    t.stackCount = System.Math.Max(1, UnityEngine.Mathf.CeilToInt(c.count * scale));
                    things.Add(t);
                }
                if (things.Count > 0)
                    DropPodUtility.DropThingsNear(spot.IsValid ? spot : DropCellFinder.TradeDropSpot(map), map, things, faction: f);
            }
            return def.letterInfoText.Formatted(f.Named("FACTION"), fame.ToString("0.#").Named("FAME"));
        }
    }
}
