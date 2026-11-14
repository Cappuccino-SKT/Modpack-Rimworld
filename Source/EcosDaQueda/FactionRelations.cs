using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Uma relação fixa entre duas facções do mod.</summary>
    public class FactionRelationEntry
    {
        public FactionDef a;
        public FactionDef b;
        /// <summary>Boa vontade inicial (-100 a 100). &lt;= -75 vira hostil; &gt;= 75 vira aliado.</summary>
        public int goodwill;
        /// <summary>Nome da relação mostrado na aba Reputação (ex.: "ódio mortal").</summary>
        public string label;
        /// <summary>Inimigos para sempre: a hostilidade é reimposta e o Hegemony não deixa fazerem paz.</summary>
        public bool permanentEnemies;
    }

    /// <summary>Matriz de relações entre as potências (1.7/Defs/EQ_FactionRelations.xml).</summary>
    public class FactionRelationPresetDef : Def
    {
        public List<FactionRelationEntry> relations = new List<FactionRelationEntry>();

        public static FactionRelationEntry Find(FactionDef x, FactionDef y)
        {
            foreach (FactionRelationPresetDef preset in DefDatabase<FactionRelationPresetDef>.AllDefsListForReading)
                foreach (FactionRelationEntry e in preset.relations)
                    if ((e.a == x && e.b == y) || (e.a == y && e.b == x))
                        return e;
            return null;
        }

        public static bool ArePermanentEnemies(Faction x, Faction y)
        {
            if (x == null || y == null) return false;
            return Find(x.def, y.def)?.permanentEnemies ?? false;
        }
    }

    /// <summary>
    /// Aplica a matriz de relações uma vez por mundo (ao criar ou ao carregar um save antigo)
    /// e, diariamente, reimpõe a hostilidade entre inimigos permanentes.
    /// </summary>
    public class WorldComponent_FactionRelations : WorldComponent
    {
        private bool applied;
        private const int EnforceInterval = 60000; // 1 dia

        public WorldComponent_FactionRelations(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref applied, "applied", false);
        }

        public override void FinalizeInit(bool fromLoad)
        {
            base.FinalizeInit(fromLoad);
            if (applied || world?.factionManager == null)
                return;
            ForEachPair((fa, fb, e) => SetRelation(fa, fb, e.goodwill));
            applied = true;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % EnforceInterval != 0)
                return;
            ForEachPair((fa, fb, e) =>
            {
                if (e.permanentEnemies && (fa.GoodwillWith(fb) > -75 || fa.RelationKindWith(fb) != FactionRelationKind.Hostile))
                    SetRelation(fa, fb, System.Math.Min(e.goodwill, -80));
            });
        }

        private void ForEachPair(System.Action<Faction, Faction, FactionRelationEntry> action)
        {
            foreach (FactionRelationPresetDef preset in DefDatabase<FactionRelationPresetDef>.AllDefsListForReading)
            {
                foreach (FactionRelationEntry e in preset.relations)
                {
                    if (e?.a == null || e.b == null)
                        continue;
                    foreach (Faction fa in world.factionManager.AllFactionsListForReading)
                    {
                        if (fa.def != e.a || fa.defeated || EQUtil.IsProxy(fa)) continue;
                        foreach (Faction fb in world.factionManager.AllFactionsListForReading)
                        {
                            if (fb.def != e.b || fb.defeated || fb == fa || EQUtil.IsProxy(fb)) continue;
                            action(fa, fb, e);
                        }
                    }
                }
            }
        }

        private static void SetRelation(Faction fa, Faction fb, int goodwill)
        {
            goodwill = System.Math.Max(-100, System.Math.Min(100, goodwill));
            FactionRelationKind kind = goodwill <= -75 ? FactionRelationKind.Hostile
                : goodwill >= 75 ? FactionRelationKind.Ally
                : FactionRelationKind.Neutral;
            Apply(fa.RelationWith(fb, allowNull: true), goodwill, kind);
            Apply(fb.RelationWith(fa, allowNull: true), goodwill, kind);
        }

        private static void Apply(FactionRelation rel, int goodwill, FactionRelationKind kind)
        {
            if (rel == null) return;
            rel.baseGoodwill = goodwill;
            rel.kind = kind;
        }
    }
}
