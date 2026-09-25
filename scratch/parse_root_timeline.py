with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

# Parse top-level tags by scanning start tags
# A top-level tag is <item type="..." ...>
# If type is DefineSpriteTag, it has nested items until </item> matching that tag.

pos = content.find('<tags>')
if pos == -1:
    pos = 0

print("Scanning tags...")
# Let's write a parser that handles nested <item> tags
# We can find all FrameLabels and track which frame and label each belongs to.

# Let's inspect root timeline:
# Let's find all FrameLabelTag that are not inside DefineSpriteTag
# How to know if pos is inside DefineSpriteTag?
# Precompute all [start, end] ranges of DefineSpriteTag

sprite_ranges = []
for m in re.finditer(r'<item type="DefineSpriteTag"[^>]*>', content):
    s_start = m.start()
    # Find matching </item>
    # Since sprites only contain sub-tags (which can be item), let's find the matching </item>
    # by counting depth
    depth = 0
    p = s_start
    while True:
        next_open = content.find('<item', p + 1)
        next_close = content.find('</item>', p + 1)
        if next_close == -1:
            break
        if next_open != -1 and next_open < next_close:
            depth += 1
            p = next_open
        else:
            if depth == 0:
                sprite_ranges.append((s_start, next_close + 7))
                break
            depth -= 1
            p = next_close

print(f"Found {len(sprite_ranges)} sprite ranges.")

def is_in_sprite(p):
    # binary search or check ranges
    for s, e in sprite_ranges:
        if s <= p < e:
            return True
        if s > p:
            break
    return False

# Now scan all root tags!
# Collect all ShowFrameTag, FrameLabelTag, PlaceObject2Tag, RemoveObject2Tag that are NOT in sprite
root_events = []

for m in re.finditer(r'<item type="(ShowFrameTag|FrameLabelTag|PlaceObject2Tag|RemoveObject2Tag|RemoveObjectTag)"[^>]*>.*?</item>', content, re.DOTALL):
    if not is_in_sprite(m.start()):
        root_events.append((m.start(), m.group(1), m.group(0)))

print(f"Found {len(root_events)} root timeline events.")

# Now simulate root display list frame by frame!
def parse_transform(tag_str):
    tx_m = re.search(r'translate[Xx]="([^"]+)"', tag_str)
    ty_m = re.search(r'translate[Yy]="([^"]+)"', tag_str)
    sx_m = re.search(r'scale[Xx]="([^"]+)"', tag_str)
    sy_m = re.search(r'scale[Yy]="([^"]+)"', tag_str)
    tx = float(tx_m.group(1))/20.0 if tx_m else 0.0
    ty = float(ty_m.group(1))/20.0 if ty_m else 0.0
    sx = float(sx_m.group(1)) if sx_m else 1.0
    sy = float(sy_m.group(1)) if sy_m else 1.0
    return tx, ty, sx, sy

def get_tag_attr(tag_str, attr):
    m = re.search(fr'{attr}="([^"]+)"', tag_str)
    return m.group(1) if m else None

frame = 1
current_label = ""
active_display = {}

interesting_labels = ['pre_title_screen', 'title_screen', 'credits', 'help', 'game_select', 'level_select_1p', 'level_select_2p', 'game']

for pos, ttype, tag_str in root_events:
    if ttype == 'FrameLabelTag':
        current_label = get_tag_attr(tag_str, 'name')
    elif ttype == 'ShowFrameTag':
        if current_label in interesting_labels or frame in [21, 31, 41, 51, 61, 71, 81, 91, 101, 111, 121]:
            print(f"\n==================== Root Frame {frame}: Label='{current_label}' ====================")
            for d in sorted(active_display.keys(), key=lambda x: int(x)):
                info = active_display[d]
                print(f"  Depth {d:4s}: cid={info['cid']:5s} name={str(info['name']):18s} pos=({info['tx']:6.1f}, {info['ty']:6.1f}) scale=({info['sx']:.2f},{info['sy']:.2f}) {info['extra']}")
        frame += 1
        current_label = ""
    elif ttype == 'PlaceObject2Tag':
        depth = get_tag_attr(tag_str, 'depth')
        cid = get_tag_attr(tag_str, 'characterId')
        name = get_tag_attr(tag_str, 'name')
        tx, ty, sx, sy = parse_transform(tag_str)
        extra = ""
        text_m = re.search(r'text\s*=\s*"([^"]+)"', tag_str)
        if text_m:
            extra += f"text='{text_m.group(1)}' "
        
        if depth in active_display and cid is None:
            active_display[depth]['tx'] = tx
            active_display[depth]['ty'] = ty
            active_display[depth]['sx'] = sx
            active_display[depth]['sy'] = sy
            if name: active_display[depth]['name'] = name
            if extra: active_display[depth]['extra'] += extra
        else:
            active_display[depth] = {
                'cid': cid or '',
                'name': name or '',
                'tx': tx, 'ty': ty, 'sx': sx, 'sy': sy,
                'extra': extra
            }
    elif ttype in ('RemoveObject2Tag', 'RemoveObjectTag'):
        depth = get_tag_attr(tag_str, 'depth')
        if depth in active_display:
            del active_display[depth]

