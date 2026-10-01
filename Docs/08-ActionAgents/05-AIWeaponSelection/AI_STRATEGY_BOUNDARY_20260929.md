# AI 策略开关与输入/输出边界（2026-09-29）

历史记录：本文件描述 `f2d6251` 的开关框架阶段和当时的实测结果。用户随后授权实现 [完整效果模拟初版](AI_ENHANCED_SIMULATION_V1.md)，增强路由已经从 bootstrap/fallback 更新为 effects-v1，连续计划也已加入；2026-10-02 GM 最近成功命令按钮及历史已移除，见 [当前控制台规格](../../11-VerificationAndDebug/05-GMTools/GM_CONSOLE_REDESIGN.md)。下文原始规格和 403/403 证据保留，不用新算法或新控制台覆盖旧验收结论。

## 授权、基线与范围

用户授权：在 `feature/aienhance` 建立 `aienhance 1/0` 和独立策略入口，验证后提交并推送。实施基线为 `main` 的 `10c99ea`。本文件先于代码实现建立。

此阶段不实现完整伤害模拟、新评分、扇风或连续武器优化。增强入口明确为 **enhanced-bootstrap / legacy-fallback**，使用现有兼容算法；GM 与日志必须显示该状态，不宣称已增强。原版来源不适用：以下均为用户授权扩展，不计为 Flash 一致性实现。

## 行为规格

| 稳定 ID | 可观察行为及状态转换 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- |
| GM-14 | 正式 GM 解析只接受 `aienhance 0/1`，大小写/空白兼容；缺参、越界、非数字、额外参数失败，不改变开关或成功历史；成功加入历史并可重放 | MutinyGMManager.ExecuteCommand → SetEnhancementEnabled | 正式解析及历史重放；Help；非法输入 | 已实现；生产解析专项通过；界面点击/排版待目视验收 |
| EXT-AI-STRAT-01 | 默认兼容模式；开关为会话状态，切关保留，SubsystemRegistration 重置；不写存档、不改 XML/角色 Luck/库存/人类控制 | MutinyAIController 策略配置 | 创建/销毁控制器、实际重开、GM 接管及设置互不干扰 | 已实现；新 Play 默认、控制器生命周期、实际第 7 关重开及设置独立性通过；无 Domain Reload 设置待实机验证 |
| EXT-AI-STRAT-02 | 同一回合协程、快照、预算泵和执行器，分别路由 legacy / enhanced-bootstrap；共享行动结果，策略不能自行提交真实行动；bootstrap 明确回退，不更改候选、随机顺序、评分和胜者 | IAIDecisionStrategy → DecisionWork → AIMove → ExecuteMove | 15 武器固定种子完整 trace 对比基线与三种模式；原生回合和接管实际提交 | 已实现；完整 trace 180/180 一致；绑定身份、原生 AI 和 GM 接管提交通过 |
| EXT-AI-STRAT-03 | 开关实际改变时递增配置版本；未提交搜索及镜头等待的旧结果失效，下一次按新模式重算；重复设同值不取消；来回切换也不能重新接受旧结果 | DecisionIsCurrent / production coroutine | 真实 GM 在分帧搜索、镜头等待中切换；无旧动作或扣弹；有效重算；ABA 切换 | 已实现；双方向搜索切换、同值、ABA、镜头等待取消和新模式提交通过 |
| EXT-AI-STRAT-04 | 已提交跳跃/武器不中断；动作绑定提交时策略身份。金币等完整武器序列继续使用绑定身份，下一独立决策使用新设置；当前阶段专用操作仍为原有实现 | ExecuteMove / MutinyWeapon 策略上下文 | 实际跳跃飞行中切换、金币八枚中切换；身份保持、一次扣库存、资格不重置 | 已实现；跳跃后同角色续行动、真实八枚金币结束及单次扣库存通过；箱体/炮弹身份传递静态确认 |
| EXT-AI-STRAT-05 | 行动日志标注请求模式、策略 ID、实际算法、回退和配置版本；legacy 原有 trace JSON 字段不改，bootstrap 也保持相同算法的重放兼容 | LogCommittedAction / strategy diagnostics | 捕获生产行动日志；完整 JSON 对比；取消不发布未完成 trace | 已实现；生产金币行动日志、完整 trace 对比及搜索取消不发布通过 |

## 输入与输出边界

- 回合适配器采集 `DecisionWork` 的角色/库存/物理快照、有效 Luck、强制武器、阶段及策略版本。此次快照仍是 Unity 主线程适配数据，包含场景引用；**不是**已经可用于后台线程的纯数据模拟世界。
- 决策策略只驱动候选计算并返回 `AIMove`；共同协程负责分帧、校验、选择角色、运镜和实际提交。
- Legacy 策略保持原有计算次序及首次/续行动胜者门；EnhancedBootstrap 策略通过显式兼容回退复用 Legacy，不复制公式，不同时运行两种搜索。
- `ExecuteMove` 的正式武器实例绑定此次策略上下文。该上下文只描述已提交动作，不读取后来变化的全局开关；供后续连续控制器扩展使用。
- 共享物理、回合、扣弹和武器效果规则不因开关分叉。此阶段没有新增 AI 扇风、金币优化或伤害结算副本。

