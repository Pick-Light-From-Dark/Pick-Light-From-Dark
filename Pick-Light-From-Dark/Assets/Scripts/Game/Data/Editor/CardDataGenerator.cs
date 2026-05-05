using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Game.Data;

namespace Game.Data.Editor
{
    /// <summary>
    /// 从"第一夜关卡.txt"数据一键生成 6 张卡牌 ScriptableObject
    /// 菜单: Tools → 生成第一夜卡牌
    /// </summary>
    public static class CardDataGenerator
    {
        [MenuItem("Tools/生成第一夜卡牌")]
        public static void GenerateFirstNightCards()
        {
            string folder = "Assets/Resources/Card/";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Resources", "Card");

            CreateCard(folder, 2001, "掀开被子", "查看被中零食/道具，离开被子",
                "被子中有什么东西来着？掀开看看……", CardType.NonStackable, 1,
                new[] { new Segment(1f, true), new Segment(1f, false) },
                3, 5, 0, RelatedType.Persistent, new[] { 2007, 2009, 2010, 2011 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE");

            CreateCard(folder, 2007, "撕开薯片袋", "打开袋子可以吃薯片",
                "送明月的专属零食，市面上少见的款式，据说吃了它心情会变好。", CardType.NonStackable, 1,
                new[] { new Segment(2f, true), new Segment(1f, false) },
                3, 2, 5, RelatedType.NonPersistent, new[] { 2008, 2009, 2010, 2011 }, 0,
                "1001", BedStateChange.None, false, "");

            CreateCard(folder, 2008, "吃薯片", "食用薯片，完成任务",
                "好吃，咸咸的，很脆", CardType.Stackable, 9,
                new[] { new Segment(4f, true) },
                4, 2, 5, RelatedType.NonPersistent, new[] { 2009, 2010, 2011 }, 3001,
                "1001", BedStateChange.None, true,
                "当此卡牌被手动打断时，不会因为打断卡牌而重置读条进度");

            CreateCard(folder, 2009, "喝水", "喝水降低兴奋值和慌乱值",
                "一口水下去，感觉冷静下来了。", CardType.Stackable, 3,
                new[] { new Segment(1f, true), new Segment(1f, false), new Segment(1f, true) },
                3, -3, -5, RelatedType.NonPersistent, new[] { 2007, 2010, 2011 }, 0,
                "all", BedStateChange.None, false, "");

            CreateCard(folder, 2010, "深呼吸", "降低慌乱值和兴奋值",
                "深呼吸，没什么好笑的，也没什么好怕的……", CardType.Stackable, 10,
                new[] { new Segment(3f, true) },
                0, -2, -3, RelatedType.NonPersistent, new[] { 2007, 2009, 2010, 2011 }, 0,
                "all", BedStateChange.None, false, "");

            CreateCard(folder, 2011, "盖回被子", "盖回被子中，回到装睡状态",
                "身体融入被子中，还是盖着被子有安全感……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                4, -4, -2, RelatedType.Persistent, new[] { 2001, 2010 }, 0,
                "all", BedStateChange.EnterBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，回到被子中，这个值就会变成TRUE");

            AssetDatabase.Refresh();
            Debug.Log("[CardDataGenerator] 6 张卡牌生成完毕 → Assets/Resources/Card/");
        }

        private static void CreateCard(string folder, int id, string name, string effect,
            string desc, CardType type, int stack, Segment[] segments,
            int interruptPanic, int panicDelta, int exciteDelta,
            RelatedType relatedType, int[] relatedIds, int taskId,
            string levelIds, BedStateChange bed, bool saveProgress, string special)
        {
            var data = new CardData
            {
                id = id,
                cardName = name,
                description = $"【作用】{effect}\n【描述】{desc}",
                cardType = type,
                initialStack = stack,
                segments = new List<Segment>(segments),
                interruptPanicAdd = interruptPanic,
                panicDelta = panicDelta,
                exciteDelta = exciteDelta,
                relatedType = relatedType,
                relatedCardIds = new List<int>(relatedIds),
                bindTaskId = taskId,
                allowedLevelIds = levelIds == "all"
                    ? new List<string> { "all" }
                    : new List<string> { levelIds },
                bedStateChange = bed,
                saveProgressOnInterrupt = saveProgress,
                specialEffect = special
            };

            var container = ScriptableObject.CreateInstance<CardDataContainer>();
            container.cardData = data;

            string path = $"{folder}Card_{id}_{name}.asset";
            AssetDatabase.CreateAsset(container, path);
        }
    }
}
