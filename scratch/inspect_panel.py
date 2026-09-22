with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

for name in ["text_d", "text_0", "text_space"]:
    m = re.search(rf'<item type="ExportAssetsTag"[^>]*>.*?<names>.*?<item>{name}</item>.*?</names>.*?</item>', content, re.DOTALL)
    if m:
        # get tag id
        tag_chunk = m.group(0)
        print(f"Export for {name}:")
        # find matching tag id in <tags>
        print(tag_chunk)
