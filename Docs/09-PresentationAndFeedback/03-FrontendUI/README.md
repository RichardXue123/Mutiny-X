# 09.03 · 前端 UI

[返回上级模块](../README.md)

## 职责

管理游戏前端页面的渲染与用户交互，包括：
1. **主标题页（Title Page）**：游戏 Logo、Play / Scores / Help / Credits 按钮。
2. **模式选择页（Game Select Page）**：1 Player / 2 Player 选项及 Back 按钮。
3. **关卡选择页（Level Select Page）**：15 个单人关卡缩略图槽位、解锁/未解锁状态及 Back 按钮。
4. **右上角音频控制（Corner Audio Controls）**：主菜单阶段显示并响应音乐（Music）和音效（SFX）开关，Quit 按钮仅在进入关卡后显示。

## 原版 Flash 对应

| Unity 类 | Flash 对应 | 说明 |
|---|---|---|
| `MutinyFrontendController` | 根时间轴 `title_screen`、`game_select`、`level_select_1p` 帧 | 统一在 IMGUI `OnGUI` 内渲染主菜单各页面 |
| `MutinyFrontendFlow` | ActionScript 页面路由逻辑 | 维护 `CurrentPage` 与页面跳转状态机 |
| `MutinyBitmapFont` | `PirateFont.as` + `DangleFont.as` | 前端按钮文字、面板标题及关卡编号渲染 |

## 架构与类关系

```
MutinyFrontendController (MonoBehaviour, DisallowMultipleComponent)
├── 持有 MutinyFrontendFlow      → 管理当前页面 (Title / GameSelect / LevelSelect / Gameplay)
├── 引用 MutinyLevelController    → 驱动进入关卡或隐藏关卡根节点
├── 调用 MutinyBitmapFont        → 绘制 PirateText / DangleText
├── 调用 MutinyAudioManager      → 播放背景音乐 (menu_music)、按钮与悬停音效、切换音频设置
└── 渲染 Corner Audio Controls   → 仅渲染 Music 与 SFX（Quit 仅在关卡中由 MutinyGameHUD 渲染）
```

## 画布与布局标准

### Android 屏幕方向

| ID | 可观察行为 | 来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AND-DSP-01 | Android 启动及运行期间只使用横屏；允许设备在 Landscape Left 与 Landscape Right 之间翻转，不允许进入两个竖屏方向 | 用户授权的 Android 适配要求 | `ProjectSettings.asset`：`defaultScreenOrientation=AutoRotation`，仅开启两个 Landscape 方向 | 构建后分别以竖持、左横持和右横持启动；画面始终为横屏且左右横屏可翻转 | 已配置；待 Android 构建与真机验证 |

前端 UI 使用与战斗 HUD 完全一致的 550×400 原版 Flash 舞台基准和 letterbox 缩放算法：

```csharp
float scale = Mathf.Min(Screen.width / CanvasWidth, Screen.height / CanvasHeight);
float left = (Screen.width - CanvasWidth * scale) * 0.5f;
float top = (Screen.height - CanvasHeight * scale) * 0.5f;
GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
```

> **注意**：Unity IMGUI 在设置 `GUI.matrix` 后，会自动将 `Event.current.mousePosition` 映射入当前的矩阵坐标系（即 550×400 的逻辑画布空间）。因此判定区域（`Rect`）直接使用 `Event.current.mousePosition` 进行 `Contains` 判定即可，切勿在此基础上重复进行二次屏幕点到画布点的缩放偏移转换。

---

## 页面逻辑与元素规范

### 1. 主标题页 (`DrawTitle`)

- **背景与 Logo**：
  - 全屏背景：`UI/Frontend/background` (550×400)
  - 标题 Logo：`UI/Frontend/title_logo` 矩形 `(54, 36)` 大小 `452×154`
- **按钮清单**（均使用 `button_small` 163×24，PirateFont 居中绘制）：
  - `play`：`(193, 187)` → 进入 `GameSelect`
  - `scores`：`(193, 216)`
  - `help`：`(193, 245)`
  - `credits`：`(193, 274)`

### 2. 模式选择页 (`DrawGameSelect`)

- **面板**：`UI/Frontend/game_select_panel` `(44, 24)` 460×350
- **标题**：PirateFont "select game" `(44, 38)` 460×26
- **提示文案**：DangleFont "click one of the buttons below.||play against the computer or|against a friend!" `(161, 76)` 300×60
- **装饰插图**：`UI/Frontend/game_type_pirates` `(127, 169.5)` 345×80
- **按钮**：
  - `1 player`：`button_wide` (200×24) `(63, 263)` → 进入 `LevelSelect`
  - `2 player`：`button_wide` (200×24) `(287, 263)`
  - `back`：`button_back` (140×24) `(205, 334)` → 返回 `Title`

### 3. 关卡选择页 (`DrawLevelSelect`)

- **面板**：`UI/Frontend/level_select_panel` `(44, 24)` 462×352
- **标题**：PirateFont "select level" `(44, 34)` 462×26
- **关卡槽位矩阵**：5 列 × 3 行，共 15 关
  - 单格位置：`x = 110 + col * 70`, `y = 68 + row * 90`，尺寸 51×77
  - 底框图：`UI/Frontend/level_slot`
  - 缩略图：`UI/Frontend/LevelPreviews/{01..15}` 位于 `(x + 5.25, y + 3)` 39×56
  - 解锁状态：
    - 已解锁：显示两位数关卡编号（如 "01"），点击/按下调用 `StartLevel(level)`
    - 未解锁：缩略图纯黑遮罩，中央显示白色 "?"
  - **悬停提亮规范（原版 Sprite 669 `level_select_button`）**：
    - 原版 Flash 中，鼠标悬停到已解锁槽位时，在第 10 帧（`over` 帧）的最顶层（depth 7）绘制 Sprite 668（51×77 槽位白色遮罩），并应用 `alphaMultTerm=102`（约 40% 的白色半透明覆层）。
    - 项目使用 `UI/Frontend/level_slot_over`，在绘制完底框、缩略图与文字后，在最顶层以 40% 不透明度（`Color(1f, 1f, 1f, 102f / 255f)`）叠画该白色遮罩，实现 1:1 的槽位白色提亮。
