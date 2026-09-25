import re

with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

m = re.search(r'<item type="PlaceObject2Tag"[^>]*characterId="330"[^>]*>.*?</item>', content, re.DOTALL)
if m:
    print("Character 330 place:")
    print(m.group(0))

m2 = re.search(r'<item type="PlaceObject2Tag"[^>]*characterId="333"[^>]*>.*?</item>', content, re.DOTALL)
if m2:
    print("Character 333 place:")
    print(m2.group(0))

m3 = re.search(r'<item type="PlaceObject2Tag"[^>]*characterId="319"[^>]*>.*?</item>', content, re.DOTALL)
if m3:
    print("Character 319 place:")
    print(m3.group(0))
