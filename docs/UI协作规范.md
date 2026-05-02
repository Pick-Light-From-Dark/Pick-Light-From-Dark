# UI协作规范

**版本**：v1.0  
**日期**：2026-05-01  
**适用人员**：好捅天破、阿喵、oxy

---

## 一、文件夹结构规范

### 1.1 Prefab组织结构

```
Assets/UI/
├── GameScene/                 # 游戏场景专用Prefabs（阿喵 + oxy）
│   ├── Panels/
│   │   ├── GamePanel.prefab
│   │   ├── TaskBox.prefab
│   │   ├── EmotionDisplay.prefab
│   │   ├── CardSlot.prefab
│   │   ├── CardDetailBox.prefab
│   │   ├── DialogueBox.prefab
│   │   └── TimerDisplay.prefab
│   │
│   ├── Components/            # 游戏内组件
│   │   ├── CardGrid.prefab
│   │   ├── ProgressBar.prefab
│   │   └── ActionProgressBar.prefab
│   │
│   └── Widgets/               # 游戏内小组件
│       ├── CharacterPlaceholder.prefab
│       └── ScenePlaceholder.prefab
│
├── Others/                    # 其他场景Prefabs（oxy整理）
│   ├── MainMenu/             # oxy负责整理
│   │   ├── BeginPanel.prefab
│   │   ├── AboutUsPanel.prefab
│   │   ├── SettingPanel.prefab
   │   └── AchievementPanel.prefab
│   │
│   ├── Story/                # 剧情相关（oxy整理）
│   │   ├── CGContentPanel.prefab
│   │   ├── EndingContentPanel.prefab
│   │   └── ExperiencePanel.prefab
│   │
│   └── Common/                # 通用面板（oxy整理）
│       ├── TipPanel.prefab
│       ├── StopGamePanel.prefab
│       └── SaveGamePanel.prefab
│
├── Components/                # 可复用基础组件
│   ├── Buttons/
│   │   ├── PrimaryButton.prefab
│   │   ├── SecondaryButton.prefab
│   │   └── IconButton.prefab
│   ├── ProgressBars/
│   │   ├── HorizontalProgressBar.prefab
│   │   └── CircularProgressBar.prefab
│   └── Lists/
│       ├── ScrollList.prefab
│       └── GridList.prefab
│
├── Widgets/                   # 功能性小组件
│   ├── TimerWidget.prefab
│   ├── EmotionBarWidget.prefab
│   └── CardPreviewWidget.prefab
│
└── Templates/                 # 开发模板
    ├── PanelTemplate.prefab
    └── PopupTemplate.prefab
```

### 1.2 场景组织

```
Assets/Scenes/
├── GameScene.unity            # 主游戏场景
├── MainMenuScene.unity        # 主菜单场景
├── StoryScene.unity            # 剧情场景
├── Test/                       # 测试场景（个人开发用）
│   ├── Test_Amao.unity        # 阿喵测试场景
│   ├── Test_Oxy.unity         # oxy测试场景
│   └── Test_Main.unity        # 好捅天破测试场景
```

---

## 二、命名规范

### 2.1 Prefab命名

**格式**：`[功能][类型].prefab`

**示例**：
- `BeginPanel.prefab` - 主菜单开始面板
- `TaskBox.prefab` - 任务栏组件
- `PrimaryButton.prefab` - 主按钮
- `EmotionDisplayWidget.prefab` - 情绪值显示小组件

**后缀规则**：
- `Panel` - 完整面板（独立页面）
- `Box` - 容器组件（包含其他UI）
- `Widget` - 功能小组件
- `Button` - 按钮
- `Bar` - 进度条
- `Item` - 列表项
- `Display` - 显示组件

### 2.2 GameObject命名

- **Canvas/Panel**: PascalCase（如 `GamePanel`, `LeftArea`）
- **子元素**: camelCase（如 `panicText`, `exciteText`）
- **按钮**: `[动词]Button`（如 `StartButton`, `CloseButton`）

### 2.3 脚本命名

- **面板脚本**: `XxxPanel.cs` 或 `XxxView.cs`
- **组件脚本**: `XxxComponent.cs` 或 `XxxDisplay.cs`
- **控制器**: `XxxController.cs` 或 `XxxManager.cs`

---

## 三、Prefab-场景对应关系

### 3.1 对应表

| Prefab路径 | 使用场景 | 负责人 | 备注 |
|-----------|---------|--------|------|
| `UI/GameScene/**/*.prefab` | GameScene | 阿喵、oxy | 游戏内UI，主要开发区域 |
| `UI/Others/MainMenu/*.prefab` | MainMenuScene | oxy | 主菜单相关，由oxy整理 |
| `UI/Others/Story/*.prefab` | StoryScene | oxy | 剧情相关，由oxy整理 |
| `UI/Others/Common/*.prefab` | 多场景共用 | oxy | 通用面板，由oxy整理 |

