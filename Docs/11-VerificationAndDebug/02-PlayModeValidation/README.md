# 11.02 · Play Mode 验证

[返回上级模块](../README.md)

## 职责

记录 Unity 中实际操作、画面、声音和状态结果。

## 边界

未运行的步骤不得标记通过。

## 本轮记录

- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Seagull Presentation Play Mode`，31/31 断言通过。25/60/120 FPS 子弹出生及连续投放共用父海鸥插值相位，实际 Transform 在中间帧更新；物理每 tick 一次，自主更新不重复推进；出生后 Start 不回写显示位置；实际撞地与入水的结算 tick 跨 FPS 一致，撞地只产生一次 50/50 爆炸，入水无爆炸。包含既有原版海鸥图层/时间轴回归；主工程投弹观感及 Android 真机待验收。

  本次共享 `MutinyPhysicsBody` 显示入口修改后，现有 `Validate Camera Movement` 11/11、`Validate Pieces Of Eight Presentation Play Mode` 4/4 复跑通过，未修改既有断言标准。

- [手柄验证（2026-09-26）](CONTROLLER_VERIFICATION.md)：真实 Input System 虚拟设备驱动生产 Update / OnGUI / Twang；大炮部署/瞄准/开炮、其他特殊武器、连续放置、飞行期控制及双扳机累计力度，最新 278/278 断言通过（含原大炮/AI/效果 24/24）。实物与 Android 手柄待验收。

- 2026-09-26：GM-08 `aitakeover 1` 隔离 Unity 6000.6.0f1 Play Mode 回归，最新 `RunGM()` 24/24 断言通过，含新增 12 条接管规则。真实玩家跳跃与正式回合结算、AI 候选/执行入口覆盖空中/落地后接管、只评价原角色、武器完整生命周期期间保持接管、换队/重开/GameOver 恢复控制及非法参数。实际鼠标、手机、拾取空投现场和全部武器画面待验收；详情见 [GM 工具矩阵](../05-GMTools/README.md)。
- GM-08 同轮额外实际协程 smoke 2/2 通过：未跳跃接管自动射击并换队恢复；真实玩家跳跃飞行中接管，落地后同角色自动射击并在 GameOver 恢复。自然驱动 AI、物理、回合 `Update`，不直接调用 AI 求值/执行测试入口；验证环境仍为隔离工程，不代表主工程目视验收。

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
