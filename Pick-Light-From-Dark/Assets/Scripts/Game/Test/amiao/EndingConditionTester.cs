using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 结局条件测试器 — 在 Inspector 中手动勾选条件，测试会进入哪个结局
    /// 用于验证 EndingBranchAnalysis.md 中的判定规则
    /// </summary>
    public class EndingConditionTester : MonoBehaviour
    {
        [Header("各关通关血量")]
        [Range(0, 5)]
        public int level1Lives = 2;
        [Range(0, 5)]
        public int level2Lives = 2;
        [Range(0, 5)]
        public int level3Lives = 2;
        [Range(0, 5)]
        public int level5Lives = 2;

        [Header("卡牌使用记录")]
        [Tooltip("第二关是否使用了分享泡面卡 (2017)")]
        public bool usedCard2017 = false;
        [Tooltip("第五关是否使用了寻求宋明月帮助 (2026)")]
        public bool usedCard2026 = false;

        [Header("木门选择")]
        [Tooltip("两卡全部使用后，在木门前做出的选择")]
        public RooftopChoice rooftopChoice = RooftopChoice.None;

        [Header("判定结果（只读）")]
        [SerializeField]
        private string lastResult = "点击右键菜单 '测试结局判定' 或按 T 键进行测试";

        public enum RooftopChoice
        {
            [InspectorName("未选择")]
            None,
            [InspectorName("独自前往")]
            Alone,
            [InspectorName("邀请宋明月")]
            WithFriend
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                EvaluateEnding();
            }
        }

        [ContextMenu("测试结局判定")]
        public void EvaluateEnding()
        {
            int endingId = 0;
            string endingName = "";
            string reason = "";

            // P0: 莫比乌斯环 — 最高优先级，强制覆盖
            if (level1Lives == 1 && level2Lives == 1 && level3Lives == 1 && level5Lives == 1)
            {
                endingId = 6002;
                endingName = "莫比乌斯环";
                reason = "P0：第1/2/3/5关通关血量全部为 1 点";
            }
            // P1: 至少一关血量 > 1
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
    }
}
