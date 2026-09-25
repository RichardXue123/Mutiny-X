import xml.etree.ElementTree as ET
import re

xml_path = "Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml"

print("Parsing SWF with iterparse...")

# Pre-pass: map characterId -> {type, bounds, name, sub_text}
char_info = {}

# Also collect Sprite 342 and Root Timeline
s342_frames = {}
root_frames = {}

# We will use iterparse with start and end events
stack = []
current_sprite_id = None
current_frame = 1
current_label = ""
active_display = {}

# For sprite 342
s342_frame = 1
s342_label = ""
s342_display = {}

for event, elem in ET.iterparse(xml_path, events=('start', 'end')):
    if event == 'start':
        stack.append(elem)
        if elem.tag == 'item' and elem.attrib.get('type') == 'DefineSpriteTag':
            current_sprite_id = elem.attrib.get('spriteId')
            if current_sprite_id == '342':
                s342_frame = 1
                s342_label = ""
                s342_display = {}
    elif event == 'end':
        if elem.tag == 'item':
            ttype = elem.attrib.get('type')
            
            # Record character definition bounds
            cid = elem.attrib.get('characterId') or elem.attrib.get('spriteId') or elem.attrib.get('shapeId')
            if cid and ttype in ('DefineShapeTag', 'DefineShape2Tag', 'DefineShape3Tag', 'DefineShape4Tag', 'DefineSpriteTag', 'DefineButton2Tag', 'DefineEditTextTag'):
                bounds = None
                bounds_elem = elem.find('shapeBounds')
                if bounds_elem is None:
                    bounds_elem = elem.find('bounds')
                if bounds_elem is not None:
                    rect = bounds_elem.find('RECT')
                    if rect is not None:
                        bounds = (
                            float(rect.attrib.get('xMin', 0))/20.0,
                            float(rect.attrib.get('xMax', 0))/20.0,
                            float(rect.attrib.get('yMin', 0))/20.0,
                            float(rect.attrib.get('yMax', 0))/20.0,
                        )
                char_info[cid] = {
                    'type': ttype,
                    'bounds': bounds,
                    'name': elem.attrib.get('name', '')
                }
            
            # Check if root or sprite 342
            is_root = (len(stack) == 2) # root is tags -> item
            
            if is_root:
                if ttype == 'FrameLabelTag':
                    current_label = elem.attrib.get('name', '')
                elif ttype == 'ShowFrameTag':
                    root_frames[current_frame] = {
                        'label': current_label,
                        'display': dict(active_display)
                    }
                    current_frame += 1
                    current_label = ""
                elif ttype == 'PlaceObject2Tag':
                    depth = elem.attrib.get('depth')
                    cid = elem.attrib.get('characterId')
                    name = elem.attrib.get('name')
                    # Parse matrix
                    tx, ty, sx, sy = 0.0, 0.0, 1.0, 1.0
                    m_elem = elem.find('matrix')
                    if m_elem is not None:
                        m_inner = m_elem.find('MATRIX')
                        if m_inner is not None:
                            tx = float(m_inner.attrib.get('translateX', 0))/20.0
                            ty = float(m_inner.attrib.get('translateY', 0))/20.0
                            sx = float(m_inner.attrib.get('scaleX', 1.0))
                            sy = float(m_inner.attrib.get('scaleY', 1.0))
                    
                    # Parse text if any in clip actions
                    extra = ""
                    clip_actions = elem.find('clipActions')
                    if clip_actions is not None:
                        # find action strings
                        for act in clip_actions.iter('CLIPACTIONRECORD'):
                            # check text in action
                            for sub in act.iter():
                                if sub.text and 'text =' in sub.text:
                                    m_txt = re.search(r'text\s*=\s*"([^"]+)"', sub.text)
                                    if m_txt: extra += f"text='{m_txt.group(1)}' "
                                if sub.attrib.get('actionBytes'):
                                    # action bytes might contain text
                                    pass
                    
                    if depth in active_display and cid is None:
                        active_display[depth]['tx'] = tx
                        active_display[depth]['ty'] = ty
                        active_display[depth]['sx'] = sx
                        active_display[depth]['sy'] = sy
                        if name: active_display[depth]['name'] = name
                        if extra: active_display[depth]['extra'] += extra
                    else:
                        active_display[depth] = {
                            'cid': cid or '',
                            'name': name or '',
                            'tx': tx, 'ty': ty, 'sx': sx, 'sy': sy,
                            'extra': extra
                        }
                elif ttype in ('RemoveObject2Tag', 'RemoveObjectTag'):
                    depth = elem.attrib.get('depth')
                    if depth in active_display:
                        del active_display[depth]
            
            elif current_sprite_id == '342':
                if ttype == 'FrameLabelTag':
                    s342_label = elem.attrib.get('name', '')
                elif ttype == 'ShowFrameTag':
                    s342_frames[s342_frame] = {
                        'label': s342_label,
                        'display': dict(s342_display)
                    }
                    s342_frame += 1
                    s342_label = ""
                elif ttype == 'PlaceObject2Tag':
                    depth = elem.attrib.get('depth')
                    cid = elem.attrib.get('characterId')
                    name = elem.attrib.get('name')
                    tx, ty, sx, sy = 0.0, 0.0, 1.0, 1.0
                    m_elem = elem.find('matrix')
                    if m_elem is not None:
                        m_inner = m_elem.find('MATRIX')
                        if m_inner is not None:
                            tx = float(m_inner.attrib.get('translateX', 0))/20.0
                            ty = float(m_inner.attrib.get('translateY', 0))/20.0
                            sx = float(m_inner.attrib.get('scaleX', 1.0))
                            sy = float(m_inner.attrib.get('scaleY', 1.0))
                    
                    extra = ""
                    clip_actions = elem.find('clipActions')
                    if clip_actions is not None:
                        for act in clip_actions.iter('CLIPACTIONRECORD'):
                            for sub in act.iter():
                                if sub.text and 'text =' in sub.text:
                                    m_txt = re.search(r'text\s*=\s*"([^"]+)"', sub.text)
                                    if m_txt: extra += f"text='{m_txt.group(1)}' "
                                    
                    if depth in s342_display and cid is None:
                        s342_display[depth]['tx'] = tx
                        s342_display[depth]['ty'] = ty
                        s342_display[depth]['sx'] = sx
                        s342_display[depth]['sy'] = sy
                        if name: s342_display[depth]['name'] = name
                        if extra: s342_display[depth]['extra'] += extra
                    else:
                        s342_display[depth] = {
                            'cid': cid or '',
                            'name': name or '',
                            'tx': tx, 'ty': ty, 'sx': sx, 'sy': sy,
                            'extra': extra
                        }
                elif ttype in ('RemoveObject2Tag', 'RemoveObjectTag'):
                    depth = elem.attrib.get('depth')
                    if depth in s342_display:
                        del s342_display[depth]
            
            if ttype == 'DefineSpriteTag':
                current_sprite_id = None
        
        stack.pop()
        elem.clear()

