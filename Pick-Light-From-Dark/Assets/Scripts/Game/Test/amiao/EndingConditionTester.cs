using UnityEngine;
using Game.Config;
using Game.Flow;

namespace Game.Test
{
    /// <summary>
    /// 结局条件测试器 — 在 Inspector 中手动勾选条件测试判定规则，或直接触发任意结局画面
    /// </summary>
    public class EndingConditionTester : MonoBehaviour
    {
        // ========== 直接触发结局（Inspector 快捷操作） ==========
        [Header("直接触发结局画面")]
        [Tooltip("选择要直接触发的结局，然后点右键菜单或按数字键触发")]
        public DirectTriggerEnding directTrigger = DirectTriggerEnding.Ending1_SunRises;

        [Tooltip("结局数据配置（未赋值时自动从 Resources/Config/EndingData 加载）")]
        public EndingDataSO endingData;

        [Tooltip("场景中无 EndingManager 时是否自动创建")]
        public bool autoInitManager = true;

        [Tooltip("运行时按对应数字键触发结局 (1/2/4/5=结局1/2/4/5, 0=死亡结局)")]
        public bool enableHotkeys = true;

        public enum DirectTriggerEnding
        {
            [InspectorName("结局一：太阳照常升起")] Ending1_SunRises = 6001,
            [InspectorName("结局二：莫比乌斯环")] Ending2_Mobius = 6002,
            [InspectorName("结局四：星垂之夜")] Ending4_StarryNight = 6004,
            [InspectorName("结局五：北极星")] Ending5_Polaris = 6005,
            [InspectorName("死亡结局")] Death = -1,
        }

        // ========== 条件判定测试（保留原有功能） ==========
        [Header("条件判定测试")]
        [Range(0, 5)]
        public int level1Lives = 2;
        [Range(0, 5)]
        public int level2Lives = 2;
        [Range(0, 5)]
        public int level3Lives = 2;
        [Range(0, 5)]
        public int level5Lives = 2;

        [Tooltip("第二关是否使用了分享泡面卡 (2017)")]
        public bool usedCard2017 = false;
        [Tooltip("第五关是否使用了寻求宋明月帮助 (2026)")]
        public bool usedCard2026 = false;

        [Tooltip("两卡全部使用后，在木门前做出的选择")]
        public RooftopChoice rooftopChoice = RooftopChoice.None;

        [Header("判定结果（只读）")]
        [SerializeField]
        private string lastResult = "点击右键菜单 '测试结局判定' 或按 T 键进行测试";

        public enum RooftopChoice
        {
            [InspectorName("未选择")] None,
            [InspectorName("独自前往")] Alone,
            [InspectorName("邀请宋明月")] WithFriend
        }

        void Start()
        {
            EnsureEndingData();
        }

        void Update()
        {
            if (!enableHotkeys) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                TriggerDirectEnding(DirectTriggerEnding.Ending1_SunRises);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                TriggerDirectEnding(DirectTriggerEnding.Ending2_Mobius);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                TriggerDirectEnding(DirectTriggerEnding.Ending4_StarryNight);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                TriggerDirectEnding(DirectTriggerEnding.Ending5_Polaris);
            else if (Input.GetKeyDown(KeyCode.Alpha0))
                TriggerDirectEnding(DirectTriggerEnding.Death);
            else if (Input.GetKeyDown(KeyCode.T))
                EvaluateEnding();
        }

        // ========== 直接触发结局 ==========

        [ContextMenu("触发选中的结局")]
        public void TriggerSelectedEnding()
        {
            TriggerDirectEnding(directTrigger);
        }

