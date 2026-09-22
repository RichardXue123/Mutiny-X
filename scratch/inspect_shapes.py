with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml', 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

import re

for bid in ['356', '352', '345', '355', '351', '344', '348', '347']:
    m = re.search(rf'<item type="DefineBitsLossless2Tag"[^>]*characterID="{bid}"[^>]*', content)
    if m:
        print(bid, m.group(0))
