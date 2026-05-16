# CrossLevelSaveSystem 接入接口说明

> 本文档面向接入存档/结局系统的游戏逻辑层（LevelFlowCoordinator、CardManager 等）。
> 6001（太阳照常升起）由第一关剧情选项直接触发，**不经过本系统判定**。

---

## 一、接口清单

### 1. 关卡进度（读档定位）

```csharp
// 剧情节点存档 — 进入/离开剧情时调用
public void SaveStoryProgress(int levelId, string storyFileName, int lineIndex = 0)

// 游玩进度存档 — 进入 gameplay 时调用（参数已简化）
public void SaveGameplayProgress(int levelId)

// 读档 — 返回检查点数据
public LevelCheckpoint LoadCheckpoint()
```

### 2. 卡牌记录（全局快速查询）

```csharp
// 记录卡牌使用 — 卡牌生效时调用
public void RecordCardUsed(int cardId)

// 查询是否使用过指定卡牌
public bool HasUsedCard(int cardId)
```

### 3. 关卡结果（跨关卡结局判定用）

```csharp
// 关卡结束时调用，写入本关最终数据
public void RecordLevelResult(
    int levelId,
    int finalLives,      // 通关时剩余血量
    float timeUsed,      // 本关耗时（秒）
    bool card2017,       // 本关是否使用了分享泡面
    bool card2026        // 本关是否使用了寻求帮助
)

// 获取指定关卡结果（判定前读取）
public LevelResult GetLevelResult(int levelId)
```

### 4. 结局判定（第五关专用）

```csharp
/// <summary>
/// 判定 6002/6004/6005。
/// 6001 由剧情选项直接触发，不调用本接口。
/// </summary>
/// <param name="rooftopChoice">
/// 0 = 未选择（返回 0 表示需弹出木门选项）
/// 1 = 独自前往 → 6004
/// 2 = 邀请宋明月 → 6005
/// </param>
/// <returns>结局 ID（6002/6004/6005），0 表示条件不足或未选择</returns>
public int EvaluateEnding(int rooftopChoice = 0)
```

### 5. 整档操作

```csharp
public void ClearAll()
public bool HasSave()
public string GetSaveSummary()
```

---

## 二、数据结构

```csharp
[Serializable]
public class LevelCheckpoint
{
    public int currentLevelId;    // 当前关卡ID
    public string storyFileName;  // 当前剧情文件名
    public int storyLineIndex;    // 剧情行索引（固定 0 = 回到开头）
    public long saveTime;         // 存档时间戳
    public bool isInGameplay;     // true=游玩中, false=剧情中
}

[Serializable]
public class LevelResult
{
    public int levelId;
    public int finalLives;        // 通关时剩余血量
    public float timeUsed;        // 耗时
    public bool usedCard2017;     // 是否使用分享泡面
    public bool usedCard2026;     // 是否使用寻求帮助
}
```

---

## 三、接入流程

### 3.1 场景初始化

在任意场景（或持久化场景如 `TitleScreen`）中创建空物体，挂载 `CrossLevelSaveSystem` 组件。

```csharp
// 自动单例访问
var save = CrossLevelSaveSystem.Instance;
```

### 3.2 剧情节点存档

在 `LevelFlowCoordinator.StartOpeningStory()` / `StartEndingStory()` 中：

```csharp
CrossLevelSaveSystem.Instance.SaveStoryProgress(
    levelId: 1,
    storyFileName: openingStory.name,
    lineIndex: 0
);
```

### 3.3 卡牌使用记录

卡牌生效时（如 CardManager 的卡牌处理逻辑中）：

```csharp
// 分享泡面（第二关）
CrossLevelSaveSystem.Instance.RecordCardUsed(2017);

// 寻求宋明月帮助（第五关）
CrossLevelSaveSystem.Instance.RecordCardUsed(2026);
```

### 3.4 关卡结束存档

在 `GameWin` / `GameLose` 回调中（如 `LevelFlowCoordinator.OnGameWin()`）：

```csharp
CrossLevelSaveSystem.Instance.RecordLevelResult(
    levelId: currentLevelId,
    finalLives: GameFlowController.Instance.GetCurrentLives(),
    timeUsed: elapsedTime,
    card2017: CrossLevelSaveSystem.Instance.HasUsedCard(2017),
    card2026: CrossLevelSaveSystem.Instance.HasUsedCard(2026)
);
```

### 3.5 第五关结局判定

在玩家使用"前往厕所"卡后调用：

```csharp
int endingId = CrossLevelSaveSystem.Instance.EvaluateEnding(rooftopChoice: 0);

if (endingId == 0)
{
    // 条件满足且两卡全用，需弹出木门选项
    ShowRooftopMenu(); // 【独自前往】 / 【邀请宋明月】
}
else if (endingId == 6002)
{
    // 莫比乌斯环 — 四关血量全为 1
    TriggerEnding(6002);
}
else if (endingId == 6004)
{
    // 星垂之夜 — 未全卡 或 选择独自前往
    TriggerEnding(6004);
}
```

玩家做出木门选择后：

```csharp
// 独自前往
int endingId = CrossLevelSaveSystem.Instance.EvaluateEnding(rooftopChoice: 1); // → 6004

// 邀请宋明月
int endingId = CrossLevelSaveSystem.Instance.EvaluateEnding(rooftopChoice: 2); // → 6005
```

### 3.6 读档流程

主菜单点击"继续游戏"时：

```csharp
if (CrossLevelSaveSystem.Instance.HasSave())
{
    var cp = CrossLevelSaveSystem.Instance.LoadCheckpoint();
    // 加载对应关卡场景
    SceneManager.LoadScene($"Level{cp.currentLevelId}");
    // 进入场景后，LevelFlowCoordinator 读取 cp.storyFileName 启动对应剧情
}
```

---

## 四、判定规则速查

| 结局 | 触发条件 | 数据依赖 |
|------|----------|----------|
| **6001 太阳照常升起** | 第一关剧情选项"不吃" | **剧情直接触发，存档不记录** |
| **6002 莫比乌斯环** | 1/2/3/5关 `finalLives` **全=1** | `levelResults[1,2,3,5]` |
| **6004 星垂之夜** | 至少一关>1 + 未全卡 或 独自前往 | `levelResults[1,2,3,5]` + `cardsUsed` |
| **6005 北极星** | 至少一关>1 + 两卡全用 + 邀请宋明月 | `levelResults[1,2,3,5]` + `rooftopChoice` |

---

## 五、Fungus 桥接（可选）

若剧情需要通过 Fungus 命令调用，场景中挂载 `EndingConditionBridge` 组件：

| Fungus 命令 | 目标对象 | 方法名 | 返回值变量 |
|-------------|----------|--------|------------|
| Call Method | EndingBridge | `RecordCard2017` | — |
| Call Method | EndingBridge | `RecordCard2026` | — |
| Invoke Method | EndingBridge | `HasUsedCard2017` | Boolean: `HasCard2017` |
| Invoke Method | EndingBridge | `CanShowRooftopChoice` | Boolean: `CanShowRooftop` |

> `EndingConditionBridge` 内部已适配新版 `CrossLevelSaveSystem`，通过 `HasUsedCard()` 查询。
