using UnityEngine;
using Game.Emotion;
using Game.Config;
using Framework.EventCenter;

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
            Debug.Log("=== 情绪值系统测试开始 ===");

            // 获取或创建情绪值系统
            emotionSystem = EmotionSystem.Instance;

            // 初始化
            if (testLevelConfig != null)
            {
                emotionSystem.Initialize(testLevelConfig);
                Debug.Log($"✓ 初始化成功: {emotionSystem.GetEmotionInfo()}");
            }
            else
            {
                Debug.LogError("✗ 未配置testLevelConfig");
                return;
            }

            // 监听事件
            EventCenter.Instance.AddEventListener(GameEvents.EmotionChanged, OnEmotionChanged);
            EventCenter.Instance.AddEventListener(GameEvents.PanicChanged, OnPanicChanged);
            EventCenter.Instance.AddEventListener(GameEvents.ExciteChanged, OnExciteChanged);

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
            Debug.Log("\n--- 测试: 增加慌乱值 ---");
            emotionSystem.ChangePanic(10);
        }

        void TestDecreaseExcite()
        {
            Debug.Log("\n--- 测试: 降低兴奋值 ---");
            emotionSystem.ChangeExcite(-5);
        }

        void TestCriticalCheck()
        {
            Debug.Log("\n--- 测试: 临界值检查 ---");
            bool isCritical = emotionSystem.IsCaughtByCriticalValue();
            Debug.Log($"是否超标: {isCritical}");
        }

        void TestEyeClose()
        {
            Debug.Log("\n--- 测试: 闭眼降低情绪值 ---");
            emotionSystem.DecreaseEmotionWhileEyeClose(1f);
        }

        void EndTest()
        {
            Debug.Log("\n=== 情绪值系统测试完成 ===");
            Debug.Log($"最终状态: {emotionSystem.GetEmotionInfo()}");
        }

        void OnEmotionChanged(EmotionInfo info)
        {
            Debug.Log($"[事件] 情绪值变化 - {info}");
        }

        void OnPanicChanged(int value)
        {
            Debug.Log($"[事件] 慌乱值: {value}");
        }

        void OnExciteChanged(int value)
        {
            Debug.Log($"[事件] 兴奋值: {value}");
        }

        void OnDestroy()
        {
            // 移除事件监听
            EventCenter.Instance.RemoveEventListener(GameEvents.EmotionChanged, OnEmotionChanged);
            EventCenter.Instance.RemoveEventListener(GameEvents.PanicChanged, OnPanicChanged);
            EventCenter.Instance.RemoveEventListener(GameEvents.ExciteChanged, OnExciteChanged);
        }
    }
}
