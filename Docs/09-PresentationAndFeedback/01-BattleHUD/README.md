# 09.01 · 战斗 HUD

[返回上级模块](../README.md)

## 职责

显示回合中的全局战斗信息：小地图、队伍血条与头像、武器选择面板（行动菜单）、投掷/结束回合按钮、退出确认弹窗、游戏结束弹窗，以及右上角音频/退出控制按钮。

## 原版 Flash 对应

| Unity 类 | Flash 对应 | 说明 |
|---|---|---|
| `MutinyGameHUD` | 根时间轴 `_root` 上的 `Map`、`team1/team2`、`ActionPanel`、`IngamePopup`、`CornerControls` 多个 MovieClip | 统一在 IMGUI `OnGUI` 内绘制 |
| `MutinyBitmapFont` | `PirateFont.as` + `DangleFont.as` | 位图字体渲染，替代 Flash 的 DynamicText |
| `MutinyIngameTextArea` | 舞台上的 `text`（symbol 1896）、`IngameTextArea` | 回合与拾取宝箱的底部提示队列；由 HUD 使用 DangleFont 绘制 |

## 回合与宝箱提示

- `Team.as::startTurn` 提交 `Computer, take your turn` 或 `Player N, take your turn`；Unity 使用 `MutinyTurnManager.OnTurnStarted`，仅在实际开始新回合时加入一次，不在角色的剩余行动间重复提示。
- `TreasureChest.as::advance` 在首次接触 10 tick 后发放第一件武器，此后每隔 40 tick 发放下一件。每次武器实际入库的同一 tick 提交 `collected ` 加 `WeaponSelectButton.hoverText` 标题，并播放现有 `icon_collect`。Unity 复用 `GetOriginalActionCopy` 的武器标题映射，例如 `collected tidal wave`。
- SWF 中 `Controller.root.text` 位于 550×400 舞台 `(275,400)`，剪辑内只有居中的 DangleFont `textField`，位于剪辑原点下方 10 px。文字在 HUD 的 stage space 绘制，随 letterbox 等比缩放，随关卡根节点清除。
- 提示按 FIFO 排队，每个 25 Hz tick 调整显示剪辑的 Y 位置；船长对话开始前或进行中暂存并暂停底部提示。原版 `IngameTextArea` 的受保护 AS2 动作只保留了 `lines`、`thisLineFrame`、`onEnterFrame`、`_y`、`speechBubble` 等结构，精确的运动速度与保持帧数尚无可读源码。当前升起 10 tick、停留 40 tick、退场 10 tick，待原版逐帧录像核定这些数值。
- 原版 `Controller.changeLevel()` 与 `TileSystem.readXML()` 均可能调用 `startTurn()`；Unity 提示挂接关卡当前 `OnTurnStarted`，避免该重复入口造成同一句排队两次。

## 架构与类关系

```
MutinyGameHUD (MonoBehaviour, DisallowMultipleComponent)
├── 引用 MutinyTurnManager      → 读取回合状态、队伍、GameOver 结果
├── 引用 MutinyPlayerInput       → 读取 IsActionMenuOpen
├── 引用 MutinyLevelController   → 读取关卡索引、分数
├── 调用 MutinyBitmapFont        → 位图文字绘制
├── 调用 MutinyAudioManager      → 悬停音效、音频开关
└── 调用 MutinyFrontendController → "back to menu" 页面跳转
```

## 核心数据结构与常量

### 画布与缩放

| 常量 | 值 | 说明 |
|---|---|---|
| `OriginalCanvasWidth` | 550 px | 原版 Flash 舞台宽度 |
| `OriginalCanvasHeight` | 400 px | 原版 Flash 舞台高度 |
| `OriginalUiTickSeconds` | 1/25 s | 原版 25 Hz UI 刷新率 |
| `OriginalPanelAlphaStep` | 0.25 | 行动面板每 tick 透明度步进 |
| `OriginalPopupAlphaStep` | 0.25 | 弹窗每 tick 透明度步进 |

所有 HUD 绘制使用 `GUI.matrix` 进行 letterbox 缩放：

```
scale = Min(Screen.width / 550, Screen.height / 400)
left  = (Screen.width  - 550 * scale) / 2
top   = (Screen.height - 400 * scale) / 2
GUI.matrix = TRS(left, top, scale)
```

### 枚举定义

