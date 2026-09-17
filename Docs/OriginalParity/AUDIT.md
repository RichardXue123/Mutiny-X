# 原版与 Unity 当前实现审计

## 已确认的原版行为

### 关卡读取

原版 `TileSystem.readXML` 使用 32 px 网格。前景和背景放在 `(x*32, y*32)`；角色中心 x 为 `(x+0.5)*32`，默认脚底范围下的 y 对应 `(y+0.75)*32`。`redPirate` 和 `redPirateCaptain` 属于玩家队，其他角色 type 属于敌队。角色库存直接来自 XML attributes。

XML 中除 `x/y/type/luck/maxChests` 外的武器 attribute，会按数值次数展开为空投权重池。有效空投列从上向下扫描：先遇到 `antichest` 背景则排除，先遇到实体前景则接受。

### 回合

原版 `Team.startTurn` 会重置所有存活角色的行动能力，并明确清空 `selectedCharacter`。首个队伍回合用船长作为镜头目标；之后使用距离当前镜头视觉中心最近的存活角色。角色尚未被选择并不代表回合完成。

Unity 当前 `MutinyTeam.StartTurn` 自动选船长或沿用角色，同时 `IsTurnComplete()` 把未选择角色视为已完成。这是原版进入关卡后可能立即换回合/平局问题的结构性来源，需要由 TurnManager 记录“本回合是否已经提交行动”来解决。

### 空投

原版 `TreasureChest` 同时最多 3 个。每箱从关卡权重池有放回抽取 1–3 件；从 y=-300 px 开始，每 tick 下落 3 px，在地面 y-15 px 停止。候选位置会避开相邻角色、已有空投同列和附近 box。角色接触后 10 tick 收第一件，此后每 40 tick 收一件，并播放 `icon_collect`。

当前 Unity 已接入完整生成入口、权重选择、有效列、单列失败不重抽、90 帧生命周期、逐件拾取、三个声音事件和 AI 空投评分；C# 编译通过，Unity Play Mode 与原版画面对照仍待执行。详见 `Specs/AIR_DROP.md`。

### 镜头

原版 550×400 视口的边缘区为 40 px，滚动速度每 tick 向 ±10 逼近 1。角色/武器追踪通常以 `(x-275,y-250)` 为目标、速度 30；空投前 100 tick 以 `(x-275,y-200)`、速度 50 跟踪。武器面板和拖拽状态会影响滚屏。

当前 `MutinyCameraController` 已有边缘滚动和行动对象跟踪的基础，但缺少空投、气泡、准确优先级、首回合/后续镜头目标和完整地图约束。

本轮已加入空投前 100 tick、速度 50、无垂直偏移的跟踪；气泡与多目标优先级仍未完成。

### 投掷预测线

原版 `Solid.drawTwangLine` 使用 2 px 白色实线显示拉力，拉力限制为 `twangMaxForce*4`；之后绘制 8 px 实/8 px 空的白色虚线，预测恰好 15 个模拟 tick，并逐段降低 alpha。每 tick 先按武器自身 `weight` 更新纵向速度。

Unity 已恢复外形和 15 tick 框架，但目前预测重力仍需按所选武器参数驱动。

## 已确认的资源利用问题

- 15 个武器图标和红/蓝武器选择面板已经导出，Unity HUD 只完成了部分布局和状态，SWF 按钮时间轴尚未完整恢复。
- 27 个角色此前只使用静态 Preview PNG；本轮已接入每种角色独立的 35 帧导出序列。嵌套 MovieClip action、死亡 symbol 和遮挡关系仍未完整恢复。
- `MutinyAudioManager`、多个武器和关卡加载器在运行时代码中使用 `UnityEditor.AssetDatabase`。这些对象在编辑器里可见，在 Player build 中不会被该代码加载。
- 15 个武器存在同名 C# 类不代表原版行为完成；多个特殊武器仍通过通用抛射/爆炸路径近似实现。

本轮已经把 27×35 个角色帧接入独立角色动画器，恢复 idle/hit 两个已知帧段及飞行旋转；死亡 symbol、时间轴 action 和装备遮挡仍待恢复。空投的触发、权重池、有效列、避让、下落、逐件拾取、inactivity、镜头和 AI 流程已建立，并恢复完整 1–90 根时间轴；仍需 Unity 运行验证。

## 验证原则

数值行为以反编译 AS2 和 25 Hz tick 日志为准；视觉以 SWF symbol、帧标签、变换矩阵和原版运行画面共同验证；音频以原版 `playSound` 调用点与导出 wav 名称共同验证。无法仅凭静态反编译判定的视觉节奏会列为人工验收。

## 2026-09-15 运行问题：角色无重力

场景由编辑器菜单提前生成，而 `PhysicsBodyState` 此前没有 `[Serializable]`。场景重新进入 Play Mode 后，该结构体的 `Weight`、extents 和 friction 全为 0，造成 Throw Self 匀速飞出。现已将状态标为可序列化，并在 `MutinyPhysicsBody.Awake/Start` 检测全零旧状态后恢复默认值；`MutinyCharacter.Start` 还会强制恢复原版角色参数 weight 1、friction 2、左右 extent 6、上下 extent 8。
