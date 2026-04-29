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
        [MenuItem("Tools/《灯下黑》/创建测试数据")]
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

            // 创建卡牌数据
            CardData cardData = new CardData
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

            // 创建卡牌数据容器
            CardDataContainer cardContainer = ScriptableObject.CreateInstance<CardDataContainer>();
            cardContainer.cardData = cardData;

            AssetDatabase.CreateAsset(cardContainer, $"{folderPath}/TestCardData.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"测试数据已创建到: {folderPath}");
            Debug.Log("- TestLevelConfig: 关卡配置");
            Debug.Log("- TestCardData: 测试卡牌（翻身）");
        }
    }
}
