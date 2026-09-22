import zlib
import struct

def print_ascii_png(path):
    with open(path, 'rb') as f:
        data = f.read()
    w, h = struct.unpack('>LL', data[16:24])
    pos = 8
    idat = b''
    while pos < len(data):
        length, chunk_type = struct.unpack('>L4s', data[pos:pos+8])
        if chunk_type == b'IDAT':
            idat += data[pos+8:pos+8+length]
        pos += 12 + length
    raw = bytearray(zlib.decompress(idat))
    
    stride = w * 4 + 1
    # PNG unfiltering (sub, up, average, paeth)
    pixels = []
    prev_row = bytearray(w * 4)
    for y in range(h):
        filter_type = raw[y * stride]
        curr_row = raw[y * stride + 1 : (y + 1) * stride]
        recon_row = bytearray(w * 4)
        for x in range(w * 4):
            filt = curr_row[x]
            a = recon_row[x - 4] if x >= 4 else 0
            b = prev_row[x]
            c = prev_row[x - 4] if x >= 4 else 0
            if filter_type == 0: val = filt
            elif filter_type == 1: val = (filt + a) & 0xFF
            elif filter_type == 2: val = (filt + b) & 0xFF
            elif filter_type == 3: val = (filt + ((a + b) >> 1)) & 0xFF
            elif filter_type == 4:
                p = a + b - c
                pa = abs(p - a)
                pb = abs(p - b)
                pc = abs(p - c)
                if pa <= pb and pa <= pc: pr = a
                elif pb <= pc: pr = b
                else: pr = c
                val = (filt + pr) & 0xFF
            recon_row[x] = val
        prev_row = recon_row
        
        row_str = ""
        for x in range(w):
            alpha = recon_row[x * 4 + 3]
            r = recon_row[x * 4]
            g = recon_row[x * 4 + 1]
            b = recon_row[x * 4 + 2]
            if alpha < 10:
                row_str += " "
            elif r > 200 and g > 200 and b > 200:
                row_str += "."
            elif r < 50 and g < 50 and b < 50:
                row_str += "#"
            else:
                row_str += "*"
        pixels.append(row_str)
    
    print(f"=== {path} ===")
    for y, line in enumerate(pixels):
        if line.strip():
            print(f"{y:02d}: {line}")

print_ascii_png('Assets/Mutiny/Resources/UI/CornerControls/quit_up.png')
print_ascii_png('Assets/Mutiny/Resources/UI/CornerControls/quit_over.png')
