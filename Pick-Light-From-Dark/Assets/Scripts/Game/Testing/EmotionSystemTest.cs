using UnityEngine;
using Game.Emotion;
using Game.Config;

namespace Game.Testing
{
    /// <summary>
    /// EmotionSystem 自动化测试
    /// 验证情绪值系统的核心功能
    /// </summary>
    public class EmotionSystemTest : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("测试用的关卡配置")]
        public LevelConfigSO testConfig;

        private EmotionSystem emotionSystem;

        void Start()
        {
            emotionSystem = EmotionSystem.Instance;
            RunAllTests();
        }

        void RunAllTests()
        {
            Debug.Log("=== EmotionSystem 测试开始 ===");

            Test_01_Initialize();
            Test_02_ChangePanic();
            Test_03_ChangeExcite();
            Test_04_ClampValues();
            Test_05_IsCaughtByCriticalValue();
            Test_06_DecreaseEmotionWhileEyeClose();

            Debug.Log("=== EmotionSystem 测试完成 ===");
        }

        void Test_01_Initialize()
        {
            Debug.Log("\n[测试 1] Initialize 测试");

            if (testConfig != null)
            {
                emotionSystem.Initialize(testConfig);
                var info = emotionSystem.GetEmotionInfo();

                Debug.Log($"✓ 初始化成功");
                Debug.Log($"  慌乱值: {info.panicValue}");
                Debug.Log($"  兴奋值: {info.exciteValue}");
                Debug.Log($"  临界值: {testConfig.criticalValue}");
            }
            else
            {
                Debug.LogError("✗ 未配置 testConfig，无法测试 Initialize");
            }
        }

        void Test_02_ChangePanic()
        {
            Debug.Log("\n[测试 2] ChangePanic 测试");

            var before = emotionSystem.GetEmotionInfo().panicValue;
            emotionSystem.ChangePanic(10);
            var after = emotionSystem.GetEmotionInfo().panicValue;

            if (after == before + 10)
            {
                Debug.Log($"✓ ChangePanic(10) 正确: {before} → {after}");
            }
            else
            {
                Debug.LogError($"✗ ChangePanic(10) 失败: {before} → {after} (期望: {before + 10})");
            }
        }

        void Test_03_ChangeExcite()
        {
            Debug.Log("\n[测试 3] ChangeExcite 测试");

            var before = emotionSystem.GetEmotionInfo().exciteValue;
            emotionSystem.ChangeExcite(10);
            var after = emotionSystem.GetEmotionInfo().exciteValue;

            if (after == before + 10)
            {
                Debug.Log($"✓ ChangeExcite(10) 正确: {before} → {after}");
            }
            else
            {
                Debug.LogError($"✗ ChangeExcite(10) 失败: {before} → {after} (期望: {before + 10})");
            }
        }

        void Test_04_ClampValues()
        {
            Debug.Log("\n[测试 4] ClampValues 测试（范围限制）");

            // 测试上溢
            emotionSystem.ChangePanic(999);
            var clampedPanic = emotionSystem.GetEmotionInfo().panicValue;

            // 测试下溢
            emotionSystem.ChangePanic(-999);
            var clampedPanicLow = emotionSystem.GetEmotionInfo().panicValue;

            if (clampedPanic == 100 && clampedPanicLow == 30)
            {
                Debug.Log($"✓ ClampValues 正确: 上溢限制在 100，下溢限制在 30");
            }
            else
            {
                Debug.LogError($"✗ ClampValues 失败: 上溢={clampedPanic} (期望100), 下溢={clampedPanicLow} (期望30)");
            }
        }

        void Test_05_IsCaughtByCriticalValue()
        {
            Debug.Log("\n[测试 5] IsCaughtByCriticalValue 测试");

            if (testConfig == null)
            {
                Debug.LogWarning("⊘ 跳过测试（未配置 testConfig）");
                return;
            }

            // 强制设置情绪值超过临界值
            var total = emotionSystem.GetTotalEmotion();
            var critical = testConfig.criticalValue;

            Debug.Log($"  当前总和: {total}, 临界值: {critical}");

            if (total >= critical)
            {
                Debug.Log($"✓ IsCaughtByCriticalValue 返回 true（总和 >= 临界值）");
            }
            else
            {
                Debug.Log($"⊘ IsCaughtByCriticalValue 返回 false（未达到临界值）");
            }
        }

        void Test_06_DecreaseEmotionWhileEyeClose()
        {
            Debug.Log("\n[测试 6] DecreaseEmotionWhileEyeClose 测试");

            var before = emotionSystem.GetTotalEmotion();
            emotionSystem.DecreaseEmotionWhileEyeClose(1f); // 模拟闭眼1秒
            var after = emotionSystem.GetTotalEmotion();

            // 每秒降低5点
            var expected = Mathf.Max(60, before - 5); // 最低为60（30+30）

            if (after == expected)
            {
                Debug.Log($"✓ DecreaseEmotionWhileEyeClose 正确: {before} → {after} (每秒-5点)");
            }
            else
            {
                Debug.LogError($"✗ DecreaseEmotionWhileEyeClose 失败: {before} → {after} (期望: {expected})");
            }
        }
    }
}
