# 工作目录限制

## 白名单（允许修改）

- `Pick-Light-From-Dark/Assets/Scripts/Game/Test/amiao/`
- `Pick-Light-From-Dark/Assets/Scenes/Amiao_Test/`
- `Pick-Light-From-Dark/Assets/Resources/Dialogue/`

## 黑名单（禁止修改）

- `Pick-Light-From-Dark/Assets/Scripts/Game/Flow/`（GameFlowController、LevelFlowCoordinator 等核心流程）
- `Pick-Light-From-Dark/Assets/Scripts/Game/Card/`（CardManager 等核心卡牌系统）
- `Pick-Light-From-Dark/Assets/Scripts/Game/AI/`（TeacherAI 等核心 AI）
- `Pick-Light-From-Dark/Assets/Scripts/UI/GamePanel.cs`（核心 UI）
- `Pick-Light-From-Dark/Assets/Resources/TestData/`（TestLevelConfig 等共享配置）
- `Pick-Light-From-Dark/Assets/Resources/Config/`（关卡配置）
- `Pick-Light-From-Dark/Assets/Scripts/Game/Emotion/`（情绪系统）
- `Pick-Light-From-Dark/Assets/Scripts/Game/Data/`（核心数据结构）
- `Pick-Light-From-Dark/Assets/Scripts/Game/Task/`（任务系统）
- `Pick-Light-From-Dark/Assets/Scripts/Game/EyeClose/`（闭眼系统）
- `Pick-Light-From-Dark/Assets/Scripts/Framework/`（框架层）

## 规则说明

1. **除非用户明确要求，否则绝不修改黑名单中的文件。**
2. **如果白名单中的文件需要引用黑名单中的文件（如 Prefab 引用共享脚本），只修改白名单文件本身，不修改被引用的黑名单文件。**
3. **如果任务确实需要修改黑名单中的文件，必须先向用户明确请求许可，说明修改理由和影响范围。**
4. **禁止修改 `.claude/settings.local.json` 和 `CLAUDE.md` 本身。**
