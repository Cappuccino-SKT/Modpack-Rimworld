using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// Operações de debug compartilhadas pelo menu de debug do jogo e pelo executor de testes.
    /// Cada método devolve um texto com o resultado.
    /// </summary>
    public static class EQDebug
    {
        private static Func<Faction> originalProvider;
        private static bool providerOverridden;

        // ------------------------------------------------------------ utilidades

        public static IEnumerable<Faction> Powers
        {
            get
            {
                var s = ReputationSettingsDef.Settings;
                if (s == null) yield break;
                foreach (FactionDef d in s.factions)
                {
                    Faction f = EQUtil.MainFaction(d);
                    if (f != null) yield return f;
                }
            }
        }

        /// <summary>Aceita o defName (EQ_Faction_Helice) ou o apelido (helice, silencio, forja, coro).</summary>
        public static Faction FindFaction(string name)
        {
            if (name.NullOrEmpty()) return null;
            FactionDef def = DefDatabase<FactionDef>.GetNamedSilentFail(name)
                ?? DefDatabase<FactionDef>.AllDefsListForReading.FirstOrDefault(d => d.defName.Equals("EQ_Faction_" + name, StringComparison.OrdinalIgnoreCase));
            return EQUtil.MainFaction(def);
        }

        public static Map Map => Find.AnyPlayerHomeMap ?? Find.CurrentMap;

        private static IntVec3 HomeCell(Map map)
        {
            Pawn colonist = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            IntVec3 c = colonist?.Position ?? map.Center;
            if (!CellFinder.TryFindRandomCellNear(c, map, 6, x => x.Standable(map) && !x.Fogged(map), out IntVec3 result))
                result = c;
            map.areaManager.Home[result] = true;
            return result;
        }

        // ------------------------------------------------------------ reputação

        public static string SetReputation(Faction f, bool fame, float value)
        {
            var rec = WorldComponent_Reputation.Current.Get(f);
            if (fame) rec.fame = value; else rec.infamy = value;
            return f.Name + ": fama=" + rec.fame + " infamia=" + rec.infamy + " titulo=" + ReputationSettingsDef.Settings.Tier(rec.fame, rec.infamy)?.label;
        }

        public static string AddReputation(Faction f, float fame, float infamy)
        {
            WorldComponent_Reputation.Current.Record(f, fame, infamy);
            return DescribeReputation(f);
        }

        public static string DescribeReputation(Faction f)
        {
            var rec = WorldComponent_Reputation.Current.Get(f);
            var tier = ReputationSettingsDef.Settings.Tier(rec.fame, rec.infamy);
            return f.Name + ": fama=" + rec.fame.ToString("0.#") + " infamia=" + rec.infamy.ToString("0.#") + " titulo=" + tier?.label
                + " precos=" + (tier?.priceGain ?? 0f) + " lastEvent=" + rec.lastEventTick;
        }

        public static string Goodwill(Faction f, int delta)
        {
            bool ok = Faction.OfPlayer.TryAffectGoodwillWith(f, delta, canSendMessage: false, canSendHostilityLetter: false);
            return "goodwill " + delta + " ok=" + ok + " -> " + f.GoodwillWith(Faction.OfPlayer) + " " + f.RelationKindWith(Faction.OfPlayer) + " | " + DescribeReputation(f);
        }

        public static string SetHostile(Faction f, bool hostile)
        {
            foreach (FactionRelation rel in new[] { Faction.OfPlayer.RelationWith(f), f.RelationWith(Faction.OfPlayer) })
            {
                rel.baseGoodwill = hostile ? -100 : 0;
                rel.kind = hostile ? FactionRelationKind.Hostile : FactionRelationKind.Neutral;
            }
            return f.Name + " agora " + f.RelationKindWith(Faction.OfPlayer) + " (" + f.GoodwillWith(Faction.OfPlayer) + ")";
        }

        /// <summary>Assentamentos móveis (VEF MovingBase) no mapa-múndi: dono, posição, se está andando e o estoque.</summary>
        public static string MovingBases()
        {
            Type t = AccessTools.TypeByName("VEF.Planet.MovingBase");
            if (t == null) return "VEF.Planet.MovingBase não existe";
            var sb = new StringBuilder();
            int n = 0;
            foreach (WorldObject o in Find.WorldObjects.AllWorldObjects.Where(x => t.IsInstanceOfType(x)))
            {
                n++;
                object pather = Traverse.Create(o).Field("pather").GetValue();
                bool moving = pather != null && Traverse.Create(pather).Property("Moving").GetValue<bool>();
                string trader = (o as ITrader)?.TraderKind?.defName ?? "-";
                sb.AppendLine("  " + o.Label + " [" + o.def.defName + "] de " + o.Faction?.Name + " tile=" + o.Tile + (moving ? " ANDANDO" : " parada") + " estoque=" + trader
                    + " podeNegociar=" + ((o as ITrader)?.CanTradeNow.ToString() ?? "-"));
            }
            return "assentamentos móveis=" + n + "\n" + sb.ToString().TrimEnd();
        }

        /// <summary>Lista todas as facções do mundo: def, mod de origem, oculta, assentamentos, relação com o jogador.</summary>
        public static string ListFactions()
        {
            var sb = new StringBuilder();
            foreach (Faction f in Find.FactionManager.AllFactionsListForReading.OrderBy(x => x.Hidden).ThenByDescending(x => Find.WorldObjects.Settlements.Count(s => s.Faction == x)))
            {
                if (f.IsPlayer) continue;
                sb.AppendLine((f.Hidden ? "(oculta) " : "") + (f.temporary ? "(temporária) " : "") + f.Name + " [" + f.def.defName + " | " + (f.def.modContentPack?.PackageId ?? "?") + "]"
                    + " assentamentos=" + Find.WorldObjects.Settlements.Count(s => s.Faction == f)
                    + " tech=" + f.def.techLevel + " humana=" + f.def.humanlikeFaction
                    + " jogador=" + f.GoodwillWith(Faction.OfPlayer) + "/" + f.RelationKindWith(Faction.OfPlayer)
                    + (f.def.permanentEnemy ? " INIMIGA-PERMANENTE" : ""));
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>Descreve qualquer facção (potência ou menor): líder, assentamentos, ideologia e relações fixas.</summary>
        public static string DescribeFaction(Faction f)
        {
            var sb = new StringBuilder();
            sb.AppendLine(f.Name + " [" + f.def.defName + "] oculta=" + f.Hidden + " derrotada=" + f.defeated
                + " assentamentos=" + Find.WorldObjects.Settlements.Count(x => x.Faction == f));
            sb.AppendLine("  líder=" + (f.leader == null ? "NENHUM" : f.leader.Name?.ToStringFull + " (" + f.leader.kindDef.defName + ", " + f.leader.genes?.Xenotype?.defName + ")"));
            sb.AppendLine("  com jogador: " + f.GoodwillWith(Faction.OfPlayer) + " " + f.RelationKindWith(Faction.OfPlayer));
            Ideo ideo = f.ideos?.PrimaryIdeo;
            if (ideo != null)
            {
                sb.AppendLine("  ideologia: " + ideo.name + " | membro=" + ideo.memberName + " | adjetivo=" + ideo.adjective + " | líder=" + ideo.leaderTitleMale + "/" + ideo.leaderTitleFemale
                    + " | ícone=" + ideo.iconDef?.defName + " cor=" + ideo.colorDef?.defName + " | memes=" + string.Join(",", ideo.memes.Select(m => m.defName)));
                sb.AppendLine("  preceitos=" + ideo.PreceptsListForReading.Count + " | divindades=" + ((ideo.foundation as IdeoFoundation_Deity)?.DeitiesListForReading.Select(d => d.name).ToCommaList() ?? "-"));
                sb.AppendLine("  cargos: " + string.Join("; ", ideo.RolesListForReading.Select(r => r.Label + "=" + (r.ChosenPawns().FirstOrDefault()?.LabelShort ?? "vago"))));
                sb.AppendLine("  rituais/prédios/relíquias: " + string.Join("; ", ideo.PreceptsListForReading.Where(p => p.def.visible && (p is Precept_Ritual || p is Precept_Building || p is Precept_Relic)).Select(p => p.Label)));
                sb.AppendLine("  descrição: " + (ideo.description ?? "").Replace("\n", " ").Truncate(160));
            }
            foreach (Faction o in Find.FactionManager.AllFactionsListForReading)
            {
                if (o == f || o.IsPlayer || EQUtil.IsProxy(o)) continue;
                FactionRelationEntry e = FactionRelationPresetDef.Find(f.def, o.def);
                if (e != null)
                    sb.AppendLine("  com " + o.Name + ": " + f.GoodwillWith(o) + " " + f.RelationKindWith(o) + " (fixo: " + e.label + (e.permanentEnemies ? ", permanente" : "") + ")");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>Força um ataque da facção contra a colônia e conta quem veio (tipo e xenotipo).</summary>
        public static string Raid(Faction f, float points)
        {
            Map map = Map;
            if (!f.HostileTo(Faction.OfPlayer)) SetHostile(f, true);
            int before = map.mapPawns.AllPawnsSpawned.Count(p => p.Faction == f);
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            parms.faction = f;
            parms.forced = true;
            if (points > 0f) parms.points = points;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            bool ok;
            try { ok = IncidentDefOf.RaidEnemy.Worker.TryExecute(parms); }
            catch (Exception ex) { return "EXCEÇÃO " + ex.GetType().Name + ": " + ex.Message; }
            var cont = new Dictionary<string, int>();
            foreach (Pawn p in map.mapPawns.AllPawnsSpawned.Where(p => p.Faction == f))
            {
                string k = p.kindDef.defName + (p.genes?.Xenotype != null ? " / " + p.genes.Xenotype.defName : "") + (p.IsMutant ? " (mutante)" : "");
                cont[k] = cont.TryGetValue(k) + 1;
            }
            var sb = new StringBuilder("ataque de " + f.Name + ": executou=" + ok + " pontos=" + parms.points.ToString("0") + " chegaram=" + (map.mapPawns.AllPawnsSpawned.Count(p => p.Faction == f) - before));
            foreach (var kv in cont.OrderByDescending(kv => kv.Value)) sb.Append("\n  " + kv.Value + "x " + kv.Key);
            return sb.ToString();
        }

        public static string Retaliate(Faction f)
        {
            MethodInfo m = AccessTools.Method(typeof(WorldComponent_Reputation), "SendBountyHunters");
            object r = m.Invoke(null, new object[] { f, Map, ReputationSettingsDef.Settings });
            return "retaliacao de " + f.Name + ": executou=" + r;
        }

        public static string SendGift(Faction f, bool tribute)
        {
            MethodInfo m = AccessTools.Method(typeof(WorldComponent_Reputation), tribute ? "SendTribute" : "SendGift");
            object r = m.Invoke(null, new object[] { f, Map, ReputationSettingsDef.Settings });
            return (tribute ? "tributo" : "presente") + " de " + f.Name + ": executou=" + r;
        }

        // ------------------------------------------------------------ lealdade

        /// <summary>Força o jogador a ser membro de f (null = ninguém; usa de novo o Hegemony com restore=true).</summary>
        public static string Join(Faction f, bool restore = false)
        {
            if (!providerOverridden)
            {
                originalProvider = EQAllegiance.PlayerOverlordProvider;
                providerOverridden = true;
            }
            if (restore)
            {
                EQAllegiance.PlayerOverlordProvider = originalProvider;
                providerOverridden = false;
                return "lealdade voltou a ser lida do Hegemony";
            }
            EQAllegiance.PlayerOverlordProvider = () => f;
            return "lealdade forçada: " + (f?.Name ?? "nenhuma") + " (vale na próxima hora de jogo)";
        }

        public static string MakeDue(bool tithe)
        {
            var alg = WorldComponent_Allegiance.Current;
            if (alg?.memberOf == null) return "não é membro de nenhuma potência";
            if (tithe) { alg.lastTitheTick = -9999999; alg.pendingTitheUntil = -1; }
            else alg.lastStipendTick = -9999999;
            return (tithe ? "dízimo" : "estipêndio") + " vence na próxima hora de jogo";
        }

        // ------------------------------------------------------------ início de jogo e uso dos mods

        /// <summary>Narradores e cenários visíveis, cenário/narrador em uso, xenos dos colonos e o menu de xenotipos filtrado.</summary>
        public static string StartInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("narradores visíveis: " + string.Join(", ", DefDatabase<StorytellerDef>.AllDefs.Where(t => t.listVisible).Select(t => t.defName + " (" + t.label + ", " + t.comps.Count + " comps)")));
            sb.AppendLine("narradores ocultos: " + DefDatabase<StorytellerDef>.AllDefs.Count(t => !t.listVisible));
            bool dev = Prefs.DevMode;
            Prefs.DevMode = false;
            try
            {
                sb.AppendLine("cenários visíveis: " + string.Join(", ", ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef).Select(x => x.name)));
                sb.AppendLine("cenários ocultos: " + (DefDatabase<ScenarioDef>.DefCount - ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef).Count()));
            }
            finally { Prefs.DevMode = dev; }
            sb.AppendLine("em uso: cenário=" + Find.Scenario?.name + " narrador=" + Find.Storyteller?.def?.defName);
            sb.AppendLine("xenos permitidos no cenário: " + string.Join(", ", EQGameStart.StarterChances().Select(c => c.xenotype.defName + "=" + c.chance)));
            sb.AppendLine("colonos: " + string.Join(", ", PawnsFinder.AllMaps_FreeColonists.Select(p => p.LabelShort + "=" + (p.genes?.Xenotype?.defName ?? "-"))));
            var menu = new List<FloatMenuOption> { new FloatMenuOption("AnyNonArchite".Translate().CapitalizeFirst(), null), new FloatMenuOption("XenotypeEditor".Translate() + "...", null) };
            foreach (XenotypeDef x in DefDatabase<XenotypeDef>.AllDefs)
                menu.Add(new FloatMenuOption(x.LabelCap, null));
            int before = menu.Count;
            EQGameStart.FilterXenotypeMenu(menu, test: true);
            sb.Append("menu de xenotipos: " + before + " opções -> " + menu.Count + ": " + string.Join(" | ", menu.Select(o => o.Label)));
            var rep = WorldComponent_Reputation.Current;
            foreach (Faction f in Powers)
                sb.Append("\n  " + f.Name + ": boa vontade=" + f.PlayerGoodwill);
            return sb.ToString();
        }

        /// <summary>Auditoria: por mod, quantas coisas dele as nossas facções usam (unidades, armas, roupas,
        /// estoque de comércio) e quanto conteúdo ele tem. "todos" mostra também os que já têm uso.</summary>
        public static string ModUsage(string arg)
        {
            bool Ours(Def d) => d.modContentPack != null && d.modContentPack.PackageId.ToLowerInvariant().StartsWith("ecosdaqueda");
            var used = new Dictionary<ModContentPack, HashSet<Def>>();
            void Use(Def d)
            {
                if (d?.modContentPack == null || d.modContentPack.IsOfficialMod || Ours(d)) return;
                if (!used.TryGetValue(d.modContentPack, out var set)) used[d.modContentPack] = set = new HashSet<Def>();
                set.Add(d);
            }
            var kinds = new HashSet<PawnKindDef>();
            var traders = new HashSet<TraderKindDef>();
            foreach (FactionDef f in DefDatabase<FactionDef>.AllDefs.Where(Ours))
            {
                if (f.pawnGroupMakers != null)
                    foreach (PawnGroupMaker g in f.pawnGroupMakers)
                        foreach (PawnGenOption o in g.options.Concat(g.traders).Concat(g.carriers).Concat(g.guards))
                            if (o.kind != null) kinds.Add(o.kind);
                if (f.basicMemberKind != null) kinds.Add(f.basicMemberKind);
                if (f.fixedLeaderKinds != null) kinds.AddRange(f.fixedLeaderKinds);
                if (f.caravanTraderKinds != null) traders.AddRange(f.caravanTraderKinds);
                if (f.visitorTraderKinds != null) traders.AddRange(f.visitorTraderKinds);
                if (f.baseTraderKinds != null) traders.AddRange(f.baseTraderKinds);
                if (f.xenotypeSet != null)
                    for (int i = 0; i < f.xenotypeSet.Count; i++)
                        foreach (GeneDef g in f.xenotypeSet[i].xenotype.AllGenes) Use(g);
            }
            foreach (PawnKindDef k in DefDatabase<PawnKindDef>.AllDefs.Where(Ours)) kinds.Add(k);
            foreach (XenotypeDef x in DefDatabase<XenotypeDef>.AllDefs.Where(Ours))
                foreach (GeneDef g in x.AllGenes) Use(g);
            foreach (PawnKindDef k in kinds)
            {
                Use(k);
                Use(k.race);
                var wTags = k.weaponTags ?? new List<string>();
                var aTags = k.apparelTags ?? new List<string>();
                foreach (ThingDef t in DefDatabase<ThingDef>.AllDefs)
                {
                    if (t.IsWeapon && t.weaponTags != null && t.weaponTags.Any(wTags.Contains)) Use(t);
                    if (t.IsApparel && t.apparel?.tags != null && t.apparel.tags.Any(aTags.Contains)) Use(t);
                }
                if (k.apparelRequired != null) foreach (ThingDef t in k.apparelRequired) Use(t);
                if (k.techHediffsRequired != null) foreach (ThingDef t in k.techHediffsRequired) Use(t);
                if (k.abilities != null) foreach (AbilityDef a in k.abilities) Use(a);
            }
            foreach (TraderKindDef tk in traders)
            {
                Use(tk);
                foreach (StockGenerator g in tk.stockGenerators)
                    foreach (ThingDef t in DefDatabase<ThingDef>.AllDefs)
                    {
                        bool has;
                        try { has = g.HandlesThingDef(t); } catch { has = false; }
                        if (has && t.tradeability != Tradeability.None) Use(t);
                    }
            }
            foreach (MemeDef m in DefDatabase<MemeDef>.AllDefs)
                if (Find.FactionManager.AllFactions.Any(f => Ours(f.def) && f.ideos?.PrimaryIdeo != null && f.ideos.PrimaryIdeo.memes.Contains(m))) Use(m);
            foreach (Faction f in Find.FactionManager.AllFactions.Where(f => Ours(f.def) && f.ideos?.PrimaryIdeo != null))
                foreach (Precept p in f.ideos.PrimaryIdeo.PreceptsListForReading) Use(p.def);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("uso dos mods pelas nossas facções (unidades, armas e roupas por etiqueta, estoque de comércio, genes, memes e preceitos)");
            sb.AppendLine("mod | usados | conteúdo (itens/armas/roupas/construções/pesquisas/missões+eventos) | exemplos usados");
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(m => !m.IsOfficialMod && !m.PackageId.ToLowerInvariant().StartsWith("ecosdaqueda")))
            {
                var defs = mod.AllDefs.ToList();
                int things = defs.OfType<ThingDef>().Count(t => t.category == ThingCategory.Item && !t.IsWeapon && !t.IsApparel);
                int weapons = defs.OfType<ThingDef>().Count(t => t.IsWeapon);
                int apparel = defs.OfType<ThingDef>().Count(t => t.IsApparel);
                int buildings = defs.OfType<ThingDef>().Count(t => t.category == ThingCategory.Building);
                int research = defs.OfType<ResearchProjectDef>().Count();
                int events = defs.OfType<QuestScriptDef>().Count() + defs.OfType<IncidentDef>().Count();
                int content = things + weapons + apparel + buildings + research + events + defs.OfType<GeneDef>().Count() + defs.OfType<PawnKindDef>().Count();
                used.TryGetValue(mod, out var u);
                int n = u?.Count ?? 0;
                if (content == 0 && n == 0) continue;
                if (arg != "todos" && n > 0 && arg != mod.PackageId.ToLowerInvariant()) continue;
                sb.AppendLine(mod.Name + " | " + n + " | " + things + "/" + weapons + "/" + apparel + "/" + buildings + "/" + research + "/" + events
                    + " | " + (u == null ? "-" : string.Join(", ", u.Take(arg == mod.PackageId.ToLowerInvariant() ? 400 : 6).Select(d => d.defName))));
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>Armas (ou "roupas") de todos os mods com as etiquetas que as unidades usam para sortear equipamento.</summary>
        public static string WeaponTags(string what)
        {
            var sb = new System.Text.StringBuilder();
            bool apparel = what == "roupas";
            foreach (ThingDef t in DefDatabase<ThingDef>.AllDefs.Where(d => apparel ? d.IsApparel : d.IsWeapon)
                .OrderBy(d => d.modContentPack?.Name ?? "?").ThenBy(d => d.defName))
            {
                List<string> tags = apparel ? t.apparel?.tags : t.weaponTags;
                if (tags.NullOrEmpty() && !t.destroyOnDrop == false) continue;
                sb.AppendLine((t.modContentPack?.Name ?? "?") + " | " + t.defName + " | " + t.label + " | " + t.techLevel + " | $" + t.BaseMarketValue.ToString("0")
                    + " | " + (t.IsRangedWeapon ? "tiro" : apparel ? "roupa" : "corpo") + " | " + (tags == null ? "-" : string.Join(",", tags)));
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>Gera n unidades de um PawnKind (ou de todas as nossas de uma facção, "fac:nome") e resume o
        /// equipamento: armas sorteadas, quantos vieram sem arma, munição de reserva (Combat Extended) e roupas.</summary>
        public static string Kit(string arg, int n)
        {
            var kinds = new List<PawnKindDef>();
            if (arg != null && arg.StartsWith("fac:"))
            {
                Faction f = FindFaction(arg.Substring(4));
                if (f == null) return "facção não encontrada: " + arg;
                kinds.AddRange(DefDatabase<PawnKindDef>.AllDefs.Where(k => k.defaultFactionDef == f.def && k.defName.StartsWith("EQ_")));
            }
            else
            {
                PawnKindDef k = arg == null ? null : DefDatabase<PawnKindDef>.GetNamedSilentFail(arg);
                if (k == null) return "PawnKind não encontrado: " + arg;
                kinds.Add(k);
            }
            var sb = new StringBuilder();
            int problemas = 0;
            foreach (PawnKindDef kind in kinds)
            {
                Faction fac = kind.defaultFactionDef != null ? Find.FactionManager.FirstFactionOfDef(kind.defaultFactionDef) : null;
                var armas = new Dictionary<string, int>();
                var roupas = new Dictionary<string, int>();
                int semArma = 0, semMunicao = 0, deTiro = 0, municao = 0, foraDoArsenal = 0;
                for (int i = 0; i < n; i++)
                {
                    Pawn p;
                    try { p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, fac, forceGenerateNewPawn: true)); }
                    catch (Exception ex) { problemas++; sb.AppendLine("  ERRO gerando " + kind.defName + ": " + ex.Message); continue; }
                    ThingWithComps w = p.equipment?.Primary;
                    if (w == null) semArma++;
                    else
                    {
                        armas[w.def.defName] = armas.TryGetValue(w.def.defName, out int c) ? c + 1 : 1;
                        if (kind.weaponTags != null && (w.def.weaponTags == null || !w.def.weaponTags.Any(kind.weaponTags.Contains))) foraDoArsenal++;
                        if (w.def.IsRangedWeapon)
                        {
                            deTiro++;
                            int a = p.inventory == null ? 0 : p.inventory.innerContainer.Where(t => t.def.defName.StartsWith("Ammo_") || t.def.thingClass.Name.Contains("Ammo")).Sum(t => t.stackCount);
                            municao += a;
                            if (a == 0) semMunicao++;
                        }
                    }
                    if (p.apparel != null)
                        foreach (Apparel ap in p.apparel.WornApparel)
                            roupas[ap.def.defName] = roupas.TryGetValue(ap.def.defName, out int c2) ? c2 + 1 : 1;
                    p.Destroy();
                }
                bool querArma = !kind.weaponTags.NullOrEmpty();
                if (querArma && semArma > n / 2) problemas++;
                sb.AppendLine("  " + kind.defName + " (" + kind.label + "): sem arma " + semArma + "/" + n + (querArma && semArma > n / 2 ? " !!" : "")
                    + " | de tiro " + deTiro + ", sem munição de reserva " + semMunicao + ", média " + (deTiro == 0 ? 0 : municao / deTiro)
                    + (foraDoArsenal > 0 ? " | fora das etiquetas: " + foraDoArsenal : ""));
                sb.AppendLine("    armas: " + string.Join(", ", armas.OrderByDescending(x => x.Value).Select(x => x.Key + "×" + x.Value)));
                sb.AppendLine("    roupas: " + string.Join(", ", roupas.OrderByDescending(x => x.Value).Take(10).Select(x => x.Key + "×" + x.Value)));
            }
            sb.Append(problemas == 0 ? "  OK" : "  PROBLEMAS: " + problemas);
            return sb.ToString();
        }

        /// <summary>Itens comerciáveis dos mods, agrupados por mod, categoria e etiquetas de comércio (para montar estoques).</summary>
        public static string TradeInfo()
        {
            var sb = new System.Text.StringBuilder();
            var groups = DefDatabase<ThingDef>.AllDefs
                .Where(t => t.category == ThingCategory.Item && t.tradeability != Tradeability.None && !t.IsWeapon && !t.IsApparel && t.modContentPack != null && !t.IsCorpse)
                .GroupBy(t => (t.modContentPack.IsOfficialMod ? "(jogo)" : t.modContentPack.Name) + " | " + (t.thingCategories.NullOrEmpty() ? "-" : t.thingCategories[0].defName)
                    + " | " + (t.tradeTags.NullOrEmpty() ? "-" : string.Join(",", t.tradeTags)))
                .OrderBy(g => g.Key);
            foreach (var g in groups)
                sb.AppendLine(g.Key + " | " + g.Count() + " | " + string.Join(", ", g.Take(5).Select(t => t.defName)));
            return sb.ToString().TrimEnd();
        }

        /// <summary>Gera o estoque de um comerciante (ou de todos os nossos, sem argumento) e resume por tipo.</summary>
        public static string TraderStock(string arg)
        {
            var kinds = arg == null
                ? DefDatabase<TraderKindDef>.AllDefs.Where(t => t.defName.StartsWith("EQ_")).ToList()
                : new List<TraderKindDef> { DefDatabase<TraderKindDef>.GetNamedSilentFail(arg) };
            if (kinds.Any(k => k == null)) return "TraderKindDef não encontrado: " + arg;
            var sb = new StringBuilder();
            int problemas = 0;
            foreach (TraderKindDef k in kinds)
            {
                List<Thing> stock;
                try
                {
                    ThingSetMakerParams parms = default(ThingSetMakerParams);
                    parms.traderDef = k;
                    // No jogo todo comerciante tem facção (livros, por exemplo, usam a facção para gerar o autor).
                    parms.makingFaction = Find.FactionManager.AllFactions.FirstOrDefault(f => !f.IsPlayer && !f.def.hidden && f.def.humanlikeFaction
                        && ((f.def.baseTraderKinds?.Contains(k) ?? false) || (f.def.caravanTraderKinds?.Contains(k) ?? false) || (f.def.visitorTraderKinds?.Contains(k) ?? false)))
                        ?? Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false);
                    parms.tile = Find.AnyPlayerHomeMap?.Tile ?? PlanetTile.Invalid;
                    stock = ThingSetMakerDefOf.TraderStock.root.Generate(parms);
                }
                catch (Exception ex) { problemas++; sb.AppendLine("  ERRO em " + k.defName + ": " + ex.Message); continue; }
                int armas = stock.Count(t => t.def.IsWeapon), roupas = stock.Count(t => t.def.IsApparel);
                int municao = stock.Where(t => t.def.defName.StartsWith("Ammo_")).Sum(t => t.stackCount);
                sb.AppendLine("  " + k.defName + " (" + k.label + "): " + k.stockGenerators.Count + " geradores, " + stock.Count + " itens, valor "
                    + stock.Sum(t => t.MarketValue * t.stackCount).ToString("0") + " | armas " + armas + ", roupas " + roupas + ", munição " + municao);
                sb.AppendLine("    armas/roupas: " + string.Join(", ", stock.Where(t => t.def.IsWeapon || t.def.IsApparel).Select(t => t.def.defName).Distinct().Take(14)));
                sb.AppendLine("    outros: " + string.Join(", ", stock.Where(t => !t.def.IsWeapon && !t.def.IsApparel && !t.def.defName.StartsWith("Ammo_"))
                    .GroupBy(t => t.def.defName).OrderByDescending(g => g.Sum(t => t.MarketValue * t.stackCount)).Take(16).Select(g => g.Key + "×" + g.Sum(t => t.stackCount))));
                if (EcosDaQueda.EQTrade.Dropped.TryGetValue(k, out List<string> dropped)) sb.AppendLine("    tirados da cópia: " + string.Join(", ", dropped));
                if (stock.Count == 0) problemas++;
                foreach (Thing t in stock) t.Destroy();
            }
            sb.Append(problemas == 0 ? "  OK" : "  PROBLEMAS: " + problemas);
            return sb.ToString();
        }

        public static string DescribeAllegiance()
        {
            var alg = WorldComponent_Allegiance.Current;
            if (alg == null) return "lealdade: (sem componente)";
            Pawn ideoLeader = EQLeader.IdeoLeader;
            string s = "lealdade: " + (alg.memberOf?.Name ?? "nenhuma") + (alg.pendingPower != null ? " | cerimônia pendente com " + alg.pendingPower.Name : "");
            if (alg.memberOf != null)
                s += " cargo=" + alg.RankIndex + " (" + alg.RankLabel + ") " + alg.Def.FavorLabel + "=" + alg.Favor + " hegLinked=" + alg.hegLinked
                    + " lastStipend=" + alg.lastStipendTick + " lastTithe=" + alg.lastTitheTick + " pendingTithe=" + alg.pendingTitheUntil;
            s += "\nlíder=" + (alg.leader?.LabelShort ?? "nenhum") + (alg.leader != null && alg.leader.Dead ? " (MORTO)" : "")
                + " líderDaIdeologia=" + (ideoLeader?.LabelShort ?? "nenhum") + " sucessor=" + (alg.successor?.LabelShort ?? "nenhum")
                + " precisaEscolher=" + alg.needsLeaderChoice + " cache=" + (alg.cachedTitle?.defName ?? "-") + "/" + alg.cachedFavor;
            Pawn l = alg.leader ?? ideoLeader;
            if (l != null)
                s += " | nívelPsíquico=" + l.GetPsylinkLevel() + " vínculoMecanoide=" + MechanitorUtility.IsMechanitor(l);
            if (l?.royalty != null)
                foreach (Faction f in Powers)
                    if (l.royalty.GetFavor(f) != 0 || l.royalty.GetCurrentTitle(f) != null)
                        s += "\n  " + l.LabelShort + " com " + f.Name + ": título=" + (l.royalty.GetCurrentTitle(f)?.defName ?? "-") + " favor=" + l.royalty.GetFavor(f);
            return s;
        }

        // ------------------------------------------------------------ cerimônia, líder e favor

        public static string Ceremony(Faction f)
        {
            string block = EQCeremony.BlockReason(f);
            if (block != null) return "BLOQUEADO: " + block;
            string err = EQCeremony.Request(f);
            return err != null ? "FALHOU: " + err : "cerimônia solicitada com " + f.Name + "\n" + Quests();
        }

        public static string CeremonyDiag(Faction f)
        {
            var sb = new StringBuilder();
            Pawn l = EQLeader.IdeoLeader;
            FactionAllegianceDef d = FactionAllegianceDef.For(f.def);
            sb.AppendLine("líder=" + l + " royalty=" + (l?.royalty != null) + " títulos da facção=" + f.def.RoyalTitlesAwardableInSeniorityOrderForReading.Count + " tags=" + string.Join(",", f.def.royalTitleTags ?? new List<string>()));
            EQFavor.Run(() => l.royalty.GainFavor(f, d.FirstTitle.favorCost));
            sb.AppendLine("favor=" + l.royalty.GetFavor(f) + " podeAtualizar=" + l.royalty.CanUpdateTitle(f) + " deveTerMissão=" + RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(l, f)
                + " missãoAtual=" + (RoyalTitleUtility.GetCurrentBestowingCeremonyQuest(l, f) != null));
            QuestScriptDef script = DefDatabase<QuestScriptDef>.GetNamedSilentFail("EQ_Ceremony");
            sb.AppendLine("script=" + script + " root=" + script?.root?.GetType().Name);
            var slate = new RimWorld.QuestGen.Slate();
            slate.Set("titleHolder", l);
            slate.Set("bestowingFaction", f);
            try { sb.AppendLine("TestRun=" + script.root.TestRun(slate.DeepCopy()) + " CanRun=" + script.CanRun(slate, l.MapHeld)); }
            catch (Exception ex) { sb.AppendLine("EXCEÇÃO TestRun: " + ex); }
            try
            {
                Quest q = QuestUtility.GenerateQuestAndMakeAvailable(script, slate);
                sb.AppendLine("gerou: " + q?.name + " partes=" + q?.PartsListForReading.Count + " estado=" + q?.State);
            }
            catch (Exception ex) { sb.AppendLine("EXCEÇÃO ao gerar: " + ex); }
            return sb.ToString().TrimEnd();
        }

        public static string Quests()
        {
            var sb = new StringBuilder();
            foreach (Quest q in Find.QuestManager.QuestsListForReading)
                if (!q.Historical || q.root?.defName == "EQ_Ceremony")
                    sb.AppendLine("  [" + q.id + "] " + q.name + " (" + q.root?.defName + ") estado=" + q.State);
            return sb.Length == 0 ? "  (nenhuma missão)" : sb.ToString().TrimEnd();
        }

        public static string QuestText(string part)
        {
            Quest q = Find.QuestManager.QuestsListForReading.LastOrDefault(x => x.name.IndexOf(part ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            return q == null ? "missão não encontrada: " + part : q.name + "\n" + q.description;
        }

        public static string AcceptQuest(string part)
        {
            Quest q = Find.QuestManager.QuestsListForReading.LastOrDefault(x => x.State == QuestState.NotYetAccepted && x.name.IndexOf(part ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            if (q == null) return "missão não encontrada (não aceita): " + part;
            foreach (QuestPart p in q.PartsListForReading)
                if (p is QuestPart_RequirementsToAccept r)
                {
                    AcceptanceReport rep = r.CanAccept();
                    if (!rep.Accepted) return "não pode aceitar: " + rep.Reason;
                }
            q.Accept(null);
            return "aceitou \"" + q.name + "\" estado=" + q.State;
        }

        private static Verse.AI.Group.Lord CeremonyLord()
        {
            foreach (Map map in Find.Maps)
                foreach (Verse.AI.Group.Lord lord in map.lordManager.lords)
                    if (lord.LordJob is LordJob_BestowingCeremony)
                        return lord;
            return null;
        }

        public static bool CeremonyReady() => CeremonyLord()?.CurLordToil is LordToil_BestowingCeremony_Wait;

        public static string CeremonyStatus()
        {
            Verse.AI.Group.Lord lord = CeremonyLord();
            if (lord == null) return "sem cerimônia no mapa";
            var job = (LordJob_BestowingCeremony)lord.LordJob;
            return "cerimônia: etapa=" + lord.CurLordToil?.GetType().Name + " emissário=" + job.bestower?.LabelShort + " (" + job.bestower?.kindDef?.defName + ", no mapa=" + (job.bestower?.Spawned ?? false)
                + ") alvo=" + job.target?.LabelShort + " peões=" + lord.ownedPawns.Count;
        }

        /// <summary>Começa o ritual (o que o jogador faz clicando no emissário), com todos os colonos assistindo.</summary>
        public static string CeremonyStart()
        {
            Verse.AI.Group.Lord lord = CeremonyLord();
            if (lord == null) return "sem cerimônia no mapa";
            if (!(lord.CurLordToil is LordToil_BestowingCeremony_Wait wait))
                return "o emissário ainda não está pronto: " + CeremonyStatus();
            var job = (LordJob_BestowingCeremony)lord.LordJob;
            List<Pawn> pawns = job.target.Map.mapPawns.FreeColonistsSpawned.Where(p => !p.Downed).ToList();
            if (!pawns.Contains(job.target)) pawns.Add(job.target);
            AccessTools.Method(typeof(LordToil_BestowingCeremony_Wait), "StartRitual").Invoke(wait, new object[] { pawns });
            return "ritual iniciado com " + pawns.Count + " participantes: " + CeremonyStatus();
        }

        public static string Favor(int amount)
        {
            var alg = WorldComponent_Allegiance.Current;
            if (alg?.memberOf == null) return "não é membro de nenhuma potência";
            bool ok = EQFavor.Award(alg.memberOf, amount, "debug");
            return "favor " + amount + " ok=" + ok + "\n" + DescribeAllegiance() + "\n" + Quests();
        }

        public static string FavorEvent(string type, int count)
        {
            if (!Enum.TryParse(type, true, out FavorEventType t)) return "evento desconhecido: " + type;
            EQFavor.Notify(t, count);
            return "evento " + t + " x" + count + "\n" + DescribeAllegiance();
        }

        public static string Leader(string action, string arg)
        {
            var alg = WorldComponent_Allegiance.Current;
            List<Pawn> cands = EQLeader.Candidates();
            switch (action)
            {
                case null:
                case "show":
                    return DescribeAllegiance() + "\n  candidatos: " + string.Join(", ", cands.Select((p, i) => i + "=" + p.LabelShort));
                case "set":
                {
                    int i = int.Parse(arg ?? "0");
                    if (i < 0 || i >= cands.Count) return "candidato inválido (há " + cands.Count + ")";
                    bool ok = EQLeader.Assign(cands[i]);
                    alg.LeaderTick();
                    return "líder := " + cands[i].LabelShort + " ok=" + ok + "\n" + DescribeAllegiance();
                }
                case "other":
                {
                    Pawn p = cands.FirstOrDefault(x => x != EQLeader.IdeoLeader);
                    if (p == null) return "sem outro candidato";
                    bool ok = EQLeader.Assign(p);
                    alg.LeaderTick();
                    return "líder := " + p.LabelShort + " ok=" + ok + "\n" + DescribeAllegiance();
                }
                case "successor":
                {
                    Pawn p = cands.FirstOrDefault(x => x != EQLeader.IdeoLeader);
                    if (p == null) return "sem candidato a sucessor";
                    alg.successor = p;
                    return "sucessor := " + p.LabelShort;
                }
                case "kill":
                {
                    Pawn p = EQLeader.IdeoLeader;
                    if (p == null) return "sem líder para matar";
                    p.Kill(null);
                    alg.LeaderTick();
                    return "matou o líder " + p.LabelShort + "\n" + DescribeAllegiance();
                }
                case "unassign":
                {
                    Pawn p = EQLeader.IdeoLeader;
                    if (p == null) return "sem líder";
                    EQLeader.LeaderRole.Unassign(p, false);
                    alg.LeaderTick();
                    return "tirou o cargo de líder de " + p.LabelShort + "\n" + DescribeAllegiance();
                }
                default:
                    return "ação desconhecida: " + action;
            }
        }

        public static string Renounce()
        {
            string r = WorldComponent_Allegiance.Current?.Renounce();
            return (r ?? "rompeu o juramento") + "\n" + DescribeAllegiance();
        }

        public static string Services()
        {
            var alg = WorldComponent_Allegiance.Current;
            var svc = WorldComponent_Services.Current;
            if (alg?.memberOf == null) return "sem serviços: não é membro";
            var sb = new StringBuilder();
            foreach (FactionServiceDef s in WorldComponent_Services.ServicesOf(alg.memberOf.def))
                sb.AppendLine("  " + s.defName + " [" + s.label + "] prata=" + s.silver + " -> " + (svc.BlockReason(s, Map) ?? "DISPONÍVEL"));
            return sb.ToString().TrimEnd();
        }

        public static string UseService(string defName)
        {
            FactionServiceDef s = DefDatabase<FactionServiceDef>.GetNamedSilentFail(defName);
            if (s == null) return "serviço não existe: " + defName;
            var svc = WorldComponent_Services.Current;
            string block = svc.BlockReason(s, Map);
            if (block != null) return "BLOQUEADO: " + block;
            svc.Use(s, Map);
            return "usado. recarga=" + svc.CooldownTicksLeft(s) + " ticks";
        }

        // ------------------------------------------------------------ mundo / mapa

        public static string Give(string thingDefName, int count)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(thingDefName);
            if (def == null) return "ThingDef não existe: " + thingDefName;
            Map map = Map;
            int left = count;
            while (left > 0)
            {
                Thing t = ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def));
                t.stackCount = Math.Min(left, def.stackLimit);
                left -= t.stackCount;
                IntVec3 c = HomeCell(map);
                GenPlace.TryPlaceThing(t, c, map, ThingPlaceMode.Near, out Thing placed);
                if (placed != null)
                {
                    placed.SetForbidden(false, false);
                    map.areaManager.Home[placed.Position] = true;
                }
            }
            return "deu " + count + "x " + def.defName + " (total no mapa: " + EQItems.CountOnMap(map, def) + ")";
        }

        public static string SpawnPrisoner()
        {
            Map map = Map;
            Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false) ?? Powers.FirstOrDefault());
            GenSpawn.Spawn(p, HomeCell(map), map);
            p.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
            p.health.AddHediff(HediffDefOf.Anesthetic); // sem cela ele fugiria do mapa
            return "prisioneiro criado: " + p.LabelShort + " (prisioneiros: " + map.mapPawns.PrisonersOfColonySpawned.Count + ")";
        }

        /// <summary>Cria um membro da facção e o mata com um colono como autor (testa Infâmia por morte).</summary>
        public static string KillMember(Faction f)
        {
            Map map = Map;
            Pawn killer = map.mapPawns.FreeColonistsSpawned.FirstOrDefault();
            Pawn victim = PawnGenerator.GeneratePawn(f.def.basicMemberKind, f);
            GenSpawn.Spawn(victim, HomeCell(map), map);
            string before = DescribeReputation(f);
            victim.Kill(new DamageInfo(DamageDefOf.Bullet, 9999f, 999f, -1f, killer));
            return "matou " + victim.LabelShort + " (" + victim.kindDef.defName + ") autor=" + killer?.LabelShort + "\n  antes:  " + before + "\n  depois: " + DescribeReputation(f);
        }

        public static string UseSerum(int times)
        {
            Map map = Map;
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail("EQ_Soro");
            if (def == null) return "EQ_Soro não existe";
            var sb = new StringBuilder();
            Pawn pawn = map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => p.genes != null);
            if (pawn == null) return "sem colono com genes";
            for (int i = 0; i < times; i++)
            {
                Thing serum = ThingMaker.MakeThing(def);
                GenPlace.TryPlaceThing(serum, pawn.Position, map, ThingPlaceMode.Near);
                string before = pawn.genes.Xenotype?.defName;
                foreach (CompUseEffect comp in ((ThingWithComps)serum).GetComps<CompUseEffect>().OrderByDescending(c => c.OrderPriority).ToList())
                    comp.DoEffect(pawn);
                sb.AppendLine("  " + pawn.LabelShort + ": " + before + " -> " + pawn.genes.Xenotype?.defName + " (xenogenes=" + pawn.genes.Xenogenes.Count + ", endogenes=" + pawn.genes.Endogenes.Count + ", padrão=" + (EcosDaQueda.EQXenotypeUtil.GuessBase(pawn)?.defName ?? "nenhum") + ", soro destruído=" + serum.Destroyed + ")");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Compatibilidade com o Combat Extended: lista, por mod, armas de fogo sem verbo CE, armas/itens com
        /// golpes (tools) que não são ToolCE, roupas sem Bulk/WornBulk e torres de veículo sem projétil CE.
        /// Usa os mesmos critérios dos autopatchers do CE. filtro: "armas", "melee", "roupas", "veiculos" ou vazio.
        /// </summary>
        public static string CECheck(string filtro)
        {
            if (AccessTools.TypeByName("CombatExtended.VerbPropertiesCE") == null) return "Combat Extended não está ativo";
            bool Want(string k) => filtro.NullOrEmpty() || filtro == k;
            string Mod(Def d) => d.modContentPack == null ? "(sem mod)" : d.modContentPack.Name + " [" + d.modContentPack.PackageId + "]";
            bool IsCE(object o) => o != null && o.GetType().FullName.StartsWith("CombatExtended");
            var groups = new SortedDictionary<string, List<string>>();
            void Add(string cat, Def d, string info)
            {
                string key = cat + " | " + Mod(d);
                if (!groups.TryGetValue(key, out var l)) groups[key] = l = new List<string>();
                l.Add(d.defName + " (" + d.label + ")" + (info.NullOrEmpty() ? "" : " " + info));
            }
            int total = 0, okGuns = 0, okMelee = 0, okApparel = 0;
            foreach (ThingDef d in DefDatabase<ThingDef>.AllDefs)
            {
                if (d.IsRangedWeapon && !d.Verbs.NullOrEmpty())
                {
                    bool patched = (d.weaponTags != null && d.weaponTags.Contains("Patched")) || d.Verbs.Any(v => IsCE(v));
                    if (patched) okGuns++;
                    else if (Want("armas"))
                    {
                        VerbProperties v = d.Verbs[0];
                        string kind = d.weaponTags != null && d.weaponTags.Contains("TurretGun") ? "torre" : (d.destroyOnDrop ? "interna" : "arma");
                        Add(d.modContentPack != null && d.modContentPack.IsOfficialMod ? "ARMA sem verbo CE (jogo/DLC: o próprio CE deixa assim)" : "ARMA DE FOGO", d, "[" + kind + "; verbo=" + v.verbClass?.Name + "; projétil=" + v.defaultProjectile?.defName + "/" + v.defaultProjectile?.thingClass?.Name
                            + "; tech=" + d.techLevel + "; alcance=" + v.range + "; rajada=" + v.burstShotCount + "; tags=" + (d.weaponTags == null ? "" : string.Join(",", d.weaponTags)) + "]");
                        total++;
                    }
                }
                if (!d.tools.NullOrEmpty() && (d.IsWeapon || d.IsApparel || d.equipmentType != EquipmentType.None))
                {
                    if (d.tools.All(t => IsCE(t))) okMelee++;
                    else if (Want("melee"))
                    {
                        Add(d.IsRangedWeapon ? "CORONHADA (arma de fogo)" : "CORPO A CORPO", d, "[tech=" + d.techLevel + "; golpes=" + string.Join(", ", d.tools.Select(t => t.label + ":" + t.power + "/" + t.cooldownTime + "s")) + "]");
                        total++;
                    }
                }
                if (d.IsApparel)
                {
                    bool hasBulk = d.statBases != null && d.statBases.Any(m => m.stat.defName == "Bulk" || m.stat.defName == "WornBulk");
                    if (hasBulk) okApparel++;
                    else if (Want("roupas"))
                    {
                        float sharp = d.statBases?.FirstOrDefault(m => m.stat == StatDefOf.ArmorRating_Sharp)?.value ?? 0f;
                        float blunt = d.statBases?.FirstOrDefault(m => m.stat == StatDefOf.ArmorRating_Blunt)?.value ?? 0f;
                        float stuffMult = d.statBases?.FirstOrDefault(m => m.stat == StatDefOf.StuffEffectMultiplierArmor)?.value ?? 0f;
                        Add(d.modContentPack != null && d.modContentPack.IsOfficialMod ? "ROUPA sem volume (jogo/DLC: o próprio CE deixa assim)" : "ROUPA/ARMADURA", d, "[camadas=" + string.Join(",", d.apparel.layers.Select(l => l.defName)) + "; partes=" + string.Join(",", d.apparel.bodyPartGroups.Select(b => b.defName))
                            + "; sharp=" + sharp + "; blunt=" + blunt + "; stuffMult=" + stuffMult + "; massa=" + d.BaseMass + "; tech=" + d.techLevel + "]");
                        total++;
                    }
                }
            }
            int okTur = 0, vehicles = 0;
            if (Want("veiculos"))
            {
                Type turType = AccessTools.TypeByName("Vehicles.VehicleTurretDef");
                if (turType != null)
                    foreach (Def t in GenDefDatabase.GetAllDefsInDatabaseForDef(turType))
                    {
                        var proj = Traverse.Create(t).Field("projectile").GetValue<ThingDef>();
                        var ammo = Traverse.Create(t).Field("ammunition").GetValue<ThingFilter>();
                        bool ce = proj == null || IsCE(proj.projectile);
                        if (ce) okTur++;
                        else { Add("TORRE DE VEÍCULO", t, "[projétil=" + proj.defName + "/" + proj.thingClass?.Name + "; munição=" + (ammo == null ? "nenhuma" : string.Join(",", ammo.AllowedThingDefs.Take(4).Select(a => a.defName))) + "]"); total++; }
                    }
                Type vehType = AccessTools.TypeByName("Vehicles.VehicleDef");
                if (vehType != null)
                    foreach (ThingDef v in DefDatabase<ThingDef>.AllDefs.Where(x => vehType.IsInstanceOfType(x)))
                    { vehicles++; Add("VEÍCULO (informativo)", v, ""); }
            }
            var sb = new StringBuilder();
            sb.AppendLine("já compatíveis: armas de fogo=" + okGuns + " itens com golpes=" + okMelee + " roupas=" + okApparel + " torres de veículo=" + okTur + " | veículos=" + vehicles);
            foreach (var kv in groups)
            {
                sb.AppendLine("== " + kv.Key + " : " + kv.Value.Count);
                foreach (string l in kv.Value.OrderBy(x => x)) sb.AppendLine("   " + l);
            }
            sb.Append("sem compatibilidade=" + total);
            return sb.ToString();
        }

        /// <summary>
        /// Gera N pawns de um PawnKind (ou de todos os PawnKinds EQ_ de uma facção, com "fac:<nome>") e conta
        /// xenotipo, padrão por baixo (endogenes) e gênero. Os pawns são descartados.
        /// </summary>
        public static string XenoGen(string arg, int n)
        {
            var kinds = new List<PawnKindDef>();
            if (arg.StartsWith("fac:"))
            {
                Faction f = FindFaction(arg.Substring(4));
                if (f == null) return "facção não encontrada: " + arg;
                kinds.AddRange(DefDatabase<PawnKindDef>.AllDefs.Where(k => k.defaultFactionDef == f.def && k.defName.StartsWith("EQ_")));
            }
            else
            {
                PawnKindDef k = DefDatabase<PawnKindDef>.GetNamedSilentFail(arg);
                if (k == null) return "PawnKind não encontrado: " + arg;
                kinds.Add(k);
            }
            var sb = new StringBuilder();
            int problemas = 0;
            foreach (PawnKindDef kind in kinds)
            {
                Faction fac = kind.defaultFactionDef != null ? Find.FactionManager.FirstFactionOfDef(kind.defaultFactionDef) : null;
                var cont = new Dictionary<string, int>();
                for (int i = 0; i < n; i++)
                {
                    Pawn p;
                    try { p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, fac, forceGenerateNewPawn: true)); }
                    catch (Exception ex) { problemas++; sb.AppendLine("  ERRO gerando " + kind.defName + ": " + ex.Message); continue; }
                    XenotypeDef x = p.genes?.Xenotype;
                    XenotypeDef b = EcosDaQueda.EQXenotypeUtil.GuessBase(p);
                    string chave;
                    if (x == null) chave = "(sem genes)";
                    else if (x.inheritable) chave = x.defName;
                    else chave = x.defName + " sobre " + (b?.defName ?? "NENHUM PADRÃO");
                    chave += " " + p.gender;
                    if (x == null || !x.defName.StartsWith("EQ_") || (!x.inheritable && b == null)) { problemas++; chave += " <-- PROBLEMA"; }
                    cont[chave] = cont.TryGetValue(chave) + 1;
                    p.Discard();
                }
                sb.AppendLine(kind.defName + ":");
                foreach (var kv in cont.OrderByDescending(kv => kv.Value))
                    sb.AppendLine("  " + kv.Value + "x " + kv.Key);
            }
            sb.Append("problemas=" + problemas);
            return sb.ToString();
        }

        /// <summary>Caminhos psíquicos licenciados (VPE). psy [show] | psy title &lt;n&gt; (cargo do Coro no líder; 0 tira) |
        /// psy grant | psy gen &lt;PawnKind&gt; &lt;n&gt;.</summary>
        public static string Psy(string action, string a2, string a3)
        {
            if (!EQPsycasts.Active) return "VPE ausente";
            var alg = WorldComponent_Allegiance.Current;
            Pawn l = alg?.leader ?? PawnsFinder.AllMaps_FreeColonists.FirstOrDefault();
            string lic = "licenças: " + string.Join(", ", EQPsycasts.Licenses.Select(kv => kv.Key.defName + "=" + kv.Value.allegiance.ranks[kv.Value.rank].title.defName));
            switch (action)
            {
                case null:
                case "show":
                    break;
                case "title":
                {
                    Faction coro = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("EQ_Faction_Coro"));
                    FactionAllegianceDef d = FactionAllegianceDef.For(coro.def);
                    int n = int.Parse(a2 ?? "0");
                    l.royalty.SetTitle(coro, n <= 0 ? null : d.ranks[n - 1].title, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
                    lic = "título do Coro := " + (n <= 0 ? "nenhum" : d.ranks[n - 1].title.defName) + "\n" + lic;
                    break;
                }
                case "grant":
                {
                    if (EQPsycasts.AbilitiesHediff(l) == null)
                        l.ChangePsylinkLevel(1, sendLetter: false);
                    var g = EQPsycasts.GrantLicensed(l);
                    lic = "destravados: [" + string.Join(", ", g.Select(p => p.defName)) + "]\n" + lic;
                    break;
                }
                case "gen":
                {
                    PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(a2);
                    if (kind == null) return "PawnKind não encontrado: " + a2;
                    int n = int.Parse(a3 ?? "10");
                    Faction fac = kind.defaultFactionDef != null ? Find.FactionManager.FirstFactionOfDef(kind.defaultFactionDef) : null;
                    var cont = new Dictionary<string, int>();
                    int semPoder = 0;
                    for (int i = 0; i < n; i++)
                    {
                        Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, fac, forceGenerateNewPawn: true));
                        var paths = EQPsycasts.UnlockedPaths(p);
                        if (EQPsycasts.AbilitiesHediff(p) == null) semPoder++;
                        foreach (Def d in paths) cont[d.defName] = cont.TryGetValue(d.defName) + 1;
                        p.Discard();
                    }
                    return kind.defName + " (" + n + "): semPoderes=" + semPoder + " caminhos: "
                        + string.Join(", ", cont.OrderByDescending(kv => kv.Value).Select(kv => kv.Key + "=" + kv.Value));
                }
                default:
                    return "ação desconhecida: " + action;
            }
            return lic + "\n" + l.LabelShort + ": nívelPsíquico=" + l.GetPsylinkLevel() + " " + EQPsycasts.Describe(l);
        }

        /// <summary>Diagnóstico: para cada incidente-assinatura da potência, diz se pode disparar e tenta executar.</summary>
        public static string Signature(Faction f, bool execute)
        {
            FactionAllegianceDef d = FactionAllegianceDef.For(f.def);
            if (d == null) return "sem FactionAllegianceDef";
            var sb = new StringBuilder("chance=" + d.hostileIncidentChance + " incidentes=" + d.hostileIncidents.Count);
            foreach (IncidentDef inc in d.hostileIncidents)
            {
                sb.AppendLine();
                if (inc == null) { sb.Append("  (null)"); continue; }
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(inc.category, Map);
                parms.forced = true;
                bool can = inc.Worker.CanFireNow(parms);
                sb.Append("  " + inc.defName + " worker=" + inc.Worker.GetType().Name + " pontos=" + parms.points.ToString("0") + " podeDisparar=" + can);
                if (execute)
                {
                    string r;
                    try { r = inc.Worker.TryExecute(parms).ToString(); } catch (Exception ex) { r = "EXCEÇÃO " + ex.GetType().Name + ": " + ex.Message; }
                    sb.Append(" executou=" + r);
                }
            }
            return sb.ToString();
        }

        public static string FireIncident(string defName)
        {
            IncidentDef inc = DefDatabase<IncidentDef>.GetNamedSilentFail(defName);
            if (inc == null) return "IncidentDef não existe: " + defName;
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(inc.category, Map);
            parms.forced = true;
            return "incidente " + defName + ": executou=" + inc.Worker.TryExecute(parms);
        }

        // ------------------------------------------------------------ cartas

        public static string Letters()
        {
            var sb = new StringBuilder();
            int i = 0;
            foreach (Letter l in Find.LetterStack.LettersListForReading)
            {
                sb.AppendLine("  [" + i++ + "] " + l.Label + " (" + l.def.defName + (l.relatedFaction != null ? ", " + l.relatedFaction.Name : "") + ")");
                if (l is ChoiceLetter cl && !(l is StandardLetter))
                {
                    int j = 0;
                    foreach (DiaOption o in cl.Choices)
                        sb.AppendLine("       opção " + j++ + ": " + OptionText(o) + (o.disabled ? "  [DESATIVADA: " + o.disabledReason + "]" : ""));
                }
            }
            return sb.Length == 0 ? "  (nenhuma carta)" : sb.ToString().TrimEnd();
        }

        private static string OptionText(DiaOption o) => (string)AccessTools.Field(typeof(DiaOption), "text").GetValue(o);

        public static string Choose(string labelPart, int option)
        {
            Letter l = Find.LetterStack.LettersListForReading.LastOrDefault(x => x.Label.ToString().IndexOf(labelPart, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!(l is ChoiceLetter cl)) return "carta de escolha não encontrada: " + labelPart;
            List<DiaOption> opts = cl.Choices.ToList();
            if (option < 0 || option >= opts.Count) return "opção inválida (há " + opts.Count + ")";
            DiaOption o = opts[option];
            if (o.disabled) return "opção desativada: " + o.disabledReason;
            string text = OptionText(o);
            o.action?.Invoke();
            return "escolheu \"" + text + "\" em \"" + l.Label + "\"";
        }

        // ------------------------------------------------------------ relatório

        public static string Dump()
        {
            var sb = new StringBuilder();
            sb.AppendLine("tick=" + Find.TickManager.TicksGame + " dia=" + (Find.TickManager.TicksGame / 60000f).ToString("0.0"));
            Map map = Map;
            if (map != null)
            {
                sb.AppendLine("colonos=" + map.mapPawns.FreeColonistsSpawnedCount + " prisioneiros=" + map.mapPawns.PrisonersOfColonySpawned.Count
                    + " prata=" + EQItems.CountOnMap(map, ThingDefOf.Silver));
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                    sb.AppendLine("  colono " + p.LabelShort + " xenotipo=" + p.genes?.Xenotype?.defName + " ideo=" + p.Ideo?.name);
            }
            sb.AppendLine(DescribeAllegiance());
            List<Faction> powers = Powers.ToList();
            foreach (Faction f in powers)
            {
                sb.AppendLine("== " + f.Name + " [" + f.def.defName + "] líder=" + f.leader?.Name?.ToStringFull + " derrotada=" + f.defeated
                    + " assentamentos=" + Find.WorldObjects.Settlements.Count(s => s.Faction == f));
                sb.AppendLine("   com jogador: " + f.GoodwillWith(Faction.OfPlayer) + " " + f.RelationKindWith(Faction.OfPlayer));
                sb.AppendLine("   " + DescribeReputation(f));
                foreach (Faction o in powers)
                    if (o != f)
                        sb.AppendLine("   com " + o.Name + ": " + f.GoodwillWith(o) + " " + f.RelationKindWith(o));
                Ideo ideo = f.ideos?.PrimaryIdeo;
                if (ideo != null)
                {
                    sb.AppendLine("   ideologia: " + ideo.name + " | membro=" + ideo.memberName + " | ícone=" + ideo.iconDef?.defName + " cor=" + ideo.colorDef?.defName
                        + " | memes=" + string.Join(",", ideo.memes.Select(m => m.defName)));
                    sb.AppendLine("   preceitos(" + ideo.PreceptsListForReading.Count + "): " + string.Join(", ",
                        ideo.PreceptsListForReading.Select(p => (p.def.visible ? "" : "~") + (p is Precept_Role || p is Precept_Ritual ? p.def.defName + "=\"" + p.Label + "\"" : p.def.defName))));
                }
            }
            sb.AppendLine("proxies: " + string.Join(", ", Find.FactionManager.AllFactionsListForReading.Where(EQUtil.IsProxy).Select(x => x.Name + "[" + x.def.defName + "]")));
            return sb.ToString().TrimEnd();
        }
    }
}
