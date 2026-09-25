# 单人通关结局（原版一致性）

基线：`Mutiny Source/mutiny-flash-game/mutiny.swf`，SHA-256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`；FFDec 26.3.0 导出见 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/` 与 `Art/`。以下均为原版静态确认，运行效果另验。

| ID | 可观察行为与状态转换 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| END-SEQ-01 | 单人第 15 关胜利后的 `game_complete_1p` 弹窗按下 congratulations，过场转入 `congratulations`，卸载战斗关卡；其他结算不进入结局 | `Controller.as::enterFrame`；`CongratulationsButton.as::onRelease`；根帧 131 | `MutinyGameHUD.CompleteCampaignAndShowEnding` → `MutinyFrontendController.ShowEnding` | 经生产胜利结算入口打开最终弹窗并触发按钮，检查页面、分数；非最终关不能进入 | 静态确认；已实现；Play Mode 生产结算入口通过；非最终关仍待运行 |
| END-SEQ-02 | 结局背景保持菜单云、水动画，船画面常驻；影片剪辑 801 从帧 1 播放，帧 774 `stop()` | SWF sprite 802 内嵌 801；`MenuBackgroundAnim.as`；`DefineSprite_801/frame_774/DoAction.as` | `MutinyFrontendController.DrawEnding`/`Update` | 以 25 Hz 从帧 1 推进至 774，核对船和背景、停帧 | 静态确认；已实现；Play Mode 时间轴停帧通过；画面待目视 |
| END-SEQ-03 | 对话剪辑按帧 40–300、310–410、425–525、535–635、645–746 显示；每段文字按行约 10 帧揭开，空档只显示船 | `DefineSprite_801` 放置/移除及 787/789/792/794/796 的遮挡条逐帧变换；784/788/791/793/795 的 `DangleFont` 构造脚本 | `MutinyEndingSequence.DialogueIndexAtFrame`、`LineRevealAtFrame` 与 `DrawEnding` | 驱动关键帧前后，核对段号、文本、逐行进度及空档 | 静态确认；已实现；Play Mode 时间轴与资源通过；逐像素效果待目视 |
| END-SEQ-04 | 帧 754–770 分数面板从上方落入，显示当前总分；按钮自 754 帧随面板出现，back to title 经转场回标题并启动菜单音乐 | `DefineSprite_801` 深度 57 矩阵；`DefineSprite_800/frame_1/DoAction.as`；`BackButton.as::onRelease`；根帧 41 | `MutinyEndingSequence.ScorePanelTop`、`BackButtonRect`；`MutinyFrontendController.DrawEnding`/`ReturnFromEndingToTitle` | 检查面板位置、分数、按钮资格和返回标题 | 静态确认；已实现；Play Mode 位置/资格/返回入口通过；音画待目视 |
| END-SEQ-05 | 船是单帧容器，但其中红衣角色 767、黑帽角色 779 各自循环 12 帧；船底五个波纹实例分别循环 16 帧；角色每 3 帧换一次表情/姿势。外层 801 停在 774 帧后子剪辑继续循环 | SWF 780 的子符号 767/779/707/724/741/758；各子符号时间轴；sprite 780 的深度与矩阵 | `MutinyFrontendController.DrawEndingShip`、`MutinyEndingSequence.ShipFrameAt` | 从结局页生产时钟推进，核对 1→4→7→10→1 的角色帧、波纹 16 帧循环、所有实例的深度与位置；774 帧后再推进子时钟 | 静态确认；已实现；Play Mode 生产入口与资源/循环断言通过；画面待目视 |
| END-SEQ-06 | 结局对白由嵌套剪辑 787/789/792/794/796 的时间轴遮挡条揭示；关卡内 speech 才由 `SpeechBubble.advance` 每帧追加 3 字并由 `TileSystem.mouseDown` 点击补全。原版结局没有该点击补全文字入口 | 上述五个 sprite 的 `DoAction`/`PlaceObject` 及构造文字；`SpeechBubble.as::advance`、`TileSystem.as::mouseDown`、`Controller.as::endGame` | `MutinyFrontendController.DrawEnding` 保持原版定时揭示；不调用战斗 `SpeechBubble` | 在结局对白显示期间点击画面空白处，帧号和揭示进度仍由时间轴决定；关卡 speech 的原交互另测 | 静态确认；Unity 现状符合；结局点击运行验证待做 |
| END-SEQ-07 | 结局沿用关卡的 `game_music` 播放位置，不切换到菜单曲，也不从头重播；从结局按 back to title 时才切到 `menu_music` | 根帧 121 `startGameMusic()`；`CongratulationsButton.as::onRelease` 只执行 `Controller.endGame()` 与 `gotoAndStop("congratulations")`；根帧 131 只有 `stop()`；MusicController 实例与两条 Sound 宿主跨帧保留；根帧与 sprite 802 可达时间轴无 `StartSound` / 音频流数据；根帧 41 启动菜单曲 | `MutinyFrontendController.ShowEnding` / `ReturnFromEndingToTitle`、`MutinyAudioManager.PlayMusic` | 经生产最终胜利入口进入结局，比较进入前后 `AudioSource.clip` 与播放位置；返回标题检查切换菜单曲；静音时只保留战斗曲逻辑目标 | 静态确认；已实现；Play Mode 曲目切换通过；播放位置及静音场景待运行验证 |

