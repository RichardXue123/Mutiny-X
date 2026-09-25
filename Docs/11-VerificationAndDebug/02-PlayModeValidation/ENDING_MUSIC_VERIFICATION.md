# 结局音乐验证（2026-09-26）

## 静态确认

- 原版根帧 121 的 `DoAction.as` 在进入战斗后调用 `startGameMusic()`；`MusicController.as` 只绑定 `menu_music` 和 `game_music` 两首循环曲。
- `CongratulationsButton.as` 在最终胜利后调用 `Controller.endGame()` 并跳到 `congratulations`；目标根帧 131 只执行 `stop()`。结局时间轴没有新的音乐调用或直接音频标签，MusicController 及 Sound 宿主仍保留。因此结局继续播放原有 `game_music`；返回标题页的根帧 41 才启动 `menu_music`。
- Unity 的 `game_music.wav`、`menu_music.wav` 分别与原版导出的 `438_game_music.wav`、`436_menu_music.wav` 逐文件 SHA-256 相同。原版来源见 [音频行为规格](../../09-PresentationAndFeedback/07-Audio/ORIGINAL_BEHAVIOR_SPEC.md)和 [END-SEQ-07](../../01-GameFlowAndProgression/03-ResultsAndProgression/ENDING_SEQUENCE.md)。

## 已实现

`MutinyFrontendController.ShowEnding` 在当前曲目已经是 `game_music` 时不调用音频管理器，保留同一个 `AudioSource` 和播放状态；若异常入口持有其他曲目，则恢复 `game_music`。`ReturnFromEndingToTitle` 继续调用 `PlayMusic("menu_music")`。

## 实际测试通过

- Unity 6000.6.0f1 隔离工程，Windows batch Play Mode，执行 `Mutiny.Verification.Editor.MutinyEndingVerificationMenu.Validate`，结果为 `Ending sequence Play Mode verification passed: 16/16 assertions.`。
- 用 `MutinyTurnManager.AdvanceSimulationTick` 走到单人第 15 关生产胜利结算，经 `MutinyGameHUD.SynchronizeGameEndPopup` 和 `CompleteCampaignAndShowEnding` 进入结局。新增 `END-SEQ-07` 断言验证入场及结局帧 774 时 `AudioSource.clip` 都是 `game_music`，返回标题后变为 `menu_music`。
- 隔离工程日志：`D:/My Project/Mutiny X/_codex_ending_music_verify_20260926/ending-music-verification-complete.log`。这项测试验证实际生产结算入口和曲目切换，没有通过录音核对人耳听感。

## 待运行验证

- 在带音频输出的实际游戏中记录结局切换前后的 `AudioSource.timeSamples`，确认无采样位置跳变，并与原版 Flash 实机听感对照。
- 音乐开关关闭时从战斗进入结局、在结局重新打开，以及返回标题后的曲目行为。

## 已知差异

当前没有确认音乐资源或结局切换逻辑的差异；播放位置与实际听感仍按上节待验证，不记为通过。
