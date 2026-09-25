with open("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml", "r", encoding="utf-8", errors="ignore") as f:
    content = f.read()

idx = content.find('shapeId="326"')
print(content[idx:idx+500])

idx2 = content.find('spriteId="330"')
print(content[idx2:idx2+1000])
