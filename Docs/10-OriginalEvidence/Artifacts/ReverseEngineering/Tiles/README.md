# Mutiny Tile Mapping (任务 06)

本目录记录全 18 关全部 107 种 XML tile 标记到 SWF Symbol 及 Unity Sprite 的映射与注册点配置。

## 统计概览

- **Tile 总量**：107
- **视觉瓦片 (visual)**：106
  - 单帧瓦片：99（导入至 `Assets/Mutiny/Art/Tiles/Single/`）
  - 动画瓦片：7（各 16 帧水面/涟漪，导入至 `Assets/Mutiny/Art/Tiles/Animated/`）
- **逻辑标记 (logic)**：1（`antichest`，无视觉贴图）
- **未匹配数 (unmatched)**：0（见 `unmatched.csv`）
- **已复制贴图总文件数**：211

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
