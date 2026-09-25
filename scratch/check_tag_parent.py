with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

# Find all occurrences of FrameLabel 'title_screen'
for m in re.finditer(r'<item type="FrameLabelTag"[^>]*name="title_screen"', content):
    pos = m.start()
    print("title_screen at pos:", pos)
    # Check parent tag: is it inside DefineSpriteTag?
    # Find preceding <item type=
    prev_tag = content.rfind('<item type="DefineSpriteTag"', 0, pos)
    prev_end = content.rfind('</item>', 0, pos)
    print(f"prev DefineSprite: {prev_tag}, prev end: {prev_end}")
    if prev_tag > prev_end:
        print("Inside DefineSprite!")
    else:
        print("OUTSIDE DefineSprite (on root timeline)!")
