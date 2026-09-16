# 角色基础时间轴与重启后闪烁规格

基线：`Mutiny Source/mutiny-flash-game/mutiny.swf`。本规格处理运行时重建关卡后角色周期性消失的问题，范围为角色 `static` 与 `hit` 两段基础时间轴。

## 原版证据

| ID | 类型 | 来源 | 结论 |
| --- | --- | --- | --- |
| CHAR-TL-E01 | A | `Docs/ReverseEngineering/Art/frame-labels.csv` | 全部 27 个角色 symbol 的 `static` 标签位于第 1 帧，`hit` 标签位于第 15 帧。 |
| CHAR-TL-E02 | A/S | `DefineSprite_1324_redPirate/frame_13/DoAction.as` 及其余角色同构脚本 | 进入第 13 帧时立即执行 `this.gotoAndPlay("static")`；第 13 帧不作为一张可见 idle 帧停留。 |
| CHAR-TL-E03 | A/S | `DefineSprite_1324_redPirate/frame_35/DoAction.as` 及其余角色同构脚本 | 进入第 35 帧时立即执行 `this.gotoAndPlay("static")`；受击可见序列结束后回到第 1 帧。 |
| CHAR-TL-E04 | A | `Docs/ReverseEngineering/Art/placements.csv:1337-1342` | redPirate 的可见静态姿势在第 1、4、7、10 帧切换；第 15 帧替换为 hit 子 symbol。 |
| CHAR-TL-E05 | A | 27 个 `Assets/Mutiny/Resources/Art/Characters/Animations/*/13.png` 与 `14.png` | 所有角色的第 13、14 张导出图 alpha 像素数均为 0，不能当作角色可见姿势播放。 |
| CHAR-TL-U01 | U | 用户 2026-09-16 运行反馈 | 点击 Restart Level 后，每个角色都会持续闪烁。 |
| CHAR-IDLE-E01 | A | `Docs/ReverseEngineering/Art/placements.csv` 中 27 个角色 symbol | 所有角色都在第 1、4、7、10 帧替换静态姿势子 symbol，因此一个 static 循环由四个三 tick 段组成。 |
| CHAR-IDLE-E02 | A | 27 组 `Animations/<type>/1.png`–`12.png` 的 SHA-256 检查 | 每个角色的 1–3、4–6、7–9、10–12 帧分别完全相同，且四组中至少存在两个不同画面；导出资源保留了原版的三 tick 保持与弹动姿势。 |
| CHAR-IDLE-U01 | U | 用户 2026-09-16 运行反馈 | 游玩时角色应持续播放“一弹一弹”的 idle 动画。 |

## 行为规则

| ID | 前置与事件 | 可观察结果 | Unity 入口 |
| --- | --- | --- | --- |
| CHAR-TL-01 | 角色处于普通 `static` 动画 | 25 Hz 播放可见帧 1–12；下一 tick 直接回到第 1 帧，永不把透明的第 13、14 帧提交给渲染器 | `MutinyCharacterAnimator.AdvanceOriginalTick` |
| CHAR-TL-02 | 角色收到 hit 且仍在运动 | 每 tick 重置并保持第 15 帧，等价于原版持续调用 `gotoAndPlay("hit")` | `MutinyCharacterAnimator.PlayHit` / `AdvanceOriginalTick` |
| CHAR-TL-03 | hit 角色停止运动 | 从第 15 帧之后继续播放至可见第 34 帧；到达原版第 35 帧动作时直接回到第 1 帧 | `MutinyCharacterAnimator.AdvanceOriginalTick` |
| CHAR-TL-04 | 初始烘焙关卡或 Restart 运行时重建关卡 | 两条创建路径使用同一动画状态范围；重建后不得出现透明帧造成的周期性闪烁 | `MutinyLevelBuilder.BuildCharacters` → `MutinyCharacterAnimator.Initialize` |
| CHAR-IDLE-01 | 存活角色处于普通状态 | 以 25 Hz 顺序播放 1–12：姿势 A 保持 1–3，B 保持 4–6，C 保持 7–9，D 保持 10–12，随后循环；形成原版的弹动节奏 | `MutinyCharacterAnimator.Update` → `AdvanceOriginalTick` |
| CHAR-IDLE-02 | 首次进入带烘焙角色的 Main 场景 | Animator 在 `Start` 从同对象序列化的 `CharacterType` 自动加载对应角色的 35 帧资源并从 static 第 1 帧开始 | `MutinyCharacterAnimator.Start` → `EnsureInitialized` |
| CHAR-IDLE-03 | Restart 通过 Builder 创建角色 | Builder 立即用同一 `CharacterType` 初始化；后续 `Start` 检测已初始化，不重置动画相位或重复加载 | `MutinyLevelBuilder.BuildCharacters`、`MutinyCharacterAnimator.EnsureInitialized` |

## 回归用例

| ID | 操作与断言 | 状态 |
| --- | --- | --- |
| CHAR-TL-TC01 | 初始化真实 `MutinyCharacterAnimator`，驱动 12 个生产动画 tick；序列从 1 到 12 后直接回 1，永不返回 13 或 14 | 待 Unity 自动验证 |
| CHAR-TL-TC02 | 调用真实 `PlayHit`，在静止条件下驱动至第 34 帧；下一 tick 直接回 1，永不提交第 35 帧动作帧 | 待 Unity 自动验证 |
| CHAR-TL-TC03 | Play Mode 点击一次 Restart Level，观察全部角色至少 10 秒；角色持续可见，Hierarchy 中只有一个活动关卡根 | 待用户手动验证 |
| CHAR-IDLE-TC01 | 创建具有序列化 `CharacterType` 的角色和 Animator，不手动调用 `Initialize`；经生产 `EnsureInitialized` 后必须加载对应帧并停在第 1 帧 | 待 Unity 自动验证 |
| CHAR-IDLE-TC02 | 驱动生产动画 tick；第 3、6、9 个 tick 后当前帧依次为 4、7、10，第 12 个 tick 后回到 1 | 待 Unity 自动验证 |
| CHAR-IDLE-TC03 | 首次进入 Main 和点击 Restart 后分别观察至少两个循环；角色均以相同四段节奏弹动，无透明闪烁 | 待用户手动验证 |

## 已知范围

- 当前资源是 SWF 父时间轴的合成帧。逐角色内部嵌套 MovieClip 的独立播放相位仍属于 P5-02 / P5-03 后续工作。
- 本修复只确认并恢复 `static`/`hit` 父时间轴的跳转边界；不据此宣称全部角色姿势动画已经完成原版对照。
