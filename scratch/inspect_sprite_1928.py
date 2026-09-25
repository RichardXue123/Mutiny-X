import re

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

s_idx = content.find('spriteId="1928"')
tag_start = content.rfind('<item type="DefineSpriteTag"', 0, s_idx)
# Find the next DefineSpriteTag or end of tags
next_sprite = content.find('<item type="DefineSpriteTag"', s_idx)
if next_sprite == -1:
    # It might be the last sprite before root tags or end of file!
    # Let's find </tags>
    next_sprite = content.find('</tags>', s_idx)

print(f"Sprite 1928 starts at {tag_start}, next sprite/tag at {next_sprite}")
sprite_chunk = content[tag_start:next_sprite]
print(f"Chunk length: {len(sprite_chunk)}")

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

# Check frame count of sprite 1928
fc_m = re.search(r'frameCount="([^"]+)"', sprite_chunk[:200])
print(f"Sprite 1928 frameCount = {fc_m.group(1) if fc_m else 'unknown'}")

# Simulate timeline of Sprite 1928!
frame = 1
current_label = ""
active_display = {}

interesting_labels = ['pre_title_screen', 'title_screen', 'credits', 'help', 'game_select', 'level_select_1p', 'level_select_2p', 'game']

for m in re.finditer(r'<item type="([^"]+)"[^>]*>(.*?)</item>', sprite_chunk, re.DOTALL):
    ttype = m.group(1)
    tag_body = m.group(0)
    
    if ttype == 'FrameLabelTag':
        current_label = get_tag_attr(tag_body, 'name')
    elif ttype == 'ShowFrameTag':
        if current_label in interesting_labels or any(lbl == current_label for lbl in interesting_labels):
            print(f"\n==================== Sprite 1928 Frame {frame}: Label='{current_label}' ====================")
            for d in sorted(active_display.keys(), key=lambda x: int(x)):
                info = active_display[d]
                print(f"  Depth {d:4s}: cid={info['cid']:5s} name={str(info['name']):18s} pos=({info['tx']:6.1f}, {info['ty']:6.1f}) scale=({info['sx']:.2f},{info['sy']:.2f}) {info['extra']}")
        frame += 1
        current_label = ""
    elif ttype == 'PlaceObject2Tag':
        depth_val = get_tag_attr(tag_body, 'depth')
        cid = get_tag_attr(tag_body, 'characterId')
        name = get_tag_attr(tag_body, 'name')
        tx, ty, sx, sy = parse_transform(tag_body)
        extra = ""
        text_m = re.search(r'text\s*=\s*"([^"]+)"', tag_body)
        if text_m:
            extra += f"text='{text_m.group(1)}' "
        centered_m = re.search(r'centered\s*=\s*([^;]+);', tag_body)
        if centered_m:
            extra += f"centered={centered_m.group(1).strip()} "
            
        if depth_val in active_display and cid is None:
            active_display[depth_val]['tx'] = tx
            active_display[depth_val]['ty'] = ty
            active_display[depth_val]['sx'] = sx
            active_display[depth_val]['sy'] = sy
            if name: active_display[depth_val]['name'] = name
            if extra: active_display[depth_val]['extra'] += extra
        else:
            active_display[depth_val] = {
                'cid': cid or '',
                'name': name or '',
                'tx': tx, 'ty': ty, 'sx': sx, 'sy': sy,
                'extra': extra
            }
    elif ttype in ('RemoveObject2Tag', 'RemoveObjectTag'):
        depth_val = get_tag_attr(tag_body, 'depth')
        if depth_val in active_display:
            del active_display[depth_val]
