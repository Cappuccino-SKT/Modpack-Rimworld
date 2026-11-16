using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Xenotipos do Ecos da Queda (Defs/Xenotypes, gerados por EcosDaQueda-Dev/scripts/xenotipos.py).
    /// Padrões são hereditários (endogenes); os de elite não são (xenogenes) e o jogo sorteia um padrão
    /// por baixo via XenotypeDef.doubleXenotypeChances.
    /// </summary>
    public static class EQXenotypeUtil
    {
        /// <summary>Padrão sorteado para ocupar o lugar do Baseliner; null se não houver configuração.</summary>
        public static XenotypeDef RandomWorldBase()
        {
            WorldXenotypesDef d = DefDatabase<WorldXenotypesDef>.AllDefsListForReading.FirstOrDefault();
            if (d == null || d.baselinerReplacements.NullOrEmpty()) return null;
            return d.baselinerReplacements.TryRandomElementByWeight(x => x.chance, out XenotypeChance c) ? c.xenotype : null;
        }

        /// <summary>Remove os endogenes, mantendo cor de pele/cabelo quando o novo xeno não define as suas.</summary>
        public static void ClearEndogenesKeepingColors(Pawn pawn, XenotypeDef next)
        {
            bool nextSkin = next.genes.Any(g => g.endogeneCategory == EndogeneCategory.Melanin);
            bool nextHair = next.genes.Any(g => g.endogeneCategory == EndogeneCategory.HairColor);
            foreach (Gene g in pawn.genes.Endogenes.ToList())
            {
                if (!nextSkin && g.def.endogeneCategory == EndogeneCategory.Melanin) continue;
                if (!nextHair && g.def.endogeneCategory == EndogeneCategory.HairColor) continue;
                pawn.genes.RemoveGene(g);
            }
        }

        /// <summary>
        /// Padrão (hereditário) que melhor explica os endogenes do pawn: o xeno padrão com mais genes presentes
        /// entre os endogenes. Null se nenhum padrão do Ecos bater.
        /// </summary>
        public static XenotypeDef GuessBase(Pawn pawn)
        {
            if (pawn?.genes == null) return null;
            var endo = new HashSet<GeneDef>(pawn.genes.Endogenes.Select(g => g.def));
            XenotypeDef best = null;
            int bestCount = 0;
            foreach (XenotypeDef x in DefDatabase<XenotypeDef>.AllDefs)
            {
                if (!x.inheritable || !x.defName.StartsWith("EQ_")) continue;
                int n = x.genes.Count(endo.Contains);
                if (n == x.genes.Count && n > bestCount) { best = x; bestCount = n; }
            }
            return best;
        }
    }

    /// <summary>Quem nasce no lugar do Baseliner no mundo todo (Defs/Xenotypes/EQ_WorldXenotypes.xml).</summary>
    public class WorldXenotypesDef : Def
    {
        public List<XenotypeChance> baselinerReplacements = new List<XenotypeChance>();
        /// <summary>Xenotipos de fora que continuam sendo gerados (ex.: androides, sanguófagos de missão).</summary>
        public List<XenotypeDef> keepXenotypes = new List<XenotypeDef>();
    }

    /// <summary>
    /// O mundo só tem as nossas espécies: todo pawn que seria gerado como Baseliner ou com um xenotipo de
    /// outro mod (colonos iniciais, outras facções, sem facção) recebe um padrão nosso. Exceções: xenos da
    /// lista keepXenotypes, xeno pedido de propósito (ForcedXenotype) e xeno personalizado do jogador.
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
    public static class Patch_PawnGenerator_GenerateGenes
    {
        public static void Prefix(Pawn pawn, ref XenotypeDef xenotype, PawnGenerationRequest request)
        {
            if (pawn?.genes == null || xenotype == null) return;
            // Colonos iniciais: só os xenotipos do cenário (o menu da criação já esconde os outros).
            if (request.Context == PawnGenerationContext.PlayerStarter && EQGameStart.RestrictStarters
                && request.ForcedCustomXenotype == null && !(Prefs.DevMode && request.ForcedXenotype != null))
            {
                if (!EQGameStart.AllowedForStart(xenotype))
                {
                    XenotypeDef starter = EQGameStart.RandomStarter();
                    if (starter != null) xenotype = starter;
                }
                return;
            }
            if (xenotype.defName.StartsWith("EQ_")) return;
            if (request.ForcedXenotype != null || request.ForcedCustomXenotype != null) return;
            WorldXenotypesDef d = DefDatabase<WorldXenotypesDef>.AllDefsListForReading.FirstOrDefault();
            if (d == null || d.keepXenotypes.Contains(xenotype)) return;
            XenotypeDef repl = EQXenotypeUtil.RandomWorldBase();
            if (repl != null) xenotype = repl;
        }
    }
}
