# 🚀 施工计划

数据配置、简单系统、让AI生成代码
- Day 1：简单数据类
- Day 2-3：独立系统
- Day 4+：复杂交互


- [x] 确认需求 figma、文字+后端、Lua
- [x] CardData.cs、LevelConfigSO.cs
- [x] 任务系统实现（阿喵）
  - [x] TaskGoal 任务目标类
  - [x] TaskManager 任务管理器
  - [x] LevelConfigSO 添加任务清单字段
  - [x] 卡牌完成事件集成
- [x] 动画系统基础（阿喵）
  - [x] Animation 文件夹结构
  - [x] 眨眼动画控制器和片段
  - [x] 角色 LuYing 眨眼精灵图
  - [x] amiao.unity 场景示例
- [x] 单例优化（tianpo）
  - [x] 使用 isInitialized 标志替代检查 remainingTime
  - [x] 在 Update 中检查 remainingTime 防止过早执行
  - [x] 添加多实例日志记录
  - [x] 使用 Resources.FindObjectsOfTypeAll 完全清理
- [x] 情绪值系统
  - [x] panic/excite 数值管理
  - [x] 情绪值影响逻辑
  - [x] 测试功能（EmotionTest + EmotionDisplay Prefab）
- [x] 闭眼系统
  - [x] 眨眼机制（EyeCloseSystem + EyeCloseDisplay Prefab）
  - [x] 测试功能
- [x] 教师ai
- [x] 第一关完整测试场景
  - [x] DevModeController 开发者模式（OnGUI 调参 + 颜文字 + ContextMenu）
  - [x] FirstLevelDevRunner 场景运行器（自动聚合系统 + 一键初始化）
  - [x] FirstLevelDevRunner.prefab（双组件预设）
  - [x] TeacherAI 音频占位钩子（接近/检查/被抓）
  - [x] FirstLevelTest.md 测试文档（10 TC + 组合场景）
- [ ] 存档系统（评估后跳过，当前仅 PlayerDataStore 做关卡记录持久化）
- [ ] dotween 动画集成（需 Unity 编辑器导入 DG.Tweening 包，当前仅有 DOTweenAnimationMgr.cs 占位实现）

---

## 第一夜关卡测试案 v2 支持进度

- [x] CardData 结构完善（cardType / initialStack / bindTaskId / relatedCardIds / allowedLevelIds / specialEffect）
- [x] Segment 区间列表支持（可打断/不可打断片段区间 `[start, end]`）
- [x] 读条打断进度保存（吃薯片卡特殊效果）
- [x] 床上/床下状态与卡牌联动（掀开被子/盖回被子）
- [x] EyeCloseSystem 参数走 LevelConfigSO（降率、阈值、倍率）
- [x] LevelConfigSO 加 initialInBed 字段
- [x] TeacherAI 手电筒慌乱值增长实现（修复 RoundToInt 截断 bug）

---

## 后端测试套件（Scenes/Amiao/TestPrefabs/）

- [x] 8 套 TestRunner 脚本（Inspector 调参 + OnGUI + ContextMenu + 快捷键）
  - EmotionSystemTester / EyeCloseSystemTester / PlayerStateTester
  - CardReadingTester / CardManagerTester / TaskSystemTester
  - TeacherAITester / GameFlowTester
- [x] 对应 8 个 .prefab（默认绑 TestLevelConfig）
- [x] 测试用例文档 README.md（45+ TC，含集成场景示例）

---

## 巡检修复记录

