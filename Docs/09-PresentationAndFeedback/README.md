# 09 · 表现与反馈

## 职责

- HUD、角色标识、血条、菜单、光标和轨迹。
- 镜头跟随、震动和目标切换。
- 角色动画、武器效果、音效和音乐。

## 当前入口

- `Assets/Mutiny/Scripts/Presentation/MutinyGameHUD.cs`
- `Assets/Mutiny/Scripts/Presentation/MutinyCharacterOverlay.cs`
- `Assets/Mutiny/Scripts/Presentation/MutinyCameraController.cs`
- `Assets/Mutiny/Scripts/Presentation/MutinyAudioManager.cs`

## 动画与视觉效果基线

- [动画与视觉效果总览](ANIMATION_AND_VISUAL_EFFECTS.md)：原版 Flash 时间轴/AS2 与当前 Unity 实现的跨模块对照、缺口和验收顺序。
- [角色动画](05-CharacterAnimation/README.md)：待机、受击、旋转、落水和死亡表现。
- [武器画面效果](06-WeaponEffects/README.md)：投射物时间轴、爆炸、烟迹、火焰和特殊武器。

基线日期为 2026-09-23；本轮只完成静态取证与实现盘点，没有将未运行的 Unity 用例登记为通过。

## 边界

表现层读取权威状态和事件，不自行决定伤害、库存、行动资格或回合结束。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-BattleHUD](01-BattleHUD/README.md) | 战斗 HUD |
| [02-CharacterOverlay](02-CharacterOverlay/README.md) | 角色覆盖层 |
| [03-FrontendUI](03-FrontendUI/README.md) | 前端 UI |
| [04-Camera](04-Camera/README.md) | 镜头 |
| [05-CharacterAnimation](05-CharacterAnimation/README.md) | 角色动画 |
| [06-WeaponEffects](06-WeaponEffects/README.md) | 武器画面效果 |
| [07-Audio](07-Audio/README.md) | 音频 |
| [08-CursorsAndTrajectory](08-CursorsAndTrajectory/README.md) | 光标与轨迹 |

## 后续文档

UI 布局、时间轴、锚点、镜头优先级及音频事件表。
