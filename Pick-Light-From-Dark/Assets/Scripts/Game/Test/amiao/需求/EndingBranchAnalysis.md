# 结局分歧点分析 v2

> 本文档基于 2026-05-15 策划新规则重写，分析《Pick Light From Dark》5 个结局在第五关 gameplay 结束后的触发条件、优先级与数据需求。
> 代码参考：GameFlowController (生命值)、CardManager (卡牌 2017/2026)、LevelRecordManager/JsonLevelRecord (存档 JSON)、EmotionSystem (情绪值)。

---

## 一、结局总览

| 结局ID | 结局名称 | 触发关卡 | 触发时机 | 优先级 |
|--------|----------|----------|----------|--------|
| 6001 | 【结局一：太阳照常升起】 | 第一关剧情 | `Dialogue1-1` 选项"不吃" | 剧情分支，与第五关判定无关 |
| 6002 | 【结局二：莫比乌斯环】 | 第五关 | 使用"前往厕所"卡后 | **P0（最高）** |
| 6003 | 【结局三：人心不足蛇吞象】 | 第五关 | — | 当前规则未定义触发条件，待定 |
| 6004 | 【结局四：星垂之夜】 | 第五关 | 使用"前往厕所"卡后 | P1 |
| 6005 | 【结局五：北极星】 | 第五关 | 使用"前往厕所"卡后 | P1 |

> **优先级说明**：第五关使用"前往厕所"卡后，系统按 P0 → P1 顺序判定。一旦匹配高优先级条件，低优先级不再检查。

---

## 二、需要记录的数据

### 2.1 数据来源（JSON 存档）

结局判定依赖 **跨关卡汇总数据**，需从 `PlayerDataFile.records`（即 `JsonLevelRecord` 列表）中提取：

| 数据项 | 来源字段 | 说明 |
|--------|----------|------|
| 第1关通关血量 | `records[i].levelId=1` 的 `isWin=true` 时的 `finalLives` | 需 GameFlowController 在 `GameWin` 时写入 |
| 第2关通关血量 | `records[i].levelId=2` 的 `isWin=true` 时的 `finalLives` | 同上 |
| 第3关通关血量 | `records[i].levelId=3` 的 `isWin=true` 时的 `finalLives` | 同上 |
| 第5关通关血量 | `records[i].levelId=5` 的 `isWin=true` 时的 `finalLives` | 同上 |
| 第1关通关时间 | `records[i].levelId=1` 的 `timeUsed` | 秒，LevelRecordManager 已记录 |
| 第2关通关时间 | `records[i].levelId=2` 的 `timeUsed` | 同上 |
| 第3关通关时间 | `records[i].levelId=3` 的 `timeUsed` | 同上 |
| 第5关通关时间 | `records[i].levelId=5` 的 `timeUsed` | 同上 |
| 第二关是否使用分享泡面卡 | `records[i].levelId=2` 的 `cardUses` 中是否有 `cardId=2017` | CardManager case 2017 |
| 第五关是否使用寻求宋明月帮助 | `records[i].levelId=5` 的 `cardUses` 中是否有 `cardId=2026` | CardManager case 2026 |

### 2.2 当前存档结构缺口

`JsonLevelRecord` 当前已有字段：
- `levelId`, `timeUsed`, `isWin`, `cardUses` (List<CardUseEntry>), `taskGoals`
- 但 **缺少 `finalLives`（通关时剩余生命值）**

**建议补充**（仅分析，后端接口预留）：
```
JsonLevelRecord 新增字段：
  int finalLives = 0;      // 关卡结束时的剩余血量（GameWin/GameLose 时写入）
  int finalEmotion = 0;    // 关卡结束时的情绪值总和（已有 EmotionSystem 可读取）
```

写入时机：
- `LevelRecordManager.EndRecording(bool isWin)` 中，调用 `GameFlowController.Instance.GetCurrentLives()` 和 `EmotionSystem.Instance.GetTotalValue()` 写入。

---

## 三、结局触发条件表（第五关判定）

### 触发时机统一说明

**所有第五关结局（6002/6004/6005）的判定时机**：
> 玩家在第五关 gameplay 中使用 **"前往厕所"卡**（对应卡牌 ID 需确认，暂记为 `goto_toilet`）后，进入结局判定流程。

判定流程：
1. 系统读取跨关卡 JSON 数据（1/2/3/5 关的 `finalLives`、`timeUsed`、卡牌使用记录）
2. 按优先级从高到低匹配条件
3. 匹配成功后调用 `EndingManager.TriggerEnding(id)`

