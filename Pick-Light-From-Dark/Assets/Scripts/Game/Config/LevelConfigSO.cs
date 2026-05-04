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
        public int flashPanicPerSec;

        [Header("初始卡牌")]
        public List<int> initialCards;

        [Header("生命值")]
        public int maxLives = 2;

        [Header("任务清单")]
        public List<TaskGoal> taskGoals;
    }
}
