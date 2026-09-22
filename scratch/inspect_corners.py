with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml', 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

import re

for m in re.finditer(r'<item[^>]*characterId="358"[^>]*>', content):
    pos = m.start()
    print("Found at", pos)
    # Find nearest preceding frame label
    fl_matches = list(re.finditer(r'<item type="FrameLabelTag"[^>]*name="([^"]+)"', content[:pos]))
    if fl_matches:
        print("Preceding frame label:", fl_matches[-1].group(1))
    print(content[pos:pos+400])
    print("="*60)