### 3.1 【莫比乌斯环】6002 — P0 最高优先级

| 条件项 | 要求 |
|--------|------|
| 触发关卡 | 第五关 |
| 触发时机 | 使用"前往厕所"卡后 |
| 血量条件 | 第1、2、3、5关通关时的 `finalLives` **全部等于 1** |
| 卡牌条件 | 无要求 |
| 剧情选项 | 无要求 |
| 覆盖规则 | **此条件为强制覆盖**，只要四关血量全为1，不论其他条件如何，直接触发本结局 |

> 设计意图：玩家在所有关卡都以最低血量（1点）通关，体现一种"苟延残喘、循环往复"的压抑感。

### 3.2 【星垂之夜】6004 — P1

**情况A：两卡未全部使用**

| 条件项 | 要求 |
|--------|------|
| 触发关卡 | 第五关 |
| 触发时机 | 使用"前往厕所"卡后 |
| 血量条件 | 第1、2、3、5关通关时的 `finalLives` **至少有一关 > 1** |
| 卡牌条件 | **NOT** (第二关使用了分享泡面卡 2017 **AND** 第五关使用了寻求宋明月帮助 2026) |
| 剧情选项 | 无要求 |

**情况B：两卡全部使用 + 选择"独自前往"**

| 条件项 | 要求 |
|--------|------|
| 触发关卡 | 第五关 |
| 触发时机 | 使用"前往厕所"卡后，木门剧情选项 |
| 血量条件 | 第1、2、3、5关通关时的 `finalLives` **至少有一关 > 1** |
| 卡牌条件 | 第二关使用了分享泡面卡 2017 **AND** 第五关使用了寻求宋明月帮助 2026 |
| 剧情选项 | 木门前选择 **【独自前往】** |

### 3.3 【北极星】6005 — P1

| 条件项 | 要求 |
|--------|------|
| 触发关卡 | 第五关 |
| 触发时机 | 使用"前往厕所"卡后，木门剧情选项 |
| 血量条件 | 第1、2、3、5关通关时的 `finalLives` **至少有一关 > 1** |
| 卡牌条件 | 第二关使用了分享泡面卡 2017 **AND** 第五关使用了寻求宋明月帮助 2026 |
| 剧情选项 | 木门前选择 **【邀请宋明月】** |

> **星垂之夜(6004) 与 北极星(6005) 的互斥关系**：两者血量条件和卡牌条件完全相同，仅在木门选项处分歧。系统在判定到这一步时，不直接触发结局，而是弹出选项，由玩家选择后触发对应结局。

---

## 四、优先级判定流程图

```
第五关使用"前往厕所"卡后
        |
        v
[读取跨关卡JSON数据]
        |
        v
[P0] 检查莫比乌斯环条件
      1/2/3/5关 finalLives 全部 == 1 ?
        |
       是 → 触发 6002 莫比乌斯环（结束）
        |
       否 → 继续
        |
        v
[P1] 检查基础条件
      1/2/3/5关 finalLives 至少有一关 > 1 ?
        |
       否 → 无匹配（可进入兜底/默认逻辑）
        |
       是 → 继续
        |
        v
      第二关使用2017(分享泡面) 且 第五关使用2026(寻求帮助) ?
        |
       否 → 触发 6004 星垂之夜（情况A，结束）
        |
       是 → 弹出木门选项：【独自前往】/【邀请宋明月】
                |
        【独自前往】 → 触发 6004 星垂之夜（情况B）
        【邀请宋明月】 → 触发 6005 北极星
```

---

## 五、JSON 数据结构建议（供后端接口设计）

### 5.1 跨关卡汇总数据（EndingEvaluationData）

建议后端在判定前，从 `PlayerDataFile.records` 聚合出以下结构：

```json
{
  "levelSummary": {
    "1": { "finalLives": 1, "timeUsed": 120.5, "won": true },
    "2": { "finalLives": 2, "timeUsed": 180.0, "won": true, "usedCard2017": true },
    "3": { "finalLives": 1, "timeUsed": 95.0, "won": true },
    "5": { "finalLives": 2, "timeUsed": 210.0, "won": true, "usedCard2026": true }
  },
  "flags": {
    "allLivesEqualOne": false,
    "anyLivesGreaterThanOne": true,
    "usedCard2017": true,
    "usedCard2026": true,
    "bothCardsUsed": true
  }
}
```

### 5.2 建议补充到 JsonLevelRecord 的字段

