"""Map 107 XML level tiles to SWF symbols, compute Unity pivots, and copy textures to Assets/Mutiny/Art/Tiles."""
import csv
import hashlib
import json
import pathlib
import shutil

PROJECT = pathlib.Path(__file__).resolve().parents[2]
DOCS = PROJECT / 'Docs'
ART_DOCS = DOCS / 'ReverseEngineering/Art'
LEVEL_SCAN = DOCS / 'LevelScan'
OUTPUT_DOCS = DOCS / 'ReverseEngineering/Tiles'
ASSETS_TILES = PROJECT / 'Assets/Mutiny/Art/Tiles'
ASSETS_DATA_TILES = PROJECT / 'Assets/Mutiny/Data/Tiles'

# Explicit special handling
SPECIAL_MAPPINGS = {
    'antichest': {
        'kind': 'logic',
        'symbol_id': '',
        'linkage_name': '',
        'notes': 'Pure logic tile in XML; used in TileSystem.getValidDropColumns to disallow chest drops.'
    },
    'ship_anchor_4': {
        'kind': 'visual',
        'linkage_name': 'Ship_anchor_4',
        'notes': 'Case difference between XML (ship_anchor_4) and SWF linkage (Ship_anchor_4).'
    }
}


def main():
    OUTPUT_DOCS.mkdir(parents=True, exist_ok=True)
    single_dir = ASSETS_TILES / 'Single'
    animated_dir = ASSETS_TILES / 'Animated'
    single_dir.mkdir(parents=True, exist_ok=True)
    animated_dir.mkdir(parents=True, exist_ok=True)

    # 1. Read tile types
    tiles = []
    with open(LEVEL_SCAN / 'tile-types.csv', 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            tiles.append(row['tile'])
    print(f'Loaded {len(tiles)} tiles from tile-types.csv')

    # 2. Read symbols
    symbols_by_name = {}
    with open(ART_DOCS / 'symbols.csv', 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            names = [n for n in row['linkage_names'].split(';') if n]
            for name in names:
                symbols_by_name[name] = row

    # 3. Read sprite origins
    origins_by_symbol = {}
    with open(ART_DOCS / 'sprite-origins.csv', 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            sym_id = row['symbol_id']
            origins_by_symbol.setdefault(sym_id, []).append(row)

    # Sort origins by frame order (extracting number from raster_path, e.g. 1.png, 2.png)
    for sym_id, origin_list in origins_by_symbol.items():
        def frame_key(item):
            filename = pathlib.Path(item['raster_path']).stem
            try:
                return int(filename)
            except ValueError:
                return 0
        origin_list.sort(key=frame_key)

    mapping_records = []
    unmatched_records = []
    copied_files_count = 0

    for tile in tiles:
        special = SPECIAL_MAPPINGS.get(tile)
        if special and special['kind'] == 'logic':
            mapping_records.append({
                'tile_name': tile,
                'kind': 'logic',
                'symbol_id': '',
                'linkage_name': '',
                'frame_count': 0,
                'png_width': 0,
                'png_height': 0,
                'origin_x_from_left_px': 0.0,
                'origin_y_from_top_px': 0.0,
                'candidate_unity_pivot_x': 0.0,
                'candidate_unity_pivot_y': 0.0,
                'dest_asset_path': '',
                'notes': special['notes']
            })
            continue

        linkage_name = special['linkage_name'] if special else tile
        if linkage_name not in symbols_by_name:
            unmatched_records.append({'tile_name': tile, 'reason': 'Linkage name not found in symbols.csv'})
            continue

        sym = symbols_by_name[linkage_name]
        sym_id = sym['symbol_id']
        origins = origins_by_symbol.get(sym_id, [])

        if not origins:
            unmatched_records.append({'tile_name': tile, 'reason': f'No origin records for symbol {sym_id}'})
            continue

        frame_count = len(origins)
        first_frame = origins[0]
        w = int(first_frame['png_width'])
        h = int(first_frame['png_height'])
        ox = float(first_frame['origin_x_from_left_px'])
        oy = float(first_frame['origin_y_from_top_px'])
        piv_x = float(first_frame['candidate_unity_pivot_x'])
        piv_y = float(first_frame['candidate_unity_pivot_y'])

        notes = special['notes'] if special else ''

        if frame_count == 1:
            dest_file = single_dir / f'{tile}.png'
            src_file = ART_DOCS / 'raster' / first_frame['raster_path']
            shutil.copy2(src_file, dest_file)
            copied_files_count += 1
            dest_asset_path = f'Assets/Mutiny/Art/Tiles/Single/{tile}.png'
        else:
            tile_anim_dir = animated_dir / tile
            tile_anim_dir.mkdir(parents=True, exist_ok=True)
            for idx, f_info in enumerate(origins, start=1):
                src_file = ART_DOCS / 'raster' / f_info['raster_path']
                dest_file = tile_anim_dir / f'{tile}_{idx:02d}.png'
                shutil.copy2(src_file, dest_file)
                copied_files_count += 1
            dest_asset_path = f'Assets/Mutiny/Art/Tiles/Animated/{tile}/'

        mapping_records.append({
            'tile_name': tile,
            'kind': 'visual',
            'symbol_id': sym_id,
            'linkage_name': linkage_name,
            'frame_count': frame_count,
            'png_width': w,
            'png_height': h,
            'origin_x_from_left_px': ox,
            'origin_y_from_top_px': oy,
            'candidate_unity_pivot_x': piv_x,
            'candidate_unity_pivot_y': piv_y,
            'dest_asset_path': dest_asset_path,
            'notes': notes
        })

    # Save mapping CSV to Docs and Assets
    ASSETS_DATA_TILES.mkdir(parents=True, exist_ok=True)
    mapping_csv_path = OUTPUT_DOCS / 'tile-mapping.csv'
    data_mapping_csv_path = ASSETS_DATA_TILES / 'tile-mapping.csv'
    fieldnames = [
        'tile_name', 'kind', 'symbol_id', 'linkage_name', 'frame_count',
        'png_width', 'png_height', 'origin_x_from_left_px', 'origin_y_from_top_px',
        'candidate_unity_pivot_x', 'candidate_unity_pivot_y', 'dest_asset_path', 'notes'
    ]
    for p in [mapping_csv_path, data_mapping_csv_path]:
        with open(p, 'w', newline='', encoding='utf-8') as f:
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(mapping_records)

    # Save unmatched CSV
    unmatched_csv_path = OUTPUT_DOCS / 'unmatched.csv'
    with open(unmatched_csv_path, 'w', newline='', encoding='utf-8') as f:
        writer = csv.DictWriter(f, fieldnames=['tile_name', 'reason'])
        writer.writeheader()
        writer.writerows(unmatched_records)

    # Save README
    visual_count = sum(1 for m in mapping_records if m['kind'] == 'visual')
    single_count = sum(1 for m in mapping_records if m['kind'] == 'visual' and m['frame_count'] == 1)
    animated_count = sum(1 for m in mapping_records if m['kind'] == 'visual' and m['frame_count'] > 1)
    logic_count = sum(1 for m in mapping_records if m['kind'] == 'logic')

    readme_content = f"""# Mutiny Tile Mapping (任务 06)

本目录记录全 18 关全部 107 种 XML tile 标记到 SWF Symbol 及 Unity Sprite 的映射与注册点配置。

## 统计概览

- **Tile 总量**：107
- **视觉瓦片 (visual)**：{visual_count}
  - 单帧瓦片：{single_count}（导入至 `Assets/Mutiny/Art/Tiles/Single/`）
  - 动画瓦片：{animated_count}（各 16 帧水面/涟漪，导入至 `Assets/Mutiny/Art/Tiles/Animated/`）
- **逻辑标记 (logic)**：{logic_count}（`antichest`，无视觉贴图）
- **未匹配数 (unmatched)**：{len(unmatched_records)}（见 `unmatched.csv`）
- **已复制贴图总文件数**：{copied_files_count}

## 特殊映射处理

1. **antichest**：
   - 原版代码 `TileSystem.getValidDropColumns` 证实此标记为关卡列宝箱掉落限制标记，无可视贴图，标记为 `logic`。
2. **ship_anchor_4**：
   - XML 标记为 `ship_anchor_4`，SWF 符号链接名为 `Ship_anchor_4`（首字母大写），Symbol ID 为 1898。
3. **eartyh_bottom_middle**：
   - 原版 XML 笔误保持原样映射到同名 SWF 链接 `eartyh_bottom_middle`。

## 注册点与 Pivot 计算

- Flash 舞台瓦片定位以左上角 `(gridX * 32, gridY * 32)` 为局部原点 `(0, 0)`。
- 部分瓦片存在局部边界外扩（如 `grass_top_left` 34×32，左偏移 2px；`big_shell` 36×22，上偏移 -10px）。
- Unity 归一化 Pivot 公式：
  - `pivot_x = origin_x / png_width`
  - `pivot_y = 1.0 - (origin_y / png_height)`
- 在 Unity 中将 Sprite 的 Pivot 设置为上述精确数值后，放置在 `(gridX, -gridY)` 网格坐标时将与原版 Flash 像素级对齐。
"""
    (OUTPUT_DOCS / 'README.md').write_text(readme_content, encoding='utf-8')

    print(f'Done: {visual_count} visual ({single_count} single, {animated_count} animated), {logic_count} logic.')
    print(f'Unmatched: {len(unmatched_records)}. Total copied PNGs: {copied_files_count}.')


if __name__ == '__main__':
    main()
