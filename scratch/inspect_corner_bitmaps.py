with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml', 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

import re
for cid in ['357', '353', '346', '355', '351', '344', '348']:
    m = re.search(rf'shapeId="{cid}"[\s\S]*?</item>', content)
    if m:
        bids = re.findall(r'bitmapId="(\d+)"', m.group(0))
        print(cid, 'bitmapIds:', bids)
        # also find bitmap dimensions
        for bid in bids:
            if bid != '65535':
                bm = re.search(rf'characterID="{bid}"[^>]*', content)
                if bm:
                    print('   bitmap:', bm.group(0))
