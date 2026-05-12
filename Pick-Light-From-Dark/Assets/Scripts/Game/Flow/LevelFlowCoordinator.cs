using UnityEngine;
using Game.Config;
using Game.Test;
using Fungus;

namespace Game.Flow
{
    /// <summary>
    /// 关卡流程协调器 — 统筹一关的完整三段式生命周期：
    /// 开场剧情 → 游玩(打牌) → 结尾剧情 → 结果面板
    /// </summary>
    public class LevelFlowCoordinator : MonoBehaviour
    {
        [Header("剧情文本")]
        public TextAsset openingStory;
        public TextAsset endingStory;

        [Header("关卡配置")]
        public LevelConfigSO levelConfig;
        public int levelId = 1;

        [Header("VN 控制器")]
        public FungusVNController vnController;

        [Header("下一关预制体（NextLevel 分支时实例化）")]
        public GameObject nextLevelPrefab;

        private bool isGameOver = false;
        private Game.AI.TeacherAI teacherAI;

        void Start()
        {
            if (vnController == null)
            {
                vnController = FindObjectOfType<FungusVNController>();
                if (vnController == null)
                {
                    Debug.LogError("[LevelFlowCoordinator] 未找到 FungusVNController，请在 Inspector 中赋值或确保场景中存在");
                    return;
                }
            }

            StartOpeningStory();
        }

        /// <summary>阶段1：开场剧情</summary>
        void StartOpeningStory()
        {
            if (openingStory == null)
            {
                Debug.LogWarning("[LevelFlowCoordinator] openingStory 未赋值，跳过开场剧情直接进入游玩");
                StartGameplay();
                return;
            }

            Debug.Log("[LevelFlowCoordinator] === 阶段1：开场剧情 ===");
            vnController.dialogueText = openingStory;
            vnController.OnDialogueExit = OnOpeningStoryExit;
            vnController.OnDialogueComplete = OnOpeningStoryEnd;
            vnController.skipToChoiceIfAvailable = (levelId == 1);
            // 重置并启动 VN 控制器
            vnController.ClearPlaceholder();
            vnController.SetSkipButtonVisible(true);
            vnController.RestartDialogue();
        }

        /// <summary>开场剧情分支结束回调（带分支类型）</summary>
        void OnOpeningStoryExit(VNExitType exitType)
        {
            Debug.Log($"[LevelFlowCoordinator] 开场剧情分支结束: {exitType}");

            switch (exitType)
            {
                case VNExitType.Ending:
                    // 直接进入结局流程（跳过游玩）
                    StartEndingStory();
                    break;
                case VNExitType.NextLevel:
                    // 接下一个预制体
                    LoadNextPrefab();
                    break;
                case VNExitType.Gameplay:
                case VNExitType.None:
                default:
                    // 默认进入游玩
                    OnOpeningStoryEnd();
                    break;
            }
        }

        /// <summary>开场剧情结束 → 自动存档 → 进入游玩</summary>
        void OnOpeningStoryEnd()
        {
            Debug.Log("[LevelFlowCoordinator] 开场剧情结束，自动存档...");

            // 自动存档：使用 Fungus SaveManager
            if (vnController != null && vnController.saveFlowchart != null)
            {
                vnController.saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", true);
            }

            var saveManager = FungusManager.Instance.SaveManager;
            saveManager.AddSavePoint("OpeningComplete", "开场剧情结束自动存档");
            saveManager.Save("vn_save");
            Debug.Log("[LevelFlowCoordinator] 开场剧情结束，Fungus 自动存档完成");

            StartGameplay();
        }

        /// <summary>阶段2：游玩（打牌）</summary>
        void StartGameplay()
        {
            Debug.Log("[LevelFlowCoordinator] === 阶段2：游玩 ===");

            // 创建老师AI
            var teacherObj = new GameObject("TeacherAI");
            teacherAI = teacherObj.AddComponent<Game.AI.TeacherAI>();

            // 显示游戏面板并初始化
            UIMgr.Instance.ShowPanel<GamePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    panel.InitializeWithConfig(levelConfig);
                    teacherAI.Initialize(levelConfig);
                }
            );

            // 监听游玩结束事件
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        /// <summary>阶段3a：游戏胜利 → 结尾剧情</summary>
        void OnGameWin()
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log("[LevelFlowCoordinator] 游戏胜利，进入结尾剧情...");
            CleanupTeacherAI();
            UIMgr.Instance.HidePanel<GamePanel>();
            UnsubscribeGameEvents();
            StartEndingStory();
        }

        /// <summary>阶段3b：游戏失败 → 显示失败提示</summary>
        void OnGameLose(string reason)
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log($"[LevelFlowCoordinator] 游戏失败: {reason}");
            CleanupTeacherAI();
            UIMgr.Instance.HidePanel<GamePanel>();
            UnsubscribeGameEvents();

            UIMgr.Instance.ShowPanel<TipPanel>();
        }

        /// <summary>阶段3：结尾剧情</summary>
        void StartEndingStory()
        {
            if (endingStory == null)
            {
                Debug.LogWarning("[LevelFlowCoordinator] endingStory 未赋值，跳过结尾剧情");
                ShowVictoryPanel();
                return;
            }

            Debug.Log("[LevelFlowCoordinator] === 阶段3：结尾剧情 ===");
            vnController.dialogueText = endingStory;
            vnController.OnDialogueExit = OnEndingStoryExit;
            vnController.OnDialogueComplete = OnEndingStoryEnd;
            vnController.ClearPlaceholder();
            vnController.SetSkipButtonVisible(false);
            vnController.RestartDialogue();
        }

        /// <summary>结尾剧情分支结束回调（带分支类型）</summary>
        void OnEndingStoryExit(VNExitType exitType)
        {
            Debug.Log($"[LevelFlowCoordinator] 结尾剧情分支结束: {exitType}");

            switch (exitType)
            {
                case VNExitType.NextLevel:
                    LoadNextPrefab();
                    break;
                case VNExitType.Ending:
                case VNExitType.Gameplay:
                case VNExitType.None:
                default:
                    OnEndingStoryEnd();
                    break;
            }
        }

        /// <summary>结尾剧情结束 → 显示胜利/结算面板</summary>
        void OnEndingStoryEnd()
        {
            Debug.Log("[LevelFlowCoordinator] 结尾剧情结束，显示结算...");
            ShowVictoryPanel();
        }

        /// <summary>加载下一关预制体</summary>
        void LoadNextPrefab()
        {
            if (nextLevelPrefab != null)
            {
                Debug.Log($"[LevelFlowCoordinator] 实例化下一关预制体: {nextLevelPrefab.name}");
                Instantiate(nextLevelPrefab);
                // 销毁当前关卡对象（可选：如果不销毁，新预制体会叠加在场景中）
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[LevelFlowCoordinator] nextLevelPrefab 未赋值，无法进入下一关");
                ShowVictoryPanel();
            }
        }

        /// <summary>显示胜利结算面板</summary>
        void ShowVictoryPanel()
        {
            UIMgr.Instance.ShowPanel<EndingContentPanel>();
            Debug.Log("[LevelFlowCoordinator] 已显示 EndingContentPanel（胜利结算），可在此扩展结算 UI");
        }

        /// <summary>取消游戏事件订阅</summary>
        void UnsubscribeGameEvents()
        {
            EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        void CleanupTeacherAI()
        {
            if (teacherAI != null)
            {
                Destroy(teacherAI.gameObject);
                teacherAI = null;
            }
        }

        void OnDestroy()
        {
            CleanupTeacherAI();
            UnsubscribeGameEvents();
        }
    }
}
