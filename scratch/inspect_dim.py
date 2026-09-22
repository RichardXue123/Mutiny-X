with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

for m in re.finditer(r'<item type="PlaceObject2Tag"[^>]*characterId="1855"[^>]*>.*?</item>', content, re.DOTALL):
    tag = m.group(0)
    pos = m.start()
    sprite_start = content.rfind('<item type="DefineSpriteTag"', 0, pos)
    sprite_tag = content[sprite_start:sprite_start+100] if sprite_start != -1 else "MAIN"
    print("Placement:")
    print("Sprite:", sprite_tag)
    print("Tag:", tag)
