using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Etapa 2, bloco M (1.7/Defs/EQ_Vehicles.xml): veículo de combate (Vehicle Framework, categoria Combat) montado
    /// na colônia sem o cargo da Forja que licencia blindados é pirataria (Infâmia); com o cargo, é unidade registrada.
    /// </summary>
    public class VehiclePolicyDef : Def
    {
        public FactionDef licensor;
        /// <summary>Índice do cargo (0 = primeiro) que licencia blindados.</summary>
        public int licenseRank = 3;
        public int piracyGoodwill = -10;
        public string piracyText;
        public string licensedText;

        public static VehiclePolicyDef Current => DefDatabase<VehiclePolicyDef>.AllDefsListForReading.FirstOrDefault();
    }

    /// <summary>
    /// Acesso por reflexão ao Vehicle Framework (opcional; sem ele tudo aqui fica inerte).
    /// </summary>
    public static class EQVehicles
    {
        private const int CategoryCombat = 4;

        private static Type tVehicleDef, tVehiclePawn, tBuildDef;
        private static FieldInfo fThingToSpawn, fCategory, fType, fStatHandler, fComponents;
        private static MethodInfo mSpawnRandomized, mClosewalkCell, mSetHealth, mMarkAllDirty, mConsumeFuel;
        private static PropertyInfo pMaxHealth, pCompFuel, pFuelCapacity;

        /// <summary>Contadores para teste.</summary>
        public static int Built, Pirated, Licensed, Delivered;

        public static bool Active => mSpawnRandomized != null;

        public static void Apply()
        {
            tVehicleDef = AccessTools.TypeByName("Vehicles.VehicleDef");
            tVehiclePawn = AccessTools.TypeByName("Vehicles.VehiclePawn");
            tBuildDef = AccessTools.TypeByName("Vehicles.VehicleBuildDef");
            if (tVehicleDef == null || tVehiclePawn == null || tBuildDef == null)
                return;
            fThingToSpawn = AccessTools.Field(tBuildDef, "thingToSpawn");
            fCategory = AccessTools.Field(tVehicleDef, "vehicleCategory");
            fType = AccessTools.Field(tVehicleDef, "type");
            fStatHandler = AccessTools.Field(tVehiclePawn, "statHandler");
            Type tStat = AccessTools.TypeByName("Vehicles.VehicleStatHandler");
            Type tComp = AccessTools.TypeByName("Vehicles.VehicleComponent");
            Type tFuel = AccessTools.TypeByName("Vehicles.CompFueledTravel");
            fComponents = tStat == null ? null : AccessTools.Field(tStat, "components");
            mMarkAllDirty = tStat == null ? null : AccessTools.Method(tStat, "MarkAllDirty");
            mSetHealth = tComp == null ? null : AccessTools.Method(tComp, "SetHealth");
            pMaxHealth = tComp == null ? null : AccessTools.Property(tComp, "MaxHealth");
            pCompFuel = AccessTools.Property(tVehiclePawn, "CompFueledTravel");
            pFuelCapacity = tFuel == null ? null : AccessTools.Property(tFuel, "FuelCapacity");
            mConsumeFuel = tFuel == null ? null : AccessTools.Method(tFuel, "ConsumeFuel", new[] { typeof(float) });
            mSpawnRandomized = AccessTools.Method(AccessTools.TypeByName("Vehicles.VehicleSpawner"), "SpawnVehicleRandomized");
            mClosewalkCell = AccessTools.Method(AccessTools.TypeByName("Vehicles.CellFinderExtended"), "RandomClosewalkCellNear");
        }

        public static bool IsVehicleDef(ThingDef d) => d != null && tVehicleDef != null && tVehicleDef.IsInstanceOfType(d);

        public static bool IsCombat(ThingDef vehicleDef) =>
            IsVehicleDef(vehicleDef) && (Convert.ToInt32(fCategory.GetValue(vehicleDef)) & CategoryCombat) != 0;

        public static bool IsLand(ThingDef vehicleDef) =>
            IsVehicleDef(vehicleDef) && fType != null && fType.GetValue(vehicleDef)?.ToString() == "Land";

        /// <summary>Veículo que a planta (VehicleBuildDef) monta, ou null.</summary>
        public static ThingDef VehicleOfBuildDef(BuildableDef b) =>
            b != null && tBuildDef != null && tBuildDef.IsInstanceOfType(b) ? fThingToSpawn.GetValue(b) as ThingDef : null;

        /// <summary>Blindado montado na colônia. Devolve true se foi pirataria.</summary>
        public static bool OnCombatVehicleBuilt(ThingDef vehicle, GlobalTargetInfo where)
        {
            VehiclePolicyDef d = VehiclePolicyDef.Current;
            if (d == null || Current.ProgramState != ProgramState.Playing || Faction.OfPlayerSilentFail == null)
                return false;
            Faction lic = EQUtil.MainFaction(d.licensor);
            if (lic == null || lic.defeated)
                return false;
            Built++;
            if (EQAndroids.HasLicense(d.licensor, d.licenseRank))
            {
                Licensed++;
                if (!d.licensedText.NullOrEmpty())
                    Messages.Message(d.licensedText.Formatted(lic.Named("FACTION"), vehicle.label.Named("VEHICLE")),
                        new LookTargets(where), MessageTypeDefOf.NeutralEvent, historical: false);
                return false;
            }
            Pirated++;
            if (!d.piracyText.NullOrEmpty())
                Messages.Message(d.piracyText.Formatted(lic.Named("FACTION"), vehicle.label.Named("VEHICLE"),
                    EQAndroids.LicenseTitleLabel(d.licensor, d.licenseRank).Named("TITLE")),
                    new LookTargets(where), MessageTypeDefOf.NegativeEvent, historical: false);
            if (d.piracyGoodwill != 0 && lic != Faction.OfPlayer)
                Faction.OfPlayer.TryAffectGoodwillWith(lic, d.piracyGoodwill, canSendMessage: true, canSendHostilityLetter: true);
            return true;
        }

        /// <summary>
        /// Gera e põe no mapa um veículo do jogador perto de root. componentHealth = fração da vida de cada peça
        /// (sorteada por peça); fuel = fração do tanque. Devolve o veículo ou null.
        /// </summary>
        public static Pawn SpawnVehicle(ThingDef vehicleDef, IntVec3 root, Map map, Faction faction, FloatRange componentHealth, FloatRange fuel)
        {
            if (!Active || !IsVehicleDef(vehicleDef) || map == null)
                return null;
            IntVec3 cell = root;
            if (mClosewalkCell != null)
            {
                try { cell = (IntVec3)mClosewalkCell.Invoke(null, new object[] { root, map, vehicleDef, 12, null }); }
                catch (Exception e) { Log.Warning("[EcosDaQueda] Célula para veículo: " + e.InnerException?.Message); }
            }
            var v = mSpawnRandomized.Invoke(null, new object[] { vehicleDef, cell, map, faction, null, false }) as Pawn;
            if (v == null)
                return null;
            Delivered++;
            Wear(v, componentHealth, fuel);
            return v;
        }

        /// <summary>Desgaste de usado: vida das peças e combustível.</summary>
        public static void Wear(Pawn v, FloatRange componentHealth, FloatRange fuel)
        {
            try
            {
                if (componentHealth.max < 1f && fStatHandler != null && fComponents != null && mSetHealth != null && pMaxHealth != null)
                {
                    object handler = fStatHandler.GetValue(v);
                    if (handler != null && fComponents.GetValue(handler) is IList comps)
                    {
                        foreach (object c in comps)
                        {
                            float max = (float)pMaxHealth.GetValue(c);
                            mSetHealth.Invoke(c, new object[] { max * componentHealth.RandomInRange });
                        }
                        mMarkAllDirty?.Invoke(handler, null);
                    }
                }
                if (fuel.max < 1f && pCompFuel != null && pFuelCapacity != null && mConsumeFuel != null)
                {
                    object comp = pCompFuel.GetValue(v);
                    if (comp != null)
                    {
                        float cap = (float)pFuelCapacity.GetValue(comp);
                        mConsumeFuel.Invoke(comp, new object[] { cap * (1f - fuel.RandomInRange) });
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("[EcosDaQueda] Desgaste do veículo usado: " + (e.InnerException ?? e).Message);
            }
        }

        /// <summary>Vida média das peças (0–1) e fração do tanque, para teste.</summary>
        public static string Describe(Pawn v)
        {
            var parts = new List<string> { v.def.defName };
            try
            {
                object handler = fStatHandler?.GetValue(v);
                if (handler != null && fComponents.GetValue(handler) is IList comps && comps.Count > 0)
                {
                    float sum = 0f;
                    foreach (object c in comps)
                        sum += Convert.ToSingle(AccessTools.Property(c.GetType(), "Health").GetValue(c)) / (float)pMaxHealth.GetValue(c);
                    parts.Add("pecas=" + (sum / comps.Count).ToString("0.00"));
                }
                object comp = pCompFuel?.GetValue(v);
                if (comp != null)
                    parts.Add("tanque=" + Convert.ToSingle(AccessTools.Property(comp.GetType(), "FuelPercent").GetValue(comp)).ToString("0.00"));
            }
            catch { }
            return string.Join(" ", parts);
        }
    }

    /// <summary>Montagem de veículo pelo Vehicle Framework: o prefixo dele troca a planta pronta pelo veículo.</summary>
    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public static class Patch_Frame_CompleteConstruction_Vehicle
    {
        /// <summary>Antes do prefixo do Vehicle Framework, que devolve false e pula os prefixos seguintes.</summary>
        [HarmonyPriority(Priority.First)]
        public static void Prefix(Frame __instance, Pawn worker, out ThingDef __state)
        {
            __state = null;
            if (!EQVehicles.Active || worker?.Faction == null || !worker.Faction.IsPlayer)
                return;
            ThingDef v = EQVehicles.VehicleOfBuildDef(__instance.def.entityDefToBuild);
            if (EQVehicles.IsCombat(v))
                __state = v;
        }

        public static void Postfix(Pawn worker, ThingDef __state)
        {
            if (__state != null)
                EQVehicles.OnCombatVehicleBuilt(__state, worker);
        }
    }

    // ===================================================================== Título de posse (item que vira veículo)

    public class CompProperties_UseEffectVehicleTitle : CompProperties_UseEffect
    {
        /// <summary>defNames dos veículos possíveis (sem referência cruzada: o Vehicle Framework é opcional).</summary>
        public List<string> vehicles = new List<string>();
        public FloatRange componentHealth = new FloatRange(1f, 1f);
        public FloatRange fuel = new FloatRange(1f, 1f);
        /// <summary>Exige o cargo da política de veículos (blindado registrado).</summary>
        public bool requiresLicense;
        public string successText = "{0} recebeu {1}.";

        public CompProperties_UseEffectVehicleTitle()
        {
            compClass = typeof(CompUseEffect_VehicleTitle);
        }

        public IEnumerable<ThingDef> AvailableVehicles =>
            vehicles.Select(n => DefDatabase<ThingDef>.GetNamedSilentFail(n)).Where(EQVehicles.IsVehicleDef);
    }

    public class CompUseEffect_VehicleTitle : CompUseEffect
    {
        private CompProperties_UseEffectVehicleTitle Props => (CompProperties_UseEffectVehicleTitle)props;

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!EQVehicles.Active || !Props.AvailableVehicles.Any())
                return "Nenhum veículo disponível (exige o Vehicle Framework)";
            if (Props.requiresLicense)
            {
                VehiclePolicyDef d = VehiclePolicyDef.Current;
                if (d != null && !EQAndroids.HasLicense(d.licensor, d.licenseRank))
                    return "A transferência exige o cargo de " + EQAndroids.LicenseTitleLabel(d.licensor, d.licenseRank);
            }
            return base.CanBeUsedBy(p);
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            ThingDef def = Props.AvailableVehicles.RandomElementWithFallback();
            Pawn v = def == null ? null : EQVehicles.SpawnVehicle(def, usedBy.Position, usedBy.Map, Faction.OfPlayer, Props.componentHealth, Props.fuel);
            if (v == null)
            {
                Messages.Message("O veículo não pôde ser entregue.", usedBy, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            Messages.Message(string.Format(Props.successText, usedBy.LabelShort, v.LabelCap), v, MessageTypeDefOf.PositiveEvent);
        }
    }
}
