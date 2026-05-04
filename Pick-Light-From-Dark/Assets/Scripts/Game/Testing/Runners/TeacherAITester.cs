using UnityEngine;
using Game.AI;
using Game.Config;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 教师 AI 手动测试器
    /// 配合 PlayerState/Emotion/CardReading 测试查寝判定
    /// </summary>
    public class TeacherAITester : MonoBehaviour
    {
        [Header("测试关卡配置")]
        public LevelConfigSO testConfig;

        [Header("UI 显示")]
        public bool showOnGUI = true;

        private TeacherAI teacherAI;
        private int stateChangedCount;
        private int caughtCount;

        void Awake()
        {
            teacherAI = GetComponent<TeacherAI>();
            if (teacherAI == null)
                teacherAI = gameObject.AddComponent<TeacherAI>();
        }

        void Start()
        {
            if (testConfig != null) teacherAI.Initialize(testConfig);
            EventCenter.Instance.AddEventListener<TeacherState>(E_EventType.TeacherStateChanged, OnStateChanged);
            EventCenter.Instance.AddEventListener(E_EventType.PlayerCaught, OnCaught);
            Debug.Log("[TeacherAITester] 已就绪");
        }

        void OnDestroy()
        {
            if (EventCenter.Instance == null) return;
            EventCenter.Instance.RemoveEventListener<TeacherState>(E_EventType.TeacherStateChanged, OnStateChanged);
            EventCenter.Instance.RemoveEventListener(E_EventType.PlayerCaught, OnCaught);
        }

        private void OnStateChanged(TeacherState s)
        {
            stateChangedCount++;
            Debug.Log($"[TeacherAITester] TeacherStateChanged → {s}（第{stateChangedCount}次）");
        }

        private void OnCaught()
        {
            caughtCount++;
            Debug.Log($"[TeacherAITester] PlayerCaught 第{caughtCount}次");
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            int x = 10, y = 10, w = 380, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 220), "TeacherAI 测试器");
            y += 25;

            if (teacherAI != null)
            {
                GUI.Label(new Rect(x, y, w, h), $"状态: {teacherAI.GetCurrentState()}  类型: {teacherAI.GetCurrentInspectType()}"); y += h;
                GUI.Label(new Rect(x, y, w, h), $"接近进度: {teacherAI.GetApproachProgress():P0}  可见: {teacherAI.IsVisible()}"); y += h;
                GUI.Label(new Rect(x, y, w, h), $"正在检查: {teacherAI.IsInspecting()}  接近中: {teacherAI.IsApproaching()}"); y += h;
            }
            y += 4;
            GUI.Label(new Rect(x, y, w, h), $"事件计数: 状态变更={stateChangedCount} 被抓={caughtCount}"); y += h + 4;

            if (GUI.Button(new Rect(x, y, w, h), "Initialize")) DoInitialize(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "重置计数器"))
            {
                stateChangedCount = caughtCount = 0;
            }
        }

        [ContextMenu("Initialize")]
        public void DoInitialize()
        {
            if (testConfig == null)
            {
                Debug.LogError("[TeacherAITester] testConfig 未指定");
                return;
            }
            teacherAI.Initialize(testConfig);
        }
    }
}
