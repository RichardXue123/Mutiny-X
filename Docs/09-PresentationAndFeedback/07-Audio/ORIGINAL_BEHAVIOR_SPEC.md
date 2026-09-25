# 原版音频行为规格

## 证据基线

本规格只使用原版 AS2、pcode/SWF XML、声音导出和时间轴证据。Unity 当前 C# 仅用于实现对照，不用于反推原版规则。

主要证据：

- `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/game/MusicController.as`
- `.../com/nitrome/game/SfxManager.as`
- `.../com/nitrome/game/MusicToggle.as`、`FxToggle.as`
- `.../com/nitrome/throwgame/*.as`
- `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml`
- `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/symbols/symbols.csv`
- `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Audio/*.wav`
- `Docs/10-OriginalEvidence/Artifacts/OriginalParityTables/audio-original-calls.csv`

## 播放模型

### AUD-CORE-001 · SFX 总开关

`SfxManager.playSound(id)` 先读取 `MusicController.getSfxOn()`；只有为 `true` 时才对 `sfx_manager[id]` 执行 `gotoAndPlay(2)`。未知 ID 没有后备音、模糊匹配或自动替换。

这意味着实现必须保留原版 ID（包括空格与大小写语义），并在缺失时保持静默且输出可诊断日志。原版 `"cannon explosion"` 含空格，Unity 文件系统名可使用下划线，但需要显式别名。

### AUD-CORE-002 · 重触发与并发

每个 SFX ID 对应 `sfx_manager` 内的一个两帧 MovieClip；第 2 帧包含 `StartSoundTag`，且 `syncNoMultiple=false`。再次调用会把该实例跳回第 2 帧并再次触发声音。不同 ID 可并发；同 ID 也不应被全局去重。

### AUD-CORE-003 · 音量包络

大多数声音以完整增益播放；下表声音在 `StartSoundTag` 上带左右声道包络。数值除以 65535 得到近似 Unity 线性增益：

| ID | 左 | 右 | 近似增益 L/R |
| --- | ---: | ---: | --- |
| `banana_bounce` | 20665 | 21845 | 0.32 / 0.33 |
| `bluePirate` | 23321 | 23026 | 0.36 / 0.35 |
| `cabinBoy` | 13875 | 13580 | 0.21 / 0.21 |
| `crab` | 24502 | 24797 | 0.37 / 0.38 |
| `femalePirate` | 13284 | 12694 | 0.20 / 0.19 |
| `parrot` | 21255 | 20074 | 0.32 / 0.31 |
| `poop1` | 26864 | 26273 | 0.41 / 0.40 |
| `poop2` | 26864 | 25978 | 0.41 / 0.40 |
| `poop3` | 23912 | 25388 | 0.36 / 0.39 |
| `rainbowBeard` | 21550 | 21550 | 0.33 / 0.33 |
| `redPirate` | 23321 | 22731 | 0.36 / 0.35 |
| `soldier` | 11808 | 10627 | 0.18 / 0.16 |

启动动画中的三次 `pop` 另使用 9742/8561（约 0.15/0.13）；它不是普通玩法 `pop` 的全局音量。

## 音乐状态机

### AUD-MUS-001 · 初始化

`MusicController` 创建绑定到 `menu_music_mc`、`game_music_mc` 的两个 `Sound`，分别 `attachSound("menu_music")` 和 `attachSound("game_music")`，然后从 `NitromeGame` 的 SharedObject 读取 `musicon`、`sfxon`。字段不存在时均默认 `true`。

### AUD-MUS-002 · 菜单音乐

`startMenuMusic(from_toggle)` 在当前类型不是 `menu` 或 `from_toggle == true` 时执行：若音乐开启，取消战斗音乐的完成回调、停止战斗音乐、从头启动菜单音乐，并把菜单音乐完成回调设为再次 `start()`；无论音乐是否开启，最后都把 `music_type` 设为 `menu`。

### AUD-MUS-003 · 战斗音乐

