# 新球桌接入准备包

**Blender 最新为 R13 台呢厚度和球底接触**：用户指出 R12 仍过于均匀、缺少厚度。最新文件/实际渲染/高度契约见 `art-source/luxury_pool_table/20261102_felt_depth_r13`。1.35 mm 实体布层的未受压视觉绒面为 Z=0.80018 m，但物理基准必须仍用 Z=0.800 m；球心与原规则不变。导出环境时排除 16 个观察球、指定球杆与球底参考绒丝网格，后者不能烘焙进静态房间。压绒材质的 16 个球坐标输入需要运行时适配。当前 Unity 候选仍 R3b，待同步保护保留；没有进行游戏导入、烘焙、对局或帧率验收。

**Blender 最新为 R12 实拍参考桌面修正**：台呢材质/纤维与库边截面已细化，清理了袋口附近的旧库边残片。新完整源及实际渲染见 `art-source/luxury_pool_table/20261102_tabletop_realism_r12`。原 R4 库鼻轮廓和 R6 木边接缝保留；材质三向投影、微法线与 Sheen 后续需适配游戏 shader 或烘焙。当前候选仍 R3b，未导入最新房间/球杆/台呢；待同步保护有效，不可直接应用。静态预览球仍应从环境导出中排除，沿用现有游戏状态与规则。

**Blender 最新为 R11**：已四墙封闭，加入静态 16 球和用户指定新球杆/前半段预览镜头。`art-source/luxury_pool_table/20261101_closed_room_balls_cue_r11/Unity接入实施方案.md` 列出后续接入步骤；当前尚未执行 Unity 替换。Blender 预览球不可再作为静态环境导入，应由现有游戏状态创建动态球。旧候选仍 R3b，待同步保护有效。

**Blender 最新为 R10 第一版参考房间样板**：补齐墙顶、地毯等细节，并把六个袋网从合计 193920 优化到 34368 三角面；见 `art-source/luxury_pool_table/20261101_room_visual_r10`。当前 Unity 候选资源与旧完整包仍未同步，仍为 R3b，不能直接应用。R10 的 Cycles 灯光和渲染不等同于已完成 Unity 静态烘焙或帧率验收，待同步保护保留。

**Blender 最新为 R9**：新增沙发右座位小龙与左侧墙角高株植物，见 `art-source/luxury_pool_table/20261031_dragon_plant`。只改 Blender 房间源，Unity 候选与旧完整包仍未同步，待同步保护继续有效。

**Blender 当前为 R8 单桌房间**：新增正对球桌远端的座位、旁边独立球杆架和三面墙/顶面/地面，见 `art-source/luxury_pool_table/20261030_lounge_seating`。当前候选资源与旧完整包尚未同步，仍不可直接应用；待同步保护保留。

**当前 Blender 为 R7 吊灯/照明场景**：球桌本体仍为 R6，新增居中的长方形灯具及实际面光。见工作区 `art-source/luxury_pool_table/20261030_pendant_light`。灯具、最新网格与灯光尚未导出绑定到本候选；旧完整 ZIP/模型包仍为 R3b，待同步保护有效。

**Blender 已更新为 R6**：接合六段台呢与木边、清除台下重复模型，粗糙度提高至约 0.95。当前入口为工作区 `art-source/luxury_pool_table/20261030_seam_roughness`；R4 库鼻轮廓保留。候选场景与旧完整 ZIP/模型包仍是 R3b，尚未同步 R6，待同步保护保留。R6 独立材质包仅更新粗糙度贴图，没有把整桌绑定游戏。

**R5 独立台呢材质已完成**：Blender 主源为 R4 几何加 R5 轻量细绒，见工作区 `art-source/luxury_pool_table/20261030_felt_lite`。独立材质包已在本候选工程新增 `Assets/FeltLite` 并通过 Unity 编译，未绑定场景；本目录旧模型包/完整 ZIP 仍为 R3b。待同步保护标记继续有效，正式游戏未覆盖。

**当前有待同步的 R4 Blender 修正**：用户要求仅修正六个袋口台呢的连续圆弧。本轮只改六个台呢网格，木框、袋罩和配色等其他 53 个对象未变。此目录及压缩包内的 Unity 资源仍是上一版 R3b，需按新曲线重新导出碰撞并验证后才能应用；当前切换工具会拒绝应用旧曲线。R4 模型和证明在 `art-source/luxury_pool_table/20261030_g2_jaws`，见工作区根目录交接指南。下文为已验证的 R3b 交付记录。

完成日期：2026-10-30。目标：为 `Pool3D-Integrated-V1` 准备一套能成套切换的球桌显示、材质和几何配置。**当前原工程未覆盖**；本目录的独立候选工程已使用新桌，并通过编辑模式验证。

**当前为 R3b 最小修正版，R2 已回退**：恢复 R2 之前的木框和袋罩，保留其形状；仅调整直边区域 UV/材质分界并清理指定白色残片。所有袋口周围使用原来的贴图和材质，不将木纹映射延伸到袋角，也不增加新配色或新护角设计。六个袋口两侧共 12 段台呢收口参考用户附图最下方的曲线，内沿仍为 2540×1270 mm。袋角原色恢复只修改 UV 和材质归属，顶点与面索引完全不变。证据见 `Docs/rail_fix_checks.json`。

## 交付入口

