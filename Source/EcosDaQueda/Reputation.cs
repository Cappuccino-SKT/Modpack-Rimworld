using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>Um título de reputação (célula da matriz Fama × Infâmia) e seus efeitos.</summary>
    public class ReputationTier
    {
        public string label;
        public string description;
        /// <summary>Somado à melhora de preço no comércio com a facção (0.10 = 10% melhor para o jogador; negativo = pior).</summary>
        public float priceGain;
        /// <summary>Chance, a cada verificação, de a facção mandar um presente (só se não for hostil).</summary>
        public float giftChance;
        /// <summary>Chance de mandar um tributo em prata "para evitar problemas" (só se não for hostil).</summary>
        public float tributeChance;
        /// <summary>Chance de mandar caçadores de recompensa (só se for hostil).</summary>
        public float raidChance;
    }

    /// <summary>Configuração da reputação (1.7/Defs/EQ_Reputation.xml). Use só uma instância.</summary>
    public class ReputationSettingsDef : Def
    {
        /// <summary>Facções que têm Fama/Infâmia, na ordem da aba.</summary>
        public List<FactionDef> factions = new List<FactionDef>();
        /// <summary>Limiares de nível (0 → 1 → 2) para Fama e para Infâmia.</summary>
        public List<float> thresholds = new List<float> { 30f, 150f };
        public float maxValue = 999f;
        /// <summary>9 títulos, linha = nível de Fama (0..2), coluna = nível de Infâmia (0..2).</summary>
        public List<ReputationTier> tiers = new List<ReputationTier>();

        // Quanto se ganha por evento
        public float infamyPerHumanKilled = 4f;
        public float infamyPerOtherKilled = 1f;
        public float infamyPerCapture = 3f;
        // Efeito nas outras potências: inimigas (boa vontade <= enemyThreshold) e amigas (>= friendThreshold)
        public float enemyThreshold = -50f;
        public float friendThreshold = 50f;
        public float enemyFameFromInfamy = 0.5f;
        public float enemyInfamyFromFame = 0.25f;
        public float friendInfamyFromInfamy = 0.25f;

        // Eventos periódicos
        public float checkIntervalDays = 3f;
        public float minDaysBetweenEvents = 8f;
        public FloatRange giftMarketValue = new FloatRange(300f, 900f);
        public IntRange tributeSilver = new IntRange(150, 500);
        public float raidPointsFactor = 1f;
        public float raidMinPoints = 200f;
        public string giftLetterLabel = "Presente: {0}";
        public string giftLetterText = "{0} enviou um presente para você.";
        public string tributeLetterLabel = "Tributo: {0}";
        public string tributeLetterText = "{0} enviou prata para manter boas relações.";
        public string raidLetterText = "{0} enviou caçadores de recompensa atrás de você.";

        public static ReputationSettingsDef Settings => DefDatabase<ReputationSettingsDef>.AllDefsListForReading.FirstOrFallback();

        public int Level(float value)
        {
            int lvl = 0;
            for (int i = 0; i < thresholds.Count; i++)
                if (value >= thresholds[i]) lvl = i + 1;
            return System.Math.Min(lvl, 2);
        }

        public ReputationTier Tier(float fame, float infamy)
        {
            int idx = Level(fame) * 3 + Level(infamy);
            return tiers != null && idx < tiers.Count ? tiers[idx] : null;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string e in base.ConfigErrors())
                yield return e;
            if (tiers == null || tiers.Count != 9)
                yield return "ReputationSettingsDef precisa de exatamente 9 tiers (3 níveis de Fama × 3 de Infâmia).";
        }
    }

    public class ReputationRecord : IExposable
    {
        public Faction faction;
        public float fame;
        public float infamy;
        public int lastEventTick = -999999;

        public void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref fame, "fame", 0f);
            Scribe_Values.Look(ref infamy, "infamy", 0f);
            Scribe_Values.Look(ref lastEventTick, "lastEventTick", -999999);
        }
    }

    /// <summary>Guarda Fama e Infâmia do jogador com cada potência (estilo New Vegas) e dispara os efeitos periódicos.</summary>
    public class WorldComponent_Reputation : WorldComponent
    {
        private List<ReputationRecord> records = new List<ReputationRecord>();

        public WorldComponent_Reputation(World world) : base(world)
        {
        }

        public static WorldComponent_Reputation Current => Find.World?.GetComponent<WorldComponent_Reputation>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref records, "records", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (records == null) records = new List<ReputationRecord>();
                records.RemoveAll(r => r?.faction == null || EQUtil.IsProxy(r.faction));
            }
        }

        public ReputationRecord Get(Faction faction)
        {
            foreach (ReputationRecord r in records)
                if (r.faction == faction) return r;
            var rec = new ReputationRecord { faction = faction };
            records.Add(rec);
            return rec;
        }

        public ReputationTier TierFor(Faction faction)
        {
            faction = EQUtil.Canonical(faction);
            var s = ReputationSettingsDef.Settings;
            if (s == null || !Tracks(faction)) return null;
            ReputationRecord r = Get(faction);
            return s.Tier(r.fame, r.infamy);
        }

        public static bool Tracks(Faction f)
        {
            var s = ReputationSettingsDef.Settings;
            return f != null && !f.IsPlayer && !EQUtil.IsProxy(f) && s != null && s.factions.Contains(f.def);
        }

        /// <summary>Registra uma ação do jogador contra/a favor de uma facção e propaga às outras potências.</summary>
        public void Record(Faction target, float fameGain, float infamyGain)
        {
            target = EQUtil.Canonical(target);
            var s = ReputationSettingsDef.Settings;
            if (s == null || !Tracks(target) || (fameGain <= 0f && infamyGain <= 0f))
                return;
            Add(Get(target), fameGain, infamyGain, s.maxValue);

            foreach (Faction other in Find.FactionManager.AllFactionsListForReading)
            {
                if (other == target || !Tracks(other) || other.defeated)
                    continue;
                int gw = other.GoodwillWith(target);
                if (gw <= s.enemyThreshold)
                    Add(Get(other), infamyGain * s.enemyFameFromInfamy, fameGain * s.enemyInfamyFromFame, s.maxValue);
                else if (gw >= s.friendThreshold)
                    Add(Get(other), 0f, infamyGain * s.friendInfamyFromInfamy, s.maxValue);
            }
        }

        private static void Add(ReputationRecord r, float fame, float infamy, float max)
        {
            r.fame = System.Math.Min(max, r.fame + System.Math.Max(0f, fame));
            r.infamy = System.Math.Min(max, r.infamy + System.Math.Max(0f, infamy));
        }

        // ---------------- Eventos periódicos ----------------

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            var s = ReputationSettingsDef.Settings;
            if (s == null)
                return;
            int interval = System.Math.Max(2500, (int)(s.checkIntervalDays * GenDate.TicksPerDay));
            int now = Find.TickManager.TicksGame;
            if (now % interval != 0)
                return;
            Map map = Find.AnyPlayerHomeMap;
            if (map == null)
                return;

            foreach (Faction f in Find.FactionManager.AllFactionsListForReading)
            {
                if (!Tracks(f) || f.defeated)
                    continue;
                ReputationRecord rec = Get(f);
                if (now - rec.lastEventTick < s.minDaysBetweenEvents * GenDate.TicksPerDay)
                    continue;
                ReputationTier tier = s.Tier(rec.fame, rec.infamy);
                if (tier == null)
                    continue;
                bool hostile = f.HostileTo(Faction.OfPlayer);
                bool fired = false;
                if (!hostile && tier.giftChance > 0f && Rand.Chance(tier.giftChance))
                    fired = SendGift(f, map, s);
                else if (!hostile && tier.tributeChance > 0f && Rand.Chance(tier.tributeChance))
                    fired = SendTribute(f, map, s);
                else if (hostile && tier.raidChance > 0f && Rand.Chance(tier.raidChance))
                    fired = SendBountyHunters(f, map, s);
                if (fired)
                    rec.lastEventTick = now;
            }
        }

        private static bool SendGift(Faction f, Map map, ReputationSettingsDef s)
        {
            var parms = new ThingSetMakerParams
            {
                totalMarketValueRange = s.giftMarketValue,
                makingFaction = f,
                qualityGenerator = QualityGenerator.Reward
            };
            List<Thing> things = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
            if (things.NullOrEmpty())
                return false;
            return Drop(f, map, things, string.Format(s.giftLetterLabel, f.Name), string.Format(s.giftLetterText, f.Name), LetterDefOf.PositiveEvent);
        }

        private static bool SendTribute(Faction f, Map map, ReputationSettingsDef s)
        {
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = s.tributeSilver.RandomInRange;
            return Drop(f, map, new List<Thing> { silver }, string.Format(s.tributeLetterLabel, f.Name), string.Format(s.tributeLetterText, f.Name, silver.stackCount), LetterDefOf.PositiveEvent);
        }

        private static bool Drop(Faction f, Map map, List<Thing> things, string label, string text, LetterDef letter)
        {
            IntVec3 cell = DropCellFinder.TradeDropSpot(map);
            DropPodUtility.DropThingsNear(cell, map, things, faction: f);
            Find.LetterStack.ReceiveLetter(label, text, letter, new LookTargets(cell, map), f);
            return true;
        }

        /// <summary>Retaliação própria da potência (definida no FactionAllegianceDef), em vez de uma invasão comum.</summary>
        private static bool TrySignatureIncident(Faction f, Map map)
        {
            FactionAllegianceDef d = FactionAllegianceDef.For(f.def);
            if (d == null || d.hostileIncidents.NullOrEmpty() || !Rand.Chance(d.hostileIncidentChance))
                return false;
            foreach (IncidentDef inc in d.hostileIncidents.InRandomOrder())
            {
                if (inc?.Worker == null) continue;
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(inc.category, map);
                parms.forced = true;
                // Sem CanFireNow: ele recusa incidentes forçados (testado), mas TryExecute funciona e devolve false se falhar.
                if (!inc.Worker.TryExecute(parms)) continue;
                if (!d.hostileIncidentText.NullOrEmpty())
                    Find.LetterStack.ReceiveLetter(d.hostileIncidentLabel ?? f.Name, d.hostileIncidentText, LetterDefOf.NegativeEvent, null, f);
                return true;
            }
            return false;
        }

        private static bool SendBountyHunters(Faction f, Map map, ReputationSettingsDef s)
        {
            if (TrySignatureIncident(f, map))
                return true;
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            parms.faction = f;
            parms.forced = true;
            // Piso de pontos: em colônias muito novas o narrador dá pontos insuficientes para montar o grupo
            parms.points = System.Math.Max(parms.points * s.raidPointsFactor, s.raidMinPoints);
            parms.customLetterText = string.Format(s.raidLetterText, f.Name);
            return IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
        }
    }
}
