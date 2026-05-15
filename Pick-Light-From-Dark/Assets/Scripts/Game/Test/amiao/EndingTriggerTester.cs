using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 结局场景调试器 — Inspector 分组调试 + 按键快捷触发 + 后端接口
    /// 直接 Instantiate 结局预制体，不依赖 EndingManager 完整管线
    /// </summary>
    public class EndingTriggerTester : MonoBehaviour
    {
        // ========== 通用设置 ==========
        [Header("通用设置")]
        [Tooltip("运行时按对应数字键触发结局 (1=结局一, 2=结局二, 4=结局四, 5=结局五, 0=死亡结局, Esc=销毁)")]
        public bool enableHotkeys = true;

        [Header("结局预制体")]
        public GameObject ending1Prefab;
        public GameObject ending2Prefab;
        public GameObject ending4Prefab;
        public GameObject ending5Prefab;
        public GameObject deathEndingPrefab;

        private GameObject currentEndingPanel;

        // ========== 区域一：死亡结局测试（随时触发） ==========
        [Header("【区域一】死亡结局测试 — 随时触发")]
        [Tooltip("当场剩余血量（测试用标注，不影响直接触发）")]
        [Range(0, 5)]
        public int currentLives = 0;

        // ========== 区域二：结局一 — 太阳照常升起（第一关触发） ==========
        [Header("【区域二】结局一：太阳照常升起 — 第一关触发")]
        [Tooltip("第一关分支选择")]
        public Level1Choice level1Choice = Level1Choice.NotEat;

        public enum Level1Choice
        {
            [InspectorName("不吃（结局一）")] NotEat,
            [InspectorName("吃（普通继续）")] Eat,
        }

        // ========== 区域三：结局分支 — 第五关条件区分 ==========
        [Header("【区域三】结局分支 — 第五关条件区分")]
        [Range(0, 5)] public int level1Lives = 2;
        [Range(0, 5)] public int level2Lives = 2;
        [Range(0, 5)] public int level3Lives = 2;
        [Range(0, 5)] public int level5Lives = 2;

        [Tooltip("第二关是否使用了分享泡面卡 (2017)")]
        public bool usedCard2017 = false;
        [Tooltip("第五关是否使用了寻求宋明月帮助 (2026)")]
        public bool usedCard2026 = false;

        [Tooltip("两卡全部使用后，在木门前做出的选择")]
        public RooftopChoice rooftopChoice = RooftopChoice.None;

        [Header("判定结果（只读）")]
        [SerializeField]
        private string evaluateResult = "点击 Inspector 按钮进行判定";

        public enum RooftopChoice
        {
            [InspectorName("未选择")] None,
            [InspectorName("独自前往")] Alone,
            [InspectorName("邀请宋明月")] WithFriend
        }

        // ========== 生命周期 ==========

        void Update()
        {
            if (!enableHotkeys) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                ShowEnding(ending1Prefab, 6001);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                ShowEnding(ending2Prefab, 6002);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                ShowEnding(ending4Prefab, 6004);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                ShowEnding(ending5Prefab, 6005);
            else if (Input.GetKeyDown(KeyCode.Alpha0))
                TriggerDeadEnd(currentLives);
            else if (Input.GetKeyDown(KeyCode.Escape) && currentEndingPanel != null)
                Destroy(currentEndingPanel);
        }

        // ========== 区域一：死亡结局 ==========

        /// <summary>
        /// 使用 Inspector 中的 currentLives 触发死亡结局测试
        /// </summary>
        [ContextMenu("【区域一】测试死亡结局")]
        public void TriggerDeadEndFromInspector()
        {
            TriggerDeadEnd(currentLives);
        }

        /// <summary>
        /// 后端接口：当场血量测试死亡结局
        /// </summary>
        public void TriggerDeadEnd(int lives)
        {
            Debug.Log($"[EndingTriggerTester] 触发死亡结局测试 | 当场血量: {lives}");
            ShowEnding(deathEndingPrefab, -1);
        }

        // ========== 区域二：结局一 ==========

        /// <summary>
        /// 使用 Inspector 中的 level1Choice 触发结局一
        /// </summary>
        [ContextMenu("【区域二】触发结局一")]
        public void TriggerEnding1FromInspector()
        {
            TriggerEnding1(level1Choice == Level1Choice.NotEat ? "not_eat" : "eat");
        }

        /// <summary>
        /// 后端接口：触发结局一（太阳照常升起）
        /// </summary>
        public void TriggerEnding1(string level1ChoiceId = "not_eat")
        {
            Debug.Log($"[EndingTriggerTester] 触发结局一 | 第一关选择: {level1ChoiceId}");
            ShowEnding(ending1Prefab, 6001);
        }

        // ========== 区域三：第五关结局分支 ==========

        /// <summary>
        /// 使用 Inspector 中的条件判定并触发对应结局
        /// </summary>
        [ContextMenu("【区域三】判定并触发结局")]
        public void EvaluateAndTriggerEnding5FromInspector()
        {
            TriggerEnding5Branch(level1Lives, level2Lives, level3Lives, level5Lives,
                usedCard2017, usedCard2026, rooftopChoice);
        }

        /// <summary>
        /// 后端接口：根据第五关条件判定并触发对应结局
        /// </summary>
        public void TriggerEnding5Branch(int l1, int l2, int l3, int l5, bool card2017, bool card2026, RooftopChoice choice)
        {
            int endingId = 0;
            string endingName = "";
            string reason = "";

            // P0: 莫比乌斯环
            if (l1 == 1 && l2 == 1 && l3 == 1 && l5 == 1)
            {
                endingId = 6002;
                endingName = "莫比乌斯环";
                reason = "P0：第1/2/3/5关通关血量全部为 1 点";
            }
            else if (l1 > 1 || l2 > 1 || l3 > 1 || l5 > 1)
            {
                bool bothCardsUsed = card2017 && card2026;

                if (!bothCardsUsed)
                {
                    endingId = 6004;
                    endingName = "星垂之夜";
                    reason = "P1-情况A：至少一关血量>1，两卡未全部使用";
                }
                else if (choice == RooftopChoice.Alone)
                {
                    endingId = 6004;
                    endingName = "星垂之夜";
                    reason = "P1-情况B：至少一关血量>1，两卡全部使用，选择【独自前往】";
                }
                else if (choice == RooftopChoice.WithFriend)
                {
                    endingId = 6005;
                    endingName = "北极星";
                    reason = "P1：至少一关血量>1，两卡全部使用，选择【邀请宋明月】";
                }
                else
                {
                    evaluateResult = "请在 Inspector 中选择【独自前往】或【邀请宋明月】后再次测试";
                    Debug.LogWarning($"[EndingTriggerTester] {evaluateResult}");
                    return;
                }
            }
            else
            {
                evaluateResult = "不满足任何结局条件（血量异常或数据缺失）";
                Debug.LogWarning($"[EndingTriggerTester] {evaluateResult}");
                return;
            }

            evaluateResult = $"结局 [{endingId}] {endingName}\n原因：{reason}";
            Debug.Log($"[EndingTriggerTester] 判定结果：{evaluateResult}");

            GameObject prefab = endingId switch
            {
                6002 => ending2Prefab,
                6004 => ending4Prefab,
                6005 => ending5Prefab,
                _ => null
            };

            ShowEnding(prefab, endingId);
        }

        [ContextMenu("【区域三】重置为默认条件")]
        public void ResetEnding5Conditions()
        {
            level1Lives = 2;
            level2Lives = 2;
            level3Lives = 2;
            level5Lives = 2;
            usedCard2017 = false;
            usedCard2026 = false;
            rooftopChoice = RooftopChoice.None;
            evaluateResult = "已重置为默认值";
            Debug.Log("[EndingTriggerTester] 第五关条件已重置为默认值");
        }

        // ========== 底层通用接口 ==========

        /// <summary>
        /// 底层通用接口：实例化并显示指定结局预制体
        /// </summary>
        public void ShowEnding(GameObject prefab, int endingId)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[EndingTriggerTester] 结局预制体为 null (ID: {endingId})");
                return;
            }

            // 销毁已显示的结局面板
            if (currentEndingPanel != null)
                Destroy(currentEndingPanel);

            // 查找或创建 Canvas
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("EndingCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // 确保 EventSystem 存在
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            currentEndingPanel = Instantiate(prefab, canvas.transform);
            Debug.Log($"[EndingTriggerTester] 显示结局面板 ID: {endingId}");
        }

        /// <summary>
        /// 获取上次判定结果（供 Inspector / 外部调用读取）
        /// </summary>
        public string GetEvaluateResult()
        {
            return evaluateResult;
        }
    }
}
