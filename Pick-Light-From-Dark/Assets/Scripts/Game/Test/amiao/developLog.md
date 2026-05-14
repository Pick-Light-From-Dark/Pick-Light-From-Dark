# 开发日志

## 2026-05-14 跳过到选项时对话文本同步

**功能**：修复跳过按钮跳到选项位置时，对话框文本未同步更新问题
- `ShowChoice` 中新增向前查找逻辑：从选项行往前遍历 lines，找到最近一句 `对话/旁白/场景` 文本
- 将其 `content` 设置到 `sayDialog.StoryText`，确保跳过到达选项时对话框显示选项前的最后一句内容
- 正常流程不受影响（StoryText 已显示正确内容，再次设置无视觉差异）

**测试方式**：
1. 在任意含选项的 VN 剧情中点击"跳过"按钮
2. 观察对话框是否显示选项前最后一句对话/旁白（而非跳过前的旧文本）

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`（ShowChoice 方法第1371行附近）

## 2026-05-14 无人值守编译错误修复

**功能**：自动巡检并修复 Unity 编译错误
- 读取 `CoplayLogs/last_compile_errors.json` 发现 6 个 CS1022 错误
- 根因：`/// </summary>` 与 `public class` 声明挤在同一行
- 修复文件：`DevModeBase.cs`、`SaveLoadTestRunner.cs`、`FastForwardDevMode.cs`

**测试方式**：
1. 等待 Unity 编译或运行 `tsc` 检查错误列表是否清空
2. 确认 Coplay 面板不再报错

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/DevModeBase.cs`
- 代码：`Assets/Scripts/Game/Test/amiao/SaveLoadTestRunner.cs`
- 代码：`Assets/Scripts/Game/Test/amiao/FastForwardDevMode.cs`

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

## 2026-05-13 居中大字演出 + 选项说话人显示

**功能**：
- `[center_text:xxx]` 指令：隐藏对话框，在画面中央显示 72 号白色大字，3 秒后自动继续
- 选项行继承上一行说话人：第一关选项面板现在显示"陆萤"

**修改内容**：
- `DialogueLine.cs`：新增 `centerText` 字段
- `DialogueParser.cs`：解析 `[center_text:xxx]`，选项行继承上一行 speaker
- `FungusVNController.cs`：`ShowCenterText` / `HideCenterText` 方法，`ShowChoice` 显示 speaker
- `Dialogue1-1.txt`：选项前 `[旁白]` 改为 `陆萤：`

**测试方式**：
1. 在剧情文本末尾添加 `[center_text:巡逻开始 第二夜]`
2. 运行后观察对话框隐藏、居中大字显示
3. 第一关选项处确认 NameText 显示"陆萤"

**重要路径**：
- 数据结构：`Assets/Scripts/Game/System/Dialogue/DialogueLine.cs`
- 解析器：`Assets/Scripts/Game/System/Dialogue/DialogueParser.cs`
- 控制器：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`
- 剧本：`Assets/Resources/Dialogue/Dialogue1-1.txt`

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

## 2026-05-13 [auto] 修复 PlaceholderDisplay 字

**功能**：[auto] 修复 PlaceholderDisplay 字体缺失


## 2026-05-13 [auto] 批量替换: [陆萤] -> 陆萤

**功能**：[auto] 批量替换: [陆萤] -> 陆萤

## 2026-05-14 StoryChainTestRunner 重构：多关分支 + 第五关 + 结局判定

**功能**：重构剧情串联测试器，支持全关分支选项与结局画面自动触发。