`startGameMusic(from_toggle)` 与菜单逻辑对称：停止菜单音乐、从头启动并循环战斗音乐，且无论开关状态都把 `music_type` 设为 `game`。

### AUD-MUS-004 · 关闭与重新开启

关闭音乐会停止两首曲目并持久化 `false`，不是暂停。重新开启时根据 `music_type` 调用对应的 `start*Music(true)`，从曲首播放并持久化 `true`。

主时间轴调用点：frame 40、41、91、101、111 启动菜单音乐；frame 121 在 `startGame` 后启动战斗音乐。

单人结局沿用战斗音乐：`CongratulationsButton.onRelease` 转场后执行 `Controller.endGame()` 与根时间轴 `gotoAndStop("congratulations")`，目标 frame 131 只执行 `stop()`，没有菜单曲调用。frame 40 创建的 MusicController 与两条 Sound 宿主未在 frame 131 移除；结局 sprite 802 及可达时间轴也没有直接 `StartSound`/音频流数据。因此 `game_music` 不被重启，直到结局按钮返回标题的 frame 41 才切换到 `menu_music`。规则见 [END-SEQ-07](../../01-GameFlowAndProgression/03-ResultsAndProgression/ENDING_SEQUENCE.md)。

## SFX 事件规格

### 玩法事件

| 规则 | 声音 | 原版条件与时序 | 原版来源 |
| --- | --- | --- | --- |
| AUD-SFX-001 | 队伍类型语音 | `Team.select(character, again)` 成功选择存活角色且 `again` 为假时播放；ID 来自队伍第一个角色类型并移除 `Captain`/`Chief` | `Team.as:202-215,325-330` |
| AUD-SFX-002 | 队伍类型语音 | 胜负台词 `SpeechBubble.setTarget` 时播放目标角色所属队伍类型；平局弹窗没有此调用 | `Controller.as:139-180`; `SpeechBubble.as:40-46` |
| AUD-SFX-003 | `die` | 角色 `shownHealth < 1`、切换为死亡表现并置 `alive=false` 的同一 tick | `Character.as:155-163` |
| AUD-SFX-004 | `hitwall` | 角色一次接触累计 `contactTime > 5` 时播放，然后把 `contactTime` 清零 | `Character.as:710-719` |
| AUD-SFX-005 | `splash` | `Solid.splashCheck` 检测跨越水面状态时播放；由共同实体逻辑负责，不是爆炸逻辑 | `Solid.as:45-55` |
| AUD-SFX-006 | `anchor` | 锚落下并完成破坏/停止垂直速度的处理后播放一次 | `Anchor.as:89-118` |
| AUD-SFX-007 | `banana_bounce` | 已发射且未结束的香蕉每次 `contact(side)` | `Banana.as:35-41` |
| AUD-SFX-008 | `pop` | 香蕉主动爆炸、创建爆炸并隐藏自身的同一 tick | `Banana.as:107-113` |
| AUD-SFX-009 | `cannon explosion` | 大炮创建并发射炮弹后的同一 tick | `Cannon.as:170-177` |
| AUD-SFX-010 | `pop` | 炮弹与地形 `contact` 后立即播放；炮弹直接命中角色的分支创建爆炸但不播放 | `Cannonball.as:28-44,71-105` |
| AUD-SFX-011 | `pop` | 樱桃炸弹接触并创建爆炸的同一 tick | `CherryBomb.as:25-39` |
| AUD-SFX-012 | `pop` | 已发射炸药静止、创建爆炸的同一 tick；进水熄灭不播放 | `Dynamite.as:32-58` |
| AUD-SFX-013 | `mine_beep` | 地雷激活后 elapsed 达到 0,15,30,38,45,49,53,55,57,59 tick，各播放一次 | `Mine.as:46-53` 及构造时 `beepTimes` |
| AUD-SFX-014 | `pop` | 地雷倒计时结束、创建爆炸的同一 tick | `Mine.as:148-162` |
| AUD-SFX-015 | `pop` | 降落伞炸弹接触并创建爆炸的同一 tick | `ParachuteBomb.as:34-47` |
| AUD-SFX-016 | `fan` | 人类玩家按住风扇输入时，每 `framesFromFire % 12 == 0` 播放 | `ParachuteBomb.as:84-108` |
| AUD-SFX-017 | `pop` | 八枚金币的当前金币以 `explode=true` 结束、创建爆炸的同一 tick | `PiecesOfEight.as:64-82` |
| AUD-SFX-018 | `pop` | 朗姆酒瓶接触、创建爆炸的同一 tick | `RumBottle.as:41-53` |
| AUD-SFX-019 | `poop1..3` | 海鸥成功创建投射物时均匀随机选择 `floor(random*3)+1` | `Seagull.as:98-106` |
| AUD-SFX-020 | `chest_appear` | 宝箱构造、放置到 y=-300 并开始 falling 动画时 | `TreasureChest.as:16-35` |
| AUD-SFX-021 | `click` | 宝箱首次检测到角色、进入 open 动画并设置 10 tick 出货等待时 | `TreasureChest.as:191-220` |
| AUD-SFX-022 | `icon_collect` | 每件武器实际加入角色库存并显示 collected 文本时 | `TreasureChest.as:151-165` |
| AUD-SFX-023 | `voodoo` | 玩家把巫毒目标改为另一角色后；选择同一目标不重播 | `TileSystem.as:692-700` |
| AUD-SFX-024 | `voodoo` | AI 执行巫毒方案、写入速度与目标后的同一 tick | `VoodooDoll.as:91-97` |

