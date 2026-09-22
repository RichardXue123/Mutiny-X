from measure_boxes import parse_png

w, h, img = parse_png(r"C:\Users\27487\.gemini\antigravity-ide\brain\afc973be-1aa4-4926-8fa3-55007508e07a\.user_uploaded\media_1790093496323.png")
print(f"Screenshot size: {w}x{h}")

# Search for the fire extinguisher slot or "05"
# Let's find white pixels near the top-right
for y in range(40, 150, 2):
    row_text = ""
    for x in range(250, 360, 2):
        idx = x * 4
        r, g, b, a = img[y][idx:idx+4]
        if r > 200 and g > 200 and b > 200:
            row_text += "#"
        elif r > 180 and g < 60:
            row_text += "R"
        elif r < 60 and g < 60 and b < 60:
            row_text += "."
        else:
            row_text += " "
    if "#" in row_text:
        print(f"y={y:3d}: {row_text}")
