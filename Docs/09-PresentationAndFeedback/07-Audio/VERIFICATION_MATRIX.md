# 音频规则与验收矩阵

## 状态定义

- **静态确认**：AS2、pcode、SWF XML、资源或 Unity 源码足以确认。
- **已实现**：生产入口存在，但不代表运行结果正确。
- **实际测试通过**：通过真实生产入口执行并记录了结果。
- **待运行验证**：规格/入口已知，但本轮未运行 Unity 或原版。
- **已知差异**：当前 Unity 行为与原版证据不一致。

## 核心与音乐

| 规则 | 原版来源 | Unity 入口 | 验收用例 | 实际结果（2026-09-24） |
| --- | --- | --- | --- | --- |
| AUD-CORE-001 未知/关闭 SFX 静默 | `SfxManager.as:9-15` | `MutinyAudioManager.PlaySFX` | 从生产事件请求有效/无效 ID；关闭 SFX 后重试；核对 source 与诊断 | 已实现开关与静默；未知 ID 无诊断；待运行验证 |
| AUD-CORE-002 同/异 ID 可重触发 | SWF XML SFX wrappers, `syncNoMultiple=false` | `AudioSource.PlayOneShot` | 同 tick 连播相同与不同 ID，录制输出/voice 数 | 静态实现可能支持；待运行验证 |
| AUD-CORE-003 原版增益包络 | SWF XML `SOUNDENVELOPE` | 当前只有 `volumeScale` | 对 12 个带包络 SFX 测左右 RMS/峰值 | 已知差异，未实现映射 |
| AUD-MUS-001 默认与持久化 | `MusicController.as:11-19`; `NitromeGame.as:482-534` | `Awake`; `MutinySaveSystem` | 清空设置启动；分别切换并重启 | 已实现；现有测试修改字段验证持久化，未做完整重启听音 |
| AUD-MUS-002 菜单/战斗切换与循环 | `MusicController.as:21-56`; root frames 40/41/91/101/111/121 | Frontend、`PlayMusic` | 标题→关卡→菜单；回合管理器后台初始化不得覆盖主界面曲目；每首播至结尾 | 已修复主界面覆盖问题并新增 `AUDIO-MUSIC-T01`；完整循环仍待运行验证 |
| AUD-MUS-003 关闭后从头恢复当前逻辑曲目 | `MusicController.as:63-92` | `PlayMusic`; `MutinyGameHUD.ToggleCornerMusic`; `ToggleMusic` | 战斗中关闭→静音状态请求菜单曲→按音乐按钮，应立即从菜单曲首播放 | 已修复并新增 `AUDIO-MUSIC-T02`；代码编译通过，Unity Play Mode 听音待运行 |
| AUD-MUS-004 SFX/音乐开关互不影响 | `MusicController.as:94-119` | `ToggleSFX`; `ToggleMusic` | 分别切换并触发另一 bus | 已实现；待运行听音 |

## 玩法 SFX

