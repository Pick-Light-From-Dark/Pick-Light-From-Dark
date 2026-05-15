using UnityEngine;
using System.Collections.Generic;
using Game.Config;
using Game.Flow;

namespace Game.Test
{
    /// <summary>
    /// 剧情串联测试器 v2：支持多关分支选项与结局判定。
    /// 将各关剧情 Prefab 串联，根据选项进入不同分支，最终触发结局画面。
    /// </summary>
    public class StoryChainTestRunner : MonoBehaviour
    {
        // ========== 原有字段（保持 prefab 兼容）==========
        [Header("剧情 Prefab 序列")]
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

        [Header("第五关 Prefab")]
        [Tooltip("第五关主流程（含选项，可选）")]
        public GameObject day5_2;
        [Tooltip("第五关 迷茫结局分支")]
        public GameObject day5_2a;
        [Tooltip("第五关 网吧结局分支")]
        public GameObject day5_2b;
        [Tooltip("第五关 天台结局分支")]
        public GameObject day5_2c;
        [Tooltip("第五关 邀友同行结局分支")]
        public GameObject day5_2d;
        [Tooltip("第五关 邀友同行结局分支（备用）")]
        public GameObject day5_2e;

        [Header("选项分支配置")]
        [Tooltip("是否强制进入 吃 分支（测试用）")]
        public bool forceEatBranch = false;
        [Tooltip("是否强制进入 不吃 分支（测试用）")]
        public bool forceNotEatBranch = false;
        [Tooltip("强制第五关分支（测试用，为 None 则按选项或默认）")]
        public Day5Branch forceDay5Branch = Day5Branch.None;

        // ========== 新增：结局与条件配置 ==========
        [Header("结局数据配置（为空则使用内置默认）")]
        public EndingDataSO endingData;

        [Header("结局触发条件（Inspector 可手动配置，按列表顺序优先匹配）")]
        public List<EndingCondition> endingConditions = new List<EndingCondition>();

        [Header("测试用玩家状态（用于结局判定）")]
        [Tooltip("当前生命值")]
        public int testLives = 3;
        [Tooltip("当前情绪值")]
        public int testEmotion = 50;
        [Tooltip("已使用卡牌ID列表")]
        public List<int> testUsedCards = new List<int>();

        // ========== 运行时状态 ==========
        private List<GameObject> sequence = new List<GameObject>();
        private int currentIndex = -1;
        private GameObject currentInstance;
        private string choiceResult = "";
        private bool isEndingTriggered = false;

        private Dictionary<string, GameObject> nodeMap = new Dictionary<string, GameObject>();
        private Dictionary<string, string> branchRoutes = new Dictionary<string, string>();

        void Start()
        {
            BuildNodeMap();
            BuildBranchRoutes();
            InitializeEndingManager();
            BuildInitialSequence();
            PlayNext();
        }

        void BuildNodeMap()
        {
            nodeMap.Clear();
            AddToNodeMap("day1_1", day1_1);
            AddToNodeMap("day1_2a", day1_2a);
            AddToNodeMap("day1_2b", day1_2b);
            AddToNodeMap("day2_1", day2_1);
            AddToNodeMap("day2_2", day2_2);
            AddToNodeMap("day3_1", day3_1);
            AddToNodeMap("day3_2", day3_2);
            AddToNodeMap("day4_1", day4_1);
            AddToNodeMap("day4_2", day4_2);
            AddToNodeMap("day5_2", day5_2);
            AddToNodeMap("day5_2a", day5_2a);
            AddToNodeMap("day5_2b", day5_2b);
            AddToNodeMap("day5_2c", day5_2c);
            AddToNodeMap("day5_2d", day5_2d);
            AddToNodeMap("day5_2e", day5_2e);
        }

        void AddToNodeMap(string id, GameObject prefab)
        {
            if (prefab != null && !nodeMap.ContainsKey(id))
                nodeMap[id] = prefab;
        }

