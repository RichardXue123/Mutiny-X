# 11.02 · Play Mode 验证

[返回上级模块](../README.md)

## 职责

记录 Unity 中实际操作、画面、声音和状态结果。

## 边界

未运行的步骤不得标记通过。

## 本轮记录

- 2026-09-27：用户授权的可选 `excamera` 运镜在隔离 Unity 6000.6.0f1 `Validate Explosion Camera Play Mode` 29/29 通过，默认关闭时既有 `Validate Camera Movement` 11/11 复跑通过。正式 GM 解析及真实爆炸/物理/镜头入口验证最近单角色、同帧汇总、不接力、连锁锁定、新批次、60/120 FPS 显示同源、空投/武器竞争和释放边界；规格与剩余项见 [EXT-EXCAM-01..04](../../09-PresentationAndFeedback/04-Camera/README.md)。没有 Flash 原版对应；主工程完整对局与 Android 真机待验收。

- 2026-09-27：`Mutiny/Parity/Validate AI Box Camera Play Mode` 在 Unity 6000.6.0f1 隔离工程 125/125 通过。实际 AI 执行木箱/火药桶后，每次下一计划点在 40 tick 准备期更新镜头，包含不同位置及堆叠、60/120 FPS 步长、空投优先级、释放与资格边界；原版 10 tick 间隔、三木箱/两火药桶数量及玩家自由滚屏不变。向上堆叠重新接管、第一箱后间隙保持目标为用户授权扩展，原版与扩展分别登记。主工程完整场景及 Android 真机待验收。

- 2026-09-27：`Mutiny/Parity/Validate Cannon Presentation Play Mode` 在 Unity 6000.6.0f1 隔离工程 45/45 通过（显示规则 21 条、既有大炮 24 条）。25/60/120 FPS 采样使用真实大炮摆放/碰撞入口；炮身与拉栓平滑显示，物理不重复推进，墙体接触和短拖释放保持逻辑位置，取消/开火清除显示滞后；既有 AI 待发射回合、炮弹烟迹与爆炸音效无数值回归。主工程真实鼠标/触摸操作观感与 Android 真机待验收。

- 2026-09-27：`Mutiny/Parity/Validate Air Drop Presentation Play Mode` 在 Unity 6000.6.0f1 隔离工程 5/5 通过；既有 `Validate Camera Movement` 复跑 11/11 通过。生产宝箱管理器的 25 Hz 下降、120/60 Hz 中间帧显示采样、降落伞帧号不重复推进及第 106 tick 落地转 `touchdown` 均已数值验证；主工程实际降落画面和 Android 真机待验收。

- 2026-09-27：`Mutiny/Parity/Validate Bottom Notices Play Mode` 在 Unity 6000.6.0f1 隔离工程 11/11 通过；`Validate Battle HUD` 27/27 通过。两类底部提示共用 FIFO；原版 70 tick 显示、71 tick 换下一条，speech 阶段暂停；25/60/120 FPS 的显示位置按剩余帧时间插值，实际队列与文本没有被渲染帧重复推进。原版时序静态依据是 `IngameTextArea.as::onEnterFrame`；高帧率位移为用户授权扩展。主工程实际 IMGUI 文字观感与 Android 真机待验收。

- 2026-09-27：新 GM-08 `aitakeoverwithluck {luck}` 在隔离 Unity 6000.6.0f1 Play Mode 的 `RunGM()` 通过 35/35 断言。旧命令拒绝、有限 `0..100` 参数、分数 Luck 候选采样、跳后续行动、原属性不变及回合结束清理经正式生产入口核对；真实鼠标/手机和空投现场仍待验收。证据见 [GM-08 记录](../05-GMTools/Artifacts/GM-08-LUCK-20260927.txt)。
- 新 GM-08 另以实际 AI 协程、物理和回合 `Update` 跑过 2/2 场景：接管首次行动与玩家跳跃途中接管，均由 AI 自动选海啸并在结束后恢复人类控制；角色原 Luck 未变。候选数由上述 35 条专项断言验证；真实界面输入仍待验收。

- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Seagull Presentation Play Mode`，31/31 断言通过。25/60/120 FPS 子弹出生及连续投放共用父海鸥插值相位，实际 Transform 在中间帧更新；物理每 tick 一次，自主更新不重复推进；出生后 Start 不回写显示位置；实际撞地与入水的结算 tick 跨 FPS 一致，撞地只产生一次 50/50 爆炸，入水无爆炸。包含既有原版海鸥图层/时间轴回归；主工程投弹观感及 Android 真机待验收。

  本次共享 `MutinyPhysicsBody` 显示入口修改后，现有 `Validate Camera Movement` 11/11、`Validate Pieces Of Eight Presentation Play Mode` 4/4 复跑通过，未修改既有断言标准。

- [手柄验证（2026-09-26）](CONTROLLER_VERIFICATION.md)：真实 Input System 虚拟设备驱动生产 Update / OnGUI / Twang；修复后台角色聚焦干扰主菜单十字键高亮，修复前 299/301、修复后最新 301/301 通过，含新增前端/战斗并存 7 条、自动角色焦点 16 条及原大炮/AI/效果 24/24；主菜单实际悬停截图已检查。实物与 Android 手柄待验收。

- 历史（2026-09-26）：旧 GM-08 `aitakeover 1` 隔离 Unity 6000.6.0f1 Play Mode 回归 `RunGM()` 24/24 断言通过，含旧命令 12 条接管规则。2026-09-27 该命令已由 `aitakeoverwithluck {luck}` 替换；历史数字不能作为新命令通过证据。实际鼠标、手机、拾取空投现场和全部武器画面待验收；详情见 [GM 工具矩阵](../05-GMTools/README.md)。
- 历史旧 GM-08 同轮额外实际协程 smoke 2/2 通过：未跳跃接管自动射击并换队恢复；真实玩家跳跃飞行中接管，落地后同角色自动射击并在 GameOver 恢复。此项使用已取消的旧语法，仅为历史结果；新语法的实际协程结果见上方 2026-09-27 记录。

- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Camera Initialization Play Mode`，13/13 断言通过：真实编号加载、重开、下一关、退出再进入及旧版烘焙场景均重置原版起点；第 7/13/30 关水位限制正确；延迟销毁旧关期间镜头已绑定新关并释放旧目标；开场气泡未显示时，镜头从重置位置按原版速度的 120 FPS 等效步长平移。现有 `Validate Camera Movement` 复跑 11/11 通过，涵盖跳跃、海鸥、海啸、炮弹显示位置、高刷新率步长及空投优先级。主工程完整开场画面与 Android 真机待验收。

- 2026-09-26：按 `WPN-CAN-PRESS-01` 复跑 Unity 6000.6.0f1 隔离工程 `Mutiny/Parity/Validate Character Aim Overlay Play Mode`，31/31 断言通过。武器和跳跃在叉外按下后拖到叉上持续按住不取消，在叉上松开正常发射/起跳；只有叉上新按下才取消，第二指按叉取消保留，覆盖层与右键行为无回退。此前 5/5、25/25 中有关“叉上松开取消”的规格已经撤销，仅作历史结果。真实鼠标操作与 Android 真机待验收。

- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Character Aim Overlay Play Mode`，25/25 断言通过：正式跳跃蓄力并显示轨迹时保留 P1 标记、选择框、血条和取消叉；右键/叉取消不消耗跳跃；正式松手起跳才隐藏；生产行动延续恢复标记/血条/选择框。包含既有覆盖层及触屏取消回归。原版 AS2/pcode 与用户截图一致；主工程画面和 Android 真机未运行，仍待验收。此前将 `Aiming` 解释为 `dragging` 的来源结论已更正。

- 2026-09-26：Unity 6000.6.0f1 隔离工程 Play Mode 执行当前 GM 专项 `RunGM()`，`12/12` 断言通过：GM-07 7 条、最近成功命令 GM-UI-02 3 条、`unlockalllevels` GM-03 2 条；GM-03 通过生产解析与历史按钮同源入口验证全部 18 关资格和真实存档键，测试结束恢复原进度。本次使用最新生产及回归文件，没有屏蔽断言。历史五按钮另有图形模式布局截图；鼠标点击/悬停、重启及实际选关 UI 待验收。详见 [GM 工具](../05-GMTools/README.md)。

- [Credits 页面验证（2026-09-25）](CREDITS_VERIFICATION.md)
- [结局角色动画验证（2026-09-26）](ENDING_IDLE_VERIFICATION.md)
- [结局音乐验证（2026-09-26）](ENDING_MUSIC_VERIFICATION.md)
- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Level Lifecycle Play Mode`，最终 8/8 断言通过：经生产放置链摆放的木箱/火药桶和已布置地雷在胜负 `GameOver`/结果弹窗期间保留，新关初始化及显式 `ClearLevel()` 卸载时清除旧对象。胜利 speech 的实际画面/角色站箱观感仍待手动验收。
- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Anchor Animation Play Mode`，9/9 断言通过：实际投放路径下落静帧、触地双侧碎屑子时间轴与 10 tick 原版白化参数均通过。使用项目原有的 `AnchorImpact` PNG/GUID；实际战斗画面与原版逐帧截图待验收。
- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Cannon Effects Play Mode`，19/19 断言通过：炮弹实际物理撞墙与直击角色均各发一次 `pop`，爆炸伤害入口及重复结束调用不双响；`CAN-AUD-03` 是用户授权扩展，原版角色直击静音。主工程实际战斗听感待验收。
- 历史记录（规格已被 `WPN-CAN-PRESS-01` 修正）：2026-09-26 Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Aim Cancel Touch Play Mode`，当时 5/5 通过；其中“跳跃蓄力在叉上松开取消”不是当前规格，应以本轮 31/31 为准。Android 真机多指触控与画面待验收。既有 `Validate Weapon Ready And Cancel` 广域测试另有 `WRDY-T01` 失败，原因待查，不计为本专项通过。