| 规则 | Unity 入口 | 验收重点 | 状态 |
| --- | --- | --- | --- |
| AUD-SFX-001 角色选择队伍语音 | `MutinyTeam.SelectCharacter` → `PlayCharacterVoice` | 每次新选择一次；继续同角色的阶段切换不重播；ID 应取队伍类型 | 已实现但取所选角色类型；同质队伍静态等价，混合队伍已知风险 |
| AUD-SFX-002 胜负台词队伍语音 | `MutinySpeechController.StartLine` 使用队伍首名角色类型播放语音；`MutinyTurnManager` 不再以 `ching/fan/die` 代替 | 1P 胜/负各一次正确队伍语音；平局无替代音 | 已接入；待 Unity 运行听音 |
| AUD-SFX-003 死亡 | `MutinyCharacter` | 生命表现降到 0 的状态转换只播一次 | 已实现；资源测试存在，不等于完整生产次数通过 |
| AUD-SFX-004 强碰撞 | `MutinyCharacter` | `contactTime > 5` 播；伤害 API 本身不播 | 现有生产入口回归已记录通过；本轮未重跑 |
| AUD-SFX-005 过水面 | Character、Weapon、Boulder、PiecesOfEight、ParachuteBomb、TidalWave 等 | 每次跨越只播一次；不因停留水中连播 | 分散实现；部分现有回归覆盖，仍需全实体运行矩阵 |
| AUD-SFX-006 锚 | `MutinyAnchor` | 锚落地/完成冲击 tick 一次 | 已实现；待运行验证 |
| AUD-SFX-007 香蕉反弹 | `MutinyBanana.OnContact` | 地/墙/顶每次有效接触一次 | 已实现；现有资源/入口回归覆盖，待听音 |
| AUD-SFX-008 香蕉爆炸 | `MutinyBanana.Explode` | 同 tick 一次，Explosion 第 3 帧不重播 | 已实现结构正确；待运行验证 |
| AUD-SFX-009 大炮开火 | `MutinyCannon` | 发射炮弹同 tick 一次 | 已知差异：ID 无法解析，当前静默 |
| AUD-SFX-010 炮弹 | `MutinyCannonball.Explode`; `MutinyExplosion.ApplyHit` | 地形命中一次；角色直击零次 | 已知差异：地形双响、角色直击误响 |
| AUD-SFX-011 樱桃炸弹 | `MutinyCherryBomb`; `MutinyExplosion` | 接触 tick 一次 | 已知差异：晚到爆炸动画第 3 帧 |
| AUD-SFX-012 炸药 | `MutinyDynamite`; `MutinyExplosion` | 静止爆炸 tick 一次；入水零次 | 已知差异：有声爆炸晚两帧；待验证入水 |
| AUD-SFX-013 地雷蜂鸣 | `MutinyMine.AdvanceOriginalTick` | 激活后恰好 10 次，elapsed=0/15/30/38/45/49/53/55/57/59 | 已实现；待生产入口计数验证 |
| AUD-SFX-014 地雷爆炸 | `MutinyMine.Explode` | 倒计时结束 tick 一次 | 已实现结构正确；待运行验证 |
| AUD-SFX-015 降落伞爆炸 | `MutinyParachuteBomb.Explode` | 接触 tick 一次 | 已实现结构正确；待运行验证 |
| AUD-SFX-016 降落伞风扇 | `MutinyParachuteBomb.ApplyFanInput` | 人类按住时每 12 原版 tick；AI/未按住为零 | 已实现；待运行计数验证 |
| AUD-SFX-017 八枚金币 | `MutinyPiecesOfEight.ResolveCoin` | 每枚爆炸 coin 同 tick一次；落水 coin 不播 pop | 已实现结构正确；待运行验证 |
| AUD-SFX-018 朗姆酒瓶 | `MutinyRumBottle.Explode` | 接触 tick 一次 | 已实现结构正确；待运行验证 |
| AUD-SFX-019 海鸥 | `MutinySeagull.RequestShot` | 每次成功投弹恰好一个 `poop1..3`；无效请求不播 | 已实现；现有测试仅检查三资源存在，待统计分布/次数 |
| AUD-SFX-020 宝箱出现 | `MutinyTreasureChest` | 构造进入 falling 时一次 | 已实现；待运行验证 |
| AUD-SFX-021 宝箱打开 | `MutinyTreasureChest` | 首次角色接触时一次 | 已实现；待运行验证 |
| AUD-SFX-022 宝箱收集 | `MutinyTreasureChest` | 每件实际入库存武器一次 | 已实现；待多物品宝箱计数验证 |
| AUD-SFX-023 玩家巫毒改目标 | `MutinyPlayerInput.TrySelectVoodooTarget` | 改为不同目标时一次；重复同目标不播 | 已实现；现有入口测试覆盖，待完整输入运行 |
| AUD-SFX-024 AI 巫毒执行 | `MutinyVoodooDoll` | 写入 AI 方案后一次 | 已实现；待运行验证 |

## UI、时间轴与非原版扩展

