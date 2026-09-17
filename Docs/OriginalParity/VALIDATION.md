# 验证记录

## FRONT-01..08 单机前端流程（待 Play Mode）

1. 清除 `mutiny_highest_unlocked_level` PlayerPrefs 后进入 `Main` Play Mode。
2. 确认首先看到 550×400 比例的海面菜单背景、Mutiny Logo、Play / Scores / Help / Credits；移动鼠标时四个按钮文字出现悬停变化。
3. 点击 Scores / Help / Credits，页面不得跳转；点击 Play，应进入人数选择页。
4. 人数页确认 1 Player / 2 Player / Back 均有悬停效果；点击 2 Player 不跳转，Back 返回标题页。
5. 再进入人数页并点击 1 Player，确认 15 格按 5×3 显示。
6. 初始 01 显示两位编号和彩色敌方海盗；02..15 不显示编号，显示黑色海盗与问号，且没有解锁格的悬停高亮。
7. 点击锁定关卡不跳转；点击 01 加载 `level_01` 并恢复现有游戏 HUD、输入与游戏音乐。
8. 完成第 1 关后重新进入选关页，确认 02 解锁；重启 Play Mode 后解锁仍保留。
9. 特别核对标题页 `scores` / `credits` 的 R 与 `back` 的 K：它们的字身顶部应与相邻普通大写字母一致，只有原版装饰笔画向下延伸；普通态与悬停态不能发生纵向跳动。
10. 记录 550×400 截图并与原版第 41、91、101 帧对照。菜单背景分层滚动和位图字体最终逐像素结果仍属待确认项。

静态/编译验证（2026-09-17）：`PirateFont.as` 的逐字 `attachMovie` 路径以及 K/N/P/R 的 SVG 注册点已核对；`FRONT-08` 回归已加入生产字体布局入口。`dotnet build Assembly-CSharp.csproj --no-restore` 为 0 errors、1 个既有 warning。Unity Play Mode 视觉核对尚未执行。

## BUG-ROT-002 旋转衰减与显示平滑（待 Unity / 原版运行对照）

- 静态确认：`Character.advance` 在物理后按 `velocityX*3` 旋转；`Dynamite.advanceMotion` 在物理前按 `velocityX*2` 旋转；`Solid.advanceMotion` 每次落地按对象 `friction` 递减 X 速度、撞墙乘 `-0.4`。原版的角速度衰减来自后续 tick 读取变小后的 X 速度，不存在独立角速度字段。
- 已实现：逻辑角仍只在 25 Hz 生产物理回调中更新；角色、Dynamite 及其他原版可旋转武器在两个逻辑角之间按物理累积时间插值。角色 overlay 在插值旋转之后回正；Boulder 仍只旋转内部石体层。
- 回归：`ROT-TC01..04/07/08` 已更新，覆盖逻辑倍率、落地回正、水中旋转、Dynamite 1.7 摩擦造成的 `12°→8.6°` tick 增量和 `0/-6/-12°` 渲染采样。尚未在 Unity 菜单实际执行。
- 编译：2026-09-18 执行 `dotnet build Assembly-CSharp.csproj --no-restore`，0 errors、1 个既有 `MutinyLevelTest.levelXml` warning。
- 人工验收：Level 1 分别观察主动 Throw Self、爆炸击飞和 Dynamite。飞行阶段转速随 X 速度；每次落地/撞墙后逐步变慢，渲染帧间不能再出现“停住—跳角度”；角色最终仍逐次回正。Console 的 `[Mutiny:Rotation] character/weapon angular step` 应只在 vx 变化时显示 tick、vx、该 tick 角增量和目标角，`character floor damping` 显示落地回正前后值。

## 2026-09-15 自动检查

