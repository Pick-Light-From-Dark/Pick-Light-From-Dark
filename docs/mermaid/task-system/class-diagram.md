# 任务系统类图

```mermaid
classDiagram
    %% 定义卡牌基础配置数据
    class 卡牌数据_CardData {
        +int 唯一ID
        +string 卡牌名称
        +string 描述文本
        +string 图标路径
        +List~时间片段~ 时间片段列表
        +int 慌乱值增减
        +int 兴奋值增减
        +int 打断额外慌乱惩罚
        +计算总时长() float
    }

    %% 定义时间片段结构
    class 时间片段_Segment {
        +float 持续时间
        +bool 是否可打断
    }

    %% 定义运行时的卡牌实例
    class 卡牌实例_CardInstance {
        +卡牌数据 配置引用
        +int 实例ID
        +bool 是否已使用
        +bool 是否读条成功
        +float 当前读条进度
        +int 当前片段索引
        +获取当前片段() Segment
        +判断当前能否打断() bool
    }

    %% 定义任务目标
    class 任务目标_TaskGoal {
        +int 目标卡牌ID
        +int 目标次数
        +int 当前次数
        +int 任务状态(0未1进2完)
        +检查是否达成() bool
    }

    %% 定义关卡配置文件 (ScriptableObject)
    class 关卡配置_LevelConfigSO {
        +int 关卡ID
        +float 时间限制
        +int 临界情绪值
        +List~任务目标~ 本关任务清单
        +List~int~ 初始手牌ID列表
    }

    %% 定义任务管理器
    class 任务管理器_TaskManager {
        -List~任务目标~ 活跃任务列表
        +初始化任务(LevelConfigSO)
        +处理卡牌完成事件(int id)
        +检查全关通关判定()
    }

    %% 定义全局事件中心
    class 游戏事件中心_GameEvents {
        <<static>>
        +Action~int~ 卡牌读条成功事件
    }

    %% 建立类之间的关系
    卡牌数据_CardData "1" *-- "多" 时间片段_Segment : 包含
    卡牌实例_CardInstance --> 卡牌数据_CardData : 引用静态数据
    关卡配置_LevelConfigSO "1" *-- "多" 任务目标_TaskGoal : 强包含(SO配置)
    任务管理器_TaskManager --> 任务目标_TaskGoal : 管理与计数
    任务管理器_TaskManager ..> 游戏事件中心_GameEvents : 监听广播
    卡牌实例_CardInstance ..> 游戏事件中心_GameEvents : 发送成功广播
```

# 任务系统时序图（前端接入参考）

## 1. 初始化流程

```mermaid
sequenceDiagram
    autonumber
    participant 调用者 as 调用者（场景入口）
    participant GameFlow as GameFlowController
    participant EventC as EventCenter
    participant Emotion as EmotionSystem
    participant TaskMgr as TaskManager
    participant TeacherAI as TeacherAI
    participant CardSys as CardReadingSystem

    调用者 ->> GameFlow: Initialize(levelConfig)
    GameFlow ->> Emotion: Initialize(levelConfig)
    Emotion ->> Emotion: 设置慌乱值/兴奋值/临界值
    Emotion ->> EventC: EventTrigger(EmotionChanged, info)

    GameFlow ->> TaskMgr: Initialize(levelConfig)
    TaskMgr ->> TaskMgr: _activeGoals.Clear()
    loop 遍历 config.taskGoals
        TaskMgr ->> TaskMgr: 创建 TaskGoal(targetCardId, targetCount)
        TaskMgr ->> TaskMgr: state = InProgress
        TaskMgr ->> TaskMgr: _activeGoals.Add(goal)
    end

    GameFlow ->> TeacherAI: Initialize(levelConfig)
    GameFlow ->> CardSys: Initialize(levelConfig)

    GameFlow ->> EventC: EventTrigger(GameStart)
    GameFlow ->> EventC: EventTrigger(LevelStart, levelId)

    Note over TaskMgr: TaskManager.Start() 同一帧执行<br/>完成 CardReadComplete 事件监听注册
    TaskMgr ->> EventC: AddEventListener(CardReadComplete, OnCardReadComplete)
```

