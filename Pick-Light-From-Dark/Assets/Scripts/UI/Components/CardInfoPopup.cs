using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Data;

namespace Game.UI
{
    public class CardInfoPopup : MonoBehaviour
    {
        [Header("卡牌名 — CardNameText")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Header("卡牌作用 — FunctionText")]
        [SerializeField] private TextMeshProUGUI functionText;

        [Header("慌乱值变化 — ChaosText")]
        [SerializeField] private TextMeshProUGUI panicText;

        [Header("兴奋值变化 — HapplyText")]
        [SerializeField] private TextMeshProUGUI exciteText;

        [Header("总读条时长 — TotalTimeText")]
        [SerializeField] private TextMeshProUGUI totalTimeText;

        [Header("可打断标识 — AlreadingLoadingText")]
        [SerializeField] private TextMeshProUGUI interruptibleText;

        [Header("不可打断标识 — ReadyLoadingText")]
        [SerializeField] private TextMeshProUGUI nonInterruptibleText;

        [Header("片段可视化 — LoadingCDImgBk(红底) + LoadingCDImg(绿条)")]
        [SerializeField] private Image loadingBarBg;
        [SerializeField] private RectTransform loadingBarFillRt;

        [Header("背景 — PopWindow 的 Image")]
        [SerializeField] private Image background;

        private RectTransform rectTransform;
        private Coroutine animRoutine;
        private string functionFullText;
        private float typeSpeed = 0.02f;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        public void Show(CardData data, RectTransform targetCard)
        {
            if (data == null) return;
            FillData(data);
            PositionAt(targetCard);
            gameObject.SetActive(true);

            if (animRoutine != null) StopCoroutine(animRoutine);
            animRoutine = StartCoroutine(AnimateShow());
        }

        public void Hide()
        {
            if (animRoutine != null) StopCoroutine(animRoutine);
            animRoutine = StartCoroutine(AnimateHide());
        }

        public bool IsShown => gameObject.activeSelf;

        private System.Collections.IEnumerator AnimateShow()
        {
            float duration = 0.18f;
            float elapsed = 0f;
            rectTransform.localScale = Vector3.one * 0.9f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                t = 1f - (1f - t) * (1f - t);
                rectTransform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, t);
                yield return null;
            }
            rectTransform.localScale = Vector3.one;
        }

        private System.Collections.IEnumerator AnimateHide()
        {
            float duration = 0.1f;
            float elapsed = 0f;
            Vector3 from = rectTransform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(from, Vector3.one * 0.9f, elapsed / duration);
                yield return null;
            }
            gameObject.SetActive(false);
            rectTransform.localScale = Vector3.one;
        }

        private void FillData(CardData data)
        {
            SetText(nameText, data.cardName);

            if (!string.IsNullOrEmpty(data.description))
            {
                var parts = data.description.Split(new[] { "\n【描述】" },
                    System.StringSplitOptions.None);
                functionFullText = parts.Length >= 2 ? parts[0] : data.description;
            }
            else
            {
                functionFullText = null;
            }

            // 设置全文但暂不显示，等待打字机动画
            if (functionText != null)
            {
                if (!string.IsNullOrEmpty(functionFullText))
                {
                    functionText.text = functionFullText;
                    functionText.gameObject.SetActive(true);
                }
                else
                {
                    functionText.gameObject.SetActive(false);
                }
            }

            if (data.segments != null && data.segments.Count > 0)
            {
                float interruptibleTotal = 0f;
                float nonInterruptibleTotal = 0f;
                foreach (var seg in data.segments)
                {
                    if (seg.isInterruptible)
                        interruptibleTotal += seg.duration;
                    else
                        nonInterruptibleTotal += seg.duration;
                }

                SetText(interruptibleText, interruptibleTotal > 0 ? $"可打断段: {interruptibleTotal:F0}s" : null);
                SetText(nonInterruptibleText, nonInterruptibleTotal > 0 ? $"不可打断段: {nonInterruptibleTotal:F0}s" : null);
            }
            else
            {
                SetText(interruptibleText, null);
                SetText(nonInterruptibleText, null);
            }

            BuildSegmentBar(data);

            SetText(panicText, $"慌乱{data.panicDelta:+0;-0}");
            SetText(exciteText, $"兴奋{data.exciteDelta:+0;-0}");

            float total = data.CalculateTotalDuration();
            SetText(totalTimeText, $"耗时{total:F0}s");
        }

        private void BuildSegmentBar(CardData data)
        {
            if (loadingBarFillRt == null) return;

            // 清除旧的片段子对象
            for (int i = loadingBarFillRt.childCount - 1; i >= 0; i--)
                Destroy(loadingBarFillRt.GetChild(i).gameObject);

            if (data.segments == null || data.segments.Count == 0) return;

            float total = data.CalculateTotalDuration();
            if (total <= 0f) return;

            float barWidth = loadingBarFillRt.rect.width;
            float cumulative = 0f;

            foreach (var seg in data.segments)
            {
                float segWidth = (seg.duration / total) * barWidth;
                if (segWidth < 2f) segWidth = 2f; // 太窄不可见

                var segGo = new GameObject("seg", typeof(RectTransform), typeof(Image));
                segGo.transform.SetParent(loadingBarFillRt, false);
                var segRt = segGo.GetComponent<RectTransform>();
                segRt.pivot = new Vector2(0f, 0.5f);
                segRt.anchorMin = new Vector2(0f, 0.5f);
                segRt.anchorMax = new Vector2(0f, 0.5f);
                segRt.anchoredPosition = new Vector2(cumulative / total * barWidth, 0f);
                segRt.sizeDelta = new Vector2(segWidth, loadingBarFillRt.rect.height);

                var segImg = segGo.GetComponent<Image>();
                segImg.color = seg.isInterruptible
                    ? new Color(0.3f, 0.9f, 0.2f)   // 绿色 = 可打断
                    : new Color(0.9f, 0.2f, 0.2f);    // 红色 = 不可打断
                segImg.raycastTarget = false;

                cumulative += seg.duration;
            }
        }

        private void PositionAt(RectTransform target)
        {
            if (target == null || rectTransform == null) return;

            var parentRt = rectTransform.parent as RectTransform;
            if (parentRt == null) return;

            rectTransform.SetAsLastSibling();

            Vector2 localPos;
            Vector2 targetCenter = target.TransformPoint(target.rect.center);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt,
                RectTransformUtility.WorldToScreenPoint(null, targetCenter),
                null,
                out localPos);

            float cardWidth = target.rect.width;
            float cardHeight = target.rect.height;
            float gap = 20f;

            rectTransform.anchorMin = Vector2.one * 0.5f;
            rectTransform.anchorMax = Vector2.one * 0.5f;
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = localPos
                + Vector2.up * (cardHeight * 0.5f)
                + Vector2.left * (cardWidth * 0.5f + gap);
        }

        private void SetText(TextMeshProUGUI tmp, string value)
        {
            if (tmp == null) return;
            if (string.IsNullOrEmpty(value))
            {
                tmp.gameObject.SetActive(false);
            }
            else
            {
                tmp.gameObject.SetActive(true);
                tmp.text = value;
            }
        }
    }
}