### UI 和前导时间轴

| 规则 | 声音 | 原版条件与时序 | 备注 |
| --- | --- | --- | --- |
| AUD-UI-001 | `rollover` 请求 | 音乐/SFX 开关从非悬停进入悬停 | `MusicToggle.as:17-21`; `FxToggle.as:17-21`。`sfx_manager` 无该 ID，因此静态预期为静默 |
| AUD-UI-002 | 无 | 普通菜单按钮、武器按钮、取消按钮只切换画面/状态，没有音频调用 | `buttons/SimpleButton.as`; `game/SimpleButton.as`; `WeaponSelectButton.as` |
| AUD-TL-001 | `ching` | MTV Arcade 前导动画 Symbol 1910 第 33 帧 | SWF XML `StartSoundTag soundId=380` |
| AUD-TL-002 | `pop` ×3 | 前导/品牌动画 Symbol 1922 第 41、44、48 帧；每次使用约 0.15/0.13 包络 | SWF XML `soundId=412` |
| AUD-TL-003 | `nitrome_sound` | Symbol 1922 第 62 帧 | SWF XML `soundId=400` |

## 资源表

原版 `sfx_manager` 的 37 个有效 ID 如下；括号内为 Unity 文件名不同或尚无已证实玩法调用的情况。

| 类别 | ID |
| --- | --- |
| 武器/世界 | `anchor`, `banana_bounce`, `cannon explosion`（Unity 文件为 `cannon_explosion`）, `chest_appear`, `click`, `die`, `fan`, `hitwall`, `icon_collect`, `mine_beep`, `pop`, `splash`, `voodoo`, `poop1`, `poop2`, `poop3` |
| 队伍/角色语音 | `blindPirate`, `bluePirate`, `bossGuy`, `bossGuyZombie`, `cabinBoy`, `crab`, `femalePirate`, `monkey`, `oldPirate`, `parrot`, `rainbowBeard`, `redPirate`, `shark`, `skeletonPirate`, `soldier`, `squid`, `tribe` |
| 前导时间轴 | `ching`, `nitrome_sound` |
| 已导出但调用未证实 | `alternative_bannana`, `smack` |

`rollover` 不在 37 个有效 ID 中。原版旧汇总表只覆盖静态字符串调用，遗漏动态队伍语音、`poop1..3`、开关悬停请求和时间轴直放声音；实现与验收应以本规格补充后的范围为准。