静态资源：船 sprite 780、气泡 sprite 783/shape 790、分数面板 shape 797 均从原 SWF 导出。整帧 PNG 未执行 `DangleFont` 的动态绘制，不能将其当作带对白的最终画面。

已实现：结局页、背景与船叠层、五段对白和逐行揭示、分数面板运动、按钮反馈和回标题；单人最终胜利结算接入该页。`MutinyEndingSequence` 存原版帧号、文字与位置。2026-09-26 将船画面从整张首帧图改为 SWF 的 28 个深度图层，直接引用 91 张原始子帧图片；四个红衣角色实例和一个黑帽角色实例按各自 12 帧循环，五个波纹实例按 16 帧循环。结局入口若已有 `game_music`，保留原 `AudioSource` 不作播放调用；若入口曲目错误，则恢复战斗曲。返回标题时切换 `menu_music`。

实际测试通过：隔离 Unity 6000.6.0f1 Play Mode 调用 `MutinyEndingVerificationMenu.Validate`，2026-09-26 回归 `16/16` 断言通过。用生产 `MutinyTurnManager.AdvanceSimulationTick` 生成第 15 关胜利、`MutinyGameHUD.SynchronizeGameEndPopup` 打开最终弹窗、`CompleteCampaignAndShowEnding` 进入结局；随后用生产结局时钟推进到 774 并额外推进子剪辑，再执行返回标题。覆盖 91 张船部件的资源尺寸、角色/波纹循环、774 帧后子剪辑继续、对白起止帧、逐行进度、面板位置与按钮资格；新增断言确认入场和停帧时均使用 `game_music`，返回标题改用 `menu_music`。船部件与原 SWF 导出物逐个 SHA-256 核对 `91/91` 相同；28 个图层 ID、深度和坐标与 SWF XML 逐项核对相同。结果记录见 [角色动画验证](../../11-VerificationAndDebug/02-PlayModeValidation/ENDING_IDLE_VERIFICATION.md)及[结局音乐验证](../../11-VerificationAndDebug/02-PlayModeValidation/ENDING_MUSIC_VERIFICATION.md)。

待运行验证：与原版 Flash 同步录屏比较逐帧文字遮罩、船图层像素位置、音乐延续及悬停状态；测量结局切换前后的音频采样位置及静音状态；非最终关的禁止进入用例；结局背景点击不跳字的原版运行观察。

已知差异：当前对白以原版 DangleFont 字形作按行线性裁切，原版遮挡条的逐像素边缘尚未做视频对照。结局角色是独立的 12 帧子剪辑，和关卡角色的 35 帧待机/受击时间轴不同。结局点击立即补全文字并非原版行为；若作为扩展添加，应单独登记，不应标为原版一致性。
