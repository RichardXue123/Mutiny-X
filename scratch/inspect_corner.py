with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

import re

idx = content.find('translateX="9818"')
# find previous FrameLabelTag
prev_label_idx = content.rfind('<item type="FrameLabelTag"', 0, idx)
print(content[prev_label_idx:prev_label_idx+200])
