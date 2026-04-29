using UnityEngine;
using Game.Flow;
using Game.AI;
using Game.Emotion;
using Game.Card;
using Game.Config;
using Game.Data;
using Game.EyeClose;
using System.Collections;
using System.Collections.Generic;

namespace Game.Testing
{
    /// <summary>
    /// 自动化游戏系统测试
    /// 自动测试所有核心功能，无需人工干预
    /// </summary>
    public class AutomatedGameTest : MonoBehaviour
    {
        [Header("测试配置")]
        public LevelConfigSO testLevelConfig;
        public float testDelay = 0.5f; // 测试间隔时间

        private GameFlowController gameFlow;
        private TeacherAI teacherAI;
        private EmotionSystem emotionSystem;
        private CardReadingSystem cardReadingSystem;
        private PlayerState playerState;
        private EyeCloseSystem eyeCloseSystem;

        private bool isTesting = false;
        private int currentTestStep = 0;

        void Start()
        {
            // 在测试开始前，强制清理所有可能存在的旧实例
            Debug.Log("[TEST] === 测试启动：清理所有旧实例 ===");

            int destroyedCount = 0;

            // 使用Resources.FindObjectsOfTypeAll找到所有对象（包括inactive和DontDestroyOnLoad）
            GameFlowController[] oldFlows = Resources.FindObjectsOfTypeAll<GameFlowController>();
            foreach (var flow in oldFlows)
            {
                Debug.Log($"[TEST]   销毁旧GameFlowController InstanceID:{flow.GetInstanceID()} from {flow.gameObject.name}");
                DestroyImmediate(flow.gameObject);
                destroyedCount++;
            }

            EmotionSystem[] oldEmotions = Resources.FindObjectsOfTypeAll<EmotionSystem>();
            foreach (var emotion in oldEmotions)
            {
                Debug.Log($"[TEST]   销毁旧EmotionSystem InstanceID:{emotion.GetInstanceID()} from {emotion.gameObject.name}");
                DestroyImmediate(emotion.gameObject);
                destroyedCount++;
            }

            Debug.Log($"[TEST] 清理完成，共销毁 {destroyedCount} 个旧实例");
            Debug.Log("[TEST] === 开始自动化测试 ===");

            // 等待一帧让销毁生效
            StartCoroutine(DelayedTestStart());
        }

        IEnumerator DelayedTestStart()
        {
            yield return null; // 等待一帧让DestroyImmediate生效
            yield return null; // 再等一帧确保完全清理
            StartCoroutine(RunAutomatedTests());
        }

        IEnumerator RunAutomatedTests()
        {
            Debug.Log("========================================");
            Debug.Log("开始自动化游戏系统测试");
            Debug.Log("========================================");

            TestAssertions.Reset();

            // 步骤1: 初始化系统
            yield return StartCoroutine(Test_Initialize());

            // 步骤2: 测试游戏流程
            yield return StartCoroutine(Test_GameFlow());

            // 步骤3: 测试情绪系统
            yield return StartCoroutine(Test_EmotionSystem());

            // 步骤4: 测试玩家状态
            yield return StartCoroutine(Test_PlayerState());

            // 步骤5: 测试老师AI
            yield return StartCoroutine(Test_TeacherAI());

            // 步骤6: 测试卡牌系统
            yield return StartCoroutine(Test_CardSystem());

            // 步骤7: 测试闭眼系统
            yield return StartCoroutine(Test_EyeCloseSystem());

            // 步骤8: 测试游戏结束
            yield return StartCoroutine(Test_GameOver());

            // 输出测试结果
            PrintTestResults();
        }

        IEnumerator Test_Initialize()
        {
            Debug.Log("\n[测试1] 初始化系统...");

            // 获取系统实例（已经在Start()中清理了旧实例）
            gameFlow = GameFlowController.Instance;
            emotionSystem = EmotionSystem.Instance;
            playerState = PlayerState.Instance;
            eyeCloseSystem = EyeCloseSystem.Instance;

            // 查找AI和读条系统
            teacherAI = FindObjectOfType<TeacherAI>();
            cardReadingSystem = FindObjectOfType<CardReadingSystem>();

            // 验证系统实例
            TestAssertions.AssertNotNull(gameFlow, "GameFlowController实例化");
            TestAssertions.AssertNotNull(emotionSystem, "EmotionSystem实例化");
            TestAssertions.AssertNotNull(playerState, "PlayerState实例化");
            TestAssertions.AssertNotNull(eyeCloseSystem, "EyeCloseSystem实例化");

            yield return new WaitForSeconds(testDelay);
        }

