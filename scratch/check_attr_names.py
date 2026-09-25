with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

for cid in ['326', '328', '330', '333', '335', '337', '339', '341']:
    # search for 326
    matches = re.findall(rf'<item type="[^"]+"[^>]*="{cid}"[^>]*>', content)
    print(f"CID {cid}: matches = {matches[:3]}")
