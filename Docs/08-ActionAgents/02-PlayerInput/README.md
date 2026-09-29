# 08.02 · 玩家输入

[返回上级模块](../README.md)

## 职责

把鼠标和按键转换为标准行动命令。

## 边界

输入层不直接修改战斗状态。

## 选中角色死亡时的输入保护（2026-09-29）

`TURN-DEATH-INPUT-01`：死亡时先释放蓄力、轨迹、未提交武器与桌面/触屏/手柄手势，再拒绝本回合玩法操作；保留队伍的死者引用供回合管理器正常结算。不能把死亡变成普通取消后换同队角色，旧松手也不能投掷死者。已发射或已提交的效果继续结算，未发射武器不消耗库存。speech/退出弹窗仍能响应，包括 Android 的对白触摸推进。

来源：用户确认的 Unity 输入保护方案；AS2 静态依据只确认死亡无需等待 Twang 提交即可结束回合，不宣称原版 `Mine` 会主动清除拉线。存活炸飞仍保持 `MIN-AIM-01`。规格、生产入口和实际测试状态见 [TURN-DEATH-01..03](../../03-TurnAndActions/04-ActionSettlement/README.md)。隔离 Unity Play Mode 专项实际 45/45，另复跑覆盖层/取消 32/32；Android/手柄真机、胜负对白触摸仍待验收。

## 蓄力中被动移动的轨迹

遵循 [AIM-MOVE-01/02](../../09-PresentationAndFeedback/08-CursorsAndTrajectory/README.md)：保持原有手势，以角色或武器的实时起点重画；松手从当前权威位置计算方向、力度及最小拉距。鼠标/触屏端保持指针端点，手柄端保持既有方向/累计力度并重建端点。实际 Mine 与死亡组合 49/49、取消 32/32、镜头 11/11；设备真机仍待验收。

## Android 授权适配规格

手柄扩展另见 [玩家操作手册](CONTROLLER_MANUAL.md)、[基础规格](CONTROLLER_SUPPORT.md)、[特殊武器规格](CONTROLLER_SPECIAL_WEAPONS.md)、[双扳机力度规格](CONTROLLER_TRIGGER_POWER.md)和[大炮手柄规格](CONTROLLER_CANNON.md)，分支为 `feature/controller_support`。

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