        IEnumerator Test_GameFlow()
        {
            Debug.Log("\n[测试2] 游戏流程...");

            // 初始化游戏
            gameFlow.Initialize(testLevelConfig);

            // 等待一帧让Initialize完成
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame(); // 确保第一帧已经跳过

            // 验证游戏状态（第一帧后立即检查，避免时间流逝）
            TestAssertions.AssertFalse(gameFlow.IsGameOver(), "游戏初始化后未结束");
            TestAssertions.AssertFalse(gameFlow.IsPaused(), "游戏初始化后未暂停");
            TestAssertions.AssertEqual(testLevelConfig.timeLimit, gameFlow.GetRemainingTime(), "倒计时初始化正确", 1f);

            // 测试暂停和恢复
            gameFlow.PauseGame();
            yield return new WaitForSeconds(testDelay);
            TestAssertions.AssertTrue(gameFlow.IsPaused(), "游戏暂停成功");

            gameFlow.ResumeGame();
            yield return new WaitForSeconds(testDelay);
            TestAssertions.AssertFalse(gameFlow.IsPaused(), "游戏恢复成功");
        }

        IEnumerator Test_EmotionSystem()
        {
            Debug.Log("\n[测试3] 情绪系统...");

            var emotionInfo = emotionSystem.GetEmotionInfo();

            // 验证初始情绪值
            TestAssertions.AssertEqual(testLevelConfig.initialPanic, emotionInfo.panicValue, "初始慌乱值");
            TestAssertions.AssertEqual(testLevelConfig.initialExcite, emotionInfo.exciteValue, "初始兴奋值");

            // 测试情绪变化
            emotionSystem.ChangePanic(10);
            yield return new WaitForSeconds(testDelay);
            emotionInfo = emotionSystem.GetEmotionInfo();
            TestAssertions.AssertEqual(testLevelConfig.initialPanic + 10, emotionInfo.panicValue, "慌乱值增加");

            emotionSystem.ChangeExcite(-5);
            yield return new WaitForSeconds(testDelay);
            emotionInfo = emotionSystem.GetEmotionInfo();
            TestAssertions.AssertEqual(testLevelConfig.initialExcite - 5, emotionInfo.exciteValue, "兴奋值减少");
        }

        IEnumerator Test_PlayerState()
        {
            Debug.Log("\n[测试4] 玩家状态...");

            // 测试卧床状态
            playerState.SetInBed(true);
            yield return new WaitForSeconds(testDelay);
            TestAssertions.AssertTrue(playerState.IsInBed(), "玩家上床成功");

            playerState.SetInBed(false);
            yield return new WaitForSeconds(testDelay);
            TestAssertions.AssertFalse(playerState.IsInBed(), "玩家下床成功");

            // 恢复卧床状态
            playerState.SetInBed(true);

            // 测试闭眼状态
            playerState.SetEyesClosed(true);
            yield return new WaitForSeconds(testDelay);
            TestAssertions.AssertTrue(playerState.IsEyesClosed(), "玩家闭眼成功");

            playerState.SetEyesClosed(false);
            yield return new WaitForSeconds(testDelay);
            TestAssertions.AssertFalse(playerState.IsEyesClosed(), "玩家睁眼成功");
        }

        IEnumerator Test_TeacherAI()
        {
            Debug.Log("\n[测试5] 老师AI...");

            // 初始化AI
            if (teacherAI == null)
            {
                GameObject teacherObj = new GameObject("TeacherAI");
                teacherAI = teacherObj.AddComponent<TeacherAI>();
            }

            teacherAI.Initialize(testLevelConfig);
            yield return new WaitForSeconds(testDelay);

            // 验证AI状态
            TestAssertions.AssertNotNull(teacherAI, "TeacherAI实例化");
            TestAssertions.AssertTrue(teacherAI.GetCurrentState() == TeacherState.Idle, "AI初始状态为Idle");

            // 等待AI状态转换
            float initialTime = Time.time;
            while (teacherAI.GetCurrentState() == TeacherState.Idle && Time.time - initialTime < 5f)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // 验证AI已经离开空闲状态
            TestAssertions.AssertTrue(teacherAI.GetCurrentState() != TeacherState.Idle, "AI状态发生变化");
        }

