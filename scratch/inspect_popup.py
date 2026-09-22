with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

# find sprite 342
idx = content.find('spriteId="342"')
tag_start = content.rfind('<item type="DefineSpriteTag"', 0, idx)
next_sprite = content.find('<item type="DefineSpriteTag"', idx)
chunk = content[tag_start:next_sprite]

frame = 1
for m in re.finditer(r'<item type="([^"]+)"[^>]*>.*?</item>', chunk, re.DOTALL):
    tag = m.group(0)
    ttype = m.group(1)
    if ttype == 'ShowFrameTag':
        frame += 1
    elif ttype == 'PlaceObject2Tag':
        cid_m = re.search(r'characterId="([^"]+)"', tag)
        cid = cid_m.group(1) if cid_m else ""
        ty_m = re.search(r'translateY="([^"]+)"', tag)
        ty = float(ty_m.group(1))/20.0 if ty_m else 0
        depth_m = re.search(r'depth="([^"]+)"', tag)
        depth = depth_m.group(1) if depth_m else ""
        print(f"Frame {frame} depth={depth} cid={cid} y={ty}")
