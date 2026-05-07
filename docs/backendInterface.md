# 后端 Prefab 接口文档

> 路径：`Assets/Scenes/Amiao/TestPrefabs/`  
> 配套脚本：`Assets/Scripts/Game/Testing/Runners/`  
> 目标：供策划、前端、其他后端开发者了解各系统测试入口，以便拼装集成。

---

## 一、设计定位

`TestPrefabs` 是《灯下黑》各后端系统的**运行时测试桩**。每个 Prefab 对应一个核心系统，可单独使用，也可多桩组合形成集成测试场景。

**核心原则**：
- 每个 Prefab 都是 `MonoBehaviour` 脚本 + 序列化字段，拖到场景即可运行
- 单例系统（`EmotionSystem`、`PlayerState`、`GameFlowController` 等）在首次 `.Instance` 访问时自动挂载到 `DontDestroyOnLoad`，同一场景中全局唯一
- OnGUI 仅用于开发期调试，正式 UI 由 `Assets/Scripts/UI/Components/` 接管

---

## 二、Prefab 清单与接口

### 1. EmotionSystemTester（情绪系统测试桩）

| 项 | 说明 |
|---|---|
| **系统** | `Game.Emotion.EmotionSystem`（单例） |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/EmotionSystemTester.cs` |
| **对外接口** | `EmotionSystem.Instance.Initialize(config)` / `ChangePanic(delta)` / `ChangeExcite(delta)` / `DecreaseEmotionWhileEyeClose(dt, rate)` / `GetEmotionInfo()` |
| **Inspector 可调** | `panicDelta`、`exciteDelta`、`simulateEyeCloseSeconds`、`decreaseRate`、`littleExcitedThreshold`、`highExcitedThreshold` |
| **快捷键** | F1=Initialize / F2=ChangePanic / F3=ChangeExcite / F4=一键全跑 |
| **角色动画** | 支持根据 `exciteValue` 自动切换 LuYing/LuYingStand 三套动画（Blink/LittleExcited/Excited） |
| **推荐组合** | 可与任意系统组合，情绪值影响教师 AI 判定与角色动画 |

**拼装建议**：
- 任何需要情绪值参与的场景都必须先拖入此 Prefab 并 Initialize
- 角色动画展示依赖 `SpriteRenderer` + `Animator`，由本 Tester 在 `Start()` 中自动创建

---

### 2. EyeCloseSystemTester（闭眼系统测试桩）

| 项 | 说明 |
|---|---|
| **系统** | `Game.EyeClose.EyeCloseSystem`（单例） |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/EyeCloseSystemTester.cs` |
| **对外接口** | `EyeCloseSystem.Instance.Initialize(config)` / `GetEyeCloseDuration()` / `IsTimeAccelerated()` / `Reset()` |
| **联动** | 依赖 `PlayerState.IsEyesClosed()`，闭眼时自动降低情绪值并可能触发时间加速 |
| **快捷键** | F1=Initialize / C=切换闭眼 / F5=Reset |
| **推荐组合** | + `PlayerStateTester` + `EmotionSystemTester` |

**拼装建议**：
- 查寝玩法必须组合 `TeacherAITester` + `PlayerStateTester`
- 闭眼加速效果影响全局 `Time.timeScale`，注意与 `GameFlowController.PauseGame()` 的时序

---

### 3. PlayerStateTester（玩家状态测试桩）

| 项 | 说明 |
|---|---|
| **系统** | `Game.Data.PlayerState`（单例） |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/PlayerStateTester.cs` |
| **对外接口** | `PlayerState.Instance.Initialize(config)` / `SetInBed(bool)` / `SetEyesClosed(bool)` / `ToggleEyesClosed()` / `IsInBed()` / `IsEyesClosed()` |
| **事件输出** | `PlayerEyeCloseChanged` / `EyeCloseStart` / `EyeCloseEnd` |
| **推荐组合** | + `EyeCloseSystemTester` + `TeacherAITester` |

**拼装建议**：
- 所有查寝判定、姿态切换卡牌的终点都依赖 `PlayerState`
- 初始化时建议由外部统一调用 `Initialize(config)`，避免跨关状态残留

---

### 4. CardReadingTester（卡牌读条测试桩）

| 项 | 说明 |
|---|---|
| **系统** | `Game.Card.CardReadingSystem`（场景组件） |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/CardReadingTester.cs` |
| **对外接口** | `CardReadingSystem.Instance.Initialize(config)` / `StartReading(card)` / `InterruptReading()` / `CanInterrupt()` / `IsReading()` / `GetProgress()` |
| **核心机制** | 读条分多段，每段有可打断/不可打断属性；打断后可选是否保存进度 |
| **推荐组合** | + `CardManagerTester` + `TaskSystemTester` + `TeacherAITester` |