```csharp
// 现有字段保持不变，新增：
public int finalLives;       // 关卡结束时的剩余血量（GameWin/GameLose 时从 GameFlowController 读取）
public int finalEmotion;     // 关卡结束时的情绪值总和（从 EmotionSystem 读取）
```

写入逻辑建议（LevelRecordManager）：
```
EndRecording(isWin) 中：
  _currentRecord.finalLives = Game.Flow.GameFlowController.Instance.GetCurrentLives();
  _currentRecord.finalEmotion = Game.Emotion.EmotionSystem.Instance.GetTotalValue();
```

### 5.3 卡牌 ID 映射表（供判定使用）

| 卡牌名称 | 卡牌 ID | 记录位置 |
|----------|---------|----------|
| 分享拌面 | 2017 | `JsonLevelRecord.cardUses`（levelId=2 的记录中查找 cardId=2017） |
| 寻求宋明月帮助 | 2026 | `JsonLevelRecord.cardUses`（levelId=5 的记录中查找 cardId=2026） |
| 前往厕所 | 2038 | 触发判定的时机卡 |

---

## 六、后端接口设计建议（留足扩展）

### 6.1 数据层接口（JsonLevelRecord / PlayerDataStore 扩展）

```
// 从存档中提取指定关卡的记录
JsonLevelRecord GetLevelRecord(int levelId)

// 从存档中提取指定关卡的血量（通关时）
int GetLevelFinalLives(int levelId)

// 从存档中提取指定关卡的耗时
float GetLevelTimeUsed(int levelId)

// 检查指定关卡是否使用过某张卡牌
bool HasUsedCard(int levelId, int cardId)

// 聚合跨关卡数据（返回 EndingEvaluationData）
EndingEvaluationData AggregateEndingData(int[] levelIds)
```

### 6.2 判定层接口（EndingBranchSystem 扩展）

```
// 主入口：第五关使用"前往厕所"卡后调用
int EvaluateEnding()

// 判定前准备：加载跨关卡数据（由存档系统注入）
void LoadCrossLevelData(EndingEvaluationData data)

// 检查 P0 条件（莫比乌斯环）
bool CheckMobiusCondition()

// 检查 P1 基础条件（至少一关血量>1）
bool CheckBasicCondition()

// 检查两卡使用情况
bool CheckBothCardsUsed()

// 根据木门选项触发结局（供剧情系统调用）
void TriggerByChoice(string choiceId) // "alone" → 6004, "friend" → 6005
```

### 6.3 事件层接口（EventCenter 扩展建议）

```
// 第五关使用"前往厕所"卡时触发
E_EventType.EndingEvaluationRequested

// 结局判定完成，通知剧情系统显示选项或直接进入结局
E_EventType.EndingEvaluated (int endingId, bool needsChoice)

// 玩家在木门前做出选择
E_EventType.RooftopChoiceMade (string choiceId)
```

---

## 七、条件判定对照表（快速查阅）

| 结局 | 血量 | 卡牌2017 | 卡牌2026 | 木门选项 | 优先级 |
|------|------|----------|----------|----------|--------|
| 6002 莫比乌斯环 | 1,2,3,5关全=1 | — | — | — | P0 |
| 6004 星垂之夜(A) | 至少一关>1 | 未全用 | 未全用 | — | P1 |
| 6004 星垂之夜(B) | 至少一关>1 | 已用 | 已用 | 【独自前往】 | P1 |
| 6005 北极星 | 至少一关>1 | 已用 | 已用 | 【邀请宋明月】 | P1 |
| 6001 太阳照常升起 | — | — | — | — | 第一关剧情分支，独立 |
| 6003 人心不足蛇吞象 | — | — | — | — | 当前规则未定义 |

---

## 八、待确认事项

1. **"前往厕所"卡的具体 ID**：当前 CardManager 中未明确标记此卡，需确认对应 `cardId`。
2. **木门选项的剧本位置**：`Dialogue5-2c/2d/2e` 中哪个文件包含【独自前往】/【邀请宋明月】选项，需确认。
3. **结局三（6003）的触发条件**：当前规则未定义，是否保留或合并到其他结局中，需策划确认。
4. **JsonLevelRecord.finalLives 的补充**：需要后端在 `LevelRecordManager.EndRecording` 中接入 `GameFlowController.GetCurrentLives()`。
5. **时间记录的必要性**：当前规则未使用时间作为判定条件，但策划要求记录。是否用于后续扩展（如速通结局），需确认。
