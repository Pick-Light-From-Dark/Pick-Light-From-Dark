using System.Collections;
using System.Linq;
using Game.Data;
using Game.Flow;
using Game.Task;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 第二关玩法场景（Level2）测试：F8 跳过开场并触发 GameWin → 经 bridge 进 Level2_end。
    /// 仅挂在 Level2，不要挂在 Level2_end。
    /// </summary>
    public class Level2QuickWinHotkey : MonoBehaviour
    {
        const string LogTag = "[Level2QuickWin]";
        const string PlaySceneName = "Level2";
        const string VictorySceneName = "Level2_end";

        [Tooltip("一键通关快捷键（Unity 2022 下 F8 = 310）")]
        public KeyCode hotKey = KeyCode.F8;

        [Tooltip("仅在该 levelId 的关卡流程中生效")]
        public int levelId = 2;

        [Tooltip("等待进入游玩阶段的最长时间（秒）")]
        public float waitGameplaySeconds = 12f;

        bool _running;

        void Update()
        {
            if (_running) return;
            if (!Input.GetKeyDown(KeyCode.F8) && !Input.GetKeyDown(hotKey)) return;

            var sceneName = SceneManager.GetActiveScene().name;

            if (IsVictoryStoryScene(sceneName))
            {
                HandleVictorySceneF8();
                return;
            }

            if (!IsPlayScene(sceneName))
            {
                Debug.LogWarning($"{LogTag} 当前场景「{sceneName}」不是 {PlaySceneName}，请从 Level2 运行后再按 F8");
                return;
            }

            var coord = ResolveCoordinator();
            if (coord == null)
            {
                Debug.LogWarning($"{LogTag} 未找到 LevelFlowCoordinator（请在 {PlaySceneName} 场景运行，且物体已启用）");
                return;
            }

            if (coord.levelId != levelId)
            {
                Debug.LogWarning($"{LogTag} 当前 levelId={coord.levelId}，本快捷键仅对 levelId={levelId} 生效");
                return;
            }

            StartCoroutine(QuickClearRoutine(coord));
        }

        static bool IsPlayScene(string sceneName) =>
            sceneName == PlaySceneName || sceneName == "Level2";

        static bool IsVictoryStoryScene(string sceneName) =>
            sceneName == VictorySceneName
            || sceneName.Equals("level2_end", System.StringComparison.OrdinalIgnoreCase);

        static LevelFlowCoordinator ResolveCoordinator() =>
            LevelFlowCoordinator.Instance != null
                ? LevelFlowCoordinator.Instance
                : FindFirstObjectByType<LevelFlowCoordinator>();

        void HandleVictorySceneF8()
        {
            var vn = FindFirstObjectByType<FungusVNController>();
            if (vn != null && vn.IsStoryPlaying)
            {
                Debug.Log($"{LogTag} Level2_end：跳过胜利剧情");
                vn.SkipCurrentStory();
                return;
            }

            var runner = FindFirstObjectByType<Level2VictoryStoryRunner>();
            var next = runner != null ? runner.nextSceneName : "Level3";
            Debug.Log($"{LogTag} Level2_end：直接进入 {next}");
            SceneManager.LoadScene(next);
        }

        IEnumerator QuickClearRoutine(LevelFlowCoordinator coord)
        {
            _running = true;

            var flow = GameFlowController.Instance;
            if (flow != null && flow.IsGameOver())
            {
                Debug.Log($"{LogTag} 已在通关流程（将进 {VictorySceneName}），无需再按 F8");
                _running = false;
                yield break;
            }

            Debug.Log($"{LogTag} F8：跳过开场并触发 GameWin → {VictorySceneName}");

            var vn = coord.vnController != null
                ? coord.vnController
                : FindFirstObjectByType<FungusVNController>();

            float waited = 0f;
            while (waited < waitGameplaySeconds)
            {
                flow = GameFlowController.Instance;
                if (flow != null && flow.IsInitialized)
                    break;

                SkipOpeningStoryOnly(vn);
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            flow = GameFlowController.Instance;
            if (flow == null)
            {
                Debug.LogWarning($"{LogTag} GameFlowController 不存在");
                _running = false;
                yield break;
            }

            if (!flow.IsInitialized)
            {
                Debug.LogWarning($"{LogTag} 超时仍未进入游玩，请再按一次 F8 或等开场结束");
                _running = false;
                yield break;
            }

            if (flow.IsGameOver())
            {
                Debug.Log($"{LogTag} 已进入通关流程");
                _running = false;
                yield break;
            }

            Debug.Log($"{LogTag} 强制完成任务 → GameWin");
            ForceCompleteAllTasks();
            flow.GameWin();

            Debug.Log($"{LogTag} 完成；将经 bridge 进入 {VictorySceneName}");
            _running = false;
        }

        static void SkipOpeningStoryOnly(FungusVNController vn)
        {
            if (vn == null || !vn.IsStoryPlaying) return;
            var name = vn.dialogueText != null ? vn.dialogueText.name : "";
            if (name.Contains("win-bridge") || name.Contains("2-2"))
                return;

            Debug.Log($"{LogTag} 跳过开场 VN: {name}");
            vn.SkipCurrentStory();
        }

        static void ForceCompleteAllTasks()
        {
            var tm = TaskManager.Instance;
            if (tm == null)
            {
                Debug.LogWarning($"{LogTag} TaskManager 不存在，仍将仅靠 GameWin 进结尾");
                return;
            }

            var goals = tm.GetActiveGoals().ToList();
            foreach (var goal in goals)
            {
                if (goal.state == TaskState.Pending)
                    tm.RevealHiddenTask(goal.targetCardId);
            }

            foreach (var goal in tm.GetActiveGoals())
            {
                if (goal.state == TaskState.Completed) continue;
                int remain = Mathf.Max(1, goal.targetCount - goal.currentCount);
                for (int i = 0; i < remain; i++)
                    tm.HandleCardCompleted(goal.targetCardId);
            }
        }
    }
}
