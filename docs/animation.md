# 动画资源目录与规范

> 适用：《灯下黑》Q 版角色动画系统  
> 角色：LuYing（陆萤）  
> 技术方案：SpriteRenderer + Animator（Sprite Swap 逐帧动画）

---

## 一、目录结构

```
Assets/
├── Art/
│   ├── Animations/                          # AnimatorController + .anim 剪辑
│   │   ├── LuYing_Blink.anim                # 床上-眨眼
│   │   ├── LuYing_LittleExcited.anim        # 床上-微兴奋
│   │   ├── LuYing_Excited.anim              # 床上-兴奋
│   │   ├── LuYing_Chew.anim                 # 床上-咀嚼（卡牌动作）
│   │   ├── LuYingStand_Blink.anim           # 站立-眨眼
│   │   ├── LuYingStand_LittleExcited.anim   # 站立-微兴奋
│   │   ├── LuYingStand_Excited.anim         # 站立-兴奋
│   │   ├── LuYingStand_Chew.anim            # 站立-咀嚼
│   │   ├── LuYing_Blink.controller          # 床上 Blink 控制器
│   │   ├── LuYing_LittleExcited.controller
│   │   ├── LuYing_Excited.controller
│   │   ├── LuYing_Chew.controller
│   │   ├── LuYingStand_Blink.controller     # 站立 Blink 控制器
│   │   ├── LuYingStand_LittleExcited.controller
│   │   ├── LuYingStand_Excited.controller
│   │   └── LuYingStand_Chew.controller
│   ├── Characters/
│   │   └── LuYing/                          # 角色原始素材
│   │       ├── Blink_01.png ~ Blink_05.png  # 眨眼序列帧
│   测试脚本都写在哪个文件夹里？│       ├── little_excited1.png ~ 5.png  # 微兴奋序列帧
│   │       ├── excited1.png ~ excited5.png  # 兴奋序列帧
│   │       ├── chew_01.png ~ chew_04.png    # 咀嚼序列帧
│   │       ├── excited_special.png          # 兴奋特殊表情（静态）
│   │       ├── sweating.png                 # 汗滴素材
│   │       └── Eff_Sweat.png              # 汗滴特效素材
│   └── Shaders/
│       ├── SweatDrip.shader                 # 汗滴滴落 shader
│       └── SpriteOutline.shader             # 角色描边 shader
├── Scenes/
│   └── Amiao/
│       ├── TestPrefabs/
│       │   └── EmotionSystemTester.prefab   # 集成动画测试的 Prefab
│       └── AmiaoTestScene.unity             # 动画测试场景
└── Scripts/
    └── Game/
        ├── Emotion/
        │   └── EmotionSystem.cs             # 情绪值驱动动画切换
        ├── Effects/
        │   ├── SweatDripController.cs       # 汗滴特效控制器
        │   └── SpriteOutlineController.cs   # 描边效果控制器
        └── Testing/
            └── RandomBlinkController.cs     # 眨眼随机冻结控制器
```

---

## 二、动画资源对照表

### 床上姿态（LuYing_ 前缀）

| 动画文件 | 控制器 | 序列帧素材 | 循环 | 随机触发 | 备注 |
|---|---|---|---|---|---|
| `LuYing_Blink.anim` | `LuYing_Blink.controller` | `Blink_01~05.png` | Ping-pong | 是 | 眨眼，间隔 2~5 秒随机播放一次 |
| `LuYing_LittleExcited.anim` | `LuYing_LittleExcited.controller` | `little_excited1~5.png` | Ping-pong | 是 | 微兴奋 |
| `LuYing_Excited.anim` | `LuYing_Excited.controller` | `excited1~5.png` | Ping-pong | 是 | 高兴奋 |
| `LuYing_Chew.anim` | `LuYing_Chew.controller` | `chew_01~04.png` | Ping-pong | 否 | 咀嚼动作，卡牌触发 |

### 站立姿态（LuYingStand_ 前缀）

| 动画文件 | 控制器 | 序列帧素材 | 循环 | 随机触发 | 备注 |
|---|---|---|---|---|---|
| `LuYingStand_Blink.anim` | `LuYingStand_Blink.controller` | `Blink_01~05.png` | Ping-pong | 是 | 站立眨眼 |
| `LuYingStand_LittleExcited.anim` | `LuYingStand_LittleExcited.controller` | `little_excited1~5.png` | Ping-pong | 是 | 站立微兴奋 |
| `LuYingStand_Excited.anim` | `LuYingStand_Excited.controller` | `excited1~5.png` | Ping-pong | 是 | 站立兴奋 |
| `LuYingStand_Chew.anim` | `LuYingStand_Chew.controller` | `chew_01~04.png` | Ping-pong | 否 | 站立咀嚼 |

