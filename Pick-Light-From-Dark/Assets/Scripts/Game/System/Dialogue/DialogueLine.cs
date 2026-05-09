public class DialogueLine
{
    public string type;           // 旁白 / 场景 / 对话 / 选项 / 指令
    public string speaker;        // 说话人
    public string content;        // 内容
    public string choice1;        // 选择1文本
    public string choice2;        // 选择2文本
    public string choice1Result;  // 选择1后的文本
    public string choice2Result;  // 选择2后的文本

    // 显式演出指令（与文本解耦）
    public string bg;             // [bg:xxx]
    public string se;             // [se:xxx]
    public string bgm;            // [bgm:xxx]
}
