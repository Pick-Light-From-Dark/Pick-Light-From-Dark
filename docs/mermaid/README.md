# Pick-Light-From-Dark 系统文档

本目录包含游戏《灯下黑》的核心系统设计文档，使用 Mermaid 图表展示类图、时序图和流程图。

---

## 📑 目录

### [任务系统 (Task System)](task-system/README.md)

管理关卡任务进度、卡牌使用记录与通关判定。

- [类图](task-system/class-diagram.md) — TaskManager、TaskGoal、LevelConfigSO 关系
- [初始化流程](task-system/initialization-flow.md) — GameFlowController 如何初始化各系统
- [卡牌完成上报流程](task-system/card-completion-flow.md) — CardReadComplete 事件如何更新任务进度
- [任务进度同步](task-system/progress-sync-flow.md) — UI 如何查询当前任务状态
- [关卡通关流程](task-system/level-complete-flow.md) — 所有任务完成时的处理流程
- [关卡失败流程](task-system/level-fail-flow.md) — 时间耗尽/情绪超标的失败路径
- [重开/重试流程](task-system/retry-flow.md) — 场景重建与系统重置

---

### [情绪值系统 (Emotion System)](emotion-system/README.md)

管理玩家的慌乱值与兴奋值，影响教师AI的巡逻检查。

- [类图](emotion-system/class-diagram.md) — EmotionSystem 核心属性与方法
- [初始化流程](emotion-system/initialization-flow.md) — 如何从 LevelConfigSO 加载情绪值配置
- [交互流程](emotion-system/interaction-flow.md) — 与 TeacherAI、卡牌系统的交互

---

### [闭眼系统 (Eye Close System)](eye-close-system/README.md)

管理玩家闭眼状态，提供时间加速与情绪恢复机制。

- [类图](eye-close-system/class-diagram.md) — EyeCloseSystem 核心属性与方法
- [时间加速流程](eye-close-system/time-acceleration-flow.md) — 闭眼10秒后触发2倍速
- [情绪恢复流程](eye-close-system/emotion-recovery-flow.md) — 闭眼时持续降低情绪值

---

## 🚀 快速导航

**新手入门**：建议按以下顺序阅读：
1. 任务系统类图 → 了解整体架构
2. 任务系统初始化流程 → 理解系统启动顺序
3. 情绪值系统类图 → 理解情绪值机制
4. 闭眼系统时间加速流程 → 理解闭眼玩法

**问题排查**：
- JSON 未生成 → 任务系统初始化流程
- 任务不更新 → 任务系统卡牌完成上报流程
- 情绪值异常 → 情绪值系统交互流程

---

## 📝 文档说明

- 所有时序图遵循 Mermaid 语法
- 参与者名称使用中文，与代码中的类名/变量名对应
- 关键代码路径在图表下方标注

**最后更新**：2026-05-01