### 素材复用规则

- 床上/站立的 **Blink**、**LittleExcited**、**Excited**、**Chew** 共用同一套序列帧素材
- 通过 `Animator.runtimeAnimatorController` 在运行时切换，实现姿态差分
- `EmotionSystemTester` 中通过 `PlayerState.IsInBed()` 决定使用 `luYingControllers[]` 还是 `luYingStandControllers[]`

---

## 三、动画切换规则

### 情绪值驱动（由 `EmotionSystemTester.UpdateCharacterAnimation()` 实现）

```
excitedValue < 50              → Blink（眨眼待机）
50 ≤ excitedValue < 70         → LittleExcited（微兴奋）
excitedValue ≥ 70              → Excited（高兴奋）
```

### 姿态切换（由 `PlayerState.IsInBed()` 决定）

```
IsInBed() == true   → 使用 LuYing_* 系列控制器
IsInBed() == false  → 使用 LuYingStand_* 系列控制器
```

### 卡牌动作（单次触发）

- 使用特定卡牌时，由外部系统调用切换 AnimatorController 到 `Chew`
- 播放完成后切换回情绪值对应的常态动画

---

## 四、序列帧规范

### Ping-pong 播放格式

所有表情动画采用 `1-2-3-4-5-4-3-2-1` 顺序循环：
- 第 1 帧和最后一帧：`3/12 秒`（约 0.25s）
- 中间帧：`2/12 秒`（约 0.167s）
- `SampleRate = 12`
- `LoopTime = 1`

### 随机触发逻辑（`RandomBlinkController.cs`）

- 平时 `animator.speed = 0`，冻结在第一帧
- 随机间隔后 `animator.speed = 1`，播放一轮
- 不同表情有不同随机间隔：
  - Blink：`2~5` 秒
  - LittleExcited：`2~5` 秒
  - Excited：`3~6` 秒
- 动作类动画（Chew）保持正常循环播放

---

## 五、特效资源

### 汗滴效果（`SweatDripController`）

| 配置项 | 默认值 | 说明 |
|---|---|---|
| 触发条件 | 外部显式调用 `Play()` | 不自动绑定情绪值 |
| 材质 | `Custom/SweatDrip` | 支持透明度、纵向拉伸、底部淡出 |
| 滴落方向 | 左上 → 左下 | `spawnOffset` + `dripOffsetX` 控制 |
| 单轮周期 | 2 秒 | `dripCycle` |

### 描边效果（`SpriteOutlineController`）

| 配置项 | 默认值 | 说明 |
|---|---|---|
| 启用状态 | 默认开启 | `enableOutline = true` |
| 描边颜色 | `Color(0,0,0,0.6)` | 黑色半透明影子 |
| 描边粗细 | 3 | `outlineSize`，像素单位 |
| Shader | `Custom/SpriteOutline` | 双 Pass 外描边 |

---

## 六、命名规范

| 类型 | 命名格式 | 示例 |
|---|---|---|
| 动画剪辑 | `{Role}_{State}.anim` | `LuYing_Blink.anim` |
| 动画控制器 | `{Role}_{State}.controller` | `LuYing_Blink.controller` |
| 序列帧 | `{state}_{序号}.png` | `Blink_01.png`、`excited3.png` |
| 站立差分 | `{Role}Stand_{State}.anim` | `LuYingStand_Blink.anim` |
| 特效素材 | `Eff_{Effect}.png` | `Eff_Sweat.png` |

---

## 七、扩展指南

### 新增一种表情动画

1. 准备序列帧素材 `new_emotion1.png ~ new_emotionN.png`，放入 `Assets/Art/Characters/LuYing/`
2. 创建 `.anim` 文件，配置 `m_PPtrCurves` 指向序列帧，设置 Ping-pong 循环
3. 创建 `.controller` 文件，绑定 `.anim` 作为默认状态
4. 在 `EmotionSystemTester` 的 `luYingControllers[]` / `luYingStandControllers[]` 中新增槽位
5. 修改 `CharacterState` 枚举和 `UpdateCharacterAnimation()` 逻辑

### 新增特效

1. 创建 Shader（如 `Custom/NewEffect`），放入 `Assets/Art/Shaders/`
2. 创建 Controller 脚本（如 `NewEffectController.cs`），放入 `Assets/Scripts/Game/Effects/`
3. 在角色 Prefab 或 `EmotionSystemTester` 上挂载 Controller
4. 提供公共接口 `Play()` / `Stop()` 供外部系统调用
