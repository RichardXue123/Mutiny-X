import re

xml_path = "Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml"

print("Reading XML...")
with open(xml_path, "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

print("XML length:", len(content))

# 1. Main timeline FrameLabels
print("\n=== Main Timeline FrameLabels ===")
for m in re.finditer(r'<item type="FrameLabelTag"[^>]*name="([^"]+)"[^>]*>', content):
    pos = m.start()
    # find which frame this is in main timeline
    # count ShowFrameTag before pos that are outside DefineSpriteTag
    print("Label:", m.group(1))

# Let's find all frame labels with their surrounding tags
labels = []
for m in re.finditer(r'<item type="FrameLabelTag"[^>]*name="([^"]+)"', content):
    labels.append((m.start(), m.group(1)))

print("Total FrameLabels found:", len(labels))
for pos, name in labels:
    print(f"  Pos {pos}: {name}")
