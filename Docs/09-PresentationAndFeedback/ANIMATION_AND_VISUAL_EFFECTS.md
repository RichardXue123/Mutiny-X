# 动画与视觉效果总览

[返回表现与反馈](README.md)

## 目的与范围

本文是项目动画、时间轴和画面效果的跨模块索引。它记录可观察的帧序列、显隐、旋转、缩放、透明度、颜色、层级、水面/背景和镜头反馈；伤害、物理、库存与回合结束仍归权威逻辑模块。

详细规格分流到：

- [角色动画](05-CharacterAnimation/README.md)
- [武器画面效果](06-WeaponEffects/README.md)
- [镜头](04-Camera/README.md)
- [战斗 HUD](01-BattleHUD/README.md)
- [前端 UI](03-FrontendUI/README.md)
- [光标与轨迹](08-CursorsAndTrajectory/README.md)
- [宝箱下落与落地](../07-DynamicWorldEvents/02-ChestDescentAndLanding/README.md)

## 取证基线

| 证据类型 | 原版入口 | 用途 |
| --- | --- | --- |
| AS2 逻辑 | `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/` | 确认何时切标签、创建/销毁效果、切换颜色及跟随目标 |
| 时间轴帧脚本 | `.../deobfuscated/scripts/DefineSprite_*/frame_*/DoAction.as` | 确认 `stop`、`gotoAndPlay`、`cl.hit`、`cl.destroy`和 `cl.createNext` 的精确帧 |
| 导出帧 | `Assets/Mutiny/Resources/Art/` | 确认 Unity 实际可加载的帧数、尺寸与透明分隔帧 |
| 符号/锚点 | `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Art/` 和 `Swf/` | 确认注册点、复合子时间轴与显示层级 |

不从 Unity 帧数、注释或旧 TODO 反推原版；本文中的原版结论均可回到上述证据。

## 实现架构摘要

| 系统 | 原版实现 | Unity 实现 | 当前状态 |
| --- | --- | --- | --- |
| 时钟 | Flash 主循环与 MovieClip 时间轴，项目基线 25 Hz | 多数战斗动画以 `MutinyPhysics.TimeStep` 或 `FrameRate=25` 推进 | 已实现，帧级录像对照待运行 |
| 像素风格 | SWF 矢量/位图组合 | PNG 序列 + `SpriteRenderer`，运行生成 Sprite 时使用 `FilterMode.Point` 与 32 PPU | 大部分已接入 |
| 角色 | 每种角色 35 帧复合时间轴 | 27 种角色各 35 张 PNG，`MutinyCharacterAnimator` 直接换帧 | 已实现，待 Play Mode |
| 通用特效 | MovieClip 帧脚本在特定帧调用逻辑 | `MutinyExplosion`、`MutinySplashEffect`、`MutinySweepingFlame` 等各自推进帧序列 | 水花按原版销毁帧和跨水线条件接入；待 Play Mode 验证 |
| 世界画面 | 水面颜色标签、背景多层视差、部分地形 MovieClip | `MutinyWaterSurface`、`MutinyBattleBackground`、`MutinyAnimatedTiles` | 背景视差与 ripple/火把动画已接入，待 Unity 运行验证 |
| UI | 根时间轴、SimpleButton 状态和颜色变换 | `MutinyGameHUD` / `MutinyFrontendController` 在 550×400 逻辑画布上绘制 | HUD 淡入、血条滑动、按钮态已实现 |
| 镜头 | `TileSystem.advanceScrolling` 跟随、回移、边缘滚动和边界 | `MutinyCameraController` | 已实现主要分支，待运行/真机验证 |

