import re

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

# 1. Find sprite 342
s_idx = content.find('spriteId="342"')
tag_start = content.rfind('<item type="DefineSpriteTag"', 0, s_idx)
next_sprite = content.find('<item type="DefineSpriteTag"', s_idx)
s342_chunk = content[tag_start:next_sprite]

print("=== Sprite 342 PlaceObject2Tags ===")
frame = 1
for m in re.finditer(r'<item type="([^"]+)"[^>]*>(.*?)</item>', s342_chunk, re.DOTALL):
    ttype = m.group(1)
    tag_body = m.group(0)
    if ttype == 'ShowFrameTag':
        frame += 1
    elif ttype == 'FrameLabelTag':
        lbl = re.search(r'name="([^"]+)"', tag_body).group(1)
        print(f"\n--- Frame {frame}: Label '{lbl}' ---")
    elif ttype == 'PlaceObject2Tag':
        cid_m = re.search(r'characterId="([^"]+)"', tag_body)
        cid = cid_m.group(1) if cid_m else "(none)"
        depth_m = re.search(r'depth="([^"]+)"', tag_body)
        depth = depth_m.group(1) if depth_m else "(none)"
        name_m = re.search(r'name="([^"]+)"', tag_body)
        name = name_m.group(1) if name_m else ""
        tx_m = re.search(r'translateX="([^"]+)"', tag_body)
        ty_m = re.search(r'translateY="([^"]+)"', tag_body)
        tx = float(tx_m.group(1))/20.0 if tx_m else 0.0
        ty = float(ty_m.group(1))/20.0 if ty_m else 0.0
        
        # Also check clip action text or scripts
        txt = ""
        txt_m = re.findall(r'text\s*=\s*"([^"]+)"', tag_body)
        if txt_m: txt = f" text={txt_m}"
        # Or look for strings in actionBytes
        ab_m = re.search(r'actionBytes="([^"]+)"', tag_body)
        if ab_m:
            try:
                import binascii
                raw = binascii.unhexlify(ab_m.group(1))
                strs = re.findall(b'[\x20-\x7e]{2,}', raw)
                strs = [s.decode('ascii') for s in strs]
                txt += f" strings={strs}"
            except: pass
            
        print(f"  Frame {frame} Depth {depth}: cid={cid:5s} name={name:16s} pos=({tx:6.1f}, {ty:6.1f}) {txt}")

