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

## 2026-05-13 音效未加载修复

**问题**：`FungusVNController` 中 `[se:xxx]` / `[bgm:xxx]` 指令只能播放 Inspector 预配置的音频，若映射列表为空则显示占位符，无法动态加载 Resources 下的音效。

**修复**：
- `FungusVNController`：SE/BGM 播放增加 Resources 动态加载回退
  - SE 回退路径：`Sound/sound/{name}` → `Sound/sound/DXH_SOUND/{name}` → `Audio/SFX/{name}` → `Audio/SFX/DXH_SOUND/{name}`
  - BGM 回退路径：`Sound/BkMusic/{name}` → `Audio/Music/{name}`
  - Editor 额外回退 `AssetDatabase.LoadAssetAtPath` 从 `Assets/Audio/` 和 `Assets/Resources/` 加载
- 新增 `SoundTestRunner`：支持按键 1/2/3/4 测试直接播放、MusicMgr、VN 动态加载

**测试方式**：
1. 挂载 `SoundTestRunner` 到场景任意 GameObject
2. 运行后观察 Console 自动测试输出
3. 或按键 1~4 分别测试不同加载路径

**重要路径**：
- 修复代码：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`（`LoadAudioClip` 方法 + SE/BGM 播放逻辑）
- 测试脚本：`Assets/Scripts/Game/Test/amiao/SoundTestRunner.cs`
- 音效资源：`Assets/Resources/Sound/sound/`、`Assets/Audio/SFX/`

## 2026-05-13 演出效果增强

**功能**：参考 Dialogue1.txt 演出方式，为 Dialogue2/3/4 增强画面与音效演出，不修改对话原文。

**Dialogue2.txt 增强**：
- 电话场景：`[se:DXH_SOUND/07.对话声]` 环境音
- 挂断瞬间：`[se:按钮点击音效]` + `[hide_dialog]` / `[wait:2]` / `[show_dialog]` 停顿
- 回寝：`[se:DXH_SOUND/04.移动被子]`
- 吃面：黑屏转场 → `[bg:the spread quilt]` → `[bg:quilt aside]` + `[se:DXH_SOUND/04.移动被子]`
- 巡逻：`[se:DXH_SOUND/08.脚步声]`

**Dialogue3.txt 增强**：
- 自省段落：`[hide_dialog]` / `[wait:2]` / `[show_dialog]`
- 熄灯：`[bg:indoor light]`
- 翻柜子：`[bg:cabinet _ open _ backpack]`
- 失眠：`[bg:night_room]` + `[se:DXH_SOUND/06.深呼吸]`
- 巡逻：`[se:DXH_SOUND/08.脚步声]`
- 结尾：`[solid:black,fade,1]` 沉思氛围

**Dialogue4.txt 增强**：
- 聊天：`[se:DXH_SOUND/07.对话声]` + `[bg:寝室]`
- 熄灯：`[hide_dialog]` / `[wait:3]` / `[show_dialog]`
- 巡逻：`[bg:night_room]` + `[se:DXH_SOUND/08.脚步声]`

**素材引用**：
- 图片：`the spread quilt`、`quilt aside`、`indoor light`、`cabinet _ open _ backpack`、`night_room`、`寝室`（均来自 `Assets/Art/Scene/`、`Assets/Art/DialogueTestArt/`）
- 音效：`07.对话声`、`08.脚步声`、`06.深呼吸`、`04.移动被子`、`按钮点击音效`（均来自 `Assets/Audio/SFX/`、`Assets/Resources/Sound/sound/`）

**重要路径**：
- 剧本：`Assets/Resources/Dialogue/Dialogue2.txt`、`Dialogue3.txt`、`Dialogue4.txt`

## 2026-05-13 Dialogue5 格式规范化 + 剧情分支测试 Prefab

**功能**：Dialogue5.txt 文本格式规范化，新增结局分支测试 Prefab。

**Dialogue5.txt 修改**：
- `(隐藏对话框3秒)` → `[hide_dialog]` / `[wait:3]` / `[show_dialog]`
- `[bg:black_sence](隐藏对话框3秒)` → 拆分为 `[bg:black_sence]` + `[hide_dialog]` / `[wait:3]` / `[show_dialog]`
- `[bg:]：夜晚十点四十五分...` → `[场景]：...` + `[bg:]`
- 四个分支剧情前添加 `[block:xxx]` 段落标记

**新增 EndingBranchTestRunner**：
- 运行时左上角显示 IMGUI 菜单窗口（F2 切换显隐）
- 支持一键触发 6002~6005 四个结局分支
- 支持「从头播放」「快进模式」快捷操作
- 自动初始化 EndingManager + 内置5个结局数据

**新增 SimpleSkipButton**：
- 不依赖 FungusVNController，独立创建 Canvas + Button
- 运行时自动显示跳过按钮，点击仅打印日志（无实际功能）
- 用于验证按钮在场景中是否可见

**重要路径**：
- 剧本：`Assets/Resources/Dialogue/Dialogue5.txt`
- 分支测试：`Assets/Scripts/Game/Test/amiao/EndingBranchTestRunner.cs`
- 分支 Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/EndingBranchTester.prefab`
- 简易按钮：`Assets/Scripts/Game/Test/amiao/SimpleSkipButton.cs`
- 按钮 Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/SimpleSkipButton.prefab`

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

## 2026-05-13 SimpleSkipButton UI 修复

**功能**：修复 SimpleSkipButton 巨大白色方块 + 字体缺失问题

**修复内容**：
- TMPro.TextMeshProUGUI → Legacy Text（避免字体材质缺失导致的方块字）
- Resources.GetBuiltinResource Sprite → 纯色背景（避免内置资源不存在导致纯白巨块）
- 增加 delayCreate 字段，支持延迟创建（中和 SkipButtonTest 的延迟加载优点）
- 字体加载逻辑与 FungusVNController.CreateButton 一致（Resources/Font/LXGWWenKaiScreen 或 文软雅黑）

**测试**：挂载 SimpleSkipButton 到空 GameObject，运行场景，右上角应显示正确中文字体的跳过按钮

**代码路径**：`Assets/Scripts/Game/Test/amiao/SimpleSkipButton.cs`


## 2026-05-13 Dialogue5 旁白/场景改为陆萤心里话

**功能**：将 Dialogue5.txt 中的旁白和场景描述统一改为陆萤的心里话

**修改内容**：
-  → （共 47 处）
-  → （共 1 处）
- 所有旁白/场景内容用  包裹，表示陆萤内心独白

**重要路径**：
- 剧本：


## 2026-05-13 Dialogue5 旁白/场景改为陆萤心里话

**功能**：将 Dialogue5.txt 中的旁白和场景描述统一改为陆萤的心里话

**修改内容**：
- [旁白]： -> [陆萤]：（...）（共 47 处）
- [场景]： -> [陆萤]：（...）（共 1 处）
- 所有旁白/场景内容用 （） 包裹，表示陆萤内心独白

**重要路径**：
- 剧本：Assets/Resources/Dialogue/Dialogue5.txt


## 2026-05-13 Dialogue2~5 剧情文本分段

**功能**：按关卡流程（上段-游玩-下段）对 Dialogue2~5 进行分段，更新 LevelConfig 引用。

**分段结果**：
- Dialogue2-1.txt：上段（电话~熄灯前），Dialogue2-2.txt：下段（结尾剧情）
- Dialogue3-1.txt：上段（宿舍闲聊~巡逻开始），Dialogue3-2.txt：下段（深夜孤独~入睡）
- Dialogue4-1.txt：上段（放假聊天~巡逻开始），Dialogue4-2.txt：下段（次日期待）
- Dialogue5-1.txt：上段（周一回校~巡逻开始），下段为四个分支结局保留在原 Dialogue5.txt

**LevelConfig 更新**：
- LevelConfig_2：pre→Dialogue2-1，post→Dialogue2-2
- LevelConfig_3：pre→Dialogue3-1，post→Dialogue3-2
- LevelConfig_5：pre→Dialogue5-1，isChoiceLevel=1

**重要路径**：
- 分段文本：`Assets/Resources/Dialogue/Dialogue2-1.txt` ~ `Dialogue5-1.txt`
- 配置：`Assets/Resources/Config/LevelConfig_2.asset`、`LevelConfig_3.asset`、`LevelConfig_5.asset`

## 2026-05-13 FungusVN_1~4.prefab 跳过按钮默认可见

**功能**：复刻 SimpleSkipButton 可见性逻辑到 FungusVN_1~4.prefab。

**实现**：
- `FungusVNController` 新增 `showSkipButtonOnStart` bool 字段
- `Start()` 中若启用则自动调用 `SetSkipButtonVisible(true)`
- FungusVN_1~4.prefab 全部启用该字段，运行时跳过按钮自动显示

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`
- Prefab：`Assets/Scenes/Amiao_Test/FungusVN_1.prefab` ~ `FungusVN_4.prefab`

