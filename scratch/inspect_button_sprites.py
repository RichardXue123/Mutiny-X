import re

with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml', 'r', encoding='utf-8') as f:
    text = f.read()

for sid in [676, 672, 669, 631, 358, 335]:
    pattern = rf'<item type="DefineSpriteTag"[^>]*spriteId="{sid}"[\s\S]*?</item>'
    m = re.search(pattern, text)
    if m:
        print(f"=== Sprite {sid} ===")
        # Print frame labels and PlaceObject tags
        content = m.group(0)
        for line in content.split('\n'):
            if any(k in line for k in ['FrameLabelTag', 'PlaceObject', 'ColorTransform', 'shapeId', 'characterId', 'name=']):
                print("  ", line.strip())
