# Nitrome Mutiny - 外部依赖与 Unity 对应处理报告

**日期**：2026-09-15  
**目标**：梳理原版 Flash SWF 中的域名锁定、广告及外部网络依赖，记录在 Unity 6000 中的解耦与重构方案。

---

## 1. 原版 Flash 外部依赖梳理

通过反编译 `mutiny.swf` 中的 AS2 脚本，识别出以下原版外部服务调用：

| 模块 / 符号 | 原版 AS2 逻辑 | 触发条件 | 在 Unity 6000 中的对应处理 |
|:---|:---|:---|:---|
| **Domain Check (域名锁)** | 检测 `_root._url` 是否匹配 `nitrome.com`。若在非授权站点运行，跳转到 nitrome 提示页。 | 游戏启动第 1 帧 | **移除**。离线原生独立运行，无需外部域名校验。 |
| **MochiAds / Preloader** | 原版用于 Flash 网页开屏广告与分发跟踪。 | 加载期 | **移除**。Unity 启动即直接进入主游戏与关卡流程。 |
| **Nitrome Highscores / Leaderboards** | 通过 `LoadVars.sendAndLoad` 向 Nitrome 官方服务器提交得分与关卡耗时。 | 游戏通关 / 胜负结算 | **解耦**。替换为本地持久化存档系统 `MutinySaveSystem`（基于 `PlayerPrefs`），记录最高解锁关卡与设置。 |
| **Web Links (跳转链接)** | `getURL("http://www.nitrome.com", "_blank")` 等 Nitrome 官网宣传按钮。 | 点击 Logo / 更多游戏 | **安全阻断**。保留美术展示，避免在离线运行时弹出浏览器跳转。 |
| **SharedObject (Flash LSO 本地存档)** | 原版通过 `SharedObject.getLocal("nitrome_mutiny")` 保存关卡进度与声音静音状态。 | 回合结束 / 退出游戏 | **直接映射**：由 `MutinySaveSystem` 统一接管，支持跨平台本地持久化保存。 |

---

## 2. 结论

Unity 6000 重构版本实现了 100% 独立离线运行能力：
- 零外部网络请求依赖。
- 零外部广告 SDK 或 Flash 插件依赖。
- 音视频、物理演算、AI、关卡数据与存档全部内置于本地项目工程中。

