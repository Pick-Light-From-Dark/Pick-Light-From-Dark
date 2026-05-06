# 《灯下黑》第一关完整测试文档

> 配套 Prefab：`Assets/Scenes/Amiao/TestPrefabs/FirstLevelDevRunner.prefab`
> 配套脚本：`Assets/Scripts/Game/Testing/Runners/FirstLevelDevRunner.cs`
> 开发者面板：`Assets/Scripts/Game/Testing/Runners/DevModeController.cs`

---

## 快速开始

1. 新建空场景（或打开 `AmiaoTestScene.unity`）
2. 拖入 `FirstLevelDevRunner.prefab`
3. Play 运行 → 自动初始化第一关（测试案v2 参数）
4. 按 `` ` `` 键切换开发者面板

---

## 第一关参数（测试案v2）

| 参数 | 值 | 说明 |
|------|-----|------|
| levelId | 1001 | 第一夜 |
| timeLimit | 600s | 10分钟 |
| maxLives | 2 | 2条命 |
| initialInBed | true | 开局在床上 |
| initialPanic | 15 | 初始慌乱 |
| initialExcite | 15 | 初始兴奋 |
| criticalValue | 80 | 情绪临界值 |
| patrolIntervals | 15~25s | 巡逻间隔 |
| patrolTime | 8~10s | 接近时间 |
| eyeCheckDuration | 3s | 眼神检查时长 |
| flashCheckDuration | 3s | 手电筒检查时长 |
| flashPanicPerSec | 2 | 手电筒每秒慌乱增长 |
| initialCards | [2001, 2010] | 初始手牌 |
| taskGoals | 2008 x1 | 吃薯片任务 |

---

## 核心流程测试

### TC-FL-01 自动初始化
- 操作：拖入 Prefab 后直接 Play
- 期望：Console 输出 `第一关初始化完成`，左上角显示运行器面板，右上角显示开发者面板

### TC-FL-02 教师巡逻周期
- 操作：Play 后等待 ≥60 秒
- 期望：状态依次出现 Idle→Approaching→Inspecting→Leaving→Idle，Console 有对应日志

### TC-FL-03 眼神查寝（被抓）
- 前置：保持 InBed=true、EyesClosed=false
- 操作：等待 Teacher 进入 Inspecting(Eye)
- 期望：PlayerCaught 触发，生命减 1，暂停 2 秒后恢复

### TC-FL-04 眼神查寝（安全）
- 前置：InBed=true、EyesClosed=true
- 操作：等待 Teacher 进入 Inspecting(Eye)
- 期望：无 PlayerCaught，自然进入 Leaving

### TC-FL-05 手电筒情绪累积
- 前置：EmotionSystem 已初始化
- 操作：等待 Flash 检查，持续 3 秒不睁眼
- 期望：panic 值增长 ≈ 6（3s x 2/s），Console 显示慌乱增长日志

### TC-FL-06 手电筒超临界被抓
- 操作：在 Flash 检查前，用 DevMode 将 panic+excite 总和调至 ≥80
- 期望：进入 Flash Inspecting 后立即 PlayerCaught

### TC-FL-07 不可打断片段闭眼无效
- 前置：用 CardReadingTester 启动一张含不可打断段的卡
- 操作：在不可打断片段期间，保持 EyesClosed=true
- 期望：进入 Inspecting 后仍判定被抓

### TC-FL-08 吃薯片任务通关
- 操作：用 DevMode 添加卡牌 2008（吃薯片），完成读条
- 期望：TaskGoalCompleted → LevelComplete → GameWin

### TC-FL-09 时间耗尽失败
- 操作：用 DevMode `SetTime(5)`，等待 5 秒
- 期望：GameLose("时间耗尽")

### TC-FL-10 生命值耗尽
- 操作：连续两次在 Eye 检查时不闭眼（TC-FL-03 x2）
- 期望：第二次被抓后 GameLose("生命值耗尽")

---

## 开发者面板功能测试

| 功能 | 操作 | 验证 |
|------|------|------|
| 时间跳跃 | 点 +30秒 / -30秒 | 剩余时间变化 |
| 直接设时间 | 改 setRemainingTime，点「直接设置」 | 时间立即生效 |
| 改情绪值 | 改 Panic/Excite，点「设置」 | 颜文字变化 |
| 改玩家状态 | 改 InBed/EyesClosed，点「应用」 | 状态切换 |
| 加卡牌 | 改 AddID，点「添加」 | 手牌增加 |
| 强制教师状态 | 点「跳到Approaching/Inspecting」 | 状态立即切换 |
| 颜文字显示 | 观察左上角 | 6 级 kaomoji 随情绪变化 |

颜文字分级：
- ratio ≥1.0: `(╯°□°)╯` 超临界
- ratio ≥0.8: `(；´Д｀)` 高
- ratio ≥0.6: `(；一_一)` 中高
- ratio ≥0.4: `(￣▽￣)` 中
- ratio ≥0.2: `(｡♥‿♥｡)` 低
- ratio <0.2: `(✿◠‿◠)` 很低

---

## 音频占位钩子

TeacherAI 中已埋入以下音频占位点（Console 输出 `[AUDIO]` 标记）：

| 触发时机 | 日志标记 | 对应事件 |
|----------|----------|----------|
| 教师开始接近 | `[AUDIO] ▶️ 播放教师接近脚步声` | TeacherFootstepStart |
| 教师开始检查 | `[AUDIO] ▶️ 播放检查开始音效` | TeacherInspectStart |
| 玩家被抓 | `[AUDIO] ▶️ 播放被抓音效` | PlayCaughtSound + PlayerCaught |

后续接入 AudioManager 时，监听上述事件即可替换占位。

---

## 已知限制

- `FirstLevelDevRunner` 会自动创建 `TeacherAI` 和 `CardReadingSystem` 场景物体；若场景中已有这些组件，会复用现有实例
- 颜文字显示使用 `OnGUI`，仅在 Editor 有效；正式 UI 由 `UIManager` 接管
- 音频占位仅输出 Debug.Log，无实际音频播放

---

## 组合测试建议

**完整查寝流程**（推荐操作序列）：
1. Play → 自动初始化
2. 观察左上角颜文字 `(✿◠‿◠)`
3. 按 `` ` `` 打开开发者面板，确认生命=2、时间=600
4. 保持 EyesClosed=false，等待第一次 Eye 检查 → 被抓，生命=1
5. 2 秒恢复后，立即 EyesClosed=true
6. 等待下一次检查 → 安全通过
7. 用 DevMode 加卡牌 2008，StartReading → 在不可打断段时等待检查 → 被抓，生命=0 → GameLose
