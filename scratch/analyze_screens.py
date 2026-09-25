import re

xml_path = "Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml"

with open(xml_path, "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

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

# Find where root tags begin (after all DefineSpriteTag, DefineShapeTag, etc.)
# Root timeline tags are at the end of the SWF
# Let's find the last DefineSpriteTag
last_define_sprite = content.rfind('</item>\n  <item type="DefineSpriteTag"')
if last_define_sprite == -1:
    last_define_sprite = content.rfind('<item type="DefineSpriteTag"')
# Find end of that last sprite
end_last_sprite = content.find('</item>', last_define_sprite) + 7
print(f"End of definitions around pos {end_last_sprite}")

root_chunk = content[end_last_sprite:]

# Let's iterate through tags in root_chunk
# Track frame number, frame label, active display list {depth: info}
active_display = {}
frame = 1

tag_iter = re.finditer(r'<item type="([^"]+)"[^>]*>(.*?)</item>', root_chunk, re.DOTALL)

interesting_labels = ['pre_title_screen', 'title_screen', 'credits', 'help', 'game_select', 'level_select_1p', 'level_select_2p', 'game']
current_label = None

for m in tag_iter:
    ttype = m.group(1)
    tag_body = m.group(0)
    
    if ttype == 'FrameLabelTag':
        current_label = get_tag_attr(tag_body, 'name')
    elif ttype == 'ShowFrameTag':
        if current_label in interesting_labels or frame in [21, 31, 41, 51, 61, 71, 81, 91, 101, 111, 121]:
            print(f"\n==================== Frame {frame}: Label='{current_label}' ====================")
            for d in sorted(active_display.keys(), key=lambda x: int(x)):
                info = active_display[d]
                print(f"  Depth {d:4s}: cid={info['cid']:5s} name={str(info['name']):16s} pos=({info['tx']:6.1f}, {info['ty']:6.1f}) scale=({info['sx']:.2f},{info['sy']:.2f}) {info['extra']}")
        frame += 1
        current_label = None
    elif ttype == 'PlaceObject2Tag':
        depth = get_tag_attr(tag_body, 'depth')
        cid = get_tag_attr(tag_body, 'characterId')
        name = get_tag_attr(tag_body, 'name')
        tx, ty, sx, sy = parse_transform(tag_body)
        
        # Check text or construct
        extra = ""
        text_m = re.search(r'text\s*=\s*"([^"]+)"', tag_body)
        if text_m:
            extra += f"text='{text_m.group(1)}' "
        
        # In flash, PlaceObject2 can modify existing depth or replace it
        if depth in active_display and cid is None:
            # Modify existing
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
    elif ttype == 'RemoveObject2Tag':
        depth = get_tag_attr(tag_body, 'depth')
        if depth in active_display:
            del active_display[depth]

