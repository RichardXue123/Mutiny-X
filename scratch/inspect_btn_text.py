with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re
import binascii

for cid in ['329', '330', '332', '333', '334', '335', '336', '337', '338', '339', '340', '341']:
    # find definition
    m = re.search(rf'<item type="[^"]+"[^>]*Id="{cid}"[^>]*>(.*?)</item>', content, re.DOTALL)
    if m:
        tag_str = m.group(0)
        # find strings in actionBytes or names
        strs = []
        for ab in re.findall(r'actionBytes="([^"]+)"', tag_str):
            try:
                raw = binascii.unhexlify(ab)
                strs.extend([s.decode('ascii') for s in re.findall(b'[\x20-\x7e]{2,}', raw)])
            except: pass
        # find bounds
        bounds_m = re.search(r'shapeBounds.*?<RECT[^>]*Xmin="([^"]+)"[^>]*Xmax="([^"]+)"[^>]*Ymin="([^"]+)"[^>]*Ymax="([^"]+)"', tag_str, re.DOTALL)
        b_str = ""
        if bounds_m:
            b_str = f"bounds: x=[{float(bounds_m.group(1))/20.0}, {float(bounds_m.group(2))/20.0}], y=[{float(bounds_m.group(3))/20.0}, {float(bounds_m.group(4))/20.0}]"
        print(f"CID {cid} ({m.group(0)[:40]}...): {b_str} strs={strs}")
