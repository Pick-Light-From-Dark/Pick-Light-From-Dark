using System.Collections.Generic;
using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 关卡引导配置 — ScriptableObject，每个关卡一个.asset文件
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Tutorial Config", fileName = "TutorialConfig_Level1")]
    public class TutorialConfigSO : ScriptableObject
    {
        [Tooltip("关卡ID")]
        public int levelId = 1;

        [Tooltip("引导步骤列表")]
        public List<TutorialStep> steps = new List<TutorialStep>();

        /// <summary>是否已配置步骤</summary>
        public bool HasSteps => steps != null && steps.Count > 0;

        /// <summary>按索引获取步骤，越界返回null</summary>
        public TutorialStep GetStep(int index)
        {
            if (steps == null || index < 0 || index >= steps.Count) return null;
            return steps[index];
        }
    }
}
