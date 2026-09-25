with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re
import xml.etree.ElementTree as ET

s_idx = content.find('spriteId="342"')
tag_start = content.rfind('<item type="DefineSpriteTag"', 0, s_idx)
lines = content[tag_start:tag_start+50000].splitlines()

collected = []
for line in lines:
    collected.append(line)
    if '</item>' in line and line.strip() == '</item>':
        test_str = "\n".join(collected)
        try:
            root = ET.fromstring(test_str)
            break
        except Exception:
            continue

print("Children of root:", [c.tag for c in root])
tags_container = root.find('controlTags') or root.find('subTags') or root[0]
print("tags_container tag:", tags_container.tag)

frame = 1
for child in tags_container:
    ttype = child.attrib.get('type')
    if ttype == 'ShowFrameTag':
        frame += 1
    elif ttype == 'FrameLabelTag':
        print(f"\n==================== Frame {frame}: Label = '{child.attrib.get('name')}' ====================")
    elif ttype == 'PlaceObject2Tag':
        cid = child.attrib.get('characterId')
        depth = child.attrib.get('depth')
        name = child.attrib.get('name')
        m = child.find('matrix')
        tx = float(m.attrib.get('translateX', 0))/20.0 if m is not None else 0
        ty = float(m.attrib.get('translateY', 0))/20.0 if m is not None else 0
        
        txt = []
        ca = child.find('clipActions')
        if ca is not None:
            for rec in ca.iter('item'):
                ab = rec.attrib.get('actionBytes')
                if ab:
                    import binascii
                    try:
                        raw = binascii.unhexlify(ab)
                        strs = re.findall(b'[\x20-\x7e]{2,}', raw)
                        txt.extend([s.decode('ascii') for s in strs if s not in (b'centered', b'line_spacing', b'tracking', b'enabled', b'visible', b'minHeight', b'minWidth', b'align')])
                    except: pass
        print(f"  Frame {frame} Depth {depth}: cid={cid} name={name} pos=({tx:6.1f}, {ty:6.1f}) txt={txt}")
    elif ttype in ('RemoveObject2Tag', 'RemoveObjectTag'):
        depth = child.attrib.get('depth')
        print(f"  Frame {frame} REMOVE depth {depth}")
