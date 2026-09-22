from measure_boxes import parse_png

w, h, img = parse_png("Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/raster/sprites/DefineSprite_1855_weapon select/1.png")

# Inspect column 15 (inside button_throw, which starts at x=11 rel to panel)
# Panel top at col 150 was y=23
panel_top_y = 23
for y in range(20, 60):
    idx = (116 + 15) * 4
    r, g, b, a = img[y][idx:idx+4]
    if a > 0:
        print(f"y={y:2d} (rel={y-panel_top_y:2d}): ({r:3d}, {g:3d}, {b:3d}, {a:3d})")