print("Parsing complete!")
print(f"Total root frames: {len(root_frames)}")
print(f"Total sprite 342 frames: {len(s342_frames)}")

# Print Summary of Target Root Frames
print("\n========================= ROOT TIMELINE SCREENS =========================")
target_labels = ['pre_title_screen', 'title_screen', 'credits', 'help', 'game_select', 'level_select_1p', 'level_select_2p', 'game']

for fnum, f_data in root_frames.items():
    lbl = f_data['label']
    if lbl in target_labels:
        print(f"\n>>> Frame {fnum}: '{lbl}'")
        for depth in sorted(f_data['display'].keys(), key=lambda x: int(x)):
            item = f_data['display'][depth]
            cid = item['cid']
            c_info = char_info.get(cid, {})
            b = c_info.get('bounds')
            bounds_str = f"bounds={b}" if b else ""
            ctype = c_info.get('type', '')
            print(f"  Depth {depth:4s}: cid={cid:5s} ({ctype:15s}) name={str(item['name']):16s} pos=({item['tx']:6.1f}, {item['ty']:6.1f}) scale=({item['sx']:.2f},{item['sy']:.2f}) {bounds_str} {item['extra']}")

# Print Summary of Sprite 342 Frames
print("\n========================= INGAME POPUP (SPRITE 342) =========================")
for fnum, f_data in s342_frames.items():
    lbl = f_data['label']
    if lbl or fnum in [1, 2, 10, 20, 30, 31, 41, 51]:
        print(f"\n>>> Sprite 342 Frame {fnum}: '{lbl}'")
        for depth in sorted(f_data['display'].keys(), key=lambda x: int(x)):
            item = f_data['display'][depth]
            cid = item['cid']
            c_info = char_info.get(cid, {})
            b = c_info.get('bounds')
            bounds_str = f"bounds={b}" if b else ""
            ctype = c_info.get('type', '')
            print(f"  Depth {depth:4s}: cid={cid:5s} ({ctype:15s}) name={str(item['name']):16s} pos=({item['tx']:6.1f}, {item['ty']:6.1f}) scale=({item['sx']:.2f},{item['sy']:.2f}) {bounds_str} {item['extra']}")
