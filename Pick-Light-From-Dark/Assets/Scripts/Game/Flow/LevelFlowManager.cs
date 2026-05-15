using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Game.Config;
using Game.Test;

namespace Game.Flow
{
    public class LevelFlowManager : SingletonAutoMono<LevelFlowManager>
    {
        private enum FlowState { Idle, ChapterIntro, PreDialogue, Gameplay, PostDialogue, NextLevelTransition }

        [SerializeField] private string[] levelSequence = {
            "Config/LevelConfig_1",
            "Config/LevelConfig_2",
            "Config/LevelConfig_3",
            "Config/LevelConfig_5"
        };

        private FlowState currentState = FlowState.Idle;
        private int currentLevelIndex;
        private LevelConfigSO currentLevelConfig;
        private Game.AI.TeacherAI teacherAI;
        private FungusVNController vnController;
        private GameObject chapterOverlay;

        void Start()
        {
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        void OnDestroy()
        {
            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
                EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameLose);
            }
        }

        public void StartGame()
        {
            currentLevelIndex = 0;
            StartLevel(0);
        }

        private void StartLevel(int index)
        {
            if (index >= levelSequence.Length)
            {
                AllLevelsComplete();
                return;
            }

            currentLevelConfig = Resources.Load<LevelConfigSO>(levelSequence[index]);
            if (currentLevelConfig == null)
            {
                Debug.LogError($"[LevelFlow] 找不到关卡配置: {levelSequence[index]}");
                BackToMenu();
                return;
            }

            currentLevelIndex = index;
            Debug.Log($"[LevelFlow] 开始关卡 {index + 1}: {currentLevelConfig.levelName}");
            StartCoroutine(ChapterIntroThenDialogue());
        }

        // ==================== 章节图 ====================

        private IEnumerator ChapterIntroThenDialogue()
        {
            currentState = FlowState.ChapterIntro;

            string chapterName = $"Chapters/night{currentLevelIndex + 1}";
            var chapterSprite = Resources.Load<Sprite>(chapterName);
            Debug.Log($"[LevelFlow] 章节图加载: {chapterName}, 结果={chapterSprite}");

            if (chapterSprite != null)
            {
                ShowChapterOverlay(chapterSprite);
                yield return new WaitForSecondsRealtime(5f);
                yield return StartCoroutine(FadeOutChapterOverlay());
                HideChapterOverlay();
            }
            else
            {
                Debug.LogWarning($"[LevelFlow] 章节图未找到: {chapterName}，跳过章节展示");
            }

            // 进入前置剧情或直接游玩
            if (!string.IsNullOrEmpty(currentLevelConfig.preDialogueFile))
                BeginPreDialogue();
            else
                BeginGameplay();
        }

        private void ShowChapterOverlay(Sprite sprite)
        {
            if (chapterOverlay == null)
            {
                chapterOverlay = new GameObject("ChapterOverlay");
                chapterOverlay.transform.SetParent(FindOrCreateRootCanvas(), false);
                var rt = chapterOverlay.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                chapterOverlay.AddComponent<CanvasRenderer>();
                var img = chapterOverlay.AddComponent<Image>();
                img.raycastTarget = true;
                var canvas = chapterOverlay.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = 100;
                chapterOverlay.AddComponent<GraphicRaycaster>();
            }

            chapterOverlay.GetComponent<Image>().sprite = sprite;
            chapterOverlay.GetComponent<Image>().color = Color.white;
            chapterOverlay.SetActive(true);
        }

        private IEnumerator FadeOutChapterOverlay()
        {
            var img = chapterOverlay.GetComponent<Image>();
            float duration = 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float a = 1f - elapsed / duration;
                img.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
        }

        private void HideChapterOverlay()
        {
            if (chapterOverlay != null)
                chapterOverlay.SetActive(false);
        }

        private Transform FindOrCreateRootCanvas()
        {
            var rootCanvas = FindObjectOfType<Canvas>();
            return rootCanvas != null ? rootCanvas.transform : transform;
        }

        // ==================== 前置剧情 ====================

        private void BeginPreDialogue()
        {
            currentState = FlowState.PreDialogue;
            string path = "Dialogue/" + currentLevelConfig.preDialogueFile;
            var txt = Resources.Load<TextAsset>(path);
            Debug.Log($"[LevelFlow] 加载前置对话: {path}, 结果={txt}");
            if (txt == null)
            {
                Debug.LogError($"[LevelFlow] 找不到前置对话: {path}");
                BeginGameplay();
                return;
            }

            EnsureVNController();
            vnController.dialogueText = txt;
            vnController.OnDialogueExit = OnPreDialogueExit;
            vnController.OnDialogueComplete = null;
            vnController.ClearPlaceholder();
            vnController.SetSkipButtonVisible(true);
            vnController.RestartDialogue();
        }