**实现内容**：
- `StoryChainTestRunner.cs` 重构：
  - 新增第五关 Prefab 字段（day5_2, day5_2a~2e）
  - 新增 `Day5Branch` 枚举 + `forceDay5Branch` 字段（Inspector 可强制指定第五关分支）
  - 新增 `EndingCondition` 可序列化类（Inspector 可手动配置结局触发条件：生命值/情绪值/卡牌/分支）
  - 新增 `DetermineEnding()` 方法：优先匹配 Inspector 配置条件，兜底根据当前节点 ID 推断结局
  - 路由表覆盖全关：day1~day4 线性连接，day5 分支导向不同结局
  - 结局一（不吃分支）和结局二~五（第五关分支）播放完后自动调用 `EndingManager.TriggerEnding()`
  - `CreateDefaultEndingData()` 自动加载 `Assets/Art/ending/` 下的5张结局图片（Editor 下有效）
- `StoryChainTester.prefab` 更新：添加第五关字段引用 + 默认值配置

**测试方式**：
1. 将 `StoryChainTester.prefab` 拖入场景
2. Inspector 中配置第五关 Prefab（day5_2, day5_2a~2e）
3. 运行后自动从第一关开始播放
4. 第一关选项选择后进入对应分支，第五关结束后自动显示结局画面
5. 或使用 Inspector 的 `forceDay5Branch` 强制测试指定结局

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/StoryChainTestRunner.cs`
- Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/StoryChainTester.prefab`
- 结局图片：`Assets/Art/ending/结局1~5图.png`

## 2026-05-14 文本框坐标调整

**功能**：统一调整所有预制体中姓名文本框向上、对话文本框向下。

**实现**：
- `FungusVNController` 新增 `namePositionAdjusted` / `storyPositionAdjusted` 标志位
- `SetupNameTextAlignment()`：姓名文本 `anchoredPosition.y += 20`
- `SetupStoryTextPosition()`：对话文本 `anchoredPosition.y -= 20`
- 防止重复累加，仅首次生效

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`

## 2026-05-14 PlaceholderTester 脚本缺失修复 + 自动测试

**功能**：修复 PlaceholderTester.prefab 脚本引用丢失，新增自动显示示例。

**修复**：
- `PlaceholderTester.prefab`：脚本 GUID 从 `c470d3a41e5f4794bb9404f1e87dd981` 修复为 `3d00e60daf661bc4887171364efff953`（对应 `PlaceholderTestRunner.cs`）
- `PlaceholderTestRunner.cs`：新增 `autoShowOnStart` 字段，运行后 0.5 秒自动显示一行缺失素材示例文字

**重要路径**：
- Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/PlaceholderTester.prefab`
- 代码：`Assets/Scripts/Game/Test/amiao/PlaceholderTestRunner.cs`

## 2026-05-14 快进开发模式

**功能**：按住空格快进剧情，松手停止。建立开发者功能父类，支持一键禁用。

**实现**：
- `DevModeBase.cs`：抽象基类，所有开发模式功能继承此类
  - `DisableAllDevModes()` 静态方法：一键禁用场景中所有开发模式
  - 运行时自动标记 GameObject 名称为 `[DEV] xxx`
- `FastForwardDevMode.cs`：按住 `Space` 快进，松开停止
  - 自动查找场景中的 `FungusVNController`
  - 调用 `targetVN.ToggleFastForward()` 切换快进状态
  - 完全独立，不影响游戏其他系统

**测试方式**：
1. 挂载 `FastForwardDevMode` 到场景任意 GameObject
2. 运行后按住空格，剧情自动快进；松手即恢复正常速度
3. Inspector 中点击「禁用所有开发模式」可一键关闭

**重要路径**：
- 基类：`Assets/Scripts/Game/Test/amiao/DevModeBase.cs`
- 快进模式：`Assets/Scripts/Game/Test/amiao/FastForwardDevMode.cs`

## 2026-05-14 居中字体调小 + 每关第一段加居中大字

**功能**：调小居中大字字号，为第3/4/5关第一段剧情结尾添加居中大字。

