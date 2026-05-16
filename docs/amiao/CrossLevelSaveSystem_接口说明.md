这里是将原文转换为 Markdown 格式后的内容，在保证内容完全不变的前提下，优化了换行引起的分段，并使用了标准的 Markdown 语法（如代码块、表格、列表等）进行排版：

---

# CrossLevelSaveSystem（跨关卡进度存档）

* **文件**：`Assets/Scripts/Game/Test/amiao/CrossLevelSaveSystem.cs`
* **存储**：`PlayerPrefs`（键名 `CrossLevelSave_v2`），JSON 序列化
* **单例访问**：`CrossLevelSaveSystem.Instance`

---

## 核心数据结构

```text
CrossLevelSaveData
├── checkpoint: LevelCheckpoint        // 关卡进度定位
│   ├── currentLevelId                 // 当前关卡
│   ├── storyFileName                  // 剧情文件名（如 Dialogue2-1）
│   ├── storyLineIndex                 // 剧情行号
│   └── isInGameplay                   // true=游玩中 / false=剧情中
├── endingData: EndingAccumulatedData  // 结局累积数据
│   └── cardsUsed: List<int>           // 全局卡牌使用记录（去重）
└── levelResults: List<LevelResult>    // 每关最终结果
    ├── levelId
    ├── finalLives                     // 通关剩余血量
    ├── usedCard2017                   // 是否使用卡牌2017（分享泡面）
    └── usedCard2026                   // 是否使用卡牌2026（寻求帮助）

```

---

## 后端接口

* **接口**: `SaveStoryProgress`
* **参数**: `int levelId, string storyFile, int lineIndex=0`
* **说明**: 保存剧情进度（剧情→游玩切换时调用）


* **接口**: `SaveGameplayProgress`
* **参数**: `int levelId`
* **说明**: 保存游玩进度（游玩中存档）


* **接口**: `LoadCheckpoint`
* **参数**: 无
* **说明**: 返回 `LevelCheckpoint`，读档定位用


* **接口**: `RecordCardUsed`
* **参数**: `int cardId`
* **说明**: 记录卡牌使用（全局去重）


* **接口**: `HasUsedCard`
* **参数**: `int cardId`
* **说明**: 查询是否使用过某卡牌


* **接口**: `RecordLevelResult`
* **参数**: `int levelId, int lives, bool card2017, bool card2026`
* **说明**: 记录关卡结果（通关后调用）


* **接口**: `GetLevelResult`
* **参数**: `int levelId`
* **说明**: 获取指定关卡结果


* **接口**: `EvaluateEnding`
* **参数**: `int rooftopChoice=0`
* **说明**: 结局判定：0=未选, 1=独自(6004), 2=邀请(6005)


* **接口**: `HasSave`
* **参数**: 无
* **说明**: 是否有有效存档


* **接口**: `ClearAll`
* **参数**: 无
* **说明**: 清除所有跨关卡存档



---

## 结局判定规则（EvaluateEnding）

| 优先级 | 条件 | 结局 |
| --- | --- | --- |
| **P0** | 1/2/3/5关 finalLives 全=1 | 6002 莫比乌斯环 |
| **P1** | 至少一关 finalLives > 1，但未全用卡 | 6004 星垂之夜 |
| **P2** | 两卡全用 + 独自前往 | 6004 星垂之夜 |
| **P3** | 两卡全用 + 邀请宋明月 | 6005 北极星 |

> ▎ 6001（太阳照常升起）由第一关剧情选项直接触发，不经过本接口。

---

## 使用示例（来自 AmiaoDemoRunner）

```csharp
// 剧情结束时存档
saveSystem.SaveStoryProgress(2, "Dialogue2-1", 0);

// 记录卡牌使用
saveSystem.RecordCardUsed(2017);

// 通关后记录结果
saveSystem.RecordLevelResult(2, selectedLives, card2017: true, card2026: false);

// 第五关结束后判定结局
int endingId = saveSystem.EvaluateEnding(rooftopChoice: 2); // 6005

// 读档
var cp = saveSystem.LoadCheckpoint();
// cp.currentLevelId, cp.storyFileName, cp.isInGameplay

```

---

## 与 PlayerDataStore 的区别

|  | CrossLevelSaveSystem | PlayerDataStore |
| --- | --- | --- |
| **用途** | 当前游戏进度（读档用） | 历史通关记录（统计用） |
| **存储** | PlayerPrefs | player_data.json |
| **时机** | 任意节点（剧情中/游玩中） | 关卡结束后 |
| **数据** | 检查点 + 结局累积 | 每次游玩的完整明细 |