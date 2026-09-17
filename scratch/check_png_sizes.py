import os
import struct

def get_png_size(file_path):
    with open(file_path, 'rb') as f:
        data = f.read(24)
        if len(data) >= 24 and data[:8] == b'\x89PNG\r\n\x1a\n':
            w, h = struct.unpack('>LL', data[16:24])
            return w, h
    return None

weapons_dir = "Assets/Mutiny/Resources/Art/Weapons"

for root, dirs, files in os.walk(weapons_dir):
    for f in sorted(files):
        if f.endswith('.png') and not f.endswith('.meta'):
            p = os.path.join(root, f)
            size = get_png_size(p)
            rel = os.path.relpath(p, weapons_dir)
            if f in ['1.png', 'rotating.png', 'overlay.png', '6.png']:
                print(f"{rel}: {size}")
