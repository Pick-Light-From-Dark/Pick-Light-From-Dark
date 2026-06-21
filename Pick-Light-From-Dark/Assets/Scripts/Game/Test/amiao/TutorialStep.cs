using System;
using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 引导步骤指示器类型
    /// </summary>
    public enum IndicatorType
    {
        Arrow,   // 箭头
        Finger,  // 手指
        Border   // 高亮边框（默认）
    }

    /// <summary>
    /// 引导步骤完成条件
    /// </summary>
    public enum TutorialTrigger
    {
        ClickTarget,  // 点击目标
        DragToTarget, // 拖拽到目标区
        AnyClick      // 任意点击（确认理解后点击任意处）
    }

    /// <summary>
    /// 镂空框锚点位置
    /// </summary>
    public enum CutoutAnchor
    {
        Target,      // 跟随目标元素（默认）
        TopRight,    // 右上角
        BottomLeft,  // 左下角
        Center       // 屏幕正中
    }

    /// <summary>
    /// 单步引导数据
    /// </summary>
    [Serializable]
    public class TutorialStep
    {
        [Tooltip("步骤唯一标识，如 lv1_step_card_click")]
        public string stepId;

        [Tooltip("目标UI物体名称（GameObject.Find用）")]
        public string targetObjectName;

        [Tooltip("引导文案")]
        [TextArea(1, 3)]
        public string instructionText;

        [Tooltip("镂空区域大小（像素）")]
        public Vector2 cutoutSize = new Vector2(300, 120);

        [Tooltip("镂空框锚点")]
        public CutoutAnchor cutoutAnchor = CutoutAnchor.Target;

        [Tooltip("镂空区域相对于锚点的偏移（仅Target模式相对目标中心，其他模式相对屏幕边角）")]
        public Vector2 cutoutOffset = Vector2.zero;

        [Tooltip("是否显示半透明遮罩")]
        public bool showMask = true;

        [Tooltip("指示器类型")]
        public IndicatorType indicator = IndicatorType.Border;

        [Tooltip("完成条件")]
        public TutorialTrigger trigger = TutorialTrigger.ClickTarget;
    }
}