**实现**：
- `FungusVNController` 居中大字 `fontSize` 从 72 调为 48
- `Dialogue3-1.txt` 结尾：添加 `[center_text:夜晚十点四十五分 宿管巡逻 开始了]`
- `Dialogue4-1.txt` 结尾：同上
- `Dialogue5-1.txt` 结尾：同上（替换原有文本行）

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`
- 剧本：`Assets/Resources/Dialogue/Dialogue3-1.txt`、`Dialogue4-1.txt`、`Dialogue5-1.txt`

## 2026-05-14 存档读档测试 Prefab

**功能**：简单 UI 验证存档数据是否被正确记录。

**实现**：
- `SaveLoadTestRunner.cs`：
  - 运行时左上角显示 IMGUI 窗口（F3 切换显隐）
  - 「模拟保存」生成一条带结局分支/卡牌使用/任务目标的测试记录
  - 「读取显示」刷新窗口并打印 Console 日志
  - 「清除存档」调用 `PlayerDataStore.ClearAllRecords()`
- `SaveLoadTester.prefab`：预制体挂载 `SaveLoadTestRunner`

**测试方式**：
1. 将 `SaveLoadTester.prefab` 拖入场景
2. 运行后按 F3 显示 UI
3. 点击「模拟保存」→ 再点击「读取显示」，观察存档数据是否正确

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/SaveLoadTestRunner.cs`
- Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/SaveLoadTester.prefab`

## 2026-05-14 结局分歧点分析文档

**功能**：分析并记录5个结局在游戏操作中的触发位置与推测条件。

**分歧点文档**：见 `Assets/Scripts/Game/Test/amiao/EndingBranchAnalysis.md`

---

## 2026-05-13 跳过逻辑统一：优先跳到选项，无选项则结束并接下一个 prefab

**功能**：统一 `FungusVNController` 跳过按钮行为，使第一关、第五关及所有含选项的剧情段都能正确跳到选项，无选项段则结束并进入下一个 prefab。

**问题**：
- `skipToChoiceIfAvailable` 默认 false，所有 prefab 均未开启（除测试 Prefab 外）
- `LevelFlowCoordinator` 仅在第一关运行时开启，第五关及剧情链测试中的 prefab 点击跳过直接结束，无法跳到选项

**修复**：
- `FungusVNController.OnSkipStory()`：移除对 `skipToChoiceIfAvailable` 的依赖，默认总是从当前行向后扫描第一个 `"选项"` 类型行并跳转
- 若未找到选项，则调用 `EndDialogue()`，由外部 `StoryChainTestRunner` / `LevelFlowCoordinator` 自动接下一个 prefab
- `LevelFlowCoordinator.StartOpeningStory()`：删除 `vnController.skipToChoiceIfAvailable = (levelId == 1)` 运行时设置，逻辑已内聚到控制器

**测试方式**：
1. 将 day1-1.prefab 拖入场景，运行后点击"跳过"，应直接跳到"吃/不吃"选项
2. 将 day5-2.prefab 拖入场景，运行后点击"跳过"，应跳到结局分支选项
3. 将 day2-1.prefab（无选项）拖入场景，运行后点击"跳过"，应结束剧情

**重要路径**：
- 修复代码：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`（`OnSkipStory` 方法）
- 流程协调器：`Assets/Scripts/Game/Flow/LevelFlowCoordinator.cs`

## 2026-05-15 修复 GameScene 自动跳过第一句话

**问题**：`GameScene.unity` / `Level1.unity` 运行时剧情自动跳过第一句话，`day1-1.prefab` 独立测试无此问题。

**根因**：
- `FungusVNController.Start()` 自行调用 `RestartDialogue()` 启动剧情
- `LevelFlowCoordinator.Start()` 也调用 `vnController.RestartDialogue()` 重新初始化
- 剧情驱动被初始化两次：`sayDialog.Say()` 被重复调用，旧的 Writer 状态与新流程交错，导致首句被跳过

**修复**：
- `FungusVNController.Start()`：检测 `LevelFlowCoordinator.Instance != null` 时跳过自动启动，由外部流程协调器统一管理
- `RestartDialogue()`：方法开头增加 `StopAllCoroutines()` + `writer.Stop()`，防止重入时协程/打字机回调污染状态

