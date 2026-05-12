/// <summary>
/// 视觉小说剧情结束后的分支类型
/// </summary>
public enum VNExitType
{
    /// <summary>正常结束（无特殊分支）</summary>
    None,
    /// <summary>进入游玩（打牌）</summary>
    Gameplay,
    /// <summary>进入结局</summary>
    Ending,
    /// <summary>进入下一关/下一预制体</summary>
    NextLevel
}
