# 美术来源

- 球台：nicholas-maltbie/PoolSet-Unity，固定提交 adc1e575a7ae6dd7f73046f91471eea8a88504f5，MIT；完整许可位于 UnityProject/Assets/ThirdParty/PoolSet/LICENSE.txt。
- 球杆：原 3D Pool Ball 恢复资源 Cue0/Cue_A_mesh，经 V3 尺寸、网格和木纹适配。它不属于 PoolSet 的 MIT 内容，没有因为本次整合取得额外公开再分发授权。
- 球、台呢、木纹及简化房间表现：复用工作区 V3 制作资源；缺失球号贴图按同一程序 UV 扩展至完整 16 球，未使用 AI 生成图像。
- 加塞算法：沿用 SpinLab 的既有 pooltool C# 适配，Apache-2.0 许可和差异说明保留于本目录 pooltool 子目录。
- Shapely 仅供离线几何生成工具使用，不进入 Unity Player；Source/requirements.txt 记录工具依赖。
