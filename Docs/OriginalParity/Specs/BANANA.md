# WPN-03 — Banana（香蕉）

基线：原版 `mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。坐标是 Flash 像素；生产模拟 tick 为 25 Hz。

## 证据与入口

| 原版入口 | 证据 | Unity 入口 |
| --- | --- | --- |
| `Banana.as` 构造、`contact`、`advanceMotion`、`fire` | S；pcode 的常量池和控制流复核 | `MutinyBanana` |
| `Weapon.as release` | S | `MutinyBanana.Twang` |
| `Solid.as advanceMotion` | S | `MutinyPhysics.Step`、`MutinyPhysicsBody` 事件 |
| `WeaponSelectButton.as hover_banana` | S | 行动面板既有香蕉文案 |
| `DefineSprite_921_banana`，`banana_bounce` 声音 symbol 365 | A | `Art/Weapons/Banana/1`、`Audio/SFX/banana_bounce` |

## 规则

### WPN-03-EFF-01 — 创建与抛掷

- **原版**：extent 四向均为 7，bounce 为 0.8，friction 为 0.5，`hitsBoxes=true`，不可拖动，可拉力表最大 30。释放时公共 `Weapon.release()` 把最终速度限制至 20 px/tick；飞行时每 tick `rotation += velocityX * 2`。
- **Unity**：`MutinyBanana.Initialize/Twang` 与公共 `MutinyWeapon` 旋转入口。
- **回归**：生产工厂经 `SpawnAndLaunch` 发射最大拖拽，断言 `velocityX=20`。
- **状态**：回归已加入，未在 Unity Editor 执行。

### WPN-03-EFF-02 — 碰撞和静止引爆

- **原版**：任何 `contact(side)` 在已发射、未结束时播放 `banana_bounce`；它不因碰撞直接爆炸。`advanceMotion` 在完成本 tick 物理后，当 `velocityX == 0 && abs(velocityY) < 0.5` 时引爆，创建 `Explosion(x,y,160,80,owner)` 并在同 tick 播放 `pop`。
- **Unity**：`MutinyPhysicsBody.OnFloorLanded/OnCeilingHit/OnWallHit` → `MutinyBanana.OnContact`；`AdvanceOriginalTick` 与 `Explode`。
- **回归**：生产物理落地验证 bounce 0.8、friction 0.5，速度变为 `(2.5,-1.6)` 且仍未引爆。
- **状态**：回归已加入，未在 Unity Editor 执行。

### WPN-03-INT-01 — 玩家二次点击

- **前置**：人类队伍已发射香蕉。`Banana.fire` 会把原发射点击的 `tileSystem.mouseButtonDown` 清为 false。
- **原版**：之后任意下一次鼠标按下会在香蕉的 `advanceMotion` 引爆，不要求香蕉静止或角色仍有 `CanShoot`。
- **Unity**：`MutinyPlayerInput.CanProcessCurrentTurnInput` 在本队已发射香蕉时允许 `ActionExecuting` 阶段继续接收该全局点击；`Update` 再把左键交给 `MutinyBanana.TryRequestPlayerDetonation`，下一生产模拟 tick 由 `AdvanceOriginalTick` 引爆。
- **拒绝**：AI 队、未发射、已完成和非拥有队的香蕉不能被该输入请求引爆。
- **回归**：通过生产工厂发射、消耗 `CanShoot` 并进入 `ActionExecuting` 后，先验证生产输入门控仍放行，再经 `TryRequestPlayerDetonation` 和生产 tick 验证 160/80 爆炸。
- **状态**：回归已加入，未在 Unity Editor 执行。

### WPN-03-EFF-03 — AI 引爆

- **原版**：AI 每 tick 扫描 `Controller.teams` 的所有角色，取最小 `(character.x-x)^2+(character.y-y)^2`。小于 400 时引爆。代码也包含“距离大于 `lastSqDistance` 且小于 2500”分支；本 SWF 的 `Banana.as` 将 `lastSqDistance` 初始化为 Infinity，且该类与 pcode 中没有写回，故这条分支在该基线不会成为真。
- **Unity**：`MutinyBanana.FindOriginalNearestCharacterDistanceSquared/AdvanceOriginalTick` 保留该不可达条件并实现 `<400` 分支；没有改造成“飞离目标自动引爆”。
- **回归**：AI 所有者、任一角色在 16 px 处时，生产 tick 引爆。
- **状态**：回归已加入，未在 Unity Editor 执行。

### WPN-03-AUD-01 — 声音时序

- **原版**：每次接触 `banana_bounce`；引爆 tick 立即 `pop`。没有额外发射音效调用。
- **Unity**：`OnContact` 与 `Explode`。香蕉创建爆炸时禁用 `Explosion` 第 3 帧的通用 pop，避免重复。
- **状态**：资源存在；声音实际时序仍待 Unity/原版对照。

## 待验收与已知差异

- 缺陷 `BUG-WPN-03-001`：此前 `MutinyPlayerInput.Update` 在 `ActionExecuting` 时提前返回，导致第二次按下从未到达香蕉。已按 `Banana.as advanceMotion` 修复为仅在本队存在已发射香蕉时开放全局点击；不会开放普通菜单或瞄准输入。
- 自动回归覆盖工厂发射、`ActionExecuting` 门控、二次点击、落地反弹和 AI 近距离触发；尚未实际执行 Unity Editor 测试。
- 仍需在关卡中录制：墙、顶、水、箱子碰撞；静止阈值边界；玩家二次点击的输入命中与回合结算；原版逐 tick 画面和音频对照。
- 香蕉原始 sprite 只有一个 27×15 帧，当前 Resources 中已存在，无额外时间轴帧待导入。
