# 项目目录参考

快速定位资源，避免每次从头搜索。

## 脚本

| 路径 | 内容 |
|---|---|
| `Assets/Scripts/Framework/` | 框架层：单例、对象池、事件中心、UI管理、音效、场景、资源加载 |
| `Assets/Scripts/Framework/Pool/PoolMgr.cs` | 对象池（池根节点有 DontDestroyOnLoad） |
| `Assets/Scripts/Framework/Music/MusicMgr.cs` | 音效管理器 |
| `Assets/Scripts/Framework/Scene/SceneMgr.cs` | 场景跳转入口（所有跳转必须走这里） |
| `Assets/Scripts/Framework/UI/UIMgr.cs` | UI 面板加载/显示/隐藏 |
| `Assets/Scripts/Framework/UI/BasePanel.cs` | 面板基类 |
| `Assets/Scripts/UI/` | 具体 UI 面板：GamePanel、BeginPanel、TipPanel、EndingContentPanel 等 |
| `Assets/Scripts/UI/Components/EmotionDisplay.cs` | 情绪值文本显示组件 |
| `Assets/Scripts/Game/Emotion/EmotionSystem.cs` | 情绪值系统（慌乱/兴奋，范围 15~50，总和上限 100） |
| `Assets/Scripts/Game/Card/` | 卡牌系统：CardManager、CardGrid、CardSlot、CardDropZone |
| `Assets/Scripts/Game/Data/CardData.cs` | 卡牌数据结构 |
| `Assets/Scripts/Game/Config/LevelConfigSO.cs` | 关卡配置 ScriptableObject |
| `Assets/Scripts/Game/Flow/GameFlowController.cs` | 关卡流程控制器 |
| `Assets/Scripts/Game/AI/TeacherAI.cs` | 老师 AI |
| `Assets/Scripts/Game/EyeClose/EyeCloseSystem.cs` | 闭眼系统 |

## 资源

| 路径 | 内容 |
|---|---|
| `Assets/Art/ui/` | UI 素材（emotion_bar/orange/red.png 等） |
| `Assets/Art/ui/*.meta` | 已配好 Sprite 模式（spriteMode:1, textureType:8） |
| `Assets/Resources/UI/MainFlow/GamePanel.prefab` | 游戏主面板预制体 |
| `Assets/Resources/UI/Content/` | 内容面板预制体 |
| `Assets/Resources/UI/Icon/` | 图标素材（open_eye, close_eye 等） |
| `Assets/Resources/UI/Background/` | 背景图（Bg_5001 等） |
| `Assets/Resources/Sound/` | 音效资源（soundObj.prefab、BGM、音效文件） |
| `Assets/Resources/Dialogue/` | 剧情对话文本（Dialogue1~5.txt、分支对话等） |
| `Assets/Resources/Font/characters.txt` | TMP 字体字符集（见下方"文本收集"说明） |
| `Assets/Resources/Config/` | 关卡配置（LevelConfig_1~5.asset） |
| `Assets/Resources/TestData/` | 测试配置 |
| `Assets/Resources/Card/` | 卡牌相关资源 |

## 场景

| 路径 | 内容 |
|---|---|
| `Assets/Scenes/Level1.unity` ~ `Level5` | 关卡场景 |
| `Assets/Scenes/GameScene.unity` | 主菜单/游戏入口场景 |
| `Assets/Scenes/Tianpo_Test/test_ui.unity` | UI 编辑测试场景 |

## 第三方

| 路径 | 内容 |
|---|---|
| `Assets/ThirdParty/Fungus/` | Fungus 对话系统（有自己的场景加载，不走 SceneMgr） |
| `Assets/ThirdParty/TextMesh Pro/` | TMP |
| `Assets/ThirdParty/Demigiant/` | DOTween |

## 字体文本收集

更新 `characters.txt` 时，需要扫描以下位置的所有中文文本：

| 来源 | 路径 | 说明 |
|---|---|---|
| 对话脚本 | `Assets/Resources/Dialogue/*.txt` | Dialogue1~5.txt 及分支（Dialogue1-1、Dialogue1-2eat、Dialogue1-2noeat、Dialogue2-1、Dialogue2-2 等） |
| 卡牌数据 | `Assets/Resources/Card/*.asset` | 第一夜卡牌（掀开被子、撕开薯片袋等） |
| | `Assets/Resources/Card_Level2/*.asset` | 第二夜卡牌（泡面相关） |
| | `Assets/Resources/Card_Level3/*.asset` | 第三夜卡牌（手机/面包相关） |
| | `Assets/Resources/Card_Level5/*.asset` | 第五夜卡牌（厕所/天台相关） |
| 关卡配置 | `Assets/Resources/Config/LevelConfig_*.asset` | 关卡名称（第一夜~第五夜）、关联对话文件名 |
| 结局数据 | `Assets/Resources/Config/EndingData.asset` | 结局名称和描述文本 |
| 对话配置 | `Assets/Resources/UI/Dialogue/*.asset` | RoleConfig（角色名）、BackgroundConfig（背景名） |
| C# 脚本 | `Assets/Scripts/**/*.cs` | UI 面板中的硬编码中文、Header/Tooltip 注释等 |

**更新流程**：收集完字符后，在 Unity 编辑器中运行 `Tools → 《灯下黑》 → 更新TextMeshPro字体资源` 重新生成 `wenkai.asset`。

## 文档

| 路径 | 内容 |
|---|---|
| `docs/checklist.md` | 检查清单（场景跳转必须走 SceneMgr 等） |
