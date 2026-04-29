using UnityEngine;
using Game.Flow;
using Game.AI;
using Game.Emotion;
using Game.Card;
using Game.Config;
using Game.Data;
using Game.EyeClose;
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
        public PlayerState playerState;
        public EyeCloseSystem eyeCloseSystem;

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
            playerState = PlayerState.Instance;
            eyeCloseSystem = EyeCloseSystem.Instance;

            // 查找或创建AI实例
            teacherAI = FindObjectOfType<TeacherAI>();
            if (teacherAI == null)
            {
                GameObject teacherObj = new GameObject("TeacherAI");
                teacherAI = teacherObj.AddComponent<TeacherAI>();
            }

            // 查找或创建读条系统实例
            cardReadingSystem = FindObjectOfType<CardReadingSystem>();
            if (cardReadingSystem == null)
            {
                GameObject cardObj = new GameObject("CardReadingSystem");
                cardReadingSystem = cardObj.AddComponent<CardReadingSystem>();
            }

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
                Debug.LogError("[TEST FAIL] 未配置testLevelConfig");
                return;
            }

            Debug.Log("[TEST] 游戏系统初始化开始");

            // 初始化各个系统
            gameFlow.Initialize(testLevelConfig);
            teacherAI.Initialize(testLevelConfig);
            cardReadingSystem.Initialize(testLevelConfig);

            isInitialized = true;
            Debug.Log($"[TEST] 目标: 在{testLevelConfig.timeLimit}秒内存活");
            Debug.Log("[TEST] 5秒后自动测试卡牌读条");
            Debug.Log("[TEST] 10秒后自动测试打断功能");
            Debug.Log("[TEST] 按C键测试闭眼功能");
            Debug.Log("[TEST] 闭眼10秒后会触发时间加速");
            Debug.Log("[TEST] 默认玩家状态：已卧床");

            // 设置默认玩家状态：在床上
            playerState.SetInBed(true);

            // 5秒后测试卡牌读条
            Invoke(nameof(TestCardReading), 5f);

            // 10秒后测试打断
            Invoke(nameof(TestCardInterrupt), 10f);
        }

        void TestCardReading()
        {
            if (!isInitialized || gameFlow.IsGameOver())
            {
                Debug.LogError("[TEST FAIL] 测试卡牌时游戏未初始化或已结束");
                return;
            }

            Debug.Log("[TEST] 开始卡牌读条测试");

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
            if (!isInitialized || gameFlow.IsGameOver())
            {
                Debug.LogError("[TEST FAIL] 测试打断时游戏未初始化或已结束");
                return;
            }

            Debug.Log("[TEST] 测试打断功能");
            bool success = cardReadingSystem.InterruptReading();
            Debug.Log($"[TEST] 打断测试: {(success ? "成功" : "失败（正常，因为卡牌可能已完成）")}");
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
            EventCenter.Instance.AddEventListener(E_EventType.PlayerCaught, OnPlayerCaught);
        }

        void OnGameStart()
        {
            Debug.Log("[EVENT] 游戏开始");
        }

        void OnGamePause()
        {
            Debug.Log("[EVENT] 游戏暂停");
        }

        void OnGameResume()
        {
            Debug.Log("[EVENT] 游戏恢复");
        }

        void OnGameWin()
        {
            Debug.Log("[EVENT] 游戏胜利！");
            if (this != null) CancelInvoke();
        }

        void OnGameLose(string reason)
        {
            Debug.Log($"[EVENT] 游戏失败: {reason}");
            if (this != null) CancelInvoke();
        }

        void OnTeacherStateChanged(TeacherState newState)
        {
            Debug.Log($"[AI] 老师状态: {newState}");
        }

        void OnInspectStart(InspectType inspectType)
        {
            Debug.Log($"[AI] 开始检查: {inspectType}");
        }

        void OnInspectEnd()
        {
            Debug.Log("[AI] 检查结束");
        }

        void OnCardReadStart(CardInstance card)
        {
            Debug.Log($"[CARD] 读条开始: {card.data.cardName}");
        }

        void OnCardReadComplete(CardInstance card)
        {
            Debug.Log($"[CARD] 读条完成: {card.data.cardName}");
        }

        void OnCardReadInterrupt(CardInstance card)
        {
            Debug.Log($"[CARD] 读条打断: {card.data.cardName}");
        }

        void OnEmotionChanged(EmotionInfo info)
        {
            // 每5秒输出一次即可
            if (Time.frameCount % 300 == 0)
            {
                Debug.Log($"[EMOTION] {info}");
            }
        }

        void OnPlayerCaught()
        {
            Debug.Log("[TEST] 玩家被抓！测试查寝判定");
        }

        void OnDestroy()
        {
            // 清理事件监听
            EventCenter.Instance.RemoveEventListener(E_EventType.GameStart, OnGameStart);
            EventCenter.Instance.RemoveEventListener(E_EventType.GamePause, OnGamePause);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameResume, OnGameResume);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.RemoveEventListener(E_EventType.PlayerCaught, OnPlayerCaught);
        }

        void OnGUI()
        {
            if (!isInitialized) return;

            // 设置字体大小
            GUI.skin.label.fontSize = 28;

            // 显示游戏状态
            GUILayout.BeginArea(new Rect(10, 10, 600, 500));
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

            GUILayout.Space(10);
            GUILayout.Label($"=== 玩家状态 ===");
            GUILayout.Label($"卧床: {playerState.IsInBed()}");
            GUILayout.Label($"闭眼: {playerState.IsEyesClosed()} (按C键切换)");
            string timeStatus = eyeCloseSystem.IsTimeAccelerated() ? "时间加速中" : "正常";
            GUILayout.Label($"闭眼时长: {eyeCloseSystem.GetEyeCloseDuration():F1}秒 ({timeStatus})");

            GUILayout.EndArea();
        }
    }
}
