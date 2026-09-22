# 08.02 · 玩家输入

[返回上级模块](../README.md)

## 职责

把鼠标和按键转换为标准行动命令。

## 边界

输入层不直接修改战斗状态。

## Android 授权适配规格

| ID | 可观察行为 | 来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AND-INP-01 | Android 第一根有效触点映射为统一主指针：`Began`=按下沿，`Moved/Stationary`=持续按住，`Ended`=松开；点击选人、按住拖动显示轨迹、松开后沿用桌面端同一跳跃/武器发射入口 | 用户授权的 Android 操作方案；桌面生产入口 `MutinyPlayerInput.Update` | `TryReadPointer`、`TryReadTouchPointer`、`PointerFrameState` | 对各触摸阶段断言统一指针状态；通过现有生产蓄力/发射入口验证状态转换 | 已实现；C# 编译通过，待 Unity/Android 运行验证 |
| AND-INP-02 | 一次手势只由最先 `Began` 的触点拥有，其他触点不得抢占位置或触发松开发射；全部释放后下一次 `Began` 才能取得所有权 | 用户授权的 Android 操作方案 | `m_ActiveTouchId`、`TryReadTouchPointer` | 触点锁定逻辑回归；Android 真机双指干扰验证 | 已实现；触点资格静态回归已加入，待真机验证 |
| AND-INP-03 | `Canceled`（系统手势、切后台或输入设备取消）只取消当前蓄力/拖动并清理轨迹，不得提交跳跃、武器或火炮发射 | 用户授权的 Android 操作方案 | `HandlePointerCancellation`、`MutinyCannon.CancelPointer` | 阶段映射断言 `Canceled` 不产生 release；火炮取消保持未提交 | 已实现；C# 编译通过，待 Unity/Android 运行验证 |

以上为移动端输入适配，不改变 Flash 原版的选取半径、最小拖动距离、方向、力度、轨迹或库存消耗规则。桌面端继续读取 `Mouse.current`。
