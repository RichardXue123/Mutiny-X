from measure_boxes import parse_png

w, h, img = parse_png(r"C:\Users\27487\.gemini\antigravity-ide\brain\afc973be-1aa4-4926-8fa3-55007508e07a\.user_uploaded\media_1790093496323.png")

# find top of throw button: gray box around x=40..100, y=40..100
for y in range(40, 100):
    idx = 50 * 4
    r, g, b, a = img[y][idx:idx+4]
    print(f"y={y:2d}: ({r:3d}, {g:3d}, {b:3d})")
