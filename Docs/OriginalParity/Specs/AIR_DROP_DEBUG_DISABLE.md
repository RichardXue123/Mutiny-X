# 空投调试停用规格

## 范围与授权

这是用户于 2026-09-16 明确授权的临时调试扩展，不代表原版规则变化。原版空投行为仍由 P6-01～P6-07 跟踪；恢复空投时必须重新执行这些条目的验收。

## 证据

| ID | 类型 | 来源 | 结论 |
| --- | --- | --- | --- |
| AIRDBG-E01 | U | 用户报告：角色跳跃时错误播放拾取空投声音 | 根因已单独确认是 Throw Self 路径误播空投箱 `click`，而不是禁用空投系统仍在拾取；修复与回归见 `CHARACTER_THROW_AUDIO.md`。调试期间仍需排除空投系统对角色行动、声音和回合静止状态的干扰。 |
| AIRDBG-E02 | S | `TreasureChest.as:151-164` | `icon_collect` 只在箱子实际移除一件内容并加入角色库存时播放。 |
| AIRDBG-E03 | S | `TreasureChest.as:181-217` | 原版允许存活、未被拖拽的角色与已落地箱子矩形相交后开始领取；角色是否在跳跃中不是拒绝条件。 |

## 行为规则

| ID | 前置/入口 | 调试状态下的结果 | Unity 入口 |
| --- | --- | --- | --- |
| AIRDBG-01 | 关卡创建或加载空投管理器 | 系统报告一次禁用日志，不推进任何箱子 | `MutinyTreasureChestManager.Update` |
| AIRDBG-02 | 回合切换请求生成空投 | 不抽取随机数、不创建箱子、不播放 `chest_appear` | `MutinyTreasureChestManager.TryDropNew` |
| AIRDBG-03 | 热重载前已有空投箱 | 首次禁用处理时销毁，避免镜头继续跟随或稍后领取 | `MutinyTreasureChestManager.DisableExistingChests` |
| AIRDBG-04 | 任意代码误调用箱子 tick | 箱子不移动、不检测角色、不修改库存、不播放 `click`/`icon_collect`、不重置回合 inactivity | `MutinyTreasureChest.AdvanceOriginalTick` |

## 回归用例

| ID | 操作与断言 | 状态 |
| --- | --- | --- |
| AIRDBG-TC01 | 调用管理器 `TryDropNew`，箱子数保持 0。 | 待 Unity 自动验证 |
| AIRDBG-TC02 | 构造带内容的箱子并调用其生产 tick，`TimeTaken`、角色库存和箱子状态均不变化。 | 待 Unity 自动验证 |
| AIRDBG-TC03 | Level 1 连续完成多个回合并跳跃；层级中无 `TreasureChest_*`，日志出现一次 disabled，且没有 `chest_appear`、箱子 `click` 或 `icon_collect`。 | 待 Play Mode 验证 |

## 恢复要求

只修改 `MutinyTreasureChestManager.SystemEnabled` 还不足以把 P6 标为完成。恢复后还需重新核对原版生成概率、最多数量、有效列、下降、拾取时序、镜头和声音。
