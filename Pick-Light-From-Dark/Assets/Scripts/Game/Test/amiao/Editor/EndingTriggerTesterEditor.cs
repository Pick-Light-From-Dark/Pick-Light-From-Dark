using UnityEditor;
using UnityEngine;

namespace Game.Test.Editor
{
    [CustomEditor(typeof(EndingTriggerTester))]
    public class EndingTriggerTesterEditor : UnityEditor.Editor
    {
        EndingTriggerTester tester;

        void OnEnable()
        {
            tester = (EndingTriggerTester)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ========== 通用设置 ==========
            DrawCommonSection();
            EditorGUILayout.Space(10);

            // ========== 区域一：死亡结局 ==========
            DrawDeadEndSection();
            EditorGUILayout.Space(10);

            // ========== 区域二：结局一 ==========
            DrawEnding1Section();
            EditorGUILayout.Space(10);

            // ========== 区域三：结局分支 ==========
            DrawEndingBranchSection();
            EditorGUILayout.Space(10);

            // ========== 底部：预制体引用 ==========
            DrawPrefabsSection();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawCommonSection()
        {
            EditorGUILayout.LabelField("通用设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableHotkeys"));
        }

        void DrawDeadEndSection()
        {
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f, 1f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = bg;

            EditorGUILayout.LabelField("【区域一】死亡结局测试 — 随时触发", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentLives"));

            if (GUILayout.Button("触发死亡结局", GUILayout.Height(28)))
            {
                tester.TriggerDeadEndFromInspector();
            }

            EditorGUILayout.EndVertical();
        }

        void DrawEnding1Section()
        {
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.9f, 0.7f, 0.2f, 1f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = bg;

            EditorGUILayout.LabelField("【区域二】结局一：太阳照常升起 — 第一关触发", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("level1Choice"));

            if (GUILayout.Button("触发结局一", GUILayout.Height(28)))
            {
                tester.TriggerEnding1FromInspector();
            }

            EditorGUILayout.EndVertical();
        }

        void DrawEndingBranchSection()
        {
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f, 1f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = bg;

            EditorGUILayout.LabelField("【区域三】结局分支 — 第五关条件区分", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("各关通关血量", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("level1Lives"), new GUIContent("L1"), GUILayout.Width(60));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("level2Lives"), new GUIContent("L2"), GUILayout.Width(60));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("level3Lives"), new GUIContent("L3"), GUILayout.Width(60));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("level5Lives"), new GUIContent("L5"), GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("usedCard2017"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("usedCard2026"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rooftopChoice"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("判定并触发结局", GUILayout.Height(28)))
            {
                tester.EvaluateAndTriggerEnding5FromInspector();
            }
            if (GUILayout.Button("重置", GUILayout.Width(60), GUILayout.Height(28)))
            {
                tester.ResetEnding5Conditions();
            }
            EditorGUILayout.EndHorizontal();

            string result = tester.GetEvaluateResult();
            if (!string.IsNullOrEmpty(result))
            {
                EditorGUILayout.HelpBox(result, MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        void DrawPrefabsSection()
        {
            EditorGUILayout.LabelField("结局预制体引用", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ending1Prefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ending2Prefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ending4Prefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ending5Prefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("deathEndingPrefab"));
        }
    }
}
