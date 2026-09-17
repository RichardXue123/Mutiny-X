import csv

for sym in ['869', '870', '871', '872']:
    with open('Docs/ReverseEngineering/Art/symbols.csv', 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            if row['symbol_id'] == sym:
                print(f"symbols.csv: Sym {sym}: {row}")

    with open('Docs/ReverseEngineering/Art/sprite-origins.csv', 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            if row['symbol_id'] == sym:
                print(f"sprite-origins.csv: Sym {sym}: {row['raster_path']}, origin=({row['origin_x_from_left_px']}, {row['origin_y_from_top_px']}), size=({row['png_width']}x{row['png_height']}), pivot=({row['candidate_unity_pivot_x']}, {row['candidate_unity_pivot_y']})")
