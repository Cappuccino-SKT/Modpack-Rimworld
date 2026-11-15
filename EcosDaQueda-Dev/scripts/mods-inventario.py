# Inventário dos mods instalados (para documentar o modpack).
# Uso: python -I mods-inventario.py <raiz do jogo> <saida.json>
# Para cada mod em Mods/: nome, packageId, autor, descrição, se está ativo, DLLs,
# contagem de tipos de Def (e quais não existem no jogo base), patches, se tem Source.
import json, os, re, sys, io
from collections import Counter
import xml.etree.ElementTree as ET

root, out = sys.argv[1], sys.argv[2]
mods_dir = os.path.join(root, 'Mods')

def read(p):
    return io.open(p, encoding='utf-8-sig', errors='replace').read()

def text(t, tag):
    m = re.search(r'<%s>\s*(.*?)\s*</%s>' % (tag, tag), t, re.S)
    return re.sub(r'\s+', ' ', m.group(1)).strip() if m else ''

def def_tags(folder):
    c = Counter()
    for dp, _, fs in os.walk(folder):
        for f in fs:
            if not f.lower().endswith('.xml'):
                continue
            try:
                r = ET.fromstring(read(os.path.join(dp, f)).encode('utf-8'))
            except Exception:
                continue
            if r.tag != 'Defs':
                continue
            for ch in r:
                if isinstance(ch.tag, str):
                    c[ch.tag] += 1
    return c

def version_dirs(mdir):
    """Pastas carregadas (aproximação): raiz, Common, a versão mais nova (1.7 > 1.6) e o que o loadFolders citar."""
    ds = [mdir]
    for v in ('Common', 'common'):
        if os.path.isdir(os.path.join(mdir, v)):
            ds.append(os.path.join(mdir, v))
    for v in ('1.7', '1.6', '1.5'):
        if os.path.isdir(os.path.join(mdir, v)):
            ds.append(os.path.join(mdir, v))
            break
    lf = [f for f in os.listdir(mdir) if f.lower() == 'loadfolders.xml']
    if lf:
        t = read(os.path.join(mdir, lf[0]))
        m = re.search(r'<v1\.7>(.*?)</v1\.7>', t, re.S) or re.search(r'<v1\.6>(.*?)</v1\.6>', t, re.S)
        if m:
            ds = [mdir]
            for li in re.findall(r'<li[^>]*>([^<]*)</li>', m.group(1)):
                li = li.strip().strip('/\\')
                p = os.path.join(mdir, li) if li else mdir
                if os.path.isdir(p) and p not in ds:
                    ds.append(p)
    return ds

# tipos de Def do jogo base
vanilla = Counter()
for d in os.listdir(os.path.join(root, 'Data')):
    p = os.path.join(root, 'Data', d, 'Defs')
    if os.path.isdir(p):
        vanilla |= def_tags(p)

cfg = read(os.path.join(root, 'modpack', 'ModsConfig.xml'))
active = [x.lower() for x in re.findall(r'<li>([^<]+)</li>', cfg.split('</activeMods>')[0])]

res = []
for d in sorted(os.listdir(mods_dir)):
    mdir = os.path.join(mods_dir, d)
    ab = os.path.join(mdir, 'About', 'About.xml')
    if not os.path.isfile(ab):
        continue
    t = read(ab)
    try:
        meta = ET.fromstring(t.encode('utf-8'))
        top = lambda tag: re.sub(r'\s+', ' ', (meta.findtext(tag) or '')).strip()
    except Exception:
        top = lambda tag: text(t, tag)
    pid = top('packageId').lower()
    defs, dlls, patches = Counter(), set(), 0
    for vd in version_dirs(mdir):
        if os.path.isdir(os.path.join(vd, 'Defs')):
            defs += def_tags(os.path.join(vd, 'Defs'))
        a = os.path.join(vd, 'Assemblies')
        if os.path.isdir(a):
            dlls |= {f for f in os.listdir(a) if f.lower().endswith('.dll')}
        pp = os.path.join(vd, 'Patches')
        if os.path.isdir(pp):
            patches += sum(len([f for f in fs if f.lower().endswith('.xml')]) for _, _, fs in os.walk(pp))
    deps = re.search(r'<modDependencies>(.*?)</modDependencies>', t, re.S)
    res.append({
        'folder': d, 'name': top('name'), 'packageId': pid, 'author': top('author'),
        'active': active.index(pid) + 1 if pid in active else None,
        'description': re.sub(r'\[/?[a-z0-9=]+\]', '', top('description'))[:1500],
        'deps': re.findall(r'<packageId>([^<]+)</packageId>', deps.group(1)) if deps else [],
        'dlls': sorted(dlls), 'hasSource': os.path.isdir(os.path.join(mdir, 'Source')),
        'patches': patches, 'defs': dict(defs.most_common()),
        'newDefTypes': sorted(k for k in defs if k not in vanilla),
    })
io.open(out, 'w', encoding='utf-8').write(json.dumps(res, ensure_ascii=False, indent=1))
print(len(res), 'mods;', sum(1 for r in res if r['active']), 'ativos')
