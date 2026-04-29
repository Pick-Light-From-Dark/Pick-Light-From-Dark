using UnityEngine;
using Game.Emotion;
using Game.Config;
using Game.Data;

namespace Game.System
{
    /// <summary>
    /// 情绪值系统测试脚本
    /// </summary>
    public class EmotionTest : MonoBehaviour
    {
        [Header("测试配置")]
        public LevelConfigSO testLevelConfig;

        [Header("测试参数")]
        public bool runOnStart = true;
        public bool autoTest = true;

        private EmotionSystem emotionSystem;

        void Start()
        {
            if (!runOnStart) return;

            RunTest();
        }

        void RunTest()
        {
            Debug.LogError("=== 情绪值系统测试 ===");

            // 获取或创建情绪值系统
            emotionSystem = EmotionSystem.Instance;

            // 初始化
            if (testLevelConfig != null)
            {
                emotionSystem.Initialize(testLevelConfig);
                Debug.LogError($"[INIT] {emotionSystem.GetEmotionInfo()}");
            }
            else
            {
                Debug.LogError("[FAIL] 未配置testLevelConfig");
                return;
            }

            // 监听事件
            EventCenter.Instance.AddEventListener<EmotionInfo>(E_EventType.EmotionChanged, OnEmotionChanged);
            EventCenter.Instance.AddEventListener<int>(E_EventType.PanicChanged, OnPanicChanged);
            EventCenter.Instance.AddEventListener<int>(E_EventType.ExciteChanged, OnExciteChanged);

            // 自动测试
            if (autoTest)
            {
                Invoke(nameof(TestIncreasePanic), 1f);
                Invoke(nameof(TestDecreaseExcite), 2f);
                Invoke(nameof(TestCriticalCheck), 3f);
                Invoke(nameof(TestEyeClose), 4f);
                Invoke(nameof(EndTest), 5f);
            }
        }

        void TestIncreasePanic()
        {
            Debug.LogError("[TEST] 增加慌乱值 +10");
            emotionSystem.ChangePanic(10);
        }

        void TestDecreaseExcite()
        {
            Debug.LogError("[TEST] 降低兴奋值 -5");
            emotionSystem.ChangeExcite(-5);
        }

        void TestCriticalCheck()
        {
            Debug.LogError("[TEST] 临界值检查");
            bool isCritical = emotionSystem.IsCaughtByCriticalValue();
            Debug.LogError($"[RESULT] 是否超标: {isCritical}");
        }

        void TestEyeClose()
        {
            Debug.LogError("[TEST] 闭眼降低情绪值");
            emotionSystem.DecreaseEmotionWhileEyeClose(1f);
        }

        void EndTest()
        {
            Debug.LogError("[DONE] 情绪值系统测试完成");
            Debug.LogError($"[FINAL] {emotionSystem.GetEmotionInfo()}");
        }

        void OnEmotionChanged(EmotionInfo info)
        {
            Debug.LogError($"[EVENT] {info}");
        }

        void OnPanicChanged(int value)
        {
            Debug.LogError($"[EVENT] 慌乱值: {value}");
        }

        void OnExciteChanged(int value)
        {
            Debug.LogError($"[EVENT] 兴奋值: {value}");
        }

        void OnDestroy()
        {
            // 移除事件监听
            EventCenter.Instance.RemoveEventListener<EmotionInfo>(E_EventType.EmotionChanged, OnEmotionChanged);
            EventCenter.Instance.RemoveEventListener<int>(E_EventType.PanicChanged, OnPanicChanged);
            EventCenter.Instance.RemoveEventListener<int>(E_EventType.ExciteChanged, OnExciteChanged);
        }
    }
}
