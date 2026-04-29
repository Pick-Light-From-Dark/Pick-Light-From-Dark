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
    /// 完整游戏系统集成测试
    /// 测试查寝判定、被抓、闭眼等完整流程
    /// </summary>
    public class FullGameTest : MonoBehaviour
    {
        [Header("系统引用")]
        public GameFlowController gameFlow;
        public TeacherAI teacherAI;
        public EmotionSystem emotionSystem;
        public CardReadingSystem cardReadingSystem;
        public PlayerState playerState;
        public EyeCloseSystem eyeCloseSystem;
        public CardManager cardManager;

        [Header("测试配置")]
        public LevelConfigSO testLevelConfig;
        public bool autoStart = true;

        [Header("测试信息")]
        [SerializeField] private bool isInitialized;
        [SerializeField] private float gameTime;

        void Start()
        {
            // 获取所有系统实例
            gameFlow = GameFlowController.Instance;
            emotionSystem = EmotionSystem.Instance;
            playerState = PlayerState.Instance;
            eyeCloseSystem = EyeCloseSystem.Instance;
            cardManager = CardManager.Instance;

            // 创建AI和读条系统
            GameObject teacherObj = new GameObject("TeacherAI");
            teacherAI = teacherObj.AddComponent<TeacherAI>();

            GameObject cardObj = new GameObject("CardReadingSystem");
            cardReadingSystem = cardObj.AddComponent<CardReadingSystem>();

            // 监听事件
            SetupEventListeners();

            if (autoStart)
            {
                InitializeGame();
            }
        }

        void InitializeGame()
        {
            if (testLevelConfig == null)
            {
                Debug.LogError("[TEST FAIL] 未配置testLevelConfig");
                return;
            }

            Debug.Log("[TEST] === 完整游戏系统测试 ===");

            // 初始化所有系统
            gameFlow.Initialize(testLevelConfig);
            teacherAI.Initialize(testLevelConfig);
            cardReadingSystem.Initialize(testLevelConfig);
            cardManager.Initialize(testLevelConfig);

            // 设置初始玩家状态
            playerState.SetInBed(true);

            isInitialized = true;
            Debug.Log("[TEST] === 测试场景说明 ===");
            Debug.Log("[TEST] 1. 老师会自动巡逻查寝");
            Debug.Log("[TEST] 2. 按C键切换闭眼（可规避手电筒）");
            Debug.Log("[TEST] 3. 闭眼10秒后时间会加速");
            Debug.Log("[TEST] 4. 如果条件不符会被抓");
            Debug.Log("[TEST] 5. 按1键手动使用测试卡牌");

            // 5秒后提示测试
            Invoke(nameof(ShowTestInstructions), 5f);
        }

        void ShowTestInstructions()
        {
            Debug.Log("[TEST] === 可以开始测试了 ===");
            Debug.Log("[TEST] 尝试以下操作:");
            Debug.Log("[TEST] - 按C键闭眼，观察老师检查时的判定");
            Debug.Log("[TEST] - 按C键睁眼，观察被抓情况");
            Debug.Log("[TEST] - 按1键使用测试卡牌");
            Debug.Log("[TEST] - 观察闭眼10秒后的时间加速");
        }

        void Update()
        {
            if (!isInitialized) return;

            gameTime += Time.deltaTime;

            // 测试按键
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseTestCard();
            }
        }

        void UseTestCard()
        {
            if (cardReadingSystem.IsReading())
            {
                Debug.Log("[TEST] 已有卡牌在读条中");
                return;
            }

            Debug.Log("[TEST] 手动使用测试卡牌");

            // 创建测试卡牌
            CardData testCard = new CardData
            {
                id = 999,
                cardName = "测试卡",
                description = "测试用卡牌",
                segments = new List<Segment>
                {
                    new Segment(3f, true),
                    new Segment(3f, false)
                },
                panicDelta = 0,
                exciteDelta = -5,
                interruptPanicAdd = 10
            };

            CardInstance instance = new CardInstance(testCard, 999);
            cardReadingSystem.StartReading(instance);
        }

        void SetupEventListeners()
        {
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);
            EventCenter.Instance.AddEventListener(E_EventType.PlayerCaught, OnPlayerCaught);
            EventCenter.Instance.AddEventListener(E_EventType.EyeCloseStart, OnEyeCloseStart);
        }

        void OnGameLose(string reason)
        {
            Debug.Log($"[EVENT] 游戏结束: {reason}");
        }

        void OnPlayerCaught()
        {
            Debug.LogError("[TEST] ⚠️ 玩家被抓！观察老师判定逻辑");
        }

        void OnEyeCloseStart()
        {
            Debug.LogWarning("[TEST] ⚠️ 闭眼过久，时间开始加速！");
        }

        void OnGUI()
        {
            if (!isInitialized) return;

            GUILayout.BeginArea(new Rect(10, 10, 450, 450));
            GUILayout.Label("=== 游戏状态 ===");
            GUILayout.Label($"运行时间: {gameTime:F1}秒");
            GUILayout.Label($"剩余时间: {gameFlow.GetRemainingTime():F1}秒");
            GUILayout.Label($"时间流速: {Time.timeScale}x");

            GUILayout.Space(10);
            GUILayout.Label("=== 老师状态 ===");
            GUILayout.Label($"状态: {teacherAI.GetCurrentState()}");
            GUILayout.Label($"检查类型: {teacherAI.GetCurrentInspectType()}");
            GUILayout.Label($"接近进度: {teacherAI.GetApproachProgress():F2}");

            GUILayout.Space(10);
            GUILayout.Label("=== 玩家状态 ===");
            GUILayout.Label($"卧床: {playerState.IsInBed()}");
            GUILayout.Label($"闭眼: {playerState.IsEyesClosed()} (按C切换)");
            GUILayout.Label($"闭眼时长: {eyeCloseSystem.GetEyeCloseDuration():F1}秒");

            GUILayout.Space(10);
            GUILayout.Label("=== 读条状态 ===");
            GUILayout.Label($"读条中: {cardReadingSystem.IsReading()}");
            if (cardReadingSystem.IsReading())
            {
                var card = cardReadingSystem.GetCurrentCard();
                GUILayout.Label($"当前卡牌: {card?.data.cardName}");
                GUILayout.Label($"读条进度: {cardReadingSystem.GetProgress():F2}");
                GUILayout.Label($"可打断: {cardReadingSystem.CanInterrupt()}");
            }

            GUILayout.Space(10);
            GUILayout.Label("=== 情绪值 ===");
            var emotionInfo = emotionSystem.GetEmotionInfo();
            GUILayout.Label($"{emotionInfo}");

            GUILayout.Space(10);
            GUILayout.Label("=== 操作提示 ===");
            GUILayout.Label("C键: 切换闭眼");
            GUILayout.Label("1键: 使用测试卡牌");

            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameLose);
            EventCenter.Instance.RemoveEventListener(E_EventType.PlayerCaught, OnPlayerCaught);
            EventCenter.Instance.RemoveEventListener(E_EventType.EyeCloseStart, OnEyeCloseStart);
        }
    }
}