## 世界与环境规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| VIS-WATER-01 | `TileSystem.loadLevel` 按关卡设置 `skyColour`，`Controller.setSkyColour` 同时切换背景和 `water1..3`；水面在相机移动时每 49 px 对齐，每色 10 帧循环。导出水面 1024 px 宽，水下不透明/半透明画面只到 x=979；对应纯色 `waterBackground` 在原版宽 550 px | `TileSystem.as::loadLevel`、`Controller.setSkyColour`、`Water.as::Water/advance`；symbol 1651、`sprite-origins.csv`、导出 PNG、原版 `waterBackground1..3` | `MutinyLevelBuilder.BuildWater`、`MutinyLevelRoot.EnsureRuntimeWater`、`MutinyWaterSurface.Initialize` | 构建关卡 1/6/11，检查对应首帧/背景色、980 px 有效画面连续铺设、全宽底色；逐 tick 对照帧序 | 三色已传至新关卡和旧场景补建；按原版有效宽度裁去 44 px 透明尾部，关卡范围延展原版纯色水底背景；待 Unity Play Mode 验证 |
| VIS-SPLASH-01 | `Solid.splashCheck` 在注册点 `y < water.y` 状态变化时，于物体当前 X、水线 Y 生成水花并播放音效；等于水线算水下；按天色播放 1..18 / 25..42 / 49..66，可见段后的全局帧 19/43/67 执行 `cl.destroy()`；水面重新 `show()` 后覆盖同层 splash | `Solid.as::splashCheck`、`Clip.as::show/hide`、`Character.as::advance`、`Weapon.as::advance`；symbol 1780 帧脚本、`sprite-origins.csv`、导出 PNG | `MutinyWaterSurface.CheckSplashCrossing/SpawnSplash`、`MutinySplashEffect`；角色和普通武器物理帧及专用武器路径 | 生产角色恰好到水线时核对坐标、溺水时不重复生成；三色检查水花低于水面的层级、首帧/第 18 帧和第 19 帧销毁 | 已接入；现有 PNG 与原版导出逐字节一致；Unity Play Mode 待运行 |
| VIS-BG-01 | 战斗背景的 cloudBase/hills/frontClouds/backClouds 根据镜头 X/Y 以不同系数位移，水底背景跟随水面 | `Water.as::advance:18-31`、`Global.negativeModulo`、symbol 1998 的三色图层 | `MutinyBattleBackground`、`MutinyOriginalBackground.BattleLayerPosition` | 在关卡 1/6/11/16 移动镜头至四角，逐帧比较注册点、层级、位移和循环边界 | 已接入；原版首帧图层重组像素一致；待 Unity Play Mode 对照 |
| VIS-TILE-01 | `boat_ripple_*` 和 `tile_ripple_*` 七组各 16 帧；`Tile.show` 在显示时跳到 `1 + animationCounter % 16`，之后正常播放 | `Tile.as::show`、`TileSystem.as::advance/panCamera`、`tile-mapping.csv` 和符号 741/707/724/1901/1592/1609/1575 | `MutinyAnimatedTiles` 从 `BuildBackground/BuildTerrain` 注册，共用 25 Hz 帧计数 | 在含七组 ripple 的关卡逐 tick 记录 1..16→1；镜头移出再移入检查相位；确认碰撞未变化 | 112 张资源与原版导出逐字节一致；已接入；待 Unity Play Mode 验证 |
| VIS-TORCH-01 | `cave_torch` 外层只有 1 帧，内层 symbol 1485 火焰有 24 帧，位于 `(14,13)` px；移出视野销毁、重新显示从第 1 帧播放 | symbol 1486 的 placements、1485 的 24 帧导出、`Clip.show/hide` 和 `Tile.show` | `MutinyAnimatedTiles.RegisterTorch`、`Art/Tiles/Single/cave_torch_base`、`Art/Tiles/Animated/cave_torch_flame` | 静态底座 + 24 帧火焰逐帧对照；移出/移入视野确认火焰重新从 1 开始；碰撞保持原值 | 底座与火焰 frame 1 重组像素等于原版首帧；已接入；待 Unity Play Mode 验证 |
| VIS-FRONT-01 | 菜单水面每帧 X-8 并在 -64 循环；后云 -1/900、山 -2/840、前云 -4/1000、云底 -2/550 各自回卷 | `MenuBackgroundAnim.as::onEnterFrame`；symbols 191/187/190 | `MutinyFrontendController.Update/DrawAnimatedBackground` | 标题、模式及关卡选择页连续录制至各层首次循环，核对每帧位移 | 已接入；原版首帧图层重组像素一致；待 Unity Play Mode 对照 |

