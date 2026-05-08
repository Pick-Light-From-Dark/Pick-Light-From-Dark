using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Sprite 描边控制器
    /// 自动为 SpriteRenderer 创建并应用描边材质，默认启用
    /// </summary>
    public class SpriteOutlineController : MonoBehaviour
    {
        [Header("描边设置")]
        [Tooltip("是否启用描边")]
        public bool enableOutline = true;

        [Tooltip("描边颜色（默认棕色影子）")]
        public Color outlineColor = new Color(0.4f, 0.26f, 0.13f, 1f);

        [Tooltip("描边粗细（世界单位），建议 0.1~2")]
        [Range(0f, 10f)]
        public float outlineSize = 0.5f;

        [Tooltip("像素对齐")]
        public bool pixelSnap = true;

        private SpriteRenderer spriteRenderer;
        private Material outlineMaterial;
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int PixelSnapId = Shader.PropertyToID("PixelSnap");

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("[SpriteOutlineController] 未找到 SpriteRenderer，描边未生效");
                enabled = false;
                return;
            }

            ApplyOutline();
        }

        void OnValidate()
        {
            // Inspector 中修改参数时实时更新
            if (Application.isPlaying && spriteRenderer != null && outlineMaterial != null)
            {
                UpdateMaterialProperties();
            }
        }

        /// <summary>
        /// 重新应用描边材质（切换 sprite 后调用）
        /// </summary>
        public void ApplyOutline()
        {
            if (spriteRenderer == null) return;

            if (enableOutline)
            {
                EnsureMaterial();
                spriteRenderer.material = outlineMaterial;
                UpdateMaterialProperties();
            }
            else
            {
                spriteRenderer.material = null; // 恢复默认 sprite 材质
            }
        }

        /// <summary>
        /// 设置描边颜色
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            if (outlineMaterial != null)
            {
                outlineMaterial.SetColor(OutlineColorId, color);
            }
        }

        /// <summary>
        /// 设置描边粗细
        /// </summary>
        public void SetOutlineSize(float size)
        {
            outlineSize = Mathf.Clamp(size, 0, 10);
            if (outlineMaterial != null)
            {
                outlineMaterial.SetFloat(OutlineWidthId, outlineSize);
            }
        }

        /// <summary>
        /// 开关描边
        /// </summary>
        public void SetOutlineEnabled(bool enabled)
        {
            enableOutline = enabled;
            ApplyOutline();
        }

        void EnsureMaterial()
        {
            if (outlineMaterial != null) return;

            Shader shader = Shader.Find("Custom/SpriteOutline");
            if (shader == null)
            {
                Debug.LogError("[SpriteOutlineController] 找不到 Shader 'Custom/SpriteOutline'，请确保 SpriteOutline.shader 已导入");
                return;
            }

            outlineMaterial = new Material(shader);
            outlineMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        void UpdateMaterialProperties()
        {
            if (outlineMaterial == null) return;

            outlineMaterial.SetColor(OutlineColorId, outlineColor);
            outlineMaterial.SetFloat(OutlineWidthId, outlineSize);
            outlineMaterial.SetFloat(PixelSnapId, pixelSnap ? 1f : 0f);
        }

        void OnDestroy()
        {
            if (outlineMaterial != null)
            {
                Destroy(outlineMaterial);
            }
        }
    }
}
