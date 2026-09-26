# 手柄特殊武器扩展规格（2026-09-26）

本规格在特殊武器扩展实现前建立，验收栏已按实际运行更新。来源分为用户授权的手柄操作（左摇杆移动释放位置、A 等同鼠标点击、显示原版武器图标）和原版武器自身规则；后者仍以 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/` 的 AS2 为准。大炮当时明确暂缓，后续已获用户授权实现，见 [大炮手柄规格](CONTROLLER_CANNON.md)。

## 共用输入规则

- 选择武器后，左摇杆只移动现有软件指针；镜头平移仍由右摇杆负责。手柄使用与鼠标相同的屏幕指针转世界坐标入口、原版资源光标、可用性与命中检查，不制造物理 Mouse/IMGUI 事件。
- 点击型武器在 `WeaponReady` 按 A，等同于鼠标指针当前位置的新按下。A 持续按住不得重复放置；B 沿用原有取消资格。同帧 B 优先于 A。
- 拉拽型武器维持 RT 蓄力、左摇杆向后拉方向、A 发射；仅在武器已有额外飞行期点击/持续输入时复用对应生产入口。所有库存与回合状态仍由现有武器方法提交。
- 点击型武器的 A 不得误进普通 RT/Twang 分支；弹窗/对话/过渡期间既不能提交武器，也不能透过遮挡操作底层控件。

## 行为与验收

| ID | 可观察行为与状态转换 | 原版/授权来源 | Unity 生产入口 | 必做验收 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| GP-SPC-01 | 手柄待释放期间沿用原版 Anchor、Seagull、TidalWave、Voodoo、木箱/火药桶及非法放置叉、降落伞风扇光标；图标随软件指针逐帧移动，普通黄色指针让位；切回鼠标或取消后恢复 | 用户本次要求；`TileSystem.as::advance`、DefineSprite 1813/1898 | `MutinyPlayerInput.UpdateSpecialWeaponCursor`、`MutinySpecialWeaponCursor`、`MutinyInputHub` | Gamepad 左杆移动后检查实际 Mode、图像、位置与独占可见性；断连/取消/切换设备 | 已实现；虚拟手柄验证图标模式/资源、锚指针移动、非法叉与风扇。逐帧实机目视和特殊图标切换设备待验收 |
| GP-SPC-02 | Anchor A 在软件指针 X 坠落，Y 固定 -200；Seagull A 取软件指针高度开始路径，飞行中每次新 A 请求投弹；TidalWave A 从固定 `(-550,waterY)` 启动，软件指针只决定点击时机 | 用户本次映射；`Anchor.as::place`、`Seagull.as::place/advance`、`TidalWave.as::place` | `MutinyPlayerInput.TryActivateClickWeapon`、`MutinySeagull.TryRequestPlayerShot` | 真实 Gamepad 选武器、移指针、A 触发；核对 X/Y、固定海啸起点、库存/阶段、重复 A 不连发 | 已实现；上述虚拟手柄 Play Mode 入口和状态断言通过 |
| GP-SPC-03 | 木箱三次、火药桶两次按 A 放置；非法位置显示叉并拒绝，不消耗库存；即使第一次后回合转执行阶段，剩余放置继续接受手柄输入 | 用户本次映射；`BoxWeapon.as::place`、`WoodenCrate.as`、`GunpowderBarrel.as` | `TryActivateClickWeapon`、`MutinyBoxRegistry`、`CanProcessCurrentTurnInput` | 真实指针驱动合法/非法点击、数量、库存只扣一次与执行阶段续放 | 已实现；虚拟手柄 Play Mode 检出并修复执行阶段角色门，三次/两次及库存断言通过 |
| GP-SPC-04 | Voodoo 先以原版娃娃光标移到敌角色按 A 绑定目标，绑定后恢复普通光标；再通过 RT/向后拉/A 走同源 Twang | 用户本次映射；`VoodooDoll.as`、`TileSystem.as::mouseDown` | `TrySelectVoodooTarget`、`BeginAim`、`ResolveAimRelease` | 目标边界、绑定后光标变化、实际投掷和库存；无目标时 RT 不进入瞄准 | 已实现；虚拟手柄 Play Mode 验证无目标禁瞄、目标绑定、图标消失和实际投掷；边界与库存专项待补 |
| GP-SPC-05 | Banana、ParachuteBomb 与 PiecesOfEight 可由手柄按普通拉拽提交；飞行中 Banana 新 A 引爆、ParachuteBomb 持续按 A 按软件指针左右扇风、PiecesOfEight 下一枚可再 RT/A 投掷且不能换武器 | 用户本次“完善不支持武器”；`Banana.as`、`ParachuteBomb.as`、`PiecesOfEight.as` | `ProcessControllerFrame`、`TryRequestPlayerDetonation`、`TryGetFanInput`、`CanFireNextCoin` | 首发 30/30/20 力；飞行期 A/按住状态；钱币第二枚入口；不重复扣库存 | 已实现；虚拟手柄 Play Mode 验证追加引爆、首次 A 不扇风、重按 A 扇风、金币第二枚与库存；完整八枚和实机待验收 |
| GP-SPC-06 | 操作资格由回合、角色、弹窗、对话、武器阶段共同决定；B 不回滚已提交的飞行/放置，手柄与鼠标切换保留原入口 | 用户本次映射；`TileSystem.as::mouseDown/mouseUp`、各武器原状态机 | `MutinyInputHub`、`MutinyPlayerInput`、`MutinyControllerUI` | 弹窗、AI、设备断连、鼠标切回与旧普通投掷回归 | 已实现；170/170 组合回归含旧投掷、弹窗、设备切换；特殊武器完整战斗与实机仍待验收 |

## 后续范围与实机验收

- Cannon 的旧暂缓状态已被用户后续的部署/瞄准操作要求解除；不再作为当前未适配武器。
- 实物手柄、Android 蓝牙、武器飞行过程的视觉手感仍需设备验收；自动 Play Mode 不替代实测。