- 18 个关卡运行时副本齐全。
- 37 个 SFX、2 个 Music 已移入 `Resources/Audio`。
- 179 张武器 PNG、原有 tile/character/effect 资源已移入 `Resources/Art` 并保留原 GUID。
- 27 种角色 × 35 帧，共 945 张原版角色帧已加入运行时资源。
- TreasureChest 90 帧已加入运行时资源。
- 非 Editor/Verification 运行时代码已不再引用 `UnityEditor.AssetDatabase` 或 `Application.dataPath` 读取资源。
- `dotnet build Assembly-CSharp.csproj --no-restore`：0 errors，4 个既有序列化/测试字段 warnings。新增脚本通过临时 compile item 一并验证，生成的 csproj 已原样恢复。
- 跳跃与行动 UI 修复后再次执行 `dotnet build Assembly-CSharp.csproj --no-restore`：0 errors，1 个既有 `MutinyLevelTest.levelXml` 未赋值 warning。
- Unity 6.6 中实际执行 `Mutiny > Parity > Validate Turn Action UI`：29/29 assertions，Console 0 error。覆盖五张原版状态资源、红蓝/hover/disabled 映射、面板淡入门控、生产输入提交、跳跃后武器资格、`inactivity > 10` 及新回合重置。
- 新增四张按钮图与原版导出帧逐项 SHA256 相同，五种运行时状态图均为 86×57；Unity 资源 GUID 无重复。

## 下一次 Unity 人工验收

当前 Unity 正处于 Play Mode，新增 `.cs` 文件不会进入正在运行的旧脚本域。请先停止 Play Mode，等待右下角导入/编译完成，再确认 Console 没有红色编译错误。

编译完成后先执行菜单 `Mutiny > Parity > Validate Runtime Resources`。预期日志为 18 levels、39 audio clips、945 character frames、90 chest frames，且无 missing resource。

再执行 `Mutiny > Parity > Validate Turn Action UI`，预期 29/29 assertions。该结果验证资源与生产方法，不能替代下面的真实场景和原版画面对照。

重新进入 Level 1 后按顺序检查：

1. 等待 2 秒且不要点击，回合不得自动切换或显示平局。
2. 鼠标移到红方角色，出现角框；点击后出现行动菜单。
3. 先执行 Throw Self，安全落地并完成结算后，同一角色的菜单返回：跳跃显示禁用，有库存武器仍可使用；再提交通用武器后等待回合结束。另测新回合直接提交通用武器，此时不应再保留跳跃。原版静态证据见 `Specs/TURN_ACTION_UI.md`（E02/E06/E07）；本步骤尚未运行验证，特殊武器需独立核对覆盖行为。
4. 下一回合开始前尝试生成空投：应从画面上方落下，镜头最多跟随前 100 tick；不是每次都一定成功，因为原版随机列可能因角色/箱子阻挡而放弃本次空投。
5. 空投下落时降落伞应循环活动；注册点停在地面上方 15 px 后播放落地段并收起降落伞，箱体不能有一半埋进地面。
6. 主动按住角色拉力穿过箱子不应领取；松手后的跳跃或爆炸击飞穿过箱子应领取。开箱后第 10 tick 弹出第一件，此后每 40 tick 弹出一件，图标和红/蓝阵营框应对应实际领取者。
7. Console 中应分别出现 `[Mutiny][Chest] spawned/landed/opened/collected` 和 `[Mutiny][Camera] airdrop tracking started/released`；听到的声音顺序应为生成 `chest_appear`、首次触碰 `click`、每件入库 `icon_collect`。
5. 香蕉飞行后再次左键应立刻引爆；不点击则在静止后引爆，并在每次碰撞播放 `banana_bounce`。
6. 角色 idle 应使用各自 1–14 帧，受击飞行保持 hit 起始姿态，静止后播放 15–35 帧恢复段。
7. 选择 Anchor 后直接点击目标横坐标，不需要从角色身上拉抛物线；落地造成 60 伤害，停留 30 tick、淡出 10 tick。
8. 选择 Tidal Wave 后单击即可从水面左侧出现，以 20 px/tick 向右运动。

如果有失败，请保存 Console 第一条异常及其完整 stack trace，并在 `TODO.md` 对应编号下记录。
