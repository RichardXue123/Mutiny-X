# 角色与投掷物旋转一致性规格

基线：`Mutiny Source/mutiny-flash-game/mutiny.swf`，反编译 AS2 位于
`Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/`。
本规格只覆盖对象整体旋转及角色落地回正；角色逐帧姿势、表情与特效时间轴仍归 P5-02～P5-06。

## 原版证据

| ID | 类型 | 原版来源 | 可观察规则 |
| --- | --- | --- | --- |
| ROT-E01 | S | `Character.as:166-171` | 角色完成一次 `advanceMotion` 后，在未被拖拽时执行 `rotation += velocityX * 3`。该入口由原版 25 Hz 主循环驱动。 |
| ROT-E02 | S | `Character.as:180-196` | 角色入水后先将 X/Y 速度乘 0.8；若 Y 速度仍大于 1.5，则减 4、最低钳制到 1.5，然后额外执行 `rotation += (velocityX + velocityY) * 4`。 |
| ROT-E03 | S | `Character.as:697-713` | 角色接触地面时把角度归一到 `[-180,180]`，乘 0.5；结果严格位于 `(-1,1)` 时归零。墙面接触不执行回正。 |
| ROT-E04 | S | `Banana.as:43-49`、`Dynamite.as:32-38` | Banana 和 Dynamite 在各自 `advanceMotion` 调用父类运动前，按当时 `velocityX * 2` 旋转。 |
| ROT-E05 | S | `RumBottle.as:67-74`、`VoodooDoll.as:45-52` | 已发射的 RumBottle 和 VoodooDoll 每逻辑 tick 按 `velocityX * 2` 旋转。 |
| ROT-E06 | S | `Boulder.as:39-45` | Boulder 的 `mc.rotating` 子层在父类运动前按 `velocityX * 2.5` 旋转。 |
| ROT-E07 | S | `CherryBomb.as`、`Weapon.as`、`Solid.as` | CherryBomb 和通用 Weapon/Solid 没有整体旋转公式；CherryBomb 的时间轴动画不等于 Transform 旋转。 |
| ROT-E08 | S | `Solid.as:advanceMotion` 地面接触与墙面接触分支 | 每次落地先将 `abs(velocityX)` 减去对象自身 `friction`，最低为 0；撞墙将 `velocityX` 乘以 `-0.4`。后续 tick 的旋转增量读取这个已衰减的 X 速度，因此角速度按角色 2、Dynamite 1.7 等各自摩擦逐步下降。 |

Flash 正角度为顺时针，Unity Z 正角度为逆时针，因此 Unity 使用相反符号。所有倍率均是“每个 25 Hz 逻辑 tick 的角度增量”，不能在渲染帧 `Update` 中重复应用。

## 行为规则与 Unity 入口

| ID | 前置/事件 | 结果 | Unity 入口 |
| --- | --- | --- | --- |
| ROT-01 | 角色处于跳跃、爆炸击飞、受击或碰撞后的运动中；物理移动完成 | 每逻辑 tick 增加 `-velocityX * 3` 度 | `MutinyPhysicsBody.OnAfterMotionStep` → `MutinyCharacter.AdvanceOriginalRotationTick` |
| ROT-02 | 角色在同一 tick 接触地面 | 先按 ROT-E03 回正，再应用该 tick 物理移动后的角色旋转 | `OnFloorLanded` → `MutinyCharacter.HandleFloorContact` |
| ROT-03 | 角色在水下且进入 ROT-E02 的 Y 速度分支 | 使用水阻及钳制后的速度额外旋转 `-(velocityX + velocityY) * 4` 度 | `OnWaterMotionAdjusted` → `MutinyCharacter.AdvanceOriginalWaterRotationTick` |
| ROT-04 | 已发射的 Banana/Dynamite/RumBottle/VoodooDoll/Boulder 开始一次物理 tick | 分别按 `-velocityX * 2/2.5` 旋转一次；与渲染帧率无关 | `MutinyPhysicsBody.OnBeforeSimulationStep` → `MutinyWeapon.AdvanceOriginalRotationTick` |
| ROT-05 | CherryBomb 或其他没有原版公式的武器飞行 | 不施加通用 Transform 旋转 | `MutinyRotationRules.WeaponRotationDelta` |
| ROT-06 | 角色因跳跃、爆炸、碰撞或落水而旋转 | 只旋转角色内部画面；characterOverlay 跟随位置并保持屏幕正向 | `MutinyCharacterOverlay.KeepOverlayUpright` |
| ROT-07 | Unity 渲染帧位于两个原版 25 Hz tick 之间 | 逻辑角度只在原版 tick 按 ROT-01～04 更新；显示角度在上一个和当前逻辑角度之间插值，不额外积分、不改变碰撞或落地回正结果 | `MutinyRotationState`、`MutinyPhysicsBody.SimulationInterpolationAlpha`、角色/武器 `LateUpdate` |
| ROT-08 | 角色或可旋转武器连续落地/撞墙 | 下一 tick 的角度增量使用衰减后的 `velocityX`，呈现明确的快→慢过程；角色落地角度仍按 ROT-02 同 tick 回正 | `MutinyPhysics.Step` → 旋转状态生产回调 |

