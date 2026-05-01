# 情绪值系统 (Emotion System)

情绪值系统管理玩家的**慌乱值**（Panic）和**兴奋值**（Excite），当两者之和超过临界值时，教师AI的手电筒检查会发现玩家。

---

## 核心组件

| 组件 | 路径 | 职责 |
|------|------|------|
| **EmotionSystem** | `Assets/Scripts/Game/Emotion/EmotionSystem.cs` | 单例，管理情绪值计算与状态检测 |
| **PlayerState** | `Assets/Scripts/Game/Data/PlayerState.cs` | 管理玩家在床/闭眼状态 |
| **LevelConfigSO** | `Assets/Scripts/Game/Config/LevelConfigSO.cs` | 配置初始情绪值与临界值 |

---

## 文档索引

1. [类图](class-diagram.md) — EmotionSystem 核心属性与方法
2. [初始化流程](initialization-flow.md) — 从 LevelConfigSO 加载配置
3. [交互流程](interaction-flow.md) — 与 TeacherAI、卡牌系统的交互

---

## 关键属性

| 属性 | 范围 | 说明 |
|------|------|------|
| `panicValue` | 30 ~ 100 | 慌乱值（太紧张会被发现） |
| `exciteValue` | 30 ~ 100 | 兴奋值（太激动会被发现） |
| `criticalValue` | 可配置 | 临界值（`panic + excite >= criticalValue` 时被抓） |

---

## 关键方法

| 方法 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| `Initialize(LevelConfigSO)` | 关卡配置 | void | 初始化情绪值与临界值 |
| `ChangePanic(int delta)` | 增量 | void | 修改慌乱值（自动限制在 [30, 100]） |
| `ChangeExcite(int delta)` | 增量 | void | 修改兴奋值（自动限制在 [30, 100]） |
| `IsCaughtByCriticalValue()` | 无 | bool | 检查是否超过临界值 |
| `GetTotalEmotion()` | 无 | int | 获取情绪值总和 |
| `DecreaseEmotionWhileEyeClose(float deltaTime)` | 时间增量 | void | 闭眼时降低情绪值（每秒5点） |

---

## 关键事件

| 事件 | 参数 | 触发时机 |
|------|------|---------|
| `EmotionChanged` | `EmotionInfo` | 情绪值变化 |
| `PanicChanged` | `int newValue` | 慌乱值变化 |
| `ExciteChanged` | `int newValue` | 兴奋值变化 |

---

## 使用场景

**卡牌使用**：
```csharp
// 卡牌增加慌乱值
EmotionSystem.Instance.ChangePanic(card.panicDelta);
EmotionSystem.Instance.ChangeExcite(card.exciteDelta);
```

**教师AI检查**：
```csharp
// 手电筒检查时判断是否被抓
if (EmotionSystem.Instance.IsCaughtByCriticalValue())
{
    // 抓到玩家，触发 PlayerCaught 事件
}
```

**闭眼恢复**：
```csharp
// 闭眼时持续降低情绪值
EmotionSystem.Instance.DecreaseEmotionWhileEyeClose(Time.deltaTime);
```
