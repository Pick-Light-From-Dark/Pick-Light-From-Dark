using UnityEngine;
using System.Collections.Generic;

namespace Game.Test
{
    /// <summary>
    /// 剧情串联测试器：将多个分段 Prefab 按顺序播放，形成完整剧情 Demo。
    /// 跳过游玩部分，纯剧情流程测试。
    /// </summary>
    public class StoryChainTestRunner : MonoBehaviour
    {
        [Header("剧情 Prefab 序列（按顺序）")]
        [Tooltip("第一关主流程")]
        public GameObject day1_1;
        [Tooltip("第一关 吃分支")]
        public GameObject day1_2a;
        [Tooltip("第一关 不吃分支")]
        public GameObject day1_2b;
        [Tooltip("第二关上段")]
        public GameObject day2_1;
        [Tooltip("第二关下段")]
        public GameObject day2_2;
        [Tooltip("第三关上段")]
        public GameObject day3_1;
        [Tooltip("第三关下段")]
        public GameObject day3_2;
        [Tooltip("第四关上段")]
        public GameObject day4_1;
        [Tooltip("第四关下段")]
        public GameObject day4_2;

        [Header("选项分支配置")]
        [Tooltip("是否强制进入 吃 分支（测试用）")]
        public bool forceEatBranch = false;
        [Tooltip("是否强制进入 不吃 分支（测试用）")]
        public bool forceNotEatBranch = false;

        private List<GameObject> sequence = new List<GameObject>();
        private int currentIndex = -1;
        private GameObject currentInstance;
        private string choiceResult = "";

        void Start()
        {
            BuildSequence();
            PlayNext();
        }

        void BuildSequence()
        {
            sequence.Clear();

            // 第一关主流程
            if (day1_1 != null) sequence.Add(day1_1);

            // 第一关分支（根据选项结果）
            if (forceEatBranch && day1_2a != null)
            {
                sequence.Add(day1_2a);
            }
            else if (forceNotEatBranch && day1_2b != null)
            {
                sequence.Add(day1_2b);
            }
            // 否则等待运行时选项事件决定

            // 后续关卡
            if (day2_1 != null) sequence.Add(day2_1);
            if (day2_2 != null) sequence.Add(day2_2);
            if (day3_1 != null) sequence.Add(day3_1);
            if (day3_2 != null) sequence.Add(day3_2);
            if (day4_1 != null) sequence.Add(day4_1);
            if (day4_2 != null) sequence.Add(day4_2);
        }

        void PlayNext()
        {
            // 如果当前是第一关主流程之后，需要处理分支
            if (currentIndex == 0 && choiceResult == "")
            {
                // 等待选项结果，先不继续
                return;
            }

            currentIndex++;

            // 如果第一关分支还没加入序列，现在加入
            if (currentIndex == 1 && choiceResult != "")
            {
                if (choiceResult == "eat" && day1_2a != null)
                    sequence.Insert(1, day1_2a);
                else if (choiceResult == "not_eat" && day1_2b != null)
                    sequence.Insert(1, day1_2b);
            }

            if (currentIndex >= sequence.Count)
            {
                Debug.Log("[StoryChainTestRunner] 全部剧情播放完毕");
                return;
            }

            var prefab = sequence[currentIndex];
            if (prefab == null)
            {
                Debug.LogWarning($"[StoryChainTestRunner] 序列第 {currentIndex} 项为空，跳过");
                PlayNext();
                return;
            }

            // 销毁上一个实例
            if (currentInstance != null)
            {
                Destroy(currentInstance);
                currentInstance = null;
            }

            Debug.Log($"[StoryChainTestRunner] 播放第 {currentIndex + 1}/{sequence.Count} 段: {prefab.name}");
            currentInstance = Instantiate(prefab);

            // 找到 VN 控制器并订阅结束事件
            var vn = currentInstance.GetComponentInChildren<FungusVNController>();
            if (vn != null)
            {
                vn.OnDialogueComplete += OnSegmentEnd;
                vn.OnDialogueExit += OnSegmentExit;
            }
            else
            {
                Debug.LogWarning($"[StoryChainTestRunner] {prefab.name} 中未找到 FungusVNController");
            }
        }

        void OnSegmentExit(VNExitType exitType)
        {
            Debug.Log($"[StoryChainTestRunner] 剧情段结束: {exitType}");
        }

        void OnSegmentEnd()
        {
            Debug.Log("[StoryChainTestRunner] 剧情段播放完成");
            PlayNext();
        }

        void OnEnable()
        {
            EventCenter.Instance.AddEventListener<string>(E_EventType.StoryChoiceMade, OnStoryChoice);
        }

        void OnDisable()
        {
            if (EventCenter.Instance != null)
                EventCenter.Instance.RemoveEventListener<string>(E_EventType.StoryChoiceMade, OnStoryChoice);
        }

        void OnStoryChoice(string choiceId)
        {
            Debug.Log($"[StoryChainTestRunner] 选项结果: {choiceId}");

            // 第一关选项判断
            if (choiceId.Contains("eat") || choiceId.Contains("吃"))
            {
                if (!choiceId.Contains("not") && !choiceId.Contains("不"))
                    choiceResult = "eat";
                else
                    choiceResult = "not_eat";
            }

            // 如果正在等待第一关分支，继续播放
            if (currentIndex == 0 && choiceResult != "")
            {
                PlayNext();
            }
        }

        [ContextMenu("测试：强制选择 吃 分支")]
        void TestForceEat()
        {
            forceEatBranch = true;
            forceNotEatBranch = false;
            BuildSequence();
        }

        [ContextMenu("测试：强制选择 不吃 分支")]
        void TestForceNotEat()
        {
            forceEatBranch = false;
            forceNotEatBranch = true;
            BuildSequence();
        }
    }
}
