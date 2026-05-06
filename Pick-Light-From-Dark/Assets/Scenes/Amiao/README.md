# 后端测试 Prefab 使用说明

> 路径：`Assets/Scenes/Amiao/TestPrefabs/`
> 配套脚本：`Assets/Scripts/Game/Testing/Runners/`
> 默认引用配置：`Assets/Resources/TestData/TestLevelConfig.asset`

## 设计目标

为《灯下黑》后端各系统提供**独立、可输入参数**的运行时测试 Prefab。开发者只需把 Prefab 拖到任意场景即可：

1. 在 Inspector 修改公开字段调参
2. 运行后通过 OnGUI 浮窗按钮 / 快捷键 / 右键 ContextMenu 触发 API
3. 在 Console 查看断言式日志

每个测试 Prefab 都可与其他 Prefab 组合使用（单例系统会被多个 Tester 共享读写），形成集成测试场景。

---

## Prefab 一览

| Prefab | 系统 | 主要测试入口 | 推荐组合 |
| --- | --- | --- | --- |
| `EmotionSystemTester.prefab` | `EmotionSystem` | Initialize / ChangePanic / ChangeExcite / Clamp / DecreaseEmotionWhileEyeClose | 单独 |
| `EyeCloseSystemTester.prefab` | `EyeCloseSystem` | Initialize / Reset / 与 PlayerState 联动 | + PlayerState |
| `PlayerStateTester.prefab` | `PlayerState` | SetInBed / SetEyesClosed / Toggle / 事件计数 | + EyeClose |
| `CardReadingTester.prefab` | `CardReadingSystem` | StartReading / InterruptReading / 进度恢复 | + Emotion + PlayerState |
| `CardManagerTester.prefab` | `CardManager` | Initialize / AddCard / DiscardOtherCards | + CardReading |
| `TaskSystemTester.prefab` | `TaskManager` | Initialize / HandleCardCompleted / 事件路径 | + CardReading |
| `TeacherAITester.prefab` | `TeacherAI` | 状态机时序 / 查寝判定 | + Emotion + PlayerState + CardReading |
| `GameFlowTester.prefab` | `GameFlowController` | Pause / Resume / Win / Lose / OnPlayerCaught | 总控用 |

---

## 通用使用流程

1. 新建空场景或打开 `AmiaoTestScene.unity`。
2. 拖入需要的 Tester Prefab（可以多个并存）。
3. 选中 Prefab 实例，Inspector 中：
   - 确认 `Test Config` 已指向 `TestLevelConfig`（默认已绑）。
   - 按需修改输入字段（如 `panicDelta`）。
4. Play 运行。
5. 用左上角 OnGUI 浮窗或快捷键触发 API，观察 Console。
6. 想自动化跑一批用例，点 "一键跑全部用例"（仅 EmotionSystemTester 提供，其余按场景需要自行组合）。

> ⚠️ 涉及单例的系统（`EmotionSystem` / `PlayerState` / `EyeCloseSystem` / `CardManager` / `TaskManager` / `GameFlowController`）会在第一次访问 `.Instance` 时自动挂到 DontDestroyOnLoad 的物体上。同一场景中只该有一个对应单例。

---

## 测试用例清单

每条用例标注：**前置 / 操作 / 期望**。所有断言通过 Console 日志判断。

### 1. EmotionSystemTester

#### TC-EM-01 Initialize
- 前置：`testConfig` 指向 `TestLevelConfig`（initialPanic=35, initialExcite=35, criticalValue=100）
- 操作：按 F1 或点 "[F1] Initialize 关卡配置"
- 期望：`GetEmotionInfo` 返回 `慌乱:35 兴奋:35 总和:70 临界:100`

#### TC-EM-02 ChangePanic
- 操作：`panicDelta = 10`，按 F2
- 期望：日志 `ChangePanic(10) 35→45`，事件 `PanicChanged` 触发一次

#### TC-EM-03 ChangeExcite
- 操作：`exciteDelta = 10`，按 F3
- 期望：日志 `ChangeExcite(10) 35→45`

#### TC-EM-04 Clamp 边界
- 操作：连续点 ChangePanic(+999)，再点 ChangePanic(-999)
- 期望：上限 100，下限 30；两端均触发对应事件

#### TC-EM-05 临界态切换
- 操作：将 `panicDelta` 调到 +50 后连点，使总和跨越 100
- 期望：首次跨越触发 `EmotionCritical`；回落后触发 `EmotionRecovered`

#### TC-EM-06 闭眼降情绪
- 操作：模拟 `simulateEyeCloseSeconds=2`、`decreaseRate=5`，点对应按钮
- 期望：日志显示 total 减少 10（每秒 5），下界为 60（30+30）

