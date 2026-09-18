# 音乐状态机

范围：Flash 原版 `MusicController` 的背景音乐状态、页面切换、关卡切换和 Music Toggle 行为。短 SFX、角色语音和武器声音不作为 BGM 状态。

## 原版证据

| ID | 类型 | 原版定位 | 结论 |
| --- | --- | --- | --- |
| MUSIC-S01 | S | `MusicController.as::MusicController` | 原版只创建两条循环音乐：`menu_music` 与 `game_music`。没有第三个 completion/victory music state。 |
| MUSIC-S02 | S | `MusicController.as::startMenuMusic` | 进入 menu state 时停止 game sound 并从头播放 menu sound；播放完成后自行重播。若已经是 menu 且不是 toggle 恢复，则不重新开始。 |
| MUSIC-S03 | S | `MusicController.as::startGameMusic` | 进入 game state 时停止 menu sound 并从头播放 game sound；播放完成后自行重播。若已经是 game 且不是 toggle 恢复，则不重新开始。 |
| MUSIC-S04 | S | 根时间轴 frame 40/41、91、101、111 | 标题页、人数选择、单人选关以及双人结果页进入/保持 menu music。 |
| MUSIC-S05 | S | 根时间轴 frame 121 | 实际战斗入口在 `Controller.startGame` 后调用 `startGameMusic()`。 |
| MUSIC-S06 | S | `NextLevelButton.as`、`RestartLevelButton.as`、`Controller.changeLevel` | Next level / Restart 只换关卡，不调用音乐切换；因此 game music 连续播放，不因每关重启。 |
| MUSIC-S07 | S | `Controller.nextTurn/enterFrame`、`popup` 流程 | 单人胜利/失败 popup 仍在 gameplay 时间轴，没有调用第三首 BGM；game music 继续。结果 speech bubble 的声音属于 SFX/角色语音。 |
| MUSIC-S08 | S | `CongratulationsButton.as`、根时间轴 frame 131 | 最终 Congratulations 只 `endGame` 并跳到 congratulations 帧；frame 131 只有 `stop()`，没有新的 music state。 |
| MUSIC-S09 | S | `MusicController.as::turnOffMusic/turnOnMusic` | 关闭音乐会停止两条 Sound，但保留 `music_type`；重新开启会按当前 menu/game 类型从头重新播放。 |
| MUSIC-A01 | A | `Assets/Mutiny/Resources/Audio/Music` | Unity 运行时也只有 `menu_music.wav` 与 `game_music.wav` 两个 BGM 资源；`ching.wav`、`fan.wav` 等位于 SFX。 |

## 状态转换

| 当前状态 | 事件 | 下一状态 | 播放结果 |
| --- | --- | --- | --- |
| None | 进入标题/菜单 | Menu | music on 时从头播放 `menu_music` |
| Menu | 菜单页之间跳转 | Menu | 不重启，继续当前 loop |
| Menu | 进入战斗 | Game | 停 menu，从头播放 `game_music` |
| Game | Next level / Restart | Game | 不重启，继续当前 loop |
| Game | 单人胜利/失败 popup | Game | 不切歌；继续 game BGM |
| Game | Back to menu / Level Select | Menu | 停 game，从头播放 menu BGM |
| Menu/Game | Music Off | 状态保持 | 停止可听音乐 |
| Menu/Game（静音） | 页面/战斗状态变化 | 对应新状态 | 不播放，但必须更新 state |
| Menu/Game（静音） | Music On | 状态保持 | 从头播放当前 state 对应 BGM |

## Unity 实现

生产入口：`MutinyAudioManager`。

- `StartMenuMusic()` / `StartGameMusic()` 对应原版两个显式方法。
- `CurrentMusicState` 对应原版 `music_type`，即使 `MusicEnabled == false` 也继续更新。
- 同一状态的普通调用不会重启 BGM；Music Toggle 从 off → on 使用 `fromToggle` 语义从头恢复当前曲目。
- `MutinyFrontendController` 负责前端 Menu/Game 边界；`MutinyTurnManager.StartGame` 只确保进入 Game 状态，同一 game state 不会在换关时重播。
- GameOver 不创建“第三首通关音乐”。之前 Unity 在胜负处直接播放 `ching` / `fan` 的逻辑没有对应的原版 MusicController 状态，其中 `fan` 本身是 Parachute Bomb SFX，因此已移除。原版结果 speech bubble 角色语音仍属于单独的一致性任务。

## 验证

| ID | 用例 | 预期 |
| --- | --- | --- |
| MUSIC-T01 | Menu 播放中关闭 Music → 静音状态进入 Game → 再开启 Music | state 在静音时变为 Game；重新开启后解析到 `game_music`，不能恢复旧 menu clip |
| MUSIC-T02 | Game 中加载下一关或 Restart | state 仍为 Game，普通 StartGameMusic 不重启正在 loop 的 game track |
| MUSIC-T03 | Game 中回到单人选关 | 切为 Menu 并从头播放 `menu_music` |
| MUSIC-T04 | 单人胜/负进入结果 popup | state 保持 Game；没有 completion BGM 切换 |
| MUSIC-T05 | 同一 menu 页面族之间切换 | state 保持 Menu；不会因为重复调用而从头重播 |

`MUSIC-T01` 已并入 `MutinyTurnActionUiVerificationTest` 的 Music Toggle 生产验证。其余播放位置以对应前端/结算流程的 Play Mode 对照继续验收。

## 关于“第三首音乐”

静态 SWF 证据和当前导出的运行时 Music 资源都只显示两条 BGM。若原版实际运行录制中能听到一段独立的“通关音乐”，应先记录触发时刻并确认它来自 SFX MovieClip、时间轴声音或外部资源，再把它作为新的证据项加入；在此之前不把某个短 SFX 人为升级成第三个 BGM state。
