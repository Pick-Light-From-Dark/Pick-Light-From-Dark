# Unity场景搭建指南 - 阶段1.1

本文档说明如何在Unity编辑器中搭建游戏UI场景。

---

## 准备工作

**打开场景**：`Assets/Scenes/GameScene.unity`

这是游戏的主场景，所有的UI搭建都在这个场景中进行。

---

## 一、创建Canvas

1. **创建Canvas**
   - 右键 Hierarchy → UI → Canvas
   - 命名为 `GameCanvas`
   - Canvas Scaler 设置：
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 x 1080
     - Match: 0.5（width和height平衡）

2. **创建EventSystem**（如果没有自动创建）
   - 右键 Hierarchy → UI → Event System

---

## 二、创建4区域布局

### 2.1 顶部区域（TopArea）

1. **创建父对象**
   - 右键 GameCanvas → Create Empty
   - 命名为 `TopArea`
   - 添加组件：RectTransform, Canvas Group

2. **设置RectTransform**
   - Anchor: Top
   - Pivot: (0.5, 1)
   - Pos: (0, 0, 0)
   - Width: 1920
   - Height: 100

3. **创建倒计时显示**
   - 右键 TopArea → UI → Text - TextMeshPro
   - 命名为 `TimerDisplay`
   - 添加组件：TimerDisplay（脚本）
   - TextMeshPro设置：
     - Text: "600/600"
     - Font Size: 48
     - Color: White
     - Alignment: Center

---

### 2.2 左侧区域（LeftArea）

1. **创建父对象**
   - 右键 GameCanvas → Create Empty
   - 命名为 `LeftArea`
   - 添加组件：RectTransform, Vertical Layout Group

2. **设置RectTransform**
   - Anchor: Left
   - Pivot: (0, 0.5)
   - Pos: (0, 0, 0)
   - Width: 300
   - Height: 980（留出顶部100像素）

3. **Vertical Layout Group设置**
   - Child Alignment: Upper Center
   - Control Child Size: 勾选Width
   - Child Force Expand: 勾选Width

4. **创建任务栏（TaskBox）**
   - 右键 LeftArea → UI → Scroll View
   - 命名为 `TaskBox`
   - 高度: 300
   - Content 添加组件：Vertical Layout Group

5. **创建情绪值显示（EmotionDisplay）**
   - 右键 LeftArea → Create Empty
   - 命名为 `EmotionDisplay`
   - 添加组件：EmotionDisplay（脚本）
   - 高度: 150
   - 创建两个Text对象显示慌乱值和兴奋值
   - **重要**：Text对象使用 TextMeshPro 组件
   - **临时方案**：文本内容先用英文，如 "Panic: 30", "Excite: 30"
   - **原因**：默认字体不支持中文，需要后续配置中文字体资源

6. **创建角色占位（CharacterPlaceholder）**
   - 右键 LeftArea → UI → Image
   - 命名为 `CharacterPlaceholder`
   - 高度: 200
   - Color: 灰色
   - 添加子对象Text显示"Q版小人"

---

### 2.3 中央区域（CenterArea）

1. **创建父对象**
   - 右键 GameCanvas → Create Empty
   - 命名为 `CenterArea`
   - 添加组件：RectTransform

2. **设置RectTransform**
   - Anchor: Center
   - Pivot: (0.5, 0.5)
   - Pos: (0, -50, 0)
   - Width: 1000
   - Height: 980

3. **创建场景占位（ScenePlaceholder）**
   - 右键 CenterArea → UI → Image
   - 命名为 `ScenePlaceholder`
   - 高度: 700
   - Color: 灰色
   - 添加子对象Text显示"游戏场景"

4. **创建对话框（DialogueBox）**
   - 右键 CenterArea → UI → Text - TextMeshPro
   - 命名为 `DialogueBox`
   - 高度: 200
   - Text: "..."
   - Font Size: 24

---

### 2.4 右侧区域（RightArea）

1. **创建父对象**
   - 右键 GameCanvas → Create Empty
   - 命名为 `RightArea`
   - 添加组件：RectTransform, Vertical Layout Group

2. **设置RectTransform**
   - Anchor: Right
   - Pivot: (1, 0.5)
   - Pos: (0, 0, 0)
   - Width: 400
   - Height: 980

