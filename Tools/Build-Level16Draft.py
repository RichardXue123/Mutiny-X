"""Author Level 16 revision 05: open the flat freighter's forward bay for two robots."""
from collections import deque
from pathlib import Path
import csv
import json
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Docs/02-LevelAndWorld/04-ObjectsAndSpawns/Level16Draft'
W, H, WATER, SPACE_X = 80, 27, 24, 52
terrain = [['-' for _ in range(W)] for _ in range(H)]
background = [['-' for _ in range(W)] for _ in range(H)]


def decode(text):
    cells = []
    for token in text.split(','):
        name, sep, count = token.partition(':')
        cells.extend([name] * (int(count) if sep else 1))
    return cells


# Fourth level's complete left ship: upper crow's nest, middle spars and main
# deck. Keep its bottom and waterline, not only the top decorative hull strip.
original = ET.parse(ROOT / 'Assets/Mutiny/Data/Levels/level_1_04.xml').getroot()
for y, row in enumerate(original.findall('row')):
    if y < 17:  # Existing animated galaxy supplies the waterline instead of sea ripples.
        terrain[y + 7][2:25] = decode(row.text)[:23]
for y, row in enumerate(original.findall('bgRow')):
    if y > 0:
        background[y + 7][2:25] = decode(row.text)[:23]

# Flattened disk hull, twin forward mandibles and offset cockpit.
# The underside ends at y=21, three cells clear of the galaxy waterline.
for y, lo, hi in ((13, 71, 74), (14, 60, 75), (15, 57, 76),
                  (16, 52, 77), (17, 58, 77), (18, 58, 76),
                  (19, 52, 74), (20, 58, 71)):
    for x in range(lo, hi + 1):
        terrain[y][x] = 'ship_tile_1' if (x + y) % 2 == 0 else 'ship_tile_2'
        if terrain[y-1][x] == '-':
            terrain[y][x] = 'ship_top_middle'
for x in (61, 64, 67, 70):
    terrain[17][x] = 'cannon_port_2'
for x in (72, 73, 74):
    terrain[14][x] = 'cannon_port_2'
terrain[16][77] = 'cannon_port_1'
terrain[17][77] = 'cannon_port_1'

# Forward bay is open to the notch. Two robots occupy its floor. The roof hatch
# provides a direct upward exit as well as the existing forward boarding route.
for y in (17, 18):
    for x in range(58, 65):
        terrain[y][x] = '-'
        background[y][x] = 'cave_middle_2'
for y in range(14, 19):
    for x in (63, 64):
        terrain[y][x] = '-'
        background[y][x] = '-'

# Unsymmetrical rock clusters: independent silhouettes, variable widths and
# depths, branching routes. No grass caps, regular staircase or mirrored grid.
asteroids = [
    (26, 19, [(1, 3), (0, 4), (1, 3)]),
    (34, 21, [(0, 2), (1, 2)]),
    (43, 19, [(1, 4), (0, 5), (1, 3)]),
    (30, 14, [(1, 5), (0, 5), (1, 3)]),
    (40, 11, [(2, 5), (0, 6), (1, 5)]),
    (48, 16, [(0, 2), (0, 1)]),
    (25, 8, [(1, 3), (0, 3), (1, 2)]),
    (33, 6, [(1, 4), (0, 5), (1, 4)]),
    (45, 4, [(1, 3), (0, 4), (1, 2)]),
    (40, 2, [(0, 1)]), (28, 3, [(0, 1), (1, 1)]),
    (38, 18, [(0, 1)]), (33, 10, [(0, 1)]),
]
for ox, oy, spans in asteroids:
    for dy, (lo, hi) in enumerate(spans):
        for dx in range(lo, hi + 1):
            assert terrain[oy+dy][ox+dx] == '-'
            tile = 'earth_edge_middle_1' if (dx+dy) % 2 else 'earth_edge_middle_2'
            if dx == lo:
                tile = 'earth_edge_left'
            elif dx == hi:
                tile = 'earth_edge_right'
            if dy == len(spans)-1:
                tile = 'earth_bottom_right' if dx == lo else 'earth_bottom_left' if dx == hi else 'eartyh_bottom_middle'
            terrain[oy+dy][ox+dx] = tile