| 规则 | Unity 入口 | 验收重点 | 状态 |
| --- | --- | --- | --- |
| AUD-UI-001 开关悬停静默请求 | `MutinyGameHUD.UpdateCornerHover` | 首次进入悬停只请求一次；因无 `rollover` clip 实际无声 | 行为静态等价，但缺失 ID 无诊断；待运行验证 |
| AUD-UI-002 普通按钮/武器释放不播 click | `MutinyPlayerInput`; `MutinyWeaponFactory` | 菜单按钮、武器选择与释放均零次；宝箱打开除外 | 已知差异：武器释放额外 click |
| AUD-TL-001 MTV Arcade ching | 无 | Symbol 1910 第 33 帧 | 未实现；是否纳入 Unity 产品流程待确认 |
| AUD-TL-002 前导三次低音量 pop | 无 | Symbol 1922 第 41/44/48 帧及专属包络 | 未实现；待确认范围 |
| AUD-TL-003 Nitrome 声音 | 无 | Symbol 1922 第 62 帧 | 未实现；待确认范围 |
| AUD-EXT-001 用户音量 | `SetSfxVolume`; `SetMusicVolume` | 0..1 clamp、持久化、实时应用 | Unity 扩展；SFX 新值影响后续 one-shot，音乐实时更新；待运行验证 |

## 必须新增的缺陷回归

每次修复下列问题时，应把用例接入生产入口，而不是直接改管理器字段来伪造结果：

1. **CannonFireResolvesOriginalId**：调用实际 `MutinyCannon.Fire`，监听成功解析事件，必须得到一次原版 ID `cannon explosion`/目标 clip `cannon_explosion`。
2. **ExplosionCallerOwnsPopTiming**：分别驱动炮弹地形、炮弹角色、樱桃炸弹、炸药、地雷、香蕉、金币、酒瓶、降落伞、火药桶；核对每条分支的次数与事件 tick。
3. **MutedMusicTracksLogicalState**：战斗中关闭音乐，通过真实返回菜单入口，再开启；必须播放 `menu_music` 且 `time` 从曲首开始。
4. **GameOverUsesTeamVoice**：用实际回合结算产生胜、负、平；核对队伍声音与静默分支，不接受直接设置 `GameResult`。
5. **WeaponLaunchHasNoClick**：通过玩家和 AI 的生产发射入口，确保普通武器释放不产生 `click`；宝箱打开仍产生一次。
6. **OriginalEnvelopeApplied**：抽查 `soldier`、`banana_bounce`、`poop1` 与无包络 `pop`，验证事件增益不同且原始 WAV 未修改。

现有缺陷回归：

- **AUDIO-MUSIC-T01**：先通过音频生产入口选择 `menu_music`，再执行真实 `MutinyTurnManager.Initialize/StartGame`；断言当前 clip 仍为 `menu_music`。该用例能直接检测本次主界面被战斗音乐覆盖的缺陷。
- **AUDIO-MUSIC-T02**：通过生产入口播放战斗曲并关闭音乐，静音期间请求菜单曲，再调用 HUD 音乐按钮的 `ToggleCornerMusic`；断言目标 clip 已在静音时更新，且运行态按键打开后立即进入播放。

## 本轮执行记录

| 检查 | 方法 | 实际结果 |
| --- | --- | --- |
| SFX 数量 | 枚举 `Assets/Mutiny/Resources/Audio/SFX/*.wav` | 37，符合原版 `sfx_manager` 有效资源数 |
| 音乐数量 | 枚举 `Assets/Mutiny/Resources/Audio/Music/*.wav` | 2，`menu_music`、`game_music` |
| 文件内容一致性 | 对 37+2 个 Unity WAV 与原始导出执行 SHA-256 | 39/39 完全一致 |
| 原版 AS2 调用扫描 | 扫描全部 `playSound(...)`、动态队伍 ID、随机 poop、音乐控制器 | 已完成静态确认；发现旧 CSV 漏项 |
| SWF 时间轴扫描 | 解析全部 `StartSoundTag` 的父 sprite、帧与包络 | 已完成静态确认；额外确认 Symbol 1910/1922 直放声音 |
| Unity 调用扫描 | 扫描全部 `PlaySFX`、`PlayCharacterVoice`、`PlayMusic` | 已完成静态审计；确认本文列出的差异 |
| 本次音乐修复编译 | `dotnet build Assembly-CSharp.csproj --no-restore` | 通过，0 error；仅保留既有 `MutinyLevelTest.levelXml` CS0649 warning |
| Unity Play Mode | 未执行 | 不得标记听音、时序或次数为通过 |
| 原版运行对照 | 未执行 | `syncNoMultiple` 的实际重叠听感及启动流程仍待运行确认 |