## 回归用例

| 用例 | 操作与断言 | 状态 |
| --- | --- | --- |
| ROT-TC01 | 通过 `MutinyPhysicsBody.AdvanceSimulationTick` 驱动角色，X 速度为 4 时逻辑角单 tick 为 -12°；连续 tick 只随逻辑 tick 数增长。 | 自动断言已添加；Unity 待执行 |
| ROT-TC02 | 角色以 270° 接触地面后得到 -45°；2° 得到 1°，后续接触归零。 | 自动断言已添加；Unity 待执行 |
| ROT-TC03 | 入水速度经 0.8 阻尼及 Y 钳制后，只由角色收到额外水中旋转事件。 | 自动断言已添加；Unity 待执行 |
| ROT-TC04 | 五种有旋转公式的武器倍率分别为 2/2.5；CherryBomb 为 0；渲染帧不再重复积分整体角度。 | 自动断言已添加；Unity 待执行 |
| ROT-TC05 | Unity Level 1 手动观察跳跃、爆炸击飞、墙/地面碰撞、落水以及五种武器飞行，与原版录像逐 tick 对照。 | 待 Play Mode / 原版运行对照 |
| ROT-TC06 | 将生产角色 Transform 旋转 73° 并移动，驱动覆盖层刷新；断言 overlay 世界位置跟随角色且世界 Z 旋转为 0°。 | 自动断言已添加；Unity 待执行 |
| ROT-TC07 | 角色以 `vx=6` 连续接触地面：摩擦后逻辑旋转增量依次为 -12°、-6°、0°；Dynamite 以自身 1.7 摩擦得到严格递减的 -12°、-8.6°…… | 自动断言已添加；Unity 待执行 |
| ROT-TC08 | 同一生产逻辑角从 0° 到 -12°时，渲染采样 alpha=0/0.5/1 分别为 0/-6/-12°，且不改变逻辑角；跨 360° 使用最短可见路径。 | 自动断言已添加；Unity 待执行 |

## 缺陷记录

| ID | 用户观察 / 最早分歧 | 根因 | 修复与状态 |
| --- | --- | --- | --- |
| BUG-ROT-002 | 2026-09-18：角色主动移动、爆炸击飞及 Dynamite 旋转呈阶梯状，快→慢衰减不清晰。逻辑 tick 数值变化存在，但 Unity 渲染帧之间角度完全不变。 | 原实现直接在 25 Hz 回调中 `Transform.Rotate`；60 Hz 等渲染环境会看到“停住—跳一格”，摩擦导致的逐 tick 角速度下降也只能以突变显示。 | 保留原版速度、摩擦、倍率和更新顺序，新增独立逻辑角状态，并在相邻 tick 角度之间只做显示插值。已实现；C#/Unity/原版验证状态见 `VALIDATION.md`。 |

## 已知结构差异

- 原版 Boulder 旋转 `mc.rotating` 子层；Unity 已将同一逻辑角和插值只应用到 `RotatingVisual`，上层 overlay 不旋转。