**拼装建议**：
- 一张卡牌 = 一个 `CardReadingConfig` + 多段 `ReadingSegment`
- 不可打断段内触发查寝 → 即使闭眼也判定被捕（与 `TeacherAI` 联动）

---

### 5. CardManagerTester（卡牌管理测试桩）

| 项 | 说明 |
|---|---|
| **系统** | `Game.Card.CardManager`（单例） |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/CardManagerTester.cs` |
| **对外接口** | `CardManager.Instance.Initialize(config)` / `AddCard(id)` / `DiscardOtherCards(keepId)` / `GetHandCards()` |
| **推荐组合** | + `CardReadingTester` |

**拼装建议**：
- 关卡初始化时根据 `LevelConfigSO.initialCards` 自动发牌
- 手牌数据供前端 UI 渲染，卡牌使用后通过 `CardReadingSystem` 进入读条

---

### 6. TaskSystemTester（任务系统测试桩）

| 项 | 说明 |
|---|---|
| **系统** | `Game.Task.TaskManager`（单例） |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/TaskSystemTester.cs` |
| **对外接口** | `TaskManager.Instance.Initialize(config)` / `HandleCardCompleted(cardId)` |
| **事件路径** | `CardReadComplete` → `TaskManager` 内部更新 `TaskGoal.currentCount` → 达标后触发 `TaskGoalCompleted` → 最终触发 `LevelComplete` |
| **推荐组合** | + `CardReadingTester` + `GameFlowTester` |

**拼装建议**：
- 通关判定终点：`TaskManager` 确认所有 `TaskGoal` 完成 → 触发 `GameFlowController.OnLevelComplete()`
- 事件订阅由 `GameFlowController` 统一处理，Tester 仅用于调试验证

---

### 7. TeacherAITester（教师 AI 测试桩）

| 项 | 说明 |
|---|---|
| **系统** | `Game.AI.TeacherAI`（场景组件） |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/TeacherAITester.cs` |
| **对外接口** | `TeacherAI.Initialize(config)` / `GetCurrentState()` / `GetCurrentInspectType()` / `GetApproachProgress()` |
| **状态机** | `Idle → Approaching → Inspecting → Leaving → Idle` |
| **查寝类型** | `EyeCheck`（睁眼即被捕） / `FlashCheck`（手电筒，持续累积 panic） |
| **推荐组合** | + `EmotionSystemTester` + `PlayerStateTester` + `CardReadingTester` |

**拼装建议**：
- `TeacherAI` 不是单例，必须在场景中有一个实例（`FirstLevelDevRunner` 会自动创建）
- 教师状态变化事件由前端 UI 订阅，用于显示警告图标

---

### 8. GameFlowTester（游戏流程测试桩）

| 项 | 说明 |
|---|---|
| **系统** | `Game.Flow.GameFlowController`（单例） |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/GameFlowTester.cs` |
| **对外接口** | `GameFlowController.Instance.Initialize(config)` / `PauseGame()` / `ResumeGame()` / `WinGame()` / `LoseGame(string)` / `GetRemainingTime()` / `GetCurrentLives()` |
| **事件** | `GameStart` / `LevelStart` / `GameWin` / `GameLose` / `PlayerCaught` |
| **推荐组合** | 总控用，建议与所有其他 Prefab 组合 |

**拼装建议**：
- 生命周期管理：Initialize → Play → Pause/Resume → Win/Lose
- `OnPlayerCaught` 内部会自动 Pause 2 秒后 Resume（扣除生命）

---

### 9. FirstLevelDevRunner（第一关集成运行器）

