"""Ferramenta do mod de tradução PT-BR (Mods/EcosDaQuedaTraducao).

Fluxo:
  1. run-test.ps1 -Script tests/trans-export.txt -Profile full   -> EcosDaQueda-Dev/trans-export/<packageId>/DefInjected/...
  2. python traducao.py dump <packageId> [...]                    -> traducao/en/<packageId>.tsv  (inglês, uma linha por texto)
  3. traduzir: traducao/pt/<packageId>.tsv (mesmas chaves, mesmo formato)
  4. python traducao.py build [<packageId> ...]                   -> Mods/EcosDaQuedaTraducao/Mods/<packageId>/Languages/PortugueseBrazilian/...
     (verifica chaves faltando e marcadores como {0}, [PAWN_nameDef], <color>, ->)
  5. python traducao.py todo <packageId>                          -> lista o que falta traduzir (chaves novas após update do mod)

Formato TSV: linhas "## <seção>" (ex.: "## DefInjected/GeneDef/Genes.xml", "## Keyed/Keys.xml") e depois
"<chave>\t<texto>". Coleções viram "<chave>[i]". Quebra de linha é o literal \\n. Texto sem escapar XML.
"""
import os, re, sys, glob, html
import xml.etree.ElementTree as ET

DEV = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GAME = os.path.dirname(DEV)
EXPORT = os.path.join(DEV, 'trans-export')
EN = os.path.join(DEV, 'traducao', 'en')
PT = os.path.join(DEV, 'traducao', 'pt')
MOD = os.path.join(GAME, 'Mods', 'EcosDaQuedaTraducao')
LANG = 'PortugueseBrazilian'


def mod_dir(pid):
    for d in glob.glob(os.path.join(GAME, 'Mods', '*', 'About', 'About.xml')):
        try:
            r = ET.parse(d).getroot()
            if (r.findtext('packageId') or '').strip().lower() == pid:
                return os.path.dirname(os.path.dirname(d))
        except ET.ParseError:
            pass
    return None


def parse_langdata(path):
    """Retorna lista de (chave, texto) de um arquivo LanguageData (coleções viram chave[i])."""
    out = []
    with open(path, encoding='utf-8-sig') as fh:
        txt = fh.read()
    # alguns mods têm caracteres de controle ou & soltos no texto: limpa antes de ler
    txt = re.sub(r'[\x00-\x08\x0b\x0c\x0e-\x1f]', '', txt)
    txt = re.sub(r'&(?!(amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)', '&amp;', txt)
    root = ET.fromstring(txt)
    for el in root:
        if not isinstance(el.tag, str):
            continue
        lis = el.findall('li')
        if lis:
            for i, li in enumerate(lis):
                out.append((f'{el.tag}[{i}]', (li.text or '')))
        else:
            out.append((el.tag, el.text or ''))
    return out


def clean(s):
    return s.replace('\r', '').replace('\n', '\\n').replace('\t', ' ')


def dump(pids):
    os.makedirs(EN, exist_ok=True)
    for pid in pids:
        lines = []
        base = os.path.join(EXPORT, pid, 'DefInjected')
        for f in sorted(glob.glob(os.path.join(base, '**', '*.xml'), recursive=True)):
            rel = os.path.relpath(f, os.path.join(EXPORT, pid)).replace('\\', '/')
            lines.append('## ' + rel)
            lines += [f'{k}\t{clean(v)}' for k, v in parse_langdata(f)]
        md = mod_dir(pid)
        if md:
            seen = set()
            # Keyed em inglês (da pasta mais nova primeiro: 1.7, 1.6, raiz)
            cands = sorted(glob.glob(os.path.join(md, '**', 'Languages', 'English', 'Keyed', '*.xml'), recursive=True),
                           key=lambda p: (-('1.7' in p), -('1.6' in p), p))
            for f in cands:
                if re.search(r'[\\/]1\.[0-5][\\/]', f):
                    continue
                name = os.path.basename(f)
                if name in seen:
                    continue
                seen.add(name)
                try:
                    kv = parse_langdata(f)
                except ET.ParseError as e:
                    print('  !! xml inválido', f, e)
                    continue
                lines.append('## Keyed/' + name)
                lines += [f'{k}\t{clean(v)}' for k, v in kv]
        with open(os.path.join(EN, pid + '.tsv'), 'w', encoding='utf-8', newline='\n') as fh:
            fh.write('\n'.join(lines) + '\n')
        n = sum(1 for l in lines if not l.startswith('## '))
        print(f'{pid}: {n} textos')


