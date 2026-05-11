using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// 检查类型
    /// </summary>
    public enum InspectType
    {
        Eye,
        Flash
    }

    /// <summary>
    /// 老师状态
    /// </summary>
    public enum TeacherState
    {
        Idle,
        Approaching,
        Inspecting,
        Leaving
    }

    /// <summary>
    /// 关卡配置
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        [Header("基本信息")]
        public int levelId;
        public string levelName;
        public int timeLimit;

        [Header("初始状态")]
        public bool initialInBed = true;

        [Header("初始情绪值")]
        public int initialPanic = 35;
        public int initialExcite = 35;

        [Header("临界情绪值")]
        public int criticalValue = 80;

        [Header("闭眼配置")]
        public float eyeClosePanicDecreasePerSec = 1f;
        public float eyeCloseAccelerationThreshold = 10f;
        public float eyeCloseAccelerationMultiplier = 2f;

        [Header("巡逻配置")]
        public Vector2 patrolIntervals;
        public Vector2 patrolTime;
        public Vector2 eyeCheckDuration;
        public Vector2 flashCheckDuration;

        [Header("手电筒")]
        public float flashPanicPerSec;

        [Header("卡牌数据路径")]
        [Tooltip("Resources下的子路径，如 Card 或 TestData")]
        public string cardDataPath = "Card";

        [Header("初始卡牌")]
        public List<int> initialCards;

        [Header("生命值")]
        public int maxLives = 2;

        [Header("任务清单")]
        public List<TaskGoal> taskGoals;

        [Header("关卡剧情")]
        [Tooltip("前置对话文件名（Resources/Dialogue/ 下，不含扩展名）")]
        public string preDialogueFile = "";
        [Tooltip("通关后对话文件名")]
        public string postDialogueFile = "";
        [Tooltip("是否为选择关（选项2直接进结局）")]
        public bool isChoiceLevel = false;
        [Tooltip("选项2对应的结局对话文件名")]
        public string choice2EndingDialogueFile = "";
    }
}
