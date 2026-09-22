import zlib
import struct

def parse_png(file_path):
    with open(file_path, 'rb') as f:
        data = f.read()
    assert data[:8] == b'\x89PNG\r\n\x1a\n'
    pos = 8
    width = height = None
    idat = b''
    while pos < len(data):
        length, chunk_type = struct.unpack('>I4s', data[pos:pos+8])
        pos += 8
        chunk_data = data[pos:pos+length]
        pos += length + 4
        if chunk_type == b'IHDR':
            width, height, bit_depth, color_type, compression, filter_method, interlace = struct.unpack('>IIBBBBB', chunk_data)
        elif chunk_type == b'IDAT':
            idat += chunk_data
        elif chunk_type == b'IEND':
            break
            
    raw = zlib.decompress(idat)
    bpp = 4
    stride = width * bpp
    img = []
    prev_line = bytearray(stride)
    raw_pos = 0
    for y in range(height):
        filter_type = raw[raw_pos]
        raw_pos += 1
        line = bytearray(raw[raw_pos:raw_pos+stride])
        raw_pos += stride
        if filter_type == 1:
            for x in range(bpp, stride):
                line[x] = (line[x] + line[x-bpp]) & 0xff
        elif filter_type == 2:
            for x in range(stride):
                line[x] = (line[x] + prev_line[x]) & 0xff
        elif filter_type == 3:
            for x in range(stride):
                left = line[x-bpp] if x >= bpp else 0
                up = prev_line[x]
                line[x] = (line[x] + ((left + up) >> 1)) & 0xff
        elif filter_type == 4:
            for x in range(stride):
                a = line[x-bpp] if x >= bpp else 0
                b = prev_line[x]
                c = prev_line[x-bpp] if x >= bpp else 0
                p = a + b - c
                pa = abs(p - a)
                pb = abs(p - b)
                pc = abs(p - c)
                if pa <= pb and pa <= pc:
                    pr = a
                elif pb <= pc:
                    pr = b
                else:
                    pr = c
                line[x] = (line[x] + pr) & 0xff
        prev_line = line
        img.append(line)
    return width, height, img

w, h, img = parse_png("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/raster/sprites/DefineSprite_1855_weapon select/1.png")
print(f"1.png size: {w}x{h}")

white_pixels = []
for y in range(h):
    for x in range(w):
        idx = x * 4
        r, g, b, a = img[y][idx:idx+4]
        if r == 255 and g == 255 and b == 255 and a == 255:
            white_pixels.append((x, y))

box1 = [p for p in white_pixels if p[1] < 100]
box2 = [p for p in white_pixels if p[1] >= 100]

def get_bounds(pixels):
    min_x = min(p[0] for p in pixels)
    max_x = max(p[0] for p in pixels)
    min_y = min(p[1] for p in pixels)
    max_y = max(p[1] for p in pixels)
    return min_x, min_y, max_x - min_x + 1, max_y - min_y + 1

panel_pixels = []
for y in range(h):
    for x in range(w):
        idx = x * 4
        a = img[y][idx+3]
        if a > 0:
            panel_pixels.append((x, y))

px, py, pw, ph = get_bounds(panel_pixels)
print(f"Panel bounds in 1.png: x={px}, y={py}, w={pw}, h={ph}")

if box1:
    bx1, by1, bw1, bh1 = get_bounds(box1)
    print(f"White Box 1 (title placeholder): x={bx1}, y={by1}, w={bw1}, h={bh1}")
    print(f"Box 1 rel to panel: x={bx1 - px}, y={by1 - py}, w={bw1}, h={bh1}")
if box2:
    bx2, by2, bw2, bh2 = get_bounds(box2)
    print(f"White Box 2 (desc placeholder):  x={bx2}, y={by2}, w={bw2}, h={bh2}")
    print(f"Box 2 rel to panel: x={bx2 - px}, y={by2 - py}, w={bw2}, h={bh2}")
