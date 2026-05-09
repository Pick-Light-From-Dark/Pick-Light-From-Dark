using UnityEngine;
using DG.Tweening;

namespace Game.Effects
{
    /// <summary>
    /// 汗滴滴落效果控制器（DOTween 版本）
    /// 由外部系统（动画状态机、情绪系统等）显式调用播放/停止
    /// 通过 DOTween Sequence 控制位置与 Alpha，不干扰主角色 SpriteSwap 动画
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
        private Sequence dripSequence;
        private Material runtimeMaterial;

        // ========== 生命周期 ==========

        void Awake()
        {
            EnsureSweatObject();
        }

        void OnDestroy()
        {
            dripSequence?.Kill();
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
            dripSequence?.Kill();

            // 计算起点终点
            Vector3 startPos = spawnOffset;
            Vector3 endPos = startPos + new Vector3(dripOffsetX, -dripDistance, 0f);
            sweatObject.transform.localPosition = startPos;

            // Alpha 时长分段
            float fadeInDuration = dripCycle * 0.15f;
            float fadeOutDuration = dripCycle * 0.2f;
            float stayDuration = Mathf.Max(0f, dripCycle - fadeInDuration - fadeOutDuration);

            // 构建 DOTween Sequence
            dripSequence = DG.Tweening.DOTween.Sequence();

            // 位置：从 startPos 到 endPos，全程 ease-in
            dripSequence.Append(
                sweatObject.transform.DOLocalMove(endPos, dripCycle)
                    .SetEase(Ease.InQuad)
            );

            // Alpha：淡入 -> 保持 -> 淡出
            var alphaSeq = DG.Tweening.DOTween.Sequence();
            alphaSeq.Append(sweatRenderer.DOFade(1f, fadeInDuration).From(0f));
            if (stayDuration > 0f)
                alphaSeq.AppendInterval(stayDuration);
            alphaSeq.Append(sweatRenderer.DOFade(0f, fadeOutDuration));

            dripSequence.Join(alphaSeq);

            // 无限循环
            dripSequence.SetLoops(-1, LoopType.Restart);
            dripSequence.OnStepComplete(() =>
            {
                // 每轮开始时重置位置（LoopType.Restart 会自动处理，但显式重置更安全）
                sweatObject.transform.localPosition = startPos;
            });

            sweatObject.SetActive(true);
            IsPlaying = true;

            Debug.Log("[SweatDripController] DOTween 汗滴动画已启动");
        }

        /// <summary>
        /// 停止汗滴效果
        /// </summary>
        /// <param name="immediate">true=立刻隐藏；false=等当前轮结束后再隐藏</param>
        public void Stop(bool immediate = true)
        {
            if (immediate)
            {
                dripSequence?.Kill();
                IsPlaying = false;
                if (sweatObject != null)
                    sweatObject.SetActive(false);
                Debug.Log("[SweatDripController] 停止汗滴");
            }
            else
            {
                if (dripSequence != null && dripSequence.IsPlaying())
                {
                    dripSequence.SetLoops(1);
                    dripSequence.OnComplete(() =>
                    {
                        IsPlaying = false;
                        if (sweatObject != null)
                            sweatObject.SetActive(false);
                    });
                }
            }
        }

        /// <summary>
        /// 立即播放一轮（单次模式），完成后自动停止
        /// </summary>
        public void PlayOnce(System.Action onComplete = null)
        {
            if (sweatSprite == null)
            {
                Debug.LogWarning("[SweatDripController] 未设置 sweatSprite，无法播放");
                return;
            }

            EnsureSweatObject();
            dripSequence?.Kill();

            Vector3 startPos = spawnOffset;
            Vector3 endPos = startPos + new Vector3(dripOffsetX, -dripDistance, 0f);
            sweatObject.transform.localPosition = startPos;

            float fadeInDuration = dripCycle * 0.15f;
            float fadeOutDuration = dripCycle * 0.2f;
            float stayDuration = Mathf.Max(0f, dripCycle - fadeInDuration - fadeOutDuration);

            dripSequence = DG.Tweening.DOTween.Sequence();
            dripSequence.Append(sweatObject.transform.DOLocalMove(endPos, dripCycle).SetEase(Ease.InQuad));

            var alphaSeq = DG.Tweening.DOTween.Sequence();
            alphaSeq.Append(sweatRenderer.DOFade(1f, fadeInDuration).From(0f));
            if (stayDuration > 0f)
                alphaSeq.AppendInterval(stayDuration);
            alphaSeq.Append(sweatRenderer.DOFade(0f, fadeOutDuration));

            dripSequence.Join(alphaSeq);
            dripSequence.SetLoops(1);
            dripSequence.OnComplete(() =>
            {
                IsPlaying = false;
                if (sweatObject != null)
                    sweatObject.SetActive(false);
                onComplete?.Invoke();
            });

            sweatObject.SetActive(true);
            IsPlaying = true;
        }

        /// <summary>
        /// 设置滴落速度倍率（1=默认）
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            dripCycle = Mathf.Max(0.1f, 2f / multiplier);
            if (IsPlaying)
            {
                // 重建动画以应用新速度
                Play();
            }
        }

        /// <summary>
        /// 设置汗滴起始偏移位置
        /// </summary>
        public void SetSpawnOffset(Vector2 offset)
        {
            spawnOffset = offset;
            if (IsPlaying)
            {
                Play();
            }
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
                shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    Debug.LogError("[SweatDripController] 找不到任何可用 Shader，汗滴无法显示");
                    return null;
                }
                Debug.LogWarning("[SweatDripController] 未找到 Custom/SweatDrip，已回退到默认 Sprite Shader（无拉伸淡出效果）");
            }

            runtimeMaterial = new Material(shader);
            runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;
            return runtimeMaterial;
        }
    }
}
