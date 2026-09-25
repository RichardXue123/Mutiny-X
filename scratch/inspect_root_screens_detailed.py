with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re
import binascii

# Let's inspect root timeline frames:
# We know the frame labels:
# pre_title_screen (frame 31?), title_screen, help, game_select, level_select_1p, level_select_2p

labels = ['title_screen', 'help', 'game_select', 'level_select_1p', 'level_select_2p']

for lbl in labels:
    pos = content.find(f'name="{lbl}"')
    if pos == -1: continue
    # find ShowFrameTag before this, and ShowFrameTag after this
    prev_show = content.rfind('<item type="ShowFrameTag"', 0, pos)
    next_show = content.find('<item type="ShowFrameTag"', pos)
    
    chunk = content[prev_show:next_show]
    print(f"\n======================================== Root Screen: '{lbl}' ========================================")
    
    # Also let's find what was removed in this frame
    for rm in re.finditer(r'<item type="RemoveObject2Tag"[^>]*depth="([^"]+)"', chunk):
        print(f"  REMOVE Depth {rm.group(1)}")
        
    for pl in re.finditer(r'<item type="PlaceObject2Tag"[^>]*>.*?</item>|<item type="PlaceObject3Tag"[^>]*>.*?</item>', chunk, re.DOTALL):
        p_str = pl.group(0)
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
        
        # text strings
        strs = []
        for ab in re.findall(r'actionBytes="([^"]+)"', p_str):
            try:
                raw = binascii.unhexlify(ab)
                strs.extend([s.decode('ascii') for s in re.findall(b'[\x20-\x7e]{2,}', raw)])
            except: pass
            
        print(f"  Depth {depth_v:4s}: cid={cid_v:5s} name={name_v:16s} pos=({tx_v:6.1f}, {ty_v:6.1f}) strs={strs}")