        private void OnPreDialogueExit(VNExitType exitType)
        {
            Debug.Log($"[LevelFlow] 前置剧情结束，分支: {exitType}");

            if (exitType == VNExitType.Ending)
            {
                // 选择直接进入结局
                BeginEndingDialogue();
                return;
            }

            if (exitType == VNExitType.NextLevel)
            {
                AdvanceToNextLevel();
                return;
            }

            BeginGameplay();
        }

        // ==================== 游玩 ====================

        private void BeginGameplay()
        {
            currentState = FlowState.Gameplay;
            Time.timeScale = 1f;

            GameFlowController.Instance.Initialize(currentLevelConfig);

            var teacherObj = new GameObject("TeacherAI");
            teacherAI = teacherObj.AddComponent<Game.AI.TeacherAI>();

            UIMgr.Instance.ShowPanel<GamePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    panel.InitializeWithConfig(currentLevelConfig);
                    teacherAI.Initialize(currentLevelConfig);
                }
            );
        }

        // ==================== 胜利 / 失败 ====================

        private void OnGameWin()
        {
            if (currentState != FlowState.Gameplay) return;

            Debug.Log($"[LevelFlow] 关卡 {currentLevelIndex + 1} 胜利");
            CleanupTeacherAI();
            UIMgr.Instance.HidePanel<GamePanel>();

            if (!string.IsNullOrEmpty(currentLevelConfig.postDialogueFile))
                BeginPostDialogue();
            else
                AdvanceToNextLevel();
        }

        private void OnGameLose(string reason)
        {
            if (currentState != FlowState.Gameplay) return;

            Debug.Log($"[LevelFlow] 关卡 {currentLevelIndex + 1} 失败: {reason}");
            CleanupTeacherAI();
            UIMgr.Instance.HidePanel<GamePanel>();
            UIMgr.Instance.ShowPanel<TipPanel>();
        }

        // ==================== 后置剧情 ====================

        private void BeginPostDialogue()
        {
            currentState = FlowState.PostDialogue;
            var txt = Resources.Load<TextAsset>("Dialogue/" + currentLevelConfig.postDialogueFile);
            if (txt == null)
            {
                Debug.LogError($"[LevelFlow] 找不到后置对话: {currentLevelConfig.postDialogueFile}");
                AdvanceToNextLevel();
                return;
            }

            EnsureVNController();
            vnController.dialogueText = txt;
            vnController.OnDialogueExit = OnPostDialogueExit;
            vnController.OnDialogueComplete = null;
            vnController.ClearPlaceholder();
            vnController.SetSkipButtonVisible(true);
            vnController.RestartDialogue();
        }

        private void OnPostDialogueExit(VNExitType exitType)
        {
            Debug.Log($"[LevelFlow] 后置剧情结束，分支: {exitType}");
            AdvanceToNextLevel();
        }

        // ==================== 结局对话（选择关选项2） ====================

        private void BeginEndingDialogue()
        {
            if (string.IsNullOrEmpty(currentLevelConfig.choice2EndingDialogueFile))
            {
                BackToMenu();
                return;
            }

            var txt = Resources.Load<TextAsset>("Dialogue/" + currentLevelConfig.choice2EndingDialogueFile);
            if (txt == null)
            {
                BackToMenu();
                return;
            }

            EnsureVNController();
            vnController.dialogueText = txt;
            vnController.OnDialogueExit = (VNExitType _) => BackToMenu();
            vnController.OnDialogueComplete = null;
            vnController.ClearPlaceholder();
            vnController.SetSkipButtonVisible(true);
            vnController.RestartDialogue();
        }

        // ==================== 关卡跳转 ====================

        public void AdvanceToNextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= levelSequence.Length)
            {
                Debug.Log("[LevelFlow] 所有关卡完成");
                AllLevelsComplete();
            }
            else
            {
                StartLevel(currentLevelIndex);
            }
        }

        private void AllLevelsComplete()
        {
            currentState = FlowState.Idle;
            UIMgr.Instance.ShowPanel<EndingContentPanel>();
        }

        public void BackToMenu()
        {
            currentState = FlowState.Idle;
            UIMgr.Instance.HideAllPanels();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        }

        // ==================== 工具 ====================

        private void CleanupTeacherAI()
        {
            if (teacherAI != null)
            {
                Destroy(teacherAI.gameObject);
                teacherAI = null;
            }
        }

        private void EnsureVNController()
        {
            if (vnController == null)
                vnController = FindObjectOfType<FungusVNController>();

            if (vnController == null)
            {
                var go = new GameObject("FungusVNController");
                vnController = go.AddComponent<FungusVNController>();
                Debug.Log("[LevelFlow] 动态创建 FungusVNController");
            }
        }

        public int CurrentLevelIndex => currentLevelIndex;
        public bool IsRunning => currentState != FlowState.Idle;
    }
}