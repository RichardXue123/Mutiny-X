with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re
import binascii

def inspect_symbol(sid):
    print(f"\n======================================== Symbol {sid} ========================================")
    # find where it is defined
    patterns = [
        f'spriteId="{sid}"',
        f'shapeId="{sid}"',
        f'buttonId="{sid}"'
    ]
    idx = -1
    for p in patterns:
        idx = content.find(p)
        if idx != -1:
            break
            
    if idx == -1:
        print("  Not found!")
        return
        
    tag_start = content.rfind('<item type="', 0, idx)
    chunk = content[tag_start:tag_start+25000]
    # find tag end
    # if it's DefineShape, find </item>
    # if DefineSprite, find end of sprite
    
    # check bounds
    bounds_m = re.search(r'shapeBounds.*?<RECT[^>]*Xmin="([^"]+)"[^>]*Xmax="([^"]+)"[^>]*Ymin="([^"]+)"[^>]*Ymax="([^"]+)"', chunk, re.DOTALL)
    if bounds_m:
        xmin = float(bounds_m.group(1))/20.0
        xmax = float(bounds_m.group(2))/20.0
        ymin = float(bounds_m.group(3))/20.0
        ymax = float(bounds_m.group(4))/20.0
        print(f"  Shape Bounds: x=[{xmin}, {xmax}] (w={xmax-xmin}), y=[{ymin}, {ymax}] (h={ymax-ymin})")
        
    # check frame labels and placed objects
    frame = 1
    # stop if next top-level item starts
    # let's look for PlaceObject
    for pl in re.finditer(r'<item type="([^"]*PlaceObject[^"]*)"[^>]*>(.*?)</item>|<item type="([^"]*PlaceObject[^"]*)"[^>]*/>', chunk, re.DOTALL):
        p_str = pl.group(0)
        ttype = pl.group(1) or pl.group(3)
        cid = re.search(r'characterId="([^"]+)"', p_str)
        cid_v = cid.group(1) if cid else "(modify)"
        depth = re.search(r'depth="([^"]+)"', p_str)
        depth_v = depth.group(1) if depth else "?"
        name = re.search(r'name="([^"]+)"', p_str)
        name_v = name.group(1) if name else ""
        tx = re.search(r'translateX="([^"]+)"', p_str)
        ty = re.search(r'translateY="([^"]+)"', p_str)
        tx_v = float(tx.group(1))/20.0 if tx else 0
        ty_v = float(ty.group(1))/20.0 if ty else 0
        
        strs = []
        for ab in re.findall(r'actionBytes="([^"]+)"', p_str):
            try:
                raw = binascii.unhexlify(ab)
                strs.extend([s.decode('ascii') for s in re.findall(b'[\x20-\x7e]{2,}', raw)])
            except: pass
        print(f"  Depth {depth_v:4s}: {ttype} cid={cid_v:5s} name={name_v:16s} pos=({tx_v:6.1f}, {ty_v:6.1f}) strs={strs}")

for sid in ['1927', '1945', '1954', '1957', '1959', '1960', '629', '631', '672', '674', '676']:
    inspect_symbol(sid)

