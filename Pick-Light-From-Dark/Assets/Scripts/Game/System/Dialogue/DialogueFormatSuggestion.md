# 对话文本格式改进建议（非文学层面）

> 本文档仅针对**技术格式与导入效率**提出建议，不涉及任何剧情、台词、人物塑造等文学创作层面的修改。

## 当前格式的局限

当前 `Dialogue1.txt` 使用纯文本标记格式，虽然可读性好，但在游戏导入和后期维护上存在以下问题：

1. **解析容错性低**：依赖特定中文字符（`：`、`【`、`】`），容易因全角半角混用、空格差异导致解析失败
2. **元数据缺失**：无法表达音效、BGM、立绘表情、背景过渡方式等视觉/听觉信息
3. **分支结构弱**：选项与结果的关联依赖行序隐式推断，修改时容易错位
4. **无唯一标识**：每行没有 ID，调试时难以快速定位
5. **不支持变量/条件**：无法根据游戏状态（如情绪值、已读章节）动态显示内容
6. **编码风险**：纯文本文件容易出现 BOM、换行符不一致等问题

## 建议格式：JSON/YAML 结构化方案

### 推荐：JSON 数组格式

```json
[
  {
    "id": "line_001",
    "type": "narration",
    "content": "还有十五分钟熄灯。",
    "bgm": "bgm_night",
    "bg": "寝室",
    "transition": "fade_in"
  },
  {
    "id": "line_002",
    "type": "dialogue",
    "speaker": "陆萤",
    "role_id": "luying",
    "expression": "worried",
    "content": "……还没有再整理一遍试卷的错题。",
    "sfx": "sfx_sigh"
  },
  {
    "id": "line_003",
    "type": "scene",
    "content": "寝室",
    "bg": "寝室",
    "transition": "slide_from_left"
  },
  {
    "id": "line_010",
    "type": "choice",
    "content": "",
    "options": [
      {
        "text": "-吃",
        "target": "line_011",
        "condition": "has_item:薯片"
      },
      {
        "text": "-不吃",
        "target": "line_015"
      }
    ]
  }
]
```

### 备选：YAML 格式（对策划更友好）

```yaml
lines:
  - id: line_001
    type: narration
    content: "还有十五分钟熄灯。"
    bgm: bgm_night
    bg: 寝室
    transition: fade_in

  - id: line_002
    type: dialogue
    speaker: 陆萤
    role_id: luying
    expression: worried
    content: "……还没有再整理一遍试卷的错题。"
    sfx: sfx_sigh

  - id: line_010
    type: choice
    options:
      - text: "-吃"
        target: line_011
        condition: "has_item:薯片"
      - text: "-不吃"
        target: line_015
```

## 字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| `id` | string | 唯一标识，用于调试和分支跳转 |
| `type` | enum | `narration`/`dialogue`/`scene`/`choice`/`card` |
| `speaker` | string | 说话人名称（显示用） |
| `role_id` | string | 角色唯一标识（用于 RoleConfig 映射） |
| `expression` | string | 表情/立绘变体（如 `worried`/`happy`/`angry`） |
| `content` | string | 对话/旁白内容（**一个字不改**） |
| `bg` | string | 背景图标识（BackgroundConfig 映射） |
| `transition` | string | 过渡动画：`fade_in`/`slide_from_left`/`slide_from_right` |
| `bgm` | string | 背景音乐标识 |
| `sfx` | string | 音效标识 |
| `options` | array | 选项数组（仅 `choice` 类型） |
| `target` | string | 跳转目标行 ID |
| `condition` | string | 条件表达式（可选） |

## 迁移收益

1. **解析稳定**：JSON/YAML 解析器成熟，不受空格、换行影响
2. **扩展性强**：新增字段不影响旧解析逻辑
3. **分支清晰**：选项与结果通过 `target` 显式关联，修改安全
4. **支持条件**：后期可接入任务系统、情绪系统做动态分支
5. **工具链友好**：可用 JSON Schema 做格式校验，用脚本批量处理
6. **版本控制友好**：diff 时行级对比更清晰

## 兼容性建议

- 现有 `DialogueParser.cs` 可保留作为**向后兼容的 Fallback**
- 新增 `DialogueJsonParser.cs` 处理 JSON/YAML 格式
- 在 `DialogueSystem.StartDialogue()` 中根据文件扩展名自动选择解析器（`.txt` → 旧解析器，`.json` → JSON 解析器）
