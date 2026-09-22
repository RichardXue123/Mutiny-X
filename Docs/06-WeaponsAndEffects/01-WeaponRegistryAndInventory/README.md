# 06.01 · 武器注册与库存

[返回上级模块](../README.md)

## 职责

维护武器类型、工厂注册、图标和库存连接。

## 边界

不包含具体武器命中行为。

## 已确认规则

- 原版库存只注册 15 种武器；`cannonball` 是内部子投射物。
- XML 数量 `10` 表示无限，其余数量按重复条目保存；提交时只消耗一次。
- 装备点统一为角色 `(x,y-10)`，仅 Boulder 为 `(x,y-30)`。
- 原版来源：`Character.as::setWeapons/equip/weaponExpired`。
- Unity 入口：`MutinyCharacter.cs`、`MutinyWeaponFactory.cs`。

完整规则 ID、清单和验收见 [武器逻辑实现细节](../IMPLEMENTATION_DETAILS.md#3-注册库存和装备)。本轮未运行 Unity。
