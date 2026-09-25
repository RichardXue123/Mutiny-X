# 10.02 · SWF 时间轴

[返回上级模块](../README.md)

## 职责

索引帧标签、嵌套 MovieClip、按钮状态和帧事件。

## 边界

不以单张导出图替代动态时间轴。

## 动画与特效关键时间轴索引

| 稳定 ID | 符号/时间轴 | 已确认帧动作 | 下游规格 |
| --- | --- | --- | --- |
| CHAR-ANI-01/02 | 27 种角色符号，例如 1077 `bluePirate`、1324 `redPirate` | frame 13 `gotoAndPlay("static")`；frame 35 同样返回 `static` | [角色动画](../../09-PresentationAndFeedback/05-CharacterAnimation/README.md) |
| CHAR-ANI-04 | 1065 `deadCharacter` | frame 24 `stop()` | [角色动画](../../09-PresentationAndFeedback/05-CharacterAnimation/README.md) |
| VIS-EXP-01 | 1785 `explosion` | frame 3 `cl.hit()`；frame 8 `stop(); cl.destroy()` | [武器画面效果](../../09-PresentationAndFeedback/06-WeaponEffects/README.md) |
| VIS-SMOKE-01 | 866 `cannonSmokeTrail` | frame 19 `cl.destroy()` | [武器画面效果](../../09-PresentationAndFeedback/06-WeaponEffects/README.md) |
| VIS-FLAME-01 | 905 `sweepingFlame` | frame 4 `cl.createNext()`；frame 11 `cl.destroy()` | [武器画面效果](../../09-PresentationAndFeedback/06-WeaponEffects/README.md) |
| VIS-SPLASH-01 | 1780 `splash` | 三个天色段分别在全局 frame 19/43/67 `cl.destroy()` | [动画与视觉效果总览](../../09-PresentationAndFeedback/ANIMATION_AND_VISUAL_EFFECTS.md) |
| MIN-ANI-01 | 1024 `mine` | frame 1 停在 in_throw；frame 16 `stop()`；frame 30 `gotoAndPlay("warn")` | [Mine](../../06-WeaponsAndEffects/11-Mine/README.md) |
| PCB-ANI-01/02 | 939 `parachuteBomb` + 930 引信子符号 | frame 1 外层 `stop()`；frame 30 `gotoAndPlay("open")`；子时间轴 1..4 循环 | [Parachute Bomb](../../06-WeaponsAndEffects/12-ParachuteBomb/README.md) |
| TID-ANI-01 | 1058 `tidalWave` | frame 6/15/24 分别跳回 `anim1/2/3` | [Tidal Wave](../../06-WeaponsAndEffects/16-TidalWave/README.md) |
| CRT-EXP-02 | 965 `woodenCrate` | frame 1 `stop()`；frame 18 `stop(); cl.destroy()` | [Wooden Crate](../../06-WeaponsAndEffects/18-WoodenCrate/README.md) |
| GPB-ANI-01 | 968 `gunpowderBarrel` | frame 1 `stop()`；frame 12 `stop(); cl.destroy()` | [Gunpowder Barrel](../../06-WeaponsAndEffects/10-GunpowderBarrel/README.md) |
| ANC-ANI-01/02 | 1003 `anchor` 嵌套 1002 | 主 frame 1/12 `stop()`；主 frame 3 在 x=±25、y=-12 放置 1002 双侧镜像子符号；子 frame 17 移除内容、frame 18 停止 | [Anchor](../../06-WeaponsAndEffects/19-Anchor/README.md) |
| CHEST-ANI-01 | 1725 `treasureChest` | frame 1 停止；34 回 static；43/80 停止；90 销毁 | [宝箱下落与落地](../../07-DynamicWorldEvents/02-ChestDescentAndLanding/README.md) |

证据文件位于 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/DefineSprite_*/frame_*/DoAction.as`。“帧图已导出”不代表该帧在 Flash 中会被呈现；执行立即跳转或销毁的动作帧必须单独标记。
