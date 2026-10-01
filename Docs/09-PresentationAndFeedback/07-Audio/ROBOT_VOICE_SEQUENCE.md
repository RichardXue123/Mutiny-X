# 单人16机器人语音轮播（2026-10-01）

## XML固定Speech序列：实现前规格

用户授权：敌方开场回应 `speech.robot.1` 固定播放01–04，敌方胜利 `speech.robot.3` 固定播放05–08；每段结束后间隔0.5秒，XML配置可复用于其他关卡，无原版对应。

| ID | 行为 / 生产入口 | 验收 | 状态 |
| --- | --- | --- | --- |
| EXT-SPACE16-VOICE-07 | 可选speechAudio元素配置line、clips、gapSeconds；解析、Build、保存场景Play保留；未配置沿用原声音 | XML验证与实际加载/Scene保存重开 | 通过：配置保存Play、解析及5类非法配置、普通关回归 |
| EXT-SPACE16-VOICE-08 | Speech.StartLine(1/3)分别播放固定四段，替代原语音；不推进普通机器人轮播计数；不因换语言重播 | 自然开场、实际死亡/结算导致敌方胜利、顺序和真实时间，普通选择回归 | 通过：自然开场、生产Drown/PassTurn/11结算tick导致敌胜、时间与游标 |
| EXT-SPACE16-VOICE-09 | Speech优先，取消旧普通机器人待播，播放期间新角色语音排队；跳过台词/台词结束、静音、销毁/切关/重开取消尚未播放的Speech段 | 实际Click、ToggleSFX、GM/Restart、等待中动作 | 通过：优先与排队、跳过、静音、销毁/切关/重开 |

配置的音频不改变对白文字、打字速度或胜负规则；正常音频不足4秒，默认对白停留时间足够。跳过时停止Speech专用AudioSource，不停止其他SFX。固定序列不写入机器人轮播游标。

### 固定Speech修订结果

- **静态确认**：level_1_16两份XML一致，line1配置01–04、line3配置05–08，各gapSeconds=0.5；生成工具同步保留配置。地图80×27、13名角色及布局保持。
- **已实现**：可选XML解析、LevelData/Root序列化与Builder传递；SpeechController按真实对白索引选择覆盖音频；AudioManager独立Speech/Robot语音Source与协程。优先取消旧角色语音，新请求等Speech播放结束；固定Speech不推进游标。点击跳过、台词结束、Initialize/Disable/Destroy、静音取消Speech。
- **实际测试通过**：Unity 6000.6.0f1隔离Play Mode **65/65**。自然开场、保存Scene Play、XML有效/非法配置、实际全队Drown→PassTurn→11次生产结算tick触发敌胜/我胜/平局、固定顺序与约0.5秒空隙、语言切换、普通角色双段/游标、优先/排队、跳过/静音/重开/切关均通过。见[实际记录](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/ROBOT-SPEECH-AUDIO-20261001.txt)；工具 `Tools/RobotSpeechAudioVerification.cs`。隔离工程首次编译发现旧Localization缺少现有TryGetEnglishText；同步主工程现有Localization与文本后通过，未更改主工程本地化代码。
- **待运行验证**：主工程可见PIE实际听感与完整对局；记录验证真实AudioSource提交和时序，不声称已人工听音。结算测试由生产Drown建立死亡条件，并非武器伤害或完整对局验收。
- **已知差异**：用户授权对白音频扩展，无原版对应。普通机器人计数现在仅由角色选择/直接机器人SFX推进，固定Speech不推进。旧版49/43结果为历史基线，本轮65项为当前固定Speech专项。

```xml
<speechAudio line="1" clips="daftpunk_01,daftpunk_02,daftpunk_03,daftpunk_04" gapSeconds="0.5" />
<speechAudio line="3" clips="daftpunk_05,daftpunk_06,daftpunk_07,daftpunk_08" gapSeconds="0.5" />
```

## 双段播放修订：实现前规格

用户本轮授权每次触发连续播放两段。旧版单段49项通过记录保留为历史基线，不代表新时序通过。

| ID | 可观察行为与状态转换 | 来源 / Unity入口 | 验收 | 状态 |
| --- | --- | --- | --- | --- |
| EXT-SPACE16-VOICE-05 | 每次本关机器人语音请求播放连续两段；第一段结束后留0.5秒空隙再播第二段；连续请求排队，保持1,2 / 3,4 / 5,6 / 7,8 / 1,2顺序 | 用户授权扩展，无原版对应；Team.SelectCharacter / PlaySFX → AudioManager | 正式选择和直接SFX触发，观察实际提交顺序与时间；连续请求不串段 | 通过：5次排队选择、直接Robot/Captain触发、10段顺序和实际时间 |
| EXT-SPACE16-VOICE-06 | 等待使用真实时间；关闭SFX、离关或重开取消旧关尚未播放部分；计数仅在实际提交每段后推进，恢复后从未播序号继续，新关从1开始 | 既有计数/隔离规则延伸；ToggleSFX / GM / Restart | 等待中静音、重开、切关、缺资源/Source，其他声音回归 | 通过：timeScale=0、静音/恢复、重开/离关/再入、无有效Source和缺clip、普通SFX与双人关 |

队列只管理本关机器人语音，其他SFX仍立即播放。实际听音与设备响度另待可见PIE验收。

### 双段修订结果

