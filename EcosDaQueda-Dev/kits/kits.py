"""Editor de kits das facções (Ecos da Queda), fora do jogo.

Uso (na pasta EcosDaQueda-Dev):
  python kits/kits.py              abre o editor no navegador (http://127.0.0.1:8765)
  python kits/kits.py build        gera os Defs: Kits/EQ_Kits.xml (coleções), Kits/EQ_Unidades.xml (unidades) e
                                   Pesquisa/EQ_Pesquisa.xml (árvores de pesquisa, página /pesquisa)
  python kits/kits.py icons        extrai os ícones que faltam (kits/icons) a partir do catálogo
  python kits/kits.py catalog      exporta o catálogo pelo executor de testes (jogo sem janela) e depois os ícones

Coleção = uma unidade (ou mais) e as variações de kit fixo dela; o jogo sorteia uma variação por pawn (peso).
Arquivos: kits/colecoes/<id>.json (versionados), kits/kitcatalog.json e kits/icons/ (gerados, fora do git).
Os personagens salvos no Character Creator (pasta MCEPawns do jogo) podem ser importados como variações.
"""
import json
import os
import re
import subprocess
import sys
import threading
import webbrowser
import xml.etree.ElementTree as ET
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import unquote, urlparse

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pesquisa  # noqa: E402

KITS = os.path.dirname(os.path.abspath(__file__))
DEV = os.path.dirname(KITS)
GAME = os.path.dirname(DEV)
COLS = os.path.join(KITS, 'colecoes')
UNITS = os.path.join(KITS, 'unidades')
ICONS = os.path.join(KITS, 'icons')
CATALOG = os.path.join(KITS, 'kitcatalog.json')
OUT_XML = os.path.join(GAME, 'Mods', 'EcosDaQueda', 'Defs', 'Kits', 'EQ_Kits.xml')
OUT_UNITS = os.path.join(GAME, 'Mods', 'EcosDaQueda', 'Defs', 'Kits', 'EQ_Unidades.xml')
MCE = os.path.join(os.environ.get('USERPROFILE', ''), 'AppData', 'LocalLow', 'Ludeon Studios',
                   'RimWorld by Ludeon Studios', 'MCEPawns')
PORT = 8765
ID_RE = re.compile(r'^[A-Za-z0-9_]+$')
QUALITIES = ['Awful', 'Poor', 'Normal', 'Good', 'Excellent', 'Masterwork', 'Legendary']


# ---------------------------------------------------------------- coleções
def load_cols():
    os.makedirs(COLS, exist_ok=True)
    out = []
    for f in sorted(os.listdir(COLS)):
        if f.endswith('.json'):
            with open(os.path.join(COLS, f), encoding='utf-8') as fh:
                out.append(json.load(fh))
    return out


def save_col(col):
    if not ID_RE.match(col.get('id', '')):
        raise ValueError('id inválido (use letras, números e _): %r' % col.get('id'))
    os.makedirs(COLS, exist_ok=True)
    with open(os.path.join(COLS, col['id'] + '.json'), 'w', encoding='utf-8', newline='\n') as fh:
        json.dump(col, fh, ensure_ascii=False, indent=1)
        fh.write('\n')


def load_units():
    os.makedirs(UNITS, exist_ok=True)
    out = []
    for f in sorted(os.listdir(UNITS)):
        if f.endswith('.json'):
            with open(os.path.join(UNITS, f), encoding='utf-8') as fh:
                out.append(json.load(fh))
    return out


def save_units(fac):
    if not ID_RE.match(fac.get('faction', '')):
        raise ValueError('facção inválida: %r' % fac.get('faction'))
    for u in fac.get('units', []):
        if not ID_RE.match(u.get('id', '')):
            raise ValueError('nome de unidade inválido (letras, números e _): %r' % u.get('id'))
    os.makedirs(UNITS, exist_ok=True)
    with open(os.path.join(UNITS, fac['faction'] + '.json'), 'w', encoding='utf-8', newline='\n') as fh:
        json.dump(fac, fh, ensure_ascii=False, indent=1)
        fh.write('\n')


def unit_def(faction, uid):
    """EQ_Faction_Forja + cavaleiro -> EQ_U_Forja_cavaleiro (prefixo próprio: nunca colide com as unidades antigas)."""
    return 'EQ_U_%s_%s' % (faction.replace('EQ_Faction_', ''), uid)


# ---------------------------------------------------------------- Character Creator
def mce_list():
    if not os.path.isdir(MCE):
        return []
    return sorted(f[:-4] for f in os.listdir(MCE) if f.endswith('.xml'))


