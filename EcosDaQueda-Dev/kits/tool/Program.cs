// Extrator de ícones do editor de kits. Uso (normalmente pelo kits.py):
//   dotnet run --project kits/tool -c Release -- <kitcatalog.json> <pasta do jogo> <pasta de saída>
// Para cada item do catálogo acha a textura do ícone (uiIconPath, ou a textura do chão) nas pastas Textures dos mods
// ou nos Assets.zip do Waylith (jogo base e DLCs) e grava <saída>/<defName>.png. Itens já extraídos são pulados
// (apague a pasta para refazer tudo). Imprime quantos foram extraídos e os que não acharam textura.
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using AssetRipper.TextureDecoder.Bc;

string catalogPath = args[0], gameDir = Path.GetFullPath(args[1]), outDir = args[2];
Directory.CreateDirectory(outDir);
AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
{
    string p = Path.Combine(gameDir, new AssemblyName(e.Name).Name + ".dll");
    return File.Exists(p) ? Assembly.LoadFrom(p) : null;
};

// ---- índice de texturas soltas: "things/weapons/fnfal" -> arquivos (de todos os mods)
var loose = new Dictionary<string, List<string>>();
var roots = new List<string>();
foreach (string baseDir in new[] { Path.Combine(gameDir, "Mods"), Path.Combine(gameDir, "Data") })
    if (Directory.Exists(baseDir)) roots.AddRange(Directory.GetDirectories(baseDir));
foreach (string root in roots)
    foreach (string f in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
    {
        string ext = Path.GetExtension(f).ToLowerInvariant();
        if (ext != ".png" && ext != ".dds" && ext != ".jpg") continue;
        string norm = f.Replace('\\', '/');
        int i = norm.LastIndexOf("/Textures/", StringComparison.OrdinalIgnoreCase);
        if (i < 0) continue;
        string key = norm.Substring(i + 10, norm.Length - i - 10 - ext.Length).ToLowerInvariant();
        if (!loose.TryGetValue(key, out var l)) loose[key] = l = new List<string>();
        l.Add(f);
    }

// ---- índice dos Assets.zip (jogo base na raiz, DLCs em Data/<DLC>/Assets.zip)
var zips = new List<ZipArchive>();
var packed = new Dictionary<string, (ZipArchive zip, ZipArchiveEntry entry, string path)>();
foreach (string zp in new[] { Path.Combine(gameDir, "Assets.zip") }.Concat(Directory.GetFiles(Path.Combine(gameDir, "Data"), "Assets.zip", SearchOption.AllDirectories)))
{
    if (!File.Exists(zp)) continue;
    var z = ZipFile.OpenRead(zp);
    zips.Add(z);
    foreach (var e in z.Entries)
    {
        // jogo base: "Resources/Textures/..."; DLCs: "Textures/..."
        string pre = e.FullName.StartsWith("Resources/Textures/") ? "Resources/Textures/" : e.FullName.StartsWith("Textures/") ? "Textures/" : null;
        if (pre == null) continue;
        string rest = e.FullName.Substring(pre.Length);
        int dot = rest.IndexOf('.', rest.LastIndexOf('/') + 1);
        if (dot < 0) continue;
        packed.TryAdd(rest.Substring(0, dot).ToLowerInvariant(), (z, e, zp));
    }
}

var joinMethod = Assembly.LoadFrom(Path.Combine(gameDir, "Waylith.dll")).GetType("Waylith.Graphics.Textures.DdsFile")
    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).First(m => m.Name == "TryJoin");
var join = (JoinDelegate)Delegate.CreateDelegate(typeof(JoinDelegate), joinMethod);

using var doc = JsonDocument.Parse(File.ReadAllText(catalogPath));
int ok = 0, skipped = 0;
var missing = new List<string>();
var entries = doc.RootElement.GetProperty("items").EnumerateArray().ToList();
// desbloqueáveis da página Pesquisa (construções, plantas, pisos, itens fabricáveis)
if (doc.RootElement.TryGetProperty("research", out var research) && research.TryGetProperty("unlocks", out var unl))
    entries.AddRange(unl.EnumerateArray().Where(u => u.GetProperty("kind").GetString() != "recipe"));