## 表现事件与权威状态

```text
玩法/物理状态转换
        │
        ├── 角色状态 ──▶ 待机/受击/死亡帧，旋转，颜色闪烁
        ├── 武器事件 ──▶ 投射物帧、爆炸、烟迹、火焰、水花
        ├── 回合/血量 ──▶ HUD 淡入、血条滑帧、结算数字
        └── 镜头目标 ──▶ 跟随/回移/手动滚动
```

表现组件不得自行改变伤害、库存、行动资格或回合阶段。原版把逻辑放在时间轴帧脚本中时（如爆炸第 3 帧命中、火焰第 4 帧传播），Unity 可由同一 25 Hz 驱动器调用权威逻辑，但必须保留原帧时序。

## 已知差异与优先级

| 优先级 | ID | 差异 | 影响 |
| --- | --- | --- | --- |
| P0 | VIS-BG-01 | 已接入原版分层资源与视差公式，待 Unity Play Mode 核对 | 镜头移动与不同屏幕比例下的实际画面尚未确认 |
| P0 | VIS-TILE-01 / VIS-TORCH-01 | ripple 和火把子时间轴已接入，待 Unity Play Mode 核对 | 运行时帧相位与进出视野时机尚未逐帧确认 |
| P1 | VIS-SPLASH-01 | 已修正跨线触发、图像注册点、位于水面之下的层级与第 19/43/67 帧销毁，待 Unity Play Mode 核对 | 生产关卡中的入水/出水录像尚未确认 |
| P1 | VIS-WATER-01 | 已按关卡传递水面天色并修复 44 px 透明尾部造成的拼接缺口，待 Unity Play Mode 核对 | 超宽 Scene View 下的底色和实际关卡镜头仍需截图对照 |
| P1 | VIS-SMOKE-01 | 五种投射物烟迹已接入，待 Unity Play Mode 核对 | 生产场景中的位置和寿命尚未逐帧确认 |
| P1 | VIS-FRONT-01 | 已接入原版分层资源和 25 Hz 循环，待 Unity Play Mode 核对 | 各层回卷时的实际画面尚未确认 |
| P2 | VIS-MAT-01 | Anchor 已接入原版 RGB 加白和 alpha 变换材质，参数专项通过；实际画面对照未完成 | 像素级颜色过渡仍待人工截图核对 |

## 验收口径

1. 先对照导出帧和 DoAction，列出“可见帧”与“纯动作/透明分隔帧”。
2. 由实际生产入口触发，以 25 Hz 逐 tick 记录当前帧、显隐、位置、旋转、颜色和层级。
3. 每个效果同时检查生成帧、逻辑事件帧、循环/停止帧和销毁帧。
4. 验收记录分为“静态确认、已实现、实际测试通过、待运行验证、已知差异”；截至本基线日期，本文新增项均未做 Unity 录像对照。

## 后续实现顺序

1. 在 Unity Play Mode 逐色验证关卡水面、完整循环与 splash 标签范围，并对照超宽 Scene View 的拼接效果。
2. 在 Unity 中按原版 25 Hz 对照 ripple 和火把子时间轴的完整循环及进出视野帧序，确认现有碰撞边界。
3. 补导出烟迹资源，恢复 Cannonball/ParachuteBomb/RumBottle 现有调用，再在 CherryBomb/Dynamite 原始调用点接入。
4. 在 Unity 中对照战斗背景视差和前端 `MenuBackgroundAnim` 的完整循环录像；逐色检查原版图层。
5. 建立原版与 Unity 同尺寸逐帧录像/截帧对照，完成后才更新为“实际测试通过”。
