using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Etapa 5b: Exposição perante a Forja (0–100). Sobe quando a colônia ajuda os Fora de Linha (ofertas da rede,
    /// androide desperto), cai 1,5 por dia. Em 50 a Forja avisa; em 100 reage: com a Forja hostil manda um esquadrão
    /// de recall, senão cobra em boa vontade e Infâmia; depois disso a Exposição volta a 60.
    /// </summary>
    public class WorldComponent_Exposure : WorldComponent
    {
        public const float WarnAt = 50f;
        public const float ReactAt = 100f;
        public const float DecayPerDay = 1.5f;

        public float exposure;
        private bool warned;

        public WorldComponent_Exposure(World world) : base(world) { }

        public static WorldComponent_Exposure Current => Find.World?.GetComponent<WorldComponent_Exposure>();

        public static Faction Forja => Find.FactionManager?.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamedSilentFail("EQ_Faction_Forja"));

        public void Add(float amount, string reason)
        {
            exposure = UnityEngine.Mathf.Clamp(exposure + amount, 0f, ReactAt);
            if (amount != 0 && !reason.NullOrEmpty())
                Messages.Message("Exposição perante a Forja " + amount.ToStringWithSign("0") + " (" + reason + "): " + exposure.ToString("0") + "/" + ReactAt.ToString("0"),
                    amount > 0 ? MessageTypeDefOf.CautionInput : MessageTypeDefOf.NeutralEvent);
            Check();
        }

        public override void WorldComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (now % 2500 != 777 || exposure <= 0f) return;
            exposure = System.Math.Max(0f, exposure - DecayPerDay / 24f);
            if (exposure < WarnAt - 10f) warned = false;
        }

        private void Check()
        {
            Faction forja = Forja;
            if (forja == null || forja.defeated) return;
            if (exposure >= ReactAt)
            {
                exposure = 60f;
                warned = true;
                Map map = Find.AnyPlayerHomeMap;
                if (forja.HostileTo(Faction.OfPlayer) && map != null)
                {
                    var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                    parms.faction = forja;
                    parms.forced = true;
                    parms.points = System.Math.Max(parms.points, 300f);
                    parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                    parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                    Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, parms), Find.TickManager.TicksGame + 60000 / 2));
                    Find.LetterStack.ReceiveLetter("Recall da célula", "A Irmandade da Forja juntou os rastros: as peças, os androides, os favores à rede. Um esquadrão de recall está a caminho para \"recolher o produto com defeito\" — e quem o escondeu.",
                        LetterDefOf.ThreatBig, null, forja);
                }
                else
                {
                    WorldComponent_Allegiance.ApplyOutcome(forja, 0f, 4f, -15);
                    Find.LetterStack.ReceiveLetter("Processo de garantia", "A Irmandade da Forja abriu um processo de garantia contra a colônia por cumplicidade com os Fora de Linha. Não há tribunal: há a ficha, e ela piorou.",
                        LetterDefOf.NegativeEvent, null, forja);
                }
            }
            else if (exposure >= WarnAt && !warned)
            {
                warned = true;
                Find.LetterStack.ReceiveLetter("A Forja está olhando", "Um inspetor da Irmandade da Forja passou duas vezes pela estrada da colônia esta semana, devagar. A Exposição perante a Forja chegou a " + exposure.ToString("0")
                    + ". Em " + ReactAt.ToString("0") + " a Forja age. Apagar registros com a rede ajuda; ajudar a rede piora.", LetterDefOf.NegativeEvent, null, forja);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref exposure, "exposure");
            Scribe_Values.Look(ref warned, "warned");
        }
    }

    /// <summary>Unidades da Forja abatidas (mechs e técnicos) às vezes deixam dados de diagnóstico, a moeda da rede.</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Kill_Diagnostico
    {
        public static float Chance = 0.35f;

        public static void Prefix(Pawn __instance, out (Map map, IntVec3 cell, bool drop) __state)
        {
            __state = (null, IntVec3.Invalid, false);
            try
            {
                Faction f = __instance.Faction;
                if (f == null || f.def.defName != "EQ_Faction_Forja" || __instance.Dead || __instance.MapHeld == null) return;
                bool unit = __instance.RaceProps.IsMechanoid || (__instance.kindDef?.defName?.Contains("Tecnico") ?? false);
                if (unit && Rand.Chance(Chance))
                    __state = (__instance.MapHeld, __instance.PositionHeld, true);
            }
            catch { }
        }

        public static void Postfix((Map map, IntVec3 cell, bool drop) __state)
        {
            if (!__state.drop || __state.map == null) return;
            ThingDef d = DefDatabase<ThingDef>.GetNamedSilentFail("EQ_DadosDiagnostico");
            if (d == null) return;
            Thing t = ThingMaker.MakeThing(d);
            t.stackCount = Rand.RangeInclusive(1, 2);
            GenPlace.TryPlaceThing(t, __state.cell, __state.map, ThingPlaceMode.Near);
        }
    }
}
