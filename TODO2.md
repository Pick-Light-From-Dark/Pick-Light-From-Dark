# TODO2 - 对话系统视觉小说效果完善

## 需求来源
使用 Assets/Art 里的美术素材，在 Amiao_Test 场景展示一段剧情，像视觉小说一样呈现。

## 任务清单

### 1. 素材调查与映射配置
- [x] 盘点 Assets/Art 可用素材
  - DialogueTestArt: 宋明月.png、寝室.png、陆萤.png
  - Characters/cg: Song_talk_1/2.png, phone_connect_1/2/3.png
  - Characters/LuYing: 眨眼/咀嚼动画帧
  - Scene: Lying in bed_looking towards the door
- [x] 配置 RoleConfig（角色名 -> Sprite 映射）
- [x] 配置 BackgroundConfig（场景名 -> Sprite 映射）

### 2. GalDialoguePanel 扩展 - 背景划入划出
- [x] 添加背景图划入动画（从屏幕左侧滑入）
- [x] 添加背景图划出动画（向左侧滑出）
- [x] 在场景切换时自动触发划入划出

### 3. GalDialoguePanel 扩展 - 立绘淡入淡出
- [x] 添加角色立绘淡入效果（切换时）
- [x] 添加角色立绘淡出效果（切换时）
- [x] 配置 DialogueSystem 在切换角色时触发动画

### 4. GalDialoguePanel 扩展 - 打字机效果
- [x] 参考 GamePanel 思考框打字机实现（TypewriterEffect）
- [x] 给 GalDialoguePanel 的 contentText 添加逐字显示
- [x] 支持点击跳过（立即显示全部）
- [x] 打字速度可配置

### 5. 测试场景搭建
- [x] 新增 DialogueVNTest.cs：自包含 VN 测试器
- [x] 新增 OneClickVNTestSetup.cs：编辑器菜单一键创建测试场景
- [x] 运行验证对话展示效果

### 6. 剧本文档（可选）
- [x] 新增 DialogueFormatSuggestion.md：对话文本结构化格式建议（JSON/YAML）

### 7. 额外任务 - SweatDripController DOTween 化
- [x] 分析现有 SweatDripController（Shader + Update Lerp，无 DOTween）
- [x] 用 DOTween 重写滴落位置动画（DOLocalMove + Sequence）
- [x] 用 DOTween 重写 Alpha 淡入淡出（DOFade）
- [x] 保持原有 Shader 效果作为 fallback

## 约束
- 不修改 Dialogue1.txt 任何一个字
- 使用已有前端（GalDialoguePanel.prefab）
- 参考 GamePanel.cs 第 575-660 行打字机实现
