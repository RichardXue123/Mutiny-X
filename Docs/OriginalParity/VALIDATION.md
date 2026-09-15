# 验证记录

## 2026-09-15 自动检查

- 18 个关卡运行时副本齐全。
- 37 个 SFX、2 个 Music 已移入 `Resources/Audio`。
- 179 张武器 PNG、原有 tile/character/effect 资源已移入 `Resources/Art` 并保留原 GUID。
- 27 种角色 × 35 帧，共 945 张原版角色帧已加入运行时资源。
- TreasureChest 90 帧已加入运行时资源。
- 非 Editor/Verification 运行时代码已不再引用 `UnityEditor.AssetDatabase` 或 `Application.dataPath` 读取资源。
- `dotnet build Assembly-CSharp.csproj --no-restore`：0 errors，4 个既有序列化/测试字段 warnings。新增脚本通过临时 compile item 一并验证，生成的 csproj 已原样恢复。

## 下一次 Unity 人工验收

当前 Unity 正处于 Play Mode，新增 `.cs` 文件不会进入正在运行的旧脚本域。请先停止 Play Mode，等待右下角导入/编译完成，再确认 Console 没有红色编译错误。

编译完成后先执行菜单 `Mutiny > Parity > Validate Runtime Resources`。预期日志为 18 levels、39 audio clips、945 character frames、90 chest frames，且无 missing resource。

重新进入 Level 1 后按顺序检查：

1. 等待 2 秒且不要点击，回合不得自动切换或显示平局。
2. 鼠标移到红方角色，出现角框；点击后出现行动菜单。
3. 先用武器，落地结算后仍应保留 Throw Self；再投角色后才换队。
4. 下一回合开始前尝试生成空投：应从画面上方落下，镜头最多跟随前 100 tick；不是每次都一定成功，因为原版随机列可能因角色/箱子阻挡而放弃本次空投。
5. 香蕉飞行后再次左键应立刻引爆；不点击则在静止后引爆，并在每次碰撞播放 `banana_bounce`。
6. 角色 idle 应使用各自 1–14 帧，受击飞行保持 hit 起始姿态，静止后播放 15–35 帧恢复段。
7. 选择 Anchor 后直接点击目标横坐标，不需要从角色身上拉抛物线；落地造成 60 伤害，停留 30 tick、淡出 10 tick。
8. 选择 Tidal Wave 后单击即可从水面左侧出现，以 20 px/tick 向右运动。

如果有失败，请保存 Console 第一条异常及其完整 stack trace，并在 `TODO.md` 对应编号下记录。
