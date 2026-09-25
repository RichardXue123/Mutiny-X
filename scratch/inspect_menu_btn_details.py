with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re
import binascii

for sid in ['588', '671', '673', '677', '679', '681', '630']:
    idx = content.find(f'spriteId="{sid}"')
    if idx == -1:
        idx = content.find(f'shapeId="{sid}"')
    chunk = content[idx:idx+2000]
    next_s = chunk.find('spriteId=', 20)
    if next_s != -1: chunk = chunk[:next_s]
    
    texts = []
    for ab in re.findall(r'actionBytes="([^"]+)"', chunk):
        try:
            raw = binascii.unhexlify(ab)
            for s in re.findall(b'[\x20-\x7e]{2,}', raw):
                s_dec = s.decode('ascii')
                if s_dec not in ('centered', 'line_spacing', 'tracking', 'enabled', 'visible', 'minHeight', 'minWidth'):
                    texts.append(s_dec)
        except: pass
    print(f"ID {sid}: texts={texts}")

# Also check button background shapes 670, 675, 587
for sid in ['670', '675', '587']:
    idx = content.find(f'shapeId="{sid}"')
    chunk = content[idx:idx+500]
    m = re.search(r'<RECT[^>]*Xmin="([^"]+)"[^>]*Xmax="([^"]+)"[^>]*Ymin="([^"]+)"[^>]*Ymax="([^"]+)"', chunk)
    if m:
        w = (float(m.group(2)) - float(m.group(1)))/20.0
        h = (float(m.group(4)) - float(m.group(3)))/20.0
        print(f"Shape {sid}: w={w}, h={h}, bounds=({float(m.group(1))/20.0}, {float(m.group(3))/20.0}) to ({float(m.group(2))/20.0}, {float(m.group(4))/20.0})")
