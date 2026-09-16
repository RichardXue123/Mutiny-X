# 原版一致性审计

本目录用于跟踪 Flash 原版到 Unity 版的逐项复刻，不以“存在同名类”作为完成标准。

## 执行规范（2026-09-15 起）

- [REPLICATION_STANDARD.md](REPLICATION_STANDARD.md)：证据、状态转换、三层验收和完成门槛。
- [Specs/TURN_ACTION_UI.md](Specs/TURN_ACTION_UI.md)：跳跃、武器与菜单的证据、已发现缺陷和待执行用例。
- [Specs/BEHAVIOR_TEMPLATE.md](Specs/BEHAVIOR_TEMPLATE.md)：后续模块的统一模板。

详细规则与结果写入 Specs；`TODO.md` 仍是唯一工作队列。旧记录中的“已实现”或“已有人工验收步骤”不能自动解释为运行一致性已通过。完成状态按新规范分别列出证据、实现和实际验证结果。

## 现有资料

- `TODO.md`：唯一的执行清单和当前工作项。
- `AUDIT.md`：已经核对的原版证据、当前实现和差异。
- `WEAPON_AUDIT.md`：15 种武器的原版状态机和当前差异。
- `CHARACTER_AUDIT.md`：27 种角色与 SWF symbol/帧段映射。
- `BLOCKERS.md`：需要人工操作、缺少输入或暂时无法判定的事项。
- `VALIDATION.md`：本轮自动检查和下一次 Unity 人工验收步骤。

状态约定：

- `[ ]` 尚未审计或尚未实现。
- `[~]` 已开始，仍有可确认差异。
- `[x]` 已依据原版代码/资源实现，并通过对应验证。
- `[人工]` 需要在 Unity 或 Flash 运行时完成的人工步骤。
- `[阻塞]` 当前输入不足；原因和所缺材料必须写入 `BLOCKERS.md`。

原版证据来自 `Docs/ReverseEngineering/Swf/deobfuscated`、`Docs/ReverseEngineering/Art`、18 个关卡 XML 和原始 SWF。右键取消瞄准是 Unity 版保留的操作扩展，不作为原版偏差处理。