**测试方式**：
1. 运行 `Level1.unity`（含 LevelFlowCoordinator），开场剧情应从第一句正常开始
2. 直接运行 `day1-1.prefab`（无 LevelFlowCoordinator），仍应自动启动对话

**重要路径**：
- 修复代码：`Assets/Scripts/Game/Test/amiao/FungusVNController.cs`（`Start` 与 `RestartDialogue` 方法）

## 2026-05-15 结局画面功能 Prefab

**任务**：做一个结局画面的功能 prefab，包含重新开始和返回主界面按钮。

**实现**：
- `EndingScreenController.cs`：独立结局画面控制器
  - 显示结局名称 + 描述
  - 重新开始按钮：死亡结局从本关游玩部分开始，其他结局从头开始
  - 返回主界面按钮：加载主菜单场景
  - 预留 `OnRestartRequested` / `OnReturnToMainMenuRequested` 接口供存档系统接入
  - 按钮使用 SpriteSwap 过渡：`deadButton.png` → `deadButtonHover.png`（悬停发红光）
- `EndingScreen.prefab`：独立 Canvas 预制体
  - 全屏黑色背景
  - 标题文本（48px 加粗居中）
  - 描述文本（28px 居中）
  - 两个按钮（240×70），带悬停 SpriteSwap
- `EndingScreenTester.cs`：测试脚本
  - F3 = 显示测试结局
  - F4 = 切换死亡/普通结局模式
  - 数字键 1~5 = 切换结局 ID

**测试方式**：
1. 将 `EndingScreen.prefab` 拖入场景
2. 挂载 `EndingScreenTester.cs` 到空 GameObject
3. 运行后按 F3 显示结局画面，观察按钮悬停效果和文本
4. 按 F4 切换死亡模式，观察重新开始按钮文本变化

## 2026-05-15 结局分支系统设计与后端接口

**任务**：按照 ending.md 设计结局分支系统，结合游玩过程，设计留给后端调用的接口。

**设计**：
- `EndingBranchSystem.cs`：正式结局分支判定系统
  - 接收 `GameplayRecord`（游玩记录：关卡、分支选择、生命值、情绪值、卡牌使用、道具收集）
  - 按优先级匹配 `EndingBranchCondition`（Inspector 可配置）
  - 自动填充默认5结局条件
  - 兜底：根据分支标识推断结局
- `GameplayRecord.cs`：可序列化的游玩记录数据结构
  - 支持存档系统持久化
- `EndingBranchCondition`：Inspector 可配置的结局条件
  - 支持：关卡、分支选择（包含/排除）、生命值范围、情绪值范围、必须/禁止卡牌、关键道具
- 后端接口（供 EndingManager / LevelFlowCoordinator 调用）：
  - `EvaluateEnding()` — 主入口，返回结局ID
  - `EvaluateEnding(GameplayRecord)` — 一次性判定
  - `CanTriggerEnding(int)` — 检查指定结局是否可触发
  - `GetAllPossibleEndings()` — 获取所有满足条件的结局
  - `RecordBranchChoice(string)` — 记录分支选择
  - `RecordCardUsed(int)` — 记录卡牌使用
  - `RecordItemCollected(string)` — 记录道具收集
  - `SetFinalState(int, int)` — 设置最终生命/情绪值
  - `GetGameplayRecord()` — 获取当前游玩记录（供存档）
  - `OnGameplayRecordUpdated` — 游玩数据更新事件（预留存档接口）
- `EndingBranchSystemTester.cs`：IMGUI 测试界面
  - 手动配置游玩记录参数，实时测试结局判定
  - 提供5个结局的快捷测试按钮

**测试方式**：
1. 挂载 `EndingBranchSystem` 到场景空 GameObject
2. 挂载 `EndingBranchSystemTester` 到另一个 GameObject
3. 运行后调整 Inspector 中的分支/生命/情绪/道具，点击"执行判定"
4. 观察 Console 输出的匹配条件和结局ID

