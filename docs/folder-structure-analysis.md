# 项目文件夹结构分析报告

## 🔴 严重问题

### 1. 嵌套重复的 `Assets/Assets/` 文件夹

**问题**:
```
Assets/
└── Assets/          ❌ 重复！
    ├── Raw and SpriteSheets/
    ├── Scenes/
    └── UI Elements/
```

**原因**: Oxy 的分支创建了 `Assets/Assets/` 结构

**建议**: 移动到正确位置
```
Assets/
├── RawAssets/       (原 Assets/Assets/Raw and SpriteSheets)
├── _Scenes_UI/      (原 Assets/Assets/Scenes - UI 测试场景)
└── UIElements/      (原 Assets/Assets/UI Elements)
```

---

### 2. 编辑器工具混在项目资源中

**问题**:
```
Assets/
└── AssetBundles-Browser-master/  ❌ 不应该在这里
```

**建议**: 移到专门的编辑器工具文件夹
```
Assets/
└── _EditorTools/
    └── AssetBundlesBrowser/
```

---

## 🟡 中等问题

### 3. System 文件夹与命名空间不一致

**问题**:
```
Scripts/Game/System/TaskManager.cs    ❌ 文件夹是 System
namespace Game.Task                   ✅ 命名空间是 Task
```

**建议**: 重命名文件夹
```
Scripts/Game/System/  →  Scripts/Game/Task/
```

---

### 4. 场景文件分散

**当前分布**:
```
Assets/
├── Scenes/
│   ├── GameScene.unity       ✅ 主游戏场景
│   ├── OneClickTest.unity    ⚠️ 测试场景
│   └── SampleScene.unity     ⚠️ Unity 默认场景
├── amiao.unity               ⚠️ 在根目录？应该移到 Scenes/
└── Assets/Scenes/            ⚠️ 重复的 Scenes 文件夹
```

**建议**: 统一场景管理
```
Assets/
└── Scenes/
    ├── _Active/
    │   └── GameScene.unity
    ├── _Tests/
    │   ├── OneClickTest.unity
    │   └── amiao.unity
    └── _Archive/
        └── SampleScene.unity
```

---

## 🟢 轻微问题

### 5. 缺少文档文件夹

**问题**: 项目文档分散在根目录
```
项目根/
├── amiao TODO.md       ⚠️ 应该统一管理
└── docs/               ✅ 已有，但内容可以更丰富
```

**建议**:
```
项目根/
├── README.md           (项目说明)
├── docs/
│   ├── design/         (设计文档)
│   ├── api/            (API 文档)
│   ├── integration/    (集成文档)
│   └── plans/          (施工计划)
└── .github/
    └── wiki/           (GitHub Wiki)
```

---

### 6. Sprites 文件夹结构

**当前**:
```
Sprites/
└── Characters/
    └── LuYing/
        ├── Blink_01~05.png
        └── Eff_Sweat.png
```

**建议**: 按功能分类
```
Sprites/
├── Characters/
│   ├── LuYing/
│   │   ├── Idle/
│   │   ├── Blink/
│   │   └── Effects/
│   └── [其他角色]
├── UI/
│   ├── Icons/
│   ├── Buttons/
│   └── Panels/
└── Effects/
```

---

## ✅ 推荐的标准文件夹结构

```
Pick-Light-From-Dark/
├── Assets/
│   ├── _Animation/              (动画 - 下划线表示系统资源)
│   ├── _Audio/                  (音频)
│   ├── _EditorTools/            (编辑器工具)
│   │   └── AssetBundlesBrowser/
│   ├── _Materials/              (材质)
│   ├── _Prefabs/                (预制体)
│   │   ├── UI/
│   │   ├── Cards/
│   │   └── Characters/
│   ├── _Scenes/                 (场景)
│   │   ├── _Active/             (当前使用的)
│   │   ├── _Tests/              (测试场景)
│   │   └── _Archive/            (归档)
│   ├── _Scripts/                (脚本)
│   │   ├── Framework/           (框架代码)
│   │   ├── Game/                (游戏逻辑)
│   │   │   ├── AI/
│   │   │   ├── Card/
│   │   │   ├── Config/
│   │   │   ├── Data/
│   │   │   ├── Emotion/
│   │   │   ├── Flow/
│   │   │   └── Task/            (原 System)
│   │   ├── UI/
│   │   └── Editor/
│   ├── _Sprites/                (图片)
│   │   ├── Characters/
│   │   ├── UI/
│   │   └── Effects/
│   ├── Resources/               (运行时加载的资源)
│   │   ├── Card/
│   │   ├── Font/
│   │   ├── Sound/
│   │   ├── TestData/
│   │   └── UI/
│   └── StreamingAssets/         (流式资源)
├── docs/                        (项目文档)
├── .github/                     (GitHub 配置)
├── ProjectSettings/             (Unity 项目设置)
└── README.md                    (项目说明)

说明：
- 下划线前缀 (_) 表示系统/核心文件夹，排序靠前
- camelCase 表示功能文件夹
- 大写开头表示公开资源
```

---

## 🎯 立即行动项

1. **紧急**: 移动 `Assets/Assets/` 内容到正确位置
2. **高优先级**: 重命名 `Scripts/Game/System/` → `Scripts/Game/Task/`
3. **中优先级**: 整理场景文件到 `_Scenes/_Tests/`
4. **低优先级**: 创建标准文档结构

---

## 📋 需要团队讨论的问题

1. 是否接受下划线前缀命名规范？
2. `Raw and SpriteSheets` 文件夹的用途是什么？
3. 是否需要保留所有测试场景？
4. AssetBundle 的资源加载策略？
