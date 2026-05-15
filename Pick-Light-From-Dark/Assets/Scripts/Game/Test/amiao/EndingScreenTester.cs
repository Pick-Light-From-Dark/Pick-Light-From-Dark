using UnityEngine;
using Game.Config;

namespace Game.Test
{
    /// <summary>
    /// 结局画面测试器 — 快速验证 EndingScreen Prefab
    /// 挂载到场景后，运行时按 F3 显示结局画面，支持切换死亡/普通结局
    /// </summary>
    public class EndingScreenTester : MonoBehaviour
    {
        [Header("结局画面 Prefab")]
        public GameObject endingScreenPrefab;

        [Header("测试结局数据")]
        public int testEndingId = 6001;
        public bool testAsDeathEnding = false;

        [Header("按键测试")]
        public bool enableKeyInput = true;

        private EndingScreenController controller;
        private EndingDataSO endingData;

        void Start()
        {
            EnsureEndingScreen();
            endingData = CreateTestEndingData();
        }

        void Update()
        {
            if (!enableKeyInput) return;

            if (Input.GetKeyDown(KeyCode.F3))
            {
                ShowTestEnding();
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                testAsDeathEnding = !testAsDeathEnding;
                Debug.Log($"[EndingScreenTester] 死亡结局模式: {testAsDeathEnding}");
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
                testEndingId = 6001;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                testEndingId = 6002;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                testEndingId = 6003;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                testEndingId = 6004;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                testEndingId = 6005;
        }

        void EnsureEndingScreen()
        {
            if (controller != null) return;

            // 查找场景中的 EndingScreen
            controller = FindObjectOfType<EndingScreenController>();
            if (controller == null && endingScreenPrefab != null)
            {
                var go = Instantiate(endingScreenPrefab);
                controller = go.GetComponent<EndingScreenController>();
            }

            if (controller == null)
            {
                Debug.LogError("[EndingScreenTester] 未找到或创建 EndingScreen");
                return;
            }

            controller.gameObject.SetActive(false);
        }

        void ShowTestEnding()
        {
            if (controller == null || endingData == null) return;

            var entry = endingData.GetEndingById(testEndingId);
            if (entry == null)
            {
                Debug.LogWarning($"[EndingScreenTester] 找不到结局ID: {testEndingId}");
                return;
            }

            controller.ShowEndingFromData(entry, testAsDeathEnding);
            Debug.Log($"[EndingScreenTester] 显示结局 [{testEndingId}] 死亡={testAsDeathEnding}");
        }

        EndingDataSO CreateTestEndingData()
        {
            var so = ScriptableObject.CreateInstance<EndingDataSO>();
            so.endings = new System.Collections.Generic.List<EndingEntry>
            {
                new EndingEntry { id = 6001, endingName = "【结局一：太阳照常升起】", description = "薯片改变不了任何事，你也是。" },
                new EndingEntry { id = 6002, endingName = "【结局二：莫比乌斯环】", description = "一条走廊，离开起点之时，你就明白你终会回来。" },
                new EndingEntry { id = 6003, endingName = "【结局三：人心不足蛇吞象】", description = "得失荣枯总在天，机关用尽也徒然。" },
                new EndingEntry { id = 6004, endingName = "【结局四：星垂之夜】", description = "俯仰天地之间——无愧于人，无愧于心，无愧于己。" },
                new EndingEntry { id = 6005, endingName = "【结局五：北极星】", description = "我对你透露一个大秘密，这是人类最古老的玩笑——无论往哪走，都是向前走。" }
            };
            return so;
        }

        [ContextMenu("测试：显示结局")]
        void TestShowEnding() { ShowTestEnding(); }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 100, 400, 90));
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("结局画面测试 (F3=显示 F4=切换死亡模式 1~5=切换结局)");
            GUILayout.Label($"当前: ID={testEndingId} 死亡={testAsDeathEnding}");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
