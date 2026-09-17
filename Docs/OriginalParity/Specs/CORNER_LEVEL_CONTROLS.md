# 关卡右上角控制与退出弹窗

## 基线与范围

- 模块 ID：`HUD-CORNER-01..09`
- 原版：`Mutiny Source/mutiny-flash-game/mutiny.swf`，指纹见 `../REPLICATION_STANDARD.md`。
- 范围：游戏内右上角 Quit Level、Music、SFX，以及 `quit_prompt` 的 Continue / Back to menu 分支。
- 证据状态：静态确认；实现状态：已接入；验证状态：`dotnet build Assembly-CSharp.csproj --no-restore` 于本次改动后通过（0 error，既有 `MutinyLevelTest.levelXml` CS0649 warning）；Unity Play Mode / 原版画面对照待执行。

## 原版证据

| 证据 ID | 类型 | 定位 | 结论 |
| --- | --- | --- | --- |
| `HUD-CORNER-S01` | S | `CornerQuitButton.as:onRelease` | `Controller.popup.show` 为真时不响应；否则切到 `quit_prompt` 并将 `show=true`。 |
| `HUD-CORNER-S02` | S | `ContinueGameButton.as:onRelease` | Continue 只将 `popup.show=false`。 |
| `HUD-CORNER-S03` | S | `QuitGameButton.as:onRelease`、`TransitionTween.as:doTweenWithFunction` | Back to menu 经转场结束游戏，单人模式进入 `level_select_1p`。 |
| `HUD-CORNER-S04` | S | `MusicToggle.as`、`FxToggle.as` | 控件有 `_on_up/_on_over/_off_up/_off_over` 四态；RollOver 播放 `rollover`，Press 切换保存项。 |
| `HUD-CORNER-S05` | S | `MusicController.as:turnOnMusic/turnOffMusic/turnOnSfx/turnOffSfx` | 音乐关闭会停止音乐；重新开启从当前 menu/game 曲目重新开始。SFX 开关只改变 SFX 许可与保存值。 |
| `HUD-CORNER-A01` | A | `mutiny.swf.xml` 根时间轴 `popup`，矩阵 `(15500,4001)`；symbol 标签 `quit_prompt` | 弹窗是根时间轴上的独立 MovieClip，淡入淡出并覆盖游戏内控制。 |
| `HUD-CORNER-A02` | A | `DefineShape_326` bounds `(-3500,-2600)..(3499,2599)`、`quit_prompt` 中 `continue_game` 的 y=900 twips | 共用弹窗背景是 350×260 px，舞台矩形 `(100,70,350,260)`；Continue 是居中 280×24 px 按钮，舞台 y=245。 |
| `HUD-CORNER-A03` | A | `mutiny.swf.xml` 根时间轴：Quit `(9818,359)` twips、Music `(10239,359)` twips、SFX `(10658,359)` twips；`DefineSprite_358/354/349` | 三个图标的锚点分别是 `(490.9,17.95)`、`(511.95,17.95)`、`(532.9,17.95)`。普通帧为图标形状，hover 帧是包含气泡和文字的独立 shape。 |
| `HUD-CORNER-A04` | A | `Art/raster/sprites/DefineSprite_358_quit level button/{1,6}.png`、`DefineSprite_354_music toggle button/{1,6,11,16}.png`、`DefineSprite_349_sfx toggle button/{1,6,11,16}.png` | 导出的原版帧大小分别为 Quit `23×34`、Music `31×34`、SFX `47×34`；hover 帧中的像素文字依次为 `quit`、`music`、`sound fx`。 |

## 状态、输入和转换

