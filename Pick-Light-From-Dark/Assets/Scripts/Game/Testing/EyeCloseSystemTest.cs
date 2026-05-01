using UnityEngine;
using Game.EyeClose;

namespace Game.Testing
{
    /// <summary>
    /// EyeCloseSystem 自动化测试
    /// 验证闭眼系统的核心功能
    /// </summary>
    public class EyeCloseSystemTest : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("是否自动运行测试")]
        public bool autoRunOnStart = true;

        private EyeCloseSystem eyeCloseSystem;

        void Start()
        {
            eyeCloseSystem = EyeCloseSystem.Instance;

            if (autoRunOnStart)
            {
                RunAllTests();
            }
        }

        void Update()
        {
            // 按 T 键手动运行测试
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunAllTests();
            }
        }

        void RunAllTests()
        {
            Debug.Log("=== EyeCloseSystem 测试开始 ===");

            Test_01_GetEyeCloseDuration();
            Test_02_IsTimeAccelerated();
            Test_03_Reset();

            Debug.Log("=== EyeCloseSystem 测试完成 ===");
            Debug.Log("提示：按 C 键切换闭眼状态，观察时间加速触发（闭眼10秒后）");
        }

        void Test_01_GetEyeCloseDuration()
        {
            Debug.Log("\n[测试 1] GetEyeCloseDuration 测试");

            var duration = eyeCloseSystem.GetEyeCloseDuration();
            Debug.Log($"✓ GetEyeCloseDuration 返回: {duration:F2}秒");

            if (duration >= 0)
            {
                Debug.Log($"  闭眼持续时间为非负数，功能正常");
            }
            else
            {
                Debug.LogError($"✗ GetEyeCloseDuration 返回负数: {duration}");
            }
        }

        void Test_02_IsTimeAccelerated()
        {
            Debug.Log("\n[测试 2] IsTimeAccelerated 测试");

            var isAccelerated = eyeCloseSystem.IsTimeAccelerated();
            Debug.Log($"✓ IsTimeAccelerated 返回: {isAccelerated}");

            if (isAccelerated)
            {
                Debug.Log($"  当前时间流速: {Time.timeScale}x (已加速)");
            }
            else
            {
                Debug.Log($"  当前时间流速: {Time.timeScale}x (正常)");
            }
        }

        void Test_03_Reset()
        {
            Debug.Log("\n[测试 3] Reset 测试");

            eyeCloseSystem.Reset();
            var duration = eyeCloseSystem.GetEyeCloseDuration();
            var isAccelerated = eyeCloseSystem.IsTimeAccelerated();

            if (duration == 0 && !isAccelerated)
            {
                Debug.Log($"✓ Reset 正确: 闭眼时长=0, 时间加速=false");
            }
            else
            {
                Debug.LogError($"✗ Reset 失败: 闭眼时长={duration}, 时间加速={isAccelerated}");
            }
        }
    }
}
