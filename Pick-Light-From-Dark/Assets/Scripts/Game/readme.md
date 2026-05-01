# Assets/Scripts/Game 目录结构说明

## 整体架构

根据项目设计，Game 目录按照功能模块进行了清晰的划分，包含以下子目录结构：

```
Assets/Scripts/Game/
├── Card/             # 卡牌系统
├── AI/               # 老师AI
├── Emotion/          # 情绪值系统
├── EyeClose/         # 闭眼系统
├── UI/               # UI面板
├── Data/             # 数据结构
├── Config/           # 配置管理
├── System/           # 系统
└── Flow/             # 游戏流程
```

## 各模块功能详解

### 1. Flow（游戏流程）模块

**GameFlowController.cs**：游戏流程控制器，负责管理游戏的整体流程（开始、暂停、胜利、失败），控制时间、游戏状态和关卡配置。

### 2. AI（老师AI）模块

**TeacherAI.cs**：老师AI状态机，实现了完整的巡逻逻辑，包括 `Idle -> Approaching -> Inspecting -> Leaving -> Idle` 的状态转换。

### 3. Emotion（情绪值）模块

**EmotionSystem.cs**：情绪值系统，管理玩家的慌乱值和兴奋值，包含情绪值的增减、临界值检测等功能。

### 4. EyeClose（闭眼）模块

**EyeCloseSystem.cs**：闭眼系统，管理闭眼期间的情绪值降低和时间加速机制。

### 5. Card（卡牌）模块

- **CardManager.cs**：卡牌管理器，负责手牌管理、发牌、弃牌等卡牌相关功能。
- **CardReadingSystem.cs**：卡牌读取系统。

### 6. Config（配置）模块

**LevelConfigSO.cs**：关卡配置脚本，定义了可创建的 ScriptableObject 资源，包含关卡的基本信息、情绪值设置、巡逻配置、初始卡牌等参数。

### 7. Data（数据）模块

- **PlayerState.cs**：玩家状态管理
- **Segment.cs**：分段数据结构
- **CardData.cs**：卡牌数据定义s
- **CardDataContainer.cs**：卡牌数据容器

### 8. System（系统）模块

- **GameEvents.cs**：游戏事件定义
- **EmotionTest.cs**：情绪系统测试
- **DataTest.cs**：数据测试

### 9. Testing（测试）模块

- **AutomatedGameTest.cs**：自动化游戏测试
- **TestAssertions.cs**：测试断言

### 10. UI（用户界面）模块

虽然没有具体代码文件展示，但根据目录结构，该模块负责处理用户界面相关的逻辑。

## 设计特点

这个架构体现了良好的模块化设计原则，每个功能模块职责单一且相互独立，便于团队协作开发和后期维护。