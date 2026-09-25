with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

for sid in ['670', '675', '587']:
    # search with any capitalization or tag type
    idx = content.find(f'shapeId="{sid}"')
    print(f"shapeId {sid}: idx = {idx}")
    if idx != -1:
        chunk = content[idx:idx+500]
        print(chunk[:250])