- **静态确认**：沿用8个原WAV，时长约0.534–0.650秒；0.5秒指第一段结束至第二段开始的空隙，按AudioSource pitch折算播放时长。
- **已实现**：AudioManager串行处理本关机器人请求，每次播放两段；每段成功PlayOneShot后推进关卡游标。真实时间等待、静音清队列、旧关待播取消；其他SFX仍走原入口。
- **实际测试通过**：Unity 6000.6.0f1 隔离 batchmode/nographics Play Mode **43/43**。正式SelectCharacter、直接PlaySFX、ToggleSFX、GM/Restart、实际协程和AudioSource播放提交；五组测得空隙约0.500–0.511秒，包含游戏timeScale=0。见[运行记录](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/ROBOT-VOICE-PAIR-20261001.txt)，工具为 `Tools/RobotVoicePairVerification.cs`。
- **待运行验证**：主工程可见PIE实际听音、响度及连续点击体验；本次记录证明播放提交与时间，不声称人工听到声音。
- **已知差异**：用户授权扩展，旧版快速请求并发混音已改为机器人专用串行队列。首轮测试错误地要求切关后全部SFX静默，普通关启动的redPirate语音使该断言失败；修正为只观察daftpunk声音后通过，未改生产行为。首轮记录保留于 `ROBOT-VOICE-PAIR-20261001-FIRST-RUN.txt`。

## 单段历史基线

用户授权扩展，原版来源不适用。用户提供目录：`C:\Users\27487\Videos\Project\daftpunk`，文件为 `daftpunk_01.wav` 至 `daftpunk_08.wav`。原件保留，原样复制到Unity的Audio/SFX资源目录，不再生成或加工音频。

## 实现前行为规格

| ID | 可观察行为与状态转换 | Unity入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- |
| EXT-SPACE16-VOICE-01 | 单人16关的Robot和RobotCaptain语音请求共享该关计数；每次成功提交声音播放按01→08→01循环；Captain与普通机器人不各自计数 | Team.SelectCharacter → Audio.PlayCharacterVoice/PlaySFX | 实际选择交替两种机器人，连续18次观察clip事件为1..8/1..8/1..2 | 通过：18次正式选择、Captain与Robot基础SFX共用序列 |
| EXT-SPACE16-VOICE-02 | 新进关或重开创建新关卡计数，从01开始；计数属于关卡实例，持久音频单例不会保留上一局计数；场景预览进入Play同样从01开始 | Controller.BuildLevel/Restart → LevelRoot | 生产GM进入/重开/离关回来与保存场景Play各第一次为01 | 通过：保存场景Play、GM进入、重开、离关回来、退出卸载 |
| EXT-SPACE16-VOICE-03 | 只有该关机器人语音推进序列；其他角色语音、碰撞、武器、音乐不推进；关闭SFX或无法实际提交播放（缺资源、无有效Source）不推进 | Audio.PlaySFX | 海盗语音和click穿插、生产ToggleSFX静音/恢复、切换普通单/双人地图 | 通过：海盗/click、ToggleSFX、缺少/禁用Source、缺clip、普通单人及双人16均符合 |
| EXT-SPACE16-VOICE-04 | 八个WAV均以原字节导入，保留项目现有音效导入设置；诊断事件提供最终播放clip名 | Resources.LoadAll<AudioClip>/AudioSource.PlayOneShot/SfxPlayed | SHA256相同；Unity成功导入8clip；生产事件名称与顺序一致 | 通过：8/8原字节相同，8clip导入有有效样本，最终clip事件符合序列 |

仅改变现有机器人语音事件的资源选择，不改变语音触发时机，不增加新台词气泡；多个快速请求沿用现有PlayOneShot并发混音。播放计数指合法clip向可用AudioSource提交的次数；设备是否真的发声需要可见窗口听音验收。

## 完成口径

- 静态确认：源目录包含8个编号WAV；现有角色选择入口已调用PlayCharacterVoice，音频管理器将Captain/Chief剥离为基础类型；Speech也直接调用基础类型SFX。
- 已实现：八个WAV与AudioImporter配置导入 `Assets/Mutiny/Resources/Audio/SFX/`；轮播游标位于LevelRoot运行实例；AudioManager将本关Robot/RobotCaptain语音解析到当前编号clip，成功PlayOneShot提交后推进并报告最终clip事件。
- 实际测试通过：2026-10-01 Unity6000.6.0f1隔离工程batchmode/nographics，49/49断言；经过实际Scene保存/重开/Play、GM命令、Team.SelectCharacter、ToggleSFX、AudioSource.PlayOneShot和最终SfxPlayed事件。保留0.5重力和全部敌人Luck50。记录见 [ROBOT-VOICE-SEQUENCE-20261001.txt](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/ROBOT-VOICE-SEQUENCE-20261001.txt)；重跑工具为 `Tools/RobotVoiceSequenceVerification.cs`。
- 待运行验证：可见Unity窗口实际听音、响度与快速连续触发时的混音体验；无图形批处理只证明正式播放提交与clip顺序，不声称已人工听到声音。
- 已知差异：Unity扩展；声音来源为用户文件，未修改原版语音资源，队列不会等待上一段播放结束。

音频来源与逐文件SHA256见 [ROBOT_VOICE_SOURCES.csv](ROBOT_VOICE_SOURCES.csv)。循环计数不写入存档，打开音效开关会从未播放的下一个序号继续。
