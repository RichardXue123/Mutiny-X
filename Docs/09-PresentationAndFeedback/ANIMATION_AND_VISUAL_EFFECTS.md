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
| 通用特效 | MovieClip 帧脚本在特定帧调用逻辑 | `MutinyExplosion`、`MutinySplashEffect`、`MutinySweepingFlame` 等各自推进帧序列 | 爆炸/火焰已实现；水花时长有差异 |
| 世界画面 | 水面颜色标签、背景多层视差、部分地形 MovieClip | `MutinyWaterSurface` 换帧；关卡背景/地形以 SpriteRenderer 组装 | 水面已接入；视差和地形换帧未实现 |
| UI | 根时间轴、SimpleButton 状态和颜色变换 | `MutinyGameHUD` / `MutinyFrontendController` 在 550×400 逻辑画布上绘制 | HUD 淡入、血条滑动、按钮态已实现 |
| 镜头 | `TileSystem.advanceScrolling` 跟随、回移、边缘滚动和边界 | `MutinyCameraController` | 已实现主要分支，待运行/真机验证 |

## 世界与环境规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| VIS-WATER-01 | 水面按 `skyColour` 选 `water1..3`；每种颜色使用独立循环帧 | `Water.as::Water`、`Controller.setSkyColour`；water 时间轴 | `MutinyWaterSurface.Initialize` | 分别构建三种天色，逐 tick 对照原版帧序及循环点 | Unity 已实现每色 10 帧循环，但 `BuildWater` 当前固定传 `skyColour=1`；待修复与运行验证 |
| VIS-SPLASH-01 | Solid 跨越水线时创建 splash，按天色选标签；三段在全局帧 19/43/67 调用 `cl.destroy()` | `Solid.as::splashCheck`；symbol 1780 帧脚本 | `MutinyWaterSurface.SpawnSplash`、`MutinySplashEffect` | 三种天色从对应标签播放，确认销毁帧不被当作可见帧 | Unity 当前每色固定播放 24 张导出图，比原版销毁点长；已知差异 |
| VIS-BG-01 | 战斗背景的 cloudBase/hills/frontClouds/backClouds 根据镜头 X/Y 以不同系数位移，水底背景跟随水面 | `Water.as::advance:18-31` | 当前无对应视差组件 | 移动镜头至关卡四角，对照各层坐标和循环周期 | 未实现 |
| VIS-TILE-01 | `boat_ripple_*` 和 `tile_ripple_*` 是 16 帧地形 MovieClip | `tile-mapping.csv`及对应 SWF 符号 | `MutinyTileDefinition.FrameCount`、`MutinyLevelBuilder.ResolveTileSprite` | 构建含 ripple 的关卡，连续 16 tick 对照帧序 | 资源已导入，运行时只加载 frame 1；未实现换帧 |
| VIS-FRONT-01 | 菜单水面每帧 X-8 并在 -64 循环；云和山以 -1/-2/-4 不同速度循环 | `MenuBackgroundAnim.as::onEnterFrame` | `MutinyFrontendController` | 标题页连续录制至各层首次循环，核对每帧位移 | 当前只绘制静态 `UI/Frontend/background`；未实现 |

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
| P0 | VIS-BG-01 | 战斗背景视差未实现 | 镜头移动时场景纵深与原版明显不同 |
| P0 | VIS-TILE-01 | 7 组 ripple 地形有 16 帧资源但只显示首帧 | 关卡环境动态缺失 |
| P1 | VIS-SPLASH-01 | 水花按 24 帧播放，未按原版销毁动作帧截断 | 水花寿命和尾帧不一致 |
| P1 | VIS-WATER-01 | 关卡水面固定使用 sky 1 | 后段关卡配色错误 |
| P1 | VIS-SMOKE-01 | 烟迹类已写，但 `Art/Effects/CannonSmokeTrail` 资源不存在；Cannonball/ParachuteBomb/RumBottle 的调用因此为空，CherryBomb/Dynamite 还未调用 | 五种投射物的原版烟迹均不可见 |
| P1 | VIS-FRONT-01 | 前端背景为静态合成图 | 菜单水面/云层/山体不动 |
| P2 | VIS-MAT-01 | Anchor 白化受 Unity 默认 Sprite 材质限制 | 落地结束的颜色过渡不能保证像素级一致 |

## 验收口径

1. 先对照导出帧和 DoAction，列出“可见帧”与“纯动作/透明分隔帧”。
2. 由实际生产入口触发，以 25 Hz 逐 tick 记录当前帧、显隐、位置、旋转、颜色和层级。
3. 每个效果同时检查生成帧、逻辑事件帧、循环/停止帧和销毁帧。
4. 验收记录分为“静态确认、已实现、实际测试通过、待运行验证、已知差异”；截至本基线日期，本文新增项均未做 Unity 录像对照。

## 后续实现顺序

1. 补齐关卡天色传递、水面与 splash 标签范围。
2. 为动画地形添加共享 25 Hz 换帧器，保留现有碰撞边界。
3. 补导出烟迹资源，恢复 Cannonball/ParachuteBomb/RumBottle 现有调用，再在 CherryBomb/Dynamite 原始调用点接入。
4. 实现战斗背景视差，再处理前端 `MenuBackgroundAnim`。
5. 建立原版与 Unity 同尺寸逐帧录像/截帧对照，完成后才更新为“实际测试通过”。
