# UI实现方案 - 阶段1（最简可玩版）

**日期**：2026-05-01
**版本**：v1.0
**目标**：打通前后端，使用占位美术实现最小可玩UI

---

## 一、设计原则

1. **布局一致**：UI布局严格遵循设计图的4区域划分
2. **美术暂替**：使用色块+文字占位，不进行美术制作
3. **最简实现**：优先核心功能，交互流程可简化
4. **事件驱动**：UI通过EventCenter监听游戏系统事件

---

## 二、Canvas层级结构

```
GameCanvas
├── TopArea（顶部区域）
│   └── TimerDisplay（倒计时文本）
├── LeftArea（左侧区域）
│   ├── TaskBox（任务栏）
│   ├── EmotionDisplay（情绪值显示）
│   │   ├── PanicIcon + PanicValue（慌乱值）
│   │   └── ExciteIcon + ExciteValue（兴奋值）
│   └── CharacterPlaceholder（Q版小人占位）
├── CenterArea（中央区域）
│   ├── ScenePlaceholder（游戏场景占位）
│   └── DialogueBox（对话/独白框）
├── RightArea（右侧区域）
│   ├── CardDetailBox（卡牌详情框）
│   │   ├── CardName（卡牌名称）
│   │   ├── CardEffect（卡牌作用）
│   │   ├── CardDescription（卡牌描述）
│   │   ├── ProgressBarContainer（进度条容器）
│   │   └── EffectTags（效果标签）
│   ├── CardGrid（卡牌网格，2列布局）
│   └── ActionProgressBar（行动进度条）
└── BottomArea（底部区域）
    ├── CloseEyeButton（闭眼按钮）
    └── InterruptButton（打断按钮）
```

---

## 三、各组件详细设计

### 3.1 倒计时显示（TimerDisplay）

| 属性 | 配置 |
|------|------|
| **组件** | TextMeshPro |
| **格式** | "320/600"（秒） |
| **更新方式** | Update中轮询 `GameFlowController.GetRemainingTime()` |
| **占位样式** | 白色文字，48号字体 |

**实现代码**：
```csharp
void Update()
{
    if (GameFlowController.Instance != null)
    {
        float remaining = GameFlowController.Instance.GetRemainingTime();
        float total = GameFlowController.Instance.GetLevelConfig().timeLimit;
        timerText.text = $"{remaining:F0}/{total:F0}";
    }
}
```

---

### 3.2 任务栏（TaskBox）

| 属性 | 配置 |
|------|------|
| **组件** | ScrollRect + VerticalLayoutGroup + Content |
| **数据源** | `TaskManager.Instance.GetAllTasks()` |
| **占位样式** | 灰色背景，白色文字 |

**显示规则**：
- **未完成**：白色字体，无删除线，排在上部
- **已完成**：半透明字体 + 删除线，排在底部
- **隐藏任务**：完成前不显示，完成后显示并标紫色背景

**监听事件**：
- `E_EventType.TaskComplete` - 任务完成时刷新列表
- `E_EventType.TaskReveal` - 隐藏任务揭示时显示

---

### 3.3 情绪值显示（EmotionDisplay）

| 属性 | 配置 |
|------|------|
| **组件** | 两个Icon + Value Text组合 |
| **占位样式** | 色块图标 + 数值文字 |

**颜色规则**：
- **30-50**：绿色背景
- **50-临界值**：黄色背景
- **>临界值**：红色背景

**监听事件**：
- `E_EventType.PanicChanged` - 慌乱值变化
- `E_EventType.ExciteChanged` - 兴奋值变化

**交互**：点击展开显示详细数值（可选功能）

---

### 3.4 卡牌网格（CardGrid）

| 属性 | 配置 |
|------|------|
| **组件** | Grid Layout Group（2列，自适应行数） |
| **最大显示** | 10张（每页5张，可滚动） |
| **占位样式** | 灰色矩形 + 卡牌名称 |

**卡牌槽预制体结构**：
```
CardSlot（预制体）
├── Background（Image：灰色矩形）
├── CardName（Text：卡牌名称）
├── StackCount（Text：堆叠层数，仅堆叠卡显示）
└── Button（触发点击事件）
```

**交互流程**：
1. 点击卡牌 → 在CardDetailBox显示详情
2. 双击卡牌 → 直接执行（简化版交互，跳过拖拽）

**监听事件**：
- `E_EventType.CardAdded` - 添加卡牌到网格
- `E_EventType.CardRemoved` - 从网格移除卡牌

---

### 3.5 卡牌详情框（CardDetailBox）

| 属性 | 配置 |
|------|------|
| **组件** | VerticalLayoutGroup + 多个Text组件 |
| **占位样式** | 半透明黑色背景，白色文字 |

**显示内容**：
- CardName：卡牌名称
- CardEffect：卡牌作用
- CardDescription：卡牌描述
- ProgressBar：进度条（绿红分段显示可打断/不可打断）
- EffectTags：效果标签数组

**默认状态**：显示"请选择一张卡牌"

---

### 3.6 行动进度条（ActionProgressBar）

| 属性 | 配置 |
|------|------|
| **组件** | Slider + Text |
| **格式** | "0.5s/2.0s" |
| **分段显示** | 可打断段（绿色） + 不可打断段（红色） |

**实现方式**：使用2个Image叠加显示不同颜色的分段

---

### 3.7 闭眼按钮（CloseEyeButton）

| 属性 | 配置 |
|------|------|
| **组件** | Button + Text |
| **占位样式** | 蓝色矩形，显示"睁眼"/"闭眼" |
| **位置** | 屏幕左下角 |

