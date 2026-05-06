using UnityEngine;
using Game.Flow;
using Game.Config;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 游戏流程手动测试器
    /// 测试 Initialize / Pause / Resume / GameWin / GameLose / OnPlayerCaught
    /// </summary>
    public class GameFlowTester : MonoBehaviour
    {
        [Header("测试关卡配置")]
        public LevelConfigSO testConfig;

        [Header("UI 显示")]
        public bool showOnGUI = true;

        private GameFlowController gameFlow;
        private int gameStartCount, pauseCount, resumeCount, winCount, loseCount;

        void Start()
        {
            gameFlow = GameFlowController.Instance;

            EventCenter.Instance.AddEventListener(E_EventType.GameStart, OnGameStart);
            EventCenter.Instance.AddEventListener(E_EventType.GamePause, OnGamePause);
            EventCenter.Instance.AddEventListener(E_EventType.GameResume, OnGameResume);
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);

            Debug.Log("[GameFlowTester] 已就绪");
        }

        void OnDestroy()
        {
            if (EventCenter.Instance == null) return;
            EventCenter.Instance.RemoveEventListener(E_EventType.GameStart, OnGameStart);
            EventCenter.Instance.RemoveEventListener(E_EventType.GamePause, OnGamePause);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameResume, OnGameResume);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        private void OnGameStart() { gameStartCount++; Debug.Log($"[GameFlowTester] GameStart {gameStartCount}"); }
        private void OnGamePause() { pauseCount++; Debug.Log($"[GameFlowTester] GamePause {pauseCount}"); }
        private void OnGameResume() { resumeCount++; Debug.Log($"[GameFlowTester] GameResume {resumeCount}"); }
        private void OnGameWin() { winCount++; Debug.Log($"[GameFlowTester] GameWin {winCount}"); }
        private void OnGameLose(string reason) { loseCount++; Debug.Log($"[GameFlowTester] GameLose 原因={reason} {loseCount}"); }

        void OnGUI()
        {
            if (!showOnGUI) return;

            int x = 10, y = 10, w = 360, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 320), "GameFlow 测试器");
            y += 25;

            if (gameFlow != null)
            {
                GUI.Label(new Rect(x, y, w, h), $"剩余时间: {gameFlow.GetRemainingTime():F2}s"); y += h;
                GUI.Label(new Rect(x, y, w, h), $"暂停: {gameFlow.IsPaused()}  结束: {gameFlow.IsGameOver()}"); y += h;
                GUI.Label(new Rect(x, y, w, h), $"timeScale={Time.timeScale:F2}"); y += h;
            }
            y += 4;
            GUI.Label(new Rect(x, y, w, h), $"事件: Start={gameStartCount} Pause={pauseCount} Resume={resumeCount}"); y += h;
            GUI.Label(new Rect(x, y, w, h), $"      Win={winCount} Lose={loseCount}"); y += h + 4;

            if (GUI.Button(new Rect(x, y, w, h), "Initialize 关卡")) DoInitialize(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "PauseGame")) gameFlow.PauseGame(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "ResumeGame")) gameFlow.ResumeGame(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "GameWin")) gameFlow.GameWin(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "GameLose(\"测试触发\")")) gameFlow.GameLose("测试触发"); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "OnPlayerCaught (Pause+2s恢复)")) gameFlow.OnPlayerCaught(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "重置事件计数器"))
            {
                gameStartCount = pauseCount = resumeCount = winCount = loseCount = 0;
            }
        }

        [ContextMenu("Initialize")]
        public void DoInitialize()
        {
            if (testConfig == null)
            {
                Debug.LogError("[GameFlowTester] testConfig 未指定");
                return;
            }
            gameFlow.Initialize(testConfig);
        }
    }
}
