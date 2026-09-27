# Scores 页面验收记录

## 静态确认

- 根时间轴 `view_scores` 在原版使用 `PirateFont` 标题、`shape 825` 十行红边、`HiscoreBoard.as` 远端榜单和持续存在的 Back/版权对象。
- 本实现复用原版 `credits_panel`、`button_back`、版权贴图，并将 `shape 825` 导出为 `Resources/UI/Frontend/scores_rows.png`；动态文本由 Unity `DangleFont` 绘制。
- 用户授权的差异是只保存本地最高五次单人第 15 关总分，不调用原版联网榜单、分页或姓名提交流程。
- 原版失败弹窗第 21 帧有 `submitScoreButton`；用户 2026-09-27 明确要求失败时移除该按钮，仅在通关时自动入榜（`SCORE-EXT-04`）。

## 已实现

- `MutinyFrontendFlow` 增加 Scores 路由和 Back 门控。
- 主菜单 Scores 按钮进入页面，页面完整绘制原版 10 行红网格面板（397×261 px）、居中标题、三列名次/称号/分数排版、空榜引导提示、Back 按钮与版权。
- `MutinySaveSystem.RecordCompletedScore` 将成绩降序保存到 PlayerPrefs，最多五条；`GetTopCompletedScores` 在读取时再次排序并截断。
- 正式单人战斗的 `AwardSinglePlayerLevelWin` 在第 15 关总分形成时自动记录，已有每关一次的计分门控避免重复入榜。
- 失败弹窗的绘制和控制器命中列表仅包含「重试关卡」与「返回菜单」；原提交分数区域不再创建按钮或悬停态。
- 1~5 名成绩采用三列海盗风展示（排名 `01.`、军衔头衔 `FLEET ADMIRAL`/`CAPTAIN` 等、积分 `PTS`），第 1 名采用金色高亮，未填满槽位以虚位占位符显示，保证完整 10 行框架不坍塌。
- 空记录时在完整红框骨架内显示居中海盗战役引导文本（`no completed records yet | defeat all 15 campaign levels to record your high score!`）。

## 实际测试通过

- `dotnet build Assembly-CSharp.csproj --no-restore`：0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：0 错误。
- 2026-09-26 在隔离 Unity 6000.6.0f1 工程以批处理方式执行 `Mutiny/Parity/Validate Scores`：导航门控、原版资源尺寸、生产最终关计分入口的一次性自动存榜、非最终关/双人不入榜、降序排序、第五名淘汰和临时 PlayerPrefs 恢复均通过。
- 2026-09-27 在隔离 Unity 6000.6.0f1 工程 Play Mode 执行 `Validate Level Lifecycle Play Mode`：10/10 断言通过，包括失败弹窗生产按钮列表仅有重试/返回，以及生产最终关失败结果前后榜单不变。
- 2026-09-27 在同一隔离工程重新执行 `Validate Scores`：生产最终关胜利计分入口自动入榜一次，以及 Top 5 排序/截断等断言通过。

## 待运行验证

- Main 场景 Play Mode 中实际点击 Scores/Back，检查宽屏 letterbox、悬停换图、鼠标命中及动态背景。
- Main 场景 Play Mode 中实际失败一次，目视检查提交分数区域为空，鼠标/控制器无法聚焦或点击该区域，重试/返回正常。
- Main 场景 Play Mode 中通过真实第 15 关胜利回合跑一轮，确认完整 `MutinyTurnManager` → `AwardSinglePlayerLevelWin` 链路只写入一次，并在返回标题后重进 Scores 显示。

## 原版差异说明

- 原版使用 Flash 远端 Http 请求 Nitrome 官网排行榜；本实现使用本地 Top 5 通关榜。原版失败结算页的提交分数按钮按用户要求移除，保留原版榜单红网格美术资源。