        void BuildBranchRoutes()
        {
            branchRoutes.Clear();
            // 第一关分支
            branchRoutes["day1_1|eat"] = "day1_2a";
            branchRoutes["day1_1|not_eat"] = "day1_2b";
            // 后续关卡线性连接
            branchRoutes["day1_2a|next"] = "day2_1";
            branchRoutes["day1_2b|next"] = "ENDING_6001"; // 不吃分支 → 结局一
            branchRoutes["day2_1|next"] = "day2_2";
            branchRoutes["day2_2|next"] = "day3_1";
            branchRoutes["day3_1|next"] = "day3_2";
            branchRoutes["day3_2|next"] = "day4_1";
            branchRoutes["day4_1|next"] = "day4_2";
            branchRoutes["day4_2|next"] = "day5_2";
            // 第五关分支（选项映射到不同分支 prefab）
            branchRoutes["day5_2|confused"] = "day5_2a";
            branchRoutes["day5_2|internet"] = "day5_2b";
            branchRoutes["day5_2|rooftop"] = "day5_2c";
            branchRoutes["day5_2|friend"] = "day5_2d";
            // 第五关分支 prefab 结束后 → 对应结局
            branchRoutes["day5_2a|next"] = "ENDING_6002";
            branchRoutes["day5_2b|next"] = "ENDING_6003";
            branchRoutes["day5_2c|next"] = "ENDING_6004";
            branchRoutes["day5_2d|next"] = "ENDING_6005";
            branchRoutes["day5_2e|next"] = "ENDING_6005";
        }

        void InitializeEndingManager()
        {
            if (EndingManager.Instance == null)
            {
                var go = new GameObject("EndingManager");
                go.AddComponent<EndingManager>();
            }
            if (endingData == null)
            {
                endingData = CreateDefaultEndingData();
            }
            EndingManager.Instance.Initialize(endingData);
        }

        EndingDataSO CreateDefaultEndingData()
        {
            var so = ScriptableObject.CreateInstance<EndingDataSO>();
            so.endings = new List<EndingEntry>
            {
                new EndingEntry { id = 6001, endingName = "【结局一：太阳照常升起】", description = "薯片改变不了任何事，你也是。" },
                new EndingEntry { id = 6002, endingName = "【结局二：莫比乌斯环】", description = "一条走廊，离开起点之时，你就明白你终会回来。" },
                new EndingEntry { id = 6003, endingName = "【结局三：人心不足蛇吞象】", description = "得失荣枯总在天，机关用尽也徒然。" },
                new EndingEntry { id = 6004, endingName = "【结局四：星垂之夜】", description = "俯仰天地之间——无愧于人，无愧于心，无愧于己。" },
                new EndingEntry { id = 6005, endingName = "【结局五：北极星】", description = "我对你透露一个大秘密，这是人类最古老的玩笑——无论往哪走，都是向前走。" }
            };

#if UNITY_EDITOR
            // 自动加载结局图片（Editor 下有效）
            string[] guids = new string[]
            {
                "b228fbb248c9a9347a59cad511d61ae7", // 结局1
                "891b939176dee734d82be9c3a98f542b", // 结局2
                "70fd483ae45d8214385bbb5646de088f", // 结局3
                "c7b0663dea5054f4295ed0523f1bd463", // 结局4
                "c9558d27d55857f45a5019451cc90a02"  // 结局5
            };
            for (int i = 0; i < so.endings.Count && i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!string.IsNullOrEmpty(path))
                {
                    so.endings[i].endingSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (so.endings[i].endingSprite != null)
                        Debug.Log($"[StoryChainTestRunner] 已加载结局图片: {so.endings[i].endingSprite.name}");
                }
            }
#endif
            return so;
        }

        void BuildInitialSequence()
        {
            sequence.Clear();
            currentIndex = -1;
            choiceResult = "";
            isEndingTriggered = false;

            if (day1_1 != null)
                sequence.Add(day1_1);

            // 强制分支（测试模式）
            if (forceEatBranch && day1_2a != null)
                sequence.Add(day1_2a);
            else if (forceNotEatBranch && day1_2b != null)
                sequence.Add(day1_2b);
        }

