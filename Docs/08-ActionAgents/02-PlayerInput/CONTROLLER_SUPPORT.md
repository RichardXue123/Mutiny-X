# 手柄初版规格（2026-09-26）

分支：`feature/controller_support`。来源：用户授权的手柄映射方案；手柄操作属于扩展，不替代 Flash 鼠标行为。本规格在初版实现前建立；特殊武器追加行为另见 [特殊武器规格](CONTROLLER_SPECIAL_WEAPONS.md)，双扳机累计力度另见 [力度规格](CONTROLLER_TRIGGER_POWER.md)，大炮部署/瞄准另见 [大炮手柄规格](CONTROLLER_CANNON.md)。

## 范围与边界

- 十字键移动 UI / 角色焦点，A 确认；左杆非瞄准时移动软件指针，瞄准时指定**向后拉**的方向（发射方向与摇杆相反）；右杆平移相机；RT 增力、LT 减力，深度指定变化速率；A 提交；B 分级返回；Start 打开现有退出弹窗。
- 实现跳跃、普通投掷（Cherry Bomb、Dynamite、Boulder、Mine、Rum Bottle，以及原有备用炮弹）、特殊武器（Anchor、Seagull、Tidal Wave、Wooden Crate、Gunpowder Barrel、Voodoo Doll、Banana、Parachute Bomb、Pieces of Eight）及 Cannon 两阶段操作。大炮待命用左杆部署，瞄准用左杆后拉与扳机调视觉蓄力，A 固定 30 力提交，B 返回部署。
- 普通行动：选择后待命，LT 或 RT 新一次越过死区进入瞄准，从 0% 开始累计力度；左杆从中心向外拉指定拖动端点，未给出方向时 A 不提交；松开双扳机不提交且保留力度；左杆回中保持最后一次拉力方向；A 新按下才提交；B 首次回待命、第二次回行动菜单；菜单 B 仅在现有资格允许时回选人。
- 进入新页面或弹窗清除旧焦点；首次十字键输入定位到首个合格按钮，此后按空间方向导航。左杆移动会退出按钮焦点，A 可执行软件指针下的合格按钮。
- LT/RT 死区为 8%，有效区间线性映射为力度变化速度，满压默认每秒增减 50 个百分点。双扳机按净差累计并限制 0–100%。十字键长按 0.35 秒后每 0.12 秒重复；左杆指针速度为原始 550×400 画布的 260 像素/秒，并随画面缩放。
- 键盘新按下、鼠标实际移动/点击或触摸新按下会切回原输入；进行中的鼠标/触摸与手柄蓄力各自持有手势，另一设备不能抢占。断连/失焦取消蓄力，恢复后先回中与松开按键。
- 不修改原始证据、行动资格、库存、武器公式、最大力、最小有效拖动或回合结算。原版基线：`Solid.as::twang/drawTwangLine`、`TileSystem.as::mouseDown/mouseUp`，见本目录 README 及 `06-WeaponsAndEffects/02-ReadyAimAndCancel`。

## 行为与验收