**重要路径**：
- 分支系统：`Assets/Scripts/Game/Test/amiao/EndingBranchSystem.cs`
- 测试器：`Assets/Scripts/Game/Test/amiao/EndingBranchSystemTester.cs`
- 分析文档：`Assets/Scripts/Game/Test/amiao/EndingBranchAnalysis.md`

**重要路径**：
- 控制器：`Assets/Scripts/Game/Test/amiao/EndingScreenController.cs`
- 测试器：`Assets/Scripts/Game/Test/amiao/EndingScreenTester.cs`
- Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/EndingScreen.prefab`
- 按钮素材：`Assets/Art/ui/deadButton.png`、`Assets/Art/ui/deadButtonHover.png`

## 2026-05-15 Loading 画面

**任务**：GameScene.unity 剧情到游玩切换时出现透明帧，需要 Loading 画面遮挡。

**实现**：
- `LoadingScreenController.cs`：全屏 Loading 画面控制器
  - 单例模式，自动创建（若场景中不存在）
  - `Show()` / `Hide()`：带淡入淡出动画（默认 0.3s，使用 unscaledDeltaTime 不受 timeScale 影响）
  - `ShowImmediate()` / `HideImmediate()`：无动画立即显隐
  - `SetText(string)`：动态修改 Loading 文字
  - Canvas SortingOrder = 999，overrideSorting = true，确保在最上层
  - 运行时自动创建 Canvas + CanvasScaler + GraphicRaycaster + CanvasGroup + Background Image + Loading Text
- `LoadingScreen.prefab`：预制体版本
  - 全屏黑色背景（Image）
  - 中央白色 "Loading..." 文字（36px）
  - 挂载 `LoadingScreenController`，字段已绑定

**集成说明**（需后端在黑名单代码中加入）：
- `LevelFlowCoordinator.StartGameplay()` 开头：`LoadingScreenController.Show()`
- `UIMgr.Instance.ShowPanel<GamePanel>()` 回调完成时：`LoadingScreenController.Hide()`
- 同理，游玩→剧情切换（OnGameWin/OnGameLose）也可使用 LoadingScreen 遮挡过渡

**测试方式**：
1. 将 `LoadingScreen.prefab` 拖入场景
2. 任意脚本中调用 `LoadingScreenController.Show()` / `Hide()`
3. 观察黑色全屏遮罩与淡入淡出效果

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/LoadingScreenController.cs`
- Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/LoadingScreen.prefab`

## 2026-05-15 跳过功能测试 Prefab

**任务**：验证跳过行为——跳到选项时是否显示选项前对话，且跳过不应越过选项。

**实现**：
- `SkipTestRunner.cs`：运行时 IMGUI 测试窗口
  - 自动查找场景中的 `FungusVNController` 和 `SayDialog`
  - 显示 VN 状态：总行数、当前行、下一选项位置
  - 显示对话框当前文本（姓名 + 内容）
  - 「执行跳过测试」按钮：记录跳过前/后的 NameText 和 StoryText
  - 自动计算预期选项前对话（向前扫描最近一句对话/旁白/场景）
  - 验证结果：绿色=停在选项处 + 文本正确，红色=失败
  - 测试日志窗口，保留最近 50 条记录
- `SkipTester.prefab`：挂载 `SkipTestRunner` 的预制体

**测试方式**：
1. 将 `SkipTester.prefab` 拖入含 VN 剧情的场景
2. 运行后按 F4 显示/隐藏测试窗口
3. 点击「执行跳过测试」，观察跳过后是否停在选项且对话框显示正确内容

**重要路径**：
- 代码：`Assets/Scripts/Game/Test/amiao/SkipTestRunner.cs`
- Prefab：`Assets/Scenes/Amiao_Test/TestPrefabs/SkipTester.prefab`