- 候选工程：`UnityProject`，Unity 2022.3.62f2、Built-in、Linear。打开 `Assets/Scenes/IntegratedOffline.unity` 即是新桌资源和离线启动入口；本轮未启动游戏。
- 模型包：`Pool3D-NewTable-Model.unitypackage`，包含 50 个部件、9 个项目材质及依赖；导入后可使用 `Assets/TableReplacement/Prefabs/TableGameSpace.prefab`。它只导入显示资源，不能单独替换游戏物理配置。
- 完整切换：`Tools/apply_replacement.py`，配对替换 147 个文件（8 个现有文件、139 个新增资源），包括环境、场景、资源引用、604 段库边与六袋配置及必要兼容代码。默认只检查，实际切换自动备份；写入抛出异常时会回滚已写文件。
- 检查结果：`Docs/verification.json`、`replacement_checks.json`、`transaction_checks.json`。

## 已准备好什么

- Blender 中的节点材质已转换为 Unity Built-in 材质；木纹保留 4096×4096 原图和逐面 UV，金属/皮革/网袋使用对应金属度、粗糙度和颜色，台呢复用项目短绒基底。Unity 资源不依赖本机 Blender 或 Downloads 路径。不是原 Blender 节点的逐节点复制。
- 生成原生 Unity Mesh 和 Prefab，显式统一轴向与米/厘米比例；台呢位于世界 Y=-2.8575，57.15 mm 球静止球心仍为 Y=0，桌脚和房间地板对齐。
- 新桌主库鼻内沿为 2540×1270 mm；模型外框为 2840×1560 mm，桌边顶高 850 mm。库鼻高出台呢 36 mm。
- 碰撞来自导出网格实际库鼻截面，保留 R3 曲线收口和袋口，总计 604 段。六袋编号沿用原项目顺序；捕获半径为实际台面袋孔半径减球半径：角袋 22.925 mm，中袋 24.425 mm。
- 几何配置增加新模型 ID 和固定数据指纹；显示资源也绑定该 ID，拒绝模型与配置混用。没有给装饰桌体添加 MeshCollider 或第二套 Rigidbody。
- 原房间、16 球、球杆和相机保留；候选环境移除旧桌体、旧桌腿，球组 16/10 球显隐与球杆接触点通过原显示检查。原物理叶公式、规则、AI 决策代码没有重写。
- 候选工程使用独立预览存档名；切换工具保留原工程的存档名和产品设置。

## 验证

| 检查 | 结果 |
| --- | --- |
| Unity 编辑模式导入、C# 编译 | 最终进程退出 0 |
| 几何/规则绑定/摆球/碰撞缓存 | 881 项通过 |
| 球/球杆/相机/显示资源 | 301 项通过 |
| 新网格、材质、尺寸、依赖、上沿 UV/袋口曲线及配置配对 | 299 项通过 |
| 原加塞数值回归 | 通过，见 `numeric_checks.json` |
| 原工程保护 | 当前 535 个项目文件 SHA256 保持不变；另有一张旧法线图已缺失，保持原工程现场，仅在候选包中从已校验备份补齐 |
| 替换事务 | 预检、完整写入、完整恢复、注入第 3 文件失败后的自动回滚通过 |
| 模型包 | 67 个资源路径可读取，依赖齐全；不含游戏脚本或配置覆盖 |

三个 AI 候选瞄点中，至少一个从台心到每个袋口具有完整球半径的静态线段净空；实际余量见 `replacement_checks.json` 的 `clearApproaches`。这不是运行 AI 或实际击球的验收，也不表示所有角度都可进球。

没有进入 Play Mode、启动 Player、运行对局、截图或查看渲染。视觉、手感、动态性能由用户试玩确认；本轮没有恢复正式灯光、细绒层或 Windows 构建工作。

## 切换与恢复

先关闭**目标原工程**的 Unity Editor。可先在候选工程观察；修改候选资源后须重新生成替换清单并验证，不能沿用旧指纹。

在本交付目录执行：

```powershell
python Tools/apply_replacement.py
```

此命令只读核对资源包与原工程。确定要正式切换时执行：

```powershell
python Tools/apply_replacement.py --apply
```

工具默认目标是工作区中的 `deliverables/Pool3D-Integrated-V1/UnityProject`；拒绝与本次审查基线不同的文件，自动备份至目标工程旁 `ReplacementBackups/BeforeNewTable-*.zip`。不会替用户停止 Editor。切换后打开原工程，等待编译完成，再运行菜单 `Tools/Pool3D/Check replacement candidate (edit mode)`。此检查不运行对局，但会重建并保存目标整合场景。

若将准备包解压到工作区以外的位置，检查、切换和恢复命令都需加 `--target "目标 UnityProject 的绝对路径"`。

需要恢复时使用切换命令输出的实际备份路径：

```powershell
python Tools/apply_replacement.py --restore "实际的 BeforeNewTable 备份.zip 路径"
```

恢复前也必须关闭目标 Editor；若切换后又手工修改了被替换文件，工具会拒绝覆盖这些后续修改。

## 保留边界

- 装饰桌体仍是显示网格，原有接缝不拿来当物理实体。新库边由现有二维碰撞引擎消费；没有新增三维库鼻冲量、跳球或抬杆。
- R3 模型约 26.5 万三角面，尚未做运行时性能验收或网袋 LOD。台呢当前为短绒基底，正式细绒层、GI/反射/后处理仍属暂停的 M2 工作。
- 旧 `Source/build_geometry.py` 属 V3 历史生成器，不能对新桌重跑。新入口为 `Source/export_blender_table.py`、`build_replacement_geometry.py`，前者需连接保存好的 Blender 文件。
- Unity 项目首次导入需已安装/正常解析其既有 Package Manager 依赖；交付包不包含 Library、PackageCache 或原始许可日志。

新球桌来源为用户提供的 FBX；旧美术/物理资源来源见 `ThirdPartyNotices`。本轮没有更改这些来源的授权。
