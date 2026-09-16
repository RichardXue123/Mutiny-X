# 角色重合显示层

范围：两个或更多存活角色的像素精灵重合时的前后关系与稳定性；不涉及地图 tile、武器、特效或角色自身旋转规则。

## 原版证据

| ID | 类型 | 来源 | 结论 |
|---|---|---|---|
| CHAR-LAYER-E01 | S | `throwgame/Controller.as:48-55` | 原版依次创建 `tileLayer` 与 `characterLayer`；角色都放入同一个 `characterLayer`。 |
| CHAR-LAYER-E02 | S | `throwgame/Character.as:56-66`、`Solid.as:30-34` | 每个 Character 是以 `Controller.characterLayer` 为父节点的 `Solid`，且 `useHolder=true`。 |
| CHAR-LAYER-E03 | S | `util/Clip.as:40-49` | `Clip.show()` 为每个 holder 调用父节点的 `getNextHighestDepth()`；角色本体再作为 holder 的子元素创建。故 XML 中后创建的角色 holder 深度严格高于先创建角色。 |
| CHAR-LAYER-E04 | S | `throwgame/TileSystem.as:152-170` | 读取 XML `obj` 时按原始对象顺序逐一 `new Character`，随后才写入队伍。 |
| CHAR-LAYER-E05 | S | `throwgame/Character.as:101-104` | `characterOverlay` 是同一角色 holder 内、角色本体之后创建的子元素；它高于本角色本体，但低于后创建角色的 holder。 |
| CHAR-LAYER-E06 | U | 用户报告 | Unity 中角色模型重合时发生闪烁。 |

## 规则

| ID | 前置/触发 | 原版可观察结果 | Unity 入口 | 验收 |
|---|---|---|---|---|
| CHAR-LAYER-01 | 关卡 XML 顺序创建第 N 个角色 | 角色 N 的完整 holder 深度高于第 N-1 个角色；重合区域稳定显示 N，不在帧间交换 | `MutinyLevelBuilder.BuildObjects` | `VerifyCharacterLayering` |
| CHAR-LAYER-02 | 角色本体与其 overlay 同时可见 | overlay 的所有子层高于本角色模型，且低于下一个角色 holder | `MutinyCharacterOverlay.CreateOverlayUI` | `VerifyCharacterLayering` |
| CHAR-LAYER-03 | 初始角色与后续动态 characterLayer 内容并存 | 角色层的最大槽位低于动态武器/特效层，水层始终在角色层之上 | `MutinyLevelBuilder`、`MutinyWeapon`、`MutinyExplosion` | `VerifyCharacterLayering` |

## 缺陷卡 BUG-CHAR-LAYER-001

- **Unity 实际**：`BuildObjects` 为每个角色写入同一个 `sortingOrder=20`。重叠的透明 SpriteRenderer 没有确定的前后关系，渲染批次可在帧间竞争。
- **最早分歧**：第二个角色创建时；原版已经请求下一个 Flash depth，Unity 仍复用同一 sorting order。
- **修复**：按原版 XML 创建索引分配稳定的角色根层槽位；为每个角色预留足以容纳本体及 overlay 子层的 slot，并让 overlay 从所属角色根层派生排序。动态武器/特效与水层移动到全部初始角色槽位之上。
- **回归**：以 `MutinyLevelBuilder.BuildLevel` 构造按 XML 顺序的两个重合角色，断言相邻角色根层严格递增、overlay 位于本角色和下一角色之间、武器/爆炸/水层高于最大角色槽位。

## 验证状态

- 静态确认：E01–E05 已完成。
- 已实现：`BuildObjects` 按原始 XML 创建索引分配 8 个 sorting order 的角色 holder 槽位；角色 overlay 从所属角色本体层派生。武器、爆炸与水层提升到全部初始角色槽位之上。
- 代码检查：2026-09-17 `dotnet build Assembly-CSharp.csproj` 通过，0 error；仅保留既有 `MutinyLevelTest.levelXml` 未赋值警告。
- 自动测试：`VerifyCharacterLayering` 已加入总验证菜单，尚未在 Unity 中实际执行。
- Unity Play Mode、原版运行对照：待执行。