## 2026-05-13 Dialogue1 分段文本更新 + 全关分段 Prefab

**功能**：按 Dialogue1.txt 最新演出效果改写旧分段文本，并为全关创建分段 Prefab。

**Dialogue1 文本更新**：
- `Dialogue1-1.txt`：上段（light_room → 选项提示），含 fade/hide_dialog/solid:black 等演出指令
- `Dialogue1-2eat.txt`：'吃'分支（心情变好 → 新手关卡结束）
- `Dialogue1-2noeat.txt`：'不吃'分支（塞进课桌 → 结局一）

**新增分段 Prefab（10个）**：
| 关卡 | Prefab | 引用文本 |
|---|---|---|
| 1 | FungusVN_1-1 | Dialogue1-1.txt |
| 1 | FungusVN_1-2a | Dialogue1-2eat.txt |
| 1 | FungusVN_1-2b | Dialogue1-2noeat.txt |
| 2 | FungusVN_2-1 | Dialogue2-1.txt |
| 2 | FungusVN_2-2 | Dialogue2-2.txt |
| 3 | FungusVN_3-1 | Dialogue3-1.txt |
| 3 | FungusVN_3-2 | Dialogue3-2.txt |
| 4 | FungusVN_4-1 | Dialogue4-1.txt |
| 4 | FungusVN_4-2 | Dialogue4-2.txt |
| 5 | FungusVN_5-2 | Dialogue5.txt（含四个结局分支） |