def _rgba_to_hex(s):
    m = re.match(r'RGBA\(([\d.]+),\s*([\d.]+),\s*([\d.]+)', s or '')
    if not m:
        return None
    return '#' + ''.join('%02x' % round(float(x) * 255) for x in m.groups())


def _item(li, with_count=False):
    d = {'def': li.findtext('def')}
    for k in ('stuff', 'quality'):
        v = li.findtext(k)
        if v and v != 'null':
            d[k] = v
    st = li.findtext('styleDef')
    if st and st != 'null':
        d['style'] = st
    if li.findtext('colorActive') == 'True':
        c = _rgba_to_hex(li.findtext('color'))
        if c:
            d['color'] = c
    if with_count:
        d['count'] = int(li.findtext('stackCount') or 1)
    return d


def mce_import(name, catalog_kinds=None):
    """Personagem salvo no Character Creator -> variação de kit."""
    path = os.path.join(MCE, name + '.xml')
    root = ET.parse(path).getroot()
    pawn = root.find('pawn')
    var = {'name': name.split('-')[-1], 'weight': 1, 'source': 'MCEPawns/' + name, 'apparel': [], 'weapons': [], 'inventory': []}
    xeno = pawn.findtext('.//xenotype')
    if xeno:
        var['xenotype'] = xeno
    for li in pawn.findall('./apparel/wornApparel/innerList/li'):
        var['apparel'].append(_item(li))
    for li in pawn.findall('./equipment/equipment/innerList/li'):
        var['weapons'].append(_item(li))
    for li in pawn.findall('./inventory/innerContainer/innerList/li'):
        it = _item(li, with_count=True)
        kind = (catalog_kinds or {}).get(it['def'])
        if kind in ('ranged', 'melee'):     # arma no inventário = arma reserva
            it.pop('count', None)
            var['weapons'].append(it)
        else:
            var['inventory'].append(it)
    return var


# ---------------------------------------------------------------- build (JSON -> Defs)
def _hex_to_color(h):
    h = h.lstrip('#')
    r, g, b = (int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))
    return '(%.3f, %.3f, %.3f, 1)' % (r, g, b)