| 项 | 说明 |
|---|---|
| **定位** | 不是单系统测试桩，而是**全系统集成入口** |
| **脚本** | `Assets/Scripts/Game/Testing/Runners/FirstLevelDevRunner.cs` |
| **功能** | 自动聚合所有单例系统 + 创建场景组件（TeacherAI / CardReadingSystem / DevModeController / TextGameController） |
| **对外接口** | `InitializeFirstLevel()`：一键写入测试案 v2 参数并初始化所有系统 |
| **使用方式** | 拖入场景 → Play → 自动或按 F1 初始化 |

**拼装建议**：
- 前端开发者可直接用此 Prefab 作为后端入口，无需逐个拖入其他 Tester
- `TextGameController` 提供完整的文字游玩界面（Menu/Playing/GameOver）

---

## 三、如何拼装成完整游戏

### 最小可玩场景（文字版）

只需拖入 **1 个 Prefab**：

```
FirstLevelDevRunner.prefab
```

它会自动：
1. 创建所有单例系统
2. 创建 TeacherAI、CardReadingSystem、TextGameController
3. 写入测试案 v2 参数
4. 提供 OnGUI 文字游玩界面（C 键闭眼、点击手牌使用、教师状态显示）

### 调试用集成场景

拖入以下组合，用于逐系统验证：

```
EmotionSystemTester      ← 情绪值手动调参
PlayerStateTester        ← 姿态/闭眼手动切换
EyeCloseSystemTester     ← 闭眼加速效果验证
CardReadingTester        ← 读条/打断测试
CardManagerTester        ← 手牌管理测试
TaskSystemTester         ← 任务进度验证
TeacherAITester          ← 教师状态机观察
GameFlowTester           ← 总控（暂停/胜利/失败）
```

### 与前端 UI 协作

1. **后端负责**：系统初始化、状态变更、事件触发
2. **前端负责**：订阅 `EventCenter` 事件，渲染 UI
3. **交接点**：各系统的 `Initialize(config)` 和事件常量 `E_EventType`

| 前端需求 | 订阅事件 | 数据来源 |
|---|---|---|
| 显示情绪值条 | `EmotionChanged` | `EmotionSystem.GetEmotionInfo()` |
| 显示教师状态 | `TeacherStateChanged` | `TeacherAI.GetCurrentState()` |
| 显示手牌 | `HandCardsUpdated` | `CardManager.GetHandCards()` |
| 显示读条进度 | `CardReadingProgress` | `CardReadingSystem.GetProgress()` |
| 闭眼遮罩 | `EyeCloseStart` / `EyeCloseEnd` | `PlayerState.IsEyesClosed()` |
| 游戏结束弹窗 | `GameWin` / `GameLose` | `GameFlowController.IsGameOver()` |

---

## 四、目录结构

```
Assets/Scenes/Amiao/TestPrefabs/
├── EmotionSystemTester.prefab          # 情绪系统 + 角色动画
├── EyeCloseSystemTester.prefab         # 闭眼系统
├── PlayerStateTester.prefab            # 玩家状态
├── CardReadingTester.prefab            # 卡牌读条
├── CardManagerTester.prefab            # 卡牌管理
├── TaskSystemTester.prefab             # 任务系统
├── TeacherAITester.prefab              # 教师 AI
├── GameFlowTester.prefab               # 游戏流程总控
└── FirstLevelDevRunner.prefab          # 第一关集成入口

Assets/Scripts/Game/Testing/Runners/    # 对应脚本
├── EmotionSystemTester.cs
├── EyeCloseSystemTester.cs
├── PlayerStateTester.cs
├── CardReadingTester.cs
├── CardManagerTester.cs
├── TaskSystemTester.cs
├── TeacherAITester.cs
├── GameFlowTester.cs
├── FirstLevelDevRunner.cs
├── TextGameController.cs               # 文字游玩界面
└── DevModeController.cs                # 开发模式参数写入
```

---

## 五、已知限制

- OnGUI 样式简陋，仅用于开发期验证；正式 UI 请使用 `Assets/Scripts/UI/Components/` 下的组件
- `CardReadingSystem` 和 `TeacherAI` 是场景组件（非单例），多个 Tester 拖入同一场景时各挂一个，应保持唯一
- 单例系统跨场景存活，切换场景时建议调用各系统的 `Initialize(config)` 重置状态
