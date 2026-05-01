# 闭眼系统 (Eye Close System)

闭眼系统管理玩家的闭眼状态，提供**时间加速**（闭眼10秒后2倍速）和**情绪恢复**（闭眼时持续降低情绪值）机制。

---

## 核心组件

| 组件 | 路径 | 职责 |
|------|------|------|
| **EyeCloseSystem** | `Assets/Scripts/Game/EyeClose/EyeCloseSystem.cs` | 单例，管理闭眼状态与时间加速 |
| **PlayerState** | `Assets/Scripts/Game/Data/PlayerState.cs` | 管理玩家在床/闭眼状态 |

---

## 文档索引

1. [类图](class-diagram.md) — EyeCloseSystem 核心属性与方法
2. [时间加速流程](time-acceleration-flow.md) — 闭眼10秒后触发2倍速的机制
3. [情绪恢复流程](emotion-recovery-flow.md) — 闭眼时持续降低情绪值

---

## 关键属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `isClosed` | bool | 当前是否闭眼 |
| `eyeCloseTimer` | float | 闭眼持续时间（秒） |
| `eyeCloseThreshold` | float | 时间加速触发阈值（10秒） |
| `timeAccelerated` | bool | 是否已启用时间加速 |

---

## 关键方法

| 方法 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| `GetEyeCloseDuration()` | 无 | float | 获取闭眼持续时间 |
| `IsTimeAccelerated()` | 无 | bool | 是否已启用时间加速 |
| `Reset()` | 无 | void | 重置闭眼状态（关卡重开时调用） |
| `EnableTimeAcceleration()` | 无 | void | 启用时间加速（`Time.timeScale = 2f`） |
| `DisableTimeAcceleration()` | 无 | void | 禁用时间加速（`Time.timeScale = 1f`） |

---

## 关键事件

| 事件 | 参数 | 触发时机 |
|------|------|---------|
| `EyeCloseStart` | 无 | 闭眼开始（或时间加速开始） |
| `EyeCloseEnd` | 无 | 闭眼结束（或时间加速结束） |
| `PlayerEyeCloseChanged` | `bool isClosed` | 闭眼状态变化 |

---

## 玩法机制

**时间加速**：
1. 玩家闭眼 → `eyeCloseTimer` 开始计时
2. 闭眼超过 10 秒 → 触发 `EnableTimeAcceleration()`
3. 游戏时间变为 2 倍速（`Time.timeScale = 2f`）
4. 玩家睁眼 → 恢复正常时间流速

**情绪恢复**：
- 闭眼时每帧调用 `EmotionSystem.DecreaseEmotionWhileEyeClose(Time.deltaTime)`
- 每秒降低 5 点情绪值（恐慌值 + 兴奋值）
- 最低降至初始值（30）

---

## 使用场景

**玩家闭眼**：
```csharp
// 按 C 键切换闭眼状态
PlayerState.Instance.ToggleEyeClose();
// EyeCloseSystem 自动监听 PlayerEyeCloseChanged 事件
```

**检查时间加速**：
```csharp
if (EyeCloseSystem.Instance.IsTimeAccelerated())
{
    // 游戏正在 2 倍速运行
}
```

**关卡重置**：
```csharp
EyeCloseSystem.Instance.Reset();
// 重置闭眼计时器，禁用时间加速
```
