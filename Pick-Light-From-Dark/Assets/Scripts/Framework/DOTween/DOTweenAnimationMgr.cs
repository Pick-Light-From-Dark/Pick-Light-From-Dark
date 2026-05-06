using UnityEngine;
using System.Collections;

namespace Game.DOTween
{
    /// <summary>
    /// DOTween 动画管理器 - 统一管理游戏中的动画效果
    /// </summary>
    public class DOTweenAnimationMgr : SingletonAutoMono<DOTweenAnimationMgr>
    {
        [Header("动画配置")]
        [Tooltip("UI 面板淡入淡出时长")]
        public float uiFadeDuration = 0.3f;

        [Tooltip("卡牌发牌动画时长")]
        public float cardDealDuration = 0.5f;

        [Tooltip("闭眼渐变动画时长")]
        public float eyeCloseFadeDuration = 1f;

        /// <summary>
        /// UI 面板淡入
        /// </summary>
        public void UIFadeIn(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);

            // TODO: 替换为 DOTween.DOFade(canvasGroup, 1f, uiFadeDuration)
            StartCoroutine(FadeInCoroutine(canvasGroup, 1f, uiFadeDuration));
        }

        /// <summary>
        /// UI 面板淡出
        /// </summary>
        public void UIFadeOut(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) return;

            // TODO: 替换为 DOTween.DOFade(canvasGroup, 0f, uiFadeDuration)
            StartCoroutine(FadeOutCoroutine(canvasGroup, 0f, uiFadeDuration));
        }

        /// <summary>
        /// 卡牌发牌动画（从起始位置飞向目标位置）
        /// </summary>
        public void CardDealAnimation(Transform card, Vector3 targetPos)
        {
            if (card == null) return;

            // TODO: 替换为 DOTween.DOMove(card, targetPos, cardDealDuration).SetEase(Ease.OutQuad)
            StartCoroutine(MoveToCoroutine(card, targetPos, cardDealDuration));
        }

        /// <summary>
        /// 闭眼渐变动画
        /// </summary>
        public void EyeCloseFadeAnimation(Renderer renderer, bool closing)
        {
            // TODO: 替换为 DOTween.DOColor 和 DOFade 实现渐变
            Debug.Log($"[DOTweenAnimationMgr] 闭眼渐变动画: {(closing ? "闭上" : "睁开")}");
        }

        /// <summary>
        /// 时间加速视觉反馈
        /// </summary>
        public void TimeAccelerateFeedback(bool accelerated)
        {
            // TODO: 实现时间加速时的视觉扭曲效果
            Debug.Log($"[DOTweenAnimationMgr] 时间加速反馈: {(accelerated ? "开启" : "关闭")}");
        }

        // 临时的 Coroutine 实现，待 DOTween 安装后替换
        private IEnumerator FadeInCoroutine(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            float timer = 0f;
            float startAlpha = canvasGroup.alpha;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        private IEnumerator FadeOutCoroutine(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            float timer = 0f;
            float startAlpha = canvasGroup.alpha;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            canvasGroup.gameObject.SetActive(false);
        }

        private IEnumerator MoveToCoroutine(Transform obj, Vector3 targetPos, float duration)
        {
            float timer = 0f;
            Vector3 startPos = obj.position;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                obj.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            obj.position = targetPos;
        }
    }
}
