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

    /// <summary>是否正在打字中</summary>
    public bool IsTyping => typewriterRoutine != null;

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

    public void SetBackground(Sprite sprite)
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

        // 当前有背景图 → 划入划出过渡
        if (backgroundImage.sprite != null && backgroundImage.gameObject.activeSelf)
        {
            pendingBgSprite = sprite;
            if (bgSlideRoutine != null) StopCoroutine(bgSlideRoutine);
            bgSlideRoutine = StartCoroutine(BackgroundSlideTransition(sprite));
            return;
        }

        // 首次设置背景
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
