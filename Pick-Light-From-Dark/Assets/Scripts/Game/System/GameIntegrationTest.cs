using UnityEngine;
using Game.Flow;
using Game.AI;
using Game.Emotion;
using Game.Card;
using Game.Config;
using Game.Data;
using System.Collections.Generic;

namespace Game.System
{
    /// <summary>
    /// 游戏系统集成测试
    /// 测试所有核心系统的协同工作
    /// </summary>
    public class GameIntegrationTest : MonoBehaviour
    {
        [Header("系统引用")]
        public GameFlowController gameFlow;
        public TeacherAI teacherAI;
        public EmotionSystem emotionSystem;
        public CardReadingSystem cardReadingSystem;

        [Header("测试配置")]
        public LevelConfigSO testLevelConfig;
        public bool autoStart = true;

        [Header("测试信息")]
        [SerializeField] private bool isInitialized;
        [SerializeField] private float gameTime;

        void Start()
        {
            // 获取系统实例
            gameFlow = GameFlowController.Instance;
            emotionSystem = EmotionSystem.Instance;

            // 创建AI实例
            GameObject teacherObj = new GameObject("TeacherAI");
            teacherAI = teacherObj.AddComponent<TeacherAI>();

            // 创建读条系统实例
            GameObject cardObj = new GameObject("CardReadingSystem");
            cardReadingSystem = cardObj.AddComponent<CardReadingSystem>();

            // 监听事件
            SetupEventListeners();

            // 自动开始
            if (autoStart)
            {
                InitializeGame();
            }
        }

        void Update()
        {
            if (!isInitialized) return;

            gameTime += Time.deltaTime;
        }

        void InitializeGame()
        {
            if (testLevelConfig == null)
            {
                Debug.LogError("[FAIL] 未配置testLevelConfig");
                return;
            }

            Debug.LogError("[PASS] 游戏系统初始化开始");

            // 初始化各个系统
            gameFlow.Initialize(testLevelConfig);
            teacherAI.Initialize(testLevelConfig);
            cardReadingSystem.Initialize(testLevelConfig);

            isInitialized = true;
            Debug.LogError($"[INFO] 目标: 在{testLevelConfig.timeLimit}秒内存活");
            Debug.LogError("[INFO] 5秒后自动测试卡牌读条");
            Debug.LogError("[INFO] 10秒后自动测试打断功能");

            // 5秒后测试卡牌读条
            Invoke(nameof(TestCardReading), 5f);

            // 10秒后测试打断
            Invoke(nameof(TestCardInterrupt), 10f);
        }

        void TestCardReading()
        {
            if (!isInitialized || gameFlow.IsGameOver()) return;

            Debug.LogError("[TEST] 开始卡牌读条测试");

            // 创建测试卡牌
            CardData testCard = new CardData
            {
                id = 100,
                cardName = "测试翻身",
                description = "测试用卡牌",
                segments = new List<Segment>
                {
                    new Segment(2f, true),
                    new Segment(2f, false)
                },
                panicDelta = 0,
                exciteDelta = -5,
                interruptPanicAdd = 10
            };

            CardInstance instance = new CardInstance(testCard, 999);
            cardReadingSystem.StartReading(instance);
        }

        void TestCardInterrupt()
        {
            if (!isInitialized || gameFlow.IsGameOver()) return;

            Debug.LogError("[TEST] 测试打断功能");
            bool success = cardReadingSystem.InterruptReading();
            Debug.LogError($"[RESULT] 打断测试: {(success ? "成功" : "失败")}");
        }

        void SetupEventListeners()
        {
            // 游戏流程事件
            EventCenter.Instance.AddEventListener(E_EventType.GameStart, OnGameStart);
            EventCenter.Instance.AddEventListener(E_EventType.GamePause, OnGamePause);
            EventCenter.Instance.AddEventListener(E_EventType.GameResume, OnGameResume);
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);

            // 老师AI事件
            EventCenter.Instance.AddEventListener<TeacherState>(E_EventType.TeacherStateChanged, OnTeacherStateChanged);
            EventCenter.Instance.AddEventListener<InspectType>(E_EventType.TeacherInspectStart, OnInspectStart);
            EventCenter.Instance.AddEventListener(E_EventType.TeacherInspectEnd, OnInspectEnd);

