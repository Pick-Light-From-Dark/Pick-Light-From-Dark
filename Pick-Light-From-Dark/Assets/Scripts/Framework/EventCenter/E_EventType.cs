using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件中心 枚举
/// </summary>
public enum E_EventType
{
    /// <summary>
    /// 怪物死亡事件 事件参数 怪物Monster
    /// </summary>
    E_Monster_Dead,
    /// <summary>
    /// 玩家获取金钱 数据 参数为int
    /// </summary>
    E_Player_GetReward,
    /// <summary>
    /// 测试用事件 事件参数为字符串
    /// </summary>
    E_Test,
    /// <summary>
    /// 场景加载切换时，进度获取
    /// </summary>
    E_SceneLoadChange,

    /// <summary>
    /// 技能系统按下技能1 动作
    /// </summary>
    E_Input_Skill1,
    /// <summary>
    /// 技能系统按下技能2 动作
    /// </summary>
    E_Input_Skill2,
    /// <summary>
    /// 技能系统按下技能3 动作
    /// </summary>
    E_Input_Skill3,

    /// <summary>
    /// 水平等级 -1~1之间（事件参数）
    /// </summary>
    E_Input_Horizontal,

    /// <summary>
    /// 垂直等级 -1~1之间（事件参数）
    /// </summary>
    E_Input_Vertical,

    // ==================== 《灯下黑》游戏事件 ====================

    GameStart,
    GamePause,
    GameResume,
    GameWin,
    GameLose,
    LevelStart,
    LevelComplete,

    CardDragStart,
    CardDragEnd,
    CardEnterThinkingBox,
    CardLeaveThinkingBox,
    CardReadStart,
    CardReadComplete,
    CardReadInterrupt,

    EmotionChanged,
    PanicChanged,
    ExciteChanged,
    /// <summary>
    /// 情绪总和首次跨过 criticalValue 触发 无参
    /// </summary>
    EmotionCritical,
    /// <summary>
    /// 情绪总和首次回落 criticalValue 之下触发 无参
    /// </summary>
    EmotionRecovered,

    PlayerEyeCloseChanged,
    EyeCloseStart,
    EyeCloseEnd,
    /// <summary>
    /// 闭眼超 10 秒 时间开始加速
    /// </summary>
    EyeCloseTimeAccelerated,
    /// <summary>
    /// 时间恢复常速
    /// </summary>
    EyeCloseTimeNormal,

    TeacherStateChanged,
    TeacherInspectStart,
    TeacherInspectEnd,
    TeacherFootstepStart,
    PlayerCaught,

    ShowMainMenu,
    ShowGamePanel,
    ShowResultPanel,
    ClosePanel,

    PlayCardCompleteSound,
    PlayCaughtSound,
    PlayEyeCloseSound,

    TaskProgressChanged,
    TaskGoalCompleted,

    /// <summary>
    /// 背景画面跳转（int参数=背景画面ID）
    /// </summary>
    BackgroundJump,

    /// <summary>
    /// 剧情选项被选中（string参数=选项标识，如"eat1""not_eat"）
    /// </summary>
    StoryChoiceMade,
}
