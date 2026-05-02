using System;
using UnityEngine;

/// <summary>
/// 《灯下黑》游戏事件定义
/// 在Framework的E_EventType基础上扩展游戏特定事件
/// </summary>
public static class GameEvents
{
    // ==================== 游戏流程事件 ====================

    public const string GameStart = "GameStart";
    public const string GamePause = "GamePause";
    public const string GameResume = "GameResume";
    public const string GameWin = "GameWin";
    public const string GameLose = "GameLose";
    public const string LevelStart = "LevelStart";
    public const string LevelComplete = "LevelComplete";

    // ==================== 卡牌事件 ====================

    public const string CardDragStart = "CardDragStart";
    public const string CardDragEnd = "CardDragEnd";
    public const string CardEnterThinkingBox = "CardEnterThinkingBox";
    public const string CardLeaveThinkingBox = "CardLeaveThinkingBox";
    public const string CardReadStart = "CardReadStart";
    public const string CardReadComplete = "CardReadComplete";
    public const string CardReadInterrupt = "CardReadInterrupt";

    // ==================== 情绪值事件 ====================

    public const string EmotionChanged = "EmotionChanged";
    public const string PanicChanged = "PanicChanged";
    public const string ExciteChanged = "ExciteChanged";

    // ==================== 闭眼事件 ====================

    public const string PlayerEyeCloseChanged = "PlayerEyeCloseChanged";
    public const string EyeCloseStart = "EyeCloseStart";
    public const string EyeCloseEnd = "EyeCloseEnd";

    // ==================== 老师AI事件 ====================

    public const string TeacherStateChanged = "TeacherStateChanged";
    public const string TeacherInspectStart = "TeacherInspectStart";
    public const string TeacherInspectEnd = "TeacherInspectEnd";
    public const string TeacherFootstepStart = "TeacherFootstepStart";
    public const string PlayerCaught = "PlayerCaught";

    // ==================== UI事件 ====================

    public const string ShowMainMenu = "ShowMainMenu";
    public const string ShowGamePanel = "ShowGamePanel";
    public const string ShowResultPanel = "ShowResultPanel";
    public const string ClosePanel = "ClosePanel";

    // ==================== 音效事件 ====================

    public const string PlayCardCompleteSound = "PlayCardCompleteSound";
    public const string PlayCaughtSound = "PlayCaughtSound";
    public const string PlayEyeCloseSound = "PlayEyeCloseSound";
}
