# 蓄力中被炸飞：实时轨迹验证（2026-09-29）

## 静态确认与已实现

- `AIM-MOVE-01`：原版 `Character.as::advance:166-178` 先推进运动及画面，再对仍处于 `Controller.twanging` 的角色绘制轨迹；`Solid.as::drawTwangLine:76-124` 每次从 `this.x/y` 计算鼠标相对位移，在 `mcHolder` 原点绘制。因此非致命炸飞时不是锁定最初按下位置。原件位于 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/`。
- `AIM-MOVE-02`：`Solid.as::twang:60-74` 在松手时再次读取当前 `this.x/y`。普通武器同样在 `Weapon.as::advance` 运动后重画，不能强制从使用者的旧位置发射。
- 生产修复：`MutinyPlayerInput.ShowTrajectory/ResolveAimRelease` 刷新真实角色/武器起点；`LateUpdate` 在物理 Update 后用上次持续指针端点继续重画。手柄端按现有方向/力度从实时起点重建端点，大炮专用拉栓路径不变。
- Unity 显示适配：`MutinyTrajectoryRenderer` 可接收独立显示根部。初速由实时权威起点计算，拉线与虚线在同一 PhysicsBody 的插值显示位置绘制，不改变模拟、重力或力度。预测仍为原版 15 个不做地形裁剪的 tick，不因未绑定地形缓存而阻止显示。
- 死亡继续沿用上一轮清理与结算；本轮不修改 Mine 触发、倒计时、伤害、击退或回合资格。

## 实际测试

Unity **6000.6.0f1**，独立临时工程：
`C:/Users/27487/AppData/Local/Temp/mutiny-selected-death-68b8d053c2cd4e7d85d311a2cbb277f1`。
入口：`Mutiny.Verification.Editor.MutinySelectedDeathVerificationMenu.RunBatch`，batchmode/nographics 的真实 Play Mode。

`aim-move-fixed.log`：

- 第 192601/192614 行：真实 Mine 非致命爆炸后保持固定指针、不再发新指针帧，跨四个真实 25 Hz tick 检查逐显示帧拉线/虚线根部、移动距离、实时预测方向，两条通过。
- 第 192627/192640 行：当前空中起点零拉距取消不消费跳跃；再经生产开始蓄力与松手入口检查实际 Twang 初速，两条通过。
- 第 192705 行：**死亡/实时轨迹组合 49/49 通过**，包含此前死亡换回合和存活空中起跳边界。
- 第 192718 行：既有覆盖层/取消 **32/32**、镜头 **11/11**，合计 **92/92**。

- 缺陷检出：只在临时工程令 `RefreshAimOrigin` 不再赋值、绘制根部退回该缓存起点，不改变生产主工程或断言。`aim-move-frozen-origin.log` 第 192896 行 **45/49**：新增四条全部失败，其他 45 条仍通过。第 192909 行既有取消/镜头仍为 32/32、11/11。这是针对起点的受控退化，不宣称重测完整历史版本；随后已从工作区恢复修复输入脚本。
- 手柄无图形尝试 `aim-move-controller.log` 第 216970 行 **288/303**，15 条失败集中于需要实际 OnGUI 控件登记的前端/HUD/大炮 UI 及其后续输入，既有方向/扳机力度/发射生产入口断言无失败。本轮不把该运行计为全套通过；无图形运行不能替代既有图形模式基线，未修改测试期望。
- 手柄图形模式尝试 `aim-move-controller-graphics*.log` 未进入 Play Mode，无可计数的测试结果。首轮启动后未开始；第二轮因首轮进程结束后生成的 Scene Backup Detected 提示阻塞；将该临时备份完整移至隔离工程 `GraphicsStartupBackup-20260929-0228/0.backup` 后再试，并只在临时测试启动器延后进入 Play，仍未实际启动。已关闭本次自行创建的图形测试进程、恢复临时启动器，没有停止用户主工程编辑器或改主工程启动器。图形手柄全套仍待从空场景菜单运行，不将环境启动尝试写成通过。
- 本次修改的 `MutinyPlayerInput.cs`、`MutinyTrajectoryRenderer.cs`、`MutinySelectedDeathVerificationRunner.cs` 与恢复修复后的隔离工程逐文件 SHA-256 一致。既有镜头、锚及其测试/文档的工作区改动均保留，不归入本修复。

## 待运行验证与已知边界

- 主工程实际地图中鼠标按住、镜头同时滚动和高刷新率目视对照待运行。
- Android 主指保持蓄力并由第二指移动镜头、手柄真机待验收。
- 插值根部是 Unity 显示层适配；不作为 Flash 存在插值的原版结论。
- 本轮不验收其他投掷武器在爆炸中独立运动的全部专用序列。
