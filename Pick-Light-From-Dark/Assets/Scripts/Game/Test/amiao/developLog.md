# amiaoDemo 开发日志

## 2026-05-16 — 参考 GameScene 方式重写 amiaoDemo

### 功能
参考 `LevelFlowCoordinator` 架构改写 `AmiaoDemoRunner`，使其真正调用 VN 控制器播放剧情：
- **剧情阶段**：设置 `vnController.dialogueText` 并调用 `RestartDialogue()` 真正播放剧情
- **回调串联**：`OnDialogueExit` / `OnDialogueComplete` 回调推进到游玩/结尾剧情/结局
- **预置 VN**：`amiaoDemo.unity` 场景中添加 `FungusVNController` GameObject
- 保留 IMGUI 游玩模拟（血量滑块 + 卡牌勾选）
- 第一关「不吃」分支通过 `VNExitType.Ending` 触发结局一

### 修改文件
- `Assets/Scripts/Game/Test/amiao/AmiaoDemoRunner.cs` — 重写
- `Assets/Scenes/Amiao_Test/amiaoDemo.unity` — 添加 FungusVNController
- `Assets/Scripts/Game/Test/amiao/需求/gameFlow.md` — 排版优化

### 如何测试
1. 打开 `amiaoDemo.unity`
2. Play Mode 后按 F3 显隐面板
3. 点击"新游戏"，观察 VN 剧情是否正常播放
4. 剧情结束后自动进入游玩面板
5. 通关后进入下一关剧情或结局判定

---

## 2026-05-16 — amiaoDemo 完整流程演示

### 功能
在 `amiaoDemo.unity` 中串联存档系统与结局判定，提供完整的 5 关流程演示：
- 新游戏 / 继续游戏（读档）
- 每关：剧情 → 游玩（血量选择 + 卡牌勾选）→ 自动存档
- 第五关结束后：自动判定结局（6002/6004/6005）
- 两卡全用时弹出木门选项（独自/邀请）

### 新增文件
- `Assets/Scripts/Game/Test/amiao/AmiaoDemoRunner.cs` — 主控制器（OnGUI 自包含面板）
- `Assets/Scripts/Game/Test/amiao/AmiaoDemoRunner.cs.meta`
- 修改 `Assets/Scenes/Amiao_Test/amiaoDemo.unity` — 挂载 AmiaoDemoRunner GameObject

### 如何测试
1. 打开 `amiaoDemo.unity`
2. Play Mode 后按 F3 显隐面板
3. 点击"新游戏"，逐关推进
4. 第二关可勾选"分享泡面(2017)"，第五关可勾选"寻求帮助(2026)"
5. 第五关通关后自动进入结局判定
6. 使用"打印存档摘要"和"读档"按钮验证 CrossLevelSaveSystem 状态

### 重要接口路径
- 存档系统：`Assets/Scripts/Game/Test/amiao/CrossLevelSaveSystem.cs`
- 结局判定：`CrossLevelSaveSystem.EvaluateEnding(int rooftopChoice)`
- Fungus 桥接：`Assets/Scripts/Game/Test/amiao/EndingConditionBridge.cs`
