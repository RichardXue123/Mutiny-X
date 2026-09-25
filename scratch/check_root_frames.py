import re

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

# Let's verify where root timeline frames are.
# In SWF, root tags are directly inside <tags>...</tags>.
# Let's see what tags are at the root level from 6880000 onwards.

# Let's find all FrameLabelTag from 6880000 to end
for m in re.finditer(r'<item type="FrameLabelTag"[^>]*name="([^"]+)"', content[6880000:]):
    actual_pos = 6880000 + m.start()
    name = m.group(1)
    print(f"FrameLabel '{name}' at pos {actual_pos}")
