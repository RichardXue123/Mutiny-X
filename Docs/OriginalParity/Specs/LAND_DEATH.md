# 角色陆地死亡与骨头动画规格

基线：`Mutiny Source/mutiny-flash-game/mutiny.swf`。范围为普通伤害导致的陆地死亡表现；落水继续遵循 `WATER_DROWNING.md`。

## 原版证据

| ID | 类型 | 来源 | 结论 |
| --- | --- | --- | --- |
| DEATH-E01 | S | `Character.as:863-871 subtractHealth` | 伤害先 `Math.round`，生命降至 0 时立即钳制为 0 并令 `alive=false`。 |
| DEATH-E02 | S | `Character.as:152-163 advance` | 实际生命与显示生命不同时，只有 `velocityX == 0 && abs(velocityY) < 0.2` 才每 tick 将显示生命向实际生命移动 1。显示生命 `<1` 时隐藏角色、创建 `deadCharacter`、令其局部 y 等于角色 `bottomExtent`，并播放一次 `die`。 |
| DEATH-E03 | S | `Character.as:180-196` | 落水直接令 `health=0`、`shownHealth=1`、`alive=false`；不会经过创建 `deadCharacter` 和播放 `die` 的陆地分支。 |
| DEATH-E04 | A | `symbols.csv:1021-1025`、`placements.csv:1133-1186` | `deadCharacter` 是 symbol 1065，共 24 帧，由两个 `bone` 和一个 `skull` 子层组成。 |
| DEATH-E05 | A | `sprite-origins.csv:94-117` | 导出的 24 张合成帧均为 26×22 px，注册点为原图 `(12,18)`，对应 Unity pivot `(12/26, 4/22)`。 |
| DEATH-E06 | A/S | `DefineSprite_1065_deadCharacter/frame_24/DoAction.as` | 第 24 帧执行 `stop()`；动画以 SWF 25 Hz 播放一次并永久保持末帧。 |
| DEATH-E07 | A | `Assets/Mutiny/Resources/Audio/SFX/die.wav` | 原版 `die` 音频已经进入 Unity Resources。 |

## 行为规则

| ID | 前置与事件 | 可观察结果 | Unity 入口 |
| --- | --- | --- | --- |
| DEATH-01 | 普通伤害令 Health 到 0 | 立即失去行动/选择资格，但保留当前角色画面和物理运动，不立刻生成骨头或播放 `die` | `MutinyCharacter.TakeDamage` → `MarkDead` |
| DEATH-02 | 受伤角色完全静止且 ShownHealth != Health | 每个 25 Hz 物理 tick 将 ShownHealth 向 Health 移动 1；期间阻止回合静止结算 | `OnSimulationStep` → `AdvanceOriginalHealthTick`、`MutinyTurnManager.CheckAllBodiesAtRest` |
| DEATH-03 | 陆地死亡角色的 ShownHealth 首次小于 1 | 隐藏原角色，在像素 `(x, y + bottomExtent)` 生成世界角度为 0 的骨头动画，并播放一次 `die` | `PresentLandDeath` → `MutinyDeadCharacterEffect.Spawn` |
| DEATH-04 | 骨头动画开始 | 按 25 Hz 顺序播放 24 帧，随后停留在第 24 帧；尸骨不参与物理碰撞和回合阻塞 | `MutinyDeadCharacterEffect` |
| DEATH-05 | 角色落水 | 只执行水花和 `splash`；不生成骨头、不播放 `die` | `MutinyCharacter.Drown` |

## 回归用例

| ID | 操作与断言 | 状态 |
| --- | --- | --- |
| DEATH-TC01 | 对运动中的角色造成致命伤；立即 `IsAlive=false`，但角色渲染器仍可见且无 `DeadCharacter_*`。 | 待 Unity 自动验证 |
| DEATH-TC02 | 角色静止后驱动真实物理 tick；ShownHealth 每 tick 下降 1，到 0 时仅生成一个骨头对象。 | 待 Unity 自动验证 |
| DEATH-TC03 | 检查骨头 24 张资源、26×22 尺寸、pivot、8 px 脚底偏移、25 Hz 一次播放及末帧保持。 | 待 Unity 自动验证和 Play Mode 视觉验证 |
| DEATH-TC04 | 落水死亡；只出现水花并播放 splash，层级中无骨头对象。 | 待 Play Mode 回归 |
| DEATH-TC05 | 对照原版录制陆地致命爆炸至骨头第 24 帧的 tick、位置、层级和音效。 | 待原版运行对照 |