        public void TriggerDirectEnding(DirectTriggerEnding ending)
        {
            int endingId = (int)ending;

            if (endingId == -1)
            {
                TriggerDeathEnding();
                return;
            }

            EnsureEndingData();
            if (endingData == null)
            {
                Debug.LogError("[EndingConditionTester] endingData 为 null，无法触发结局");
                return;
            }

            var entry = endingData.GetEndingById(endingId);
            if (entry == null)
            {
                Debug.LogError($"[EndingConditionTester] 找不到结局ID: {endingId}");
                return;
            }

            if (autoInitManager)
                EnsureEndingManager();

            if (EndingManager.Instance == null)
            {
                Debug.LogError("[EndingConditionTester] EndingManager 单例不存在");
                return;
            }

            Debug.Log($"[EndingConditionTester] 触发结局 [{endingId}] {entry.endingName}");
            EndingManager.Instance.TriggerEnding(endingId);
        }

        [ContextMenu("触发死亡结局")]
        public void TriggerDeathEnding()
        {
            Debug.Log("[EndingConditionTester] 触发死亡结局 (GameLose)");
            EventCenter.Instance?.EventTrigger(E_EventType.GameLose, "测试死亡结局");
        }

        // ========== 条件判定（原有逻辑保留） ==========

        [ContextMenu("测试结局判定")]
        public void EvaluateEnding()
        {
            int endingId = 0;
            string endingName = "";
            string reason = "";

            // P0: 莫比乌斯环
            if (level1Lives == 1 && level2Lives == 1 && level3Lives == 1 && level5Lives == 1)
            {
                endingId = 6002;
                endingName = "莫比乌斯环";
                reason = "P0：第1/2/3/5关通关血量全部为 1 点";
            }
            else if (level1Lives > 1 || level2Lives > 1 || level3Lives > 1 || level5Lives > 1)
            {
                bool bothCardsUsed = usedCard2017 && usedCard2026;

                if (!bothCardsUsed)
                {
                    endingId = 6004;
                    endingName = "星垂之夜";
                    reason = "P1-情况A：至少一关血量>1，两卡未全部使用";
                }
                else if (rooftopChoice == RooftopChoice.Alone)
                {
                    endingId = 6004;
                    endingName = "星垂之夜";
                    reason = "P1-情况B：至少一关血量>1，两卡全部使用，选择【独自前往】";
                }
                else if (rooftopChoice == RooftopChoice.WithFriend)
                {
                    endingId = 6005;
                    endingName = "北极星";
                    reason = "P1：至少一关血量>1，两卡全部使用，选择【邀请宋明月】";
                }
                else
                {
                    endingId = -1;
                    endingName = "待选择";
                    reason = "P1：两卡全部使用，请在 Inspector 中选择【独自前往】或【邀请宋明月】后再次测试";
                }
            }
            else
            {
                endingId = -1;
                endingName = "无匹配";
                reason = "不满足任何结局条件（血量异常或数据缺失）";
            }

            lastResult = $"结局 [{endingId}] {endingName}\n原因：{reason}";
            Debug.Log($"[EndingConditionTester] 判定结果：{lastResult}");
        }

        [ContextMenu("重置为默认条件")]
        public void ResetToDefault()
        {
            level1Lives = 2;
            level2Lives = 2;
            level3Lives = 2;
            level5Lives = 2;
            usedCard2017 = false;
            usedCard2026 = false;
            rooftopChoice = RooftopChoice.None;
            lastResult = "已重置";
            Debug.Log("[EndingConditionTester] 条件已重置为默认值");
        }

        // ========== 内部工具 ==========

        void EnsureEndingData()
        {
            if (endingData != null) return;
            endingData = Resources.Load<EndingDataSO>("Config/EndingData");
        }

        void EnsureEndingManager()
        {
            EnsureEndingData();
            if (endingData == null)
            {
                Debug.LogError("[EndingConditionTester] endingData 为 null，无法初始化 EndingManager");
                return;
            }

            // 通过场景查找避免触发 SingletonAutoMono 的自动创建（那会生成未初始化的实例）
            var existing = FindFirstObjectByType<EndingManager>();
            if (existing != null)
            {
                if (existing.GetEndingData() == null)
                    existing.Initialize(endingData);
                return;
            }

            // 手动创建并初始化
            var go = new GameObject("EndingManager");
            var mgr = go.AddComponent<EndingManager>();
            mgr.Initialize(endingData);
        }
    }
}
