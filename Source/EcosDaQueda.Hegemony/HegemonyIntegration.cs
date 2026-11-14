using System;
using System.Reflection;
using FactionTerritories2;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda.HegemonyCompat
{
    /// <summary>
    /// Integração com o Hegemony (só carrega com jaeger972.factionterritories2 ativo).
    /// Inimigos permanentes (FactionRelationEntry.permanentEnemies) nunca assinam paz nem tratados.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HegemonyIntegration
    {
        private static IDisposable peaceVeto;

        private static MethodInfo overlordMethod;

        /// <summary>A quem o jogador jurou fealdade no Hegemony (FactionFealty.Overlord é interno).</summary>
        private static Faction PlayerOverlord()
        {
            FactionFealty fealty = Find.World?.GetComponent<FactionFealty>();
            if (fealty == null || overlordMethod == null)
                return null;
            return overlordMethod.Invoke(fealty, new object[] { Faction.OfPlayer }) as Faction;
        }

        static HegemonyIntegration()
        {
            try
            {
                overlordMethod = AccessTools.Method(typeof(FactionFealty), "Overlord", new[] { typeof(Faction) });
                if (overlordMethod == null)
                    Log.Warning("[Ecos da Queda] Não achei FactionFealty.Overlord no Hegemony; postos não detectarão a lealdade.");
                else
                    EQAllegiance.PlayerOverlordProvider = PlayerOverlord;

                peaceVeto = HegemonyApi.RegisterVeto(HegemonyVetoKind.AIPeaceOrTreaty, ctx =>
                    FactionRelationPresetDef.ArePermanentEnemies(ctx.Actor, ctx.Other)
                        ? ctx.Actor?.Name + " jamais fará paz com " + ctx.Other?.Name + "."
                        : null);
            }
            catch (Exception ex)
            {
                Log.Error("[Ecos da Queda] Falha ao registrar integração com o Hegemony: " + ex);
            }
        }
    }
}
