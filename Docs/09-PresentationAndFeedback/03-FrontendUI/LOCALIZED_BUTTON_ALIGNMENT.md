# 简体中文按钮文字垂直对齐（LOC-BTN-01）

| ID | 可观察行为与状态转换 | 来源 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| LOC-BTN-01 | 切到简体中文后，前端导航、结尾返回、退出确认及胜负结算中的文字在各自按钮底图内视觉居中；正常/悬停态位置一致。切回英语时继续使用原版 PirateFont 的放置位置；按钮底图、命中区域及非按钮标题不移动 | 用户 2026-09-27 反馈简中按钮文字整体偏下；简中为授权扩展。英文按钮位置源于 SWF SimpleButton 与 `PirateFont.as`，参见本模块 [README](README.md) 的按钮时间轴和资源记录 | `MutinyLocalizedText.PirateButton` → `MutinyFrontendController.DrawOriginalButton`、`DrawEnding`、`MutinyGameHUD.DrawPopupButton`、`DrawGameEndButton` | 通过生产语言切换入口在英文、简中间切换，检查按钮文字绘制矩形仅简中上移且底图/命中矩形不变；在 550×400 与宽屏画面检查标题页、Credits/Scores Back、退出确认及结算按钮正常/悬停状态 | 代码与生产调用入口已更新；隔离 Unity Play Mode 中 GM 语言切换及两种按钮尺寸的布局断言通过；实际画面仍待目视验收 |

中文字体的可见字形与原版 PirateFont 的位图边界不同。只对按钮文字做光学位置补偿，其他 `Pirate` 标题文字保持原位；不改交互状态或翻译。

## 验证记录

- **静态确认：** 前端导航、结尾返回、退出和结算按钮原来都把整张 24 px 高按钮矩形交给 `MutinyLocalizedText.Pirate`；英语由原版 PirateFont 位图渲染，简中由 Noto Sans CJK SC 的 18 px IMGUI 文字渲染。
- **已实现：** 上述按钮改用同一个 `PirateButton` 入口。简中仅将文字绘制矩形上移 3 个 550×400 画布像素；英语与非按钮标题保持原位。底图、悬停判定和点击区域仍使用原始按钮矩形。
- **实际测试通过：** 2026-09-27，`Assembly-CSharp` 和 `Assembly-CSharp-Editor` 编译通过。隔离 Unity 6000.6.0f1 Play Mode 从生产 GM `setlanguage zh-cn/en` 切换，LOC-BTN-01 两条布局断言通过；语言专项共 10/10，对白和提示 124/124 通过。
- **待运行验证：** 在主工程/玩家构建的 550×400 与宽屏画面逐页目视检查简中按钮字形的视觉中心，以及正常/悬停状态。布局断言不等于像素画面验收。
- **已知差异：** 简中使用光学位置补偿；原版仅有英文位图文字，故没有可直接对照的原版中文基线。