- [x] EyeCloseSystem timeScale 与 GameFlowController 状态同步（订阅 GameResume/GameWin/GameLose，修复被抓暂停恢复后加速永久失效）
- [x] UIManager GameLose 订阅签名错误（无参 → `<string>` 泛型版，与触发端签名一致，避免 EventInfo 类型错配抛 NullRef）
- [x] 补齐 GameFlowController.Initialize 系统初始化链（PlayerState.Initialize + EyeCloseSystem.Initialize），修复 initialInBed 字段定义后无人读取，以及 EyeCloseSystem 阈值/倍率配置在正式流程不生效的问题
- [x] 补 CardManager.Initialize 链路 + 实装生命值扣除逻辑（修复 maxLives 死字段：之前只 Debug.Log 不影响 gameplay；以及 CardManager 单例跨关残留 + 初始卡牌不发放问题）
- [x] OnPlayerCaught 中 Invoke(ResumeGame, 2f) 在 timeScale=0 下永久冻结 → 改用 WaitForSecondsRealtime 协程，确保 2 秒真实时间后自动恢复
- [x] DemoController 未初始化 TeacherAI → patrolIntervals/patrolTime 等 Vector2 保持默认值 (0,0)，状态机周期全部失效 → 修复：在 InitializeDemo 中通过 FindObjectOfType 获取并调用 TeacherAI.Initialize
- [x] GameFlowController 未订阅 LevelComplete 事件 → TaskManager 完成所有任务后触发 LevelComplete 但无人响应，游戏无法自动胜利 → 修复：Start 中订阅 LevelComplete → GameWin()
- [x] CardReadingSystem Update 缺少游戏状态保护 → 游戏暂停/结束时读条仍继续推进
- [x] CardReadingSystem segments 空引用风险 → CardData.segments 未初始化时 StartReading 后 Update 直接 NullRef
- [x] CardManager 公共 API 空引用保护 → DiscardOtherCards/TriggerLinkedCards/RecordCardUse 缺少参数检查
- [x] PlayerState Update 缺少游戏状态保护 → 游戏暂停/结束时仍能通过 C 键切换闭眼状态
- [x] 核心运行时 FindObjectOfType/FindObjectsOfType 废弃 API 替换 → Unity 2022.3 已标记 obsolete
- [x] 测试代码 DestroyImmediate 运行时隐患 → 非编辑器构建中调用 DestroyImmediate 可能导致异常
- [x] LevelConfigSO.criticalValue 无默认值 → 创建新配置后忘记设置会导致 IsCaughtByCriticalValue 恒为 true，手电筒检查必被抓
- [x] EmotionSystem NotifyEmotionChanged 无递归保护 → 事件回调中再次调用 ChangePanic/ChangeExcite 会导致无限递归直到栈溢出
- [x] GameFlowController.Initialize 空引用保护 → 入口缺少 config null 检查，传入 null 后后续 levelConfig 访问直接 NullRef
- [x] 核心系统 Initialize 统一 null 保护 → EmotionSystem/CardManager/TeacherAI/CardReadingSystem Initialize 入口缺少 config null 检查
- [x] Serializable 类默认构造函数缺失 → Segment/TaskGoal/JsonLevelRecord/CardUseEntry/TaskGoalRecord 无参构造函数缺失，Unity JsonUtility 反序列化可能失败
- [x] DevModeController 除零风险 → GetEmotionEmoji 中 criticalValue 未做零值保护
- [x] EventCenter 死代码清理 → 未使用的 Claer 拼写错误方法移除
- [x] DevModeController timeLimit 浮点赋值编译错误 → levelConfig.timeLimit 为 int，原代码赋 600f 导致 CS0266
- [x] EmotionSystem.Initialize 初始值未 Clamp → DevModeController 测试案v2 设 initialPanic=15 低于 minPanic=30，初始状态与 ChangePanic 行为不一致
- [x] PlayerDataStore.LoadOrCreate records null 风险 → JsonUtility.FromJson 后 records 可能为 null，后续 Add 抛 NullRef
- [x] TaskManager.Initialize taskGoals 元素 null 风险 → Inspector 列表中可能包含 null 元素，访问 targetCardId 抛 NullRef
- [x] CardManager 集合字段未初始化 → handCards / cardHistory 声明时未 new，SingletonAutoMono 创建后 Initialize 前调用 public API 抛 NullRef
- [x] GameWin 冗余 LevelComplete 事件触发 → TaskManager.CheckLevelComplete 已触发 LevelComplete，GameWin 中再次触发导致双发通知
- [x] TeacherAI 状态切换间隔零值保护 → LevelConfigSO Vector2 默认值 (0,0) 使 Random.Range 返回 0，stateTimer 立即归零导致状态机疯狂切换
- [x] DemoController 模拟失败绕过 GameFlowController → 直接 EventTrigger(GameLose) 跳过 isGameOver/CancelInvoke/StopAllCoroutines/timeScale 保护，改为调用 GameFlowController.Instance.GameLose()
- [x] AutomatedGameTest 时间加速断言时序 bug → Test_EyeCloseSystem 先 SetEyesClosed(false) 再检查 IsTimeAccelerated，但 EyeCloseSystem.Update 睁眼后立即 DisableTimeAcceleration，断言恒为 false
- [x] EyeCloseDisplay 硬编码加速倍率 → string.Format(accelerationFormat, 2) 固定显示 2x，但 EyeCloseSystem.accelerationMultiplier 由 LevelConfigSO 配置（v2 为 1.5x），显示与实际倍率不符 → 新增 GetAccelerationMultiplier() 获取实际倍率
- [x] PoolMgr.PushObj 缺少 null 与 key 存在性保护 → poolDic[obj.name].Push(obj) 在 obj 为 null 或 name 不在字典时直接抛异常，增加前置检查与兜底销毁
- [x] TimerMgr 超时回调无 null 保护 → item.overCallBack.Invoke() 未做 null 检查，TimerItem 回收后 overCallBack 为 null 会抛 NullRef → 改为 ?.Invoke()
- [x] EventCenter 双向类型不匹配警告 → 泛型与非泛型 EventTrigger 均增加 as-cast 检查，签名错配时输出警告日志，替代之前的静默忽略行为
- [x] PoolMgr.GetObj Resources.Load null 保护 → 资源路径不存在时 Instantiate(null) 抛 NullRef，增加前置检查并输出明确错误日志
- [x] MusicMgr.PlaySound 对象池返回 null 保护 → PoolMgr.GetObj("Sound/soundObj") 资源不存在时返回 null，直接 .GetComponent 会抛 NullRef，增加前置检查
- [x] ResMgr 卸载回调 null 保护 → ReallyUnloadUnusedAssets 和 ReallyClearDic 中 callBack() 未做 null 检查，传入 null 时抛 NullRef，改为 ?.Invoke()
- [x] TextUtil 分割方法索引越界与 null 保护 → SplitStrToIntArrTwice/SplitStrTwice 中 ints/strs2 长度为 1 时访问 [1] 越界，且 callBack 未做 null 检查

