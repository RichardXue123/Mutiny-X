import re
import binascii

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

def inspect_shape(sid):
    idx = content.find(f'shapeId="{sid}"')
    if idx == -1: return
    tag_start = content.rfind('<item type="DefineShape', 0, idx)
    tag_end = content.find('</item>', idx) + 7
    chunk = content[tag_start:tag_end]
    bounds_m = re.search(r'shapeBounds.*?<RECT[^>]*xMin="([^"]+)"[^>]*xMax="([^"]+)"[^>]*yMin="([^"]+)"[^>]*yMax="([^"]+)"', chunk, re.DOTALL)
    if bounds_m:
        xmin = float(bounds_m.group(1))/20.0
        xmax = float(bounds_m.group(2))/20.0
        ymin = float(bounds_m.group(3))/20.0
        ymax = float(bounds_m.group(4))/20.0
        print(f"Shape {sid}: x=[{xmin}, {xmax}] (w={xmax-xmin}), y=[{ymin}, {ymax}] (h={ymax-ymin})")

def inspect_sprite(sid):
    idx = content.find(f'spriteId="{sid}"')
    if idx == -1: return
    tag_start = content.rfind('<item type="DefineSpriteTag"', 0, idx)
    # find next DefineSpriteTag or end
    tag_end = content.find('</item>\n  <item type="DefineSpriteTag"', idx)
    if tag_end == -1:
        tag_end = content.find('</tags>', idx)
    else:
        tag_end += 7
    chunk = content[tag_start:tag_end]
    
    print(f"\n--- Sprite {sid} ---")
    frame = 1
    for m in re.finditer(r'<item type="([^"]+)"[^>]*>(.*?)</item>', chunk, re.DOTALL):
        ttype = m.group(1)
        tag_body = m.group(0)
        if ttype == 'ShowFrameTag':
            frame += 1
        elif ttype == 'PlaceObject2Tag':
            cid = re.search(r'characterId="([^"]+)"', tag_body)
            cid = cid.group(1) if cid else ""
            depth = re.search(r'depth="([^"]+)"', tag_body).group(1)
            tx = re.search(r'translateX="([^"]+)"', tag_body)
            ty = re.search(r'translateY="([^"]+)"', tag_body)
            tx_v = float(tx.group(1))/20.0 if tx else 0
            ty_v = float(ty.group(1))/20.0 if ty else 0
            
            # strings
            strs = []
            ab = re.search(r'actionBytes="([^"]+)"', tag_body)
            if ab:
                try:
                    raw = binascii.unhexlify(ab.group(1))
                    s_found = re.findall(b'[\x20-\x7e]{2,}', raw)
                    strs = [s.decode('ascii') for s in s_found]
                except: pass
            print(f"  Frame {frame} Depth {depth}: cid={cid} pos=({tx_v}, {ty_v}) strs={strs}")

inspect_shape('326')
inspect_shape('328')

for sid in ['330', '333', '335', '337', '339', '341']:
    inspect_sprite(sid)
