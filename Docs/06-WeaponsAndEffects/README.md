# 06 · 武器与效果

## 职责

- 管理武器选择、待抛射、瞄准、取消、发射、持续效果和结束。
- 保存所有武器共享规则。
- 为每种武器保存独立状态机、参数、动画和音频映射。

## 当前入口

- `Assets/Mutiny/Scripts/Simulation/MutinyWeapon.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyWeaponFactory.cs`
- `Assets/Mutiny/Scripts/Simulation/` 下的各武器实现类

## 实现基线

- [武器逻辑实现细节与一致性审计](IMPLEMENTATION_DETAILS.md)：本模块的规范入口，覆盖原版公共生命周期、15 种武器状态机、Unity 调用链、当前差异和验收矩阵。
- 原版规则直接取自 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/`，不得以当前 C# 或旧版 `weapon-registry.md` 反推。
- 本轮只完成静态取证和实现解析，没有运行 Unity；所有运行结果仍标记为“待运行验证”。

## 边界

公共基类只承载原版确实共享的行为；专用武器规则保留在各自实现中。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-WeaponRegistryAndInventory](01-WeaponRegistryAndInventory/README.md) | 武器注册与库存 |
| [02-ReadyAimAndCancel](02-ReadyAimAndCancel/README.md) | 待抛射、瞄准与取消 |
| [03-ProjectileLifecycle](03-ProjectileLifecycle/README.md) | 投射物生命周期 |
| [04-SharedEffects](04-SharedEffects/README.md) | 共享武器效果 |
| [05-CherryBomb](05-CherryBomb/README.md) | Cherry Bomb |
| [06-Dynamite](06-Dynamite/README.md) | Dynamite |
| [07-Banana](07-Banana/README.md) | Banana |
| [08-Boulder](08-Boulder/README.md) | Boulder |
| [09-Cannon](09-Cannon/README.md) | Cannon |
| [10-GunpowderBarrel](10-GunpowderBarrel/README.md) | Gunpowder Barrel |
| [11-Mine](11-Mine/README.md) | Mine |
| [12-ParachuteBomb](12-ParachuteBomb/README.md) | Parachute Bomb |
| [13-PiecesOfEight](13-PiecesOfEight/README.md) | Pieces of Eight |
| [14-RumBottle](14-RumBottle/README.md) | Rum Bottle |
| [15-Seagull](15-Seagull/README.md) | Seagull |
| [16-TidalWave](16-TidalWave/README.md) | Tidal Wave |
| [17-VoodooDoll](17-VoodooDoll/README.md) | Voodoo Doll |
| [18-WoodenCrate](18-WoodenCrate/README.md) | Wooden Crate |
| [19-Anchor](19-Anchor/README.md) | Anchor |

## 当前结论

公共生命周期和 15 种武器的逻辑规格已整理。后续实现与修复应先更新稳定规则 ID，再补生产入口回归并记录实际运行结果。
