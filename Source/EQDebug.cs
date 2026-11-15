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