## 命令时序与范围

- `1` 选择增强入口（当前明确 fallback），`0` 选择现有兼容入口。
- 只影响已是 AI 控制的队伍，包括 GM 接管；命令本身不接管人类。
- 正在思考、已求值但尚未提交或正在等待镜头：旧版本不可提交。已发射、已跳跃：不撤销、不返还行动或库存。
- 连续武器沿提交时绑定身份结束；跳跃后新的武器决策采集最新设置。
- 会话跨关卡保留，不写 SaveSystem；新 Play/应用重启默认关闭。
- 强制武器、Luck 与日志设置继续独立。EnhancedBootstrap 保持现有 Luck 含义；未来增强算法必须另行规格化预算映射与 trace 版本。

## 验证与完成口径

先运行未修改基线的生产同步决策并保存固定种子 15 武器完整 trace，再逐项比较修改后的关闭、开启 fallback、回切模式。生命周期测试必须通过 GM、实际生产协程、正式跳跃/武器/回合入口驱动；不得只手改工作对象证明切换有效。

分别记录编译、专项 Play Mode、现有预测/GM/高 Luck/箱体回归、主工程图形交互及 Android。没有实际运行的项保留待验收，不将完整模拟或后续控制算作已实现。

## 实现入口与后续接入点

- `Assets/Mutiny/Scripts/Simulation/MutinyAIStrategyBoundary.cs`：策略接口、兼容策略、明确回退的增强入口、配置版本和不可变身份。增强算法随后替换 `EnhancedBootstrapStrategy`，兼容策略保持独立。
- `MutinyAIController.cs`：`PrepareDecisionWork` 绑定输入版本；共同协程驱动所选策略、校验快照并提交。`ResolveDecisionWinner` 把身份附到共享 `AIMove`；`LastDecisionStrategy` / `LastCommittedStrategy` 可诊断路由。
- `MutinyWeaponFactory.cs` / `MutinyWeapon.cs`：只在实际输出提交时绑定上下文；木箱/火药桶后续实例、大炮炮弹沿父对象传递，不采集切换后的全局设置。玩家武器保持未绑定身份。
- `MutinyGMManager.cs`：正式命令、成功历史及 Help，不从 GM 绕过回合资格。

这是主线程上的策略边界：目前接口接收含 Unity 引用的 `DecisionWork`，尚未实现线程安全的纯数据世界、完整效果模拟器或专用连续控制策略。未来模拟器、增强评分、独立预算和新版 trace 应作为新增实现接入，不改写 Legacy 公式。

## 本次完成报告

### 静态确认

此次为用户授权扩展，原版来源不适用，没有改变 Flash 一致性结论。旧候选/评分/随机数/胜者门公式未改，回合与物理执行器共享；无新存档/XML 写入。箱体子实例和炮弹上下文传递已检查，未将静态检查算作实际身份传递用例通过。

### 已实现

GM-14、EXT-AI-STRAT-01..05 的开关、策略路由、输入配置版本、共同输出执行及提交身份绑定均已接入。取消发生在已有安全校验点；不会同时执行两套搜索或回滚真实动作。

### 实际测试通过

Unity 6000.6.0f1 隔离工程成功编译并进入实际 Play Mode：

- 修改前 `10c99ea` 生产决策采集：60/60。15 种武器 × Luck 0/1/7/33，种子 13731。
- 新专项：403/403。关闭／开启 fallback／回切的 180 份完整 trace SHA-256 与修改前一致，涵盖随机 draw、候选参数、预测、评分和最终胜者；其余断言经正式 GM、真实协程、武器和回合入口验证生命周期。
- 既有兼容回归：263/263，分别为高 Luck/响应式/第 12 关尸体快照 54/54、预测 18/18、GM 66/66、箱体与镜头 125/125。

可复跑入口：空场景中 `Mutiny/Parity/Validate AI Strategy Boundary Play Mode`；batch 入口 `Mutiny.Verification.Editor.MutinyAiStrategyVerificationMenu.RunBatch`。修改前完整 JSON 的独立 SHA-256 清单已随 `Resources/Verification/ai-strategy-baseline-10c99ea.json` 提交，不需依赖本机临时目录；`CaptureBaselineBatch` 只用于未改动基线，当前实现主动拒绝覆盖基线。

日志来源与范围见 [验收摘要](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/AI-STRATEGY-BOUNDARY-20260929.txt)。

### 待运行验证

主工程真实 GM 面板输入/历史按钮/提示排版、全关卡长时间对局、关闭 Domain Reload 的 Editor 配置、Android 构建及真机未运行。此次未声称全项目广域回归或性能上限验收完成。

### 已知差异与未实现部分

`aienhance 1` **尚未提升决策能力**，日志明确显示 `strategy=enhanced-bootstrap algorithm=legacy fallback=True`。完整伤害/火焰/水/连锁模拟、气球扇风、金币后续优化及新的收益评分仍未实现；武器身份绑定只是为这些后续能力建立边界。Luck 含义和现有分帧性能不变。
