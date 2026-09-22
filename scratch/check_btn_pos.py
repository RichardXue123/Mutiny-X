from measure_boxes import parse_png

w, h, img = parse_png("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/raster/sprites/DefineSprite_1855_weapon select/1.png")

# Let's inspect around x = 116 + 112 = 228
# Find button border pixels
for y in range(20, 70):
    idx = 230 * 4
    r, g, b, a = img[y][idx:idx+4]
    print(f"y={y:2d}: ({r:3d}, {g:3d}, {b:3d}, {a:3d})")
