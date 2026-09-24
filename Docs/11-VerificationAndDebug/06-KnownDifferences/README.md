# 11.06 · 已知差异

[返回上级模块](../README.md)

## 职责

记录未验证、未实现和用户授权扩展。

## 边界

不得通过修改规格隐藏差异。

## 动画与视觉效果差异（2026-09-23 静态基线）

| ID | 差异 | 证据/现状 | 验收状态 |
| --- | --- | --- | --- |
| VIS-BG-01 | 战斗背景视差已接入，仍需运行确认 | `Water.as::advance` 的系数、`Global.negativeModulo` 和 symbol 1998 图层已映射至 `MutinyBattleBackground` | 已实现；待 Unity 运行验证 |
| VIS-TILE-01 / VIS-TORCH-01 | ripple 与洞穴火把动画已接入，仍需运行确认 | `Tile.as::show` 全局相位与 torch symbol 1485 独立 24 帧由 `MutinyAnimatedTiles` 驱动；不改原碰撞 | 已实现；待 Unity 运行验证 |
| VIS-SPLASH-01 | Unity 每色播放 24 张 splash，原版在全局 frame 19/43/67 销毁 | symbol 1780 帧脚本与 `MutinySplashEffect` 对照 | 已知差异，待修复/运行验证 |
| VIS-WATER-01 | Unity 关卡水面固定 sky 1 | `BuildWater(...).Initialize(..., 1)` | 已知差异 |
| VIS-SMOKE-01 | 原版五种投射物使用 `cannonSmokeTrail`；Unity 已接入资源及五种调用，尚待运行对照 | 原版五个 AS2 `advance`、symbol 866 frame 19；Unity `CannonSmokeTrail/1..19`、`MutinyRumBottleSmokeTrail` 与五个生产入口 | 已接入；待 Unity Play Mode 验证 |
| VIS-FRONT-01 | 前端水面、云、山体循环已接入，仍需运行确认 | `MenuBackgroundAnim.as` 的五层位移由 `MutinyFrontendController` 以 25 Hz 驱动 | 已实现；待 Unity 运行验证 |
| VIS-MAT-01 | Anchor whiteOut 受默认 Sprite 材质限制 | `MutinyAnchor` 当前实现注记 | 已知差异 |
| CHAR-ANI-TINT-01 | Unity 受伤额外叠 0.2 秒红色 tint，静态原版证据只确认 hit 时间轴 | `MutinyCharacter.TakeDamage`与 `Character.as::advance` | 待原版运行取证 |

完整上下文、实现入口和优先级见 [动画与视觉效果总览](../../09-PresentationAndFeedback/ANIMATION_AND_VISUAL_EFFECTS.md)。本表不包含用户授权的 Android 适配；授权扩展应继续在各自规格中单独标注。
