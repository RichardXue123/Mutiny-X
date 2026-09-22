import csv

weapons = ['anchor', 'banana', 'boulder', 'cannon', 'cannonball', 'cherryBomb', 'dynamite', 'gunpowderBarrel', 'mine', 'parachuteBomb', 'piecesOfEight', 'rumBottle', 'seagull', 'tidalWave', 'voodooDoll', 'woodenCrate']

symbols = {}
with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/symbols.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        linkage = row['linkage_names']
        for w in weapons:
            if w.lower() in linkage.lower():
                symbols[row['symbol_id']] = (w, linkage, row['timeline_frames'])
                print(f"Symbol {row['symbol_id']}: weapon={w}, linkage={linkage}, frames={row['timeline_frames']}")

print("\n--- Sprite Origins ---")
with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/sprite-origins.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        sym = row['symbol_id']
        if sym in symbols:
            print(f"Sym {sym} ({symbols[sym][1]}): path={row['raster_path']}, origin=({row['origin_x_from_left_px']}, {row['origin_y_from_top_px']}), size=({row['png_width']}x{row['png_height']}), pivot=({row['candidate_unity_pivot_x']}, {row['candidate_unity_pivot_y']})")
