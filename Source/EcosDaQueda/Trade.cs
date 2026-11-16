using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Num TraderKindDef nosso: começa com o estoque de outro comerciante (já com o que os mods acrescentaram
    /// a ele por patch, ex.: munição do Combat Extended) e soma os stockGenerators do próprio def.
    /// dropTradeTags tira do estoque copiado os geradores por valor (StockGenerator_MarketValue) com aquelas
    /// etiquetas (ex.: WeaponRanged), para as armas virem só do arsenal da facção.
    /// </summary>
    public class TraderStockCopyExtension : DefModExtension
    {
        public TraderKindDef copyFrom;
        public List<string> dropTradeTags = new List<string>();
        /// <summary>Tira do estoque copiado todo gerador (menos os de item único) que venda armas de verdade.</summary>
        public bool dropWeapons;
    }

    /// <summary>Estoque por etiqueta de sorteio: armas (weaponTags) e roupas (apparelTags) do arsenal de uma facção.</summary>
    public class StockGenerator_EQTags : StockGenerator_MiscItems
    {
        public List<string> weaponTags = new List<string>();
        public List<string> apparelTags = new List<string>();

        public override bool HandlesThingDef(ThingDef t)
        {
            if (!base.HandlesThingDef(t))
                return false;
            if (t.IsWeapon && t.weaponTags != null && t.weaponTags.Any(weaponTags.Contains))
                return true;
            return t.IsApparel && t.apparel?.tags != null && t.apparel.tags.Any(apparelTags.Contains);
        }
    }

    public static class EQTrade
    {
        /// <summary>Para depuração: o que foi tirado do estoque copiado de cada comerciante.</summary>
        public static readonly Dictionary<TraderKindDef, List<string>> Dropped = new Dictionary<TraderKindDef, List<string>>();

        private static string Describe(StockGenerator g)
        {
            var tr = Traverse.Create(g);
            string tag = (tr.Field("tradeTag").FieldExists() ? tr.Field("tradeTag").GetValue<string>() : null)
                ?? (tr.Field("categoryDef").FieldExists() ? tr.Field("categoryDef").GetValue<ThingCategoryDef>()?.defName : null);
            return g.GetType().Name + (tag == null ? "" : ":" + tag);
        }

        private static bool SellsWeapons(StockGenerator g)
        {
            try
            {
                // Só os geradores que são principalmente de armas (a munição do CE inclui granadas, mas não é "de armas").
                int total = 0, weapons = 0;
                foreach (ThingDef t in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (t.category != ThingCategory.Item || !g.HandlesThingDef(t)) continue;
                    total++;
                    if (t.IsWeapon && t.equipmentType == EquipmentType.Primary && !t.weaponTags.NullOrEmpty()) weapons++;
                }
                return weapons > 0 && weapons * 2 >= total;
            }
            catch { return false; }
        }

        public static void Apply()
        {
            MethodInfo clone = AccessTools.Method(typeof(object), "MemberwiseClone");
            foreach (TraderKindDef t in DefDatabase<TraderKindDef>.AllDefsListForReading)
            {
                TraderStockCopyExtension ext = t.GetModExtension<TraderStockCopyExtension>();
                if (ext?.copyFrom == null || ext.copyFrom == t)
                    continue;
                var copied = new List<StockGenerator>();
                foreach (StockGenerator g in ext.copyFrom.stockGenerators)
                {
                    if (g is StockGenerator_MarketValue mv && ext.dropTradeTags.Contains(mv.tradeTag))
                        continue;
                    if (ext.dropWeapons && !(g is StockGenerator_SingleDef) && !g.GetType().Name.Contains("Buy") && !Describe(g).Contains(":CE_") && SellsWeapons(g))
                    {
                        if (!Dropped.TryGetValue(t, out List<string> l)) Dropped[t] = l = new List<string>();
                        l.Add(Describe(g));
                        continue;
                    }
                    var c = (StockGenerator)clone.Invoke(g, null);
                    c.ResolveReferences(t);
                    copied.Add(c);
                }
                t.stockGenerators.InsertRange(0, copied);
            }
        }
    }

    /// <summary>
    /// Às vezes a geração do texto de um livro falha (NullReference em Book.AppendDoerRules, também com os
    /// comerciantes do jogo). Sem isto, a exceção derruba a geração do estoque inteiro e o comerciante fica vazio.
    /// Aqui o erro vira um aviso (com o livro e os efeitos dele) e o resto do estoque segue.
    /// </summary>
    [HarmonyPatch(typeof(Book), "GenerateBook")]
    public static class Patch_Book_GenerateBook
    {
        public static System.Exception Finalizer(System.Exception __exception, Book __instance)
        {
            if (__exception != null)
            {
                string doers = "?";
                try { doers = string.Join(", ", __instance.BookComp.Doers.Select(d => d.GetType().Name)); } catch { }
                Log.Warning("[Ecos da Queda] livro " + __instance?.def?.defName + " gerado sem texto (efeitos: " + doers + "): " + __exception.GetType().Name
                    + " em " + (__exception.StackTrace ?? "").Split((char)10).FirstOrDefault()?.Trim());
            }
            return null;
        }
    }

    /// <summary>
    /// Com Combat Extended + Simple Sidearms, um pawn às vezes recebe o trabalho de largar uma arma que já foi
    /// trocada/guardada no meio do caminho; o jogo então acusa "tried to drop ... which it didn't contain".
    /// Largar o que não se tem passa a ser simplesmente "não fez nada".
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
    public static class Patch_EquipmentTracker_TryDropEquipment
    {
        public static bool Prefix(Pawn_EquipmentTracker __instance, ThingWithComps eq, ref ThingWithComps resultingEq, ref bool __result)
        {
            if (eq != null && __instance.AllEquipmentListForReading.Contains(eq))
                return true;
            resultingEq = null;
            __result = false;
            return false;
        }
    }
}
