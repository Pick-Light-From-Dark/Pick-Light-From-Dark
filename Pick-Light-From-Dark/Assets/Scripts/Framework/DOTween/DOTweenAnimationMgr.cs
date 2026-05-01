using UnityEngine;
using DG.Tweening;
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

            canvasGroup.DOFade(1f, uiFadeDuration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// UI 面板淡出
        /// </summary>
        public void UIFadeOut(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) return;

            canvasGroup.DOFade(0f, uiFadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => canvasGroup.gameObject.SetActive(false));
        }

        /// <summary>
        /// 卡牌发牌动画（从起始位置飞向目标位置）
        /// </summary>
        public void CardDealAnimation(Transform card, Vector3 targetPos)
        {
            if (card == null) return;

            card.DOMove(targetPos, cardDealDuration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// 闭眼渐变动画
        /// </summary>
        public void EyeCloseFadeAnimation(Renderer renderer, bool closing)
        {
            if (renderer == null) return;

            if (closing)
            {
                // 闭眼渐变
                renderer.material.DOFade(0f, eyeCloseFadeDuration).SetEase(Ease.InQuad);
            }
            else
            {
                // 睁眼渐变
                renderer.material.DOFade(1f, eyeCloseFadeDuration).SetEase(Ease.OutQuad);
            }

            Debug.Log($"[DOTweenAnimationMgr] 闭眼渐变动画: {(closing ? "闭上" : "睁开")}");
        }

        /// <summary>
        /// 时间加速视觉反馈
        /// </summary>
        public void TimeAccelerateFeedback(bool accelerated)
        {
            // TODO: 实现时间加速时的视觉扭曲效果
            // 例如：屏幕边缘暗角、时间扭曲粒子等
            Debug.Log($"[DOTweenAnimationMgr] 时间加速反馈: {(accelerated ? "开启" : "关闭")}");
        }
    }
}
