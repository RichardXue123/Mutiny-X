with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

for sid in ['1954', '1957']:
    idx = content.find(f'spriteId="{sid}"')
    if idx == -1: idx = content.find(f'shapeId="{sid}"')
    chunk = content[idx:idx+1500]
    print(f"\nID {sid}:")
    print(chunk[:400])
