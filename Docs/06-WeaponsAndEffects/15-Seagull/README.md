# 06.15 · Seagull

[返回上级模块](../README.md)

## 职责

记录海鸥路径、飞行、投弹和重复发射。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- 首次点击选择飞行高度，从 x=-300 以 vx=10 向右飞。
- 飞行中每次点击都可投一枚弹，原版没有固定弹数；弹从 `(bird.x-10,bird.y)` 生成。
- 每个原版 tick 先移动海鸥，再消费投弹输入；新弹会在同一 tick 内立即推进一次，后续子弹也由海鸥逐 tick 统一推进。
- 投弹时先显示炸弹，随即将海鸥 `hide(); show()` 重新挂到同一 Character 层最高深度；两者重叠时海鸥遮住炸弹。
- 飞行时间轴只显示 1..8；第 9 帧执行跳回 `flying`，第 9、10 帧的透明导出图不应显示。投弹标签从第 11 帧开始，显示 11..14 后自然回到飞行第 1 帧。
- 弹碰 Solid 爆炸 `50/50`，落水只销毁；鸟越界且活动弹清空后才结束。
- 海鸥没有飞行时间上限，不参与 Unity 的 150 tick 卡死武器强制回收。

来源：`Seagull.as`。规则：`SEA-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#611-seagull)。

## 投弹图层验收

| ID | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- |
| SEA-VIS-01 | `Seagull.as::advance` 中 `shot.show(); this.hide(); this.show()`；`Clip.as::show` 使用同一父层的 `getNextHighestDepth()` | `MutinySeagull.SpawnShot` → `MutinySeagullFire.Initialize` | 通过生产投弹请求推进一个海鸥 tick，断言新弹和海鸥同 Sorting Layer 且新弹 order 小于海鸥 | 原版静态确认；已修复；2026-09-26 隔离 Play Mode 专项 31/31 含该断言通过，主工程画面待验收 |
| SEA-ANI-01 | DefineSprite 982：帧 9 的 `gotoAndPlay("flying")`、帧 11 的 `shot` label、帧 9/10 的透明导出图 | `MutinySeagull.SpawnShot`、`AdvanceAnimation` | 生产投弹后的实际 SpriteRenderer 纹理依次为 11、12、13、14、1；连续飞行始终只显示 1..8 | 原版静态确认；已修复；2026-09-26 隔离 Play Mode 专项 31/31 含该断言通过，主工程画面待验收 |

## 高刷新率投弹显示（2026-09-26）

| ID | 可观察行为及状态转换 | 原版来源 / 扩展依据 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| SEA-PRES-01 | 子弹仍由海鸥每 0.04s 推进一次，新生成子弹同 tick 立即推进；禁止自身 Update 再推进。显示单独启用起止位置插值，插值相位读取父海鸥，与海鸥和镜头共用一个已完成 tick 的延迟 | `Seagull.as::advance:55-118`；用户授权的高刷新率显示扩展 | `MutinySeagullFire.Initialize/AdvanceOriginalTick`、`MutinyPhysicsBody.PresentationClockSource` | 实际投弹请求与物理帧入口驱动 25/60/120 FPS，核对每 tick 一次、出生与后续中间帧实际 Transform、横向与海鸥对齐、重力位移、State 不被显示改写 | 原版时序静态确认；扩展已实现；Unity 6000.6.0f1 隔离 Play Mode 专项 31/31 通过；主工程画面待验收 |
| SEA-PRES-02 | 首 tick 已推进的新子弹不允许在下一帧 Start 中把插值 Transform 回写成权威 State；碰撞和落水继续在物理 tick 结算，不依赖显示位置 | `Seagull.as::advance/contact`；显示扩展的权威状态约束 | `MutinyPhysicsBody.Start`、`MutinySeagullFire` 碰撞/水域入口 | 出生帧先执行显示入口再驱动真实 Start，权威位置与 tick 数不变；各 FPS 比较碰撞 tick、落水结束和爆炸数 | 原版碰撞时序静态确认；扩展已实现；专项 31/31 通过；Android 真机与主工程画面待验收 |

显示缺陷根因：此前子弹为父级推进而设 `IsActive=false`，该字段同时禁止了显示采样与 LateUpdate 插值，导致平滑海鸥/镜头与 25 Hz 阶梯子弹混用。美术符号 971 原版为单帧，本轮不生成动画资产，不调整物理频率、参数或原版飞行动画节拍。

## AI 候选

| ID | 可观察行为 | 原版来源 | Unity 入口 | 当前结果 |
| --- | --- | --- | --- | --- |
| AI-WPN-06 | AI 在最高敌人上方选择飞行高度，随机并排序 10 个投弹 X；敌方单发为 `1-distance/40`，己方单发为 `-(1.5-distance/40)`，仅保留正分投弹点，至少两点才形成候选 | `Seagull.as::aiSimulation` | `MutinyAIController.EvaluateSeagull`、`ScoreSeagullShot` | 已实现；炸弹预测读取共享箱体；回归已写，待 Unity 运行 |
