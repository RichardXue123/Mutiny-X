with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re
import binascii

def get_sprite_info(sid):
    idx = content.find(f'spriteId="{sid}"')
    if idx == -1: return
    chunk = content[idx:idx+3000]
    next_s = chunk.find('spriteId=', 20)
    if next_s != -1: chunk = chunk[:next_s]
    
    # find text
    texts = []
    for ab in re.findall(r'actionBytes="([^"]+)"', chunk):
        try:
            raw = binascii.unhexlify(ab)
            for s in re.findall(b'[\x20-\x7e]{2,}', raw):
                s_dec = s.decode('ascii')
                if s_dec not in ('centered', 'line_spacing', 'tracking', 'enabled', 'visible', 'minHeight', 'minWidth'):
                    texts.append(s_dec)
        except: pass
    
    # find shapes inside
    shapes = re.findall(r'characterId="([^"]+)"', chunk)
    print(f"Sprite {sid}: texts={texts} inner_cids={shapes}")

for sid in ['672', '674', '676', '678', '680', '682', '631', '589']:
    get_sprite_info(sid)