foreach (var it in entries)
{
    string def = it.GetProperty("def").GetString();
    string dst = Path.Combine(outDir, def + ".png");
    if (File.Exists(dst)) { skipped++; continue; }
    string modDir = it.GetProperty("modDir").GetString().Replace('\\', '/').TrimEnd('/');
    string icon = it.GetProperty("icon").GetString(), tex = it.GetProperty("tex").GetString(), gclass = it.GetProperty("gclass").GetString();
    var keys = new List<string>();
    if (!string.IsNullOrEmpty(icon)) keys.Add(icon);
    if (!string.IsNullOrEmpty(tex))
    {
        keys.Add(tex); keys.Add(tex + "_south"); keys.Add(tex + "_east");
        // Graphic_StackCount/Random/Collection: a textura é uma pasta; o ícone é o primeiro arquivo dela.
        string folder = tex.ToLowerInvariant().TrimEnd('/') + "/";
        keys.AddRange(loose.Keys.Concat(packed.Keys).Where(k => k.StartsWith(folder) && k.IndexOf('/', folder.Length) < 0)
            .Where(k => !k.EndsWith("_m")).OrderBy(k => k.EndsWith("_north") || k.EndsWith("_east") ? 1 : 0).ThenBy(k => k, StringComparer.Ordinal));
    }
    bool done = false;
    foreach (string key in keys.Select(k => k.ToLowerInvariant()).Distinct())
    {
        try
        {
            if (loose.TryGetValue(key, out var files))
            {
                // prefere o arquivo do próprio mod do item (e, dentro dele, a pasta de versão mais nova)
                string f = files.OrderBy(x => x.Replace('\\', '/').StartsWith(modDir, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenByDescending(x => x, StringComparer.OrdinalIgnoreCase).First();
                byte[] data = File.ReadAllBytes(f);
                if (f.EndsWith(".dds", StringComparison.OrdinalIgnoreCase)) data = DdsToPng(data);
                if (data == null) continue;
                File.WriteAllBytes(dst, data);
                done = true; break;
            }
            if (packed.TryGetValue(key, out var pk))
            {
                byte[] raw = ReadRaw(pk.path, pk.entry);
                byte[] dds = pk.entry.FullName.EndsWith(".split") && join(raw, out byte[] joined, out int len) ? joined.AsSpan(0, len).ToArray() : raw;
                byte[] png = DdsToPng(dds);
                if (png == null) continue;
                File.WriteAllBytes(dst, png);
                done = true; break;
            }
        }
        catch (Exception ex) { Console.Error.WriteLine($"{def} ({key}): {ex.Message}"); }
    }
    if (done) ok++; else missing.Add(def + " [" + (icon != "" ? icon : tex) + "]");
}
Console.WriteLine($"ícones: {ok} extraídos, {skipped} já existiam, {missing.Count} sem textura");
File.WriteAllLines(Path.Combine(outDir, "_sem-icone.txt"), missing);
foreach (var z in zips) z.Dispose();

// Dados de uma entrada do zip; o Waylith usa zstd (método 93), que o ZipArchive não lê: lê o cabeçalho local à mão.
static byte[] ReadRaw(string zipPath, ZipArchiveEntry e)
{
    var fld = typeof(ZipArchiveEntry).GetField("_offsetOfLocalHeader", BindingFlags.NonPublic | BindingFlags.Instance);
    long off = Convert.ToInt64(fld.GetValue(e));
    using var fs = File.OpenRead(zipPath);
    var h = new byte[30];
    fs.Seek(off, SeekOrigin.Begin); fs.ReadExactly(h);
    int method = BitConverter.ToUInt16(h, 8), nl = BitConverter.ToUInt16(h, 26), el = BitConverter.ToUInt16(h, 28);
    fs.Seek(off + 30 + nl + el, SeekOrigin.Begin);
    var raw = new byte[e.CompressedLength];
    fs.ReadExactly(raw);
    if (method == 93) return new ZstdSharp.Decompressor().Unwrap(raw).ToArray();
    if (method == 0) return raw;
    using var ds = new DeflateStream(new MemoryStream(raw), CompressionMode.Decompress);
    var ms = new MemoryStream(); ds.CopyTo(ms); return ms.ToArray();
}

// DDS (BC1/BC2/BC3/BC4/BC5/BC7 ou RGBA/BGRA sem compressão) -> PNG, só o primeiro nível de mip.
static byte[] DdsToPng(byte[] d)
{
    if (d.Length < 128 || d[0] != 'D' || d[1] != 'D' || d[2] != 'S') return null;
    int h = BitConverter.ToInt32(d, 12), w = BitConverter.ToInt32(d, 16);
    string four = System.Text.Encoding.ASCII.GetString(d, 84, 4);
    int bits = BitConverter.ToInt32(d, 88), dxgi = four == "DX10" ? BitConverter.ToInt32(d, 128) : 0;
    int start = four == "DX10" ? 148 : 128;
    var src = new ReadOnlySpan<byte>(d, start, d.Length - start);
    byte[] px; // BGRA
    if (four == "DXT1" || dxgi is 71 or 72) Bc1.Decompress(src, w, h, out px);
    else if (four == "DXT3" || dxgi is 74 or 75) Bc2.Decompress(src, w, h, out px);
    else if (four == "DXT5" || dxgi is 77 or 78) Bc3.Decompress(src, w, h, out px);
    else if (four is "ATI1" or "BC4U" || dxgi is 80) Bc4.Decompress(src, w, h, out px);
    else if (four is "ATI2" or "BC5U" || dxgi is 83) Bc5.Decompress(src, w, h, out px);
    else if (dxgi is 98 or 99) Bc7.Decompress(src, w, h, out px);
    else if (dxgi is 28 or 29 || (four == "\0\0\0\0" && bits == 32 && BitConverter.ToUInt32(d, 92) == 0xFF))
    {   // RGBA -> BGRA
        px = src.Slice(0, w * h * 4).ToArray();
        for (int i = 0; i < px.Length; i += 4) (px[i], px[i + 2]) = (px[i + 2], px[i]);
    }
    else if (dxgi is 87 or 91 || (four == "\0\0\0\0" && bits == 32)) px = src.Slice(0, w * h * 4).ToArray();
    else return null;
    return Png(px, w, h);
}

// PNG RGBA 8 bits a partir de BGRA.
static byte[] Png(byte[] bgra, int w, int h)
{
    var raw = new byte[(w * 4 + 1) * h];
    for (int y = 0; y < h; y++)
    {
        int o = y * (w * 4 + 1);
        raw[o] = 0;
        for (int x = 0; x < w; x++)
        {
            int s = (y * w + x) * 4, t = o + 1 + x * 4;
            raw[t] = bgra[s + 2]; raw[t + 1] = bgra[s + 1]; raw[t + 2] = bgra[s]; raw[t + 3] = bgra[s + 3];
        }
    }
    var ms = new MemoryStream();
    ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
    var ihdr = new byte[13];
    BE(ihdr, 0, w); BE(ihdr, 4, h); ihdr[8] = 8; ihdr[9] = 6;
    Chunk(ms, "IHDR", ihdr);
    var z = new MemoryStream();
    using (var zs = new ZLibStream(z, CompressionLevel.Optimal, true)) zs.Write(raw);
    Chunk(ms, "IDAT", z.ToArray());
    Chunk(ms, "IEND", Array.Empty<byte>());
    return ms.ToArray();
}

static void Chunk(Stream s, string type, byte[] data)
{
    var len = new byte[4]; BE(len, 0, data.Length); s.Write(len);
    var td = System.Text.Encoding.ASCII.GetBytes(type).Concat(data).ToArray();
    s.Write(td);
    var crc = new byte[4]; BE(crc, 0, (int)Crc(td)); s.Write(crc);
}

static void BE(byte[] b, int o, int v) { b[o] = (byte)(v >> 24); b[o + 1] = (byte)(v >> 16); b[o + 2] = (byte)(v >> 8); b[o + 3] = (byte)v; }

static uint Crc(byte[] b)
{
    uint c = 0xFFFFFFFF;
    foreach (byte x in b) { c ^= x; for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1; }
    return ~c;
}

delegate bool JoinDelegate(ReadOnlySpan<byte> src, out byte[] dst, out int len);