            // 卡牌事件
            EventCenter.Instance.AddEventListener<CardInstance>(E_EventType.CardReadStart, OnCardReadStart);
            EventCenter.Instance.AddEventListener<CardInstance>(E_EventType.CardReadComplete, OnCardReadComplete);
            EventCenter.Instance.AddEventListener<CardInstance>(E_EventType.CardReadInterrupt, OnCardReadInterrupt);

            // 情绪值事件
            EventCenter.Instance.AddEventListener<EmotionInfo>(E_EventType.EmotionChanged, OnEmotionChanged);
        }

        void OnGameStart()
        {
            Debug.LogError("[EVENT] 游戏开始");
        }

        void OnGamePause()
        {
            Debug.LogError("[EVENT] 游戏暂停");
        }

        void OnGameResume()
        {
            Debug.LogError("[EVENT] 游戏恢复");
        }

        void OnGameWin()
        {
            Debug.LogError("[EVENT] 游戏胜利！");
        }

        void OnGameLose(string reason)
        {
            Debug.LogError($"[EVENT] 游戏失败: {reason}");
        }

        void OnTeacherStateChanged(TeacherState newState)
        {
            Debug.LogError($"[AI] 老师状态: {newState}");
        }

        void OnInspectStart(InspectType inspectType)
        {
            Debug.LogError($"[AI] 开始检查: {inspectType}");
        }

        void OnInspectEnd()
        {
            Debug.LogError("[AI] 检查结束");
        }

        void OnCardReadStart(CardInstance card)
        {
            Debug.LogError($"[CARD] 读条开始: {card.data.cardName}");
        }

        void OnCardReadComplete(CardInstance card)
        {
            Debug.LogError($"[CARD] 读条完成: {card.data.cardName}");
        }

        void OnCardReadInterrupt(CardInstance card)
        {
            Debug.LogError($"[CARD] 读条打断: {card.data.cardName}");
        }

        void OnEmotionChanged(EmotionInfo info)
        {
            // 每5秒输出一次即可
            if (Time.frameCount % 300 == 0)
            {
                Debug.LogError($"[EMOTION] {info}");
            }
        }

        void OnDestroy()
        {
            // 清理事件监听
            EventCenter.Instance.RemoveEventListener(E_EventType.GameStart, OnGameStart);
            EventCenter.Instance.RemoveEventListener(E_EventType.GamePause, OnGamePause);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameResume, OnGameResume);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
        }

        void OnGUI()
        {
            if (!isInitialized) return;

            // 显示游戏状态
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Label($"=== 游戏状态 ===");
            GUILayout.Label($"运行时间: {gameTime:F1}秒");
            GUILayout.Label($"剩余时间: {gameFlow.GetRemainingTime():F1}秒");
            GUILayout.Label($"游戏结束: {gameFlow.IsGameOver()}");
            GUILayout.Label($"暂停: {gameFlow.IsPaused()}");

            GUILayout.Space(10);
            GUILayout.Label($"=== 老师状态 ===");
            GUILayout.Label($"状态: {teacherAI.GetCurrentState()}");
            GUILayout.Label($"检查类型: {teacherAI.GetCurrentInspectType()}");
            GUILayout.Label($"接近进度: {teacherAI.GetApproachProgress():F2}");
            GUILayout.Label($"可见: {teacherAI.IsVisible()}");

            GUILayout.Space(10);
            GUILayout.Label($"=== 读条状态 ===");
            GUILayout.Label($"读条中: {cardReadingSystem.IsReading()}");
            if (cardReadingSystem.IsReading())
            {
                var card = cardReadingSystem.GetCurrentCard();
                GUILayout.Label($"当前卡牌: {card?.data.cardName}");
                GUILayout.Label($"读条进度: {cardReadingSystem.GetProgress():F2}");
                GUILayout.Label($"可打断: {cardReadingSystem.CanInterrupt()}");
            }

            GUILayout.Space(10);
            var emotionInfo = emotionSystem.GetEmotionInfo();
            GUILayout.Label($"=== 情绪值 ===");
            GUILayout.Label($"{emotionInfo}");

            GUILayout.EndArea();
        }
    }
}
