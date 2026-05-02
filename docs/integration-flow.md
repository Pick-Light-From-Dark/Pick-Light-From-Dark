# 系统集成说明

## 命名空间结构

```
Game
├── Flow       - 游戏流程控制 (GameFlowController)
├── Config     - 配置数据 (LevelConfigSO)
├── Data       - 数据类 (TaskGoal, CardData, EmotionInfo)
├── Card       - 卡牌系统 (CardReadingSystem)
├── Emotion    - 情绪值系统 (EmotionSystem)
└── Task       - 任务系统 (TaskManager)
```

## GameFlowController 初始化流程

```
GameFlowController.Initialize()
├── EmotionSystem.Initialize()          // 初始化情绪值系统
├── TaskManager.Initialize()            // 初始化任务系统 (阿喵)
│   └── 加载 levelConfig.taskGoals      // 从关卡配置加载任务清单
└── EventTrigger(GameStart)             // 触发游戏开始事件
```

## 事件流：卡牌完成 → 任务进度

```
CardReadingSystem.CompleteReading()
└── EventTrigger(CardReadComplete, cardId)     // 触发卡牌完成事件
    └── TaskManager.OnCardReadComplete(cardId) // TaskManager 监听 (阿喵)
        └── HandleCardCompleted(cardId)        // 处理任务进度
            ├── 更新对应任务目标的 currentCount
            ├── 检查任务是否完成
            └── CheckLevelComplete()           // 所有任务完成后
                └── EventTrigger(LevelComplete) // 触发关卡完成事件
```

## tianpo 的优化应用

### isInitialized 模式
- **GameFlowController**：防止在初始化前执行 Update 倒计时
- **TaskManager**（新增）：防止在初始化前处理卡牌完成事件

### 多实例检测
- 在 GameFlowController.Update 中检测多个实例同时运行
- 使用 Resources.FindObjectsOfTypeAll 进行完全清理

## 系统依赖关系

```
GameFlowController (tianpo)
    ├── EmotionSystem
    └── TaskManager (阿喵)
            └── 监听 CardReadingSystem 的事件
```

## 关键修改

### TaskManager.cs
1. 命名空间从 `Game.System` 改为 `Game.Task`
2. 添加 `isInitialized` 字段
3. 在 `Initialize()` 中设置 `isInitialized = true`
4. 在 `OnCardReadComplete()` 中检查 `isInitialized`

### GameFlowController.cs
1. 添加 `taskManager` 字段引用（使用 `Task.TaskManager`）
2. 在 `Initialize()` 中调用 `taskManager.Initialize(levelConfig)`
