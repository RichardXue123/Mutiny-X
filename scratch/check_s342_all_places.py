import re

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

for m in re.finditer(r'<item type="PlaceObject2Tag"[^>]*characterId="342"[^>]*>.*?</item>', content, re.DOTALL):
    print("Found placement of 342:")
    print(m.group(0))