**说明**：`TaskManager.Initialize` 建议在 `GameFlowController.Initialize` 之后调用，确保监听器在首次 `CardReadComplete` 触发前已完成注册。

---

## 2. 卡牌完成上报流程

```mermaid
sequenceDiagram
    autonumber
    participant CardSys as CardReadingSystem
    participant EventC as EventCenter
    participant TaskMgr as TaskManager
    participant GameFlow as GameFlowController

    CardSys ->> CardSys: Update() — 读条计时
    CardSys ->> CardSys: 读条时间到达总时长
    CardSys ->> CardSys: CompleteReading()

    Note over CardSys: 1. 应用卡牌情绪效果
    CardSys ->> EventC: EventTrigger(PanicChanged, delta)
    EventC ->> Emotion: ChangePanic(delta)

    CardSys ->> EventC: EventTrigger(ExciteChanged, delta)
    EventC ->> Emotion: ChangeExcite(delta)

    CardSys ->> EventC: EventTrigger(CardReadComplete, cardId)

    EventC ->> TaskMgr: OnCardReadComplete(cardId)
    TaskMgr ->> TaskMgr: HandleCardCompleted(cardId)

    Note over TaskMgr: 2. 查找匹配 goal
    loop 遍历 _activeGoals
        alt targetCardId == cardId 且 state == InProgress
            TaskMgr ->> TaskMgr: goal.currentCount++
            alt currentCount >= targetCount
                TaskMgr ->> TaskMgr: goal.state = Completed
            end
        end
    end

    TaskMgr ->> TaskMgr: CheckLevelComplete()
    alt 全部 goal 已完成
        TaskMgr ->> EventC: EventTrigger(LevelComplete)
        EventC ->> GameFlow: GameWin()
        GameFlow ->> GameFlow: isGameOver = true
        GameFlow ->> EventC: EventTrigger(GameWin)
    else 未全部完成
        Note over TaskMgr: CheckLevelComplete 静默返回，无事件触发
    end
```

> ⚠️ **注意**：`LevelComplete` 事件在通关路径上由 `TaskManager.CheckLevelComplete` 触发一次。GameFlowController 也会在收到通知后触发 `GameWin`。UI 侧订阅事件时应注意去重。

---

## 3. 任务进度同步（现状 + 推荐 API）

```mermaid
sequenceDiagram
    participant UI as UI面板
    participant TaskMgr as TaskManager
    participant EventC as EventCenter

    Note over UI, TaskMgr: 现状：UI 无法主动查询当前任务进度
    UI ->> TaskMgr: （无公开方法）GetActiveGoals()
    Note right of TaskMgr: _activeGoals 是 private 字段<br/>无任何 public Getter

    UI ->> EventC: 替代方案：监听 CardReadComplete(cardId)
    Note over EventC: 已知 cardId，可推算该任务+1<br/>无法获知总目标数和其他 goal 状态

    Note over UI: 推荐补充以下 3 个 API

    rect rgb(240, 255, 240)
        Note over UI: --- 建议补充的 Public API ---

        GetActiveGoals() → IReadOnlyList<TaskGoal><br/>  用途：UI 渲染任务列表

        GetGoalByCardId(cardId) → TaskGoal<br/>  用途：UI 高亮当前进行中的任务

        GetOverallProgress() → (completed, total)<br/>  用途：UI 显示整体进度条
    end
```

> ⚠️ 当前 `TaskManager` 无任何 public Getter，UI 必须通过事件反向推算进度。建议优先补充 `GetActiveGoals()` 等查询接口。

---

## 4. 关卡通关流程

```mermaid
sequenceDiagram
    autonumber
    participant CardSys as CardReadingSystem
    participant EventC as EventCenter
    participant TaskMgr as TaskManager
    participant GameFlow as GameFlowController
    participant UI as UI面板

    CardSys ->> EventC: EventTrigger(CardReadComplete, cardId)
    EventC ->> TaskMgr: OnCardReadComplete(cardId)
    TaskMgr ->> TaskMgr: HandleCardCompleted()
    TaskMgr ->> TaskMgr: CheckLevelComplete()

    alt 最后一个 goal 刚完成
        TaskMgr ->> EventC: EventTrigger(LevelComplete)
        EventC ->> GameFlow: GameWin()
        GameFlow ->> GameFlow: isGameOver = true
        GameFlow ->> EventC: EventTrigger(GameWin)
        EventC ->> UI: GameWin 通知
        Note over UI: 显示胜利界面
    else 非最后一个 goal
        Note over TaskMgr: CheckLevelComplete 返回 false，无事件触发
    end
```

