using System;

namespace Game.Data
{
    /// <summary>
    /// 读条片段数据
    /// </summary>
    [Serializable]
    public class Segment
    {
        public float duration;
        public bool isInterruptible;

        public Segment() { }

        public Segment(float duration, bool isInterruptible)
        {
            this.duration = duration;
            this.isInterruptible = isInterruptible;
        }
    }
}
