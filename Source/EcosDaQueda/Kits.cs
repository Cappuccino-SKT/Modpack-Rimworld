using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Coleção de kits fixos (gerada pelo editor de kits, EcosDaQueda-Dev/kits, em Defs/Kits/EQ_Kits.xml).
    /// Cada unidade listada em <see cref="pawnKinds"/> sai com uma das variações, sorteada pelo peso: o
    /// equipamento sorteado pelo jogo (roupas, armas, inventário) é trocado pelo da variação. Os itens são
    /// nomes de def em texto, então um item de mod ausente é só pulado (sem MayRequire).
    /// </summary>
    public class KitCollectionDef : Def
    {
        public List<string> pawnKinds = new List<string>();
        /// <summary>Chance de a unidade usar a coleção (o resto sai com o sorteio normal do arsenal).</summary>
        public float chance = 1f;
        public List<KitVariation> variations = new List<KitVariation>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string e in base.ConfigErrors()) yield return e;
            if (variations.NullOrEmpty()) yield return "coleção de kits sem variações";
        }
    }

    public class KitVariation
    {
        public string label;
        public float weight = 1f;
        public List<KitItem> apparel = new List<KitItem>();
        public List<KitItem> weapons = new List<KitItem>();
        public List<KitItem> inventory = new List<KitItem>();
    }

    public class KitItem
    {
        public string thing;
        public string stuff;
        public QualityCategory quality = QualityCategory.Normal;
        public string color;
        public string style;
        public int count = 1;
        /// <summary>Chance de a peça vir (1 = sempre).</summary>
        public float chance = 1f;
    }

    public static class EQKits
    {
        private static Dictionary<PawnKindDef, List<KitCollectionDef>> byKind;
        private static readonly HashSet<string> warned = new HashSet<string>();

        public static List<KitCollectionDef> For(PawnKindDef kind)
        {
            if (byKind == null)
            {
                byKind = new Dictionary<PawnKindDef, List<KitCollectionDef>>();
                foreach (KitCollectionDef c in DefDatabase<KitCollectionDef>.AllDefsListForReading)
                    foreach (string k in c.pawnKinds)
                    {
                        PawnKindDef pk = DefDatabase<PawnKindDef>.GetNamedSilentFail(k);
                        if (pk == null) { Warn(c.defName + ": unidade não existe: " + k); continue; }
                        if (!byKind.TryGetValue(pk, out var l)) byKind[pk] = l = new List<KitCollectionDef>();
                        l.Add(c);
                    }
            }
            return byKind.TryGetValue(kind, out var r) ? r : null;
        }

        /// <summary>Última variação aplicada (para o comando de teste "kittest").</summary>
        public static KitVariation LastApplied;

        /// <summary>Liga uma coleção a uma unidade em tempo de execução (só para teste).</summary>
        public static void Bind(PawnKindDef kind, KitCollectionDef col)
        {
            For(kind);
            if (!byKind.TryGetValue(kind, out var l)) byKind[kind] = l = new List<KitCollectionDef>();
            if (!l.Contains(col)) l.Add(col);
        }

        /// <summary>Veste e arma o pawn com uma variação sorteada. Devolve a variação usada (ou null).</summary>
        public static KitVariation Apply(Pawn pawn, KitCollectionDef col, KitVariation forced = null)
        {
            if (pawn?.apparel == null || pawn.equipment == null || pawn.inventory == null) return null;
            KitVariation v = forced ?? col.variations.RandomElementByWeightWithFallback(x => x.weight);
            if (v == null) return null;
            foreach (Apparel a in pawn.apparel.WornApparel.ToList()) { pawn.apparel.Remove(a); a.Destroy(); }
            pawn.equipment.DestroyAllEquipment();
            pawn.inventory.DestroyAll();

            foreach (KitItem it in v.apparel)
            {
                if (!Rand.Chance(it.chance)) continue;
                if (!(Make(it, col) is Apparel ap)) continue;
                if (!ApparelUtility.HasPartsToWear(pawn, ap.def)) { ap.Destroy(); continue; }
                pawn.apparel.Wear(ap, dropReplacedApparel: false);
            }
            bool primary = false;
            foreach (KitItem it in v.weapons)
            {
                if (!Rand.Chance(it.chance)) continue;
                if (!(Make(it, col) is ThingWithComps w) || !w.def.IsWeapon) continue;
                if (!primary) { pawn.equipment.AddEquipment(w); primary = true; }
                else pawn.inventory.innerContainer.TryAdd(w);   // armas extras: reserva (o Simple Sidearms usa)
            }
            foreach (KitItem it in v.inventory)
            {
                if (!Rand.Chance(it.chance)) continue;
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(it.thing);
                if (def == null) { Warn(col.defName + ": item não existe: " + it.thing); continue; }
                int left = Mathf.Max(1, it.count);
                while (left > 0)
                {
                    Thing t = Make(it, col);
                    if (t == null) break;
                    t.stackCount = Mathf.Min(left, def.stackLimit);
                    left -= t.stackCount;
                    if (!pawn.inventory.innerContainer.TryAdd(t)) { t.Destroy(); break; }
                }
            }
            LastApplied = v;
            return v;
        }

        private static Thing Make(KitItem it, KitCollectionDef col)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(it.thing);
            if (def == null) { Warn(col.defName + ": item não existe: " + it.thing); return null; }
            ThingDef stuff = null;
            if (def.MadeFromStuff)
            {
                stuff = it.stuff == null ? null : DefDatabase<ThingDef>.GetNamedSilentFail(it.stuff);
                if (stuff == null || !stuff.stuffProps.CanMake(def)) stuff = GenStuff.DefaultStuffFor(def);
            }
            Thing t = ThingMaker.MakeThing(def, stuff);
            t.TryGetComp<CompQuality>()?.SetQuality(it.quality, ArtGenerationContext.Outsider);
            if (!it.style.NullOrEmpty())
            {
                ThingStyleDef st = DefDatabase<ThingStyleDef>.GetNamedSilentFail(it.style);
                if (st != null) t.StyleDef = st;
            }
            if (!it.color.NullOrEmpty() && t.TryGetComp<CompColorable>() != null)
            {
                try { t.SetColor(ParseHelper.FromString<Color>(it.color), reportFailure: false); }
                catch { Warn(col.defName + ": cor inválida: " + it.color); }
            }
            return t;
        }

        private static void Warn(string msg)
        {
            if (warned.Add(msg)) Log.Warning("[Ecos da Queda] kits: " + msg);
        }
    }

    /// <summary>Depois do sorteio normal do equipamento, unidades com coleção de kits trocam pelo kit.</summary>
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGearFor")]
    public static class Patch_PawnGenerator_GenerateGearFor_Kits
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn?.kindDef == null || !pawn.RaceProps.Humanlike) return;
            List<KitCollectionDef> cols = EQKits.For(pawn.kindDef);
            if (cols.NullOrEmpty()) return;
            KitCollectionDef col = cols.RandomElement();
            if (!Rand.Chance(col.chance)) return;
            EQKits.Apply(pawn, col);
        }
    }
}
