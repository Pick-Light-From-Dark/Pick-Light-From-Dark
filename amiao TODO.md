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
- [ ] 教师ai
- [ ] 存档系统
- [ ] dotween 动画集成

---

## 第一夜关卡测试案 v2 支持进度

- [x] CardData 结构完善（cardType / initialStack / bindTaskId / relatedCardIds / allowedLevelIds / specialEffect）
- [x] Segment 区间列表支持（可打断/不可打断片段区间 `[start, end]`）
- [x] 读条打断进度保存（吃薯片卡特殊效果）
- [x] 床上/床下状态与卡牌联动（掀开被子/盖回被子）
- [x] EyeCloseSystem 参数走 LevelConfigSO（降率、阈值、倍率）
- [x] LevelConfigSO 加 initialInBed 字段
- [x] TeacherAI 手电筒慌乱值增长实现（修复 RoundToInt 截断 bug）
