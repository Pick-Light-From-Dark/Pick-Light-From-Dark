using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Game.Data;
using Game.Config;
using Game.Card;
using Game.UI;
using Game.Emotion;

public class GamePanel : BasePanel
{
    [Header("备选区容器")]
    [SerializeField] private Transform cardSpawnPos1;
    [SerializeField] private Transform cardSpawnPos2;
    [SerializeField] private Transform cardSpawnPos3;
    [SerializeField] private Transform cardSpawnPos4;
    [SerializeField] private Transform cardSpawnPos5;
    [SerializeField] private Transform cardSpawnPos6;
    [SerializeField] private Transform cardSpawnPos7;
    [SerializeField] private Transform cardSpawnPos8;

    [Header("思考框（卡牌拖放目标）")]
    [SerializeField] private GameObject cardDropZoneObj;

    [Header("读条UI — 片段色条 + 黑色遮罩 + 时间文本 + 打断按钮")]
    [SerializeField] private Image cardLoadingBarBg;
    [SerializeField] private Image cardLoadingBarFill;
    [SerializeField] private RectTransform segmentBarRt;
    [SerializeField] private TextMeshProUGUI LoadingCount;
    [SerializeField] private Button ClearBtn;

    [Header("暂停按钮")]
    [SerializeField] private Button StopBtn;

    [Header("思考框 — 卡牌描述显示")]
    [SerializeField] private GameObject thinkingImgBk;
    [SerializeField] private TextMeshProUGUI thinkingText;

    [Header("闭眼系统")]
    [SerializeField] private Button closeEyesBtn;
    [SerializeField] private Image eyeCloseOverlay;
    private Image closeEyesBtnImage;
    private Sprite openEyeSprite;
    private Sprite closeEyeSprite;
    [SerializeField] private GameObject gameplayUIRoot;

    [Header("卡牌信息弹窗")]
    [SerializeField] private GameObject cardInfoPopupPrefab;
    [SerializeField] private GameObject cardSelectionArea;

    [Header("场景背景")]
    [SerializeField] private Image sceneBackgroundImage;

    [Header("情绪进度条")]
    [SerializeField] private Image panicBarFill;
    [SerializeField] private Image exciteBarFill;

    [Header("老师巡查状态")]
    [SerializeField] private TextMeshProUGUI teacherStatusText;

    [Header("老师画面")]
    [SerializeField] private GameObject teacherImg;

    [Header("生命值图片（按顺序 HpImg_0, HpImg_1...）")]
    [SerializeField] private GameObject[] hpImages;

    [Header("被抓UI — 剩余生命时显示")]
    [SerializeField] private GameObject caughtOverlay;

    [Header("失败UI — 生命耗尽时显示")]
    [SerializeField] private GameObject failOverlay;

    [Header("手电筒动画 — Bg5005")]
    [SerializeField] private GameObject flashlightOverlay;

    public static GamePanel Instance { get; private set; }

    private CardGrid cardGrid;
    private CardDropZone cardDropZone;
    private CardManager cardManager;
    private CardInfoPopup activePopup;
    private GameDialogueManager dialogueManager;
    private int preDialogueBackgroundId;

    // 情绪值显示
    private TextMeshProUGUI chaosCountText;
    private TextMeshProUGUI happlyCountText;
    private const int EMOTION_DISPLAY_MIN = 0;
    private const int EMOTION_DISPLAY_MAX = 50;

    // 剩余时间显示
    private TextMeshProUGUI timeText;

    // 任务显示
    private TextMeshProUGUI firstEventText;
    private TextMeshProUGUI secondEventText;
    private TextMeshProUGUI thirdEventText;

    /// <summary>读条前的备选区卡牌快照（打断时恢复用）</summary>
    private List<CardData> preReadSnapshot;

    /// <summary>当前正在读条的卡牌（放在思考框中显示）</summary>
    private CardSlot readingCardSlot;

    /// <summary>弹窗打开前的 timescale 缓存</summary>
    private float prePopupTimeScale = 1f;
    private float preDialogueTimeScale = 1f;

    // ==================== 自管读条状态 ====================

    private bool isReading;
    private float readTime;
    private float totalReadTime;
    private int currentSegmentIndex;
    private CardData readingCardData;

    /// <summary>供PlayerState等外部系统查询当前是否正在读条</summary>
    public static bool IsCardReading { get; private set; }

    /// <summary>卡牌交互是否被锁定（读条中或对话中）</summary>
    public static bool IsInteractionLocked =>
        IsCardReading || (Instance != null && Instance.dialogueManager != null && Instance.dialogueManager.IsDialogueActive);

    /// <summary>当前读条片段是否不可打断（供TeacherAI查询）</summary>
    public static bool IsInUninterruptibleSegment { get; private set; }

    private Sprite loadingFillSprite;

    /// <summary>当前读条卡牌的音效源，打断时需停止</summary>
    private AudioSource cardSfxSource;

    // ==================== 闭眼状态 ====================


    [Header("关卡配置（留空则使用 TestLevelConfig）")]
    [SerializeField] private Game.Config.LevelConfigSO overrideLevelConfig;

    // 防止同一帧内对 GameFlowController 重复初始化（Awake + InitializeWithConfig 双重调用）
    private static int lastGameFlowInitFrame = -1;

