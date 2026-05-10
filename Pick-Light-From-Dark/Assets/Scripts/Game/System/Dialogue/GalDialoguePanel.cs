using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[UIPath("UI/Dialogue")]
public class GalDialoguePanel : BasePanel, IDialoguePanel
{
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI speakerText;

    public Image roleImage;
    public Image backgroundImage;
    public Image midgroundImage;
    public Image foregroundImage;

    public Transform choiceRoot;

    [Header("快进快退按钮")]
    public Button fastForwardBtn;
    public Button rewindBtn;

    [Header("打字机效果")]
    [Tooltip("逐字显示间隔（秒）")]
    public float typingSpeed = 0.03f;

    [Header("动画时长")]
    public float roleFadeDuration = 0.25f;
    public float bgSlideDuration = 0.35f;

    // ===== 打字机状态 =====
    private string currentFullText;
    private Coroutine typewriterRoutine;

    // ===== 立绘动画状态 =====
    private Coroutine roleFadeRoutine;
    private Sprite pendingRoleSprite;

    // ===== 背景动画状态 =====
    private Coroutine bgSlideRoutine;
    private Sprite pendingBgSprite;

    // ===== 运行时按钮创建标记 =====
    private bool buttonsCreated = false;

    /// <summary>是否正在打字中</summary>
    public bool IsTyping => typewriterRoutine != null;

    protected override void Awake()
    {
        EnsureControlButtons();
        base.Awake();

        // 文字描边美化
        if (contentText != null)
        {
            Material mat = contentText.fontMaterial;
            mat.EnableKeyword("OUTLINE_ON");
            mat.SetColor("_OutlineColor", Color.black);
            mat.SetFloat("_OutlineWidth", 0.15f);
        }
        if (speakerText != null)
        {
            Material mat = speakerText.fontMaterial;
            mat.EnableKeyword("OUTLINE_ON");
            mat.SetColor("_OutlineColor", Color.black);
            mat.SetFloat("_OutlineWidth", 0.2f);
        }
    }

    /// <summary>运行时动态创建控制按钮（方案B：无需修改Prefab）</summary>
    private void EnsureControlButtons()
    {
        if (buttonsCreated) return;

        if (fastForwardBtn == null)
            fastForwardBtn = CreateControlButton("FastForwardBtn", "快进", new Vector2(-80, -40));

        if (rewindBtn == null)
            rewindBtn = CreateControlButton("RewindBtn", "快退", new Vector2(-210, -40));

        buttonsCreated = true;
    }

    private Button CreateControlButton(string name, string text, Vector2 anchoredPos)
    {
        Transform existing = transform.Find(name);
        if (existing != null)
            return existing.GetComponent<Button>();

        GameObject btnGO = new GameObject(name, typeof(RectTransform));
        btnGO.transform.SetParent(transform, false);

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(120, 50);

        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.85f);
        img.type = Image.Type.Sliced;