| 枚举 | 值 | 用途 |
|---|---|---|
| `MutinyThrowButtonVisualState` | Disabled / RedUp / RedOver / BlueUp / BlueOver | 投掷按钮 5 态 |
| `MutinyCornerToggleVisualState` | OnUp / OnOver / OffUp / OffOver | 音频开关 4 态 |
| `MutinyCornerControl` | Quit / Music / Sfx | 右上角按钮类型 |
| `MutinyGameEndPopupKind` | None / LevelComplete / LevelFailed / GameComplete | 结算弹窗类型 |

## 详细逻辑流程

### 1. Update 循环与 Tick 驱动

`Update()` 以 `Time.unscaledDeltaTime` 累积时间，每满 1/25 秒执行一次 tick：

```
AdvanceActionPanelAnimationTick()   → 行动面板淡入/淡出
AdvanceTeamHealthAnimationTick()    → 血条帧滑动
AdvanceQuitPromptAnimationTick()    → 退出弹窗淡入/淡出
AdvanceGameEndPopupAnimationTick()  → 结算弹窗淡入 + 分数滚动
```

### 2. 小地图 (`DrawOriginalMap`)

- **位置**：左上角 `(20, 20)` 像素
- **分辨率**：每个地形格 3×3 像素
- **颜色**：
  - 有地形块：`rgba(0,0,0,128)` 半透明黑
  - 无地形块：`rgba(0,0,0,51)` 浅透明黑
  - 宝箱：`rgba(255,255,0,128)` 黄色
  - Team1 角色：`rgba(255,56,41,255)` 红色
  - Team2 角色：`rgba(51,95,255,255)` 蓝色
- **边框**：白色实线 + 黑色阴影，向外扩展 10px
- **坐标映射**：`ProjectMapCoordinate(pixel, offset) = (pixel * 3) >> 5 + offset`

### 3. 队伍血条 (`DrawOriginalTeamHealth`)

- **帧动画**：97 帧（1=空 → 97=满），每 tick 向目标帧滑动 ±1
- **目标帧计算**：`1 + Floor(96 * sumHealth / sumMaxHealth)`
- **Team1**：红色填充 `rgba(255,56,41)`，从左向右
- **Team2**：蓝色填充 `rgba(51,95,255)`，从右向左
- **头像**：Team1 使用固定 `team1_portrait`；Team2 按关卡索引从 `Opponents/` 目录选取

### 4. 行动面板 / 武器选择 (`DrawBottomBar`)

- **打开条件**：`PlayerInput.IsActionMenuOpen && 当前队伍非 AI && 选中角色存活`
- **淡入**：每 tick alpha += 0.25，到 1.0 后 `contentsActive = true`
- **淡出**：每 tick alpha -= 0.25，到 ≤0 时重置为 `-0.25`（防止立即重开）
- **面板贴图**：红队用 `weapon_select_red`，蓝队用 `weapon_select_blue`
- **武器网格**：5 列 × 3 行 = 15 格，顺序为：

```
cherryBomb  boulder      dynamite     piecesOfEight  rumBottle
banana      parachuteBomb woodenCrate  gunpowderBarrel seagull
mine        cannon       anchor       voodooDoll      tidalWave
```

- **每格布局**（相对面板左上，单位 px）：
  - 格子位置：`(112 + col*31, 29 + row*43)`，大小 `24×36`
  - 图标区域：格内 `(3, 3)`，大小 `18×17`
  - 弹药区域：格内 `(0, 21)`，大小 `24×12`（数字），或 `(3, 23)` 大小 `18×9`（无限符号，SWF 540-80=460 twips 原点对齐）
- **标题/描述文字**：面板内 `(10.35, 1)` 和 `(20, 166)` 区域，悬停时动态切换

### 5. 投掷与结束回合按钮

- **投掷按钮**：位置 `(11, 29)`，大小 `86×57`
  - 5 态视觉：`ResolveThrowButtonVisualState(teamNumber, canThrow, hovered)`
  - 不可用时显示 `button_throw_disabled`
- **结束回合按钮**：位置 `(11, 93.95)`，大小 `86×57`
  - 4 态：红Up/红Over/蓝Up/蓝Over
- **取消按钮**：位置 `(256, 0)`，大小 `20×20`，仅在 `canThrow` 时显示

### 6. 退出确认弹窗 (`DrawQuitPrompt`)