def read_tsv(path):
    secs, cur = {}, None
    with open(path, encoding='utf-8') as fh:
        for ln in fh:
            ln = ln.rstrip('\n')
            if not ln.strip():
                continue
            if ln.startswith('## '):
                cur = ln[3:].strip()
                secs.setdefault(cur, {})
                continue
            if '\t' not in ln:
                raise ValueError(f'{path}: linha sem TAB: {ln[:80]}')
            k, v = ln.split('\t', 1)
            secs[cur][k.strip()] = v
    return secs


MARK = re.compile(r'\{[^{}]*\}|\[[A-Za-z_][A-Za-z0-9_]*\]|</?[a-z]+[^>]*>|->|\\n')


def marks(s):
    # pronomes ([X_possessive], {X_objective}...) podem sumir ou mudar em português; o resto precisa bater
    return sorted(set(m for m in MARK.findall(s) if m != '\\n' and not re.search(r'_(possessive|objective|pronoun)\W', m)))


def read_pt(pid):
    """Tradução de um mod: pt/<pid>.tsv e/ou vários arquivos em pt/<pid>/*.tsv (mods grandes, traduzidos em partes)."""
    secs = {}
    files = ([os.path.join(PT, pid + '.tsv')] if os.path.exists(os.path.join(PT, pid + '.tsv')) else []) + sorted(glob.glob(os.path.join(PT, pid, '*.tsv')))
    for f in files:
        for s, kv in read_tsv(f).items():
            secs.setdefault(s, {}).update(kv)
    return secs


