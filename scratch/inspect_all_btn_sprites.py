with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re
import binascii

def inspect_sprite(sid):
    idx = content.find(f'spriteId="{sid}"')
    if idx == -1: return
    # Find matching </item> for DefineSpriteTag
    # Extract 5000 characters
    chunk = content[idx:idx+15000]
    # find next DefineSpriteTag
    next_s = chunk.find('spriteId=', 20)
    if next_s != -1:
        chunk = chunk[:next_s]
    
    print(f"\n==================== Sprite {sid} ====================")
    frame = 1
    for m in re.finditer(r'<item type="([^"]+)"[^>]*>(.*?)</item>|<item type="([^"]+)"[^>]*/>', chunk, re.DOTALL):
        ttype = m.group(1) or m.group(3)
        tag_str = m.group(0)
        if ttype == 'ShowFrameTag':
            frame += 1
        elif ttype == 'FrameLabelTag':
            lbl = re.search(r'name="([^"]+)"', tag_str).group(1)
            print(f"  Frame {frame}: Label '{lbl}'")
        elif 'PlaceObject' in ttype:
            cid = re.search(r'characterId="([^"]+)"', tag_str)
            cid = cid.group(1) if cid else "(modify)"
            depth = re.search(r'depth="([^"]+)"', tag_str)
            depth = depth.group(1) if depth else "?"
            tx = re.search(r'translateX="([^"]+)"', tag_str)
            ty = re.search(r'translateY="([^"]+)"', tag_str)
            tx_v = float(tx.group(1))/20.0 if tx else 0
            ty_v = float(ty.group(1))/20.0 if ty else 0
            strs = []
            for ab in re.findall(r'actionBytes="([^"]+)"', tag_str):
                try:
                    raw = binascii.unhexlify(ab)
                    strs.extend([s.decode('ascii') for s in re.findall(b'[\x20-\x7e]{2,}', raw)])
                except: pass
            print(f"  Frame {frame} Depth {depth}: {ttype} cid={cid} pos=({tx_v}, {ty_v}) strs={strs}")

for sid in ['330', '333', '335', '337', '339', '341']:
    inspect_sprite(sid)

# Also check shape 328
idx_328 = content.find('shapeId="328"')
print("\nShape 328:")
print(content[idx_328:idx_328+300])