    public void InitializeWithConfig(Game.Config.LevelConfigSO config)
    {
        if (Time.frameCount == lastGameFlowInitFrame) return;
        lastGameFlowInitFrame = Time.frameCount;
        overrideLevelConfig = config;
        if (config != null)
            Game.Flow.GameFlowController.Instance.Initialize(config);

        // 初始化时立即设置初始背景（避免 ImgBk 显示默认占位图）
        SetSceneBackground(currentBackgroundId);
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;

        EnsureCanvasRaycaster();
        EnsureEventSystem();
        EnsureCardGrid();
        EnsureDropZone();

        // 自动查找未在Inspector中指定的UI元素
        if (teacherImg == null)
            teacherImg = transform.Find("ImgBk/TeacherImg")?.gameObject;
        if (hpImages == null || hpImages.Length == 0)
        {
            var hpRoot = transform.Find("ImgBk");
            if (hpRoot != null)
            {
                var hps = new System.Collections.Generic.List<GameObject>();
                foreach (Transform child in hpRoot)
                {
                    if (child.name.StartsWith("HpImg"))
                        hps.Add(child.gameObject);
                }
                hpImages = hps.ToArray();
            }
        }
        if (caughtOverlay == null)
            caughtOverlay = transform.Find("ImgBk/Bg5003")?.gameObject;
        if (failOverlay == null)
            failOverlay = transform.Find("ImgBk/Bg5004")?.gameObject;
        if (flashlightOverlay == null)
            flashlightOverlay = transform.Find("ImgBk/Bg5005")?.gameObject;

        // 初始隐藏
        if (teacherImg != null) teacherImg.SetActive(false);
        if (caughtOverlay != null) caughtOverlay.SetActive(false);
        if (failOverlay != null) failOverlay.SetActive(false);
        if (flashlightOverlay != null) flashlightOverlay.SetActive(false);

        cardManager = CardManager.Instance;

        // 先订阅事件，确保 CardManager.Initialize 触发 OnSelectionChanged 时
        // RefreshSelectionArea 已在监听列表中，不会错过首次刷新
        CardManager.OnSelectionChanged += RefreshSelectionArea;

        if (!Game.Flow.GameFlowController.Instance.IsInitialized)
        {
            var config = overrideLevelConfig ?? Resources.Load<Game.Config.LevelConfigSO>("TestData/TestLevelConfig");
            if (config != null && Time.frameCount != lastGameFlowInitFrame)
            {
                lastGameFlowInitFrame = Time.frameCount;
                Game.Flow.GameFlowController.Instance.Initialize(config);
            }
        }

        // 初始化生命值显示
        UpdateHpDisplay(Game.Flow.GameFlowController.Instance.GetCurrentLives());

        // 提前触发 PlayerState 单例初始化，确保其 Update() 开始运行以监听 C 键
        var ps = PlayerState.Instance;

        CardDropZone.OnCardDropped += OnCardDropped;
        CardSlot.OnCardClicked += OnCardClicked;
        CardSlot.OnCardDragStarted += OnCardDragStarted;
        CardSlot.OnCardDragEnded += OnCardDragEnded;

        if (cardLoadingBarBg != null) cardLoadingBarBg.transform.parent.gameObject.SetActive(false);
        if (LoadingCount != null) LoadingCount.gameObject.SetActive(false);
        if (ClearBtn != null)
        {
            ClearBtn.gameObject.SetActive(false);
            ClearBtn.onClick.AddListener(InterruptReading);
        }

        // 思考框自动查找 + 初始隐藏
        if (thinkingImgBk == null)
            thinkingImgBk = transform.Find("ImgBk/ThinkingImgBk")?.gameObject;
        if (thinkingImgBk != null) thinkingImgBk.SetActive(false);
        if (thinkingText == null && thinkingImgBk != null)
            thinkingText = thinkingImgBk.transform.Find("ThinkingText")?.GetComponent<TextMeshProUGUI>();

        if (StopBtn != null)
            StopBtn.onClick.AddListener(OnStopBtnClicked);

        // 加载条填充用白色精灵
        loadingFillSprite = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, 4, 4), Vector2.zero);

        // 情绪值显示
        SetupEmotionDisplay();

        // 剩余时间显示
        timeText = transform.Find("ImgBk/StageMessage/Time")?.GetComponent<TextMeshProUGUI>();

        // 老师巡查状态
        SetupTeacherStatus();

        // 任务显示
        SetupTaskDisplay();

        // 闭眼按钮 + 遮罩初始化
        SetupEyeClose();

        // 局内剧情对话管理器
        SetupDialogue();
    }

    private void SetupEmotionDisplay()
    {
        chaosCountText = transform.Find("ImgBk/PlayerMessage/ChaosCount")?.GetComponent<TextMeshProUGUI>();
        happlyCountText = transform.Find("ImgBk/PlayerMessage/HapplyCount")?.GetComponent<TextMeshProUGUI>();

        EventCenter.Instance.AddEventListener<int>(E_EventType.PanicChanged, OnEmotionChanged);
        EventCenter.Instance.AddEventListener<int>(E_EventType.ExciteChanged, OnEmotionChanged);
        UpdateEmotionDisplay();
    }

    private void OnEmotionChanged(int _)
    {
        UpdateEmotionDisplay();
    }

    private void UpdateEmotionDisplay()
    {
        var emo = EmotionSystem.Instance;
        if (emo == null) return;
        var info = emo.GetEmotionInfo();
        if (chaosCountText != null) chaosCountText.text = $"{info.panicValue}/{EMOTION_DISPLAY_MAX}";
        if (happlyCountText != null) happlyCountText.text = $"{info.exciteValue}/{EMOTION_DISPLAY_MAX}";
        if (panicBarFill != null)
            panicBarFill.fillAmount = (info.panicValue - EMOTION_DISPLAY_MIN) / (float)(EMOTION_DISPLAY_MAX - EMOTION_DISPLAY_MIN);
        if (exciteBarFill != null)
            exciteBarFill.fillAmount = (info.exciteValue - EMOTION_DISPLAY_MIN) / (float)(EMOTION_DISPLAY_MAX - EMOTION_DISPLAY_MIN);
    }

    private void SetupTeacherStatus()
    {
        if (teacherStatusText == null)
        {
            var statusGo = transform.Find("ImgBk/TeacherStatus");
            if (statusGo != null)
                teacherStatusText = statusGo.GetComponent<TextMeshProUGUI>();
        }
        if (teacherStatusText != null)
            teacherStatusText.text = "";
    }

    private void UpdateTeacherStatusDisplay()
    {
        if (teacherStatusText == null) return;

        var teacher = Game.AI.TeacherAI.Instance;
        if (teacher == null) return;

        var state = teacher.GetCurrentState();
        switch (state)
        {
            case Game.Config.TeacherState.Idle:
                teacherStatusText.text = "安静...";
                teacherStatusText.color = new Color32(180, 180, 180, 255);
                break;
            case Game.Config.TeacherState.Approaching:
                float progress = teacher.GetApproachProgress();
                int filled = Mathf.RoundToInt(progress * 10);
                string bar = new string('=', filled) + new string(' ', 10 - filled);
                int pct = Mathf.RoundToInt(progress * 100);
                teacherStatusText.text = $"脚步声 [{bar}] {pct}%";
                teacherStatusText.color = new Color32(255, 200, 0, 255);
                break;
            case Game.Config.TeacherState.Inspecting:
                if (teacher.GetCurrentInspectType() == Game.Config.InspectType.Flash)
                    teacherStatusText.text = "手电筒检查——闭眼！";
                else
                    teacherStatusText.text = "查寝中——闭眼！";
                teacherStatusText.color = new Color32(255, 60, 60, 255);
                break;
            case Game.Config.TeacherState.Leaving:
                teacherStatusText.text = "离开中...";
                teacherStatusText.color = new Color32(100, 220, 100, 255);
                break;
        }
    }

    private void SetupTaskDisplay()
    {
        firstEventText = transform.Find("ImgBk/EventImgBk/FirstEvent")?.GetComponent<TextMeshProUGUI>();
        secondEventText = transform.Find("ImgBk/EventImgBk/SecondEvent")?.GetComponent<TextMeshProUGUI>();
        thirdEventText = transform.Find("ImgBk/EventImgBk/ThirdEvent")?.GetComponent<TextMeshProUGUI>();

        EventCenter.Instance.AddEventListener<int>(E_EventType.TaskProgressChanged, OnTaskProgressChanged);
        EventCenter.Instance.AddEventListener<int>(E_EventType.TaskGoalCompleted, OnTaskGoalCompleted);

        RefreshTaskDisplay();
    }

    private void OnTaskProgressChanged(int cardId) { RefreshTaskDisplay(); }
    private void OnTaskGoalCompleted(int cardId) { RefreshTaskDisplay(); }

    private void RefreshTaskDisplay()
    {
        var eventTexts = new[] { firstEventText, secondEventText, thirdEventText };
        var goals = Game.Task.TaskManager.Instance.GetActiveGoals();
        // 过滤掉隐藏（Pending）任务
        var visibleGoals = goals?.Where(g => g.state != TaskState.Pending).ToList();

        for (int i = 0; i < eventTexts.Length; i++)
        {
            var tmp = eventTexts[i];
            if (tmp == null) continue;

            if (visibleGoals != null && i < visibleGoals.Count)
            {
                var goal = visibleGoals[i];
                string stateIcon = goal.state == TaskState.Completed ? "<color=#64DC64>✓ </color>" : "";
                tmp.text = $"{stateIcon}{goal.taskName}  {goal.currentCount}/{goal.targetCount}";
                tmp.gameObject.SetActive(true);
            }
            else
            {
                tmp.gameObject.SetActive(false);
            }
        }
    }

    private void SetupEyeClose()
    {
        if (closeEyesBtn == null)
        {
            var btnGo = transform.Find("ImgBk/CloseEyesBtn");
            if (btnGo != null) closeEyesBtn = btnGo.GetComponent<Button>();
        }
        if (closeEyesBtn != null)
        {
            closeEyesBtn.onClick.AddListener(OnCloseEyesClicked);
            closeEyesBtnImage = closeEyesBtn.GetComponent<Image>();
            openEyeSprite = Resources.Load<Sprite>("UI/Icon/open_eye");
            closeEyeSprite = Resources.Load<Sprite>("UI/Icon/close_eye");
        }

        if (eyeCloseOverlay == null)
        {
            var imgBk = transform.Find("ImgBk");
            if (imgBk != null)
            {
                var existing = imgBk.Find("EyeCloseOverlay");
                if (existing != null)
                    eyeCloseOverlay = existing.GetComponent<Image>();
                else
                {
                    var go = new GameObject("EyeCloseOverlay", typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(imgBk, false);
                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    rt.SetAsLastSibling();
                    eyeCloseOverlay = go.GetComponent<Image>();
                    eyeCloseOverlay.color = Color.black;
                    eyeCloseOverlay.raycastTarget = false;
                }
            }
        }
        if (eyeCloseOverlay != null) eyeCloseOverlay.gameObject.SetActive(false);

        if (gameplayUIRoot == null)
            gameplayUIRoot = transform.Find("ImgBk")?.gameObject;

        EventCenter.Instance.AddEventListener<bool>(E_EventType.PlayerEyeCloseChanged, OnPlayerEyeCloseChanged);
        EventCenter.Instance.AddEventListener<int>(E_EventType.BackgroundJump, OnBackgroundJump);
    }

    private void SetupDialogue()
    {
        dialogueManager = gameObject.AddComponent<GameDialogueManager>();
        var thinkRt = thinkingImgBk != null ? thinkingImgBk.GetComponent<RectTransform>() : null;
        dialogueManager.Initialize(thinkRt);

        EventCenter.Instance.AddEventListener<string>(E_EventType.GameDialogueStart, OnGameDialogueStart);
        EventCenter.Instance.AddEventListener(E_EventType.GameDialogueEnd, OnGameDialogueEnd);
    }

    /// <summary>对话背景画面覆盖值（0=使用默认5004）</summary>
    public static int DialogueBackgroundOverride = 0;
    /// <summary>对话结束后背景画面覆盖值（0=恢复对话前画面）</summary>
    public static int PostDialogueBackgroundOverride = 0;

    private void OnGameDialogueStart(string resourcePath)
    {
        // 保存当前背景画面ID，切换至对话背景（可被覆盖）
        preDialogueBackgroundId = GetCurrentBackgroundId();
        int dialogueBg = DialogueBackgroundOverride != 0 ? DialogueBackgroundOverride : 5004;
        DialogueBackgroundOverride = 0;
        SetSceneBackground(dialogueBg);

        // 暂停游戏：时间停止、老师AI暂停巡逻
        preDialogueTimeScale = Time.timeScale;
        Game.Flow.GameFlowController.Instance.PauseGame();

        dialogueManager.StartDialogue(resourcePath);
        Debug.Log("[GamePanel] 局内对话开始，游戏暂停");
    }

    private void OnGameDialogueEnd()
    {
        // 恢复背景画面（可被覆盖为指定背景）
        int restoreBg = PostDialogueBackgroundOverride != 0 ? PostDialogueBackgroundOverride : preDialogueBackgroundId;
        PostDialogueBackgroundOverride = 0;
        if (restoreBg != 0)
            SetSceneBackground(restoreBg);

        // 恢复游戏
        if (!Game.Flow.GameFlowController.Instance.IsGameOver())
        {
            Game.Flow.GameFlowController.Instance.ResumeGame();
            Time.timeScale = preDialogueTimeScale;
        }

        Debug.Log("[GamePanel] 局内对话结束，游戏恢复");
    }

    private int currentBackgroundId = 5001;
    private int GetCurrentBackgroundId() => currentBackgroundId;
    private Coroutine backgroundFadeRoutine;

    private void SetSceneBackground(int bgId, float fadeDuration = 0f)
    {
        if (sceneBackgroundImage == null)
        {
            var imgBk = transform.Find("ImgBk");
            if (imgBk != null)
                sceneBackgroundImage = imgBk.GetComponent<Image>();
        }

        if (sceneBackgroundImage == null) return;

        var sprite = Resources.Load<Sprite>($"UI/Background/Bg_{bgId}");
        if (sprite == null)
        {
            Debug.LogWarning($"[GamePanel] 背景画面资源未找到: UI/Background/Bg_{bgId}");
            return;
        }

        // 离开走廊(5010)时恢复老师巡逻
        if (currentBackgroundId == 5010 && bgId != 5010 && Game.AI.TeacherAI.IsPatrolPaused)
        {
            Game.AI.TeacherAI.IsPatrolPaused = false;
            Debug.Log("[GamePanel] 离开走廊，恢复老师巡逻");
        }

        if (fadeDuration > 0f && currentBackgroundId != bgId)
        {
            if (backgroundFadeRoutine != null) StopCoroutine(backgroundFadeRoutine);
            backgroundFadeRoutine = StartCoroutine(FadeBackground(bgId, sprite, fadeDuration));
        }
        else
        {
            currentBackgroundId = bgId;
            sceneBackgroundImage.sprite = sprite;
            Debug.Log($"[GamePanel] 背景画面设置: {bgId}");
        }
    }

    private IEnumerator FadeBackground(int bgId, Sprite newSprite, float duration)
    {
        // 交叉淡入：在旧图上方创建临时 Image 叠加新图，避免暴露相机底色
        var overlay = new GameObject("BgTransition");
        overlay.transform.SetParent(sceneBackgroundImage.transform.parent, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = sceneBackgroundImage.rectTransform.anchorMin;
        overlayRect.anchorMax = sceneBackgroundImage.rectTransform.anchorMax;
        overlayRect.offsetMin = sceneBackgroundImage.rectTransform.offsetMin;
        overlayRect.offsetMax = sceneBackgroundImage.rectTransform.offsetMax;
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.sprite = newSprite;
        overlayImg.color = new Color(1f, 1f, 1f, 0f);
        // 放在 sceneBackgroundImage 后面（渲染在其上方）
        overlay.transform.SetAsLastSibling();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            overlayImg.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }

        // 完成：将新图应用到主 Image，销毁临时对象
        currentBackgroundId = bgId;
        sceneBackgroundImage.sprite = newSprite;
        sceneBackgroundImage.color = Color.white;
        Destroy(overlay);
        Debug.Log($"[GamePanel] 背景画面设置: {bgId}");

        backgroundFadeRoutine = null;
    }

    private void OnStopBtnClicked()
    {
        Game.Flow.GameFlowController.Instance.PauseGame();
        UIMgr.Instance.ShowPanel<StopGamePanel>();
    }

    private void OnBackgroundJump(int bgId)
    {
        SetSceneBackground(bgId);
    }

    void Update()
    {
        if (isReading)
        {
            readTime += Time.unscaledDeltaTime;
            float progress = totalReadTime > 0f ? Mathf.Clamp01(readTime / totalReadTime) : 0f;

            // 黑色加载条从左向右逐步覆盖分段色条
            if (cardLoadingBarFill != null)
                cardLoadingBarFill.fillAmount = progress;

            if (LoadingCount != null)
                LoadingCount.text = $"{readTime:F1}s / {totalReadTime:F1}s";

            // 根据 readTime 判定当前片段，切换打断按钮显隐
            UpdateSegmentAndClearBtn();

            if (readTime >= totalReadTime)
                OnReadingComplete();
        }

        UpdateTeacherStatusDisplay();
        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        if (timeText == null) return;
        var flow = Game.Flow.GameFlowController.Instance;
        if (!flow.IsInitialized) return;
        float remaining = flow.GetRemainingTime();
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timeText.text = $"{minutes}:{seconds:D2}";
    }

    /// <summary>根据已读时间定位当前片段，控制 ClearBtn 显隐</summary>
    private void UpdateSegmentAndClearBtn()
    {
        if (readingCardData == null || readingCardData.segments == null) return;

        float accumulated = 0f;
        int newIndex = 0;
        for (int i = 0; i < readingCardData.segments.Count; i++)
        {
            accumulated += readingCardData.segments[i].duration;
            if (readTime < accumulated)
            {
                newIndex = i;
                break;
            }
            newIndex = i;
        }

        if (newIndex != currentSegmentIndex)
        {
            currentSegmentIndex = newIndex;
            bool canInterrupt = readingCardData.segments[currentSegmentIndex].isInterruptible;
            IsInUninterruptibleSegment = !canInterrupt;
            if (ClearBtn != null)
                ClearBtn.gameObject.SetActive(canInterrupt);
        }
    }

    void OnDestroy()
    {
        CardSlot.IsPopupActive = false;
        CardDropZone.OnCardDropped -= OnCardDropped;
        CardSlot.OnCardClicked -= OnCardClicked;
        CardSlot.OnCardDragStarted -= OnCardDragStarted;
        CardSlot.OnCardDragEnded -= OnCardDragEnded;
        CardManager.OnSelectionChanged -= RefreshSelectionArea;
        EventCenter.Instance.RemoveEventListener<bool>(E_EventType.PlayerEyeCloseChanged, OnPlayerEyeCloseChanged);
        EventCenter.Instance.RemoveEventListener<int>(E_EventType.BackgroundJump, OnBackgroundJump);
        EventCenter.Instance.RemoveEventListener<int>(E_EventType.PanicChanged, OnEmotionChanged);
        EventCenter.Instance.RemoveEventListener<int>(E_EventType.ExciteChanged, OnEmotionChanged);
        EventCenter.Instance.RemoveEventListener<int>(E_EventType.TaskProgressChanged, OnTaskProgressChanged);
        EventCenter.Instance.RemoveEventListener<int>(E_EventType.TaskGoalCompleted, OnTaskGoalCompleted);
        if (Instance == this) Instance = null;
    }

    // ==================== 老师相关UI ====================

    public void ShowTeacherImage()
    {
        if (teacherImg != null) teacherImg.SetActive(true);
    }

    public void HideTeacherImage()
    {
        if (teacherImg != null) teacherImg.SetActive(false);
    }

    public void UpdateHpDisplay(int currentLives)
    {
        if (hpImages == null) return;
        for (int i = 0; i < hpImages.Length; i++)
        {
            if (hpImages[i] != null)
                hpImages[i].SetActive(i < currentLives);
        }
    }

    public void ShowCaughtOverlay()
    {
        // 只显示Bg5003，隐藏其他所有子物体
        if (gameplayUIRoot != null)
        {
            foreach (Transform child in gameplayUIRoot.transform)
            {
                if (caughtOverlay != null && child.gameObject == caughtOverlay) continue;
                child.gameObject.SetActive(false);
            }
        }
        if (caughtOverlay != null) caughtOverlay.SetActive(true);
    }

    public void HideCaughtOverlay()
    {
        if (caughtOverlay != null) caughtOverlay.SetActive(false);
    }

    public void ShowFailOverlay()
    {
        // 只显示Bg5004，隐藏其他所有子物体
        if (gameplayUIRoot != null)
        {
            foreach (Transform child in gameplayUIRoot.transform)
            {
                if (failOverlay != null && child.gameObject == failOverlay) continue;
                child.gameObject.SetActive(false);
            }
        }
        if (failOverlay != null) failOverlay.SetActive(true);
    }

    /// <summary>强制显示闭眼UI（遮罩+闭眼按钮），用于被抓后恢复闭眼状态</summary>
    public void ShowEyeClosedUI()
    {
        if (gameplayUIRoot == null) return;

        // 恢复游戏界面并记录状态，供后续睁眼时恢复
        preEyeCloseActiveState.Clear();
        foreach (Transform child in gameplayUIRoot.transform)
        {
            if (child.name == "EyeCloseOverlay") continue;
            if (child.name == "Bg5003") continue;
            if (child.name == "Bg5004") continue;
            if (child.name == "Bg5005") continue;
            if (child.name == "TeacherImg") continue;
            if (child.name.Contains("Dialogue")) continue;

            child.gameObject.SetActive(true);
            preEyeCloseActiveState[child.name] = true;
        }

        // 闭眼遮罩 + 闭眼按钮置顶
        if (eyeCloseOverlay != null) eyeCloseOverlay.gameObject.SetActive(true);
        if (closeEyesBtn != null)
        {
            closeEyesBtn.gameObject.SetActive(true);
            closeEyesBtn.transform.SetAsLastSibling();
        }

        // 刷新卡牌备选区
        RefreshSelectionArea();
    }

    public void ShowFlashlightOverlay(float duration)
    {
        if (flashlightOverlay != null)
        {
            flashlightOverlay.transform.SetAsLastSibling();
            flashlightOverlay.SetActive(true);
            StartCoroutine(HideFlashlightAfterDelay(duration));
        }
    }

    private System.Collections.IEnumerator HideFlashlightAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (flashlightOverlay != null)
            flashlightOverlay.SetActive(false);
    }

    public override void ShowMe()
    {
        RefreshSelectionArea();
    }

    public override void HideMe() { }

    // ==================== 读条生命周期 ====================

    /// <summary>拖入卡牌 → 开始读条</summary>
    private void StartReading(CardSlot card)
    {
        if (card.CardData == null) return;
        if (isReading) return;

        // 关闭卡牌信息弹窗（如果打开）
        if (activePopup != null && activePopup.IsShown)
        {
            activePopup.Hide();
            CardSlot.IsPopupActive = false;
            if (prePopupTimeScale > 0f)
                Game.Flow.GameFlowController.Instance.ResumeGame();
        }

        readingCardSlot = card;
        readingCardData = card.CardData;
        totalReadTime = card.CardData.CalculateTotalDuration();

        // 恢复打断时保存的读条进度（吃面包等卡牌特殊效果）
        if (readingCardData.saveProgressOnInterrupt
            && cardManager.PopSavedReadProgress(readingCardData.id, out float savedTime, out int savedSeg))
        {
            readTime = savedTime;
            currentSegmentIndex = savedSeg;
            Debug.Log($"[GamePanel] 恢复读条进度: {readingCardData.cardName} 时间={readTime:F2}s 片段={currentSegmentIndex}");
        }
        else
        {
            readTime = 0f;
            currentSegmentIndex = 0;
        }
        isReading = true;
        IsCardReading = true;
        int curSegClamped = Mathf.Clamp(currentSegmentIndex, 0, card.CardData.segments.Count - 1);
        IsInUninterruptibleSegment = card.CardData.segments.Count > 0 && !card.CardData.segments[curSegClamped].isInterruptible;

        // 将卡牌移入思考框
        var dropRt = cardDropZoneObj != null ? cardDropZoneObj.GetComponent<RectTransform>() : null;
        if (dropRt != null)
        {
            card.transform.SetParent(dropRt, false);
            card.transform.localPosition = Vector3.zero;
        }

        // 快照备选区 → 隐藏
        preReadSnapshot = new List<CardData>();
        foreach (var data in cardManager.GetAvailableCards())
            preReadSnapshot.Add(data);

        // 先把当前读条卡牌从Grid列表中解除追踪，避免被Clear()误销毁
        cardGrid.DetachCard(card);
        cardGrid.Clear();
        if (cardSelectionArea != null) cardSelectionArea.SetActive(false);

        // 显示读条UI容器，隐藏静态红/绿条，用动态分段条替代
        if (cardLoadingBarBg != null) cardLoadingBarBg.transform.parent.gameObject.SetActive(true);

        Transform loadingContainer = cardLoadingBarFill != null
            ? cardLoadingBarFill.transform.parent : null;
        if (loadingContainer != null)
        {
            segmentBarRt = loadingContainer as RectTransform;
            // 隐藏静态条，动态分段完全替代
            var staticRed = loadingContainer.Find("NotInterruptImg");
            var staticGreen = loadingContainer.Find("InterruptImg");
            if (staticRed != null) staticRed.gameObject.SetActive(false);
            if (staticGreen != null) staticGreen.gameObject.SetActive(false);
        }

        BuildReadingSegmentBar();
        float startProgress = totalReadTime > 0f ? readTime / totalReadTime : 0f;
        if (cardLoadingBarFill != null)
        {
            cardLoadingBarFill.sprite = loadingFillSprite;
            cardLoadingBarFill.type = Image.Type.Filled;
            cardLoadingBarFill.fillMethod = Image.FillMethod.Horizontal;
            cardLoadingBarFill.fillOrigin = 0;
            cardLoadingBarFill.fillAmount = startProgress;
            cardLoadingBarFill.color = Color.black;
        }
        if (LoadingCount != null)
        {
            LoadingCount.gameObject.SetActive(true);
            LoadingCount.text = $"{readTime:F1}s / {totalReadTime:F1}s";
        }

        // 卡牌已放入放置区，隐藏拖拽时显示的思考框
        HideThinkingText();

        // 当前片段是否可打断（恢复进度时可能不在第一段）
        bool currentInterruptible = readingCardData.segments != null
            && readingCardData.segments.Count > 0
            && readingCardData.segments[curSegClamped].isInterruptible;
        if (ClearBtn != null)
            ClearBtn.gameObject.SetActive(currentInterruptible);

        // 播放卡牌音效
        if (!string.IsNullOrEmpty(readingCardData.sfxName))
            MusicMgr.Instance.PlaySound(readingCardData.sfxName, false, (source) =>
            {
                cardSfxSource = source;
            });
        else
            cardSfxSource = null;

        Debug.Log($"[GamePanel] 开始读条: {card.CardData.cardName} 时长={totalReadTime:F1}s");
    }

    private void BuildReadingSegmentBar()
    {
        RectTransform rt = segmentBarRt;
        if (rt == null)
        {
            // 兜底：使用 cardLoadingBarFill 的父级或 cardLoadingBarBg
            if (cardLoadingBarFill != null)
                rt = cardLoadingBarFill.rectTransform.parent as RectTransform;
            if (rt == null && cardLoadingBarBg != null)
                rt = cardLoadingBarBg.rectTransform;
        }
        if (rt == null) return;

        // 清除旧片段
        for (int i = rt.childCount - 1; i >= 0; i--)
        {
            var c = rt.GetChild(i);
            if (c.name == "seg") Destroy(c.gameObject);
        }

        if (readingCardData?.segments == null || readingCardData.segments.Count == 0) return;

        float total = totalReadTime;
        if (total <= 0f) return;

        float barWidth = rt.rect.width;
        float cumulative = 0f;

        foreach (var seg in readingCardData.segments)
        {
            float segWidth = (seg.duration / total) * barWidth;
            if (segWidth < 2f) segWidth = 2f;

            var segGo = new GameObject("seg", typeof(RectTransform), typeof(Image));
            segGo.transform.SetParent(rt, false);
            segGo.transform.SetAsFirstSibling(); // 放在遮罩下面
            var segRt = segGo.GetComponent<RectTransform>();
            segRt.pivot = new Vector2(0f, 0.5f);
            segRt.anchorMin = new Vector2(0f, 0.5f);
            segRt.anchorMax = new Vector2(0f, 0.5f);
            segRt.anchoredPosition = new Vector2(cumulative / total * barWidth, 0f);
            segRt.sizeDelta = new Vector2(segWidth, rt.rect.height);

            var segImg = segGo.GetComponent<Image>();
            segImg.color = seg.isInterruptible
                ? new Color(0.3f, 0.9f, 0.2f)   // 可打断 = 绿色
                : new Color(0.9f, 0.2f, 0.2f);    // 不可打断 = 红色
            segImg.raycastTarget = false;

            cumulative += seg.duration;
        }
    }

    private void ClearSegmentBar()
    {
        RectTransform rt = segmentBarRt;
        if (rt == null && cardLoadingBarFill != null)
            rt = cardLoadingBarFill.rectTransform.parent as RectTransform;
        if (rt == null && cardLoadingBarBg != null)
            rt = cardLoadingBarBg.rectTransform;
        if (rt == null) return;

        for (int i = rt.childCount - 1; i >= 0; i--)
        {
            if (rt.GetChild(i).name == "seg")
                Destroy(rt.GetChild(i).gameObject);
        }
    }

    private void RestoreStaticLoadingBar()
    {
        Transform loadingContainer = cardLoadingBarFill != null
            ? cardLoadingBarFill.transform.parent : null;
        if (loadingContainer != null)
        {
            var staticRed = loadingContainer.Find("NotInterruptImg");
            var staticGreen = loadingContainer.Find("InterruptImg");
            if (staticRed != null) staticRed.gameObject.SetActive(true);
            if (staticGreen != null) staticGreen.gameObject.SetActive(true);
        }
    }

    /// <summary>读条完成</summary>
    private void OnReadingComplete()
    {
        isReading = false;
        IsCardReading = false;
        IsInUninterruptibleSegment = false;
        IsCardReading = false;

        // 销毁思考框中的卡牌
        DestroyReadingCard();

        // 清除分段色条
        ClearSegmentBar();

        // 恢复静态加载条
        RestoreStaticLoadingBar();

        // 隐藏读条UI
        if (cardLoadingBarBg != null) cardLoadingBarBg.transform.parent.gameObject.SetActive(false);
        if (LoadingCount != null) LoadingCount.gameObject.SetActive(false);
        if (ClearBtn != null) ClearBtn.gameObject.SetActive(false);

        // 隐藏思考框描述文字
        HideThinkingText();

        // 应用情绪值变化
        if (readingCardData.panicDelta != 0)
            EmotionSystem.Instance.ChangePanic(readingCardData.panicDelta);
        if (readingCardData.exciteDelta != 0)
            EmotionSystem.Instance.ChangeExcite(readingCardData.exciteDelta);

        // 床上状态变化
        if (readingCardData.bedStateChange != BedStateChange.None)
        {
            bool inBed = readingCardData.bedStateChange == BedStateChange.EnterBed;
            PlayerState.Instance.SetInBed(inBed);
        }

        // 通知 CardManager
        cardManager.OnCardUsed(readingCardData);

        // 通知 TaskManager 推进任务进度
        EventCenter.Instance.EventTrigger(E_EventType.CardReadComplete, readingCardData.id);

        // 跳转背景画面
        if (readingCardData.backgroundJumpId != 0)
            SetSceneBackground(readingCardData.backgroundJumpId, 0.5f);

        // 停止卡牌音效
        if (cardSfxSource != null)
        {
            MusicMgr.Instance.StopSound(cardSfxSource);
            cardSfxSource = null;
        }

        // 播放读条完成通用音效
        MusicMgr.Instance.PlaySound("DXH_SOUND/SOUND2/24.读条完成音效");

        preReadSnapshot = null;
        RefreshSelectionArea();
        Debug.Log($"[GamePanel] 读条完成: {readingCardData.cardName}");
    }

    /// <summary>打断读条（打断按钮）</summary>
    public void InterruptReading()
    {
        if (!isReading) return;

        // 保存读条进度（吃面包等卡牌特殊效果）
        if (readingCardData != null && readingCardData.saveProgressOnInterrupt)
        {
            cardManager.SaveReadProgress(readingCardData.id, readTime, currentSegmentIndex);
            Debug.Log($"[GamePanel] 保存读条进度: {readingCardData.cardName} 时间={readTime:F2}s 片段={currentSegmentIndex}");
        }

        isReading = false;
        IsCardReading = false;
        IsInUninterruptibleSegment = false;
        readTime = 0f;

        // 停止卡牌音效
        if (cardSfxSource != null)
        {
            MusicMgr.Instance.StopSound(cardSfxSource);
            cardSfxSource = null;
        }

        DestroyReadingCard();
        ClearSegmentBar();
        RestoreStaticLoadingBar();
        if (cardLoadingBarBg != null) cardLoadingBarBg.transform.parent.gameObject.SetActive(false);
        if (LoadingCount != null) LoadingCount.gameObject.SetActive(false);
        if (ClearBtn != null) ClearBtn.gameObject.SetActive(false);

        HideThinkingText();

        // 打断时慌乱值：原慌乱值为增加时获得对应增加值，为减少时获得0
        int interruptPanic = readingCardData.panicDelta > 0 ? readingCardData.panicDelta : 0;
        if (interruptPanic != 0)
            EmotionSystem.Instance.ChangePanic(interruptPanic);

        cardManager.OnCardInterrupted(readingCardData);

        // 恢复备选区快照
        cardGrid.Clear();
        if (preReadSnapshot != null)
        {
            var spawns = GetSpawnPositions();
            for (int i = 0; i < preReadSnapshot.Count && i < spawns.Length; i++)
            {
                int stacks = cardManager.GetRemainingStacks(preReadSnapshot[i].id);
                cardGrid.AddCard(preReadSnapshot[i], spawns[i], stacks);
            }
            preReadSnapshot = null;
        }

        if (cardSelectionArea != null) cardSelectionArea.SetActive(true);
        Debug.Log("[GamePanel] 读条被打断");
    }

    private void DestroyReadingCard()
    {
        if (readingCardSlot != null)
        {
            Destroy(readingCardSlot.gameObject);
            readingCardSlot = null;
        }
    }

    private Image thinkingImg;
    private Coroutine thinkingFadeRoutine;
    private Coroutine thinkingTypeRoutine;
    private string thinkingFullText;
    private float thinkingTypeSpeed = 0.03f;

    /// <summary>显示思考框描述文字（打字机 + 淡入）</summary>
    private void ShowThinkingText(CardData data)
    {
        if (thinkingImgBk == null || thinkingText == null || data == null) return;

        if (!string.IsNullOrEmpty(data.description))
        {
            var parts = data.description.Split(new[] { "\n【描述】" },
                System.StringSplitOptions.None);
            thinkingFullText = parts.Length >= 2
                ? parts[1] : data.description;
        }
        else
        {
            thinkingFullText = "";
        }

        thinkingText.text = thinkingFullText;
        thinkingText.maxVisibleCharacters = 0;

        if (thinkingImg == null)
            thinkingImg = thinkingImgBk.GetComponent<Image>();

        if (thinkingFadeRoutine != null) StopCoroutine(thinkingFadeRoutine);
        if (thinkingTypeRoutine != null) StopCoroutine(thinkingTypeRoutine);
        thinkingFadeRoutine = StartCoroutine(FadeThinkingBox(true));
        thinkingTypeRoutine = StartCoroutine(TypewriterEffect());
    }

    private void HideThinkingText()
    {
        if (thinkingImgBk == null) return;

        if (thinkingFadeRoutine != null) StopCoroutine(thinkingFadeRoutine);
        if (thinkingTypeRoutine != null) StopCoroutine(thinkingTypeRoutine);

        if (thinkingImg == null)
            thinkingImg = thinkingImgBk.GetComponent<Image>();

        thinkingFadeRoutine = StartCoroutine(FadeThinkingBox(false));
    }

    private System.Collections.IEnumerator TypewriterEffect()
    {
        int len = thinkingFullText.Length;
        for (int i = 1; i <= len; i++)
        {
            thinkingText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(thinkingTypeSpeed);
        }
    }

    private System.Collections.IEnumerator FadeThinkingBox(bool show)
    {
        float duration = 0.18f;
        float elapsed = 0f;
        float from = show ? 0f : 1f;
        float to = show ? 1f : 0f;

        if (show) thinkingImgBk.SetActive(true);
        if (thinkingImg != null) SetImageAlpha(thinkingImg, from);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (thinkingImg != null)
                SetImageAlpha(thinkingImg, Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        if (thinkingImg != null) SetImageAlpha(thinkingImg, to);
        if (!show) thinkingImgBk.SetActive(false);
    }

    private void SetImageAlpha(Image img, float a)
    {
        var c = img.color;
        c.a = a;
        img.color = c;
    }

    // ==================== 闭眼系统 ====================

    private void OnCloseEyesClicked()
    {
        if (IsInteractionLocked) return;
        ToggleEyeClose();
    }

    private void ToggleEyeClose()
    {
        if (IsInteractionLocked) return;
        HideThinkingText();
        PlayerState.Instance?.ToggleEyesClosed();
    }

    private bool thinkingWasVisible;
    private Dictionary<string, bool> preEyeCloseActiveState = new Dictionary<string, bool>();

    private void OnPlayerEyeCloseChanged(bool closed)
    {
        if (eyeCloseOverlay != null)
            eyeCloseOverlay.gameObject.SetActive(closed);

        if (gameplayUIRoot != null)
        {
            if (closed)
            {
                // 记住每个子物体的当前状态，然后隐藏（保留CloseEyesBtn原位不动、不换图）
                preEyeCloseActiveState.Clear();
                foreach (Transform child in gameplayUIRoot.transform)
                {
                    if (child.name == "EyeCloseOverlay") continue;
                    if (child.name == "Bg5005") continue;
                    if (child.name == "CloseEyesBtn")
                    {
                        if (closeEyesBtnImage != null && closeEyeSprite != null)
                            closeEyesBtnImage.sprite = closeEyeSprite;
                        child.SetAsLastSibling();
                        continue;
                    }
                    if (child.name == "TeacherStatus")
                    {
                        child.SetAsLastSibling();
                        continue;
                    }
                    preEyeCloseActiveState[child.name] = child.gameObject.activeSelf;
                    child.gameObject.SetActive(false);
                }
            }
            else
            {
                // 恢复闭眼前的状态
                if (closeEyesBtnImage != null && openEyeSprite != null)
                    closeEyesBtnImage.sprite = openEyeSprite;
                foreach (Transform child in gameplayUIRoot.transform)
                {
                    if (child.name == "EyeCloseOverlay") continue;
                    if (child.name == "CloseEyesBtn") continue;
                    if (preEyeCloseActiveState.TryGetValue(child.name, out bool wasActive))
                        child.gameObject.SetActive(wasActive);
                }
                preEyeCloseActiveState.Clear();
            }
        }
    }

    // ==================== 备选区刷新 ====================

    private Transform[] GetSpawnPositions()
    {
        if (cardSpawnPos4 == null) cardSpawnPos4 = CreateSpawnPos("cardSpawnPos4", 4);
        if (cardSpawnPos5 == null) cardSpawnPos5 = CreateSpawnPos("cardSpawnPos5", 5);
        if (cardSpawnPos6 == null) cardSpawnPos6 = CreateSpawnPos("cardSpawnPos6", 6);
        if (cardSpawnPos7 == null) cardSpawnPos7 = CreateSpawnPos("cardSpawnPos7", 7);
        if (cardSpawnPos8 == null) cardSpawnPos8 = CreateSpawnPos("cardSpawnPos8", 8);
        return new[] { cardSpawnPos1, cardSpawnPos2, cardSpawnPos3, cardSpawnPos4, cardSpawnPos5, cardSpawnPos6, cardSpawnPos7, cardSpawnPos8 };
    }

    private Transform CreateSpawnPos(string name, int index)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(cardSelectionArea != null ? cardSelectionArea.transform : transform, false);
        return go.transform;
    }

    private void RefreshSelectionArea()
    {
        // 拖拽过程中禁止刷新备选区
        if (CardSlot.CurrentDragging != null) return;

        // 兜底：如果全局池为空，自动加载关卡配置并初始化
        // 通过 GameFlowController 统一初始化所有子系统（EmotionSystem、EyeCloseSystem 等）
        // Initialize 内部会触发 OnSelectionChanged → 递归调用本方法完成刷新，此处直接返回避免重复创建
        if (cardManager != null && cardManager.GetAvailableCardCount() == 0)
        {
            var config = overrideLevelConfig ?? Resources.Load<LevelConfigSO>("TestData/TestLevelConfig");
            if (config != null && Time.frameCount != lastGameFlowInitFrame)
            {
                lastGameFlowInitFrame = Time.frameCount;
                Game.Flow.GameFlowController.Instance.Initialize(config);
                return; // Initialize 已触发了 RefreshSelectionArea
            }
        }

        cardGrid.Clear();

        // 从全局卡牌池获取已解锁且未耗尽的卡牌（关联机制由CardManager管理）
        var available = cardManager != null
            ? cardManager.GetAvailableCards()
            : new List<CardData>();

        var spawns = GetSpawnPositions();

        Debug.Log($"[GamePanel] RefreshSelectionArea: 备选区 {available.Count} 张卡牌");

        for (int i = 0; i < available.Count && i < spawns.Length; i++)
        {
            int stacks = cardManager.GetRemainingStacks(available[i].id);
            cardGrid.AddCard(available[i], spawns[i], stacks);
        }

        if (cardSelectionArea != null)
            cardSelectionArea.SetActive(available.Count > 0);
    }

    // ==================== 卡牌交互 ====================

    private void OnCardClicked(CardSlot card)
    {
        if (card.CardData == null) return;
        if (dialogueManager != null && dialogueManager.IsDialogueActive) return;

        if (cardInfoPopupPrefab == null)
        {
            Debug.LogError("[GamePanel] cardInfoPopupPrefab 未赋值！请在 Inspector 拖入 PopWindow.prefab");
            return;
        }

        // 已打开 → 关闭
        if (activePopup != null && activePopup.IsShown)
        {
            activePopup.Hide();
            CardSlot.IsPopupActive = false;
            if (prePopupTimeScale > 0f)
                Game.Flow.GameFlowController.Instance.ResumeGame();
            return;
        }

        // 首次创建
        if (activePopup == null)
        {
            var go = Instantiate(cardInfoPopupPrefab, transform);
            activePopup = go.GetComponent<CardInfoPopup>();
            if (activePopup == null) activePopup = go.AddComponent<CardInfoPopup>();
            Debug.Log($"[GamePanel] 弹窗实例化成功: {go.name}");
        }

        CardSlot.IsPopupActive = true;
        activePopup.Show(card.CardData, card.GetComponent<RectTransform>());
        prePopupTimeScale = Game.Flow.GameFlowController.Instance.IsPaused() ? 0f : 1f;
        Game.Flow.GameFlowController.Instance.PauseGame();
        Debug.Log($"[GamePanel] 弹窗显示: {card.CardData.cardName}");
    }

    // ==================== 卡牌拖放 ====================

    private void OnCardDropped(CardSlot card)
    {
        if (card.CardData == null) return;
        if (isReading) return;
        if (dialogueManager != null && dialogueManager.IsDialogueActive) return;

        Debug.Log($"[GamePanel] 卡牌拖入思考框: {card.CardData.cardName}");
        StartReading(card);
    }

    private void OnCardDragStarted(CardSlot card)
    {
        if (card.CardData == null) return;
        if (dialogueManager != null && dialogueManager.IsDialogueActive) return;
        ShowThinkingText(card.CardData);
    }

    private void OnCardDragEnded(CardSlot card)
    {
        // 拖拽结束但未放入思考框（回到备选区），隐藏思考框
        // 若已放入思考框，StartReading 中已调用 HideThinkingText，此处为无操作
        HideThinkingText();
    }

    // ==================== 初始化 ====================

    private void EnsureCardGrid()
    {
        cardGrid = GetComponentInChildren<CardGrid>();
        if (cardGrid == null)
        {
            var go = new GameObject("CardGrid", typeof(RectTransform), typeof(CardGrid));
            go.transform.SetParent(transform, false);
            cardGrid = go.GetComponent<CardGrid>();
        }

        var baseCard = Resources.Load<GameObject>("Card/BaseCard");
        if (baseCard != null)
        {
            var field = typeof(CardGrid).GetField("cardSlotPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(cardGrid, baseCard);
        }
    }

    private void EnsureDropZone()
    {
        if (cardDropZoneObj != null)
        {
            cardDropZone = cardDropZoneObj.GetComponent<CardDropZone>();
            if (cardDropZone == null)
                cardDropZone = cardDropZoneObj.AddComponent<CardDropZone>();

            var img = cardDropZoneObj.GetComponent<Image>();
            if (img == null)
                img = cardDropZoneObj.AddComponent<Image>();
            img.raycastTarget = true;

            var rt = cardDropZoneObj.GetComponent<RectTransform>();
            if (rt.sizeDelta.x < 10 || rt.sizeDelta.y < 10)
                rt.sizeDelta = new Vector2(200, 200);
        }

        if (cardDropZone == null)
        {
            cardDropZone = GetComponentInChildren<CardDropZone>();
            if (cardDropZone == null)
            {
                var go = new GameObject("CardDropZone", typeof(RectTransform), typeof(Image), typeof(CardDropZone));
                go.transform.SetParent(transform, false);
                go.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200, 200);
                cardDropZone = go.GetComponent<CardDropZone>();
            }
        }
    }

    private void EnsureCanvasRaycaster()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();
    }

    private void EnsureEventSystem()
    {
        var all = FindObjectsOfType<EventSystem>();
        if (all.Length > 1)
            for (int i = 1; i < all.Length; i++)
                Destroy(all[i].gameObject);

        var es = FindObjectOfType<EventSystem>();
        if (es != null && es.GetComponent<StandaloneInputModule>() == null)
            es.gameObject.AddComponent<StandaloneInputModule>();
        else if (es == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
