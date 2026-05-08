using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// 随机表情播放器
    /// 表情类动画（Blink/LittleExcited/Excited）平时冻结在第一帧，随机间隔后播放一次
    /// 动作类动画（Chew）正常循环播放
    /// </summary>
    public class RandomBlinkController : MonoBehaviour
    {
        [Header("Blink 随机间隔")]
        public float blinkMinInterval = 2f;
        public float blinkMaxInterval = 5f;

        [Header("LittleExcited 随机间隔")]
        public float littleExcitedMinInterval = 2f;
        public float littleExcitedMaxInterval = 5f;

        [Header("Excited 随机间隔")]
        public float excitedMinInterval = 3f;
        public float excitedMaxInterval = 6f;

        [Header("调试")]
        public bool showDebugLog = false;

        private Animator animator;
        private float waitTimer;
        private bool isPlaying;
        private float currentClipDuration;
        private float playElapsed;

        void Start()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("[RandomBlink] 未找到 Animator 组件");
                enabled = false;
                return;
            }

            ResetTimer();
        }

        void Update()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            string ctrlName = animator.runtimeAnimatorController.name;
            bool isExpression = ctrlName.Contains("Blink")
                                || ctrlName.Contains("LittleExcited")
                                || ctrlName.Contains("Excited");

            if (!isExpression)
            {
                // 动作类动画（如 Chew）正常循环播放
                if (animator.speed == 0f)
                    animator.speed = 1f;
                return;
            }

            if (isPlaying)
            {
                playElapsed += Time.deltaTime;
                if (playElapsed >= currentClipDuration)
                {
                    isPlaying = false;
                    animator.speed = 0f;
                    animator.Play(0, 0, 0f);
                    ResetTimer();

                    if (showDebugLog)
                        Debug.Log("[RandomBlink] 播放结束，回到静态");
                }
            }
            else
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    isPlaying = true;
                    playElapsed = 0f;
                    animator.speed = 1f;

                    AnimationClip clip = GetCurrentClip();
                    currentClipDuration = clip != null ? clip.length : 1.5f;

                    if (showDebugLog)
                        Debug.Log($"[RandomBlink] 开始播放 {ctrlName}，持续 {currentClipDuration:F2}s");
                }
            }
        }

        void ResetTimer()
        {
            string ctrlName = animator.runtimeAnimatorController.name;
            if (ctrlName.Contains("Excited") && !ctrlName.Contains("Little"))
                waitTimer = Random.Range(excitedMinInterval, excitedMaxInterval);
            else if (ctrlName.Contains("LittleExcited"))
                waitTimer = Random.Range(littleExcitedMinInterval, littleExcitedMaxInterval);
            else // Blink
                waitTimer = Random.Range(blinkMinInterval, blinkMaxInterval);
        }

        AnimationClip GetCurrentClip()
        {
            if (animator.runtimeAnimatorController == null)
                return null;

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips != null && clips.Length > 0)
                return clips[0];

            return null;
        }
    }
}
