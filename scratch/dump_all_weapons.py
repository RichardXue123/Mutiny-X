import csv

weapons = ['anchor', 'banana', 'boulder', 'cannon', 'cannonball', 'cherrybomb', 'dynamite', 'gunpowderbarrel', 'mine', 'parachutebomb', 'piecesofeight', 'rumbottle', 'seagull', 'tidalwave', 'voodoodoll', 'woodencrate']

symbols = {}
with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/symbols.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        linkage = row['linkage_names']
        for w in weapons:
            if w in linkage.lower():
                symbols[row['symbol_id']] = (w, linkage, row['timeline_frames'])

print(f"Total weapon symbols found: {len(symbols)}")

origins = {}
with open('Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/sprite-origins.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        sym = row['symbol_id']
        if sym in symbols:
            w, linkage, frames = symbols[sym]
            if linkage not in origins:
                origins[linkage] = []
            origins[linkage].append(row)

for linkage, rows in sorted(origins.items()):
    print(f"\nLinkage: {linkage} (Symbol {rows[0]['symbol_id']}, {len(rows)} frames)")
    # Show first row and any unique sizes/origins
    unique_entries = set()
    for r in rows:
        key = (r['origin_x_from_left_px'], r['origin_y_from_top_px'], r['png_width'], r['png_height'], r['candidate_unity_pivot_x'], r['candidate_unity_pivot_y'])
        if key not in unique_entries:
            unique_entries.add(key)
            print(f"  Frame {r['raster_path']}: origin=({r['origin_x_from_left_px']}, {r['origin_y_from_top_px']}), size=({r['png_width']}x{r['png_height']}), pivot=({r['candidate_unity_pivot_x']}, {r['candidate_unity_pivot_y']})")
