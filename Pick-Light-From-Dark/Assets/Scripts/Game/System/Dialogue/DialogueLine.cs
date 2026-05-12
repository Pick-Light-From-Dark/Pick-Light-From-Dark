public class DialogueLine
{
    public string type;           // 旁白 / 场景 / 对话 / 选项 / 指令
    public string speaker;        // 说话人
    public string content;        // 内容
    public string choice1;        // 选择1文本
    public string choice2;        // 选择2文本
    public string choice1Result;  // 选择1后的文本
    public string choice2Result;  // 选择2后的文本
    public string choice1Id;      // 选择1标识（如 eat1）
    public string choice2Id;      // 选择2标识（如 eat2）

    // 显式演出指令（与文本解耦）
    public string bg;             // [bg:xxx] 或 [bg:xxx,fade] — 后景（向后兼容）
    public string mg;             // [mg:xxx] — 中景
    public string fg;             // [fg:xxx] — 前景
    public string transition;     // 转场类型（fade / slide 等）
    public bool isLayerCommand;   // true = [layer:...] 全换指令，仅显式指定的层更新
    public string solid;          // [solid:#000000] 或 [solid:black,fade] — 纯色背景
    public string se;             // [se:xxx]
    public string bgm;            // [bgm:xxx]
    public float wait;            // [wait:6] — 暂停秒数
}
