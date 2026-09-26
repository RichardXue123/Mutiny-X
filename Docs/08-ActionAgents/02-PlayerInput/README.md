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
| AND-INP-04 | Android 在棋盘空白处按住并拖动时平移当前视角；内容跟随手指移动，松手结束。角色、蓄力起点、武器专用点击与取消按钮优先，不得同时触发镜头拖动 | 用户授权的 Android 镜头适配要求 | `TryBeginMobileCameraDrag`、`AdvanceMobileCameraDrag`、`MutinyCameraController.PanByMobileTouchDelta` | 空白触点可取得镜头手势；屏幕位移按相机可视范围换算并受关卡边界限制；与蓄力互斥 | 已实现；C# 编译通过，待 Android 真机验证 |
| AND-INP-05 | Android 对船锚、海浪、海鸥、箱/桶和巫毒目标等点击型操作区分点击与拖动：按下后在阈值内松手才提交武器；持续移动超过阈值则不提交武器，改为平移镜头 | 用户反馈：选中点击型武器后仍需先移动视角 | `BeginMobileTapCandidate`、`AdvanceMobileTapCandidate`、`IsMobileTapActivatedInteraction` | 阈值内 release 调生产武器入口；越阈值只取得镜头手势且库存/回合状态不改变 | 已实现；阈值回归已加入、C# 编译通过，待真机验证 |
| AND-INP-06 | 一根手指正在蓄力跳跃、投掷或拖动火炮时，第二根手指可独立拖动镜头；除下方 `EXT-AIM-CAN-02` 的取消叉外，第二指不得替换行动触点、提交武器或结束第一指蓄力 | 用户授权的 Android 多指操作要求 | `UpdateSecondaryMobileCameraTouch`、`m_SecondaryCameraTouchId` | 行动触点锁定保持；第二触点在叉外只产生相机位移；任一触点单独释放不结束另一触点角色 | 已实现；角色门回归已加入、C# 编译通过，待真机多指验证 |
| EXT-AIM-CAN-02 | 武器/跳跃蓄力期间，第二指在角色下方叉上新按下优先于镜头手势取消当前行动；主指随后松开不补发射 | 用户授权的触屏取消输入；叉可见遵循原版规则 `CHAR-OVR-AIM-01` | `TryCancelAimFromSecondaryTouch` | 生产蓄力后模拟第二指新按叉，再松开主指，检查未发射且行动资格保留 | 已实现；隔离 Play Mode 31/31 组合专项通过；Android 真机待验证 |
| WPN-CAN-PRESS-01 | 取消叉只响应新按下沿；在外按住后移入叉上不取消，在叉上松开仍正常提交跳跃/武器 | `CancelWeaponButton.as::onPress`、`TileSystem.as::mouseUp`；用户要求 | `TryHandleCancelOverlayPrimaryPointer`、`ResolveAimRelease` | 分别对武器/跳跃驱动新按下、持续按住、松开，验证实际提交和库存/行动消耗 | 已实现；隔离 Play Mode 31/31 组合专项通过；实机待验收 |

以上为移动端输入适配，不改变 Flash 原版的选取半径、最小拖动距离、方向、力度、轨迹或库存消耗规则。桌面端继续读取 `Mouse.current`。