#### TC-EM-07 一键全跑
- 操作：F4
- 期望：依次输出 6 段日志且无 LogError

---

### 2. EyeCloseSystemTester（推荐配 PlayerStateTester）

#### TC-EC-01 Initialize
- 前置：场景中存在 `PlayerState` 单例（拖入 `PlayerStateTester` 即可）
- 操作：按 F1
- 期望：日志 `阈值=10s 倍率x2`（与 testConfig 对齐）

#### TC-EC-02 闭眼计时
- 操作：按 C 切换闭眼，等 3 秒
- 期望：`GetEyeCloseDuration()` 增长，OnGUI 实时显示

#### TC-EC-03 时间加速
- 操作：闭眼持续 ≥10 秒
- 期望：`IsTimeAccelerated()=true`，`Time.timeScale=2`，触发 `EyeCloseTimeAccelerated`

#### TC-EC-04 睁眼恢复
- 操作：再次按 C
- 期望：`IsTimeAccelerated()=false`，`Time.timeScale=1`，触发 `EyeCloseTimeNormal`

#### TC-EC-05 Reset
- 操作：闭眼一段时间后按 F5
- 期望：duration 归零，加速取消

#### TC-EC-06 闭眼降情绪联动
- 前置：拖入 EmotionSystemTester 并 Initialize
- 操作：闭眼 2 秒
- 期望：每秒 panic+excite 各减 1（`eyeClosePanicDecreasePerSec`）

---

### 3. PlayerStateTester

#### TC-PS-01 SetInBed
- 操作：点 SetInBed(true) / SetInBed(false)
- 期望：日志 `玩家上床` / `玩家下床`，相同值不重复触发

#### TC-PS-02 SetEyesClosed
- 操作：点对应按钮
- 期望：触发 `PlayerEyeCloseChanged` 与 `EyeCloseStart` / `EyeCloseEnd`，事件计数器 +1

#### TC-PS-03 Toggle
- 操作：点 ToggleEyesClosed 或按 C
- 期望：状态翻转

#### TC-PS-04 事件去重
- 操作：连续点同一值（如 SetInBed(true) 两次）
- 期望：第二次不输出日志、不触发事件

---

### 4. CardReadingTester

#### TC-CR-01 普通读条完成
- 前置：默认配置（3 段：2s 可打断 + 1s 不可打断 + 2s 可打断），`saveProgressOnInterrupt=false`
- 操作：点 "构造测试卡并 StartReading"，等待 5 秒
- 期望：日志 `读条完成`，触发 `CardReadComplete` 事件，`isUsed=true`

#### TC-CR-02 在可打断段打断
- 操作：StartReading 后立即点 InterruptReading
- 期望：返回 true，触发 `CardReadInterrupt`，慌乱值 +`interruptPanicAdd`

#### TC-CR-03 在不可打断段打断（应失败）
- 操作：等到第 2 段（约 2-3 秒之间）再 InterruptReading
- 期望：返回 false，无事件，无慌乱增加

#### TC-CR-04 进度保存（吃薯片卡）
- 操作：`saveProgressOnInterrupt=true`，StartReading → 1.5 秒后 Interrupt → 再次 StartReading
- 期望：第二次 StartReading 日志显示 `恢复读条进度 时间≈1.50s 片段=0`，剩余 0.5 秒后进入第二段

#### TC-CR-05 完成清进度
- 操作：等读条自然完成
- 期望：`testInstance.currentReadTime=0`，`currentSegmentIndex=0`

#### TC-CR-06 床上状态变化
- 操作：`bedStateChange = LeaveBed`（值=1），完成读条
- 期望：`PlayerState.IsInBed()=false`，`exciteDelta/panicDelta` 同步生效

---

### 5. CardManagerTester

#### TC-CM-01 Initialize 发初始牌
- 前置：`TestLevelConfig.initialCards = [1,2,3]`
- 操作：点 Initialize
- 期望：手牌 3 张，OnGUI 列出每张卡名

#### TC-CM-02 AddCard
- 操作：填入 newCardId / newCardName，点 AddCard
- 期望：手牌 +1

#### TC-CM-03 DiscardOtherCards
- 操作：`keepCardId = 2`，点 DiscardOtherCards
- 期望：仅保留 id=2 的卡，其它从手牌中移除

---

### 6. TaskSystemTester

#### TC-TM-01 Initialize
- 前置：`TestLevelConfig.taskGoals` 至少配 1 个目标
- 操作：点 Initialize
- 期望：OnGUI 显示目标列表，state=InProgress

#### TC-TM-02 直调 HandleCardCompleted
- 操作：填入 simulateCardId 等于某目标的 targetCardId，点 HandleCardCompleted
- 期望：`currentCount +=1`，事件计数 `进度+1`