3. **Vertical Layout Group设置**
   - Child Alignment: Upper Center
   - Spacing: 10

4. **创建卡牌详情框（CardDetailBox）**
   - 右键 RightArea → UI → Image
   - 命名为 `CardDetailBox`
   - 高度: 250
   - Color: 半透明黑色
   - 添加子对象显示卡牌信息

5. **创建卡牌网格（CardGrid）**
   - 右键 RightArea → Create Empty
   - 命名为 `CardGrid`
   - 添加组件：CardGrid（脚本）, Grid Layout Group
   - 高度: 500
   - Grid Layout Group设置：
     - Cell Size: (180, 250)
     - Constraint: Fixed Column Count
     - Constraint Count: 2
     - Spacing: (10, 10)

6. **创建行动进度条（ActionProgressBar）**
   - 右键 RightArea → UI → Slider
   - 命名为 `ActionProgressBar`
   - 高度: 50
   - Direction: Left To Right

---

## 三、创建卡牌槽预制体（CardSlot）

1. **创建预制体对象**
   - 右键 Hierarchy → UI → Image
   - 命名为 `CardSlot`
   - 添加组件：CardSlot（脚本）, Button

2. **设置RectTransform**
   - Width: 180
   - Height: 250

3. **创建子对象**
   - Background: Image组件，灰色背景
   - CardName: TextMeshPro，显示卡牌名称
   - StackCount: TextMeshPro，显示堆叠层数（默认隐藏）

4. **保存为预制体**
   - 将CardSlot拖到 `Assets/Resources/UI/Prefabs` 文件夹
   - 删除场景中的CardSlot对象

---

## 四、创建UIManager

1. **创建空对象**
   - 右键 Hierarchy → Create Empty
   - 命名为 `UIManager`

2. **添加脚本**
   - 添加组件：UIManager（脚本）

3. **连接引用**
   - 在Inspector中拖拽对应的UI对象到脚本的引用字段：
     - Timer Display: TimerDisplay对象
     - Emotion Display: EmotionDisplay对象
     - Card Grid: CardGrid对象

---

## 五、测试设置

1. **确保游戏系统正常**
   - 场景中应有以下单例对象：
     - GameFlowController
     - CardManager
     - EmotionSystem
     - TaskManager
     - EventCenter

2. **测试流程**
   - 运行游戏
   - 检查倒计时是否正常显示
   - 检查情绪值是否更新
   - 检查卡牌是否显示在网格中
   - 点击卡牌测试选中功能

---

## 六、常见问题

**Q: Canvas没有正确缩放**
A: 检查Canvas Scaler的设置，确保使用Scale With Screen Size模式

**Q: UI元素位置不对**
A: 检查RectTransform的Anchor和Pivot设置

**Q: 脚本引用丢失**
A: 确保所有脚本文件都在正确的位置，并且没有编译错误

**Q: 卡牌不显示**
A: 检查CardManager是否正确初始化，是否有卡牌数据

**Q: TextMeshPro 显示中文为方框 □**
A: 默认字体不支持中文。解决方案：
1. **临时方案**：先用英文显示文本，如 "Panic: 30" 替代 "慌乱:30"
2. **正式方案**：
   - Window → TextMeshPro → Font Asset Creator
   - 选择支持中文的字体文件（如 SimHei.ttf、Microsoft YaHei.ttf）
   - 生成 Font Asset
   - 在 TextMeshPro 组件的 Font Asset 字段指定该字体

---

## 八、TextMeshPro 中文支持（重要）

### 问题
TextMeshPro 默认字体（LiberationSans）不支持中文字符，会显示为方框 □

### 临时解决方案
- 在 Inspector 中设置文本时使用英文
- 或在代码中设置英文文本

### 永久解决方案（待实施）
1. 准备中文字体文件（TTF格式）
2. 使用 TMP Font Asset Creator 生成字体资源
3. 替换项目中所有 TextMeshPro 组件的字体引用
4. 或创建默认字体资源并设置到 TMP Settings



## 七、后续优化

1. **美术替换**
   - 将占位色块替换为正式美术资源
   - 添加UI动画效果

2. **交互优化**
   - 实现卡牌拖拽功能
   - 添加卡牌详情展开动画
   - 添加点击反馈

3. **性能优化**
   - 使用对象池管理卡牌槽
   - 优化TextMeshPro文本更新频率
