using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// 汗滴滴落效果控制器（被动触发式）
    /// 由外部系统（动画状态机、情绪系统等）显式调用播放/停止
    /// 通过 shader + 子 SpriteRenderer 实现，不干扰主角色 SpriteSwap 动画
    /// </summary>
    public class SweatDripController : MonoBehaviour
    {
        [Header("汗滴精灵")]
        [Tooltip("拖入 sweating.png 或 Eff_Sweat.png")]
        public Sprite sweatSprite;

        [Header("滴落参数")]
        [Tooltip("单轮滴落周期（秒）")]
        public float dripCycle = 2f;
        [Tooltip("滴落距离（本地单位），建议 0.1~0.5")]
        public float dripDistance = 0.25f;
        [Tooltip("滴落方向偏移 X（负值 = 向左）")]
        public float dripOffsetX = -0.05f;
        [Tooltip("起始位置偏移（相对于角色中心），建议 x∈[-0.2,0.2] y∈[0.1,0.4]")]
        public Vector2 spawnOffset = new Vector2(-0.1f, 0.2f);
        [Tooltip("汗滴缩放，建议 0.1~0.5")]
        public float sweatScale = 0.25f;

        [Header("Shader 参数")]
        [Tooltip("汗滴材质（使用 Custom/SweatDrip）。留空则运行时自动创建")]
        public Material sweatMaterial;

        // ========== 状态 ==========

        /// <summary>当前是否正在播放汗滴效果</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>当前滴落进度 [0, 1]</summary>
        public float CurrentProgress { get; private set; }

        private GameObject sweatObject;
        private SpriteRenderer sweatRenderer;
        private float cycleTimer;
        private Vector3 startPos;
        private Vector3 endPos;
        private Material runtimeMaterial;

        // ========== 生命周期 ==========

        void Awake()
        {
            EnsureSweatObject();
        }

        void Update()
        {
            if (!IsPlaying || sweatObject == null) return;

            cycleTimer += Time.deltaTime;
            CurrentProgress = Mathf.Clamp01(cycleTimer / Mathf.Max(0.001f, dripCycle));

            if (CurrentProgress >= 1f)
            {
                // 单轮完成，自动循环下一轮
                cycleTimer = 0f;
                CurrentProgress = 0f;
            }

            ApplyDripVisual(CurrentProgress);
        }

        void OnDestroy()
        {
            if (sweatObject != null)
            {
                Destroy(sweatObject);
            }
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }

        // ========== 公共接口 ==========

        /// <summary>
        /// 播放汗滴滴落效果（循环模式）
        /// 若已在播放则重置当前轮次
        /// </summary>
        public void Play()
        {
            if (sweatSprite == null)
            {
                Debug.LogWarning("[SweatDripController] 未设置 sweatSprite，无法播放");
                return;
            }

            EnsureSweatObject();
            ResetDripState();
            sweatObject.SetActive(true);
            IsPlaying = true;

            Debug.Log("[SweatDripController] 开始播放汗滴");
        }

        /// <summary>
        /// 停止汗滴效果
        /// </summary>
        /// <param name="immediate">true=立刻隐藏；false=等当前轮结束后再隐藏</param>
        public void Stop(bool immediate = true)
        {
            if (immediate)
            {
                IsPlaying = false;
                if (sweatObject != null)
                    sweatObject.SetActive(false);
                Debug.Log("[SweatDripController] 停止汗滴");
            }
            else
            {
                // 非即时：等当前轮结束后自动停止
                if (IsPlaying)
                    StartCoroutine(WaitForCycleEnd());
            }
        }

        /// <summary>
        /// 立即播放一轮（单次模式），完成后自动停止
        /// </summary>
        public void PlayOnce(System.Action onComplete = null)
        {
            Play();
            StartCoroutine(PlayOnceRoutine(onComplete));
        }

        /// <summary>
        /// 设置滴落速度倍率（1=默认）
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            dripCycle = Mathf.Max(0.1f, 2f / multiplier);
        }

        /// <summary>
        /// 设置汗滴起始偏移位置
        /// </summary>
        public void SetSpawnOffset(Vector2 offset)
        {
            spawnOffset = offset;
            RecalculatePath();
        }

        // ========== 内部实现 ==========

        void EnsureSweatObject()
        {
            if (sweatObject != null) return;

            sweatObject = new GameObject("SweatEffect");
            sweatObject.transform.SetParent(transform, false);
            sweatObject.transform.localPosition = spawnOffset;
            sweatObject.transform.localScale = Vector3.one * sweatScale;

            sweatRenderer = sweatObject.AddComponent<SpriteRenderer>();
            sweatRenderer.sprite = sweatSprite;
            sweatRenderer.sortingOrder = 10;

            Material mat = GetEffectiveMaterial();
            if (mat != null)
            {
                sweatRenderer.sharedMaterial = mat;
            }

            sweatObject.SetActive(false);
        }

        Material GetEffectiveMaterial()
        {
            if (sweatMaterial != null) return sweatMaterial;
            if (runtimeMaterial != null) return runtimeMaterial;

            Shader shader = Shader.Find("Custom/SweatDrip");
            if (shader == null)
            {
                // Fallback：使用默认 sprite shader，汗滴仍可显示，只是无拉伸淡出效果
                shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    Debug.LogError("[SweatDripController] 找不到任何可用 Shader，汗滴无法显示");
                    return null;
                }
                Debug.LogWarning("[SweatDripController] 未找到 Custom/SweatDrip，已回退到默认 Sprite Shader（无拉伸淡出效果）。请在 Unity 中导入 Custom/SweatDrip 材质以获得完整效果。");
            }

            runtimeMaterial = new Material(shader);
            runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;
            return runtimeMaterial;
        }

        void ResetDripState()
        {
            cycleTimer = 0f;
            CurrentProgress = 0f;
            RecalculatePath();
            if (sweatRenderer != null)
                sweatRenderer.color = new Color(1f, 1f, 1f, 0f);
        }

        void RecalculatePath()
        {
            startPos = spawnOffset;
            endPos = startPos + new Vector3(dripOffsetX, -dripDistance, 0f);
        }

        void ApplyDripVisual(float t)
        {
            // 缓动插值
            float smoothT = Mathf.SmoothStep(0, 1, t);
            sweatObject.transform.localPosition = Vector3.Lerp(startPos, endPos, smoothT);

            // Alpha 曲线：淡入 -> 保持 -> 淡出
            float alpha;
            if (t < 0.15f)
                alpha = t / 0.15f;
            else if (t > 0.8f)
                alpha = 1f - (t - 0.8f) / 0.2f;
            else
                alpha = 1f;

            if (sweatRenderer != null)
                sweatRenderer.color = new Color(1f, 1f, 1f, alpha);
        }

        System.Collections.IEnumerator PlayOnceRoutine(System.Action onComplete)
        {
            yield return new WaitForSeconds(dripCycle);
            Stop(immediate: true);
            onComplete?.Invoke();
        }

        System.Collections.IEnumerator WaitForCycleEnd()
        {
            float wait = (1f - CurrentProgress) * dripCycle;
            yield return new WaitForSeconds(wait);
            Stop(immediate: true);
        }
    }
}
