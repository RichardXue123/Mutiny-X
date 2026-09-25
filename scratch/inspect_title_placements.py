with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re
import binascii

pos_pre = content.find('name="pre_title_screen"')
pos_title = content.find('name="title_screen"')

print(f"pre_title_screen at {pos_pre}, title_screen at {pos_title}")

chunk = content[pos_pre:pos_title + 2000]

for m in re.finditer(r'<item type="([^"]+)"[^>]*>(.*?)</item>|<item type="([^"]+)"[^>]*/>', chunk, re.DOTALL):
    ttype = m.group(1) or m.group(3)
    tag_str = m.group(0)
    if ttype == 'FrameLabelTag':
        print("Label:", re.search(r'name="([^"]+)"', tag_str).group(1))
    elif 'PlaceObject' in ttype:
        cid = re.search(r'characterId="([^"]+)"', tag_str)
        cid_v = cid.group(1) if cid else "(modify)"
        depth = re.search(r'depth="([^"]+)"', tag_str)
        depth_v = depth.group(1) if depth else "?"
        name = re.search(r'name="([^"]+)"', tag_str)
        name_v = name.group(1) if name else ""
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
        print(f"  Depth {depth_v:4s}: cid={cid_v:5s} name={name_v:16s} pos=({tx_v:6.1f}, {ty_v:6.1f}) strs={strs}")
    elif 'RemoveObject' in ttype:
        depth = re.search(r'depth="([^"]+)"', tag_str)
        print(f"  REMOVE Depth {depth.group(1) if depth else '?'}")

