# 09.07 · 音频

[返回上级模块](../README.md)

## 职责

管理菜单/战斗音乐、SFX、队伍语音、音乐与音效开关、持久化设置，以及游戏事件到声音资源的映射。音频层只消费已经发生的玩法/UI 事件，不参与伤害、行动资格、回合推进或胜负判定。

## 文档入口

- [原版行为规格](ORIGINAL_BEHAVIOR_SPEC.md)：原版播放器、音乐状态机、SFX 调用时机、时间轴直放声音与音量包络。
- [Unity 实现审计](UNITY_IMPLEMENTATION_AUDIT.md)：当前架构、资源映射、生产调用入口、已知差异和建议实现顺序。
- [规则与验收矩阵](VERIFICATION_MATRIX.md)：稳定规则 ID、原版来源、Unity 入口、验收步骤和截至 2026-09-19 的实际结果。

## 当前结论

### 静态确认

- 原版共有 37 个 SFX 声音资源和 2 首音乐；Unity 中对应 WAV 均已存在，逐文件 SHA-256 与原始导出一致。
- 原版音乐只有 `menu_music`、`game_music` 两种逻辑状态；曲目播放完毕后从头重播，切换曲目时停止另一首。
- 原版音乐/SFX 开关默认开启并分别持久化。重新开启音乐时，从当前逻辑曲目的开头重新播放，而不是从暂停点续播。
- 原版 `Explosion` 本身不播放声音；需要 `pop` 的武器在各自的生产事件中显式调用。不能把爆炸画面第 3 帧当成统一音效触发点。
- 原版队伍语音使用队伍第一个角色的类型，去掉 `Captain`/`Chief` 后作为声音 ID；角色选择和胜负台词气泡各有独立触发点。
- 原版 UI 的音乐/SFX 开关悬停会请求 `rollover`，但原版 `sfx_manager` 中没有同名实例或资源；现有证据下应视为原版静默调用，不应另配一个近似音效。

### 已实现

- `MutinyAudioManager` 单例、两个 `AudioSource`、资源加载、SFX 一次性播放、音乐循环、开关和 PlayerPrefs 持久化。
- 音乐关闭期间仍记录菜单/战斗目标曲目；重新打开音乐时立即从当前逻辑曲目的开头播放，不再等待下一次 `PlayMusic` 请求。
- 菜单/战斗曲目切换、角落音乐/SFX 按钮状态、队伍选择语音，以及多数武器、碰撞、落水、宝箱和环境事件的调用入口。
- `MutinyFrontendController` 是曲目状态的唯一生产入口：主界面请求 `menu_music`，实际进入关卡后请求 `game_music`；后台初始化的回合管理器不再覆盖主界面曲目。
- 37 个 SFX 与 2 首音乐的 Unity 资源导入。
- 单人模式开场两句与胜方结算台词在气泡创建时各播放一次所属队伍类型语音；平局不播结算替代音。

### 实际测试通过

- 本轮执行了资源清单和 SHA-256 对照：37/37 SFX、2/2 音乐与 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Audio/` 中的原始导出完全一致。
- 本轮未启动 Unity Play Mode，因此没有新增“实际听到声音/听不到声音”的通过项。

### 待运行验证

- 双曲目在菜单、关卡、静音跨场景切换后的实际恢复行为。
- 同帧/连续触发相同 SFX 时的混音、重触发和音量表现。
- 角色/队伍语音、地雷十次蜂鸣、降落伞风扇每 12 tick、海鸥三选一随机音效。
- 所有 `pop` 事件是否只播放一次且发生在原版调用 tick。

### 已知差异

- `PlaySFX("cannon explosion")` 无法命中 Unity 资源 `cannon_explosion.wav`，开炮声当前静默。
- `MutinyExplosion` 默认在动画第 3 帧播放 `pop`，仍可能造成其他武器的重复、误触发或晚两帧；大炮炮弹已在创建爆炸时关闭这一额外声音，按原版分别保持地形接触一次、直接撞人零次 `pop`。
- Unity 武器发射流程额外播放 `click`；原版 `click` 的唯一 AS2 调用点是宝箱打开。
- 原版 12 个 SFX 使用时间轴左右声道音量包络，Unity 当前以统一 `SfxVolume` 播放，未复刻单音效增益/声道差异。
- 原版启动时间轴（MTV Arcade/Nitrome 前导动画）的 `ching`、`pop`、`nitrome_sound` 尚未接入 Unity 前端流程。

## 当前生产入口

- `Assets/Mutiny/Scripts/Presentation/MutinyAudioManager.cs`
- `Assets/Mutiny/Scripts/Presentation/MutinyFrontendController.cs`
- `Assets/Mutiny/Scripts/Presentation/MutinyGameHUD.cs`
- `Assets/Mutiny/Scripts/Persistence/MutinySaveSystem.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyExplosion.cs`
- 各武器、角色、队伍和环境对象中的 `PlaySFX` / `PlayCharacterVoice` 调用点

## 边界与实现原则

1. 玩法对象在原版事件实际发生的 tick 发出语义化音频事件；音频管理器负责 ID 解析、开关、音量和播放，不反向改变玩法状态。
2. 原版同一资源在不同上下文使用不同音量时，增益属于“事件映射”，不能修改原始 WAV 来掩盖差异。
3. `Resources.LoadAll` 成功只证明资源可解析；只有生产入口触发、监听到预期 clip、并确认次数/时序后才算行为通过。
4. 原版未调用的声音不因资源存在而自动使用。`alternative_bannana`、`smack` 等资源目前只登记，不推测用途。
5. 用户授权的现代扩展（主音量、混音器、淡入淡出、并发限制、可访问性设置）必须与原版一致性规则分开登记。
