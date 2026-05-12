using UnityEngine;
using Game.Config;
using Game.Flow;

namespace Game.Test
{
    /// <summary>
    /// 结局系统测试脚本 — 用于快速验证结局显示流程
    /// 挂载到场景中任意 GameObject 即可运行测试
    /// </summary>
    public class EndingTestRunner : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("要测试的结局ID（6001~6005）")]
        public int testEndingId = 6001;

        [Tooltip("自动在Start时触发测试")]
        public bool autoTriggerOnStart = true;

        [Header("按键测试（运行时）")]
        [Tooltip("按数字键1~5触发对应结局")]
        public bool enableKeyInput = true;

        void Start()
        {
            // 确保 EndingManager 已初始化
            if (EndingManager.Instance == null)
            {
                var go = new GameObject("EndingManager");
                go.AddComponent<EndingManager>();
            }

            // 创建或加载结局数据
            var endingData = CreateTestEndingData();
            EndingManager.Instance.Initialize(endingData);

            if (autoTriggerOnStart)
            {
                Invoke(nameof(TriggerTestEnding), 1f);
            }
        }

        void Update()
        {
            if (!enableKeyInput) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                EndingManager.Instance.TriggerEnding(6001);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                EndingManager.Instance.TriggerEnding(6002);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                EndingManager.Instance.TriggerEnding(6003);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                EndingManager.Instance.TriggerEnding(6004);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                EndingManager.Instance.TriggerEnding(6005);
        }

        void TriggerTestEnding()
        {
            Debug.Log($"[EndingTestRunner] 触发测试结局 ID={testEndingId}");
            EndingManager.Instance.TriggerEnding(testEndingId);
        }

        /// <summary>
        /// 创建测试用结局数据（内置5个结局）
        /// </summary>
        EndingDataSO CreateTestEndingData()
        {
            var so = ScriptableObject.CreateInstance<EndingDataSO>();
            so.endings = new System.Collections.Generic.List<EndingEntry>
            {
                new EndingEntry
                {
                    id = 6001,
                    endingName = "【结局一：太阳照常升起】",
                    description = "薯片改变不了任何事，你也是。"
                },
                new EndingEntry
                {
                    id = 6002,
                    endingName = "【结局二：莫比乌斯环】",
                    description = "一条走廊，离开起点之时，你就明白你终会回来。"
                },
                new EndingEntry
                {
                    id = 6003,
                    endingName = "【结局三：人心不足蛇吞象】",
                    description = "得失荣枯总在天，机关用尽也徒然。"
                },
                new EndingEntry
                {
                    id = 6004,
                    endingName = "【结局四：星垂之夜】",
                    description = "俯仰天地之间——无愧于人，无愧于心，无愧于己。"
                },
                new EndingEntry
                {
                    id = 6005,
                    endingName = "【结局五：北极星】",
                    description = "我对你透露一个大秘密，这是人类最古老的玩笑——无论往哪走，都是向前走。"
                }
            };
            return so;
        }

        [ContextMenu("测试结局1")]
        void TestEnding1() { EndingManager.Instance?.TriggerEnding(6001); }

        [ContextMenu("测试结局2")]
        void TestEnding2() { EndingManager.Instance?.TriggerEnding(6002); }

        [ContextMenu("测试结局3")]
        void TestEnding3() { EndingManager.Instance?.TriggerEnding(6003); }

        [ContextMenu("测试结局4")]
        void TestEnding4() { EndingManager.Instance?.TriggerEnding(6004); }

        [ContextMenu("测试结局5")]
        void TestEnding5() { EndingManager.Instance?.TriggerEnding(6005); }
    }
}
