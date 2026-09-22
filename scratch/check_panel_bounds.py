from measure_boxes import parse_png

w, h, img = parse_png("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/raster/sprites/DefineSprite_1855_weapon select/1.png")

# Let's inspect column 150 (middle of panel) from top to bottom
for y in range(h):
    idx = 150 * 4
    r, g, b, a = img[y][idx:idx+4]
    if a > 0:
        print(f"First non-transparent at col 150 is y={y}: ({r}, {g}, {b}, {a})")
        break

for y in range(h-1, -1, -1):
    idx = 150 * 4
    r, g, b, a = img[y][idx:idx+4]
    if a > 0:
        print(f"Last non-transparent at col 150 is y={y}: ({r}, {g}, {b}, {a})")
        break
