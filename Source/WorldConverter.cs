using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EcosDaQueda.DebugTools
{
    /// <summary>
    /// Mundo fixo (MUNDO.md, M2): conversor da América do Norte (preset "The Earth" do Worldbuilder, cobertura 100%)
    /// para um mundo de cobertura 25%. Com 25% o jogo centra a calota em 45°N (raio 45°) e o tile tem o mesmo tamanho,
    /// então a conversão é 1:1: mesma latitude, longitude deslocada (offset), tile mais próximo.
    /// 1) Num jogo da Terra (newgame ... cov=1 seed=SOL3 preset=...): "worlddump arquivo" grava os tiles da região.
    /// 2) Num jogo 25% (newgame ... cov=0.25): "worldconvert arquivo offset preset" aplica, gera a prévia e salva o preset.
    /// </summary>
    public static class EQWorldConverter
    {
        // Região de origem (América do Norte + México, com folga).
        public const float LatMin = -10f, LatMax = 90f, LonMin = -180f, LonMax = 0f;

        private static string F(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        public static string Dump(string path)
        {
            var grid = Find.WorldGrid;
            var surface = grid.Surface;
            var sb = new StringBuilder();
            int n = 0;
            var biomes = new Dictionary<string, int>();
            for (int i = 0; i < surface.TilesCount; i++)
            {
                var t = new PlanetTile(i, surface);
                Vector2 ll = grid.LongLatOf(t);
                if (ll.y < LatMin || ll.y > LatMax || ll.x < LonMin || ll.x > LonMax) continue;
                Tile tile = surface[i];
                string b = tile.PrimaryBiome?.defName ?? "Ocean";
                biomes[b] = biomes.TryGetValue(b, out int c) ? c + 1 : 1;
                sb.Append(F(ll.y)).Append('\t').Append(F(ll.x)).Append('\t').Append(b).Append('\t').Append(F(tile.elevation)).Append('\t')
                  .Append((int)tile.hilliness).Append('\t').Append(F(tile.temperature)).Append('\t').Append(F(tile.rainfall)).Append('\t')
                  .Append(F(tile.swampiness)).Append('\n');
                n++;
            }
            File.WriteAllText(path, sb.ToString());
            return "tiles_total=" + surface.TilesCount + " na_região=" + n + " cobertura=" + Find.World.info.planetCoverage + " semente=" + Find.World.info.seedString
                   + " biomas=[" + string.Join(", ", biomes.OrderByDescending(kv => kv.Value).Select(kv => kv.Key + "=" + kv.Value)) + "]";
        }

        /// <summary>Lat/lon de todos os tiles do mundo atual (para otimizar o encaixe fora do jogo).</summary>
        public static string GridDump(string path)
        {
            var grid = Find.WorldGrid;
            var surface = grid.Surface;
            var sb = new StringBuilder();
            for (int i = 0; i < surface.TilesCount; i++)
            {
                Vector2 ll = grid.LongLatOf(new PlanetTile(i, surface));
                sb.Append(F(ll.y)).Append('\t').Append(F(ll.x)).Append('\n');
            }
            File.WriteAllText(path, sb.ToString());
            return "tiles=" + surface.TilesCount + " cobertura=" + Find.World.info.planetCoverage;
        }

        private struct Src
        {
            public float lat, lon, elev, temp, rain, swamp;
            public int hill;
            public BiomeDef biome;
        }

        public static string LastReport;

        public static string Convert(string path, float cLat, float cLon, float k, string presetName)
        {
            if (!File.Exists(path)) return "sem arquivo " + path;
            var src = new List<Src>();
            foreach (string line in File.ReadLines(path))
            {
                string[] p = line.Split('\t');
                if (p.Length < 8) continue;
                src.Add(new Src
                {
                    lat = float.Parse(p[0], CultureInfo.InvariantCulture), lon = float.Parse(p[1], CultureInfo.InvariantCulture),
                    biome = DefDatabase<BiomeDef>.GetNamedSilentFail(p[2]) ?? BiomeDefOf.Ocean,
                    elev = float.Parse(p[3], CultureInfo.InvariantCulture), hill = int.Parse(p[4]),
                    temp = float.Parse(p[5], CultureInfo.InvariantCulture), rain = float.Parse(p[6], CultureInfo.InvariantCulture),
                    swamp = float.Parse(p[7], CultureInfo.InvariantCulture),
                });
            }
            // Caixas de 0,5° para o vizinho mais próximo.
            var bins = new Dictionary<long, List<int>>();
            long Key(int a, int b) => ((long)a << 32) ^ (uint)b;
            for (int i = 0; i < src.Count; i++)
            {
                long kk = Key(Mathf.FloorToInt(src[i].lat * 2f), Mathf.FloorToInt(src[i].lon * 2f));
                if (!bins.TryGetValue(kk, out var l)) bins[kk] = l = new List<int>();
                l.Add(i);
            }
            var grid = Find.WorldGrid;
            var surface = grid.Surface;
            int hit = 0, ocean = 0;
            var count = new Dictionary<BiomeDef, int>();
            for (int i = 0; i < surface.TilesCount; i++)
            {
                Vector2 ll = grid.LongLatOf(new PlanetTile(i, surface));
                // calota de 25% centrada em 45°N/0°: o centro (45, 0) recebe o ponto (cLat, cLon) do mapa da Terra, escala k
                float lat = cLat + (ll.y - 45f) / k, lon = cLon + ll.x / k;
                if (lon < -180f) lon += 360f;
                if (lon > 180f) lon -= 360f;
                int bi = Mathf.FloorToInt(lat * 2f), bj = Mathf.FloorToInt(lon * 2f);
                float cos = Mathf.Cos(lat * Mathf.Deg2Rad), best = float.MaxValue;
                int bestIdx = -1;
                for (int di = -1; di <= 1; di++)
                    for (int dj = -2; dj <= 2; dj++)
                        if (bins.TryGetValue(Key(bi + di, bj + dj), out var l))
                            foreach (int s in l)
                            {
                                float dl = src[s].lat - lat, dn = (src[s].lon - lon) * cos;
                                float d = dl * dl + dn * dn;
                                if (d < best) { best = d; bestIdx = s; }
                            }
                Tile tile = surface[i];
                if (bestIdx < 0 || best > 2.25f)
                {
                    // fora da região (ou longe demais): oceano
                    tile.PrimaryBiome = BiomeDefOf.Ocean;
                    tile.elevation = -400f;
                    tile.hilliness = Hilliness.Flat;
                    tile.swampiness = 0f;
                    ocean++;
                }
                else
                {
                    Src s = src[bestIdx];
                    tile.PrimaryBiome = s.biome;
                    tile.elevation = s.elev;
                    tile.hilliness = (Hilliness)s.hill;
                    tile.temperature = s.temp;
                    tile.rainfall = s.rain;
                    tile.swampiness = s.swamp;
                    hit++;
                }
                tile.pollution = 0f;
                ClearLists(tile);
                count[tile.PrimaryBiome] = count.TryGetValue(tile.PrimaryBiome, out int cc) ? cc + 1 : 1;
            }
            // Marcos, mutadores e nomes de regiões: variam por partida (decisão do usuário); o preset sai sem eles.
            try { Find.World.landmarks?.landmarks?.Clear(); } catch { }
            try { Find.WorldFeatures.features.Clear(); for (int i = 0; i < surface.TilesCount; i++) surface[i].feature = null; } catch { }
            surface.SetAllLayersDirty();
            string preview = Preview(Path.ChangeExtension(path, null) + "-preview.ppm");
            string saved = presetName.NullOrEmpty() || presetName == "-" ? "não salvo" : SavePreset(presetName);
            LastReport = "centro=(" + cLat + ", " + cLon + ") escala=" + k + " origem=" + src.Count + " destino=" + surface.TilesCount + " da_terra=" + hit + " oceano_fora=" + ocean
                         + " biomas=" + count.Count + " [" + string.Join(", ", count.OrderByDescending(kv => kv.Value).Select(kv => kv.Key.defName + "=" + kv.Value)) + "]"
                         + " prévia=" + preview + " preset=" + saved;
            return LastReport;
        }

        // ------------------------------------------------------------ M2b: pintura, rios, nomes, salvar

        private static bool IsWater(BiomeDef b) => b == BiomeDefOf.Ocean || b == BiomeDefOf.Lake || b?.defName == "SeaIce";

        private static bool Fits(Tile t, string onto, PlanetTile pt)
        {
            BiomeDef b = t.PrimaryBiome;
            string n = b?.defName;
            switch (onto)
            {
                case "agua": return b == BiomeDefOf.Ocean;
                case "frio": return n == "Tundra" || n == "IceSheet" || n == "BorealForest" || n == "SeaIce" && false;
                case "quente": return n == "Desert" || n == "AridShrubland" || n == "Grasslands" || n == "TropicalRainforest";
                case "costa":
                    if (IsWater(b) || n == "IceSheet") return false;
                    var tmp = new List<PlanetTile>();
                    Find.WorldGrid.GetTileNeighbors(pt, tmp);
                    foreach (PlanetTile x in tmp)
                    {
                        if (Find.WorldGrid[x].PrimaryBiome == BiomeDefOf.Ocean) return true;
                        var tmp2 = new List<PlanetTile>();
                        Find.WorldGrid.GetTileNeighbors(x, tmp2);
                        if (tmp2.Any(y => Find.WorldGrid[y].PrimaryBiome == BiomeDefOf.Ocean)) return true;
                    }
                    return false;
                default: return !IsWater(b) && n != "IceSheet";
            }
        }

        /// <summary>Pinta biomas por regiões (mundo/pintura.tsv): bioma, lat, lon, raio (graus), sobre, nota. Borda com ruído.</summary>
        public static string Paint(string path)
        {
            if (!File.Exists(path)) return "sem arquivo " + path;
            var grid = Find.WorldGrid;
            var surface = grid.Surface;
            var ll = new Vector2[surface.TilesCount];
            for (int i = 0; i < ll.Length; i++) ll[i] = grid.LongLatOf(new PlanetTile(i, surface));
            var res = new List<string>();
            int rule = 0;
            foreach (string line in File.ReadLines(path))
            {
                if (line.StartsWith("#") || line.Trim().Length == 0) continue;
                string[] p = line.Split('\t');
                if (p.Length < 5) continue;
                rule++;
                BiomeDef b = DefDatabase<BiomeDef>.GetNamedSilentFail(p[0]);
                if (b == null) { res.Add(p[0] + "=bioma não existe"); continue; }
                float lat = float.Parse(p[1], CultureInfo.InvariantCulture), lon = float.Parse(p[2], CultureInfo.InvariantCulture), r = float.Parse(p[3], CultureInfo.InvariantCulture);
                string onto = p[4];
                int n = 0;
                for (int i = 0; i < ll.Length; i++)
                {
                    float dl = ll[i].y - lat, dn = (ll[i].x - lon) * Mathf.Cos(ll[i].y * Mathf.Deg2Rad);
                    float d = Mathf.Sqrt(dl * dl + dn * dn);
                    if (d > r * 1.35f) continue;
                    float noise = Mathf.PerlinNoise(ll[i].x * 0.9f + rule * 17.3f, ll[i].y * 0.9f + rule * 5.1f);
                    if (d > r * (0.7f + 0.6f * noise)) continue;
                    Tile t = surface[i];
                    if (!Fits(t, onto, new PlanetTile(i, surface))) continue;
                    t.PrimaryBiome = b;
                    n++;
                }
                res.Add(p[0] + "=" + n);
            }
            surface.SetAllLayersDirty();
            return "regras=" + rule + " [" + string.Join(", ", res) + "] biomas_no_mundo=" + Enumerable.Range(0, surface.TilesCount).Select(i => surface[i].PrimaryBiome).Distinct().Count();
        }

        /// <summary>Refaz rios e nomes de regiões com os passos do próprio jogo sobre o terreno novo.</summary>
        public static string Rivers(string seed)
        {
            var surface = Find.WorldGrid.Surface;
            var r = new List<string>();
            try { new WorldGenStep_Rivers().GenerateFresh(seed, surface); r.Add("rios ok"); }
            catch (Exception ex) { r.Add("rios: " + ex.GetType().Name + " " + ex.Message); }
            try
            {
                Find.WorldFeatures.features.Clear();
                for (int i = 0; i < surface.TilesCount; i++) surface[i].feature = null;
                new WorldGenStep_Features().GenerateFresh(seed, surface);
                Find.WorldFeatures.textsCreated = false;
                r.Add("nomes=" + Find.WorldFeatures.features.Count);
            }
            catch (Exception ex) { r.Add("nomes: " + ex.GetType().Name + " " + ex.Message); }
            int rivers = 0;
            for (int i = 0; i < surface.TilesCount; i++)
                if (surface[i] is SurfaceTile st && st.Rivers != null && st.Rivers.Count > 0) rivers++;
            surface.SetAllLayersDirty();
            return string.Join("; ", r) + " tiles_com_rio=" + rivers;
        }

        public static string PreviewNow(string path) => "prévia=" + Preview(path);

        /// <summary>Rios, estradas e mutadores da grade antiga não valem mais (o terreno mudou).</summary>
        private static void ClearLists(Tile tile)
        {
            foreach (string fn in new[] { "potentialRivers", "potentialRoads", "mutatorsNullable", "mutators" })
            {
                FieldInfo f = AccessTools_Field(tile.GetType(), fn);
                if (f == null) continue;
                if (typeof(System.Collections.IList).IsAssignableFrom(f.FieldType)) f.SetValue(tile, null);
            }
        }

        private static FieldInfo AccessTools_Field(Type t, string name)
        {
            for (Type x = t; x != null; x = x.BaseType)
            {
                FieldInfo f = x.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (f != null) return f;
            }
            return null;
        }

        /// <summary>Prévia equirretangular (lat 5–85, lon da América) em PPM, uma cor por bioma.</summary>
        private static string Preview(string path)
        {
            // coordenadas do mundo novo: latitude 0–90, longitude −60 a 60
            const int W = 1200, H = 900;
            var grid = Find.WorldGrid;
            var surface = grid.Surface;
            var img = new byte[W * H * 3];
            var zbuf = new float[W * H];
            for (int i = 0; i < zbuf.Length; i++) zbuf[i] = float.MaxValue;
            for (int i = 0; i < surface.TilesCount; i++)
            {
                Vector2 ll = grid.LongLatOf(new PlanetTile(i, surface));
                float lat = ll.y, lon = ll.x;
                int x = Mathf.RoundToInt((lon + 60f) / 120f * (W - 1));
                int y = Mathf.RoundToInt((90f - lat) / 90f * (H - 1));
                Color c = BiomeColor(surface[i].PrimaryBiome);
                if (surface[i] is SurfaceTile stl && stl.Rivers != null && stl.Rivers.Count > 0) c = new Color(0.45f, 0.75f, 1f);
                if (surface[i] is SurfaceTile str && str.Roads != null && str.Roads.Count > 0) c = new Color(0.35f, 0.22f, 0.12f);
                for (int dy = -4; dy <= 4; dy++)
                    for (int dx = -4; dx <= 4; dx++)
                    {
                        int px = x + dx, py = y + dy;
                        if (px < 0 || py < 0 || px >= W || py >= H) continue;
                        float d = dx * dx + dy * dy;
                        int k = py * W + px;
                        if (d >= zbuf[k]) continue;
                        zbuf[k] = d;
                        img[k * 3] = (byte)(c.r * 255); img[k * 3 + 1] = (byte)(c.g * 255); img[k * 3 + 2] = (byte)(c.b * 255);
                    }
            }
            // Cidades (M3): quadrado na cor da facção com borda preta.
            foreach (Settlement st in Find.WorldObjects.Settlements)
            {
                if (st.Tile.Layer != surface) continue;
                Vector2 ll = grid.LongLatOf(st.Tile);
                int x = Mathf.RoundToInt((ll.x + 60f) / 120f * (W - 1)), y = Mathf.RoundToInt((90f - ll.y) / 90f * (H - 1));
                Color fc = st.Faction?.Color ?? Color.white;
                for (int dy = -6; dy <= 6; dy++)
                    for (int dx = -6; dx <= 6; dx++)
                    {
                        int px = x + dx, py = y + dy;
                        if (px < 0 || py < 0 || px >= W || py >= H) continue;
                        Color c = Math.Abs(dx) >= 5 || Math.Abs(dy) >= 5 ? Color.black : fc;
                        int k = py * W + px;
                        img[k * 3] = (byte)(c.r * 255); img[k * 3 + 1] = (byte)(c.g * 255); img[k * 3 + 2] = (byte)(c.b * 255);
                    }
            }
            using (var fs = new FileStream(path, FileMode.Create))
            {
                byte[] head = Encoding.ASCII.GetBytes("P6\n" + W + " " + H + "\n255\n");
                fs.Write(head, 0, head.Length);
                fs.Write(img, 0, img.Length);
            }
            return path;
        }

        public static Color BiomeColor(BiomeDef b)
        {
            if (b == null) return Color.black;
            switch (b.defName)
            {
                case "Ocean": return new Color(0.10f, 0.20f, 0.45f);
                case "Lake": return new Color(0.20f, 0.40f, 0.75f);
                case "SeaIce": return new Color(0.80f, 0.88f, 0.95f);
                case "IceSheet": return new Color(0.95f, 0.97f, 1f);
                case "GlacialPlain": return new Color(0.75f, 0.85f, 0.85f);
                case "Tundra": return new Color(0.60f, 0.65f, 0.55f);
                case "BorealForest": return new Color(0.15f, 0.40f, 0.30f);
                case "ColdBog": return new Color(0.35f, 0.45f, 0.35f);
                case "TemperateForest": return new Color(0.20f, 0.55f, 0.20f);
                case "TemperateSwamp": return new Color(0.30f, 0.45f, 0.20f);
                case "Grasslands": return new Color(0.60f, 0.75f, 0.30f);
                case "AridShrubland": return new Color(0.75f, 0.70f, 0.40f);
                case "Desert": return new Color(0.90f, 0.80f, 0.50f);
                case "ExtremeDesert": return new Color(0.95f, 0.65f, 0.35f);
                case "TropicalRainforest": return new Color(0.05f, 0.45f, 0.10f);
                case "TropicalSwamp": return new Color(0.15f, 0.35f, 0.15f);
            }
            int h = b.defName.GetHashCode();
            return new Color(((h >> 16) & 255) / 255f, ((h >> 8) & 255) / 255f, (h & 255) / 255f);
        }

        /// <summary>Salva o mundo atual como preset do Worldbuilder (por reflexão; o debug não depende dele).</summary>
        public static string SavePreset(string name)
        {
            Type tPreset = GenTypes.GetTypeInAnyAssembly("Worldbuilder.WorldPreset");
            Type tMod = GenTypes.GetTypeInAnyAssembly("Worldbuilder.WorldbuilderMod");
            Type tMgr = GenTypes.GetTypeInAnyAssembly("Worldbuilder.WorldPresetManager");
            if (tPreset == null || tMod == null || tMgr == null) return "sem Worldbuilder";
            try
            {
                object preset = Activator.CreateInstance(tPreset);
                void Set(string f, object v) { FieldInfo fi = tPreset.GetField(f); if (fi != null) fi.SetValue(preset, v); }
                Set("name", name);
                Set("description", "América do Norte e México depois da Queda. Mundo fixo do Ecos da Queda (biomas e relevo fixos; marcos, ruínas e sítios variam por partida).");
                Set("saveTerrain", true);
                Set("saveBases", false);
                Set("saveFactions", false);
                Set("saveIdeologies", false);
                tMod.GetMethod("SaveWorldDataToPreset", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new[] { preset });
                object ok = tMgr.GetMethod("SavePreset", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new[] { preset, null, null });
                string folder = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "Worldbuilder");
                return "salvo=" + ok + " pasta=" + folder;
            }
            catch (Exception ex) { return "erro: " + (ex.InnerException ?? ex).GetType().Name + " " + (ex.InnerException ?? ex).Message; }
        }
    }
}
