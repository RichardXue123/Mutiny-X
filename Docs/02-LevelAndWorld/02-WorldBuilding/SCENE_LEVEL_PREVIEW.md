# 任意模式关卡加载到活动场景（2026-10-01）

用户授权的 Unity 编辑器工具扩展，原版来源不适用。实现前登记以下规格；不改变原版选关、解锁或结局范围。

| ID | 可观察行为及状态转换 | 生产入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- |
| EXT-SCENE-LVL-01 | 工具选择单人/本地双人及模式内编号，发现已有XML，不硬编码15/18上限；加载前验证资源与XML；模式由所选身份决定，独立于原版players字段；失败保留已有场景关卡 | MutinyLevelSceneMenu.TryLoadLevelToScene | 加载所有现有关；缺失/坏XML拒绝且保留旧关；players=1双人关仍为两个人类队伍 | 通过：现有34关、错误保护及双人999测试夹具 |
| EXT-SCENE-LVL-02 | 活动场景仅保留一个工具加载的关卡，地形/角色在Scene可见；重复加载替换旧关；清理含失活关卡，不清理其他已打开场景；加载及清理可Undo | Load to Active Scene / Clear Loaded Level | 两次加载、清理、Undo、失活对象及附加场景保留 | 通过：替换Undo/Redo、清理Undo、失活关及附加场景/自定义宿主保留 |
| EXT-SCENE-LVL-03 | 工具加载的关卡按XML保存模式与编号；Main进入Play先显示Title，独立非Main预览场景直接Gameplay并从指定XML重建运行状态，双人双方由人控制；退出Play恢复编辑场景 | 序列化预览标记 → Frontend初始化 → Controller.BuildLevel | 保存场景Play：Main含旧true标记仍标题，经GM进入；独立预览直接游戏 | Main启动修复33项通过；原直接预览历史结果见下 |
| EXT-SCENE-LVL-04 | Main在已加载或清理后均从标题菜单启动；缺少主摄像机时创建主摄像机，已有摄像机沿用；加载清理仅在Edit Mode可用 | BootstrapAfterSceneLoad / Editor窗口 | 各种Main标题，非Main预览直入、摄像机及Play时拒绝场景编辑 | Main菜单/清理修复33项通过；既有相机与Edit资格结果仍保留；窗口鼠标待人工 |

入口：`Mutiny → Levels → Load Level to Active Scene...`；选模式与编号后点击 `Load to Active Scene`。保留旧 `Load Level 1 to Active Scene` 快捷入口。清理可用窗口按钮或 `Mutiny → Levels → Clear Loaded Level`。

当前启动方式（用户纠正后）：`Main` 按Play始终显示主菜单，需要临时单人16时在GM输入 `enterlevel 16`。如希望直接运行场景关卡，可在独立的非Main预览Scene内使用加载工具。详见 [Main启动修复](../../01-GameFlowAndProgression/01-FrontendNavigation/MAIN_SCENE_STARTUP_FIX.md)。旧105/148测试的Main直入行为已由新规格取代。

实现中证据校正：初版规格曾要求players与模式匹配；核对 [原始双人XML记录](../../10-OriginalEvidence/Artifacts/TwoPlayerLevels/README.md) 后发现双人01–05及18原件为players=1，该字段不代表菜单会话模式。以上规则已在实现时纠正为模式身份优先，原始XML不改，并添加全18双人关及players=1扩展地图的验收。

## 完成口径

- 静态确认：旧工具仅支持单人1、先清后验证资源；Main前端自动显示标题并隐藏预烘焙关卡。新工具通过已授权扩展处理以上行为。
- 已实现：模式/编号窗口、自动XML列表、旧单人1快捷入口、活动场景替换与清理Undo、序列化预览标记和模式/编号。自定义控制器宿主保留无关组件/子对象，仅清除关卡引用；工具创建的MutinyGame宿主整体清理。独立预览Play重建并进入Gameplay；Main先进入标题且清理后仍能启动菜单。
- 实际测试通过：Unity 6000.6.0f1 隔离工程 batchmode/nographics，105/105断言。实际调用生产编辑器加载/清理入口，覆盖全部34关；单人16和双人18场景真实Save/Open/EnterPlaymode，等待自然Start/Update后核对回合、队伍、地形、前端与生产重开；普通Main标题回归。结果见 [SCENE-LEVEL-PREVIEW-20261001.txt](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/SCENE-LEVEL-PREVIEW-20261001.txt)，重跑工具为 `Tools/SceneLevelPreviewVerification.cs`（复制到隔离工程Assets/Editor）。
- 待运行验证：可见Unity窗口中的菜单/下拉框鼠标操作、Scene/Game画面及镜头体验、完整人机或本地双人对战。无图形批处理通过不等于以上人工验收已完成。
- 已知差异：Unity调试工具，无Flash对应；场景关卡编辑不在Play Mode执行。

2026-10-01后续验证：单人16新增0.5重力与敌人Luck50后，该工具与低重力专项148项重新通过，记录见 [太空16新设置](../04-ObjectsAndSpawns/LEVEL_1_16_SPACE_DRAFT.md)。当前重跑 `Tools/SceneLevelPreviewVerification.cs` 时，需将 `Tools/Level16SpaceSettingsVerification.cs` 一并复制到隔离工程Assets/Editor。
