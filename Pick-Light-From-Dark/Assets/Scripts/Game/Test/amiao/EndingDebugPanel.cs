using UnityEngine;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 第五关结局调试面板 — 直接设定血量/卡牌状态，一键触发不同结局
    /// 按 F4 显隐面板
    /// </summary>
    public class EndingDebugPanel : MonoBehaviour
    {
        private bool visible = false;
        private Rect windowRect = new Rect(20, 20, 380, 460);

        private int lv1Lives = 2;
        private int lv2Lives = 2;
        private int lv3Lives = 2;
        private int lv5Lives = 2;
        private bool useCard2017 = false;
        private bool useCard2026 = false;

        private string statusMsg = "";
        private int lastEndingId = 0;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F4))
                visible = !visible;

            // F5: 直接显示结局画面
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("[EndingDebug] F5 快捷显示结局画面");
                WriteTestData();
                ShowEndingDirectly();
            }

            // F6: 循环切换预设并触发：结局二 → 结局三 → 结局四
            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (lv1Lives == 1 && lv2Lives == 1 && lv3Lives == 1 && lv5Lives == 1)
                    PresetXorCard();             // 全1血 → 结局二已触发过，下一步：仅一卡 → 结局三
                else if (!useCard2017 || !useCard2026)
                    PresetBothCards();           // → 两卡全用 → 结局四
                else
                    PresetMobius();              // → 回到全1血 → 结局二

                WriteTestData();
                var save = GetOrCreateSaveSystem();
                save.EvaluateEnding();
                EventCenter.Instance?.EventTrigger(E_EventType.GameWin);
            }
        }

        void OnGUI()
        {
            if (!visible) return;
            windowRect = GUILayout.Window(99, windowRect, DrawWindow, "结局调试面板 (F4)");
        }

        void DrawWindow(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("=== 各关通关血量 ===", GUI.skin.box);
            lv1Lives = (int)GUILayout.HorizontalSlider(lv1Lives, 1, 3);
            GUILayout.Label($"第一关血量: {lv1Lives}");
            lv2Lives = (int)GUILayout.HorizontalSlider(lv2Lives, 1, 3);
            GUILayout.Label($"第二关血量: {lv2Lives}");
            lv3Lives = (int)GUILayout.HorizontalSlider(lv3Lives, 1, 3);
            GUILayout.Label($"第三关血量: {lv3Lives}");
            lv5Lives = (int)GUILayout.HorizontalSlider(lv5Lives, 1, 3);
            GUILayout.Label($"第五关血量: {lv5Lives}");

            GUILayout.Space(5);
            GUILayout.Label("=== 卡牌使用 ===", GUI.skin.box);
            useCard2017 = GUILayout.Toggle(useCard2017, "第二关使用卡牌2017（分享泡面）");
            useCard2026 = GUILayout.Toggle(useCard2026, "第五关使用卡牌2026（寻求帮助）");

            GUILayout.Space(10);

            // 快捷预设按钮
            GUILayout.Label("=== 快捷预设 ===", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("全1血→结局二", GUILayout.Height(24))) PresetMobius();
            if (GUILayout.Button("仅一卡→结局三", GUILayout.Height(24))) PresetXorCard();
            if (GUILayout.Button("两卡全→结局四", GUILayout.Height(24))) PresetBothCards();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUI.color = Color.green;
            if (GUILayout.Button("写入存档数据", GUILayout.Height(30)))
                WriteTestData();
            GUI.color = Color.white;

            if (GUILayout.Button("仅判定结局", GUILayout.Height(24)))
                EvaluateOnly();

            GUI.color = Color.yellow;
            if (GUILayout.Button("触发完整结局流程", GUILayout.Height(30)))
                TriggerFullEnding();
            GUI.color = Color.white;

            GUI.color = Color.cyan;
            if (GUILayout.Button("直接显示结局画面（跳过对话）", GUILayout.Height(30)))
                ShowEndingDirectly();
            GUI.color = Color.white;

            // 判定结果高亮
            if (lastEndingId != 0)
            {
                GUI.color = GetEndingColor(lastEndingId);
                GUILayout.Label($"■ {GetEndingName(lastEndingId)}", GUI.skin.box);
                GUI.color = Color.white;
            }

            if (!string.IsNullOrEmpty(statusMsg))
                GUILayout.Label(statusMsg, GUI.skin.box);

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        // ========== 快捷预设 ==========

        void PresetMobius()
        {
            lv1Lives = 1; lv2Lives = 1; lv3Lives = 1; lv5Lives = 1;
            useCard2017 = false; useCard2026 = false;
            statusMsg = "预设：全1血 → 结局二 莫比乌斯环";
        }

        void PresetXorCard()
        {
            lv1Lives = 2; lv2Lives = 2; lv3Lives = 2; lv5Lives = 2;
            useCard2017 = true; useCard2026 = false;
            statusMsg = "预设：仅用2017卡 → 结局三 星垂之夜";
        }

        void PresetBothCards()
        {
            lv1Lives = 2; lv2Lives = 2; lv3Lives = 2; lv5Lives = 2;
            useCard2017 = true; useCard2026 = true;
            statusMsg = "预设：两卡全用 → 结局四 北极星";
        }

        // ========== 核心逻辑 ==========

        CrossLevelSaveSystem GetOrCreateSaveSystem()
        {
            var save = CrossLevelSaveSystem.Instance;
            if (save == null)
            {
                var go = new GameObject("CrossLevelSaveSystem");
                save = go.AddComponent<CrossLevelSaveSystem>();
            }
            return save;
        }

        void WriteTestData()
        {
            var save = GetOrCreateSaveSystem();

            save.RecordLevelResult(1, lv1Lives);
            save.RecordLevelResult(2, lv2Lives, card2017: useCard2017);
            save.RecordLevelResult(3, lv3Lives);
            save.RecordLevelResult(5, lv5Lives, card2026: useCard2026);

            if (useCard2017) save.RecordCardUsed(2017);
            if (useCard2026) save.RecordCardUsed(2026);

            statusMsg = $"已写入: Lv1={lv1Lives} Lv2={lv2Lives}{C(2017,useCard2017)} Lv3={lv3Lives} Lv5={lv5Lives}{C(2026,useCard2026)}";
        }

        void EvaluateOnly()
        {
            WriteTestData();
            var save = GetOrCreateSaveSystem();
            int result = save.EvaluateEnding();
            lastEndingId = result;
            statusMsg = $"判定: {GetEndingName(result)}";
        }

        void TriggerFullEnding()
        {
            WriteTestData();
            var save = GetOrCreateSaveSystem();
            int result = save.EvaluateEnding();
            if (result == 0) { statusMsg = "无法判定结局：请确保已设置关卡结果并使用卡牌"; return; }
            lastEndingId = result;
            EventCenter.Instance?.EventTrigger(E_EventType.GameWin);
            statusMsg = $"GameWin → {GetEndingName(lastEndingId)}";
        }

        void ShowEndingDirectly()
        {
            WriteTestData();
            var save = GetOrCreateSaveSystem();
            int endingId = save.EvaluateEnding();

            lastEndingId = endingId;
            if (endingId == 0) { statusMsg = "无法判定结局：请确保已设置关卡结果并使用卡牌"; return; }

            GameObject prefab = GetEndingPrefab(endingId);
            if (prefab == null)
            {
                statusMsg = $"未找到结局{endingId}的预制体";
                return;
            }

            Transform parent = null;
            if (UIMgr.Instance != null)
                parent = UIMgr.Instance.GetLayerFather(E_UILayer.Middle);
            if (parent == null)
                parent = FindFirstObjectByType<Canvas>()?.transform;

            var endingObj = Instantiate(prefab, parent, false);
            var rt = endingObj.GetComponent<RectTransform>();
            if (rt == null) rt = endingObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 结局一已有 Ending1SunRisesButtonBinder，不要覆盖
            if (endingId == 6001 && endingObj.GetComponent<Ending1SunRisesButtonBinder>() == null)
            {
                endingObj.AddComponent<Ending1SunRisesButtonBinder>();
            }
            else if (endingId != 6001 && endingObj.GetComponent<Ending2Panel>() == null)
            {
                endingObj.AddComponent<Ending2Panel>();
            }

            statusMsg = $"已显示: {GetEndingName(endingId)}";
        }

        GameObject GetEndingPrefab(int endingId)
        {
            // 优先从 VN Controller 获取
            var vn = FindFirstObjectByType<FungusVNController>();
            GameObject prefab = null;
            if (vn != null)
            {
                prefab = endingId switch
                {
                    6001 => vn.ending1Prefab,
                    6002 => vn.ending2Prefab,
                    6003 => vn.ending3Prefab,
                    6004 => vn.ending4Prefab,
                    _ => null
                };
            }

            // 兜底：从 Resources/UI/MainFlow/ 加载
            if (prefab == null)
            {
                string num = (endingId % 1000).ToString();
                prefab = Resources.Load<GameObject>($"UI/MainFlow/Ending{num}");
            }

            return prefab;
        }

        Color GetEndingColor(int id)
        {
            switch (id)
            {
                case 6001: return Color.gray;
                case 6002: return new Color(0.6f, 0.2f, 0.8f); // 紫
                case 6003: return new Color(0.2f, 0.4f, 0.9f); // 蓝
                case 6004: return new Color(0.9f, 0.7f, 0.1f); // 金
                default: return Color.white;
            }
        }

        string GetEndingName(int id)
        {
            switch (id)
            {
                case 6001: return "结局一：太阳照常升起";
                case 6002: return "结局二：莫比乌斯环";
                case 6003: return "结局三：星垂之夜";
                case 6004: return "结局四：北极星";
                case 0: return "无法判定...";
                default: return $"未知结局({id})";
            }
        }

        string C(int cardId, bool used) => used ? $" +卡{cardId}" : "";
    }
}
