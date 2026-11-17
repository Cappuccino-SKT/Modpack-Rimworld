using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Pacotes de genes montados só com genes de certos mods (a "Linha Exótica" da Hélice).
    /// fromMods: packageIds; mod ausente é ignorado. genesPerPack: quantos genes por pacote.
    /// Só entra gene que o jogo aceita sortear em pacote (canGenerateInGeneSet) e sem arcotita.
    /// </summary>
    public class StockGenerator_EQGenepacks : StockGenerator
    {
        public List<string> fromMods = new List<string>();
        public IntRange genesPerPack = new IntRange(1, 2);

        private List<GeneDef> poolCache;

        public List<GeneDef> Pool
        {
            get
            {
                if (poolCache == null)
                {
                    var mods = new HashSet<string>(fromMods.Select(m => m.ToLowerInvariant()));
                    poolCache = DefDatabase<GeneDef>.AllDefsListForReading
                        .Where(g => g.modContentPack != null && mods.Contains(g.modContentPack.PackageId.ToLowerInvariant())
                                    && g.canGenerateInGeneSet && g.biostatArc == 0 && g.selectionWeight > 0f)
                        .ToList();
                }
                return poolCache;
            }
        }

        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            if (!ModsConfig.BiotechActive || Pool.Count == 0)
                yield break;
            int n = countRange.RandomInRange;
            for (int i = 0; i < n; i++)
            {
                var genes = new List<GeneDef>();
                int want = genesPerPack.RandomInRange;
                for (int tries = 0; genes.Count < want && tries < 30; tries++)
                {
                    GeneDef g = Pool.RandomElementByWeight(x => x.selectionWeight);
                    if (genes.Contains(g) || genes.Any(o => o.ConflictsWith(g)))
                        continue;
                    genes.Add(g);
                }
                if (genes.Count == 0)
                    continue;
                var pack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
                pack.Initialize(genes);
                yield return pack;
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef) => thingDef == ThingDefOf.Genepack;
    }
}
