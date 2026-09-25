import re
import binascii

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

s_idx = content.find('spriteId="342"')
tag_start = content.rfind('<item type="DefineSpriteTag"', 0, s_idx)
next_sprite = content.find('<item type="DefineSpriteTag"', s_idx)
s342_chunk = content[tag_start:next_sprite]

def extract_strings(tag_str):
    res = []
    for ab in re.findall(r'actionBytes="([^"]+)"', tag_str):
        try:
            raw = binascii.unhexlify(ab)
            strs = re.findall(b'[\x20-\x7e]{2,}', raw)
            res.extend([s.decode('ascii') for s in strs])
        except: pass
    return res

frame = 1
for m in re.finditer(r'<item type="([^"]+)"[^>]*>(.*?)</item>|<item type="([^"]+)"[^>]*/>', s342_chunk, re.DOTALL):
    ttype = m.group(1) or m.group(3)
    tag_body = m.group(0)
    
    if ttype == 'ShowFrameTag':
        frame += 1
    elif ttype == 'FrameLabelTag':
        name_m = re.search(r'name="([^"]+)"', tag_body)
        print(f"=== Frame {frame}: Label '{name_m.group(1)}' ===")
    elif ttype == 'PlaceObject2Tag':
        cid = re.search(r'characterId="([^"]+)"', tag_body)
        cid = cid.group(1) if cid else "(modify)"
        depth = re.search(r'depth="([^"]+)"', tag_body)
        depth = depth.group(1) if depth else "?"
        name = re.search(r'name="([^"]+)"', tag_body)
        name = name.group(1) if name else ""
        tx_m = re.search(r'translateX="([^"]+)"', tag_body)
        ty_m = re.search(r'translateY="([^"]+)"', tag_body)
        tx = float(tx_m.group(1))/20.0 if tx_m else 0.0
        ty = float(ty_m.group(1))/20.0 if ty_m else 0.0
        strs = extract_strings(tag_body)
        print(f"  Frame {frame} Depth {depth}: cid={cid} name={name} pos=({tx}, {ty}) strs={strs}")
    elif ttype in ('RemoveObject2Tag', 'RemoveObjectTag'):
        depth = re.search(r'depth="([^"]+)"', tag_body)
        depth = depth.group(1) if depth else "?"
        print(f"  Frame {frame} Remove depth {depth}")
