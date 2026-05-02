# Pick-Light-From-Dark

> GameJam 项目 - 潜行解谜游戏

## 📁 项目结构

```
Pick-Light-From-Dark/
├── Assets/                    # Unity 项目资源
│   ├── Art/                   # 团队原创素材
│   │   ├── Animations/        # 角色动画
│   │   ├── Characters/        # 角色精灵图
│   │   ├── UI/                # UI 素材
│   │   └── Effects/           # 特效素材
│   ├── Audio/                 # 音频素材
│   │   ├── Music/             # 背景音乐
│   │   └── SFX/               # 音效
│   ├── Resources/             # 运行时动态加载
│   │   ├── Card/              # 卡牌数据
│   │   ├── Font/              # 字体
│   │   ├── Sound/             # 音效源预制体
│   │   ├── TestData/          # 测试数据
│   │   └── UI/                # UI 预制体
│   ├── Scenes/                # Unity 场景
│   ├── Scripts/               # C# 脚本
│   │   ├── Framework/         # 框架代码
│   │   └── Game/              # 游戏逻辑
│   └── ThirdParty/            # 第三方资源
├── docs/                      # 项目文档
├── ProjectSettings/           # Unity 项目设置
└── README.md                  # 本文件
```

## 🎮 核心系统

### 框架层（`Scripts/Framework/`）
- **EventCenter** - 事件中心，模块间通信
- **Music** - 音频管理（支持直接引用和 Resources 加载）
- **Pool** - 对象池管理
- **Res** - 资源加载（Resources.Load）
- **Scene** - 场景加载管理
- **UI** - UI 面板管理

### 游戏层（`Scripts/Game/`）
- **AI** - 老师 AI 行为
- **Card** - 卡牌读条系统
- **Config** - ScriptableObject 配置
- **Data** - 数据存储
- **Emotion** - 情绪系统（慌乱值/兴奋值）
- **EyeClose** - 闭眼检测系统
- **Flow** - 游戏流程控制
- **Task** - 任务管理系统

## 🚀 快速开始

### 开发环境
- Unity 2022.3 LTS
- Git

### 启动项目
1. 克隆仓库
2. 用 Unity 打开项目根目录
3. 打开 `Scenes/GameScene.unity`

### 添加新素材
- 美术原创 → `Assets/Art/[对应子文件夹]`
- 音频文件 → `Assets/Audio/Music/` 或 `Assets/Audio/SFX/`
- 需要运行时加载 → `Assets/Resources/`

### 添加新代码
- 框架功能 → `Scripts/Framework/[新模块]/`
- 游戏逻辑 → `Scripts/Game/[新系统]/`
- 命名空间与文件夹保持一致

## 📚 文档

- [📖 文档阅读指南](docs/README.md) - 团队协作必读
- [📁 文件夹结构说明](docs/folder-structure-analysis.md) - 详细的目录组织
- [🤖 技术设计说明](docs/技术设计说明.md) - 代码规范和架构
- [👥 团队协作指南](docs/团队协作指南.md) - 分工与工作流

## 👥 团队成员

- **NikolaTheCat** - 场景、UI、美术音效
- **Vol-Time** - 数据配置、简单系统
- **amlm155 (阿喵)** - 数据配置、简单系统
- **tianpo** - 核心架构、复杂系统

## 🛠️ 技术栈

- **语言**: C#
- **架构**: 事件驱动 + 单例模式
- **资源加载**: Resources.Load + 直接引用
- **UI框架**: Oxy 的 UnityUIKit
- **文本**: TextMesh Pro

## 📝 更新日志

### 2026-05-01 - 项目结构重构
- ✅ 删除未使用的 AssetBundle 系统
- ✅ 统一资源管理（Resources + 直接引用）
- ✅ 重命名 Game/System → Game/Task
- ✅ 整理文件夹结构（Art、Audio、ThirdParty）
- ✅ 清理空文件夹和重复资源