#### TC-TM-03 事件路径
- 操作：点 EventTrigger CardReadComplete
- 期望：等价于 TC-TM-02，但走 EventCenter

#### TC-TM-04 通关判定
- 操作：把所有目标的 currentCount 推到 ≥ targetCount
- 期望：`TaskGoalCompleted` 触发，最终 `LevelComplete` 触发一次（不重复）

#### TC-TM-05 未初始化保护
- 操作：场景仅放 TaskSystemTester（不点 Initialize），通过事件触发 CardReadComplete
- 期望：日志 `收到卡牌完成事件但未初始化`，状态不变

---

### 7. TeacherAITester（推荐组合 Emotion + PlayerState + CardReading）

#### TC-TA-01 Initialize
- 操作：点 Initialize
- 期望：进入 Idle 状态，stateTimer 在 `patrolIntervals` 内随机

#### TC-TA-02 状态时序
- 操作：保持运行 ≥30 秒
- 期望：依次出现 Idle→Approaching→Inspecting→Leaving→Idle，事件计数 `状态变更` 持续 +1

#### TC-TA-03 眼神查寝（未闭眼）
- 前置：拖入 PlayerStateTester 设 InBed=true、EyesClosed=false
- 操作：等待 Inspecting 阶段
- 期望：判定捕获，`PlayerCaught` 触发，立即转 Leaving

#### TC-TA-04 眼神查寝（卧床闭眼）
- 操作：Inspecting 前切到 InBed=true、EyesClosed=true
- 期望：不触发 PlayerCaught，状态自然进入 Leaving

#### TC-TA-05 手电筒慌乱累积
- 前置：让 InspectType=Flash（多次巡逻后概率上升），EmotionTester 已 Initialize
- 操作：Inspecting 持续 2-3 秒
- 期望：panic 累积增长 ≈ `flashPanicPerSec * dt`（覆盖原 RoundToInt 截断 bug 修复）

#### TC-TA-06 手电筒情绪超临界
- 操作：检查期间手动 ChangePanic 把总和推过 criticalValue
- 期望：判定捕获，`PlayerCaught` 触发

#### TC-TA-07 不可打断片段闭眼无效
- 前置：CardReadingTester 让卡牌停在不可打断片段
- 操作：进入 Inspecting
- 期望：即使闭眼，也判定捕获

---

### 8. GameFlowTester

#### TC-GF-01 Initialize
- 操作：点 Initialize
- 期望：`remainingTime=timeLimit`，触发 `GameStart` + `LevelStart`

#### TC-GF-02 PauseGame / ResumeGame
- 操作：点 Pause → Resume
- 期望：`Time.timeScale` 0→1，事件正确

#### TC-GF-03 GameWin
- 操作：点 GameWin
- 期望：`isGameOver=true`，`GameWin` + `LevelComplete` 触发，后续 Pause/Resume 无效

#### TC-GF-04 GameLose
- 操作：点 GameLose("测试触发")
- 期望：`isGameOver=true`，`GameLose("测试触发")` 触发

#### TC-GF-05 OnPlayerCaught 暂停-恢复
- 操作：点 OnPlayerCaught
- 期望：立即 Pause；2 秒后自动 Resume

#### TC-GF-06 时间耗尽
- 操作：把 `TestLevelConfig.timeLimit` 改成 5，Initialize 后等待
- 期望：5 秒后触发 `GameLose("时间耗尽")`

#### TC-GF-07 GameOver 后无效操作
- 操作：先 GameWin，再点 Pause / Resume
- 期望：日志显示已结束，无状态变化

---

## 集成场景示例

**完整查寝流程测试**（新建场景拖入下面 5 个 Prefab）：

```
EmotionSystemTester
PlayerStateTester
EyeCloseSystemTester
CardReadingTester
TeacherAITester
```

操作序列：
1. 按各 Tester 的 Initialize
2. PlayerState 设置 InBed=true、EyesClosed=true
3. 等待 Teacher 进入 Inspecting
4. 切到 EyesClosed=false → 期望被抓（TC-TA-03）
5. 切回 EyesClosed=true → 期望不被抓
6. 用 CardReadingTester 跑一张含不可打断段的卡，期间触发 Inspecting → 期望即使闭眼也被抓（TC-TA-07）

---

## 已知限制

- OnGUI 在 Editor 与发布版均有效，但样式简陋。仅用于开发期；正式 UI 由 `Assets/Scripts/UI/Components/` 下组件负责。
- `CardReadingTester`、`TeacherAITester` 各自挂 `CardReadingSystem` / `TeacherAI` 组件到自身。一个场景中放多个会产生重复，请保持唯一。
- `TaskSystemTester` 的 "EventTrigger CardReadComplete" 等价于读条完成；若同时存在 `CardReadingTester` 自然完成读条，会双触发，请按需使用。