**功能**：
```csharp
void OnClickCloseEye()
{
    EyeCloseSystem.Instance.ToggleEyeClose();
    UpdateButtonText();
}
```

---

### 3.8 打断按钮（InterruptButton）

| 属性 | 配置 |
|------|------|
| **组件** | Button |
| **占位样式** | 红色矩形，显示"打断" |
| **位置** | 屏幕右下角 |
| **启用规则** | 仅在可打断片段期间可点击 |

**功能**：
```csharp
void OnClickInterrupt()
{
    CardManager.Instance.InterruptCurrentCard();
}
```

---

## 四、事件监听清单

```csharp
// 在GamePanel或UIManager中监听以下事件
void OnEnable()
{
    // 游戏流程事件
    EventCenter.Instance.AddEventListener(E_EventType.GameStart, OnGameStart);
    EventCenter.Instance.AddEventListener(E_EventType.GamePause, OnGamePause);
    EventCenter.Instance.AddEventListener(E_EventType.GameResume, OnGameResume);
    EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
    EventCenter.Instance.AddEventListener(E_EventType.GameLose, OnGameLose);

    // 情绪值事件
    EventCenter.Instance.AddEventListener(E_EventType.EmotionChanged, OnEmotionChanged);
    EventCenter.Instance.AddEventListener(E_EventType.PanicChanged, OnPanicChanged);
    EventCenter.Instance.AddEventListener(E_EventType.ExciteChanged, OnExciteChanged);

    // 卡牌事件
    EventCenter.Instance.AddEventListener(E_EventType.CardAdded, OnCardAdded);
    EventCenter.Instance.AddEventListener(E_EventType.CardRemoved, OnCardRemoved);
    EventCenter.Instance.AddEventListener(E_EventType.CardStartProgress, OnCardStartProgress);
    EventCenter.Instance.AddEventListener(E_EventType.CardInterrupted, OnCardInterrupted);
    EventCenter.Instance.AddEventListener(E_EventType.CardCompleted, OnCardCompleted);

    // 任务事件
    EventCenter.Instance.AddEventListener(E_EventType.TaskComplete, OnTaskComplete);
    EventCenter.Instance.AddEventListener(E_EventType.TaskReveal, OnTaskReveal);
}

void OnDisable()
{
    // 移除所有监听
}
```

---

## 五、占位美术规格

| UI元素 | 占位样式 | 尺寸 |
|--------|----------|------|
| **背景** | 黑色全屏 | 1920x1080 |
| **各区域背景** | 灰色半透明矩形 | 根据布局 |
| **图标** | 64x64色块 + 文字标签 | 64x64 |
| **卡牌** | 200x300灰色矩形 + 名称 | 200x300 |
| **Q版小人** | 200x200灰色方块 | 200x200 |
| **游戏场景** | 16:9灰色方块 | 自适应 |
| **按钮** | 150x50色块 | 150x50 |

---

## 六、实现阶段

### 阶段1.1 - 核心显示（最优先）
- [x] Canvas搭建4区域布局
- [ ] 倒计时显示
- [ ] 情绪值显示（基础数值）
- [ ] 卡牌网格（显示当前手牌）
- [ ] 简单卡牌点击执行（无进度条，直接生效）

### 阶段1.2 - 进度系统
- [ ] 卡牌详情框
- [ ] 行动进度条（带绿红分段）
- [ ] 打断按钮
- [ ] 卡牌执行带真实计时

### 阶段1.3 - 完整功能
- [ ] 任务栏集成
- [ ] 闭眼按钮
- [ ] 角色和场景占位图
- [ ] 对话框
- [ ] 胜利/失败弹窗

---

## 七、脚本文件清单

需要创建/修改的脚本：

| 文件路径 | 用途 |
|----------|------|
| `Assets/Scripts/UI/GamePanel.cs` | 游戏主面板（修改现有文件） |
| `Assets/Scripts/UI/UIManager.cs` | UI管理器，统筹所有UI组件 |
| `Assets/Scripts/UI/Components/TimerDisplay.cs` | 倒计时显示组件 |
| `Assets/Scripts/UI/Components/EmotionDisplay.cs` | 情绪值显示组件 |
| `Assets/Scripts/UI/Components/CardGrid.cs` | 卡牌网格组件 |
| `Assets/Scripts/UI/Components/CardDetailBox.cs` | 卡牌详情框组件 |
| `Assets/Scripts/UI/Components/TaskBox.cs` | 任务栏组件 |
| `Assets/Scripts/UI/Components/ProgressBar.cs` | 进度条组件 |
| `Assets/Scripts/UI/Components/DialogueBox.cs` | 对话框组件 |
| `Assets/Scripts/UI/Prefabs/CardSlot.cs` | 卡牌槽预制体脚本 |

---

## 八、后续优化方向

1. **美术替换**：将占位色块替换为正式美术资源
2. **动画效果**：添加卡牌使用、进度条等动画
3. **拖拽交互**：实现真实的卡牌拖拽到思考框的交互
4. **音效集成**：连接音效系统
5. **性能优化**：对象池、图集优化等

---

## 九、注意事项

1. 所有UI使用uGUI系统，确保与Unity版本兼容
2. 事件监听一定要在OnDisable中移除，避免内存泄漏
3. 占位美术使用纯色+文字，便于后续替换
4. 交互流程可简化，但必须保证核心玩法可玩
5. 所有数值显示直接从游戏系统获取，不维护副本