### 3.2 使用规则

1. **每个场景只加载需要的Prefab**
   - 不要在GameScene加载MainMenu的Prefab
   - 使用Addressable或Resources.Load按需加载

2. **跨场景共用Prefab放在Common**
   - TipPanel、StopGamePanel等
   - 避免重复创建

3. **测试场景使用简化版**
   - Test_Amao.unity 只加载阿喵开发的Prefab
   - Test_Oxy.unity 只加载oxy开发的Prefab

---

## 四、Git工作流

### 4.1 分支命名

```
main                        # 稳定主分支
├── feature/ui-card         # 阿喵：卡牌系统
├── feature/ui-logic        # oxy：功能逻辑
├── feature/ui-framework    # 好捅天破：框架和整合
├── feature/ui-story        # 剧情系统（后续）
└── refactor/ui-reorg       # Prefab重组（一次性）
```

### 4.2 提交规范

**Commit格式**：`[模块] 简短描述`

**示例**：
- `[Card] 添加卡牌拖拽动画`
- `[Task] 实现任务完成检测`
- `[Framework] 重组UI文件夹结构`
- `[Bugfix] 修复EmotionDisplay显示错误`

### 4.3 协作流程

1. **每日开发**
   - 各自在分支开发
   - 推送到自己的远程分支

2. **每日合并**
   - 好捅天破每天合并feature分支到main
   - 解决冲突后通知对应人员

3. **代码审查**
   - 合并前由好捅天破review
   - 通过后才能合并到main

---

## 五、开发规范

### 5.1 Prefab创建流程

1. **在测试场景中创建**
   - 阿喵在 `Test_Amao.unity`
   - oxy在 `Test_Oxy.unity`

2. **功能测试通过后**
   - 保存为Prefab
   - 放到对应文件夹

3. **提交代码**
   - 包含 `.prefab` 文件
   - 包含相关脚本
   - 写清楚Commit描述

### 5.2 场景使用规范

**禁止**：
- ❌ 直接修改 `Assets/Scenes/GameScene.unity`（好捅天破负责）
- ❌ 在MainScene加载别人的未完成Prefab
- ❌ 提交 `.unity` 文件（除非是场景负责人）

**提倡**：
- ✅ 在自己的TestScene测试
- ✅ 使用Prefab变体（Prefab Variant）
- ✅ 通过UIManager动态加载/卸载Panel

### 5.3 冲突解决

**Prefab冲突**：
- Unity的 `.prefab` 文件是文本，可以合并
- 合并后测试Prefab是否正常

**脚本冲突**：
- 优先沟通，避免同时修改同一文件
- 好捅天破负责协调接口定义

---

## 六、检查清单

### 6.1 提交前检查

**Prefab提交**：
- [ ] Prefab放在正确的文件夹分类下
- [ ] Prefab命名符合规范
- [ ] 在对应测试场景中测试通过
- [ ] 没有引用无关资源
- [ ] 删除了空的/废弃的GameObject

**脚本提交**：
- [ ] 代码符合命名规范
- [ ] 添加了必要的注释
- [ ] 没有编译错误和警告
- [ ] 通过测试场景验证

### 6.2 合并前检查

- [ ] 从main拉取最新代码
- [ ] 解决所有冲突
- [ ] 本地测试通过
- [ ] 通知好捅天破review

---

## 七、待办事项

### 立即执行（好捅天破）

- [ ] 创建 `Assets/UI/GameScene/` 文件夹结构
- [ ] 创建测试场景 `Assets/Scenes/Test/Test_Amao.unity`
- [ ] 创建测试场景 `Assets/Scenes/Test/Test_Oxy.unity`
- [ ] 发送本规范文档给阿喵和oxy

### oxy待办（优先）

- [ ] 整理之前做的Prefab到 `Assets/UI/Others/` 下
- [ ] 按场景分组：MainMenu、Story、Common
- [ ] 标注每个Prefab的用途（在Inspector描述里写清楚）
- [ ] 删除废弃的Prefab
- [ ] 完成后通知好捅天破review

### 阿喵待办

- [ ] 熟悉 `Assets/UI/GameScene/` 结构
- [ ] 在 `Test_Amao.unity` 中搭建测试环境
- [ ] 开始开发游戏内UI组件

### 后续协作

- [ ] 阿喵和oxy在 `UI/GameScene/` 下并行开发
- [ ] 每天由好捅天破合并到主场景
- [ ] 定期review和规范优化

---

## 八、问题反馈

遇到规范问题或有建议，联系好捅天破。
