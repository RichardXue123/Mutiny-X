# 09.05 · 角色动画

[返回上级模块](../README.md)

## 职责

表现待机、受击、抛飞/滚动旋转、落水和死亡。原版并没有独立的“跳跃帧动画”；主要动态来自角色 MovieClip 待机/受击时间轴与整体旋转。

## 边界

动画读取角色与物理状态。

## 原版实现摘要

- 27 种角色符号均为 35 帧复合时间轴。`static` 从第 1 帧开始，可见帧为 1..12；第 13 帧执行 `gotoAndPlay("static")`，第 14 帧是标签间的透明分隔帧。
- `hit` 从第 15 帧开始，可见恢复帧为 15..34；第 35 帧跳回 `static`。`Character.advance` 在 `hit=true` 且角色仍运动时每 tick 重新 `gotoAndPlay("hit")`，因此保持第一个受击姿势；停稳后才允许恢复段播放。
- 角色非拖拽时每 tick 执行 `rotation += velocityX * 3`；落水后在特定速度分支再叠加 `(velocityX + velocityY) * 4`。
- 地面死亡并非在 `health=0` 立即换尸体。角色停稳时 `shownHealth` 每 tick 向 `health` 滑动 1，降到 1 以下才隐藏角色并在 `bottomExtent` 处创建 24 帧 `deadCharacter`；第 24 帧 `stop()` 并保持。
- 沉入水下时设置死亡并持续水下物理/旋转，不走地面 `deadCharacter` 分支。

原版主要来源：`Character.as::advance`；任一角色符号的 frame 13/35 `DoAction.as`；symbol 1065 frame 24 `DoAction.as`。

## Unity 实现摘要

- `Assets/Mutiny/Scripts/Simulation/MutinyCharacterAnimator.cs`：以 25 Hz 直接推进 1..12 待机循环和 15..34 受击段，显式跳过动作帧 13/35 与透明帧 14。
- `Assets/Mutiny/Resources/Art/Characters/Animations/<characterType>/`：27 个角色目录，每个 35 张导出 PNG。
- `MutinyCharacterAnimator.CharacterPivots`：为 27 种角色按原图注册点建立 pivot，并以 32 PPU 生成像素 Sprite。
- `MutinyCharacter.TakeDamage`：请求受击时间轴，同时有 0.2 秒红色 tint；该 tint 是当前 Unity 表现，仍需原版像素对照确认。
- `MutinyCharacter` + `MutinyRotationState`：在物理 tick 记录逻辑角度，按原版速度公式旋转并在接触后归零。
- `MutinyDeadCharacterEffect`：加载 24 帧、播放一次并保持第 24 帧；只由地面死亡表现生成。

## 行为规格与状态

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CHAR-ANI-01 | 待机只显示 1..12，12 后回 1，不呈现 13/14 | 角色 symbol frame 13；导出帧 | `AdvanceOriginalTick` | 由关卡生产构建角色，推进 13 tick 断言 1..12→1 | 已实现；现有回归已写，本轮未运行 |
| CHAR-ANI-02 | 受击且未停稳时保持 frame 15，停稳后播放 16..34 并回待机 | `Character.as::advance`；frame 35 | `PlayHit`、`AdvanceOriginalTick` | 生产爆炸命中后同时记录速度与当前帧，直至归待机 | 已实现；待 Unity 运行验证 |
| CHAR-ANI-03 | 抛飞/滚动使用整体角度，每 tick 由 `vx*3` 驱动，地面接触使角度收敛 | `Character.as::advance`、Solid 接触路径 | `MutinyRotationRules`、`AdvanceOriginalRotationTick` | 生产投掷与爆炸击飞分别记录角度/tick | 已实现；待运行验证 |
| CHAR-ANI-04 | 地面死亡等待停稳与显示血量滑落，再播 24 帧尸体并停在末帧 | `Character.as::advance`；symbol 1065 frame 24 | `AdvanceOriginalHealthTick`、`PresentLandDeath`、`MutinyDeadCharacterEffect` | 生产伤害将血降为 0，运动时不出尸体；停稳后验证 1..24 并持有 | 已实现；资源/局部回归已写，本轮未运行 |
| CHAR-ANI-05 | 落水死亡不生成地面尸体；水花由注册点跨线触发，等于水线就生成，溺水发生于低于水线时 | `Character.as::advance`、`Solid.splashCheck` | `AdvanceOriginalSplashCheck`、`Drown`、`AdvanceOriginalWaterRotationTick` | 生产角色恰好到水线生成一次，下一 tick 下沉溺水不重复；核对尸体和角度 | 水花触发已接入；透明度逐 tick 与原版待完整对照 |

## 已知差异与待确认

- `TakeDamage` 的 0.2 秒红色 tint 是 Unity 当前额外表现；静态 AS2 只能确认 `hit` 时间轴，尚未找到原版同等 tint 证据。
- 角色翻转当前由 `velocityX` 符号设置 `SpriteRenderer.flipX`；需用原版运行录像确认不同角色符号的默认朝向与翻转时机。
- 当前没有独立走路、跳跃或游泳帧序；除非新证据证明原版存在，不应自行发明这些状态。

## 完成状态

- 静态确认：35 帧角色时间轴的两个循环点、24 帧死亡时间轴停止点、`Character.advance` 的受击/旋转/死亡分支。
- 已实现：27×35 帧资源、待机/受击帧状态机、角度状态、地面死亡效果。
- 实际测试通过：本轮未运行 Unity，不新增通过记录。
- 待运行验证：所有 `CHAR-ANI-*` 的生产入口逐帧记录。