**重要路径**：
- 文本：`Assets/Resources/Dialogue/Dialogue1-1.txt` ~ `Dialogue1-2noeat.txt`
- Prefab：`Assets/Scenes/Amiao_Test/FungusVN_*-*.prefab`

## 2026-05-13 FungusVN_5.prefab

**功能**：新增第五关剧情演出 Prefab，用于测试 Dialogue5 剧情流程。

**实现**：
- 复制 FungusVN_4.prefab 并修改
- 名称改为 FungusVN_5
- dialogueText 引用 Dialogue5.txt（完整剧情含四个分支结局）
- 启用 showSkipButtonOnStart，运行时跳过按钮自动显示

**重要路径**：
- Prefab：`Assets/Scenes/Amiao_Test/FungusVN_5.prefab`
- 剧本：`Assets/Resources/Dialogue/Dialogue5.txt`

## 2026-05-13 占位文字 Prefab 测试

**功能**：独立素材缺失占位文字显示器 + 测试套件

**新增 PlaceholderDisplay**：
- 独立组件，不依赖 FungusVNController
- 运行时自动创建 Canvas（ScreenSpaceOverlay, sortingOrder=999）
- 左上角显示缺失素材列表（Image / SFX / BGM）
- Show(type, name) 方法，自动去重
- Clear() 方法清除所有占位文字
- 黄色文字 + 黑色描边，避免与背景撞色

**新增 PlaceholderTestRunner**：
- 测试脚本，挂载后按 I/S/B/C 分别测试图片/音效/BGM缺失/清除
- 支持 Inspector ContextMenu 测试
- 自动创建 PlaceholderDisplay（若场景中不存在）

**测试方式**：
1. 将 PlaceholderTester.prefab 拖入场景
2. 运行后按 I/S/B 触发占位文字
3. 按 C 清除

**重要路径**：
- 核心组件：`Assets/Scripts/Game/Test/amiao/PlaceholderDisplay.cs`

## 2026-05-13 Dialogue5 结局分支分段文本 + Prefab

**功能**：将 Dialogue5.txt 的4个结局分支提取为独立文本和 Prefab，方便单独测试。

**分段结果**：
- `Dialogue5-2a.txt` / `day5-2a.prefab`：迷茫结局（结局二：莫比乌斯环）
- `Dialogue5-2b.txt` / `day5-2b.prefab`：网吧结局（结局三：人心不足蛇吞象）
- `Dialogue5-2c.txt` / `day5-2c.prefab`：天台结局（结局四：星垂之夜）
- `Dialogue5-2d.txt` / `day5-2d.prefab`：可选结局-独自前去（结局四：星垂之夜）
- `Dialogue5-2e.txt` / `day5-2e.prefab`：可选结局-邀友同行（结局五：北极星）

**测试方式**：
1. 将对应 Prefab 拖入场景
2. 运行后直接播放该分支剧情

**重要路径**：
- 分支文本：`Assets/Resources/Dialogue/Dialogue5-2a.txt` ~ `Dialogue5-2e.txt`
- 分支 Prefab：`Assets/Scenes/Amiao_Test/day5-2a.prefab` ~ `day5-2e.prefab`
- 测试脚本：`Assets/Scripts/Game/Test/amiao/PlaceholderTestRunner.cs`
- Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/PlaceholderTester.prefab`