        IEnumerator Test_CardSystem()
        {
            Debug.Log("\n[测试6] 卡牌系统...");

            // 初始化读条系统
            if (cardReadingSystem == null)
            {
                GameObject cardObj = new GameObject("CardReadingSystem");
                cardReadingSystem = cardObj.AddComponent<CardReadingSystem>();
            }

            cardReadingSystem.Initialize(testLevelConfig);
            yield return new WaitForSeconds(testDelay);

            // 创建测试卡牌
            CardData testCard = new CardData
            {
                id = 999,
                cardName = "测试卡牌",
                description = "自动化测试用",
                segments = new List<Segment>
                {
                    new Segment(1f, true),
                    new Segment(1f, false)
                },
                panicDelta = 5,
                exciteDelta = -3,
                interruptPanicAdd = 10
            };

            CardInstance cardInstance = new CardInstance(testCard, 999);

            // 测试开始读条
            cardReadingSystem.StartReading(cardInstance);
            yield return new WaitForSeconds(testDelay);

            TestAssertions.AssertTrue(cardReadingSystem.IsReading(), "卡牌开始读条");

            // 测试打断
            bool interruptSuccess = cardReadingSystem.InterruptReading();
            yield return new WaitForSeconds(testDelay);

            TestAssertions.AssertTrue(interruptSuccess, "卡牌打断成功");
        }

        IEnumerator Test_EyeCloseSystem()
        {
            Debug.Log("\n[测试7] 闭眼系统...");

            // 测试闭眼时长
            playerState.SetEyesClosed(true);
            yield return new WaitForSeconds(1f);
            playerState.SetEyesClosed(false);

            float duration = eyeCloseSystem.GetEyeCloseDuration();
            TestAssertions.AssertTrue(duration >= 0.9f, "闭眼时长记录正确");

            // 测试是否触发时间加速
            // 模拟闭眼10秒
            playerState.SetEyesClosed(true);
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(1f);
            }
            playerState.SetEyesClosed(false);

            bool isAccelerated = eyeCloseSystem.IsTimeAccelerated();
            TestAssertions.AssertTrue(isAccelerated, "闭眼10秒后触发时间加速");
        }

        IEnumerator Test_GameOver()
        {
            Debug.Log("\n[测试8] 游戏结束...");

            // 测试游戏失败
            gameFlow.GameLose("测试结束");
            yield return new WaitForSeconds(testDelay);

            TestAssertions.AssertTrue(gameFlow.IsGameOver(), "游戏结束状态正确");

            // 重新初始化用于后续测试
            yield return new WaitForSeconds(testDelay);
            TestAssertions.Reset();
            gameFlow.Initialize(testLevelConfig);
            yield return new WaitForSeconds(testDelay);

            // 测试游戏胜利
            gameFlow.GameWin();
            yield return new WaitForSeconds(testDelay);

            TestAssertions.AssertTrue(gameFlow.IsGameOver(), "游戏胜利状态正确");
        }

        void PrintTestResults()
        {
            Debug.Log(TestAssertions.GetSummary());

            if (TestAssertions.AllTestsPassed())
            {
                Debug.Log("<color=green>✓ 所有测试通过！游戏系统运行正常。</color>");
            }
            else
            {
                Debug.LogError("<color=red>✗ 部分测试失败，请检查上述错误信息。</color>");
            }

            isTesting = false;
        }

        void OnGUI()
        {
            if (!isTesting && TestAssertions.GetTotalCount() > 0)
            {
                GUI.skin.label.fontSize = 24;
                GUILayout.BeginArea(new Rect(10, 10, 600, 400));

                GUILayout.Label("=== 自动化测试结果 ===");
                GUILayout.Label($"总测试数: {TestAssertions.GetTotalCount()}");
                GUILayout.Label($"通过: {TestAssertions.GetPassedCount()}");
                GUILayout.Label($"失败: {TestAssertions.GetFailedCount()}");

                if (TestAssertions.AllTestsPassed())
                {
                    GUI.color = Color.green;
                    GUILayout.Label("✓ 所有测试通过！");
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.red;
                    GUILayout.Label("✗ 部分测试失败");
                    GUI.color = Color.white;
                }

                GUILayout.EndArea();
            }
        }
    }
}
