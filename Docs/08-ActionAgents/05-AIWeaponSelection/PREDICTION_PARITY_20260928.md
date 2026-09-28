# AI 试射预测一致性修复（2026-09-28）

实现前行为规格。原始导出证据不修改；不调整 Unity/Flash 随机数差别、评分、Luck 或正式武器玩法。

| 规则 ID | 可观察行为与状态转换 | 原版证据 | Unity 入口 | 验收用例 |
| --- | --- | --- | --- | --- |
| AI-PHY-04 | 香蕉每个模拟 tick 先运动；`vx==0 && abs(vy)<0.5` 或距任一队伍角色严格小于 20 px 时结束本次预测，使用当前位置评分。包含 owner、队友、敌人及队伍中已死亡角色。20 px 整不触发。没有接近角色/停稳时保持公共 101-step 上限。 | `Banana.as:43-107`；`Weapon.as::randomThrows`；Banana pcode。`lastSqDistance=Infinity` 且无后续赋值，故 `<2500` 的远离分支不实际触发，不能新增历史距离行为。 | `EvaluateCharacterWeapons → SimulateWeaponImpact` | 正式候选分派在 owner 近处首 tick 结束；分离验证敌人/队友/已死亡角色；20 px 边界、50 px 内远离不结束、静止、101-step；与正式香蕉 tick 引爆位置对照。 |
| AI-PHY-05 | 普通抛射候选沿用真实工厂 `Initialize → PrepareForEquip` 后的起点，不覆盖回角色中心。普通武器 `(x,y-10)`，Boulder `(x,y-30)`；每条候选重置至相同装备起点。 | `Character.as::equip:825-838`；`Weapon.as::randomThrows:84-125` 在原始武器 x/y 处重置。 | `CreateFormalWeaponPredictionTemplate → EvaluateCharacterWeapons` | 8 种普通抛射武器经生产分派与正式模板预测对照；近地形/箱体构造中心与装备起点端点不同的用例，确保能抓住旧覆盖；真实 Cherry Bomb/香蕉 Fire/物理终止位置对照。 |

香蕉的角色坐标从当前候选的敌我角色集合读取，不在每个模拟 tick 搜索 Unity 场景。只用于香蕉预测终止，不改变各武器评分对象或正式伤害。Cannon、Anchor、箱体、海鸥、巫毒、海啸及角色跳跃的专用起点逻辑不变。

## 状态

- 静态确认：上述 AS2 与当前生产 C# 对照确认两处差异。
- 已实现：AI-PHY-04/05。普通候选保留正式模板 X/Y；香蕉预测传入敌我角色集合，与正式香蕉共用最近距离及触发判断函数。正式香蕉的判断规则未改变。
- 实际测试通过：2026-09-28 Unity 6000.6.0f1 隔离 Play Mode 新专项 **18/18**。8 种普通武器装备起点、近箱/地形及额外 Boulder/Banana 无障碍差异检查通过；20 px 边界、包含自己/队友/死者、静止、101-step 和真实香蕉 Fire/物理引爆位置通过。修复前同样的最初 16 条中有 11 条失败，修复后全部通过；另补的 2 条通过。
- GM 兼容回归：同一最终 Play Mode 运行 `RunGM()` **66/66**，含接管、强制武器、Luck 设置/重置与日志。没有新增 GM 行为。
- 扩大回归：`RunAiPredictionParity()` **46/50**，不是全通过。以下 4 条既有用例在修复前 **33/48** 时就已失败，修复后没有新增失败或改写其期望：AI-PHY-02 长跳跃超过 70 tick 到水位；BAN-TRAJ-01 香蕉预览与前 5 tick 物理；WPN-03-EFF-02 香蕉地面碰撞；WPN-04-EFF-01/02 Boulder 推挤/伤害/转动。原因仍待单独诊断，不能将当前失败简单归因于本次修改。
- 待运行验证：真实关卡中近箱体的 AI 行动与原版运行对照、Android；4 条扩大回归失败的独立诊断。专项纯状态 fixtures 使用生产公式，不把它们当作完整回合或全部地图通过。
- 已知差异：随机数差别明确不在本次范围；其他物理/评分差异不由这两项测试推定已消除。本次未更改已有 Anchor/PhysicsBody 的其他工作。

重新运行：隔离空场景进入 Play Mode 后使用 `Mutiny/Parity/Validate AI Prediction Fixes Play Mode`；存在活动关卡时该菜单拒绝运行，避免清空当前关卡箱体注册表。专项代码为 `MutinyAiPredictionVerificationTest.Run`；扩大回归为 `MutinyTurnActionUiVerificationTest.RunAiPredictionParity`。实际记录见 [验证摘录](Artifacts/PREDICTION_PARITY_20260928.txt)。

## 原版证据入口

- [Banana.as](../../10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Banana.as) 与 [原始 pcode](../../10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/pcode/scripts/__Packages/com/nitrome/throwgame/Banana.pcode)。
- [Character.as::equip](../../10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Character.as) 与 [Weapon.as::randomThrows](../../10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Weapon.as)。
- 用户指定 [1c0caa5 基线 Banana](https://github.com/RichardXue123/Mutiny-X/blob/1c0caa56bc51bde0fc5f7f1b794cdfbac88566f1/Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Banana.as#L43-L107)，本轮仅消费证据，未修改原始导出。
