using UnityEngine;
using Game.Data;
using Game.Config;

namespace Game.System
{
    /// <summary>
    /// 数据类测试脚本
    /// 用于验证Segment、CardData、LevelConfigSO是否正常工作
    /// </summary>
    public class DataTest : MonoBehaviour
    {
        [Header("测试配置")]
        public LevelConfigSO testLevelConfig;

        [Header("测试数据")]
        public CardDataContainer testCardContainer;

        void Start()
        {
            Debug.Log("=== 数据类测试开始 ===");

            // 测试LevelConfigSO
            if (testLevelConfig != null)
            {
                Debug.Log($"关卡ID: {testLevelConfig.levelId}");
                Debug.Log($"关卡名称: {testLevelConfig.levelName}");
                Debug.Log($"时间限制: {testLevelConfig.timeLimit}秒");
                Debug.Log($"初始慌乱值: {testLevelConfig.initialPanic}");
                Debug.Log($"初始兴奋值: {testLevelConfig.initialExcite}");
                Debug.Log($"临界值: {testLevelConfig.criticalValue}");
                Debug.Log($"最大生命值: {testLevelConfig.maxLives}");
            }
            else
            {
                Debug.LogWarning("未设置testLevelConfig，请在Inspector中配置");
            }

            // 测试CardData
            if (testCardContainer != null && testCardContainer.cardData != null)
            {
                CardData testCardData = testCardContainer.cardData;
                Debug.Log($"=== 卡牌数据测试 ===");
                Debug.Log($"卡牌ID: {testCardData.id}");
                Debug.Log($"卡牌名称: {testCardData.cardName}");
                Debug.Log($"卡牌描述: {testCardData.description}");
                Debug.Log($"总时长: {testCardData.CalculateTotalDuration()}秒");
                Debug.Log($"片段数量: {testCardData.segments.Count}");

                // 测试CardInstance
                CardInstance instance = new CardInstance(testCardData, 1);
                Debug.Log($"实例ID: {instance.instanceId}");
                Debug.Log($"是否可打断: {instance.CanInterrupt()}");

                Segment currentSeg = instance.GetCurrentSegment();
                if (currentSeg != null)
                {
                    Debug.Log($"当前片段: 时长={currentSeg.duration}, 可打断={currentSeg.isInterruptible}");
                }
            }
            else
            {
                Debug.LogWarning("未设置testCardContainer，请在Inspector中配置");
            }

            Debug.Log("=== 数据类测试完成 ===");
        }
    }
}