players = [(9, 11), (7, 16), (19, 16), (3, 20), (12, 21)]
robots = [(73, 12), (59, 18), (63, 18), (68, 13), (53, 15), (56, 15), (77, 15), (59, 14)]
objects = [dict(type='potentialWeapons', x=44, y=2, luck=1, maxChests=2,
                cherryBomb=3, dynamite=2, banana=2, parachuteBomb=2, seagull=1)]
for points, normal, captain, luck in (
    (players, 'redPirate', 'redPirateCaptain', 5),
    (robots, 'Robot', 'RobotCaptain', 50),
):
    for i, (x, y) in enumerate(points):
        objects.append(dict(type=captain if i == 0 else normal, x=x, y=y,
                            luck=luck, maxChests=1, cannon=10, boulder=10,
                            cherryBomb=2, dynamite=1))
objects.append(dict(type='water', x=0, y=WATER, luck=10, maxChests=3))
for x in range(W):
    if not any(terrain[y][x] != '-' for y in range(WATER)):
        background[0][x] = 'antichest'


def rle(row):
    result, start = [], 0
    while start < len(row):
        end = start + 1
        while end < len(row) and row[end] == row[start]:
            end += 1
        count = end - start
        result.append(row[start] + (f':{count}' if count > 1 else ''))
        start = end
    return ','.join(result)


catalog = {r['tile_name']: r for r in csv.DictReader(
    (ROOT / 'Assets/Mutiny/Resources/Data/Tiles/tile-mapping.csv').open(encoding='utf-8-sig'))}
assert all(t == '-' or t in catalog for layer in (terrain, background) for row in layer for t in row)
assert len(players) == 5 and len(robots) == 8 and len(set(players + robots)) == 13
for x, y in players + robots:
    assert terrain[y][x] == '-' and terrain[y - 1][x] == '-', (x, y, 'body/head blocked')
    assert terrain[y + 1][x] != '-', (x, y, 'no floor')
unseen = {(x, y) for y in range(H) for x in range(W) if terrain[y][x] != '-' and 'ripple' not in terrain[y][x]}
components = []
while unseen:
    seed = next(iter(unseen)); queue = deque([seed]); unseen.remove(seed); component = []
    while queue:
        x, y = queue.popleft(); component.append((x, y))
        for p in ((x-1, y), (x+1, y), (x, y-1), (x, y+1)):
            if p in unseen:
                unseen.remove(p); queue.append(p)
    components.append(dict(cells=len(component), min_x=min(p[0] for p in component),
                           max_x=max(p[0] for p in component), min_y=min(p[1] for p in component),
                           max_y=max(p[1] for p in component)))
components.sort(key=lambda c: c['min_x'])
assert len(asteroids) == 13
level = ET.Element('level', height=str(H), width=str(W), players='1',
                   name='Asteroid Passage - Freighter Bay 05', gravityScale='0.5',
                   visualTheme='space', spaceThemeMinX=str(SPACE_X))
level.append(ET.Comment(' User-authored revision 05: flat freighter with an open forward bay and roof hatch; two robots inside, eight total; original wooden ship and asteroid belt retained. '))
for row in terrain:
    ET.SubElement(level, 'row').text = rle(row)
for row in background:
    ET.SubElement(level, 'bgRow').text = rle(row)
for obj in objects:
    ET.SubElement(level, 'obj', {k: str(v) for k, v in obj.items()})
for line, numbers in ((1, range(1, 5)), (3, range(5, 9))):
    ET.SubElement(level, 'speechAudio', line=str(line),
                  clips=','.join(f'daftpunk_{n:02d}' for n in numbers), gapSeconds='0.5')
ET.indent(level, space='  ')
xml = ET.tostring(level, encoding='unicode') + '\n'
for directory in ('Assets/Mutiny/Data/Levels', 'Assets/Mutiny/Resources/Data/Levels'):
    (ROOT / directory / 'level_1_16.xml').write_text(xml, encoding='utf-8', newline='\n')
OUT.mkdir(parents=True, exist_ok=True)
(OUT / 'layout.json').write_text(json.dumps(dict(width=W, height=H, gravityScale=0.5,
    visualTheme='space', spaceThemeMinX=SPACE_X, waterY=WATER, terrain=terrain,
    background=background, objects=objects, components=components,
    asteroidClusters=[dict(x=x,y=y,spans=s) for x,y,s in asteroids]), indent=2), encoding='utf-8')
print(json.dumps(dict(width=W, height=H, asteroid_clusters=len(asteroids),
    players=len(players), enemies=len(robots), terrain_tiles=sum(t != '-' for row in terrain for t in row)), indent=2))