- **返回按钮**：`button_back` (140×24) `(205, 334)` → 返回 `Title`

---

### 4. 按钮悬停白色提亮规范（Flash 原版 SimpleButton 逻辑）

原版 Flash 中，所有按钮元件（如 Character 676 `play_button`、672 `scores_button`、674 `help_button`、631 `credits_button`、330 `1p_game_button`、333 `2p_game_button`、335 `back_button` 等）均继承自 Flash 核心的 `SimpleButton` 机制：
1. **帧结构**：
   - 帧 1 为常规未悬停态（`up` 帧）。
   - 帧 10 为鼠标悬停态（`over` 帧）。
2. **Flash ColorTransform 提亮参数**：
   - 在 `over` 帧上，底框形状（depth 1）应用了 Flash 颜色变换：
     `redMultTerm=128, redAddTerm=128, greenMultTerm=128, greenAddTerm=128, blueMultTerm=128, blueAddTerm=128, alphaMultTerm=256, alphaAddTerm=0`。
   - 对应数学计算：$C' = \lfloor C \times 0.5 \rfloor + 128$，即原图色彩缩减 50% 并叠加 128 的纯白色，同时保持原有 Alpha 边缘通道不变。
3. **文字高亮**：
   - 按钮内的 PirateFont 标题文字在悬停时切换至黄色字符（`isHovered = true`）。
4. **项目运行时资源**：
   - 项目在 `Assets/Mutiny/Resources/UI/Frontend/` 下提供了 1:1 数学烘焙的悬停底图资源：
     - `button_small_over.png` (163×24)
     - `button_wide_over.png` (200×24)
     - `button_back_over.png` (140×24)
   - `MutinyFrontendController.DrawOriginalButton` 与 `MutinyGameHUD.DrawGameEndButton` / `DrawPopupButton` 均在 `hovered` 时自动切换为对应的 `_over` 纹理并绘制黄色 PirateFont，完全重现原版 Flash 的白色提亮视觉体验。

---

## 右上角控制按钮差异规范（Quit vs. Music & SFX）

原版 Flash 结构中：
- `music toggle button`（characterId 354，深度 201）与 `sfx toggle button`（characterId 349，深度 205）在根时间轴的 `title_screen` 帧初始化并常驻舞台，因此**主菜单与关卡内均需显示并响应**。
- `quit level button`（characterId 358，深度 196）仅在进入关卡（`game` 帧）时生成，因此**主菜单不显示，仅在关卡进行中显示**。

### 前端音频按钮实现 (`DrawCornerAudioControls`)

| 控件 | 视觉位置 (`ResolveOriginalCornerVisualRect`) | 命中区域 (`ResolveOriginalCornerHitRect`) | 资源路径 | 交互逻辑 |
|---|---|---|---|---|
| **Music** | `(496.9, 11)` 31×34 | `(502, 11)` 19.95×13.9 | `UI/CornerControls/music_{on/off}_{up/over}` | 点击切换 `MutinyAudioManager.Instance.ToggleMusic()` |
| **SFX** | `(501.9, 11)` 47×34 | `(525.95, 11)` 13.95×13.9 | `UI/CornerControls/sfx_{on/off}_{up/over}` | 点击切换 `MutinyAudioManager.Instance.ToggleSFX()` |

- **功能提示气泡弹出逻辑**：
  - 鼠标悬停在图标（或气泡）上方时触发 `rollover` 悬停音效，对应 UI 位置下方弹出白色气泡，显示功能提示文字：
    - **Music**：弹出 31×17 px 气泡，文字提示为 `music`。
    - **SFX**：弹出 47×17 px 气泡，文字提示为 `sound fx`。
- **悬停保持机制（防止移动到气泡时闪退）**：
  - 未悬停时，判定区域为紧凑的图标形状 `hitRect`。
  - 悬停激活后，气泡展开至下方 y=28..45 区域，此时判定区域自动扩展至 `visualRect`（涵盖图标与整个提示气泡），光标下移阅读或点击气泡均能稳态保持悬停状态，完全还原 Flash 原版 MovieClip `over` 帧外包围盒特性。
- **图层渲染顺序与点击命中**：
  - 未悬停的控件优先绘制，悬停的控件与气泡置顶后绘制，保证当前激活的提示气泡绝对不被临近按钮裁切遮挡。
  - 悬停状态下点击图标或提示气泡均可触发音频开关。
- **字体资源规范与动态回退**：
  - 原版 Flash 中该提示文字是内嵌于位图形状内的 4×6 微型像素字体（非 DynamicText）；项目中的 `music_*_over.png` 与 `sfx_*_over.png` 为原版 1:1 无损导出的点阵帧图。
  - 控制系统同时提供 `MutinyGameHUD.DrawCornerTooltipBubble` 动态程序化渲染兜底，支持在贴图缺失时使用 `MutinyBitmapFont` 的 DangleFont 渲染出对齐的像素气泡与文案。
