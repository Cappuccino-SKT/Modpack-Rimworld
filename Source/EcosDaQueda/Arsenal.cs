using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EcosDaQueda
{
    /// <summary>
    /// Arsenal de uma facção (Defs/Arsenal/EQ_Arsenal.xml): acrescenta a etiqueta <c>tag</c> às armas listadas
    /// e as unidades da facção sorteiam só por ela. Assim cada facção tem as armas dela, em vez de receber
    /// qualquer coisa que tenha as etiquetas genéricas do jogo ("Gun", "IndustrialGunAdvanced").
    /// Nomes de armas de mods ausentes são ignorados (não precisa de MayRequire).
    /// </summary>
    public class ArsenalDef : Def
    {
        public string tag;
        /// <summary>defNames de armas.</summary>
        public List<string> weapons = new List<string>();
        /// <summary>Toda arma que já tenha uma destas etiquetas.</summary>
        public List<string> fromTags = new List<string>();
        /// <summary>defNames de roupas/armaduras (a etiqueta vai para apparel.tags).</summary>
        public List<string> apparel = new List<string>();
        public List<string> apparelFromTags = new List<string>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string e in base.ConfigErrors())
                yield return e;
            if (tag.NullOrEmpty())
                yield return "ArsenalDef sem tag";
        }
    }

    public static class EQArsenal
    {
        public static void Apply()
        {
            foreach (ArsenalDef a in DefDatabase<ArsenalDef>.AllDefsListForReading)
            {
                if (a.tag.NullOrEmpty())
                    continue;
                foreach (ThingDef t in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (t.IsWeapon && (a.weapons.Contains(t.defName) || (t.weaponTags != null && t.weaponTags.Any(a.fromTags.Contains))))
                    {
                        if (t.weaponTags == null) t.weaponTags = new List<string>();
                        if (!t.weaponTags.Contains(a.tag)) t.weaponTags.Add(a.tag);
                    }
                    if (t.IsApparel && t.apparel != null && (a.apparel.Contains(t.defName) || (t.apparel.tags != null && t.apparel.tags.Any(a.apparelFromTags.Contains))))
                    {
                        if (t.apparel.tags == null) t.apparel.tags = new List<string>();
                        if (!t.apparel.tags.Contains(a.tag)) t.apparel.tags.Add(a.tag);
                    }
                }
            }
            PawnWeaponGenerator.Reset();
            PawnApparelGenerator.Reset();
        }

        /// <summary>Para testes: armas de uma etiqueta.</summary>
        public static IEnumerable<ThingDef> WeaponsOf(string tag) =>
            DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.IsWeapon && t.weaponTags != null && t.weaponTags.Contains(tag));
    }
}
