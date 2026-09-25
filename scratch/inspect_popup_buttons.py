import re

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

def inspect_char(cid):
    print(f"\n==================== Character {cid} ====================")
    # find definition
    patterns = [
        rf'<item type="DefineSpriteTag"[^>]*spriteId="{cid}"[^>]*>(.*?)</item>',
        rf'<item type="DefineShapeTag"[^>]*shapeId="{cid}"[^>]*>(.*?)</item>',
        rf'<item type="DefineShape2Tag"[^>]*shapeId="{cid}"[^>]*>(.*?)</item>',
        rf'<item type="DefineShape3Tag"[^>]*shapeId="{cid}"[^>]*>(.*?)</item>',
        rf'<item type="DefineButton2Tag"[^>]*buttonId="{cid}"[^>]*>(.*?)</item>',
    ]
    matched = False
    for pat in patterns:
        m = re.search(pat, content, re.DOTALL)
        if m:
            matched = True
            tag_full = m.group(0)
            tag_inner = m.group(1)
            # Find bounds
            bounds_m = re.search(r'shapeBounds.*?<RECT[^>]*xMin="([^"]+)"[^>]*xMax="([^"]+)"[^>]*yMin="([^"]+)"[^>]*yMax="([^"]+)"', tag_full, re.DOTALL)
            if bounds_m:
                xmin = float(bounds_m.group(1))/20.0
                xmax = float(bounds_m.group(2))/20.0
                ymin = float(bounds_m.group(3))/20.0
                ymax = float(bounds_m.group(4))/20.0
                print(f"  Bounds: x=[{xmin}, {xmax}] (w={xmax-xmin}), y=[{ymin}, {ymax}] (h={ymax-ymin})")
            
            # Find inner placed objects or buttons
            for pl in re.finditer(r'<item type="PlaceObject2Tag"[^>]*>.*?</item>', tag_inner, re.DOTALL):
                p_str = pl.group(0)
                sub_cid = re.search(r'characterId="([^"]+)"', p_str)
                sub_c = sub_cid.group(1) if sub_cid else ""
                tx = re.search(r'translateX="([^"]+)"', p_str)
                ty = re.search(r'translateY="([^"]+)"', p_str)
                tx_v = float(tx.group(1))/20.0 if tx else 0
                ty_v = float(ty.group(1))/20.0 if ty else 0
                
                # check strings
                strs = []
                ab = re.search(r'actionBytes="([^"]+)"', p_str)
                if ab:
                    import binascii
                    try:
                        raw = binascii.unhexlify(ab.group(1))
                        s_found = re.findall(b'[\x20-\x7e]{2,}', raw)
                        strs = [s.decode('ascii') for s in s_found]
                    except: pass
                print(f"  Contains PlaceObject2: cid={sub_c} pos=({tx_v}, {ty_v}) strs={strs}")
            
            # Find Button records
            for b_rec in re.finditer(r'<item type="BUTTONRECORD"[^>]*characterId="([^"]+)"[^>]*>', tag_inner):
                sub_c = b_rec.group(1)
                print(f"  ButtonRecord: cid={sub_c}")
            break
            
    if not matched:
        print("  Not found!")

for cid in ['326', '328', '330', '333', '335', '337', '339', '341', '149', '319']:
    inspect_char(cid)

