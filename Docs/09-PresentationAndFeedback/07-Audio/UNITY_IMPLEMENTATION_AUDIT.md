# Unity 音频实现审计

## 当前架构

`MutinyAudioManager` 是 `DontDestroyOnLoad` 单例。若场景中不存在实例，首次读取 `Instance` 会创建 GameObject，并在 `Awake` 中：

1. 从 `MutinySaveSystem` 读取音乐/SFX 开关和音量；
2. 创建一个 SFX `AudioSource` 与一个音乐 `AudioSource`；
3. 用 `Resources.LoadAll<AudioClip>` 加载 `Audio/SFX`、`Audio/Music`；
4. 以不区分大小写的 clip 名字典解析请求；
5. SFX 使用 `PlayOneShot`，音乐使用单一 source + `loop`。

这是可工作的最小架构，但字符串由玩法类分散发出，缺少统一事件目录、别名、缺失资源诊断、原版增益和可测试的音乐逻辑状态。

## 资源完整性

2026-09-19 执行静态核验：

- `Assets/Mutiny/Resources/Audio/SFX/`：37 个 WAV；
- `Assets/Mutiny/Resources/Audio/Music/`：2 个 WAV；
- 全部 39 个文件与 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Audio/` 对应导出逐文件 SHA-256 相同；
- `MutinyParityValidationMenu` 只验证数量 37/2，不能发现 ID 拼写、错误调用时机、重复播放或音量差异。

所有检查过的 `.meta` 使用 AudioImporter、`loadType=DecompressOnLoad`、`preloadAudioData=false`、保留立体声。运行时 `AudioSource` 默认 `spatialBlend=0`，因此当前按 2D 音频播放；建议显式设置，以免预制体/场景序列化值改变行为。

## 生产调用覆盖

### 音乐

- `MutinyFrontendController.Initialize`：菜单音乐。
- `MutinyFrontendController.StartLevel`：战斗音乐。
- `MutinyFrontendController.ReturnToSinglePlayerLevelSelect`：菜单音乐。

`MutinyTurnManager` 只管理回合状态，不再直接选择音乐。此前其 `StartGame` 会在主界面加载期间无条件请求 `game_music`，覆盖前端刚设置的 `menu_music`；该调用已于 2026-09-19 移除，并增加 `AUDIO-MUSIC-T01` 回归。

### 已接线的原版 SFX

- 角色/队伍：选择语音、`die`、`hitwall`、`splash`。
- 武器：锚、香蕉反弹/爆炸、大炮、炮弹、樱桃炸弹、炸药、地雷、降落伞、金币、朗姆酒、海鸥、巫毒。
- 世界：宝箱出现/打开/逐件收集，多种实体过水面。
- UI：角落音乐/SFX/退出控件的 `rollover` 请求（因无资源实际静默）。

## 高优先级缺陷

### 已修复 · 主界面被战斗音乐覆盖

根因是主场景已有可初始化的 `MutinyTurnManager`：`MutinyFrontendController.Initialize` 先请求菜单音乐，随后回合管理器的 `StartGame` 又请求战斗音乐。修复后曲目选择归前端流程所有；只有 `StartLevel` 真正进入 Gameplay 时才切到 `game_music`。

### U-AUD-001 · 大炮音效 ID 不可解析

调用者发送 `"cannon explosion"`，字典键来自 `cannon_explosion.wav` 的 clip 名。当前没有别名或正规化，故 `TryGetValue` 失败且无日志。

建议：建立显式原版 ID → clip key 映射（至少 `"cannon explosion" -> "cannon_explosion"`），不要全局把空格替换为下划线，以免掩盖未来拼写错误。

### U-AUD-002 · `MutinyExplosion` 错误拥有通用 `pop`

原版 `Explosion` 不播放 SFX；具体调用者决定是否、何时播放。当前 `MutinyExplosion.ApplyHit` 默认在视觉第 3 帧播放 `pop`，产生以下差异：

| Unity 入口 | 当前结果 | 原版结果 |
| --- | --- | --- |
| `MutinyCannonball.Explode(playPop:true)` | 调用 tick 播一次，第 3 帧再播一次 | 地形接触 tick 一次 |
| `MutinyCannonball.Explode(playPop:false)` | 第 3 帧仍播一次 | 角色直击静默 |
| `MutinyCherryBomb.Explode` | 第 3 帧一次 | 接触 tick 一次 |
| `MutinyDynamite.Explode` | 第 3 帧一次 | 静止爆炸 tick 一次 |
| `MutinyGunpowderBarrel` | 第 3 帧一次 | 当前 AS2 调用证据未显示 `pop` |

建议：删除/停用 `MutinyExplosion` 的默认声音职责；所有已证实调用者在生产事件 tick 显式发出 `pop`。炮弹必须保留地形接触有声、角色直击无声的分支差异。

### 已修复 · U-AUD-003 静音跨状态后恢复错误曲目

修复前，`PlayMusic` 在 `MusicEnabled == false` 时立即返回，既不换 clip，也不记录逻辑音乐类型。例如战斗音乐播放中关闭音乐、返回菜单、再开启时，Unity 会重播战斗音乐；原版会在静音期间记录 `music_type="menu"`，重新开启时播放菜单音乐。

当前 `PlayMusic` 无论开关状态都会解析并记录目标 clip、循环状态和音量；关闭状态不发声，`ToggleMusic` 重新开启时立即从已记录曲目的开头播放。新增 `AUDIO-MUSIC-T02` 覆盖“战斗曲→关闭→静音时请求菜单曲→按音乐按钮”的生产流程。

### U-AUD-004 · 结算声音不是原版流程

后续处理：`MutinySpeechController` 已接入原版单人对话音效，`MutinyTurnManager` 已移除结算的 `ching/fan/die` 替代调用；以下保留为修复前审计记录。待 Unity Play Mode 验证实际听音。

`MutinyTurnManager.EvaluateTurnOrGameOver` 当前对平局、玩家胜、玩家负分别调用 `die`、`ching`、`fan`。原版 `Controller.nextTurn` 的 1P 胜/负会创建 `SpeechBubble`，而 `SpeechBubble.setTarget` 播放对应队伍语音；平局直接显示失败/平局弹窗，没有上述三项 SFX 调用。

建议：结算表现接入语义事件 `VictorySpeech(team)` / `DefeatSpeech(team)`；在语音气泡或其 Unity 等价表现建立时播放队伍类型声音。删除无证据的三分支替代音效，除非作为用户授权扩展单独登记。

### U-AUD-005 · 武器发射额外 `click`

`MutinyPlayerInput` 与 `MutinyWeaponFactory` 在武器发射后播放 `click`。原版武器选择、瞄准、释放流程没有 `playSound("click")`；静态可见的唯一 `click` 调用是宝箱打开。

建议：从通用发射入口移除。若希望保留现代操作反馈，登记为扩展并使用不同规则 ID，不能计入原版一致性。

### U-AUD-006 · 单音效增益/声道包络未复刻

当前全部 SFX 使用 `SfxVolume * volumeScale`，生产调用几乎都传默认 1。原版 12 个玩法 SFX 具有 0.16–0.41 范围的左右声道包络；Unity 会显著更响，并丢失轻微左右差异。

建议：在事件目录中保存 `gainLeft/gainRight`。若继续用 `AudioSource.PlayOneShot`，先实现等效平均增益；需要严格声道一致时使用双 source、AudioMixer 或预处理后的非破坏性派生 clip。原始 WAV 保持不变。

## 中优先级差异与风险

- `PlaySFX` 对未知 ID 完全静默且不记录；应以限频 warning 输出一次，测试环境可提升为断言。
- `SfxPlayed` 只在成功解析 clip 后触发，适合验证解析结果，但缺少音乐事件、拒绝原因、时间戳、调用上下文和实际播放次数。
- `PlayCharacterVoice(character.CharacterType)` 基于所选角色；原版基于 `team.characters[0]`。同质队伍结果相同，混合角色类型队伍会不同。
- 角色死亡既可在 `MutinyCharacter` 播 `die`，结算又可能额外播 `die`；需要以生产死亡/结算路径验证次数。
- `PlayOneShot` 允许叠加。原版也允许 `syncNoMultiple=false`，但同 ID 快速 `gotoAndPlay(2)` 的实际 Flash 混音仍需运行对照。
- 音量设置（SFX 1.0、音乐 0.8）是 Unity 扩展；原版只有开关，没有用户音量字段。应独立于一致性规格。
- 启动时间轴 1910/1922 没有 Unity 等价流程，因而 `ching`、三次低音量 `pop`、`nitrome_sound` 未播放。
- `alternative_bannana`、`smack` 已导入但未发现原版调用；保持未使用是正确默认。

## 推荐实现形态

建议引入一个不依赖场景的事件目录，而不是继续扩散裸字符串：

```text
玩法/UI 生产事件
  -> AudioEventId（CannonFire、MineBeep、TeamVoice、ChestOpen…）
  -> 原版映射（clip key、gain、是否随机、循环/一次性）
  -> MutinyAudioManager（开关、持久化、播放、诊断）
  -> AudioSource / Mixer
```

最低限度的数据字段：

- 稳定事件 ID；
- 原版声音 ID 与 Unity clip key；
- bus（Music/SFX）；
- gain L/R 或等效 gain；
- 随机集合及选择规则；
- 是否循环；
- 缺失资源策略；
- 原版证据引用。

建议修复顺序：

1. 修正 `cannon explosion` 映射；
2. 把 `pop` 所有权移出 `MutinyExplosion` 并补次数/时序回归；
3. 删除或隔离结算与武器发射的非原版声音；
4. 建立队伍语音映射和音量包络；
5. 接入前导时间轴（若 Unity 产品范围包含该流程）；
6. 增加缺失资源、音乐状态和实际播放诊断。
