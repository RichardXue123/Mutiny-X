# CHAR-AUD-01 角色接触音效行为规格

## 基线与证据

- 原版：`Mutiny Source/mutiny-flash-game/mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。
- 静态来源：`Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Character.as`，`advance`（末尾 `contactTime++`）与 `contact`；`Solid.as`（地面、墙、天花板均调用 `contact`）。

| ID | 原版规则 | Unity 入口 |
| --- | --- | --- |
| CHAR-AUD-01 | 角色没有主动移动、空中飞行或地面滚动的独立音效。 | 不在 `MarkSelfThrown`、爆炸击飞或逐 tick 移动处播放角色声音。 |
| CHAR-AUD-02 | 每次 terrain/box 的地面、墙或天花板接触均进入 `Character.contact`。若 `contactTime > 5`，播放一次 `hitwall`；随后无条件将其置零。 | `MutinyPhysicsBody.OnFloorLanded/OnWallHit/OnCeilingHit` → `MutinyCharacter.HandleOriginalPhysicalContact`。 |
| CHAR-AUD-03 | `Character.advance` 在接触处理后、每个 25 Hz tick 末尾执行 `contactTime++`；因此两次接触之间需至少六个无接触 tick 才再次出声。同 tick 多次接触只有第一次可能出声。 | `MutinyPhysicsBody.OnSimulationStep` → `AdvanceOriginalContactTimerTick`。 |
| CHAR-AUD-04 | `Character.subtractHealth` 只扣血和死亡，不播放 `hitwall`。爆炸击飞后的声音仍由后续真实碰撞决定。 | `MutinyCharacter.TakeDamage` 不再直接调用 `PlaySFX("hitwall")`。 |

## 回归用例

| 用例 | 操作 | 期望 | 状态 |
| --- | --- | --- | --- |
| CHAR-AUD-T01 | 通过 `MutinyPhysicsBody.AdvanceSimulationTick` 先驱动六个无接触 tick，再驱动地面碰撞。 | 生产碰撞路径请求一次 `hitwall`。 | 已写入 Unity 验证菜单，未执行。 |
| CHAR-AUD-T02 | 对同一角色调用 `TakeDamage`。 | 不产生碰撞音效；等待五个无接触 tick 后下一次碰撞再次出声。 | 已写入 Unity 验证菜单，未执行。 |
| CHAR-AUD-T03 | Unity/原版分别执行主动跳跃撞墙、滚动、爆炸击飞撞墙和落地。 | 仅接触后按 >5 tick 节流播放 `hitwall`，无额外移动/滚动声音。 | 待 Play Mode/原版对照。 |

## 完成记录

- 静态确认：CHAR-AUD-01..04。
- 已实现：统一物理接触音效、原版 tick 节流、移除扣血时的错误 `hitwall`。
- 已执行：`dotnet build Assembly-CSharp.csproj --no-restore` 通过（0 error；现有 `MutinyLevelTest.levelXml` 未赋值 warning 保留）。
- 待运行验证：CHAR-AUD-T01..03。
