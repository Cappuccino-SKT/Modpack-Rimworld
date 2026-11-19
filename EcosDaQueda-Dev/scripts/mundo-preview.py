"""Prévia da área de origem do conversor do mundo fixo (MUNDO.md, M2).

Lê o despejo do executor (comando worlddump: lat, lon, bioma, elevação, ...) e desenha um mapa
equirretangular com grade de 10 graus, para achar onde a América do Norte está no mapa da Terra.

Uso: python scripts/mundo-preview.py tests/out/terra-america.tsv tests/out/terra-origem.png
"""
import sys
from PIL import Image, ImageDraw

CORES = {
    'Ocean': (25, 50, 115), 'Lake': (50, 100, 190), 'SeaIce': (205, 225, 242), 'IceSheet': (242, 247, 255),
    'GlacialPlain': (190, 215, 215), 'Tundra': (150, 165, 140), 'BorealForest': (38, 100, 76), 'ColdBog': (90, 115, 90),
    'TemperateForest': (50, 140, 50), 'TemperateSwamp': (76, 115, 50), 'Grasslands': (150, 190, 76),
    'AridShrubland': (190, 178, 100), 'Desert': (230, 205, 128), 'ExtremeDesert': (242, 165, 90),
    'TropicalRainforest': (12, 115, 25), 'TropicalSwamp': (38, 90, 38),
}


def main(src, out, lat0=-10.0, lat1=90.0, lon0=-180.0, lon1=0.0, px=8):
    w, h = int((lon1 - lon0) * px), int((lat1 - lat0) * px)
    img = Image.new('RGB', (w, h), (0, 0, 0))
    d = ImageDraw.Draw(img)
    for line in open(src, encoding='utf-8'):
        p = line.rstrip('\n').split('\t')
        if len(p) < 3:
            continue
        lat, lon, b = float(p[0]), float(p[1]), p[2]
        x, y = (lon - lon0) * px, (lat1 - lat) * px
        c = CORES.get(b, (220, 40, 200))
        d.rectangle([x - 2, y - 2, x + 2, y + 2], fill=c)
    for la in range(int(lat0), int(lat1) + 1, 10):
        y = (lat1 - la) * px
        d.line([(0, y), (w, y)], fill=(255, 255, 255), width=1)
        d.text((2, y + 2), f'{la}', fill=(255, 255, 0))
    for lo in range(int(lon0), int(lon1) + 1, 10):
        x = (lo - lon0) * px
        d.line([(x, 0), (x, h)], fill=(255, 255, 255), width=1)
        d.text((x + 2, 2), f'{lo}', fill=(255, 255, 0))
    img = img.resize((w // 2, h // 2))
    img.save(out)
    print(out, img.size)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
