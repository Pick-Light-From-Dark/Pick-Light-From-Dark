using System.Collections;
using Game.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// Level2_end 专用：只播胜利剧情 Dialogue2-2，播完进 Level3。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Level2VictoryStoryRunner : MonoBehaviour
    {
        const string LogTag = "[Level2VictoryStory]";

        [Header("剧情")]
        public TextAsset story;

        [Header("VN")]
        public FungusVNController vnController;

        [Header("下一关")]
        public string nextSceneName = "Level3";

        [Header("章节图（本关）")]
        public bool showChapterSplash = false;
        public int chapterLevelId = 2;

        void Awake()
        {
            DisableStrayLevelFlowCoordinators();

            if (vnController == null)
                vnController = FindFirstObjectByType<FungusVNController>();

            if (story == null)
                story = Resources.Load<TextAsset>("Dialogue/Dialogue2-2");

            if (vnController != null && story != null)
            {
                vnController.dialogueText = story;
                EnsureVNCanvasVisible(vnController);
            }
        }

        void Start()
        {
            if (vnController == null || story == null)
            {
                Debug.LogError($"{LogTag} VN 或剧情文本缺失");
                return;
            }

            if (showChapterSplash)
                ChapterSplashController.ShowForChapter(chapterLevelId, BeginStory);
            else
                BeginStory();
        }

        void BeginStory()
        {
            Debug.Log($"{LogTag} 开始播放胜利剧情: {story.name}");

            vnController.OnDialogueExit = OnStoryExit;
            vnController.OnDialogueComplete = null;
            vnController.dialogueText = story;
            vnController.ClearPlaceholder();
            vnController.SetSkipButtonVisible(true);
            EnsureVNCanvasVisible(vnController);
            vnController.RestartDialogue(forceFromStart: true);
        }

        void OnStoryExit(VNExitType exitType)
        {
            Debug.Log($"{LogTag} 剧情结束 ({exitType})");
            StartCoroutine(TransitionToNextLevel());
        }

        IEnumerator TransitionToNextLevel()
        {
            // 遮住 EndDialogue 关 VN 后的空白帧，以及切场景瞬间（Loading 在 Level3 Awake 关闭）
            LoadingScreenController.ShowImmediate();
            yield return null;

            if (UIMgr.Instance != null)
                UIMgr.Instance.HideAllPanels();

            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);

            // Hide 不能在协程里做：LoadScene 会销毁本物体，改由 Level3 的 LevelEntrySplashOverride.Awake 关掉
        }

        static void DisableStrayLevelFlowCoordinators()
        {
            foreach (var c in FindObjectsByType<LevelFlowCoordinator>(FindObjectsSortMode.None))
            {
                if (c == null || !c.enabled) continue;
                Debug.LogWarning($"{LogTag} 已禁用 {c.gameObject.name}");
                c.enabled = false;
            }
        }

        static void EnsureVNCanvasVisible(FungusVNController vn)
        {
            if (vn.vnCanvas == null) return;
            vn.vnCanvas.gameObject.SetActive(true);
            var t = vn.vnCanvas.transform;
            if (t.localScale.sqrMagnitude < 0.01f)
                t.localScale = Vector3.one;
        }
    }
}