| ID | 可观察行为及状态转换 | 来源 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| GP-NAV-01 | 当前可操作 UI 内十字键只移动焦点，A 执行同一生产按钮；禁用、被弹窗遮挡及尚未完成淡入的控件不能执行；焦点共享悬停图像/描述 | 用户手柄方案；UI 资格沿用原入口 | MutinyInputHub / MutinyControllerUI / FrontendController / GameHUD | 实际前端选页、B 返回、武器菜单焦点及确认；重复 OnGUI 事件不重复执行 | 已实现；前端、武器面板、退出弹窗生产回归通过；前端悬停截图已检查 |
| GP-NAV-02 | 主菜单/选关等前端页面拥有手柄 UI 焦点与指针；后台存在玩家回合和已有角色焦点时，自动角色聚焦不得重写前端指针。十字键移动后松开保持按钮悬停，A 执行同一菜单按钮；回到棋盘仍可自动聚焦角色 | 用户报告主菜单十字键失效；GP-NAV-01 与 GP-CHAR-02 的授权输入边界，不属于原版新规则 | `PrepareControllerCharacterFocus`、`MutinyInputHub.CaptureInput`、`MutinyControllerUI.RouteFrame` | 真实前端与后台战斗夹具并存，十字键依次移到 Play/Scores/Help，松开等待后检查实际指针位置与 Help 回调；B 返回后再次选 Play；关闭前端后 A 仍选择镜头角色 | 已实现；修复前 299/301，检出 2 条失败；限定棋盘后 301/301 通过，含新增 7 条；实际 Help 悬停截图已检查；实物手柄/Android 待验收 |
| GP-CHAR-01 | 当前人类队伍活角色可获焦点，A 通过生产选人入口确认；移动焦点不修改 SelectedCharacter | 用户手柄方案；原版 Team.select / TileSystem.mouseDown | MutinyPlayerInput | 十字键移动焦点后角色仍未选中，A 打开行动菜单；跳跃后 B 不重选 | 已实现；焦点/选人和提交后锁定回归通过；多人空间导航实机待验收 |
| GP-CHAR-02 | 当前使用手柄进入玩家回合选人状态时，自动聚焦本回合镜头选定的活角色；镜头抵达后仍保留该目标，A 一次打开其行动菜单。十字键可换人；左杆主动移动后恢复指针选人，不在回中时强制抢回。鼠标、AI、弹窗和已提交行动不自动选人 | 用户本轮授权扩展；原版 `Team.as::startTurn:273-280` 仅指定运镜目标，不隐式选人 | `MutinyCameraController.TurnFocusCharacter`、`MutinyPlayerInput.PrepareControllerCharacterFocus`、`MutinyInputHub.CaptureInput` | 实际虚拟 Gamepad 在回合开始直接 A、运镜抵达后直接 A、鼠标切回手柄、两位队员十字键换人、左杆自由选人、正式 PassTurn 换队与第二回合、AI/死亡目标过滤；真实 HUD 同帧 A 不误触角落按钮 | 已实现；2026-09-26 隔离 Play Mode 新增 16 条与既有用例合计 294/294 通过，主工程编译通过；实物手柄/Android 待验收 |
| GP-AIM-01 | LT/RT 新按启动普通瞄准；左杆表示向后拉的方向，发射方向相反，回中保留最后拉动方向；累计力度映射至该行动原最大力；松开扳机保留力度，A 才经同源 Twang 提交 | 用户手柄方案、方向纠正与双扳机累计要求；原版 `Solid.twang/drawTwangLine` 的拖动端点/反向初速 | MutinyPlayerInput / MutinyTrajectoryRenderer | 左下拉→右上发射、左右及上下轴反向；无方向 A 不提交；20/30 最大力、半力、跳跃；松开双扳机后 A 正常发射 | 已实现；本轮 278/278 组合回归通过，包含此前双扳机 200/200 用例；此前 116/116 和特殊武器 170/170 属 RT 绝对力度的历史基线 |
| GP-CAM-01 | 右杆在普通待命/瞄准允许范围内平移，仍受原边界限制；瞄准坐标不随视角变化 | 用户手柄方案；原版边界 TileSystem.panCamera | MutinyCameraController | 实际瞄准中平移并提交，核对速度相同；对话、弹窗、AI 与跟随阶段不抢视角 | 已实现；瞄准中平移/方向保持及既有相机回归通过；完整战斗下的手柄专用边界/对话/AI 场景待实机 |
| GP-BACK-01 | B 撤销瞄准、返回菜单/选人，不扣资格或库存；取消优先于同帧 A；取消后必须先释放 LT/RT 才可再瞄准；已提交序列不能回退 | 用户手柄方案；沿用原取消资格 | MutinyPlayerInput / MutinyInputHub | A/B 同帧无提交；B 后按住任一扳机不重入；分级返回及锁定行为 | 已实现；双扳机新增回归及原 UI 返回通过 |
| GP-DEVICE-01 | 软件指针、输入死区、主动设备切换与手势所有权统一；断连/失焦不产生松开发射；恢复后需中立输入；鼠标/触摸保持原入口 | 用户手柄方案 | MutinyInputHub / MutinyPlayerInput / MutinyCursorManager | InputSystem 虚拟 Gamepad 驱动断连、失焦和设备切换；复跑现有鼠标/触摸取消及相机专项 | 已实现；虚拟设备与既有取消回归通过；实物手柄及 Android 蓝牙待验收 |
| GP-SCOPE-01（修订） | 当前武器栏全部开放手柄选择，含 Cannon；鼠标/触摸仍沿原可用性执行 | 用户本轮明确解除大炮暂缓范围 | MutinyGameHUD / MutinyPlayerInput | 大炮手柄真实菜单及命令可装备且未提交时不扣库存；鼠标原入口仍可用 | 已实现；本轮 278/278 组合回归通过，实际菜单装备/瞄准/两级 B 返回已通过；旧版大炮灰态资格已被本轮授权撤销 |

## 验证记录

详见 [初版验证记录](../../11-VerificationAndDebug/02-PlayModeValidation/CONTROLLER_VERIFICATION.md)。实物手柄和 Android 蓝牙手柄手感不因自动回归通过而登记为通过。

## 代码结构

- `MutinyInputHub` 自动启动并持有独立的 `Resources/Input/MutinyController.inputactions` 实例，每帧采样一次，确定当前界面上下文、设备和软件指针。
- `MutinyControllerUI.Button` 登记实际绘制且有资格的 IMGUI 控件。手柄确认返回同一按钮的回调入口，并按帧/页面消费一次；不伪造 Mouse 或 IMGUI 事件。
- `MutinyPlayerInput` 共享原选人、BeginAim、ResolveAimRelease、Twang、Cancel 与回合提交；控制器只把方向/力度转换为原始像素端点。
- `MutinyCameraController` 使用原相机边界和速度/加速度常量；仅手柄瞄准允许右杆平移，原鼠标蓄力的相机行为保留。
- `MutinyCursorManager` 与 HUD 提供软件指针、共享悬停图像、力度百分比和操作提示。
