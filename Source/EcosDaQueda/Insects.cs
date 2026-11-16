using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// "A colmeia sem recall" (1.7/Defs/EQ_Insects.xml): colmeia de inseto destruída pelo jogador é um recall que a
    /// fabricante não fez. A Hélice dá Fama por colmeia (e favor, se for a potência do líder, via favorSources
    /// InsectHiveDestroyed).
    /// </summary>
    public class InsectRecallDef : Def
    {
        public FactionDef faction;
        public float famePerHive = 2f;
        /// <summary>Classes de construção (nome curto) que contam além de RimWorld.Hive (ex.: tocas do VFE Insectoids 2).</summary>
        public List<string> extraThingClasses = new List<string>();
        public string messageText;

        public static InsectRecallDef Current => DefDatabase<InsectRecallDef>.AllDefsListForReading.FirstOrDefault();

        public bool Counts(Thing t)
        {
            if (t is Hive)
                return true;
            string n = t.GetType().Name;
            return extraThingClasses.Contains(n);
        }
    }

    public static class EQInsects
    {
        /// <summary>Total de colmeias destruídas pelo jogador (para teste).</summary>
        public static int Destroyed;

        public static void OnHiveKilled(Thing hive)
        {
            InsectRecallDef d = InsectRecallDef.Current;
            if (d == null || Current.ProgramState != ProgramState.Playing)
                return;
            Destroyed++;
            Faction f = EQUtil.MainFaction(d.faction);
            if (f != null && !f.defeated)
            {
                WorldComponent_Reputation.Current?.Record(f, d.famePerHive, 0f);
                if (!d.messageText.NullOrEmpty())
                    Messages.Message(d.messageText.Formatted(f.Named("FACTION"), hive.LabelShort.Named("HIVE")),
                        new LookTargets(hive.Position, hive.Map), MessageTypeDefOf.PositiveEvent, historical: false);
            }
            EQFavor.Notify(FavorEventType.InsectHiveDestroyed);
        }
    }

    /// <summary>Colmeia morta por alguém do jogador (Hive.Kill chama a base no fim).</summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.Kill))]
    public static class Patch_Thing_Kill_Hive
    {
        public static void Prefix(Thing __instance, DamageInfo? dinfo)
        {
            if (!(__instance is Building) || !__instance.Spawned || dinfo?.Instigator?.Faction == null || !dinfo.Value.Instigator.Faction.IsPlayer)
                return;
            InsectRecallDef d = InsectRecallDef.Current;
            if (d != null && d.Counts(__instance))
                EQInsects.OnHiveKilled(__instance);
        }
    }
}
