# 07.02 · 宝箱降落与落地

[返回上级模块](../README.md)

## 职责

管理降落伞动画、下降、地面锚点和落地阶段。

## 边界

镜头和声音只响应该阶段事件。

## 降落显示规格（2026-09-27）

| 规则 ID | 可观察行为与状态转换 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| `AIR-PRES-01` | 宝箱从 `y=-300` 开始，每次逻辑推进下降 3 Flash 像素；达到 `floorY-15` 的同一 tick 钳位、结束 falling、切换 touchdown。落地/拾取资格与帧序列不因显示刷新率改变。 | `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/TreasureChest.as` 的构造函数与 `advance()`；`mc.gotoAndStop("falling")`/`gotoAndPlay("touchdown")` | `MutinyTreasureChestManager.Update` → `MutinyTreasureChest.AdvanceOriginalTick` | `RunAirDropPresentation` 驱动生产 manager 的 25 Hz 入口并检查边界 | 原版静态确认；Unity 数值回归通过 |
| `AIR-PRES-02` | 用户授权扩展：25 Hz 状态不变，60/120 Hz 渲染帧仅在已完成的相邻下降 tick 间插值；宝箱与镜头使用同一显示位置，落地立即清除旧下降插值。 | 原版只有上述离散 tick 行为；高刷新插值不声称为原版规则 | `MutinyTreasureChest.SamplePresentationPosition`、`LateUpdate`、`MutinyCameraController.AdvanceCamera` | `RunAirDropPresentation` 检查中间帧、无逻辑重复推进、共享显示位置与落地清除 | Unity 数值回归通过；实际画面待验收 |

静态确认：原版 `TreasureChest.advance()` 只在逻辑推进中修改 `y`、落地状态和时间轴；没有高刷新插值。已实现：Unity 的下降权威位置、落地/拾取和 10..19 循环帧仍以 25 Hz 前进；只让显示位置在相邻 tick 间变化，镜头读取同一显示采样。实际测试：2026-09-27 Unity 6000.6.0f1 隔离工程 Play Mode 专项 5/5 通过，包含 `floorY=32` 时第 106 tick 到达 `y=17`；既有镜头跟随专项复跑 11/11 通过。待运行验证：主工程 PIE 真实 60/120 Hz、不同关卡空投可见区和 Android 真机观感。已知差异：高刷新位置插值属于用户授权扩展；降落伞贴图时间轴保持原版 25 Hz，不补造美术中间帧。