- **触发**：点击右上角退出按钮 → `OpenQuitPrompt()`
- **面板**：`(100, 70)` 大小 `350×260`，带 2px 黑色阴影
- **边框**：外层 `rgba(239,49,28)` 红色，内层黑色（内缩 3px）
- **标题**：PirateFont 渲染 "quit level"
- **按钮**：
  - "continue"：`(135, 245)` 大小 `280×24` → `ContinueQuitPrompt()`
  - "back to menu"：`(135, 280)` 大小 `280×24` → `BackToSinglePlayerMenu()`
  - **悬停视觉**：悬停时切换为 50% 纯白提亮的 `button_back_over` 纹理，文字切换为黄色 PirateFont。
- **淡入/淡出**：每 tick ±0.25，4 tick 完成

### 7. 游戏结束弹窗 (`DrawOriginalGameEndPopup`)

- **触发**：`TurnPhase.GameOver` 时自动同步 → `SynchronizeGameEndPopup()`
- **种类判断**：

| GameOverResult | 条件 | PopupKind |
|---|---|---|
| Team1Wins | 非最终关 | LevelComplete |
| Team1Wins | 最终关 (index==15) | GameComplete |
| Team2Wins / Draw | — | LevelFailed |

- **分数滚动**：LevelScore 每 tick +287，TotalScore 每 tick +347
- **LevelComplete**：显示 level score + total score，按钮 "next level" (`button_small`) / "back to title" (`button_back`)
- **LevelFailed**：显示 final score，按钮 "restart level" (`button_small`) / "back to title" (`button_back`)
- **GameComplete**：显示 final score，按钮 "congratulations" (`button_small`)
- **悬停白色提亮规范**：
  - 弹窗中所有按钮均支持原版 Flash 的 50% 纯白提亮效果：
    - `next level` / `restart level` / `congratulations` 悬停时显示 `UI/Frontend/button_small_over`。
    - `back to title` 悬停时显示 `UI/Frontend/button_back_over`。
    - 悬停时标题文字同步切换为黄色 PirateFont。

### 8. 右上角控制按钮 (`DrawOriginalCornerControls`)

- **显示范围**：
  - **退出（Quit）**：仅在关卡中显示；弹窗激活时拦截重复打开。主菜单（前端）不显示退出按钮。
  - **音乐（Music）与音效（SFX）**：全局常驻显示（主菜单由 `MutinyFrontendController` 渲染，关卡中由 `MutinyGameHUD` 渲染），均支持独立静音切换与状态持久化保存。
- **布局参数**：
  - **退出**：视觉 `(479.9, 11)` 23×34，命中 `(483.95, 11)` 13.95×13.9
  - **音乐**：视觉 `(496.9, 11)` 31×34，命中 `(502, 11)` 19.95×13.9
  - **SFX**：视觉 `(501.9, 11)` 47×34，命中 `(525.95, 11)` 13.95×13.9
- **功能提示气泡弹出逻辑**：
  - 鼠标悬停在对应控制图标上方时，触发一次 `rollover` 音效。
  - 图标正下方立即弹出白色像素对话气泡，气泡顶部带有指向图标的向上三角形箭头指针：
    - **Quit**：下方弹出 23×17 px 对话气泡，显示 `quit`。
    - **Music**：下方弹出 31×17 px 对话气泡，显示 `music`。
    - **SFX**：下方弹出 47×17 px 对话气泡，显示 `sound fx`。
- **悬停保持机制（避免光标移动到气泡时关闭）**：
  - 初始未悬停时，使用图标范围 `hitRect` 检测鼠标进入。
  - 气泡弹出后，判定范围动态扩展为 `visualRect`（包含图标与气泡）。玩家光标移入气泡时保持悬停状态，离开整个组合区域时才恢复为 `up` 状态，与 Flash 原版 MovieClip 在 `over` 帧外包围盒命中特性一致。
  - 悬停期间，点击图标或提示气泡均可触发对应功能。
- **图层排序**：
  - 未悬停的控件优先绘制，悬停的控件与提示气泡置顶绘制，保证激活的气泡绝不被临近按钮遮挡。
- **字体资源规范与动态回退**：
  - 原版 Flash 中该提示文字是内嵌于位图形状内的 4×6 微型像素字体（非 DynamicText）；项目中的 `quit_over.png`、`music_*_over.png`、`sfx_*_over.png` 为原版 1:1 无损导出的点阵帧图。
  - 控制系统同时提供 `MutinyGameHUD.DrawCornerTooltipBubble` 动态程序化渲染兜底，支持在贴图缺失时使用 `MutinyBitmapFont` 的 DangleFont 渲染出对齐的像素气泡与文案。

