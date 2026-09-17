# 单机前端最小流程规格

范围：启动标题页、人数选择页、单人 15 关选关页，以及进入现有关卡。Scores、Help、Credits、2 Player 本阶段只显示和响应悬停，不执行跳转。

## 原版静态证据

- 根时间轴 `mutiny.swf.xml`：`title_screen` 位于第 41 帧，`game_select` 位于第 91 帧，`level_select_1p` 位于第 101 帧；舞台逻辑尺寸为 550×400。
- `PlayButton.as`：释放 Play 后调用 `doTween("game_select")`。
- `OnePlayerButton.as`：按下 1 Player 后设置 `game_type = 1`，跳转 `level_select_1p`。
- `BackButton.as`：`back_ls_button` 返回 `game_select`，其余 Back 返回 `title_screen`。
- `LevelSelectButton.as`：实例名 `ls_N` 产生关卡编号；解锁时显示两位数、对应海盗帧并允许点击；锁定时隐藏编号、把海盗设为黑色、显示问号并禁用点击。
- `SimpleButton.as` 与 `frame-labels.csv`：普通按钮初始为 `up`，悬停为 `over`，移出恢复 `up`；按钮标签对应第 1/10 帧。
- `PirateFont.as`：每个字符通过 `attachMovie("pir_" + 字符)` 挂到同一个 holder，代码只推进 `_x`，不修改字符 `_y`；因此字符必须保留各自导出 symbol 的注册点，不能按裁切图高度再次垂直居中。
- `DefineSprite_258_pir_k/1.svg`、`DefineSprite_267_pir_n/1.svg`、`DefineSprite_273_pir_p/1.svg`、`DefineSprite_282_pir_r/1.svg` 与 `sprite-origins.csv`：K 的顶部注册偏移为 0 px，N 为 -2 px，P/R 为 -1 px；其余菜单字母为 0 px。K/R 较高来自向下延伸的装饰笔画，不代表字形应整体上移 4 px。
- 根时间轴：15 个单机关卡按钮按 5×3 排列，左上坐标为 `(110,68)`，横向间距 70，纵向间距 90；Back 位于 `(275,334)`。
- `Controller.as`：单人通关调用 `setLevelUnlocked(selected_level + 1)`，第 15 关是单人流程终点。

## 行为规则

| ID | 可观察行为 | Unity 入口 | 验收 |
|---|---|---|---|
| FRONT-01 | 启动时显示 Mutiny Logo 和 Play / Scores / Help / Credits；只有 Play 执行跳转 | `MutinyFrontendController`, `MutinyFrontendFlow.PressPlay` | 启动后游戏场景不可操作；四按钮可见且悬停变化；点击 Play 进入人数页 |
| FRONT-02 | 人数页显示 1 Player / 2 Player / Back；只有 1 Player 与 Back 执行跳转 | `PressOnePlayer`, `PressGameSelectBack` | 1 Player 进入选关；Back 回标题；2 Player 保持当前页 |
| FRONT-03 | 单人选关页显示 15 格、海盗预览和 Back | `DrawLevelSelect`, `PressLevelSelectBack` | 显示 5×3 关卡；Back 回人数页 |
| FRONT-04 | 新存档只解锁第 1 关；通关后沿用现有存档逐关解锁 | `MutinySaveSystem.IsLevelUnlocked`, `UnlockLevel` | 清空进度后只有 01 可用；胜利解锁下一关且重启程序后保留 |
| FRONT-05 | 解锁格显示两位编号和彩色海盗，悬停进入 over 状态并可点击 | `DrawLevelButton`, `TrySelectLevel` | 01 显示彩色预览和编号；悬停出现高亮；点击加载 `level_01` |
| FRONT-06 | 锁定格显示黑色海盗与问号，不显示编号、无悬停态且不可点击 | `DrawLevelButton`, `TrySelectLevel` | 02 初始为黑色预览和问号；点击不离开选关页 |
| FRONT-07 | 前端播放菜单音乐，进入关卡切换游戏音乐 | `ShowInitialMenu`, `StartLevel` | 三个前端页面保持菜单音乐；进入关卡后播放游戏音乐 |
| FRONT-08 | PirateFont 同一行字形按原版 symbol 注册点共享基线；K 不上移，R/P 上移 1 px，N 上移 2 px | `MutinyBitmapFont.DrawPirateText`, `GetPirateGlyphTopOffset` | 标题页 `scores` / `credits` 的 R 与人数页、选关页 `back` 的 K 不再高出同一行；普通态和悬停态位置一致 |

## 资源映射

| Unity Resource | 原版来源 |
|---|---|
| `UI/Frontend/background` | `DefineSprite_191_menu_background_anim/1.png` 的 550×400 舞台裁切 |
| `UI/Frontend/title_logo` | 根时间轴 shape 1927 |
| `UI/Frontend/game_select_panel` | 根时间轴 shape 1954 |
| `UI/Frontend/level_select_panel` | 根时间轴 shape 1930 |
| `UI/Frontend/game_type_pirates` | 根时间轴 shape 1957 |
| `UI/Frontend/button_small`, `button_wide`, `button_back` | shapes 675、670、587 |
| `UI/Frontend/level_slot` | shape 633 |
| `UI/Frontend/LevelPreviews/01..15` | `DefineSprite_664` 第 1..15 帧 |

## 当前验证边界

- 静态证据足以确认页面、按钮、坐标、锁定状态和跳转规则。
- 原版菜单背景是分层滚动动画；当前最小实现使用其第 1 帧舞台裁切，动画速度留待运行对照。
- FFDec 对动态位图字体会导出白色矩形和整套字形画布，因此不把整张按钮 PNG 直接用于 Unity。按钮底图沿用原版 shape，文字单独绘制；PirateFont 的字符注册点已按原版 symbol 静态证据恢复，最终逐像素结果仍待原版运行截图验收。
