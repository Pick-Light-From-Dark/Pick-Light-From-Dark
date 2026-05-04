using UnityEngine;
using Game.EyeClose;
using Game.Config;
using Game.Data;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 闭眼系统手动测试器
    /// 测试闭眼时长、时间加速、情绪降低
    /// </summary>
    public class EyeCloseSystemTester : MonoBehaviour
    {
        [Header("测试关卡配置")]
        public LevelConfigSO testConfig;

        [Header("UI 显示")]
        public bool showOnGUI = true;
        public KeyCode initializeKey = KeyCode.F1;
        public KeyCode toggleEyeKey = KeyCode.C;
        public KeyCode resetKey = KeyCode.F5;

        private EyeCloseSystem eyeCloseSystem;
        private PlayerState playerState;

        void Start()
        {
            eyeCloseSystem = EyeCloseSystem.Instance;
            playerState = PlayerState.Instance;
            Debug.Log("[EyeCloseSystemTester] 已就绪。F1=Initialize / C=切换闭眼 / F5=Reset");
        }

        void Update()
        {
            if (Input.GetKeyDown(initializeKey)) DoInitialize();
            if (Input.GetKeyDown(resetKey)) DoReset();
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            int x = 10, y = 10, w = 360, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 220), "EyeCloseSystem 测试器");
            y += 25;

            if (eyeCloseSystem != null)
            {
                GUI.Label(new Rect(x, y, w, h), $"闭眼时长: {eyeCloseSystem.GetEyeCloseDuration():F2}s"); y += h;
                GUI.Label(new Rect(x, y, w, h), $"时间加速: {eyeCloseSystem.IsTimeAccelerated()} (timeScale={Time.timeScale:F2})"); y += h;
            }
            if (playerState != null)
            {
                GUI.Label(new Rect(x, y, w, h), $"闭眼状态: {playerState.IsEyesClosed()}"); y += h;
                GUI.Label(new Rect(x, y, w, h), $"床上状态: {playerState.IsInBed()}"); y += h;
            }
            y += 4;

            if (GUI.Button(new Rect(x, y, w, h), "[F1] Initialize(读取关卡参数)")) DoInitialize(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "[C] 切换闭眼 (PlayerState)")) playerState.ToggleEyesClosed(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "[F5] Reset 闭眼系统")) DoReset(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "切换床上状态")) playerState.SetInBed(!playerState.IsInBed()); y += h;
        }

        [ContextMenu("Initialize")]
        public void DoInitialize()
        {
            if (testConfig == null)
            {
                Debug.LogError("[EyeCloseSystemTester] testConfig 未指定");
                return;
            }
            eyeCloseSystem.Initialize(testConfig);
            Debug.Log($"[EyeCloseSystemTester] Initialize 完成。阈值={testConfig.eyeCloseAccelerationThreshold}s 倍率x{testConfig.eyeCloseAccelerationMultiplier}");
        }

        [ContextMenu("Reset")]
        public void DoReset()
        {
            eyeCloseSystem.Reset();
            Debug.Log("[EyeCloseSystemTester] Reset 完成");
        }
    }
}
