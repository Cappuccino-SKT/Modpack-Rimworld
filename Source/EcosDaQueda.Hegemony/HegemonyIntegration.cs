using System;
using System.Linq;
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
            EQWarDiplomacy.BorderProvider = Border;
            // Fronteiras vivas (MUNDO.md, F1): leitura do território e da prosperidade.
            EQTerritory.ClaimsProvider = () =>
            {
                var l = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<RimWorld.Planet.PlanetTile, Faction>>();
                if (!HegemonyApi.IsAvailable || HegemonyApi.ApiVersion < 4) return l;
                foreach (HegemonyTileClaim c in HegemonyApi.GetTerritoryClaims())
                    if (c != null && !c.Contested && c.Owner != null)
                        l.Add(new System.Collections.Generic.KeyValuePair<RimWorld.Planet.PlanetTile, Faction>(c.Tile, c.Owner));
                return l;
            };
            EQTerritory.ProsperityProvider = s => HegemonyApi.TryGetSettlementState(s, out SettlementState st) && st != null ? st.prosperity : -1f;
            RegisterTerritoryYield();
            // Fronteiras vivas, F2: o multiplicador de decaimento de cada cidade passa pela nossa regra (Frontier.cs).
            MethodInfo decay = AccessTools.Method(AccessTools.TypeByName("FactionTerritories2.TerritorySolver"), "SettlementDecay");
            if (decay != null)
                new Harmony("ecosdaqueda.hegemony.frontier").Patch(decay, postfix: new HarmonyMethod(typeof(HegemonyIntegration), nameof(SettlementDecayPostfix)));
            else
                Log.Warning("[Ecos da Queda] Não achei TerritorySolver.SettlementDecay no Hegemony; a fronteira que cresce (F2) fica desligada.");
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

        /// <summary>Tiles de A que encostam em tiles de B (ou disputados entre os dois). Lido sob demanda, uma vez por dia no máximo.</summary>
        private static System.Collections.Generic.List<RimWorld.Planet.PlanetTile> Border(Faction a, Faction b)
        {
            var res = new System.Collections.Generic.List<RimWorld.Planet.PlanetTile>();
            if (!HegemonyApi.IsAvailable || HegemonyApi.ApiVersion < 4) return res;
            a = EQUtil.Canonical(a); b = EQUtil.Canonical(b);
            var ofB = new System.Collections.Generic.HashSet<RimWorld.Planet.PlanetTile>();
            var ofA = new System.Collections.Generic.List<RimWorld.Planet.PlanetTile>();
            foreach (HegemonyTileClaim c in HegemonyApi.GetTerritoryClaims())
            {
                if (c == null) continue;
                Faction o = EQUtil.Canonical(c.Owner);
                if (c.Contested && c.Parties != null && c.Parties.Any(p => EQUtil.Canonical(p) == a) && c.Parties.Any(p => EQUtil.Canonical(p) == b)) res.Add(c.Tile);
                else if (o == a) ofA.Add(c.Tile);
                else if (o == b) ofB.Add(c.Tile);
            }
            var tmp = new System.Collections.Generic.List<RimWorld.Planet.PlanetTile>();
            foreach (var t in ofA)
            {
                tmp.Clear();
                Find.WorldGrid.GetTileNeighbors(t, tmp);
                if (tmp.Any(ofB.Contains)) res.Add(t);
            }
            return res;
        }

        private static System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Faction, Faction>> Wars()
        {
            if (!HegemonyApi.IsAvailable) yield break;
            foreach (HegemonyWar w in HegemonyApi.GetWars())
                if (w?.First != null && w.Second != null)
                    yield return new System.Collections.Generic.KeyValuePair<Faction, Faction>(w.First, w.Second);
        }

        // ---------- F3: rendimento do território ----------
        private static MethodInfo foodChange, materialsAdd;
        private static Type foodKindType;

        /// <summary>Perfil de bioma (BiomeEffectDef é público) e entrega nos estoques por tipo (FoodStocks.Change e
        /// Materials.Add são internos: reflexão, como os outros métodos do Hegemony usados aqui).</summary>
        private static void RegisterTerritoryYield()
        {
            var profiles = new System.Collections.Generic.Dictionary<string, float[]>();
            foreach (BiomeEffectDef b in DefDatabase<BiomeEffectDef>.AllDefsListForReading)
                if (!string.IsNullOrEmpty(b.biome))
                    profiles[b.biome] = new[] { b.food + b.vegetables, b.wood + b.materials, b.stone + b.materials, b.metal + b.materials, b.components, b.prosperity, b.happiness };
            EQTerritoryYield.BiomeProfileProvider = biome => biome != null && profiles.TryGetValue(biome.defName, out float[] p) ? p : null;

            foodChange = AccessTools.Method("FactionTerritories2.FoodStocks:Change");
            materialsAdd = AccessTools.Method("FactionTerritories2.Materials:Add", new[] { typeof(SettlementState), typeof(MaterialKind), typeof(double) });
            foodKindType = AccessTools.TypeByName("FactionTerritories2.SettlementFoodKind");
            if (foodChange == null || materialsAdd == null || foodKindType == null)
            {
                Log.Warning("[Ecos da Queda] Não achei FoodStocks.Change/Materials.Add no Hegemony; o rendimento do território (F3) fica desligado.");
                return;
            }
            object vegetables = Enum.Parse(foodKindType, "Vegetables");
            EQTerritoryYield.Applier = (settlement, y) =>
            {
                if (!HegemonyApi.TryGetSettlementState(settlement, out SettlementState st) || st == null || st.UnderConstruction) return;
                if (y.food > 0f) foodChange.Invoke(null, new object[] { st, vegetables, (double)y.food });
                if (y.wood > 0f) materialsAdd.Invoke(null, new object[] { st, MaterialKind.Wood, (double)y.wood });
                if (y.stone > 0f) materialsAdd.Invoke(null, new object[] { st, MaterialKind.Stone, (double)y.stone });
                if (y.metal > 0f) materialsAdd.Invoke(null, new object[] { st, MaterialKind.Metal, (double)y.metal });
                if (y.components > 0f) materialsAdd.Invoke(null, new object[] { st, MaterialKind.IndustrialComponents, (double)y.components });
                if (y.prosperity > 0f) st.prosperity += y.prosperity;
                if (y.happiness != 0f) st.happiness = Math.Max(0f, Math.Min(100f, st.happiness + y.happiness));
            };
            EQTerritoryYield.StockReport = settlement =>
                HegemonyApi.TryGetSettlementState(settlement, out SettlementState st) && st != null
                    ? "comida=" + st.food.ToString("0") + " materiais=" + st.materials.ToString("0") + " prosp=" + st.prosperity.ToString("0") + " fel=" + st.happiness.ToString("0")
                    : "-";
        }

        /// <summary>F2: raio permitido da cidade (cresce com o tempo até o teto). Em construção/rebelde fica o do Hegemony.</summary>
        public static void SettlementDecayPostfix(SettlementState state, ref float __result)
        {
            if (state?.settlement == null || state.UnderConstruction || state.rebelling) return;
            WorldComponent_EQFrontier c = WorldComponent_EQFrontier.Current;
            if (c == null) return;
            try { __result = c.DecayFor(state.settlement, state.prosperity, __result); }
            catch (Exception ex) { Log.ErrorOnce("[Ecos da Queda] Fronteira (F2): " + ex, 0x4E51F2); }
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
