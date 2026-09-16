# 角色 Throw Self 音效

范围：玩家与 AI 提交 Throw Self（角色自身跳跃）当 tick 的直接音效；不改变落地、撞墙、落水或实际空投箱触碰后的声音。

## 证据

| ID | 类型 | 原版来源 | 结论 |
|---|---|---|---|
| CHAR-THROW-AUD-E01 | S | `throwgame/Character.as:742-746` | `Character.twang()` 只调用 `super.twang()`、标记 `thrown=true` 并消耗 `canThrow`；没有 `playSound`。 |
| CHAR-THROW-AUD-E02 | S | `throwgame/Solid.as:60-74` | `Solid.twang()` 只计算并截断初速度、清理瞄准元素；没有 `playSound`。 |
| CHAR-THROW-AUD-E03 | S | `throwgame/TreasureChest.as:151-164, 181-219` | `icon_collect` 只在实际移除一件箱内武器并加入角色库存时播放；`click` 只在已落地箱子首次与合资格角色相交、箱子打开时播放。 |
| CHAR-THROW-AUD-E04 | U | 用户报告 | 玩家和 AI Throw Self 开始时，即使没有空投，仍听到疑似空投拾取的声音。 |

## 规则

| ID | 前置与触发 | 原版可观察结果 | Unity 入口 | 验收 |
|---|---|---|---|---|
| CHAR-THROW-AUD-01 | 玩家通过行动菜单提交有效 Throw Self | 当 tick 不请求 `click`、`icon_collect` 或其他直接 SFX；保留投掷状态与回合通知 | `MutinyPlayerInput.TryCommitCharacterThrow` | `VerifyCharacterThrowAudio` |
| CHAR-THROW-AUD-02 | AI 执行 `AIMoveType.SelfThrow` | 当 tick 不请求 `click`、`icon_collect` 或其他直接 SFX；保留 `CanShoot` 供第二阶段使用 | `MutinyAIController.ExecuteMove` | `VerifyCharacterThrowAudio` |
| CHAR-THROW-AUD-03 | 角色实际触碰已落地空投箱、且箱内武器随后实际发放 | 继续由箱子路径播放 `click` / `icon_collect`，不能因本修复删除 | `MutinyTreasureChest.AdvanceOriginalTick` | `AIRDBG-02` 与恢复空投后的 P6 用例 |
| CHAR-THROW-AUD-04 | 跳跃后的地面/墙/天花板物理接触 | 保留 `CHAR-AUD-01..04` 的 `hitwall` 节流；它不是 Throw Self 起始声 | `MutinyCharacter.HandleOriginalPhysicalContact` | `VerifyCharacterCollisionAudio` |

## 缺陷卡 BUG-CHAR-THROW-AUD-001

- **Unity 实际**：`TryCommitCharacterThrow` 和 AI `SelfThrow` 分支均在提交末尾调用 `PlaySFX("click")`。
- **最早分歧**：角色初速度提交的同一 tick；原版 `Character.twang → Solid.twang` 没有任何声音调用。
- **根因**：将原版空投箱打开音 `click` 错接到两条角色自身跳跃路径。空投系统即使被临时禁用，错误调用仍会发生。
- **修复**：删除两条 Throw Self 路径的 `click` 调用，保留空投箱自身的声音入口不变。
- **回归**：`VerifyCharacterThrowAudio` 订阅生产音频管理器的实际播放事件，分别驱动玩家生产输入与 AI 生产执行方法；两次提交均不得产生 SFX 请求。

## 验证状态

- 静态确认：E01–E03 已完成。
- 已实现：已删除 `MutinyPlayerInput.TryCommitCharacterThrow` 与 `MutinyAIController.ExecuteMove` 的 Throw Self `click` 调用；保留武器发射及空投箱自身的声音入口。
- 代码检查：2026-09-17 `dotnet build Assembly-CSharp.csproj --no-restore` 通过，0 error；仅保留既有 `MutinyLevelTest.levelXml` 未赋值警告。
- 自动测试：`VerifyCharacterThrowAudio` 已加入总验证菜单，尚未在 Unity 中实际执行。
- Unity Play Mode、原版运行对照：待执行。
