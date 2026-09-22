from measure_boxes import parse_png

w, h, img = parse_png("Assets/Mutiny/Resources/UI/weapon_slot_red_up.png")
print(f"weapon_slot_red_up.png: {w}x{h}")

# print colors for column 12 from top to bottom
for y in range(h):
    idx = 12 * 4
    r, g, b, a = img[y][idx:idx+4]
    print(f"y={y:2d}: ({r:3d}, {g:3d}, {b:3d}, {a:3d})")