def write_section(dst, items):
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    out, groups = ['<?xml version="1.0" encoding="utf-8"?>', '<LanguageData>'], {}
    order = []
    for k, v, en in items:
        m = re.match(r'^(.*)\[(\d+)\]$', k)
        if m:
            if m.group(1) not in groups:
                groups[m.group(1)] = []
                order.append(('L', m.group(1)))
            groups[m.group(1)].append((v, en))
        else:
            order.append(('S', (k, v, en)))
    for kind, x in order:
        if kind == 'S':
            out.append(f'  <!-- EN: {html.escape(x[2], quote=False).replace("--", "- -")} -->')
            out.append(f'  <{x[0]}>{html.escape(x[1], quote=False)}</{x[0]}>')
        else:
            out.append(f'  <{x}>')
            for v, en in groups[x]:
                out.append(f'    <!-- EN: {html.escape(en, quote=False).replace("--", "- -")} -->')
                out.append(f'    <li>{html.escape(v, quote=False)}</li>')
            out.append(f'  </{x}>')
    out.append('</LanguageData>')
    with open(dst, 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('\n'.join(out) + '\n')


def build(pids, strict=False):
    if not pids:
        pids = sorted(set([os.path.splitext(os.path.basename(p))[0] for p in glob.glob(os.path.join(PT, '*.tsv'))] +
                          [os.path.basename(p) for p in glob.glob(os.path.join(PT, '*')) if os.path.isdir(p)]))
    problems = 0
    for pid in pids:
        en = read_tsv(os.path.join(EN, pid + '.tsv'))
        pt = read_pt(pid)
        root = os.path.join(MOD, 'Mods', pid, 'Languages', LANG)
        if os.path.isdir(root):
            import shutil
            shutil.rmtree(root)
        total = done = 0
        for sec, kv in en.items():
            tsec = pt.get(sec, {})
            items = []
            for k, v in kv.items():
                total += 1
                if k not in tsec:
                    # item de lista sem tradução (ex.: nome próprio): mantém o original, para a lista não encolher
                    m = re.match(r'^(.*)\[\d+\]$', k)
                    if m and any(x.startswith(m.group(1) + '[') for x in tsec):
                        items.append((k, v, v))
                        done += 1
                    continue
                t = tsec[k]
                if marks(t) != marks(v):
                    problems += 1
                    print(f'  !! {pid} {k}: marcadores diferentes {marks(v)} -> {marks(t)}')
                items.append((k, t, v))
                done += 1
            # coleções precisam estar completas
            if items:
                # o jogo trata pastas de carga como camadas: arquivos com o mesmo caminho relativo em pastas
                # diferentes se sobrescrevem. Por isso o nome do arquivo leva o packageId do mod.
                parts = sec.split('/')
                parts[-1] = pid.replace('.', '_') + '__' + parts[-1]
                write_section(os.path.join(root, *parts), items)
        extra = [k for s, kv in pt.items() for k in kv if k not in en.get(s, {})]
        for k in extra[:10]:
            print(f'  ?? {pid}: chave sem original (mod mudou?): {k}')
        print(f'{pid}: {done}/{total} traduzidos' + (f', {len(extra)} chaves obsoletas' if extra else ''))
    write_loadfolders()
    return problems


def write_loadfolders():
    subs = sorted(d for d in os.listdir(os.path.join(MOD, 'Mods')) if os.path.isdir(os.path.join(MOD, 'Mods', d)))
    out = ['<?xml version="1.0" encoding="utf-8"?>',
           '<!-- Gerado por EcosDaQueda-Dev/scripts/traducao.py build. Cada tradução só carrega com o mod ativo. -->',
           '<loadFolders>']
    for v in ('v1.6', 'v1.7'):
        out.append(f'  <{v}>')
        out.append('    <li>/</li>')
        out += [f'    <li IfModActive="{d}">Mods/{d}</li>' for d in subs]
        out.append(f'  </{v}>')
    out.append('</loadFolders>')
    with open(os.path.join(MOD, 'LoadFolders.xml'), 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('\n'.join(out) + '\n')


def todo(pid):
    en = read_tsv(os.path.join(EN, pid + '.tsv'))
    pt = read_pt(pid)
    for sec, kv in en.items():
        miss = [(k, v) for k, v in kv.items() if k not in pt.get(sec, {})]
        if miss:
            print('## ' + sec)
            for k, v in miss:
                print(f'{k}\t{v}')


def ref(pid, src):
    """Rascunho de revisão: trans-export/ref/<pid>.tsv com chave, inglês e a tradução de referência (ex.: mod de
    tradução da Workshop em <src>). Fica fora do repositório: é texto de terceiros, só serve de consulta."""
    refs = {}
    dirs = glob.glob(os.path.join(src, '**', 'Languages', 'PortugueseBrazilian*'), recursive=True)
    dirs.sort(key=lambda d: (re.search(r'[\\/](1\.\d)[\\/]', d) or [None, '0'])[1])
    for d in dirs:
        for f in glob.glob(os.path.join(d, '**', '*.xml'), recursive=True):
            try:
                for k, v in parse_langdata(f):
                    refs[k] = clean(v)
            except ET.ParseError:
                pass
    en = read_tsv(os.path.join(EN, pid + '.tsv'))
    os.makedirs(os.path.join(EXPORT, 'ref'), exist_ok=True)
    n = 0
    with open(os.path.join(EXPORT, 'ref', pid + '.tsv'), 'w', encoding='utf-8', newline='\n') as fh:
        for sec, kv in en.items():
            fh.write('## ' + sec + '\n')
            for k, v in kv.items():
                r = refs.get(k, '')
                n += bool(r)
                fh.write(f'{k}\t{v}\t{r}\n')
    print(f'{pid}: {n}/{sum(len(x) for x in en.values())} com referência')


if __name__ == '__main__':
    cmd, args = sys.argv[1], sys.argv[2:]
    if cmd == 'dump':
        dump([a.lower() for a in args])
    elif cmd == 'build':
        sys.exit(1 if build([a.lower() for a in args]) else 0)
    elif cmd == 'ref':
        ref(args[0].lower(), args[1])
    elif cmd == 'todo':
        todo(args[0].lower())
