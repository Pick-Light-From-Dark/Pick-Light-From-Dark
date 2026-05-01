# 项目文件夹结构说明

## 📁 当前结构（2026-05-01）

```
Pick-Light-From-Dark/
├── Assets/
│   ├── Art/                          ← 团队原创素材
│   │   ├── Animations/               (动画)
│   │   ├── Characters/               (角色精灵图)
│   │   ├── UI/                       (UI素材)
│   │   └── Effects/                  (特效素材)
│   ├── Audio/                        ← 音频素材
│   │   ├── Music/                    (背景音乐)
│   │   └── SFX/                      (音效)
│   ├── Resources/                    ← 运行时动态加载
│   │   ├── Card/                     (卡牌数据)
│   │   ├── Font/                     (字体)
│   │   ├── Sound/                    (音效源预制体)
│   │   ├── TestData/                 (测试数据)
│   │   └── UI/                       (UI预制体)
│   ├── Scenes/                       ← 场景
│   │   ├── GameScene.unity           (主游戏场景)
│   │   └── [其他测试场景]
│   ├── Scripts/                      ← 代码
│   │   ├── Framework/                (框架代码)
│   │   │   ├── EventCenter/          (事件中心)
│   │   │   ├── Music/                (音乐管理)
│   │   │   ├── Pool/                 (对象池)
│   │   │   ├── Res/                  (资源加载-Resources)
│   │   │   ├── Scene/                (场景管理)
│   │   │   ├── Singleton/            (单例基类)
│   │   │   ├── Timer/                (计时器)
│   │   │   └── UI/                   (UI管理)
│   │   ├── Game/                     (游戏逻辑)
│   │   │   ├── AI/                   (老师AI)
│   │   │   ├── Card/                 (卡牌系统)
│   │   │   ├── Config/               (配置数据SO)
│   │   │   ├── Data/                 (数据存储)
│   │   │   ├── Emotion/              (情绪系统)
│   │   │   ├── EyeClose/             (闭眼系统)
│   │   │   ├── Flow/                 (游戏流程)
│   │   │   ├── Task/                 (任务系统)
│   │   │   ├── Testing/              (自动化测试)
│   │   │   └── UI/                   (游戏UI)
│   │   └── UI/                       (UI基类)
│   ├── ThirdParty/                   ← 第三方资源
│   │   ├── TextMesh Pro/             (Unity官方文本)
│   │   └── UnityUIKit/               (Oxy的UI框架)
│   └── ProjectSettings/              (Unity项目设置)
├── docs/                             ← 项目文档
│   ├── README.md
│   ├── folder-structure-analysis.md  (本文件)
│   ├── integration-flow.md           (系统集成说明)
│   ├── 团队协作指南.md
│   └── 技术设计说明.md
└── [其他配置文件]
```

## 🎯 设计原则

### 资源组织

| 文件夹 | 用途 | 加载方式 |
|--------|------|----------|
| `Art/` | 团队原创美术素材 | Inspector引用 |
| `Audio/` | 原始音频文件 | Inspector引用 或 Resources.Load |
| `Resources/` | 运行时动态加载的资源 | Resources.Load |
| `Scenes/` | Unity场景文件 | Build Settings |
| `ThirdParty/` | 第三方素材/工具 | - |

### 代码组织

| 命名空间 | 文件夹 | 职责 |
|----------|--------|------|
| `Framework` | `Scripts/Framework/` | 底层框架，与游戏逻辑无关 |
| `Game.*` | `Scripts/Game/` | 游戏业务逻辑 |

### 命名约定

- **文件夹**：PascalCase（如 `EventCenter`）
- **命名空间**：与文件夹结构对应（如 `Game.Flow`）
- **资源加载**：
  - 原素材优先直接引用（性能好）
  - 需要动态更新时用 Resources.Load

## 📋 已清理的问题

以下问题已在2026-05-01的整理中解决：

### ✅ 删除 AB 资源系统
- 删除了未使用的 `ABMgr`, `ABResMgr`, `UWQResMgr`
- 删除了 `EditorResMgr`（开发期专用，实际未使用）
- 统一使用 `Resources.Load` + 直接引用两种方式

### ✅ 重构 Game/System → Game/Task
- 解决了命名空间与文件夹不一致的问题
- `namespace Game.Task` 现在对应 `Scripts/Game/Task/`

### ✅ 清理重复和空文件夹
- 删除 `Assets/Assets/` 嵌套结构
- 删除 `Art/Scenes/`, `Art/Effects/` 空文件夹
- 删除 `Framework/AB/`, `Framework/EditorRes/`, `Framework/UWQ/` 空文件夹
- 移除 `Editor/ArtRes/` 未使用资源

### ✅ 整理第三方资源
- `TextMesh Pro` → `ThirdParty/TextMesh Pro/`
- `UnityUIKit` → `ThirdParty/UnityUIKit/`
- `AssetBundles-Browser` → 已删除（未使用）

## 🔄 维护建议

### 添加新素材时
- 美术原创 → 放 `Art/` 对应子文件夹
- 音频文件 → 放 `Audio/Music/` 或 `Audio/SFX/`
- 需要运行时动态加载 → 放 `Resources/`

### 添加新代码时
- 框架级功能 → `Scripts/Framework/[新模块]/`
- 游戏业务逻辑 → `Scripts/Game/[新系统]/`
- 命名空间与文件夹保持一致

### 合并分支时
- 检查是否创建了 `Assets/Assets/` 嵌套
- 第三方资源应放入 `ThirdParty/`
- 不要在根目录创建 `.unity` 场景文件

## 📝 历史问题记录

<details>
<summary>点击查看已解决的历史问题</summary>

### 1. Assets/Assets/ 嵌套问题（已解决）
**原问题**：Oxy 的分支创建了嵌套结构
**解决方案**：移动到正确位置并删除旧文件夹

### 2. AB 系统复杂但未使用（已解决）
**原问题**：AssetBundle 系统增加复杂度，但实际用的是 Resources
**解决方案**：删除 AB 相关代码，保留 Resources 加载方式

### 3. System 文件夹命名冲突（已解决）
**原问题**：`Game.System` 与 .NET 的 `System` 命名空间冲突
**解决方案**：重命名为 `Game.Task`

### 4. 编辑器工具混在资源中（已解决）
**原问题**：`AssetBundles-Browser-master` 在 `Assets/` 根目录
**解决方案**：移入 `ThirdParty/` 或删除（如不需要）

</details>
