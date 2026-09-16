# 武器待抛射、取消与装备显示规格

状态：静态证据确认；Unity 生产入口已实现且 C# 编译通过；专项回归已加入，Unity 执行、Play Mode 与原版运行对照待执行。

## 范围与状态

| 状态 | 含义 |
| --- | --- |
| `ActionMenu` | 武器选择面板可见，尚未装备武器。 |
| `WeaponReady` | 已选择并创建真实武器实例；武器位于角色装备点，尚未拉力。 |
| `Aiming` | 左键已在武器附近按下，正在形成抛物线，尚未提交。 |
| `ActionExecuting` | 松开左键并成功发射；库存和行动资格按武器规则结算。 |

## 原版规则

### WRDY-S01：选择即装备真实实例

- 前置：当前存活角色可使用所选武器，库存大于零。
- 输入：在武器选择面板点击一种武器。
- 状态变化：`Character.equip(index)` 创建该武器的真实实例并设为 `equippedWeapon`；面板通过 `weaponSelected=true` 隐藏。
- 可观察结果：武器实例位于角色源坐标 `(x, y-10px)`；巨石额外上移 `20px`，即 `(x, y-30px)`。速度为零，尚不消耗库存。
- 原版证据：`Character.as::equip` 创建 15 种武器、设置 owner/type/零速度并定位；`WeaponSelectButton.as::onRelease` 调用 `equip` 后设置 `weaponSelected=true`。
- Unity 入口：`MutinyPlayerInput.SelectWeapon`、`MutinyWeapon.PrepareForEquip`。

### WRDY-S02：角色下方取消叉号

- 允许条件：当前队伍的已选角色、已选择武器、武器未锁定、未处于 dragging/twanging、装备武器未发射且角色存活。
- 可观察结果：叉号中心位于角色源坐标 `(x, y+33px)`；P1 使用红色 symbol 1817，P2 使用蓝色 symbol 1820。原版按钮 `_up` 为第 1 帧，`_over` 为第 10 帧。
- 原版证据：`Character.as::updateOverlay` 的 `overlay.cancelWeapon._visible` 条件及 `y=660` twips；`placements.csv` 的 characterOverlay 子项；symbol 1817/1820 时间轴。
- Unity 入口：`MutinyCharacterOverlay`、`MutinyPlayerInput.ShouldShowCancelWeapon`。

### WRDY-S03：点击叉号退回武器选择

- 输入：点击可见取消叉号的 20×20px 命中区域。
- 状态变化：`weaponSelected=false`，调用 `unequip()` 销毁尚未发射的装备实例。
- 可观察结果：回到武器选择面板；不消耗库存或射击资格。
- 原版证据：`CancelWeaponButton.as::onPress`；`Character.as::unequip`；`WeaponSelectPanel.as` 的显示条件。
- Unity 入口：`MutinyPlayerInput.TryCancelWeaponFromOverlay`、`CancelWeaponSelection`。

### WRDY-S04：从装备实例开始拉力和发射

- 输入：鼠标在装备武器 30px 内按下左键；移动形成预测线；松开提交。
- 状态变化：按下进入 `Aiming`；松开调用同一个装备实例的 `twang/release`，之后才消耗库存。
- 可观察结果：轨迹起点使用武器位置，不能在松开时创建第二个武器实例；拉力时隐藏取消叉号。
- 原版证据：`TileSystem.as::mouseDown/mouseUp` 使用 `selectedCharacter.equippedWeapon`，30px 判定为 `distanceSq < 900`；`Character.as::updateOverlay` 在 twanging 时隐藏取消叉号。
- Unity 入口：`MutinyPlayerInput.Update`、`Launch`。

## 用户授权扩展

### EXT-WRDY-01：右键取消当前拉力

- 原版 Flash 没有右键输入路径，本条不声明原版一致性。
- 前置：状态为 `Aiming`，武器尚未发射。
- 输入：按下右键。
- 状态变化：清除预测线并回到 `WeaponReady`；保留同一个装备武器实例、库存和射击资格。
- 可观察结果：武器仍显示在角色装备点，可再次按住左键拉力；不会打开武器选择面板。
- `WeaponReady` 中尚未开始拉力时，右键不改变状态；返回武器面板使用角色下方叉号。
- Unity 入口：`MutinyPlayerInput.CancelCurrentAim`。

## 验收用例

| ID | 操作 | 断言 | 当前结果 |
| --- | --- | --- | --- |
| WRDY-T01 | 逐一选择所有已登记武器 | 每次都立即存在且仅保留一个未发射实例；位置符合 WRDY-S01 | 生产回归已加入，待 Unity 执行与 Play Mode |
| WRDY-T02 | 选择武器后观察/点击叉号 | 待抛射时显示、拉力时隐藏；点击回到武器菜单且库存不变 | 生产回归已加入，待 Unity 执行与 Play Mode |
| WRDY-T03 | 左键拉力后右键 | 同一实例保留，状态回到 `WeaponReady`，预测线消失 | 生产回归已加入，待 Unity 执行与 Play Mode |
| WRDY-T04 | 再次拉力并松开 | 发射的是装备实例，不产生第二实例；提交时才扣库存 | 生产回归已加入，待 Unity 执行与 Play Mode |

## 已知待核对项

- Unity 的叉号悬停帧需要用原版第 10 帧资源做像素对照；缺失时不能以颜色插值宣称完全一致。
- 放置型、点选型和多阶段武器沿用各自专用提交规则，但进入各自专用阶段前也必须先显示真实装备实例。