        void PlayNext()
        {
            // 如果还有预加载的节点，直接播放
            if (currentIndex + 1 < sequence.Count)
            {
                currentIndex++;
                PlayCurrent();
                return;
            }

            // 尝试根据当前节点 + choiceResult 或线性路由添加新节点
            if (currentIndex >= 0 && currentIndex < sequence.Count)
            {
                string currentNodeId = GetNodeId(sequence[currentIndex]);
                if (!string.IsNullOrEmpty(currentNodeId))
                {
                    string nextNodeId = ResolveNextNode(currentNodeId);
                    if (!string.IsNullOrEmpty(nextNodeId))
                    {
                        if (nextNodeId.StartsWith("ENDING_"))
                        {
                            int endingId = int.Parse(nextNodeId.Replace("ENDING_", ""));
                            TriggerEnding(endingId);
                            return;
                        }
                        AddToSequence(nextNodeId);
                    }
                    else
                    {
                        // 无路由且当前是结局节点 → 触发结局判定
                        EvaluateAndTriggerEnding();
                        return;
                    }
                }
            }

            currentIndex++;
            if (currentIndex >= sequence.Count)
            {
                Debug.Log("[StoryChainTestRunner] 全部剧情播放完毕");
                EvaluateAndTriggerEnding();
                return;
            }
            PlayCurrent();
        }

        string ResolveNextNode(string currentNodeId)
        {
            // 优先尝试 choiceResult 路由
            if (!string.IsNullOrEmpty(choiceResult))
            {
                string routeKey = $"{currentNodeId}|{choiceResult}";
                if (branchRoutes.TryGetValue(routeKey, out string result))
                {
                    choiceResult = "";
                    return result;
                }
            }
            // 尝试线性路由
            string linearKey = $"{currentNodeId}|next";
            if (branchRoutes.TryGetValue(linearKey, out string linearResult))
            {
                return linearResult;
            }
            return null;
        }

        void AddToSequence(string nodeId)
        {
            if (nodeMap.TryGetValue(nodeId, out GameObject prefab) && prefab != null)
            {
                sequence.Add(prefab);
                Debug.Log($"[StoryChainTestRunner] 添加节点: {nodeId}");
            }
            else
            {
                Debug.LogWarning($"[StoryChainTestRunner] 找不到节点 prefab: {nodeId}");
            }
        }

