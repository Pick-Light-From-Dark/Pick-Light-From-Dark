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
- [ ] 情绪值系统
  - [ ] panic/excite 数值管理
  - [ ] 情绪值影响逻辑
- [ ] 闭眼系统
  - [ ] dotween 动画集成
  - [ ] 眨眼机制
- [ ] 存档系统
