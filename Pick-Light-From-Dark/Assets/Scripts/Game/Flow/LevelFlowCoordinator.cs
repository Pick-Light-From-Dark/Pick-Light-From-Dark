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

        [Header("结局预制体")]
        public GameObject ending1Prefab;
        public GameObject deathEndingPrefab;

        [Header("章节开场图")]
        [Tooltip("每关开始前展示 night1~4 章节图")]
        public bool showChapterSplash = true;

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

            BeginLevelFlow();
        }

        void BeginLevelFlow()
        {
            if (showChapterSplash)
            {
                ChapterSplashController.ShowForChapter(levelId, OnChapterSplashFinished);
                return;
            }

            OnChapterSplashFinished();
        }

        void OnChapterSplashFinished()
        {
            // 继续游戏：跳过开场剧情，直接进游玩
            var cp = Game.Test.CrossLevelSaveSystem.Instance?.LoadCheckpoint();
            if (cp != null && cp.isInGameplay)
            {
                Debug.Log("[LevelFlowCoordinator] 检测到继续游戏存档，跳过开场剧情");
                Game.Test.CrossLevelSaveSystem.Instance.SaveStoryProgress(levelId, "");
                // 确保VN UI已隐藏
                if (vnController != null)
                {
                    if (vnController.vnCanvas != null) vnController.vnCanvas.gameObject.SetActive(false);
                    vnController.SetSkipButtonVisible(false);
                }
                StartGameplay();
                return;
            }

            if (openingStory == null)
                StartGameplay();
            else
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
            if (vnController.vnCanvas != null)
                vnController.vnCanvas.gameObject.SetActive(true);
            vnController.dialogueText = openingStory;
            vnController.OnDialogueExit = OnOpeningStoryExit;
            vnController.OnDialogueComplete = OnOpeningStoryEnd;
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
                    // 第一关"不吃"→结局一
                    if (levelId == 1 && ending1Prefab != null)
                    {
                        ShowEnding(ending1Prefab);
                    }
                    else
                    {
                        StartEndingStory();
                    }
                    break;
                case VNExitType.NextLevel:
                    // 接下一个预制体
                    LoadNextPrefab();
                    break;
                case VNExitType.Gameplay:
                    OnOpeningStoryEnd();
                    break;
                case VNExitType.None:
                default:
                    OnOpeningStoryEnd();
                    break;
            }
        }

        void OnOpeningStoryEnd()
        {
            if (vnController != null)
                vnController.MarkOpeningStoryComplete();

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
            // 清除上关卡牌使用记录，避免跨关污染结局判定
            Game.Test.CrossLevelSaveSystem.Instance?.ClearCardsUsedThisLevel();
            // 先初始化 GameFlowController，避免 GamePanel.Awake 用 TestLevelConfig 自初始化
            GameFlowController.Instance.Initialize(levelConfig);

            var teacherAI = FindFirstObjectByType<Game.AI.TeacherAI>();
            if (teacherAI == null)
            {
                var teacherObj = new GameObject("TeacherAI");
                teacherAI = teacherObj.AddComponent<Game.AI.TeacherAI>();
            }
            this.teacherAI = teacherAI;

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

            // 记录关卡结果（跨关卡结局判定用，使用本关卡牌追踪避免跨关污染）
            int lives = GameFlowController.Instance.GetCurrentLives();
            var save = Game.Test.CrossLevelSaveSystem.Instance;
            bool card2017Used = save?.HasUsedCardThisLevel(2017) ?? false;
            bool card2026Used = save?.HasUsedCardThisLevel(2026) ?? false;
            save?.RecordLevelResult(levelId, lives, card2017Used, card2026Used);
            Debug.Log($"[LevelFlowCoordinator] 记录关卡结果: Lv.{levelId}, Lives={lives}, 2017={card2017Used}, 2026={card2026Used}");

            // 如果有局内对话正在播放，等待对话结束后再处理 GameWin
            if (GamePanel.IsInteractionLocked)
            {
                Debug.Log("[LevelFlowCoordinator] 局内对话进行中，延迟 GameWin 处理");
                EventCenter.Instance.AddEventListener(E_EventType.GameDialogueEnd, OnDeferredGameWin);
                return;
            }

            ProcessGameWin();
        }

        void OnDeferredGameWin()
        {
            EventCenter.Instance.RemoveEventListener(E_EventType.GameDialogueEnd, OnDeferredGameWin);
            Debug.Log("[LevelFlowCoordinator] 局内对话结束，继续 GameWin 处理");
            ProcessGameWin();
        }

        void ProcessGameWin()
        {
            Debug.Log("[LevelFlowCoordinator] 游戏胜利，进入结尾剧情...");
            CleanupTeacherAI();
            UIMgr.Instance.HidePanel<GamePanel>(true);
            UnsubscribeGameEvents();

            // 检查是否有预判结局（由卡牌2038或调试面板触发）
            int preEval = Game.Test.CrossLevelSaveSystem.Instance?.PreEvaluatedEndingId ?? 0;
            if (preEval != 0)
            {
                Debug.Log($"[LevelFlowCoordinator] 预判结局: {preEval}，覆盖结尾剧情");

                // 根据结局ID选择对应的结尾剧情文件
                string dialogueFile = GetEndingDialogueFile(preEval);
                TextAsset endingAsset = Resources.Load<TextAsset>($"Dialogue/{dialogueFile}");
                if (endingAsset != null)
                {
                    endingStory = endingAsset;
                    Debug.Log($"[LevelFlowCoordinator] 结尾剧情覆盖为: {dialogueFile}");
                }
                else
                {
                    Debug.LogWarning($"[LevelFlowCoordinator] 未找到对话文件: Dialogue/{dialogueFile}");
                }
            }

            StartEndingStory();
        }

        string GetEndingDialogueFile(int endingId)
        {
            switch (endingId)
            {
                case 6002: return "Dialogue5-2a"; // 结局二 莫比乌斯环
                case 6003: return "Dialogue5-2c"; // 结局三 星垂之夜
                case 6004: return "Dialogue5-2d"; // 结局四 北极星
                default: return "";
            }
        }

        void ShowEndingByEvaluatedId(int endingId)
        {
            string prefabPath = endingId switch
            {
                6002 => "UI/Ending2_Mobius",
                6004 => "UI/Ending4_StarryNight",
                _ => ""
            };

            GameObject prefab = null;
            if (!string.IsNullOrEmpty(prefabPath))
                prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab != null)
            {
                ShowEnding(prefab);
            }
            else
            {
                Debug.LogWarning($"[LevelFlowCoordinator] 未找到结局 {endingId} 的预制体 ({prefabPath})，显示 Ending2Panel 兜底");
                // 兜底：动态创建 Ending2Panel
                var canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    var go = new GameObject($"Ending{endingId}_Panel");
                    go.transform.SetParent(canvas.transform, false);
                    go.AddComponent<Game.Test.Ending2Panel>();
                }
            }
        }

        void OnGameLose(string reason)
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log($"[LevelFlowCoordinator] 游戏失败: {reason}");
            CleanupTeacherAI();
            UIMgr.Instance.HidePanel<GamePanel>(true);
            UnsubscribeGameEvents();

            // 优先序列化引用，其次从Resources动态加载
            GameObject deadPrefab = deathEndingPrefab;
            if (deadPrefab == null)
                deadPrefab = Resources.Load<GameObject>("UI/Content/DeadEnd");
            if (deadPrefab != null)
            {
                ShowEnding(deadPrefab);
            }
            else
            {
                SceneMgr.Instance.LoadScene("GameScene");
            }
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
            if (vnController.vnCanvas != null)
                vnController.vnCanvas.gameObject.SetActive(true);
            vnController.SetSkipButtonVisible(true);
            vnController.RestartDialogue(forceFromStart: true);
        }

        /// <summary>结尾剧情分支结束回调（带分支类型）</summary>
        void OnEndingStoryExit(VNExitType exitType)
        {
            Debug.Log($"[LevelFlowCoordinator] 结尾剧情分支结束: {exitType}");

            switch (exitType)
            {
                case VNExitType.NextLevel:
                    TryAdvanceToNextLevel();
                    break;
                case VNExitType.Ending:
                    // VN 已显示结局画面，不做额外处理
                    Debug.Log("[LevelFlowCoordinator] VN 已处理结局显示");
                    break;
                case VNExitType.Gameplay:
                case VNExitType.None:
                default:
                    OnEndingStoryEnd();
                    break;
            }
        }

        void OnEndingStoryEnd()
        {
            // 中间关通关：播完 post 剧情后进入下一关场景（与第一关「吃」线相同，不靠 EndingContentPanel）
            if (!string.IsNullOrEmpty(nextLevelSceneName))
            {
                TryAdvanceToNextLevel();
                return;
            }

            // 最后一关或无下一关配置：结局面板 / 回主菜单
            ShowVictoryPanel();
        }

        /// <summary>通关后进入下一关场景（优先）或实例化下一关预制体</summary>
        void TryAdvanceToNextLevel()
        {
            if (!string.IsNullOrEmpty(nextLevelSceneName))
            {
                AdvanceToNextLevelScene();
                return;
            }

            LoadNextPrefab();
        }

        void AdvanceToNextLevelScene()
        {
            Debug.Log($"[LevelFlowCoordinator] 进入下一关场景: {nextLevelSceneName}");
            UIMgr.Instance.HideAllPanels();
            if (vnController != null && vnController.vnCanvas != null)
                vnController.vnCanvas.gameObject.SetActive(false);
            SceneMgr.Instance.LoadScene(nextLevelSceneName);
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

        void ShowEnding(GameObject prefab)
        {
            if (prefab == null) return;
            var canvas = FindFirstObjectByType<Canvas>();
            Transform parent = null;
            if (canvas != null)
            {
                // 挂到UIMgr的System层，确保在最上层渲染
                var sysLayer = canvas.transform.Find("System");
                parent = sysLayer != null ? sysLayer : canvas.transform;
            }
            Instantiate(prefab, parent, false);
            Debug.Log($"[LevelFlowCoordinator] 显示结局: {prefab.name}");
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
            EventCenter.Instance.RemoveEventListener(E_EventType.GameDialogueEnd, OnDeferredGameWin);
            if (Instance == this) Instance = null;
        }
    }
}