        void PlayCurrent()
        {
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

            // 订阅 VN 控制器事件
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

        string GetNodeId(GameObject prefab)
        {
            if (prefab == null) return "";
            foreach (var kvp in nodeMap)
            {
                if (kvp.Value == prefab) return kvp.Key;
            }
            return prefab.name;
        }

        void OnSegmentExit(VNExitType exitType)
        {
            Debug.Log($"[StoryChainTestRunner] 剧情段结束，exitType={exitType}");
            if (exitType == VNExitType.Ending && !isEndingTriggered)
            {
                isEndingTriggered = true;
                EvaluateAndTriggerEnding();
            }
        }

        void OnSegmentEnd()
        {
            Debug.Log("[StoryChainTestRunner] 剧情段播放完成");
            if (!isEndingTriggered)
            {
                PlayNext();
            }
            isEndingTriggered = false;
        }

        void EvaluateAndTriggerEnding()
        {
            int endingId = DetermineEnding();
            TriggerEnding(endingId);
        }

        void TriggerEnding(int endingId)
        {
            if (isEndingTriggered) return;
            isEndingTriggered = true;
            Debug.Log($"[StoryChainTestRunner] 触发结局 ID: {endingId}");
            if (EndingManager.Instance != null)
            {
                EndingManager.Instance.TriggerEnding(endingId);
            }
            else
            {
                Debug.LogError("[StoryChainTestRunner] EndingManager 为空，无法触发结局");
            }
        }

        int DetermineEnding()
        {
            // 按 Inspector 配置的条件优先匹配
            foreach (var condition in endingConditions)
            {
                if (condition != null && MatchesCondition(condition))
                {
                    Debug.Log($"[StoryChainTestRunner] 匹配结局条件: {condition.endingId}");
                    return condition.endingId;
                }
            }

            // 兜底：根据当前节点 ID 推断结局
            if (currentIndex >= 0 && currentIndex < sequence.Count)
            {
                string nodeId = GetNodeId(sequence[currentIndex]);
                switch (nodeId)
                {
                    case "day1_2b": return 6001;
                    case "day5_2a": return 6002;
                    case "day5_2b": return 6003;
                    case "day5_2c": return 6004;
                    case "day5_2d": return 6005;
                    case "day5_2e": return 6005;
                }
            }

            // 根据强制分支配置推断
            switch (forceDay5Branch)
            {
                case Day5Branch.Confused: return 6002;
                case Day5Branch.Internet: return 6003;
                case Day5Branch.Rooftop: return 6004;
                case Day5Branch.Friend: return 6005;
            }

            // 默认结局一
            Debug.Log("[StoryChainTestRunner] 未匹配任何条件，使用默认结局 6001");
            return 6001;
        }

        bool MatchesCondition(EndingCondition condition)
        {
            if (condition == null) return false;
            if (testLives < condition.minLives || testLives > condition.maxLives) return false;
            if (testEmotion < condition.minEmotion || testEmotion > condition.maxEmotion) return false;
            if (!string.IsNullOrEmpty(condition.requiredBranch) && choiceResult != condition.requiredBranch) return false;
            foreach (var cardId in condition.requiredCards)
            {
                if (!testUsedCards.Contains(cardId)) return false;
            }
            return true;
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
            Debug.Log($"[StoryChainTestRunner] 收到选项: {choiceId}");

            // 标准化选项标识
            if (choiceId.Contains("eat") || choiceId.Contains("吃"))
            {
                if (!choiceId.Contains("not") && !choiceId.Contains("不"))
                    choiceResult = "eat";
                else
                    choiceResult = "not_eat";
            }
            else if (choiceId.Contains("confused") || choiceId.Contains("迷茫") || choiceId.Contains("走廊"))
            {
                choiceResult = "confused";
            }
            else if (choiceId.Contains("internet") || choiceId.Contains("网吧"))
            {
                choiceResult = "internet";
            }
            else if (choiceId.Contains("rooftop") || choiceId.Contains("天台") || choiceId.Contains("独自"))
            {
                choiceResult = "rooftop";
            }
            else if (choiceId.Contains("friend") || choiceId.Contains("邀友") || choiceId.Contains("同行"))
            {
                choiceResult = "friend";
            }
            else
            {
                choiceResult = choiceId;
            }

            // 强制第五关分支（覆盖选项结果）
            if (forceDay5Branch != Day5Branch.None)
            {
                choiceResult = forceDay5Branch.ToString().ToLower();
                Debug.Log($"[StoryChainTestRunner] 强制第五关分支: {choiceResult}");
            }
        }

        [ContextMenu("测试：强制选择 吃 分支")]
        void TestForceEat()
        {
            forceEatBranch = true;
            forceNotEatBranch = false;
            RestartWithChoice("eat");
        }

        [ContextMenu("测试：强制选择 不吃 分支")]
        void TestForceNotEat()
        {
            forceEatBranch = false;
            forceNotEatBranch = true;
            RestartWithChoice("not_eat");
        }

        [ContextMenu("测试：触发结局二（迷茫）")]
        void TestEnding2()
        {
            TriggerEnding(6002);
        }

        [ContextMenu("测试：触发结局三（网吧）")]
        void TestEnding3()
        {
            TriggerEnding(6003);
        }

        [ContextMenu("测试：触发结局四（天台）")]
        void TestEnding4()
        {
            TriggerEnding(6004);
        }

        [ContextMenu("测试：触发结局五（邀友）")]
        void TestEnding5()
        {
            TriggerEnding(6005);
        }

        void RestartWithChoice(string choice)
        {
            if (currentInstance != null)
            {
                Destroy(currentInstance);
                currentInstance = null;
            }
            isEndingTriggered = false;
            choiceResult = choice;
            sequence.Clear();
            currentIndex = -1;
            BuildInitialSequence();
            PlayNext();
        }
    }

    /// <summary>
    /// 第五关分支枚举（Inspector 下拉菜单）
    /// </summary>
    public enum Day5Branch
    {
        None,
        Confused,
        Internet,
        Rooftop,
        Friend
    }

    /// <summary>
    /// 结局条件配置 — 可在 Inspector 中手动编辑，用于测试不同结局路径。
    /// </summary>
    [System.Serializable]
    public class EndingCondition
    {
        [Tooltip("触发的结局ID（6001~6005）")]
        public int endingId = 6001;

        [Tooltip("必须使用的卡牌ID列表（为空则不检查）")]
        public List<int> requiredCards = new List<int>();

        [Tooltip("最小生命值（包含）")]
        public int minLives = 0;

        [Tooltip("最大生命值（包含）")]
        public int maxLives = 999;

        [Tooltip("最小情绪值（包含）")]
        public int minEmotion = 0;

        [Tooltip("最大情绪值（包含）")]
        public int maxEmotion = 999;

        [Tooltip("必需的分支标识（如 eat / not_eat / confused / internet / rooftop / friend）")]
        public string requiredBranch = "";
    }
}
