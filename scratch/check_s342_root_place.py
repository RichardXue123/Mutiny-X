import re

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

m = re.search(r'<item type="PlaceObject2Tag"[^>]*characterId="342"[^>]*>.*?</item>', content, re.DOTALL)
if m:
    print("Placement of character 342 on root:")
    print(m.group(0))
