import csv

symbols_to_check = ['844', '850', '853', '866', '869', '870', '871', '872', '881', '884', '918', '921', '939', '965', '968', '971', '982', '1003', '1024', '1027', '1058']

with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/sprite-origins.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        sym = row['symbol_id']
        if sym in symbols_to_check:
            print(f"Sym {sym}: path={row['raster_path']}, origin=({row['origin_x_from_left_px']}, {row['origin_y_from_top_px']}), size=({row['png_width']}x{row['png_height']}), pivot=({row['candidate_unity_pivot_x']}, {row['candidate_unity_pivot_y']})")
