using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Game.Data;
using Game.Config;

namespace Game.Editor
{
    /// <summary>
    /// 测试数据创建工具
    /// 在Unity编辑器菜单中创建测试资产
    /// </summary>
    public class TestDataCreator
    {
        [MenuItem("Tools/《灯下黑》/📦 创建测试数据")]
        public static void CreateTestData()
        {
            string folderPath = "Assets/Resources/TestData";

            // 创建文件夹
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder(parent, "TestData");
            }

            // 创建关卡配置
            LevelConfigSO levelConfig = ScriptableObject.CreateInstance<LevelConfigSO>();
            levelConfig.levelId = 1;
            levelConfig.levelName = "测试关卡";
            levelConfig.timeLimit = 300;
            levelConfig.initialPanic = 35;
            levelConfig.initialExcite = 35;
            levelConfig.criticalValue = 100;
            levelConfig.patrolIntervals = new Vector2(5f, 10f);
            levelConfig.patrolTime = new Vector2(3f, 5f);
            levelConfig.eyeCheckDuration = new Vector2(2f, 3f);
            levelConfig.flashCheckDuration = new Vector2(3f, 5f);
            levelConfig.flashPanicPerSec = 10;
            levelConfig.initialCards = new List<int> { 1, 2, 3 };
            levelConfig.maxLives = 2;

            AssetDatabase.CreateAsset(levelConfig, $"{folderPath}/TestLevelConfig.asset");

            // 创建卡牌1：翻身
            CardData card1 = new CardData
            {
                id = 1,
                cardName = "翻身",
                description = "在床上翻个身，降低少量兴奋值",
                iconPath = "Icons/card_turnover",
                segments = new List<Segment>
                {
                    new Segment(1f, true),
                    new Segment(1f, false)
                },
                panicDelta = 0,
                exciteDelta = -5,
                interruptPanicAdd = 10
            };
            CardDataContainer cardContainer1 = ScriptableObject.CreateInstance<CardDataContainer>();
            cardContainer1.cardData = card1;
            AssetDatabase.CreateAsset(cardContainer1, $"{folderPath}/Card1_TurnOver.asset");

            // 创建卡牌2：深呼吸
            CardData card2 = new CardData
            {
                id = 2,
                cardName = "深呼吸",
                description = "深呼吸放松，降低慌乱值",
                iconPath = "Icons/card_breathe",
                segments = new List<Segment>
                {
                    new Segment(2f, true),
                    new Segment(2f, false)
                },
                panicDelta = -10,
                exciteDelta = 0,
                interruptPanicAdd = 15
            };
            CardDataContainer cardContainer2 = ScriptableObject.CreateInstance<CardDataContainer>();
            cardContainer2.cardData = card2;
            AssetDatabase.CreateAsset(cardContainer2, $"{folderPath}/Card2_DeepBreath.asset");

            // 创建卡牌3：假寐
            CardData card3 = new CardData
            {
                id = 3,
                cardName = "假寐",
                description = "假装睡觉，大幅降低情绪值但风险较高",
                iconPath = "Icons/card_fakesleep",
                segments = new List<Segment>
                {
                    new Segment(1.5f, true),
                    new Segment(3f, false),
                    new Segment(1.5f, true)
                },
                panicDelta = -15,
                exciteDelta = -10,
                interruptPanicAdd = 20
            };
            CardDataContainer cardContainer3 = ScriptableObject.CreateInstance<CardDataContainer>();
            cardContainer3.cardData = card3;
            AssetDatabase.CreateAsset(cardContainer3, $"{folderPath}/Card3_FakeSleep.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"测试数据已创建到: {folderPath}");
            Debug.Log("- TestLevelConfig: 关卡配置");
            Debug.Log("- Card1_TurnOver: 翻身卡牌");
            Debug.Log("- Card2_DeepBreath: 深呼吸卡牌");
            Debug.Log("- Card3_FakeSleep: 假寐卡牌");
        }
    }
}
