using UnityEngine;
using System.Collections.Generic;

namespace Game.Testing
{
    /// <summary>
    /// 测试断言系统
    /// 用于自动化测试中的结果验证
    /// </summary>
    public static class TestAssertions
    {
        private static List<string> passedTests = new List<string>();
        private static List<string> failedTests = new List<string>();
        private static int totalTests = 0;
        private static int passedCount = 0;
        private static int failedCount = 0;

        /// <summary>
        /// 重置测试状态
        /// </summary>
        public static void Reset()
        {
            passedTests.Clear();
            failedTests.Clear();
            totalTests = 0;
            passedCount = 0;
            failedCount = 0;
        }

        /// <summary>
        /// 断言条件为真
        /// </summary>
        public static void AssertTrue(bool condition, string testName, string details = "")
        {
            totalTests++;
            if (condition)
            {
                passedCount++;
                passedTests.Add($"✓ {testName}");
                Debug.Log($"[TEST PASS] {testName} {details}");
            }
            else
            {
                failedCount++;
                failedTests.Add($"✗ {testName}: {details}");
                Debug.LogError($"[TEST FAIL] {testName} - {details}");
            }
        }

        /// <summary>
        /// 断言两个值相等
        /// </summary>
        public static void AssertEqual(float expected, float actual, string testName, float tolerance = 0.01f)
        {
            bool isEqual = Mathf.Abs(expected - actual) < tolerance;
            string details = $"Expected: {expected}, Actual: {actual}";
            AssertTrue(isEqual, testName, details);
        }

        /// <summary>
        /// 断言不为空
        /// </summary>
        public static void AssertNotNull(object obj, string testName)
        {
            AssertTrue(obj != null, testName, obj == null ? "Object is null" : "Object is not null");
        }

        /// <summary>
        /// 断言为假
        /// </summary>
        public static void AssertFalse(bool condition, string testName, string details = "")
        {
            AssertTrue(!condition, testName, details);
        }

        /// <summary>
        /// 获取测试结果摘要
        /// </summary>
        public static string GetSummary()
        {
            string summary = $"\n========== 测试结果 ==========\n";
            summary += $"总测试数: {totalTests}\n";
            summary += $"通过: {passedCount}\n";
            summary += $"失败: {failedCount}\n";
            summary += $"成功率: {(totalTests > 0 ? (passedCount * 100f / totalTests) : 0):F1}%\n";

            if (failedCount > 0)
            {
                summary += $"\n失败的测试:\n";
                foreach (string failed in failedTests)
                {
                    summary += $"  {failed}\n";
                }
            }

            summary += $"==============================\n";
            return summary;
        }

        /// <summary>
        /// 是否所有测试都通过
        /// </summary>
        public static bool AllTestsPassed()
        {
            return failedCount == 0 && totalTests > 0;
        }

        /// <summary>
        /// 获取通过数量
        /// </summary>
        public static int GetPassedCount()
        {
            return passedCount;
        }

        /// <summary>
        /// 获取失败数量
        /// </summary>
        public static int GetFailedCount()
        {
            return failedCount;
        }

        /// <summary>
        /// 获取总测试数量
        /// </summary>
        public static int GetTotalCount()
        {
            return totalTests;
        }
    }
}
