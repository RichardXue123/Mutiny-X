# 大炮手柄规格（2026-09-26）

本规格在实现前建立，验收结果按实际运行更新。用户本轮明确解除大炮暂缓范围，并确认大炮仍用后拉方向：左下拉对应右上开炮。手柄操作是用户授权扩展；原版鼠标/触摸流程继续独立保留。

原版来源：`Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Cannon.as` 与 [大炮模块](../../06-WeaponsAndEffects/09-Cannon/README.md)。原版炮身摆放有范围与碰撞，拉环 X 在 -21 至 -40，拉栓有效释放后固定 30 力；手柄 A 在任意视觉蓄力值都能提交，是本轮用户明确授权的差异。

| ID | 可观察行为及状态转换 | 来源 | Unity 生产入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| GP-CAN-01 | 大炮可在真实手柄行动面板选择。待命时左摇杆直接移动部署位置；目标先限制在角色上方原版部署圆内，再走地形/箱体碰撞，不以坐标直写穿墙。右杆只移镜头，摇杆回中后部署位置保持 | 用户本轮授权；原版部署圆/Solid 碰撞 | SelectControllerWeapon、ProcessControllerFrame、MutinyCannon.MoveForController | 实际 UI 选择；左杆移动与部署边界；墙/箱阻挡；右杆平移不搬动炮身 | 已实现；真实 UI、部署移动、范围、墙/箱阻挡通过；右杆在瞄准期验证位置保持，部署期单独平移待实机 |
| GP-CAN-02 | LT/RT 新按进入 Aiming 后左杆改为后拉方向，炮身位置冻结。LT/RT 沿用累计力度速率，只改变拉环长度；松开扳机保留瞄准/视觉力度，回中保留炮口朝向。未调左杆时沿用炮身当前朝向 | 用户本轮授权；原版旋转与 PinX | BeginAim、TryBeginControllerAim、UpdateControllerAim、ControllerPower | LT 与 RT 入瞄准；0%/半拉/满拉显示；左下拉朝右上；瞄准与移镜头不搬动炮身 | 已实现；虚拟手柄与真实 Update/UI 验证通过，半拉实际 PinX=-30.5；瞄准截图已目视检查 |
| GP-CAN-03 | A 提交任意视觉力度，装填固定 30 力，沿现有拉环回弹/独立炮弹/音效/镜头路径开炮；库存只扣一次，立即进入 ActionExecuting，装填期间保持行动未结束。部署待命时 A 不开炮 | 用户本轮授权；原版固定 30 力/回弹/炮弹生命周期 | CommitControllerShot → LoadShot → AdvanceOriginalTick/FireCannon；Cannon.IsFirePending / TurnManager.CheckAllBodiesAtRest | 0%/半拉/满拉分别实际开炮并核对相同初速、库存、阶段；按住 A 不重复；装填阶段不结算 | 已实现；0%/50%/100% 三组真实炮弹均 30 力，库存、阶段、装填阻止结算与按住 A 断言通过 |
| GP-CAN-04 | 瞄准时 B 返回部署待命，保留炮身位置与装备，复位拉环/力度，不消耗库存或行动；再按 B 回行动面板。取消后双扳机都需回中。A/B 同帧 B 优先，失焦/断连同样取消瞄准 | 用户本轮授权；既有取消/设备门 | CancelCurrentAim、CancelPointer、TryControllerBack、MutinyInputHub | B 后可继续移动炮身；RT/LT 持续按住不重入；A/B 同帧不提交；断连/恢复与焦点丢失 | 已实现；实际 UI B、同帧取消、位置/装备保留、重新部署、RT 回中保护、失焦/断连通过 |
| GP-CAN-05 | 原版鼠标/触摸仍以拉栓 `<-30` 为有效释放门，并维持原部署拖动及 AI 延迟；其他手柄普通与特殊武器继续原路径 | 原版规则与用户本轮兼容要求 | ReleasePointer、原 Cannon/AI 入口、ControllerVerification | 复跑生产 Cannon/AI 回归与全手柄组合用例 | 已实现；原 Cannon/AI/烟雾/碰撞音效 24/24；完整组合 278/278 通过 |

实物手柄、Android 蓝牙与完整关卡大炮画面/手感仍需设备验收，不能由虚拟手柄回归代替。

## 运行证据

隔离 Unity 6000.6.0f1 图形模式 Play Mode **278/278**，见 [运行报告](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/CONTROLLER-CANNON-20260926.txt)及 [实际瞄准截图](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/CONTROLLER-CANNON-20260926-aim.png)。截图已检查炮口朝右上、拉环朝左下、力度显示与 B 返回部署提示。

测试夹具在真实回合初始化前通过正式木箱放置入口建立障碍；不会先提交玩家回合再手动改回 TurnActive。每组夹具清理自身创建的爆炸，防止污染后续静止计时与音效断言。原 AI 结算用例将对方角色放在水平弹道外，经实际越界入口完成炮弹生命周期；角色/墙体命中的爆炸另由既有碰撞专项验证。
