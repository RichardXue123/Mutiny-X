# 06.12 · Parachute Bomb

[返回上级模块](../README.md)

## 职责

记录降落伞炸弹的开伞、控制和命中行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 11、twangMaxForce 30；每 tick `vx*=.95`，下降速度通过专用公式收敛。
- 非模拟状态 `vy>-10` 就开伞，不是等到抛物线最高点。
- 装备后外层时间轴停在 `closed`，但内部 DefineSprite 930 的四帧引信动画继续循环；发射后、开伞前也保持该循环。
- 开伞播放 11..29 帧；第 30 帧只有 `gotoAndPlay("open")` 动作，不是可显示帧。开放状态从 26..29 循环，不能显示透明的第 30 帧。
- 人类炸弹飞行期间鼠标替换为原版四帧 fan 图标；鼠标位于炸弹右侧时旋转 -90°，左侧时旋转 90°。
- 按住左键时 fan 子时间轴播放，每 tick 反向加 `vx ±=.2`，每 12 tick 播放 `fan`；松开时风扇动画停在当前帧。
- Solid 接触爆炸 `160/50`；顶部限制 y=-300。

来源：`ParachuteBomb.as`。规则：`PCB-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#68-parachute-bomb)。

## 行为规格与实现映射

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| PCB-ANI-01 | 选中后闭合炸弹保持可见，内部四帧引信以 25 Hz 循环；投出至开伞前继续循环 | symbol 939 frame 1 `stop()`；child symbol 930 frames 1..4 | `AdvanceClosedFusePresentation()` | `VerifyParachuteBomb()` 在未发射状态断言 1→2→3→4→1 | 已实现；待 Unity 运行验证 |
| PCB-ANI-02 | `vy>-10` 从 opening 帧 11 开伞；开放循环显示 26..29，第 30 帧动作立即跳回 26，不渲染透明导出帧 | `ParachuteBomb.as:75-80`；symbol 939 frame 30 `gotoAndPlay("open")` | `AdvanceChuteAnimation()` | 推进开伞序列并断言 29 后直接回到 26，当前帧不出现 30 | 已实现；待 Unity 运行验证 |
| PCB-CUR-01 | 人类炸弹飞行时系统鼠标替换为 fan；右侧 -90°、左侧 90°；按住播放、松开停止 fan 子动画 | `TileSystem.as:336-345`；`ParachuteBomb.as:91-111`；cursor symbol 1813/1806 | `MutinyPlayerInput.UpdateSpecialWeaponCursor()`、`MutinySpecialWeaponCursor` | ActionExecuting 输入门保持开放；校验 fan 模式、旋转和按住后的帧推进 | 已实现；待 Unity 运行验证 |
| PCB-FAN-01 | 按住左键每 tick 根据鼠标相对位置反向施加 `vx ±=.2` | `ParachuteBomb.as:91-104` | `ApplyFanInput()` | 左侧输入使 vx +0.2；相等位置走右侧分支使 vx -0.2 | 已实现；待 Unity 运行验证 |
| PCB-AUD-01 | 持续扇风时每 12 个发射 tick 播放一次 `fan` | `ParachuteBomb.as:104-108` | `ApplyFanInput()` → `PlaySFX("fan")` | 监听 `SfxPlayed`，第 12 tick 收到 `fan` 且资源可解析 | 已实现；待 Unity 运行验证 |
| PCB-HIT-01 | 与 Solid 接触时隐藏并产生 160/50 爆炸和 `pop`；顶部限制 y=-300 | `ParachuteBomb.as:27-48,113-121` | `OnContact()`、ceiling clamp | 实心地形接触后断言炸弹结束并生成 160/50 爆炸 | 已实现；待 Unity 运行验证 |
| PCB-LIFE-01 | 飞行没有固定时长上限；只在越过地图底部、达到原版静止条件或接触 Solid 时结束，不能被非原版 150 tick 回合兜底销毁 | `Weapon.as:60-78`；`ParachuteBomb.as:33-47,124-134` | `MutinyParachuteBomb.AdvanceOriginalTick()`、`MutinyTurnManager.AdvanceSimulationTick()` | 发射后仅推进回合结算 152 tick，断言炸弹仍未结束 | 已实现；待 Unity 运行验证 |
| AND-PCB-FAN-01 | Android 降落伞炸弹飞行时，按住屏幕沿用原版每 tick 扇风；触点位于炸弹左/右侧时保持原版反向水平冲量。为避免短触摸落在两个 25 Hz 物理 tick 之间，`Began` 至少锁存一个 fan tick | 用户报告：Android 点击屏幕没有扇风；原版持续按键规则见 `ParachuteBomb.as:91-110` | `CaptureMobileFanInput()`、`TryGetFanInput()`、`ResolveMobileFanActive()` | 持续触摸每 tick 生效；仅出现按下沿但物理 tick 延后时仍消费一次锁存脉冲；随后无触摸不继续施力 | 已实现；锁存消费回归已加入、C# 编译通过，待 Android 真机验证 |

## 本次缺陷原因

- Unity 原先把复合导出的第 1 帧当作静态图，遗漏了其内部独立运行的四帧引信时间轴。
- Unity 原先把第 30 帧透明导出图实际显示一个 tick；原版在该帧执行跳转，因此降落过程出现周期性闪烁/消失。
- 发射后回合进入 `ActionExecuting`，旧输入门没有为 Parachute Bomb 保持开放；专用鼠标实现也只有 Seagull 与 Tidal Wave 两种模式，所以物理扇风虽能轮询鼠标，却没有原版 fan 光标反馈。
- 回合管理器先前另有 Unity 专用的 150 tick（约 6 秒）卡死恢复逻辑，会让正常缓慢下降的 Parachute Bomb 在尚未碰撞时消失。本轮已从公共回合结算中完全移除，不再依赖单个武器豁免。
- Android 缺陷来自 `TryGetFanInput()` 只轮询 `Mouse.current`；移动端已有统一触摸指针，但物理武器没有读取 `Touchscreen.current`，所以扇形光标可能变化而实际 `vx` 完全不受力。

## 验证状态

- AS2、时间轴脚本、frame label、子符号与原版 fan 光标：静态确认。
- Unity C# 编译：已通过；仅有既存 `MutinyLevelTest.levelXml` 未赋值警告。
- `VerifyParachuteBomb()` 已覆盖引信循环、透明动作帧跳过、ActionExecuting 输入、fan 光标、音效，以及越过 150 tick 回合兜底后仍存活；待 Unity 运行验证。