---

## 5. 关卡失败流程

### 路径 A：时间耗尽

```mermaid
sequenceDiagram
    autonumber
    participant GameFlow as GameFlowController
    participant EventC as EventCenter
    participant UI as UI面板

    GameFlow ->> GameFlow: Update() — 递减 remainingTime
    GameFlow ->> GameFlow: remainingTime <= 0
    GameFlow ->> GameFlow: GameLose("时间耗尽")
    GameFlow ->> GameFlow: isGameOver = true
    GameFlow ->> EventC: EventTrigger(GameLose, "时间耗尽")
    EventC ->> UI: GameLose 通知（reason: 时间耗尽）
    Note over UI: 显示失败界面
```

### 路径 B：情绪超标（手电筒巡逻检查）

```mermaid
sequenceDiagram
    autonumber
    participant TeacherAI as TeacherAI
    participant EventC as EventCenter
    participant GameFlow as GameFlowController
    participant UI as UI面板

    TeacherAI ->> TeacherAI: Update() 状态机巡逻
    TeacherAI ->> TeacherAI: PerformInspectionCheck()
    TeacherAI ->> TeacherAI: CheckPlayerCaught()
    TeacherAI ->> Emotion: IsCaughtByCriticalValue()
    Emotion -->> TeacherAI: (panic + excite) >= criticalValue

    alt 被抓到
        TeacherAI ->> EventC: EventTrigger(PlayerCaught)
        TeacherAI ->> GameFlow: OnPlayerCaught()
        GameFlow ->> GameFlow: PauseGame()
        GameFlow ->> GameFlow: Invoke(ResumeGame, 2f)
        GameFlow ->> EventC: EventTrigger(GamePause)
        TeacherAI ->> TeacherAI: EnterState(Leaving)
        EventC ->> UI: PlayerCaught 通知
        Note over UI: 显示被抓提示，游戏暂停 2 秒
    else 未被抓到
        Note over TeacherAI: 检查通过，巡逻继续
        TeacherAI ->> TeacherAI: EnterState(Leaving)
    end
```

> ⚠️ **关键逻辑**：`EmotionSystem` 本身**不触发** `GameLose`。情绪值超标仅导致 TeacherAI 巡逻时"抓到玩家"并暂停游戏 2 秒，并非直接判负。当前代码中游戏失败仅由时间耗尽触发。

---

## 6. 重开/重试流程

```mermaid
sequenceDiagram
    autonumber
    participant 调用者 as 调用者（重试按钮）
    participant SceneMgr as SceneMgr
    participant TaskMgr as TaskManager
    participant GameFlow as GameFlowController
    participant Emotion as EmotionSystem
    participant EyeClose as EyeCloseSystem
    participant EventC as EventCenter

    调用者 ->> SceneMgr: LoadScene(当前关卡)

    Note over TaskMgr: 场景重建后所有 Singleton 重建<br/>TaskManager.OnDestroy() 自动取消监听注册<br/>TaskManager.Start() 重新注册监听器

    调用者 ->> GameFlow: Initialize(levelConfig)
    GameFlow ->> Emotion: Initialize(levelConfig)

    GameFlow ->> TaskMgr: Initialize(levelConfig)
    TaskMgr ->> TaskMgr: _activeGoals.Clear()
    loop config.taskGoals
        TaskMgr ->> TaskMgr: 新建 TaskGoal, state=InProgress
    end

    GameFlow ->> EyeClose: Reset()
    GameFlow ->> EventC: EventTrigger(GameStart)
    GameFlow ->> EventC: EventTrigger(LevelStart, levelId)
```

> ⚠️ **注意**：`TaskManager` 无独立 `Reset()` 方法。重开依赖场景重建（重新加载关卡场景），所有 Singleton 实例重建并重新执行 `Initialize()`。若需在同一场景内重开（不切换场景），需在 `GameFlowController.Initialize` 中显式调用 `TaskManager.Initialize` 并确保 `_activeGoals` 被清空重建。