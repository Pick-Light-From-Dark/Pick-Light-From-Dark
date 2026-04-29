using UnityEngine;
using Game.Flow;
using Game.AI;
using Game.Emotion;
using Game.Card;
using Game.Config;
using Game.Data;

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
                Debug.LogError("未配置testLevelConfig");
                return;
            }

            Debug.Log("=== 初始化游戏系统 ===");

            // 初始化各个系统
            gameFlow.Initialize(testLevelConfig);
            teacherAI.Initialize(testLevelConfig);
            cardReadingSystem.Initialize(testLevelConfig);

            isInitialized = true;
            Debug.Log("=== 游戏系统初始化完成，开始测试 ===");
            Debug.Log($"目标: 在{testLevelConfig.timeLimit}秒内存活，避免情绪值超过{testLevelConfig.criticalValue}");

            // 5秒后测试卡牌读条
            Invoke(nameof(TestCardReading), 5f);

            // 10秒后测试打断
            Invoke(nameof(TestCardInterrupt), 10f);
        }

        void TestCardReading()
        {
            if (!isInitialized || gameFlow.IsGameOver()) return;

            Debug.Log("--- 测试: 开始卡牌读条 ---");

            // 创建测试卡牌
            CardData testCard = new CardData
            {
                id = 100,
                cardName = "测试翻身",
                description = "测试用卡牌",
                segments = new System.Collections.Generic.List<Segment>
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

            Debug.Log("--- 测试: 尝试打断读条 ---");
            bool success = cardReadingSystem.InterruptReading();
            Debug.Log($"打断结果: {(success ? "成功" : "失败")}");
        }

        void SetupEventListeners()
        {
            // 游戏流程事件
            EventCenter.Instance.AddEventListener(GameEvents.GameStart, OnGameStart);
            EventCenter.Instance.AddEventListener(GameEvents.GamePause, OnGamePause);
            EventCenter.Instance.AddEventListener(GameEvents.GameResume, OnGameResume);
            EventCenter.Instance.AddEventListener(GameEvents.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener(GameEvents.GameLose, OnGameLose);

            // 老师AI事件
            EventCenter.Instance.AddEventListener(TeacherState, OnTeacherStateChanged);
            EventCenter.Instance.AddEventListener(GameEvents.TeacherInspectStart, OnInspectStart);
            EventCenter.Instance.AddEventListener(GameEvents.TeacherInspectEnd, OnInspectEnd);

            // 卡牌事件
            EventCenter.Instance.AddEventListener(CardInstance, OnCardReadStart);
            EventCenter.Instance.AddEventListener(CardInstance, OnCardReadComplete);
            EventCenter.Instance.AddEventListener(CardInstance, OnCardReadInterrupt);

            // 情绪值事件
            EventCenter.Instance.AddEventListener(GameEvents.EmotionChanged, OnEmotionChanged);
        }

        void OnGameStart()
        {
            Debug.Log("[事件] 游戏开始");
        }

        void OnGamePause()
        {
            Debug.Log("[事件] 游戏暂停");
        }

        void OnGameResume()
        {
            Debug.Log("[事件] 游戏恢复");
        }

        void OnGameWin()
        {
            Debug.Log("[事件] 游戏胜利！");
        }

        void OnGameLose(string reason)
        {
            Debug.Log($"[事件] 游戏失败: {reason}");
        }

        void OnTeacherStateChanged(TeacherState newState)
        {
            Debug.Log($"[事件] 老师状态变化: {newState}");
        }

        void OnInspectStart(InspectType inspectType)
        {
            Debug.Log($"[事件] 老师开始检查: {inspectType}");
        }

        void OnInspectEnd()
        {
            Debug.Log("[事件] 老师检查结束");
        }

        void OnCardReadStart(CardInstance card)
        {
            Debug.Log($"[事件] 卡牌读条开始: {card.data.cardName}");
        }

        void OnCardReadComplete(CardInstance card)
        {
            Debug.Log($"[事件] 卡牌读条完成: {card.data.cardName}");
        }

        void OnCardReadInterrupt(CardInstance card)
        {
            Debug.Log($"[事件] 卡牌读条打断: {card.data.cardName}");
        }

        void OnEmotionChanged(EmotionInfo info)
        {
            // 每秒输出一次即可，避免刷屏
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[情绪值] {info}");
            }
        }

        void OnDestroy()
        {
            // 清理事件监听
            EventCenter.Instance.RemoveEventListener(GameEvents.GameStart, OnGameStart);
            EventCenter.Instance.RemoveEventListener(GameEvents.GamePause, OnGamePause);
            EventCenter.Instance.RemoveEventListener(GameEvents.GameResume, OnGameResume);
            EventCenter.Instance.RemoveEventListener(GameEvents.GameWin, OnGameWin);
            EventCenter.Instance.RemoveEventListener(GameEvents.GameLose, OnGameLose);
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
