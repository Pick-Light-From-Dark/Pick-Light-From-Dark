# 任务系统 (Task System)

任务系统负责管理关卡任务进度、记录卡牌使用明细，并在所有任务完成时触发通关判定。

---

## 核心组件

| 组件 | 路径 | 职责 |
|------|------|------|
| **TaskManager** | `Assets/Scripts/Game/System/TaskManager.cs` | 运行时任务管理，监听 `CardReadComplete` 事件 |
| **TaskGoal** | `Assets/Scripts/Game/Data/TaskGoal.cs` | 单个任务目标的数据结构 |
| **LevelConfigSO** | `Assets/Scripts/Game/Config/LevelConfigSO.cs` | 关卡配置，包含任务清单 (`taskGoals`) |
| **LevelRecordManager** | `Assets/Scripts/Game/Backend/LevelRecordManager.cs` | 关卡记录管理器，持久化任务数据到 JSON |
| **PlayerDataStore** | `Assets/Scripts/Game/Backend/PlayerDataStore.cs` | JSON 文件读写层 |

---

## 文档索引

1. [类图](class-diagram.md) — 核心类关系与数据流向
2. [初始化流程](initialization-flow.md) — 关卡启动时如何初始化各系统
3. [卡牌完成上报流程](card-completion-flow.md) — `CardReadComplete` 事件的处理链路
4. [任务进度同步](progress-sync-flow.md) — UI 如何查询当前任务状态
5. [关卡通关流程](level-complete-flow.md) — 所有任务完成时的处理
6. [关卡失败流程](level-fail-flow.md) — 时间耗尽/情绪超标的失败路径
7. [重开/重试流程](retry-flow.md) — 场景重建与系统重置机制

---

## 关键事件

| 事件 | 参数 | 触发时机 |
|------|------|---------|
| `CardReadComplete` | `int cardId` | 卡牌读条成功 |
| `TaskProgressChanged` | `int cardId` | 单个任务进度变化 |
| `TaskGoalCompleted` | `int targetCardId` | 某个任务目标达成 |
| `LevelComplete` | 无 | 所有任务完成 |
| `GameWin` | 无 | 关卡胜利 |
| `GameLose` | `string reason` | 关卡失败（时间耗尽等） |

---

## 数据持久化

任务数据自动保存到：
```
C:/Users/<用户名>/AppData/LocalLow/DefaultCompany/Pick-Light-From-Dark/player_data.json
```

**数据结构**：
```json
{
  "records": [
    {
      "levelId": 1,
      "timestamp": 1777629843830,
      "isWin": true,
      "timeUsed": 45.3,
      "cardUses": [
        {
          "cardId": 1001,
          "useTime": 10.5,
          "success": true
        }
      ],
      "taskGoals": [
        {
          "targetCardId": 1001,
          "currentCount": 2,
          "targetCount": 2,
          "state": "Completed"
        }
      ]
    }
  ]
}
```
