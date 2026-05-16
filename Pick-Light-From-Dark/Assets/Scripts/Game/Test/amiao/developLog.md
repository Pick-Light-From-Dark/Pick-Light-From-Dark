# amiaoDemo 开发日志

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
