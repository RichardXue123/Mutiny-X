# Main场景Play启动恢复主菜单（2026-10-01）

用户反馈：编辑器Play时没有主菜单。实现前规格校正：先前Scene加载工具自动设置StartInGameplay，包括Main；前端因此自动直入Gameplay。旧回归只验证“未加载预览的Main”，漏掉“Main内加载过关卡”的实际使用路径。另一个入口缺口：Clear删除控制器后，Main前端bootstrap因找不到控制器直接返回。

| ID | 行为与状态转换 | 来源 | Unity入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| FIX-MAIN-START-01 | Main启动始终Title，即使保存场景中有旧StartInGameplay=true及单人/双人预烘焙关；加载工具在Main不再开启直入标记；战斗根对象隐藏，音乐为menu_music | 用户本轮纠正；Unity工具扩展，无新原版结论 | SceneMenu → Frontend Bootstrap/Initialize | Main分别加载单人16/双人18再保存重开Play；旧标记true仍Title；检查关卡失活、菜单曲目 | 通过：实际保存旧true单人16及双人18Main，自然Play均Title、根失活、菜单音乐 |
| FIX-MAIN-START-02 | Main已Clear或没有控制器时，Play自动建立运行时控制器并显示Title，不改变编辑场景 | 用户菜单修复；Unity入口保护 | BootstrapAfterSceneLoad | 生产Clear后保存Main并进入Play，验证Title/控制器；退出Play编辑场景仍空 | 通过：生产Clear后Play自动补控制器和菜单，退出编辑Main仍空 |
| FIX-MAIN-START-03 | Main菜单后仍可经GM进入单人16及双人关；独立非Main预览场景沿用直接Gameplay | 既有授权能力保留 | ExecuteCommand/Scene preview | Title执行enterlevel16及2_18，模式/人数/重力/Luck正确；非Main场景Save/Open/Play直接游戏 | 通过：三种Main情况下真实GM单16/双18；机器人选择播放01，0.5重力/Luck50保留；独立预览Game |

此修复替代EXT-SCENE-LVL-03中“Main中加载关卡后自动直入”的旧行为；105/148项历史记录属于旧启动规格，不作为新菜单行为的通过证据。

## 完成口径

- 静态确认：跳过菜单与Clear后bootstrap提前返回的路径已定位。
- 已实现：Main在前端初始化时优先Title并隐藏战斗；工具不再为Main打开直入；没有控制器时仅在运行时补建会话入口；工具文案说明Main与独立预览的区别。旧场景true标记无需手动修复即可显示菜单。主工程Main.unity文件未改动，避免覆盖用户未保存的编辑器场景。
- 实际测试通过：Unity6000.6.0f1隔离工程batchmode/nographics，33/33；经过真实编辑器Load/Clear、Save/Open、EnterPlaymode自然生命周期；覆盖旧true单16Main、双人18Main、Clear后Main、非Main独立预览。菜单Page、曲目、战斗根资格、GM进入及机器人01播放均符合。详见 [MAIN-STARTUP-FIX-20261001.txt](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/MAIN-STARTUP-FIX-20261001.txt)，重跑工具为 `Tools/MainSceneStartupVerification.cs`。
- 待运行验证：可见Unity窗口中菜单渲染、鼠标操作与用户当前未保存场景的实机画面。需要停止Play再进入以执行新的启动路径。
- 已知差异：独立预览场景可直接进关，Main始终进入菜单；可见窗口菜单鼠标交互需要人工验收。
