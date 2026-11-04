# Pool3D 整合工程 V1：M0/M1 阶段交付

完成日期：2026-10-27（Asia/Shanghai）。将 SpinLab 玩法与 V3 美术接入同一独立 Unity 工程，完成本轮批准的**几何统一与动态美术**两步。SpinLab、V3 及恢复主源保留。

## 入口

Unity **2022.3.62f2 / Built-in / Linear**，工程为本目录 `UnityProject`，场景为 `Assets/Scenes/IntegratedOffline.unity`。

场景保存了 V3 静态环境及离线启动器。游戏相机、16/10 球和球杆由启动器在用户运行时组装；本轮没有生成 Windows EXE，也没有替换现有 SpinLab 程序。不要使用旧 SpinLab EXE 来判断本整合工程效果。

## 已接入

- 2540×1270 mm 台面几何；球径 57.15 mm 不变。根据实际 V3 库边网格生成 31 段二维碰撞包络，规则、袋口、自由球、摆球及 AI 几何接入同一局内配置。
- 完整 0–15 号 2048²球贴图、V3 专用 UV 球网格及树脂材质；8 球模式 16 球，9 球模式 10 球。
- 原 PoolTable 状态驱动位置、旋转及显隐，复用 V3 球台、简化房间和 Cue0_Hero 球杆；不启用第二套 Rigidbody 运动。
- 保留拖动瞄准、沿杆后拉松手击球、侧栏拉杆、俯视及置球入口。球杆按真实皮头/杆尾锚点定位，左右加塞不再通过偏移后的杆中心反推瞄准角。
- 独立产品名 `Pool3DIntegrated`，独立离线 profile `integrated-v1`；原 SpinLab 统计不迁移或覆盖。
- 加塞物理、旧模式开关与 SpinPhysics.json 沿用 SpinLab；原数值模型和固定步长保留。

## 检查证据

| 证据 | 结果 | 文件 |
|---|---|---|
| Unity 编辑模式导入与 C# 编译 | 通过，最终检查进程退出 0 | `Docs/verification.json` |
| 几何/配置/布局/缓存 | 308 项通过 | `Docs/geometry_checks.json` |
| 资源/锚点/显隐/投影/释放/保存 | 301 项通过，16 球资源齐全 | `Docs/presentation_checks.json` |
| 原加塞回归 | 24 组上游、8 组方向、120 组碰撞性质、33 项配置拒绝等通过 | `numeric_checks.json` |
| 生成工具 | 目录保护 3 项、球贴图 3 项通过 | `Tools/test_prepare.py`、`Tools/test_ball_set.py` |
| 来源保护、GUID、差异清单 | 见最终核对报告 | `Docs/verification.json`、`Docs/source_changes.json` |

独立只读审查发现的 legacy/V3 配置混用问题已先复现再修复；失败证据保留于 `Docs/review_contract_red.json`，修复后的整套数值/资源检查已通过。见 `Docs/review.md`。

**没有进入 Play Mode、启动游戏、运行对局、截图或查看渲染。以上结果不代表实际操作、四种模式完整流程、外观或性能已验收。**

## 仍待后续阶段

- M2：完整动态细绒、正式灯光、GI/反射及后处理。当前刻意不实例化 V3 四球静态细绒，不复用旧 LightingData；运行时保留基础方向光/环境光，基底台呢已按厘米世界坐标换米采样。因此本版尚未达到 V3 样板的完整灯光效果。
- M3：Windows 构建、程序包/源码包与包完整性核验。
- 用户自行检查：开始/暂停/结算、实际拖杆手感、左右高低杆、袋口/自由球、相机、号码可读性及性能；根据反馈再改系数。
- 新音效、AI 新系数调优/主动加塞、完整加塞预测、正式中式袋口、抬杆/跳球/弧线均未加入。

袋口沿用现有 PoolSet 模型，捕获仍采用圆形中心检测；这是二维整合实验版，不是完整中式器材物理标定。模型高度与二维冲量的边界见 `Docs/geometry_contract.md`。

## 维护

- 几何配置：`UnityProject/Assets/StreamingAssets/TableGeometry.json`。本文件与当前模型绑定，修改尺寸、标识、库边或头线区域时，不允许混入旧几何；旧加塞模式开关只切换旋转算法，不切换球台模型。
- 加塞配置：`UnityProject/Assets/StreamingAssets/SpinPhysics.json`，范围与含义沿用 SpinLab。
- 显示：`Assets/OfflineRuntime/OfflineSceneView.cs`、`Presentation/IntegratedBallView.cs`、`IntegratedCueView.cs`；几何：`Geometry/`。
- 球贴图：`Source/generate_ball_set.py`；几何导出后生成：`Source/build_geometry.py`。工具依赖见 `Source/requirements.txt`。球贴图重制要求 Windows Georgia Bold 字体。
- 编辑模式完整检查入口：`Pool3D.Offline.Editor.IntegratedPresentationChecks.Run`，串行执行几何/加塞/独立显示检查并静态保存场景。运行前保存当前修改并确认目标工程；入口会重建检查对象及保存的整合场景。
- `Tools/run_editor.py` 使用本机 Unity 路径及当前许可 IPC 名称，只能运行白名单中的编辑模式入口；不处理凭据，不启动 Player。非本机环境需调整 Editor 路径。
- `prepare_integration.py`、`adapt_core.py`、`adapt_presentation.py` 是首次隔离/迁移记录，不要在已修改工程上重复执行。新的局部修改应直接修改相应源码并更新差异清单。
- 原始 Unity 日志在工作区研究目录，可能含环境或许可信息；不要直接整段回显或打入分发包。

来源与许可见 `ThirdPartyNotices/`。PoolSet MIT 许可不覆盖原游戏球杆资源。
