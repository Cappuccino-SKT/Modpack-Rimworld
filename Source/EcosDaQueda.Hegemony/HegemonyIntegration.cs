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
    /// - Inimigos permanentes (FactionRelationEntry.permanentEnemies) nunca assinam paz nem tratados.
    /// - A vassalagem do jogador a uma potência só acontece pela cerimônia de submissão do mod:
    ///   o juramento direto pelo Hegemony é bloqueado e a cerimônia chama FactionFealty.Submit.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HegemonyIntegration
    {
        private static IDisposable peaceVeto;

        private static MethodInfo overlordMethod;
        private static MethodInfo submitMethod;
        private static MethodInfo blockMethod;

        private static FactionFealty Fealty => Find.World?.GetComponent<FactionFealty>();

        /// <summary>A quem o jogador jurou fealdade no Hegemony (FactionFealty.Overlord é interno).</summary>
        private static Faction PlayerOverlord()
        {
            FactionFealty fealty = Fealty;
            if (fealty == null || overlordMethod == null)
                return null;
            return overlordMethod.Invoke(fealty, new object[] { Faction.OfPlayer }) as Faction;
        }

        private static string SubmitBlock(Faction overlord)
        {
            FactionFealty fealty = Fealty;
            if (fealty == null || blockMethod == null)
                return null;
            EQAllegiance.AllowSubmit = true;
            try { return blockMethod.Invoke(fealty, new object[] { Faction.OfPlayer, overlord }) as string; }
            finally { EQAllegiance.AllowSubmit = false; }
        }

        private static string Submit(Faction overlord)
        {
            FactionFealty fealty = Fealty;
            if (fealty == null || submitMethod == null)
                return "Hegemony indisponível.";
            var args = new object[] { Faction.OfPlayer, overlord, null };
            EQAllegiance.AllowSubmit = true;
            try
            {
                bool ok = (bool)submitMethod.Invoke(fealty, args);
                return ok ? null : (args[2] as string ?? "recusado");
            }
            finally { EQAllegiance.AllowSubmit = false; }
        }

        static HegemonyIntegration()
        {
            try
            {
                overlordMethod = AccessTools.Method(typeof(FactionFealty), "Overlord", new[] { typeof(Faction) });
                submitMethod = AccessTools.Method(typeof(FactionFealty), "Submit");
                blockMethod = AccessTools.Method(typeof(FactionFealty), "Block", new[] { typeof(Faction), typeof(Faction) });
                if (overlordMethod == null)
                    Log.Warning("[Ecos da Queda] Não achei FactionFealty.Overlord no Hegemony; a lealdade não será detectada.");
                else
                    EQAllegiance.PlayerOverlordProvider = PlayerOverlord;
                if (submitMethod != null)
                    EQAllegiance.SubmitHandler = Submit;
                else
                    Log.Warning("[Ecos da Queda] Não achei FactionFealty.Submit no Hegemony; a cerimônia não fará o juramento de vassalagem.");
                if (blockMethod != null)
                {
                    EQAllegiance.SubmitBlockHandler = SubmitBlock;
                    new Harmony("ecosdaqueda.hegemony").Patch(blockMethod,
                        postfix: new HarmonyMethod(typeof(HegemonyIntegration), nameof(BlockPostfix)));
                }

                RegisterForces();

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

        private const string Provider = "ecosdaqueda";
        private static IDisposable eventSub;

        /// <summary>Etapa 10d: guerras, conquista, território e aviso de guerra nova para os exércitos (Forces.cs).</summary>
        private static void RegisterForces()
        {
            EQForces.WarsProvider = Wars;
            EQForces.TerritoryProvider = f => HegemonyApi.IsAvailable ? HegemonyApi.GetTerritoryTileCount(f) : 0;
            EQForces.ConquestHandler = (settlement, attacker, id) =>
            {
                if (!HegemonyApi.IsAvailable) return "Hegemony indisponível";
                if (!HegemonyApi.AreAtWar(attacker, settlement.Faction)) return "sem guerra formal no Hegemony";
                return HegemonyApi.TryTransferSettlement(Provider, id, settlement, attacker, null, HegemonyOwnershipReason.Invasion, out string reason)
                    ? null : (reason ?? "recusado");
            };
            EQForces.DeclareWarHandler = (a, b, id) =>
            {
                if (!HegemonyApi.IsAvailable) return "Hegemony indisponível";
                return HegemonyApi.TryDeclareWar(Provider, id, a, b, WarObjective.Conquest, 1, null, 1.0, null, out string reason)
                    ? null : (reason ?? "recusado");
            };
            eventSub = HegemonyApi.SubscribeToEvents(e =>
            {
                // Callback só de leitura: enfileira e o WorldComponent processa no próximo tique.
                if (e != null && e.Kind == HegemonyEventKind.WarStarted && e.First != null && e.Second != null)
                    lock (WorldComponent_EQForces.PendingWarStarts)
                        WorldComponent_EQForces.PendingWarStarts.Add(new System.Collections.Generic.KeyValuePair<Faction, Faction>(e.First, e.Second));
                // Etapa 10e (2º bloco): invasões entre facções abrem/fecham frentes de trincheira (Fronts.cs).
                if (e != null && (e.Kind == HegemonyEventKind.InvasionArrived || e.Kind == HegemonyEventKind.InvasionResolved))
                    lock (EQFronts.PendingInvasions)
                        EQFronts.PendingInvasions.Add(new EQFronts.InvasionNote
                        {
                            arrived = e.Kind == HegemonyEventKind.InvasionArrived,
                            attacker = e.First, defender = e.Second, winner = e.Winner,
                            location = e.Location, marker = e.MarkerId, forceSize = e.ForceSize,
                        });
            });
        }

        private static System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Faction, Faction>> Wars()
        {
            if (!HegemonyApi.IsAvailable) yield break;
            foreach (HegemonyWar w in HegemonyApi.GetWars())
                if (w?.First != null && w.Second != null)
                    yield return new System.Collections.Generic.KeyValuePair<Faction, Faction>(w.First, w.Second);
        }

        /// <summary>O jogador só vira vassalo de uma potência pela cerimônia de submissão.</summary>
        public static void BlockPostfix(Faction subject, Faction overlord, ref string __result)
        {
            if (__result != null || EQAllegiance.AllowSubmit || subject == null || !subject.IsPlayer)
                return;
            if (EQAllegiance.IsPower(EQUtil.Canonical(overlord)))
                __result = "Esta potência só aceita a submissão por cerimônia (aba Reputação › Cerimônia).";
        }
    }
}
