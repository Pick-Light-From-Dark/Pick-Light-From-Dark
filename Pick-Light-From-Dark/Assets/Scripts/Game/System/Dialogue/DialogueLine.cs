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

    public string choice1JumpTarget; // 选项1跳转目标段落名（block名）
    public string choice2JumpTarget; // 选项2跳转目标段落名（block名）
    public string choice1Action;    // 选项1后续动作: ending, gameplay, nextlevel
    public string choice2Action;    // 选项2后续动作: ending, gameplay, nextlevel

    // 显式演出指令（与文本解耦）
    public string bg;             // [bg:xxx] 或 [bg:xxx,fade] — 后景（向后兼容）
    public string mg;             // [mg:xxx] — 中景
    public string fg;             // [fg:xxx] — 前景
    public string transition;     // 转场类型（fade / slide / crossfade 等）
    public float transitionHold;  // fadehold 停留时间（秒），如 [bg:xxx,fade,1.5]
    public bool isLayerCommand;   // true = [layer:...] 全换指令，仅显式指定的层更新
    public string solid;          // [solid:#000000] 或 [solid:black,fade] — 纯色背景
    public string se;             // [se:xxx]
    public string bgm;            // [bgm:xxx]
    public float wait;            // [wait:6] — 暂停秒数
    public string action;         // [action:ending/gameplay/nextlevel] — 分支动作指令
    public string dialogAction;   // [hide_dialog] / [show_dialog] — 对话框显隐控制
    public string centerText;     // [center_text:xxx] — 居中大字显示
}
