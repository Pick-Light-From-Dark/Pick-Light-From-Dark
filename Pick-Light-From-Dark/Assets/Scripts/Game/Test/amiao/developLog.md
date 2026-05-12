# 开发日志

## 2026-05-13 结局系统

**功能**：结局系统核心实现
- `EndingDataSO`：ScriptableObject 配置表，支持5个结局数据
- `EndingManager`：单例管理器，提供 `TriggerEnding(int id)` 接口
- `EndingContentPanel`：继承 BasePanel，自动查找子组件并动态创建返回主界面/读取存档按钮
- `EndingTestRunner`：测试脚本，支持按键 1~5 快速触发对应结局

**测试方式**：
1. 在场景任意 GameObject 挂载 `EndingTestRunner`
2. 运行后按数字键 1~5 触发结局 6001~6005
3. 或使用 Inspector 的 ContextMenu 测试

**重要路径**：
- 代码：`Assets/Scripts/Game/Config/EndingDataSO.cs`、`Assets/Scripts/Game/Flow/EndingManager.cs`、`Assets/Scripts/UI/EndingContentPanel.cs`
- 测试：`Assets/Scripts/Game/Test/amiao/EndingTestRunner.cs`
- 配置：`Assets/Resources/Config/EndingData.asset`
- Prefab：`Assets/Resources/UI/Content/EndingContentPanel.prefab`

## 2026-05-13 存档系统

**功能**：扩展现有存档系统，支持结局数据记录与测试验证
- `JsonLevelRecord`：新增 `endingId` 字段，记录触发的结局ID
- `LevelRecordManager`：新增 `RecordEnding(int)` 方法，结局触发时自动记录
- `SaveSystemTestRunner`：测试脚本，模拟生成4条不同关卡的存档记录并验证读写

**测试方式**：
1. 在场景任意 GameObject 挂载 `SaveSystemTestRunner`
2. 按 S 生成模拟存档，按 L 显示所有存档详情，按 C 清除存档，按 T 测试结局记录
3. 或使用 Inspector 的 ContextMenu 测试

**重要路径**：
- 数据结构：`Assets/Scripts/Game/Backend/JsonLevelRecord.cs`
- 记录器：`Assets/Scripts/Game/Backend/LevelRecordManager.cs`
- 测试：`Assets/Scripts/Game/Test/amiao/SaveSystemTestRunner.cs`
- 存档管理：`Assets/Scripts/Game/Backend/PlayerDataStore.cs`

## 2026-05-13 Skip Button 不可见修复

**问题**：`FungusVNController` 中跳过按钮（SkipBtn）在 VN 剧情中不显示，只有存档按钮可见。

**修复**：
- 为 SkipBtn / SaveBtn 动态添加独立 `Canvas` 组件（`overrideSorting = true, sortingOrder = 100`），确保按钮不被其他 UI（如 BgFadeTest 或 SayDialog）遮挡
- `SetSkipButtonVisible` 增加调试日志，方便运行时排查显隐状态

**测试方式**：
1. 挂载 `SkipButtonTest` 到场景中的 FungusVNController 所在 GameObject
2. 运行后按 F1 切换跳过按钮显隐
3. 观察 Console 日志确认 `SetSkipButtonVisible` 调用情况

**重要路径**：
- 修复代码：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`
- 测试脚本：`Assets/Scripts/Game/Test/amiao/SkipButtonTest.cs`
