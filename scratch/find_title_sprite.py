with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

for m in re.finditer(r'<item type="FrameLabelTag"[^>]*name="title_screen"', content):
    pos = m.start()
    # Find enclosing DefineSpriteTag
    prev_sprite = content.rfind('<item type="DefineSpriteTag"', 0, pos)
    sprite_id_m = re.search(r'spriteId="([^"]+)"', content[prev_sprite:prev_sprite+100])
    sprite_id = sprite_id_m.group(1) if sprite_id_m else "UNKNOWN"
    print(f"title_screen is inside spriteId={sprite_id} (tag starts at {prev_sprite})")