| 规则 ID | 前置状态 | 输入与条件 | 状态变化 | 可观察结果 |
| --- | --- | --- | --- | --- |
| `HUD-CORNER-01` | 关卡内，未显示弹窗 | 点 Quit Level | `show=true`，保持 `quit_prompt` | 25 Hz 每 tick alpha +25%，到 100% 为止；popup 位于游戏层上方并消费角色选择/瞄准鼠标输入。 |
| `HUD-CORNER-02` | 退出弹窗显示或淡出 | 再点 Quit Level | 拒绝 | 不重置或重复打开弹窗。 |
| `HUD-CORNER-03` | 退出弹窗显示 | 点 Continue | `show=false` | 25 Hz 每 tick alpha -25%；alpha < 1 时隐藏，关卡继续。 |
| `HUD-CORNER-04` | 退出弹窗显示，单人模式 | 点 Back to menu | 结束当前关卡，前端转到单人选关 | 播放菜单音乐，保留关卡解锁存档。 |
| `HUD-CORNER-05` | 任意关卡内 | 悬停 Music / SFX | 无业务状态变更 | 根据当前开关显示 `on_over` 或 `off_over`，并请求一次 `rollover` SFX。 |
| `HUD-CORNER-06` | 任意关卡内 | 点击 Music / SFX | 翻转并保存对应开关 | 离开时为 `on_up/off_up`；音乐重开当前曲目，SFX 仅修改许可。 |
| `HUD-CORNER-07` | Quit popup 可见 | 绘制及悬停 | 使用独立的全宽按钮命中区 | 标题在舞台 y=95；Continue 在 `(135,245,280,24)`。用户授权的 Back to menu 置于下一行 `(135,280,280,24)`，不得缩窄按钮或把两段文字压入同一行。 |
| `HUD-CORNER-08` | 关卡内、未显示结束弹窗 | 绘制右上角控制 | 按根时间轴的 twip 锚点和各帧负 bounds 计算视觉矩形 | 使用原版 `quit level button`、`music toggle button`、`sfx toggle button` 的像素帧；不再使用文字型通用按钮。普通图标的可点击范围仍只覆盖图标形状。 |
| `HUD-CORNER-09` | 鼠标进入任一图标的普通帧命中区 | RollOver / RollOut | Quit 选 `over/up`；Music 和 SFX 按开关选 `_on_over/_on_up/_off_over/_off_up` | 悬停时显示原版帧内的气泡：Quit `quit`、Music `music`、SFX `sound fx`；进入时仅请求一次 `rollover` SFX。 |

## Unity 映射与用例

| 规则 ID | Unity 生产入口 | 回归用例 | 实际结果 |
| --- | --- | --- | --- |
| `HUD-CORNER-01..03` | `MutinyGameHUD` 的右上角绘制、`OpenQuitPrompt`、`ContinueQuitPrompt`、25 Hz tick | `HUD-CORNER-T01` | 未执行 |
| `HUD-CORNER-04` | `MutinyGameHUD.BackToSinglePlayerMenu` → `MutinyFrontendController.ReturnToSinglePlayerLevelSelect` | `HUD-CORNER-T02` | 未执行 |
| `HUD-CORNER-05..06` | `MutinyGameHUD.ToggleCornerMusic/Sfx` → `MutinyAudioManager` | `HUD-CORNER-T03` | 未执行 |
| `HUD-CORNER-07` | `MutinyGameHUD.ResolveOriginalPopup*Rect`、`DrawQuitPrompt` | `END-POP-T07` | 未执行 |
| `HUD-CORNER-08` | `MutinyGameHUD.ResolveOriginalCornerVisualRect/HitRect`、`DrawOriginalCornerControls` | `HUD-CORNER-T04` | 未执行 |
| `HUD-CORNER-09` | `MutinyGameHUD.ResolveOriginalCornerTooltip`、`ResolveCornerToggleTexture`、`UpdateCornerHover` | `HUD-CORNER-T03/T05` | 未执行 |

## 完成记录

- 已静态确认：原版的弹窗互斥、淡入淡出、单人返回路径及四态音频按钮。
- 已实现：原版 `DefineSprite_358/354/349` 的 normal / hover / on / off 像素帧已导入 `Resources/UI/CornerControls`；右上角绘制按根时间轴锚点和帧 bounds 对齐，hover 直接显示原版内嵌的 `quit`、`music`、`sound fx` 气泡。Quit 弹窗互斥与 25 Hz alpha、Continue、单人 Back to level select、Music/SFX 存档切换和原版 Stop/restart 音乐语义保持不变；`MutinyTurnActionUiVerificationTest` 已加入生产入口回归。
- 已执行并通过：无。
- 待运行验证：以 550×400 逻辑画布依次录制 Quit、Continue、Back to menu、Music/SFX 的 on/off 与 hover；逐帧对照原版 `quit_prompt`。`MusicToggle.as`/`FxToggle.as` 确认会请求 `rollover`，但当前导出的原版音频目录不含可命名为 `rollover` 的样本；Unity 保留该生产请求，实际波形待从原始 SWF 补导后对照，不能以其他 UI 声音替代。
