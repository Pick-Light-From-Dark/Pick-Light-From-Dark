using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Config;
using Game.Test;
using Fungus;

namespace Game.Flow
{
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

        [Header("场景跳转")]
        public string nextLevelSceneName = "";
        public string currentLevelSceneName = "";

        [Header("下一关预制体（NextLevel 分支时实例化）")]
        public GameObject nextLevelPrefab;

        private bool isGameOver = false;
        private Game.AI.TeacherAI teacherAI;

        public string NextLevelSceneName => nextLevelSceneName;
        public string CurrentLevelSceneName => currentLevelSceneName;

        public static LevelFlowCoordinator Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (vnController == null)
            {
                vnController = FindObjectOfType<FungusVNController>();
                if (vnController == null)
                {
                    Debug.LogError("[LevelFlowCoordinator] 未找到 FungusVNController");
                    return;
                }
            }

            StartOpeningStory();
        }

        void StartOpeningStory()
        {
            if (openingStory == null)
            {
                StartGameplay();
                return;
            }

            Debug.Log("[LevelFlowCoordinator] === 阶段1：开场剧情 ===");
            vnController.dialogueText = openingStory;
            vnController.OnDialogueExit = OnOpeningStoryExit;
            vnController.OnDialogueComplete = OnOpeningStoryEnd;
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

        void OnOpeningStoryEnd()
        {
            if (vnController != null && vnController.saveFlowchart != null)
            {
                vnController.saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", true);
            }

            var saveManager = FungusManager.Instance.SaveManager;
            saveManager.AddSavePoint("OpeningComplete", "开场剧情结束自动存档");
            saveManager.Save("vn_save");

            StartGameplay();
        }

        void StartGameplay()
        {
            Debug.Log("[LevelFlowCoordinator] === 阶段2：游玩 ===");

            // 清理跨场景单例可能残留的脏状态
            Time.timeScale = 1f;
            // 先初始化 GameFlowController，避免 GamePanel.Awake 用 TestLevelConfig 自初始化
            GameFlowController.Instance.Initialize(levelConfig);

            var teacherObj = new GameObject("TeacherAI");
            teacherAI = teacherObj.AddComponent<Game.AI.TeacherAI>();

            UIMgr.Instance.ShowPanel<GamePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    panel.InitializeWithConfig(levelConfig);
                    teacherAI.Initialize(levelConfig);
                }
            );

            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

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

        void StartEndingStory()
        {
            if (endingStory == null)
            {
                ShowVictoryPanel();
                return;
            }

            Debug.Log("[LevelFlowCoordinator] === 阶段3：结尾剧情 ===");
            vnController.dialogueText = endingStory;
            vnController.OnDialogueExit = OnEndingStoryExit;
            vnController.OnDialogueComplete = OnEndingStoryEnd;
            vnController.ClearPlaceholder();
            vnController.SetSkipButtonVisible(true);
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

        void OnEndingStoryEnd()
        {
            ShowVictoryPanel();
        }


        /// <summary>加载下一关预制体</summary>
        void LoadNextPrefab()
        {
            if (nextLevelPrefab != null)
            {
                Debug.Log($"[LevelFlowCoordinator] 实例化下一关预制体: {nextLevelPrefab.name}");
                Instantiate(nextLevelPrefab);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[LevelFlowCoordinator] nextLevelPrefab 未赋值，无法进入下一关");
                ShowVictoryPanel();
            }
        }

        void ShowVictoryPanel()
        {
            UIMgr.Instance.ShowPanel<EndingContentPanel>();
        }

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
            if (Instance == this) Instance = null;
        }
    }
}