## 位图字体系统 (`MutinyBitmapFont`)

### PirateFont

- **用途**：弹窗标题、按钮文字（大型装饰体）
- **图集**：`Resources/UI/Fonts/pirate_font`
- **字符集**：a-z、0-9、空格、`.` `,` `?` `!` `-`
- **两行表**：Normal 行 (y≈2-76) + Hover 行 (y≈117-191)
- **特殊字距**：k=27px、n=28px、r=29px（Flash ActionScript 覆盖）
- **默认 tracking**：-3px
- **API**：`DrawPirateText(rect, text, isHovered, centered, tracking)`

### DangleFont

- **用途**：描述文字、弹药数、关卡编号（小型等宽体）
- **图集**：`Resources/UI/Fonts/dangle_font`
- **船长对话**：`speechBubble` 的 `textHolder.textField` 使用 DangleFont 字形。对话专用渲染只取图集中的白色笔画，去掉黑色外轮廓，再以原版截图中的 `#666666` 绘制；其他 DangleFont 用途仍保留原有图集外观。
- **字符集**：a-z、0-9、空格、`.` `,` `?` `!` `-` `'`
- **字形高度**：11px（逗号 12px）
- **默认 tracking**：0px，行距 13px
- **多行分隔**：`|` = 换行，`||` = 空行
- **API**：`DrawDangleText(rect, text, color, anchor, tracking, lineSpacing)`

## 资源依赖

| 路径前缀 | 内容 |
|---|---|
| `UI/weapon_select_*` | 红/蓝武器面板背景 |
| `UI/weapon_slot_*` | 武器格子状态贴图 |
| `UI/button_throw_*` | 投掷按钮 5 态 |
| `UI/button_end_turn_*` | 结束回合按钮 4 态 |
| `UI/button_cancel_*` | 取消按钮红/蓝 |
| `UI/BattleHUD/team*_panel` | 队伍面板框 |
| `UI/BattleHUD/team1_portrait` | 玩家头像 |
| `UI/BattleHUD/Opponents/*` | 各关卡对手头像 |
| `UI/WeaponIcons/*` | 15 种武器图标 |
| `UI/weapon_ammo_infinite` | 无限弹药符号 |
| `UI/CornerControls/*` | 右上角按钮帧 |
| `UI/Frontend/button_small` | 弹窗通用按钮背景 |
| `UI/Frontend/button_small_over` | 弹窗通用按钮悬停背景（50%白色提亮） |
| `UI/Frontend/button_back` | 弹窗 "back" 按钮背景 |
| `UI/Frontend/button_back_over` | 弹窗 "back" 按钮悬停背景（50%白色提亮） |
| `UI/Fonts/pirate_font` | PirateFont 图集 |
| `UI/Fonts/dangle_font` | DangleFont 图集 |

## 与其他模块的接口

| 方向 | 模块 | 接口 |
|---|---|---|
| 读取 | `MutinyTurnManager` | `CurrentPhase`、`CurrentTeam`、`Team1/Team2`、`GameResult`、`TurnCount` |
| 读取 | `MutinyPlayerInput` | `IsActionMenuOpen` |
| 读取 | `MutinyLevelController` | `CurrentLevelIndex`、`LastCompletedLevelScore`、`SinglePlayerScore`、`LevelXml` |
| 读取 | `MutinyCharacter` | `IsAlive`、`CanThrow`、`CanShoot`、`HasWeapon()`、`GetAmmunition()` |
| 调用 | `MutinyPlayerInput` | `SelectCharacterThrow()`、`SelectWeapon()`、`EndTurn()`、`ReturnToCharacterSelection()` |
| 调用 | `MutinyAudioManager` | `PlaySFX("rollover")`、`ToggleMusic()`、`ToggleSFX()` |
| 调用 | `MutinyFrontendController` | `ReturnToSinglePlayerLevelSelect()` |
| 调用 | `MutinyLevelController` | `LoadLevel()`、`RestartCurrentLevel()` |

## 验证要点

- 行动面板 alpha 从 -0.25 → 1.0 需要恰好 5 tick（200ms）
- 弹窗淡入从 0 → 1.0 需要 4 tick（160ms）
- 血条滑动每 tick 最多 ±1 帧，从满到空最多 96 tick
- 小地图坐标映射使用位移 `>> 5` 而非除法，与原版一致
- 右上角命中区域严格小于视觉区域（排除工具提示像素）
- 分数滚动步进：LevelScore +287/tick、TotalScore +347/tick
