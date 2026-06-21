#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Game.Test;

namespace Game.Test.Editor
{
    /// <summary>
    /// 一键生成 Level 1 新手引导配置
    /// </summary>
    public static class TutorialConfigGenerator
    {
        [MenuItem("Tools/生成新手引导配置/Level 1 引导配置")]
        public static void GenerateLevel1Config()
        {
            // 确保目录存在
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Tutorial"))
                AssetDatabase.CreateFolder("Assets/Resources", "Tutorial");

            string path = "Assets/Resources/Tutorial/TutorialConfig_Level1.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TutorialConfigSO>(path);
            TutorialConfigSO config;
            if (existing != null)
            {
                config = existing;
                config.steps.Clear();
            }
            else
            {
                config = ScriptableObject.CreateInstance<TutorialConfigSO>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.levelId = 1;
            config.steps = new System.Collections.Generic.List<TutorialStep>
            {
                // 步骤1：开场（无遮罩，纯文字）
                new TutorialStep
                {
                    stepId = "lv1_welcome",
                    targetObjectName = "",
                    instructionText = "深夜的宿舍里，你需要悄悄行动，完成自己的事情。\n但要小心——宿管老师正在走廊里巡逻。",
                    cutoutSize = new Vector2(0, 0),
                    showMask = false,
                    indicator = IndicatorType.Border,
                    trigger = TutorialTrigger.AnyClick
                },

                // 步骤2：认识卡牌区 — 右上角
                new TutorialStep
                {
                    stepId = "lv1_card_area",
                    targetObjectName = "CreateCardZone",
                    instructionText = "这里是你的行动卡牌。\n将卡牌拖入下方的思考框中即可使用。",
                    cutoutSize = new Vector2(380, 380),
                    cutoutAnchor = CutoutAnchor.TopRight,
                    indicator = IndicatorType.Border,
                    trigger = TutorialTrigger.AnyClick
                },

                // 步骤3：认识思考框
                new TutorialStep
                {
                    stepId = "lv1_thinkbox",
                    targetObjectName = "SetCardZone",
                    instructionText = "这是思考框——松手后卡牌开始读条。\n绿色段落可以按空格键打断，红色段落无法打断。",
                    cutoutSize = new Vector2(400, 240),
                    indicator = IndicatorType.Border,
                    trigger = TutorialTrigger.AnyClick
                },

                // 步骤4：情绪系统 — 左下角
                new TutorialStep
                {
                    stepId = "lv1_emotion",
                    targetObjectName = "PlayerMessage",
                    instructionText = "左下角是你的情绪值。点击可查看慌乱值与兴奋值的详情。\n绿色=安全  黄色=警告  红色=危险",
                    cutoutSize = new Vector2(480, 480),
                    cutoutAnchor = CutoutAnchor.BottomLeft,
                    indicator = IndicatorType.Finger,
                    trigger = TutorialTrigger.AnyClick
                },

                // 步骤5：闭眼
                new TutorialStep
                {
                    stepId = "lv1_eyeclose",
                    targetObjectName = "CloseEyesBtn",
                    instructionText = "这是闭眼按钮（或按 C 键）。\n闭上眼睛可以装睡，降低慌乱值。",
                    cutoutSize = new Vector2(160, 160),
                    indicator = IndicatorType.Border,
                    trigger = TutorialTrigger.AnyClick
                },

                // 步骤6：老师警告
                new TutorialStep
                {
                    stepId = "lv1_teacher_warning",
                    targetObjectName = "TeacherStatus",
                    instructionText = "当屏幕上出现老师状态提示时，确保自己：\n✓ 躺在床上  ✓ 闭上眼睛  ✓ 情绪值不过高",
                    cutoutSize = new Vector2(380, 150),
                    indicator = IndicatorType.Finger,
                    trigger = TutorialTrigger.AnyClick
                },

                // 步骤7：任务面板
                new TutorialStep
                {
                    stepId = "lv1_task",
                    targetObjectName = "EventImgBk",
                    instructionText = "屏幕右侧是今晚要完成的任务清单。\n全部完成后即可过关。",
                    cutoutSize = new Vector2(340, 230),
                    indicator = IndicatorType.Border,
                    trigger = TutorialTrigger.AnyClick
                },

                // 步骤8：收尾
                new TutorialStep
                {
                    stepId = "lv1_end",
                    targetObjectName = "HpImg",
                    instructionText = "你只有 2 点生命值。被老师抓住 2 次就会强制退学。\n祝你好运！",
                    cutoutSize = new Vector2(120, 120),
                    indicator = IndicatorType.Finger,
                    trigger = TutorialTrigger.AnyClick
                },
            };

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TutorialConfig] Level 1 引导配置已生成: {path} (共{config.steps.Count}步)");
            Selection.activeObject = config;
        }

        [MenuItem("Tools/生成新手引导配置/打开引导配置文件夹")]
        public static void OpenConfigFolder()
        {
            string folderPath = "Assets/Resources/Tutorial";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Tutorial");
            }
            var obj = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            Selection.activeObject = obj;
        }

        [MenuItem("Tools/生成新手引导配置/重置 Level 1 引导（重新触发）")]
        public static void ResetLevel1Tutorial()
        {
            const string key = "CrossLevelSave_v3";
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                // 简单替换：把 completedTutorials 数组清空
                var data = JsonUtility.FromJson<CrossLevelSaveData>(json);
                if (data != null && data.completedTutorials != null)
                {
                    data.completedTutorials.Remove(1);
                    string newJson = JsonUtility.ToJson(data, true);
                    PlayerPrefs.SetString(key, newJson);
                    PlayerPrefs.Save();
                    Debug.Log("[TutorialConfig] Level 1 引导已重置，下次进Level1将重新触发");
                }
            }
            else
            {
                Debug.Log("[TutorialConfig] 无存档数据，无需重置");
            }
        }

        [MenuItem("Tools/生成新手引导配置/重置全部游戏进度（清空存档）")]
        public static void ResetAllProgress()
        {
            const string key = "CrossLevelSave_v3";
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                Debug.Log("[TutorialConfig] ★ 全部游戏进度已清空（CrossLevelSave_v3 已删除）");
            }
            else
            {
                Debug.Log("[TutorialConfig] 无存档数据");
            }
        }
    }
}
#endif