def _x(s):
    return (str(s).replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;'))


def build():
    cat = {}
    c = {}
    kinds = set()
    if os.path.exists(CATALOG):
        with open(CATALOG, encoding='utf-8') as fh:
            c = json.load(fh)
        cat = {i['def']: i for i in c['items']}
        kinds = {k['def'] for k in c['pawnkinds']}
    # unidades prontas do editor: existem como PawnKind (EQ_Unidades.xml) e puxam as coleções que escolheram
    links = {}
    for fac in load_units():
        for u in fac.get('units', []):
            if u.get('status') == 'pronta':
                d = unit_def(fac['faction'], u['id'])
                kinds.add(d)
                for k in u.get('kits', []):
                    links.setdefault(k, []).append(d)
    report, lines = [], ['<?xml version="1.0" encoding="utf-8"?>',
                         '<!-- GERADO por EcosDaQueda-Dev/kits/kits.py (editor de kits). Não editar à mão: mude em kits/colecoes/*.json. -->',
                         '<Defs>']
    cols = load_cols()
    for col in cols:
        vs = [v for v in col.get('variations', []) if v.get('apparel') or v.get('weapons') or v.get('inventory')]
        if not vs:
            report.append('%s: sem variações com itens (pulada)' % col['id'])
            continue
        pks = list(dict.fromkeys(col.get('pawnKinds', []) + links.get(col['id'], [])))
        for pk in pks:
            if kinds and pk not in kinds:
                report.append('%s: unidade não existe no catálogo: %s' % (col['id'], pk))
        if not pks:
            report.append('%s: nenhuma unidade ligada (o kit não é usado no jogo ainda)' % col['id'])
        lines += ['', '  <EcosDaQueda.KitCollectionDef>',
                  '    <defName>EQ_Kit_%s</defName>' % col['id'],
                  '    <label>%s</label>' % _x(col.get('label') or col['id'])]
        if pks:
            lines.append('    <pawnKinds>' + ''.join('<li>%s</li>' % k for k in pks) + '</pawnKinds>')
        if col.get('chance', 1) != 1:
            lines.append('    <chance>%s</chance>' % col['chance'])
        lines.append('    <variations>')
        for v in vs:
            lines += ['      <li>', '        <label>%s</label>' % _x(v.get('name', '')),
                      '        <weight>%s</weight>' % v.get('weight', 1)]
            has_gun = any(cat.get(w['def'], {}).get('kind') == 'ranged' for w in v.get('weapons', []))
            has_ammo = any(i['def'].startswith('Ammo_') for i in v.get('inventory', []))
            if has_gun and not has_ammo and any(cat.get(w['def'], {}).get('pkg', '').lower() == 'ceteam.combatextended'
                                                or w['def'].startswith('CE_') for w in v.get('weapons', [])):
                report.append('%s / %s: arma de fogo sem munição no inventário (Combat Extended)' % (col['id'], v.get('name')))
            for tag in ('apparel', 'weapons', 'inventory'):
                items = v.get(tag, [])
                if not items:
                    continue
                lines.append('        <%s>' % tag)
                for it in items:
                    if cat and it['def'] not in cat:
                        report.append('%s / %s: item não existe no catálogo: %s' % (col['id'], v.get('name'), it['def']))
                    parts = ['<thing>%s</thing>' % it['def']]
                    if it.get('stuff'):
                        parts.append('<stuff>%s</stuff>' % it['stuff'])
                    if it.get('quality', 'Normal') != 'Normal':
                        parts.append('<quality>%s</quality>' % it['quality'])
                    if it.get('color'):
                        parts.append('<color>%s</color>' % _hex_to_color(it['color']))
                    if it.get('style'):
                        parts.append('<style>%s</style>' % it['style'])
                    if tag == 'inventory' and it.get('count', 1) != 1:
                        parts.append('<count>%d</count>' % it['count'])
                    if it.get('chance', 1) != 1:
                        parts.append('<chance>%s</chance>' % it['chance'])
                    lines.append('          <li>' + ''.join(parts) + '</li>')
                lines.append('        </%s>' % tag)
            lines.append('      </li>')
        lines += ['    </variations>', '  </EcosDaQueda.KitCollectionDef>']
    lines += ['', '</Defs>', '']
    os.makedirs(os.path.dirname(OUT_XML), exist_ok=True)
    with open(OUT_XML, 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('\n'.join(lines))
    head = '%d coleções -> %s' % (len(cols), os.path.relpath(OUT_XML, GAME))
    head += '\n' + build_units(report, {x['id'] for x in cols})
    rh, rr = pesquisa.build_research(c)
    head += '\n' + rh
    report += rr
    return head + ('\n' + '\n'.join('  ! ' + r for r in report) if report else '\n  sem avisos')


GROUPS = ['Combat', 'Peaceful', 'Settlement', 'TraderGuard', 'Trader']
ROLE_PARENT = {'combatente': 'MercenaryMidTierBase', 'civil': 'MercenaryMidTierBase', 'mercador': 'MercenaryMidTierBase',
               'lider': 'MercenaryEliteTierBase'}


def build_units(report, col_ids):
    """kits/unidades/*.json -> PawnKindDefs EQ_U_* e um UnitRosterDef por facção (grupos, líder, membro básico)."""
    lines = ['<?xml version="1.0" encoding="utf-8"?>',
             '<!-- GERADO por EcosDaQueda-Dev/kits/kits.py (aba Unidades do editor). Não editar à mão: mude em kits/unidades/*.json. -->',
             '<Defs>']
    n = 0
    for fac in load_units():
        f = fac['faction']
        ready = [u for u in fac.get('units', []) if u.get('status') == 'pronta']
        if not ready:
            continue
        roster = []
        for u in ready:
            d = unit_def(f, u['id'])
            n += 1
            role = u.get('role', 'combatente')
            if not u.get('kits'):
                report.append('%s: unidade sem coleção de kit (sai com o equipamento genérico de mercenário)' % d)
            for k in u.get('kits', []):
                if k not in col_ids:
                    report.append('%s: coleção de kit não existe: %s' % (d, k))
            if not any((u.get('groups') or {}).get(g) for g in GROUPS) and role != 'lider':
                report.append('%s: não aparece em nenhum grupo (só existe como unidade)' % d)
            lines.append('')
            if u.get('notes'):
                lines.append('  <!-- %s -->' % _x(u['notes']).replace('--', '—'))
            lines += ['  <PawnKindDef ParentName="%s">' % ROLE_PARENT.get(role, 'MercenaryMidTierBase'),
                      '    <defName>%s</defName>' % d,
                      '    <label>%s</label>' % _x(u.get('label') or u['id']),
                      '    <defaultFactionDef>%s</defaultFactionDef>' % f,
                      '    <combatPower>%s</combatPower>' % u.get('points', 60)]
            if u.get('labelPlural'):   # sem plural, o jogo pluraliza sozinho
                lines.append('    <labelPlural>%s</labelPlural>' % _x(u['labelPlural']))
            if role in ('civil', 'mercador'):
                lines.append('    <isFighter>false</isFighter>')
            if role == 'mercador':
                lines.append('    <trader>true</trader>')
            if role == 'lider':
                lines.append('    <factionLeader>true</factionLeader>')
            if u.get('minAge'):
                lines.append('    <minGenerationAge>%d</minGenerationAge>' % u['minAge'])
            if u.get('maxAge'):
                lines.append('    <maxGenerationAge>%d</maxGenerationAge>' % u['maxAge'])
            xs = {k: v for k, v in (u.get('xenos') or {}).items() if v}
            if xs:
                tot = sum(xs.values())
                lines += ['    <useFactionXenotypes>false</useFactionXenotypes>', '    <xenotypeSet Inherit="False">', '      <xenotypeChances>']
                lines += ['        <%s>%.3f</%s>' % (k, v / tot, k) for k, v in xs.items()]
                lines += ['      </xenotypeChances>', '    </xenotypeSet>']
            lines.append('  </PawnKindDef>')
            for g in GROUPS:
                w = (u.get('groups') or {}).get(g)
                if w:
                    roster.append((d, g, w))
        leaders = [unit_def(f, u['id']) for u in ready if u.get('role') == 'lider']
        basic = [unit_def(f, u['id']) for u in ready if u.get('basic')]
        lines += ['', '  <EcosDaQueda.UnitRosterDef>',
                  '    <defName>EQ_Elenco_%s</defName>' % f.replace('EQ_Faction_', ''),
                  '    <faction>%s</faction>' % f]
        if fac.get('replaceExisting'):
            lines.append('    <replaceExisting>true</replaceExisting>')
        if leaders:
            lines.append('    <leaders>' + ''.join('<li>%s</li>' % k for k in leaders) + '</leaders>')
        if basic:
            lines.append('    <basicMember>%s</basicMember>' % basic[0])
        if roster:
            lines.append('    <entries>')
            lines += ['      <li><kind>%s</kind><group>%s</group><weight>%s</weight></li>' % r for r in roster]
            lines.append('    </entries>')
        lines.append('  </EcosDaQueda.UnitRosterDef>')
        for g in ('Combat', 'Settlement'):
            if fac.get('replaceExisting') and not any(r[1] == g for r in roster):
                report.append('%s: nenhuma unidade nova no grupo %s; ficam as antigas nele' % (f, g))
    lines += ['', '</Defs>', '']
    with open(OUT_UNITS, 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('\n'.join(lines))
    return '%d unidades prontas -> %s' % (n, os.path.relpath(OUT_UNITS, GAME))


# ---------------------------------------------------------------- catálogo e ícones
def icons():
    proj = os.path.join(KITS, 'tool')
    r = subprocess.run(['dotnet', 'build', proj, '-c', 'Release', '-v', 'q'], capture_output=True, text=True)
    if r.returncode != 0:
        print(r.stdout[-3000:])
        sys.exit('falha ao compilar o extrator de ícones')
    dll = os.path.join(proj, 'bin', 'Release', 'net10.0', 'KitIcons.dll')
    subprocess.run(['dotnet', dll, CATALOG, GAME, ICONS], check=True)


def catalog():
    ps = os.path.join(DEV, 'scripts', 'run-test.ps1')
    subprocess.run(['powershell', '-ExecutionPolicy', 'Bypass', '-File', ps, '-Script',
                    os.path.join(DEV, 'tests', 'kitcatalog.txt')], check=True)
    icons()


# ---------------------------------------------------------------- servidor
class Handler(SimpleHTTPRequestHandler):
    def log_message(self, *a):
        pass

    def _json(self, obj, code=200):
        b = json.dumps(obj, ensure_ascii=False).encode('utf-8')
        self.send_response(code)
        self.send_header('Content-Type', 'application/json; charset=utf-8')
        self.send_header('Content-Length', str(len(b)))
        self.end_headers()
        self.wfile.write(b)

    def _file(self, path, ctype):
        if not os.path.isfile(path):
            self.send_error(404)
            return
        with open(path, 'rb') as fh:
            b = fh.read()
        self.send_response(200)
        self.send_header('Content-Type', ctype)
        self.send_header('Content-Length', str(len(b)))
        self.send_header('Cache-Control', 'max-age=3600' if ctype == 'image/png' else 'no-store')
        self.end_headers()
        self.wfile.write(b)

    def _body(self):
        n = int(self.headers.get('Content-Length', 0))
        return json.loads(self.rfile.read(n).decode('utf-8')) if n else None

    def do_GET(self):
        p = unquote(urlparse(self.path).path)
        if p in ('/', '/index.html'):
            return self._file(os.path.join(KITS, 'editor.html'), 'text/html; charset=utf-8')
        if p in ('/pesquisa', '/pesquisa.html'):
            return self._file(os.path.join(KITS, 'pesquisa.html'), 'text/html; charset=utf-8')
        if p == '/api/pesquisa':
            return self._json({'arvores': pesquisa.load_trees(), 'catalog': os.path.exists(CATALOG)})
        if p == '/api/catalog':
            return self._file(CATALOG, 'application/json; charset=utf-8')
        if p == '/api/state':
            return self._json({'colecoes': load_cols(), 'unidades': load_units(), 'mce': mce_list(), 'catalog': os.path.exists(CATALOG)})
        m = re.match(r'^/icons/([^/]+)\.png$', p)
        if m:
            return self._file(os.path.join(ICONS, os.path.basename(m.group(1)) + '.png'), 'image/png')
        m = re.match(r'^/mce/([^/]+)\.png$', p)
        if m:
            return self._file(os.path.join(MCE, os.path.basename(m.group(1)) + '.png'), 'image/png')
        m = re.match(r'^/api/mce/([^/]+)$', p)
        if m:
            name = os.path.basename(m.group(1))
            kinds = {}
            if os.path.exists(CATALOG):
                with open(CATALOG, encoding='utf-8') as fh:
                    kinds = {i['def']: i['kind'] for i in json.load(fh)['items']}
            try:
                return self._json(mce_import(name, kinds))
            except Exception as e:
                return self._json({'error': str(e)}, 400)
        self.send_error(404)

    def do_PUT(self):
        p = unquote(urlparse(self.path).path)
        m = re.match(r'^/api/unidades/([A-Za-z0-9_]+)$', p)
        if m:
            try:
                save_units(self._body())
            except ValueError as e:
                return self._json({'error': str(e)}, 400)
            return self._json({'ok': True})
        m = re.match(r'^/api/pesquisa/([A-Za-z0-9_]+)$', p)
        if m:
            try:
                pesquisa.save_tree(self._body(), m.group(1))
            except ValueError as e:
                return self._json({'error': str(e)}, 400)
            return self._json({'ok': True})
        m = re.match(r'^/api/colecao/([A-Za-z0-9_]+)$', p)
        if not m:
            return self.send_error(404)
        col = self._body()
        try:
            if col.get('id') != m.group(1):     # renomeou: apaga o arquivo antigo
                old = os.path.join(COLS, m.group(1) + '.json')
                save_col(col)
                if os.path.exists(old):
                    os.remove(old)
            else:
                save_col(col)
        except ValueError as e:
            return self._json({'error': str(e)}, 400)
        self._json({'ok': True})

    def do_DELETE(self):
        p = unquote(urlparse(self.path).path)
        m = re.match(r'^/api/pesquisa/([A-Za-z0-9_]+)$', p)
        if m:
            pesquisa.delete_tree(m.group(1))
            return self._json({'ok': True})
        m = re.match(r'^/api/colecao/([A-Za-z0-9_]+)$', p)
        if not m:
            return self.send_error(404)
        f = os.path.join(COLS, m.group(1) + '.json')
        if os.path.exists(f):
            os.remove(f)
        self._json({'ok': True})

    def do_POST(self):
        if urlparse(self.path).path == '/api/build':
            try:
                return self._json({'report': build()})
            except Exception as e:
                return self._json({'report': 'ERRO: %s' % e}, 500)
        self.send_error(404)


def serve(open_browser=True):
    if not os.path.exists(CATALOG):
        print('Falta o catálogo: rode "python kits/kits.py catalog" (abre o jogo sem janela, ~2 min).')
    srv = ThreadingHTTPServer(('127.0.0.1', PORT), Handler)
    url = 'http://127.0.0.1:%d/' % PORT
    print('Editor de kits em', url, '(Ctrl+C para sair)')
    if open_browser:
        threading.Timer(0.5, lambda: webbrowser.open(url)).start()
    try:
        srv.serve_forever()
    except KeyboardInterrupt:
        pass


if __name__ == '__main__':
    cmd = sys.argv[1] if len(sys.argv) > 1 else 'serve'
    if cmd == 'build':
        print(build())
    elif cmd == 'icons':
        icons()
    elif cmd == 'catalog':
        catalog()
    elif cmd == 'serve':
        serve('--no-browser' not in sys.argv)
    else:
        sys.exit(__doc__)