        Button btn = btnGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        colors.disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f);
        btn.colors = colors;

        GameObject textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.196f, 0.196f, 0.196f, 1f);

        if (contentText != null)
            tmp.font = contentText.font;
        else if (speakerText != null)
            tmp.font = speakerText.font;

        return btn;
    }

    /// <summary>设置打字机速度（秒/字）</summary>
    public void SetTypingSpeed(float speed)
    {
        typingSpeed = Mathf.Max(0.001f, speed);
    }

    public void SetContent(string speaker, string content)
    {
        if (speakerText != null)
            speakerText.text = speaker;

        if (contentText != null)
        {
            currentFullText = content;
            contentText.text = content;
            contentText.maxVisibleCharacters = 0;
            StartTypewriter();
        }
    }

    /// <summary>跳过打字机，立即显示全部文本</summary>
    public void SkipTyping()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        if (contentText != null)
            contentText.maxVisibleCharacters = currentFullText != null ? currentFullText.Length : 0;
    }

    private void StartTypewriter()
    {
        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);
        typewriterRoutine = StartCoroutine(TypewriterEffect());
    }

    private IEnumerator TypewriterEffect()
    {
        int len = currentFullText != null ? currentFullText.Length : 0;
        for (int i = 1; i <= len; i++)
        {
            if (contentText != null)
                contentText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        typewriterRoutine = null;
    }

    public void SetRole(Sprite sprite)
    {
        if (roleImage == null) return;

        if (sprite != null)
        {
            // 如果是同一张图，不做动画
            if (roleImage.sprite == sprite && roleImage.gameObject.activeSelf)
                return;

            // 当前有立绘且要切换新立绘 → 淡入淡出
            if (roleImage.sprite != null && roleImage.gameObject.activeSelf)
            {
                pendingRoleSprite = sprite;
                if (roleFadeRoutine != null) StopCoroutine(roleFadeRoutine);
                roleFadeRoutine = StartCoroutine(RoleFadeTransition(sprite));
                return;
            }

            // 直接显示（首次或之前为空）
            roleImage.sprite = sprite;
            roleImage.color = Color.white;
            roleImage.gameObject.SetActive(true);
        }
        else
        {
            // 隐藏立绘
            if (roleImage.gameObject.activeSelf)
            {
                if (roleFadeRoutine != null) StopCoroutine(roleFadeRoutine);
                roleFadeRoutine = StartCoroutine(RoleFadeOut());
            }
        }
    }

    private IEnumerator RoleFadeTransition(Sprite newSprite)
    {
        float elapsed = 0f;
        Color c = roleImage.color;

        // 淡出旧立绘
        while (elapsed < roleFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / roleFadeDuration);
            roleImage.color = c;
            yield return null;
        }

        // 切换 Sprite
        roleImage.sprite = newSprite;
        c.a = 0f;
        roleImage.color = c;

        // 淡入新立绘
        elapsed = 0f;
        while (elapsed < roleFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / roleFadeDuration);
            roleImage.color = c;
            yield return null;
        }

        c.a = 1f;
        roleImage.color = c;
        roleFadeRoutine = null;
    }

    private IEnumerator RoleFadeOut()
    {
        float elapsed = 0f;
        Color c = roleImage.color;

        while (elapsed < roleFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / roleFadeDuration);
            roleImage.color = c;
            yield return null;
        }

        roleImage.gameObject.SetActive(false);
        roleImage.sprite = null;
        c.a = 1f;
        roleImage.color = c;
        roleFadeRoutine = null;
    }

    public void SetBackground(Sprite sprite, string transition = null)
    {
        if (backgroundImage == null) return;

        if (sprite == null)
        {
            backgroundImage.sprite = null;
            return;
        }

        // 如果背景图相同，不做动画
        if (backgroundImage.sprite == sprite)
            return;

        // 默认或无转场：直接切换
        if (string.IsNullOrEmpty(transition) || transition == "none")
        {
            if (bgSlideRoutine != null) StopCoroutine(bgSlideRoutine);
            backgroundImage.sprite = sprite;
            backgroundImage.gameObject.SetActive(true);
            ResetBgPosition();
            return;
        }

        // 划入划出
        if (transition == "slide")
        {
            pendingBgSprite = sprite;
            if (bgSlideRoutine != null) StopCoroutine(bgSlideRoutine);
            bgSlideRoutine = StartCoroutine(BackgroundSlideTransition(sprite));
            return;
        }

        // 淡入淡出
        if (transition == "fade")
        {
            pendingBgSprite = sprite;
            if (bgSlideRoutine != null) StopCoroutine(bgSlideRoutine);
            bgSlideRoutine = StartCoroutine(BackgroundFadeTransition(sprite));
            return;
        }

        // 未知转场类型，直切
        if (bgSlideRoutine != null) StopCoroutine(bgSlideRoutine);
        backgroundImage.sprite = sprite;
        backgroundImage.gameObject.SetActive(true);
        ResetBgPosition();
    }

    private IEnumerator BackgroundSlideTransition(Sprite newSprite)
    {
        RectTransform rt = backgroundImage.rectTransform;
        Vector2 centerPos = Vector2.zero;
        float screenW = Screen.width;

        // 划出旧背景（向左）
        float elapsed = 0f;
        while (elapsed < bgSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / bgSlideDuration;
            rt.anchoredPosition = Vector2.Lerp(centerPos, centerPos + Vector2.left * screenW, t);
            yield return null;
        }

        // 切换图片并放到右侧外
        backgroundImage.sprite = newSprite;
        rt.anchoredPosition = centerPos + Vector2.right * screenW;

        // 划入新背景（从右到左回到中心）
        elapsed = 0f;
        while (elapsed < bgSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / bgSlideDuration;
            rt.anchoredPosition = Vector2.Lerp(centerPos + Vector2.right * screenW, centerPos, t);
            yield return null;
        }

        rt.anchoredPosition = centerPos;
        bgSlideRoutine = null;
    }

    private IEnumerator BackgroundFadeTransition(Sprite newSprite)
    {
        float elapsed = 0f;
        Color c = backgroundImage.color;

        // 淡出旧背景
        while (elapsed < bgSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / bgSlideDuration);
            backgroundImage.color = c;
            yield return null;
        }

        // 切换 Sprite
        backgroundImage.sprite = newSprite;
        c.a = 0f;
        backgroundImage.color = c;

        // 淡入新背景
        elapsed = 0f;
        while (elapsed < bgSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / bgSlideDuration);
            backgroundImage.color = c;
            yield return null;
        }

        c.a = 1f;
        backgroundImage.color = c;
        bgSlideRoutine = null;
    }

    public void SetLayerBackground(string layer, Sprite sprite, string transition = null)
    {
        Image target = layer switch
        {
            "bg" => backgroundImage,
            "mg" => midgroundImage,
            "fg" => foregroundImage,
            _ => null
        };

        if (target == null) return;

        if (sprite == null)
        {
            target.sprite = null;
            target.color = Color.clear;
            return;
        }

        // 暂不支持多层转场动画，直接切换
        if (bgSlideRoutine != null) StopCoroutine(bgSlideRoutine);
        target.sprite = sprite;
        target.gameObject.SetActive(true);
        target.color = Color.white;
        target.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void SetSolidBackground(Color color)
    {
        if (bgSlideRoutine != null) StopCoroutine(bgSlideRoutine);

        if (backgroundImage != null)
        {
            backgroundImage.sprite = null;
            backgroundImage.color = color;
        }
        if (midgroundImage != null) { midgroundImage.sprite = null; midgroundImage.color = Color.clear; }
        if (foregroundImage != null) { foregroundImage.sprite = null; foregroundImage.color = Color.clear; }
    }

    private void ResetBgPosition()
    {
        if (backgroundImage != null)
            backgroundImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ResetBgPosition();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (DialogueSystem.Instance == null) return;

        if (rewindBtn != null)
        {
            bool rewinding = DialogueSystem.Instance.IsAutoRewinding;
            rewindBtn.interactable = DialogueSystem.Instance.CanRewind || rewinding;
            var colors = rewindBtn.colors;
            colors.normalColor = rewinding ? new Color(0.6f, 0.8f, 1f) : Color.white;
            rewindBtn.colors = colors;
        }
        if (fastForwardBtn != null)
        {
            bool ff = DialogueSystem.Instance.IsFastForwarding;
            fastForwardBtn.interactable = DialogueSystem.Instance.CanFastForward || ff;
            var colors = fastForwardBtn.colors;
            colors.normalColor = ff ? new Color(0.6f, 1f, 0.6f) : Color.white;
            fastForwardBtn.colors = colors;
        }
    }

    protected override void ClickBtn(string btnName)
    {
        base.ClickBtn(btnName);

        if (DialogueSystem.Instance == null) return;

        if (btnName == "FastForwardBtn")
        {
            DialogueSystem.Instance.ToggleFastForward();
        }
        else if (btnName == "RewindBtn")
        {
            DialogueSystem.Instance.ToggleRewind();
        }
    }

    public override void ShowMe() { }

    public override void HideMe() { }

    void OnDisable()
    {
        // 清理协程，避免复用时状态残留
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        if (roleFadeRoutine != null)
        {
            StopCoroutine(roleFadeRoutine);
            roleFadeRoutine = null;
        }
        if (bgSlideRoutine != null)
        {
            StopCoroutine(bgSlideRoutine);
            bgSlideRoutine = null;
        }
    }
}
