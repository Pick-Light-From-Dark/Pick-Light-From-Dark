# AI 对话生成测试系统

隔离测试文件夹，用于实验本地 LLM 驱动的随机对话生成。

## 文件结构

```
AI_Dialogue_Test/
├── llama/                          # llama.cpp 可执行文件（gitignored）
│   ├── llama-server.exe
│   └── llama.dll
├── models/                         # GGUF 模型文件（gitignored）
│   └── qwen2.5-0.5b-instruct-q4_k_m.gguf
├── Scripts/
│   ├── AIDialogueGenerator.cs      # 启动 llama-server，HTTP API 调用
│   ├── AIStyleTrainer.cs           # 从文案提取风格示例，构建 few-shot prompt
│   ├── AIPlayerHistory.cs          # 玩家选择历史（PlayerPrefs 存储）
│   └── AIDialogueTestController.cs # 测试控制器：串联组件 + 测试 UI
└── README.md
```

## 使用步骤

### 1. 准备环境

确保以下文件已放入对应文件夹（已在 .gitignore 中排除，不会提交）：

- `llama/llama-server.exe` — llama.cpp Windows 预编译二进制
- `llama/llama.dll` — 依赖库
- `models/qwen2.5-0.5b-instruct-q4_k_m.gguf` — Qwen 模型文件

### 2. 创建测试场景

1. 在 Unity 中新建场景 `AI_Dialogue_Test.unity`（放于此文件夹）
2. 创建空 GameObject，命名为 `AI_Test`
3. 挂载 `AIDialogueTestController.cs`
4. 在 Inspector 中，将 `Assets/Resources/Dialogue1-1.txt` 拖到 **Style Sample Text** 字段

### 3. 运行测试

1. 点击 Play
2. `AIDialogueGenerator` 会自动启动 `llama-server.exe` 子进程
3. 等待状态显示"服务器状态: 就绪"（首次启动约需 5-10 秒加载模型）
4. 点击「记录测试选择」添加玩家历史
5. 输入情境描述，点击「生成对话」
6. 生成结果会显示在下方面板

## 组件说明

### AIDialogueGenerator
- 启动/停止 llama-server 子进程
- 通过 HTTP POST `/v1/chat/completions` 调用模型
- 手动解析 OpenAI 兼容格式的 JSON 响应

### AIStyleTrainer
- 读取 TextAsset（如 Dialogue1-1.txt）提取对话行
- 构建包含 few-shot 示例的 system prompt
- 支持 `{STYLE_EXAMPLES}`、`{PLAYER_HISTORY}`、`{CONTEXT}` 占位符替换

### AIPlayerHistory
- 使用 `PlayerPrefs` 存储玩家选项历史
- 自动限制最大记录条数（默认 20）
- `RecordChoice()` / `GetFormattedHistory()` / `ClearHistory()`

### AIDialogueTestController
- 运行时自动创建测试 UI（Canvas + InputField + Button + Text）
- 串联三个组件，一键生成对话
- 提供「记录选择」「清空历史」「生成对话」三个操作按钮
