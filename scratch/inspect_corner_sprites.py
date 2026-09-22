with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml', 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

import re

for cid in ['357', '353', '346']:
    print(f"=== DEFINITION FOR characterId={cid} ===")
    pattern = rf'<item type="([^"]+)"[^>]*shapeId="{cid}"[^>]*>([\s\S]*?)</item>'
    m = re.search(pattern, content)
    if not m:
        pattern = rf'<item type="([^"]+)"[^>]*spriteId="{cid}"[^>]*>([\s\S]*?)</item>'
        m = re.search(pattern, content)
    if m:
        print(m.group(0)[:1500])
    else:
        print("Not found with shapeId or spriteId, searching generic...")
        pattern = rf'<item[^>]*="{cid}"[^>]*>'
        for match in re.finditer(pattern, content):
            print(match.group(0))
    print("="*60)
