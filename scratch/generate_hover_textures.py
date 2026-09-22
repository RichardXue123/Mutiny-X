import zlib, struct, uuid

def transform_png(in_png, out_png, mode):
    with open(in_png, 'rb') as f:
        data = f.read()

    pos = 8
    ihdr_data = None
    idat_parts = []
    while pos < len(data):
        length, = struct.unpack('>I', data[pos:pos+4])
        ctype = data[pos+4:pos+8]
        cdata = data[pos+8:pos+8+length]
        pos += 12 + length
        if ctype == b'IHDR':
            ihdr_data = cdata
        elif ctype == b'IDAT':
            idat_parts.append(cdata)

    w, h, bitd, colortype, comp, filt, inter = struct.unpack('>IIBBBBB', ihdr_data)
    raw = zlib.decompress(b''.join(idat_parts))
    
    bpp = 4
    stride = 1 + w * bpp
    out_raw = bytearray()
    
    def paeth(a, b, c):
        p = a + b - c
        pa = abs(p - a)
        pb = abs(p - b)
        pc = abs(p - c)
        if pa <= pb and pa <= pc: return a
        elif pb <= pc: return b
        else: return c

    prev_scanline = bytearray(w * bpp)
    for y in range(h):
        filter_type = raw[y * stride]
        curr_scanline = bytearray(raw[y * stride + 1 : (y + 1) * stride])
        recon = bytearray(w * bpp)
        for x in range(w * bpp):
            filt_byte = curr_scanline[x]
            left = recon[x - bpp] if x >= bpp else 0
            up = prev_scanline[x]
            up_left = prev_scanline[x - bpp] if x >= bpp else 0
            if filter_type == 0: val = filt_byte
            elif filter_type == 1: val = (filt_byte + left) & 0xff
            elif filter_type == 2: val = (filt_byte + up) & 0xff
            elif filter_type == 3: val = (filt_byte + ((left + up) >> 1)) & 0xff
            elif filter_type == 4: val = (filt_byte + paeth(left, up, up_left)) & 0xff
            recon[x] = val
        prev_scanline = recon
        
        out_scanline = bytearray(w * bpp)
        for i in range(0, w * bpp, 4):
            r, g, b, a = recon[i], recon[i+1], recon[i+2], recon[i+3]
            if mode == 'button_over':
                if a > 0:
                    r_new = min(255, (r * 128 // 256) + 128)
                    g_new = min(255, (g * 128 // 256) + 128)
                    b_new = min(255, (b * 128 // 256) + 128)
                else:
                    r_new, g_new, b_new = r, g, b
            elif mode == 'white_mask':
                if a > 0:
                    r_new, g_new, b_new = 255, 255, 255
                else:
                    r_new, g_new, b_new = 0, 0, 0
            out_scanline[i] = r_new
            out_scanline[i+1] = g_new
            out_scanline[i+2] = b_new
            out_scanline[i+3] = a
            
        out_raw.append(0)
        out_raw.extend(out_scanline)
        
    compressed = zlib.compress(bytes(out_raw))
    
    import binascii
    def make_chunk(ctype, cdata):
        crc = binascii.crc32(ctype + cdata) & 0xffffffff
        return struct.pack('>I', len(cdata)) + ctype + cdata + struct.pack('>I', crc)
        
    out_png_data = b'\x89PNG\r\n\x1a\n'
    out_png_data += make_chunk(b'IHDR', ihdr_data)
    out_png_data += make_chunk(b'IDAT', compressed)
    out_png_data += make_chunk(b'IEND', b'')
    with open(out_png, 'wb') as f:
        f.write(out_png_data)
    print(f"Generated {out_png}: {w}x{h}")

transform_png('Assets/Mutiny/Resources/UI/Frontend/button_small.png', 'Assets/Mutiny/Resources/UI/Frontend/button_small_over.png', 'button_over')
transform_png('Assets/Mutiny/Resources/UI/Frontend/button_wide.png', 'Assets/Mutiny/Resources/UI/Frontend/button_wide_over.png', 'button_over')
transform_png('Assets/Mutiny/Resources/UI/Frontend/button_back.png', 'Assets/Mutiny/Resources/UI/Frontend/button_back_over.png', 'button_over')
transform_png('Assets/Mutiny/Resources/UI/Frontend/level_slot.png', 'Assets/Mutiny/Resources/UI/Frontend/level_slot_over.png', 'white_mask')
