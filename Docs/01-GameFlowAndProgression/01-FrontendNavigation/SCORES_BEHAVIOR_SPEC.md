# Scores 页面与本地通关榜规格

基线：`Mutiny Source/mutiny-flash-game/mutiny.swf`；`Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml` 根时间轴 `view_scores` 第 71 帧及 `HiscoreBoard` 资源。用户截图仅作为原版画面参考。以下 `SCORE-EXT-*` 为用户本次授权的 Unity 扩展，不声称与原版联网榜单逻辑一致。

| ID | 可观察行为与状态转换 | 原版来源或用户需求 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| SCORE-UI-01 | 标题页点击 Scores 进入独立页面；菜单背景、水云、右上角音频按钮继续显示 | `ScoresButton.as::onRelease` 跳转 `view_scores`；根帧 71 | `MutinyFrontendController.DrawTitle` → `MutinyFrontendFlow.PressScores` | 实际点击 Scores，检查页面与转场；其他页面不接受该路由 | Flow 门控与资源验证通过；Main 场景实际鼠标点击、背景和转场待运行 |
| SCORE-UI-02 | 页面显示原版红框深灰面板、`SCORES` 海盗字标题、Back 按钮和底部版权；榜单区域使用原版红边行资源与像素字体 | 根帧 71、持续存在的面板/Back/版权对象；sprite 828、shape 825；用户截图 | `MutinyFrontendController.DrawScores`、`Resources/UI/Frontend/scores_rows` | 核对 550×400 舞台与宽屏 letterbox 的可见性、位置、层级、字体、悬停和命中 | 397×261 行资源与 462×352 面板检查通过；Main 场景像素/悬停核对待运行 |
| SCORE-UI-03 | Scores 页点击 Back 返回标题；别的页面调用 Scores Back 不改变状态 | 根帧 Back 按钮、`BackButton.as::onRelease` | `MutinyFrontendFlow.PressScoresBack` | 实际点击、再进入，并检查页面门控 | Flow 门控验证通过；Main 场景实际按钮待运行 |
| SCORE-EXT-01 | 单人第 15 关战斗结果确认为胜利并计入最终总分时，自动保存该次总分一次；不依赖结局页的按钮，失败、双人及非最终关不入榜 | 用户指定的新规则；Unity 现有单关计分与第 15 关完成入口 | `MutinyTurnManager` → `MutinyLevelController.AwardSinglePlayerLevelWin` → `MutinySaveSystem.RecordCompletedScore` | 经正式 GameOver/计分入口完成第 15 关，重复观察不得二次记录；失败、双人、非最终关不得记录 | 隔离 Unity 以生产 Award 入口验证最终分数只记录一次；完整第 15 关 GameOver 实战待运行 |
| SCORE-EXT-02 | 历史成绩按总分从高到低排序，最多保留五次**通关记录**；同分的不同通关可分别占位，低于第五名的新分不挤出旧成绩；退出游戏后保留 | 用户指定的新规则 | `MutinySaveSystem.GetTopCompletedScores` / `RecordCompletedScore` | 依次写入超过五次分数，核对排序、同分、边界、重启后读取 | 隔离 Unity 生产持久化验证通过：降序、第五名淘汰和跨读取保留；实际用户存档重启待运行 |
| SCORE-EXT-03 | 打开 Scores 时显示当前本地前五次记录（排名、精确到秒的通关时间 `yyyy-MM-dd HH:mm:ss`、分数）；无记录时显示空榜提示，不访问原版网络接口，不显示原版 Loading/分页/提交名字 | 用户明确要求不用原版逻辑 | `MutinyFrontendController.DrawScores` | 无记录、1 条、5 条、超过 5 条时核对可见行与内容；返回再进显示最新结果 | 绘制与空榜分支已实现；Main 场景四种数据量的画面对照待运行 |

原版 `HiscoreBoard.as` 使用远端榜单，资源中有十行及分页箭头；本次只取原版外观元素。动态榜单行必须逐项绘制，不能把导出帧 PNG 当作最终 UI。原版静态导出的白色块是动态文字占位，不是目标文字外观。
