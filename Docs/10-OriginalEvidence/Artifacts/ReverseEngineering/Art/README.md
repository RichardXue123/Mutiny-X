# Mutiny 美术资源导出

使用便携 FFDec 26.3.0，于 2026-09-15 从原版 mutiny.swf 导出。产物保留在 Assets 外，尚未配置 Unity Sprite 导入。原 SWF 未修改，指纹见 metadata.json。

## 导出参数与复现

在 Unity 工程根目录运行：

```powershell
python Tools/ReverseEngineering/export_art.py
```

依赖工作区父目录 Tools 下的 FFDec 与 Java，符号索引读取先前导出的 Swf/mutiny.swf.xml。导出使用 zoom=1、透明背景和 onerror=abort。实际参数、返回码与日志保存在 commands.json 和 logs/；命令中的路径以 Unity 工程父目录（工作区根目录）为基准记录为相对路径。旧命令中的 `Mutiny X/Docs/ReverseEngineering/Art/` 是当时的输出位置，产物后来归档到本目录。重新运行会覆盖同名产物；改变来源或工具版本时应使用新的目录避免旧文件混入。

## 目录与清单

| 产物 | 内容 |
| --- | --- |
| raster/images/ | 700 张原始嵌入位图，PNG |
| raster/shapes/ | 756 个图形的 PNG 栅格化结果 |
| raster/sprites/ | 489 个有时间轴帧的 Sprite 的全部 5823 帧，PNG；另有 85 个零帧类载体 |
| raster/buttons/ | 7 个按钮的 28 个状态画面，PNG |
| raster/morphshapes/ | 11 个形变图形的 110 张采样 PNG |
| vector/ | 图形、Sprite 帧、按钮状态及形变图形的 SVG 对照，共 6618 文件 |
| symbols.csv | 2048 个美术定义的 symbol ID、tag 类型、链接名、时间轴帧数或位图尺寸 |
| png-files.csv | 每张 PNG 的来源、尺寸、颜色类型、透明通道能力、大小和 SHA256 |
| sprite-origins.csv | 配对 SVG 根变换中提取的原始注册点及候选 Unity pivot |
| frame-labels.csv | 209 个 Sprite 帧标签及所在帧 |
| placements.csv | 2503 个嵌套放置记录，包含父子 symbol ID、帧、深度及原始矩阵 |
| shape-bounds.csv | 图形边界，原 SWF twip 转为像素，保留负坐标 |
| audit-summary.json | PNG 校验及美术定义覆盖检查的数量 |
| missing-symbols.csv | 没有 PNG 来源对应的美术定义 |
| nonvisual-symbols.csv | 85 个零帧 AS2 类载体，不要求导出可绘制画面 |
| audit-errors.csv | 无法识别来源、PNG 损坏或注册点提取异常 |
| logs/ | 两次导出的 stdout 与 stderr |

此处 Flash Sprite 是时间轴容器，不等同于 Unity Sprite。symbols.csv 包括无链接名的内部资源，不能只保留有名称的文件。

## 注册点与尺寸

注册点是 Flash 局部原点在导出画布中的位置。PNG 尺寸不包含原点信息，不能直接使用中心 pivot。

Sprite SVG 的最外层矩阵在单位缩放下提供画布平移 tx/ty，清单将其作为原点距左侧/顶部的像素距离。候选 Unity pivot 为 `(tx / PNG宽, 1 - ty / PNG高)`，只是后续导入的依据，仍需检查 PNG/SVG 配对和画布舍入。原点可能位于图像外，下一阶段需使用补边或层级偏移，不能强制截断 pivot。

示例：

- grass_top_left（symbol 1528）画布 34×32，原点距左侧 2 像素、顶部 0 像素。其边缘超出 32 像素格子。
- cherryBomb（symbol 844）画布 20×32，原点位于左侧 10、顶部 22。
- redPirate（symbol 1324）SVG 宽 27.95、高 30，原点位于左侧 12、顶部 15；PNG 画布尺寸以清单为准。

原始矩阵完整保存在 placements.csv，shape-bounds.csv 可用于核对图形原点。位图 images 是图形填充的原料，不一定就是可直接显示的角色或 tile；后续映射通常需要使用链接 Sprite 的合成帧。

## 验证范围与限制

导出命令返回码均为 0，stdout 为 OK；两项 stderr 为空。抽查地形、海盗角色和爆炸帧可正常显示。逐文件检查 PNG 签名、chunk CRC、尺寸与 IDAT zlib 解压，并将文件来源对应回 symbols.csv。

最终结果：7417 个 PNG 全部通过校验，1963 个可绘制符号全部覆盖，另有 85 个零帧 AS2 类载体，无缺失符号、无校验错误。5823 个 Sprite 帧均有注册点候选记录。65 张原始位图没有透明通道，符合其 PNG 类型；透明背景参数不意味着原始不透明位图会自动去底色。详见 audit-summary.json。

文件完整性和符号覆盖不等于动画行为完全还原：

- 导出器不会执行原版 ActionScript；gotoAndPlay、动态 attachMovie、脚本绘制、运行时颜色和随机内容需另外还原。
- 保留了 Sprite 声明的全部 5823 个时间轴帧；单帧父容器中的嵌套动画不一定能从父容器单张 PNG 看出。
- 形变 PNG 为采样序列，不能直接当作原版固定帧率动画。原始 SVG 与 SWF XML 保留作为重建依据。
- 某些菜单、UI 或逻辑资源本来就可能透明，不应仅凭看起来空白判定导出失败。
- 未导出音频，Audio 目录仍待 TODO 16 接入阶段处理。

下一项：建立 `tile name → SWF symbol → Unity Sprite` 映射，从关卡所需资源开始，显式处理 antichest 和 Ship_anchor_4 的名称差异，然后配置 Unity 中的尺寸、注册点和导入参数。
