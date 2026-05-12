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
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE",
                1, 5002, "DXH_SOUND/04.移动被子");

            CreateCard(folder, 2007, "撕开薯片袋", "打开袋子可以吃薯片",
                "送明月的专属零食，市面上少见的款式，据说吃了它心情会变好。", CardType.NonStackable, 1,
                new[] { new Segment(2f, true), new Segment(1f, false) },
                3, 2, 5, RelatedType.NonPersistent, new[] { 2008, 2009, 2010, 2011 }, 0,
                "1001", BedStateChange.None, false, "", 0, 0, "DXH_SOUND/02.撕开薯片袋");

            CreateCard(folder, 2008, "吃薯片", "食用薯片，完成任务",
                "好吃，咸咸的，很脆", CardType.Stackable, 9,
                new[] { new Segment(4f, true) },
                4, 2, 5, RelatedType.NonPersistent, new[] { 2009, 2010, 2011 }, 3001,
                "1001", BedStateChange.None, true,
                "当此卡牌被手动打断时，不会因为打断卡牌而重置读条进度", 0, 0, "DXH_SOUND/03.吃薯片");

            CreateCard(folder, 2009, "喝水", "喝水降低兴奋值和慌乱值",
                "一口水下去，感觉冷静下来了。", CardType.Stackable, 3,
                new[] { new Segment(1f, true), new Segment(1f, false), new Segment(1f, true) },
                3, -3, -5, RelatedType.NonPersistent, new int[0], 0,
                "all", BedStateChange.None, false, "", 0, 0, "DXH_SOUND/05.喝水");

            CreateCard(folder, 2010, "深呼吸", "降低慌乱值和兴奋值",
                "深呼吸，没什么好笑的，也没什么好怕的……", CardType.Stackable, 10,
                new[] { new Segment(3f, true) },
                0, -2, -3, RelatedType.NonPersistent, new int[0], 0,
                "all", BedStateChange.None, false, "", 0, 0, "DXH_SOUND/06.深呼吸");

            CreateCard(folder, 2011, "盖回被子", "盖回被子中，回到装睡状态",
                "身体融入被子中，还是盖着被子有安全感……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                4, -4, -2, RelatedType.Persistent, new[] { 2001, 2010 }, 0,
                "all", BedStateChange.EnterBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，回到被子中，这个值就会变成TRUE",
                1, 5001, "DXH_SOUND/04.移动被子");

            AssetDatabase.Refresh();
            Debug.Log("[CardDataGenerator] 6 张卡牌生成完毕 → Assets/Resources/Card/");
        }

        [MenuItem("Tools/生成第二夜卡牌")]
        public static void GenerateSecondNightCards()
        {
            string folder = "Assets/Resources/Card_Level2/";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Resources", "Card_Level2");

            // 2001 掀开被子
            CreateCard(folder, 2001, "掀开被子", "查看被中零食/道具，离开被子",
                "被子中有什么东西来着？掀开看看……", CardType.NonStackable, 1,
                new[] { new Segment(1f, true), new Segment(1f, false) },
                3, 5, 0, RelatedType.Persistent, new[] { 2009, 2010, 2011 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE",
                1, 5002, "");

            // 2002 看向床下
            CreateCard(folder, 2002, "看向床下", "查看床下的零食/道具，离开被子",
                "拌面在床下，可别忘了吃……", CardType.NonStackable, 1,
                new[] { new Segment(1f, true), new Segment(1f, false) },
                0, 4, 2, RelatedType.Persistent, new[] { 2010, 2011, 2012 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE。",
                1, 5003, "");

            // 2003 看向床对面
            CreateCard(folder, 2003, "看向床对面", "转为床对面视角，可以看到舍友，也有可能和舍友聊天。仍然为在被子中状态。",
                "想换个姿势睡觉……", CardType.NonStackable, 1,
                new[] { new Segment(3f, true) },
                0, 3, 2, RelatedType.Persistent, new[] { 2001, 2002, 2010 }, 0,
                "1002", BedStateChange.None, false, "无",
                1, 5004, "");

            // 2009 喝水
            CreateCard(folder, 2009, "喝水", "喝水降低兴奋值和慌乱值",
                "一口水下去，感觉冷静下来了。", CardType.Stackable, 3,
                new[] { new Segment(1f, true), new Segment(1f, false), new Segment(1f, true) },
                0, -2, -5, RelatedType.NonPersistent, new[] { 2010, 2011 }, 0,
                "all", BedStateChange.None, false, "无",
                0, 0, "");

            // 2010 深呼吸
            CreateCard(folder, 2010, "深呼吸", "降低慌乱值和兴奋值",
                "深呼吸，没什么好笑的，也没什么好怕的……", CardType.Stackable, 10,
                new[] { new Segment(3f, true) },
                0, -2, -3, RelatedType.NonPersistent, new int[0], 0,
                "all", BedStateChange.None, false,
                "本张卡使用后，备选取的其他卡牌的显示保持不变，不按照此卡牌的关联列表生成卡牌，此卡牌按照规则减少一层数。",
                0, 0, "");

            // 2011 盖回被子
            CreateCard(folder, 2011, "盖回被子", "盖回被子中，回到装睡状态",
                "身体融入被子中，还是盖着被子有安全感……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                4, -4, -2, RelatedType.Persistent, new[] { 2001, 2002, 2003, 2010 }, 0,
                "all", BedStateChange.EnterBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，回到被子中，这个值就会变成TRUE",
                1, 5001, "");

            // 2012 倒掉泡面水
            CreateCard(folder, 2012, "倒掉泡面水", "倒掉面中的水在水瓶中",
                "得先倒掉水才能加入调料……", CardType.NonStackable, 1,
                new[] { new Segment(10f, true) },
                0, 6, 1, RelatedType.NonPersistent, new[] { 2010, 2011, 2013, 2014 }, 0,
                "1002", BedStateChange.None, false, "无",
                0, 0, "");

            // 2013 加入酱包
            CreateCard(folder, 2013, "加入酱包", "制作拌面的一步。",
                "加入火鸡面酱，全部加入！", CardType.NonStackable, 1,
                new[] { new Segment(4f, true) },
                0, 2, 3, RelatedType.NonPersistent, new[] { 2010, 2011, 2014 }, 0,
                "1002", BedStateChange.None, false,
                "使用完成后检测:如果卡牌ID 2014已经使用过，则在生成关联卡牌的基础上生成卡牌ID为2015的卡牌。",
                0, 0, "");

            // 2014 加入海苔包
            CreateCard(folder, 2014, "加入海苔包", "制作拌面的一步。",
                "加入海苔包，增加一些口感。", CardType.NonStackable, 1,
                new[] { new Segment(4f, true) },
                0, 1, 3, RelatedType.NonPersistent, new[] { 2010, 2011, 2013 }, 0,
                "1002", BedStateChange.None, false,
                "使用完成后检测:如果卡牌ID 2013已经使用过，则在生成关联卡牌的基础上生成卡牌ID为2015的卡牌。",
                0, 0, "");

            // 2015 搅拌拌面
            CreateCard(folder, 2015, "搅拌拌面", "制作拌面的一步，使用后可以获得吃拌面卡。",
                "搅拌搅拌准备开吃……", CardType.NonStackable, 1,
                new[] { new Segment(2f, false), new Segment(4f, true) },
                0, 3, 3, RelatedType.NonPersistent, new[] { 2010, 2011, 2016, 2017 }, 0,
                "1002", BedStateChange.None, false,
                "当使用这张卡牌后，将卡牌ID为2017的卡牌设置为卡牌ID为2003的关联卡牌。",
                0, 0, "");

            // 2016 吃拌面
            CreateCard(folder, 2016, "吃拌面", "吃完泡面完成吃泡面任务目标",
                "终于可以开始吃了……", CardType.Stackable, 5,
                new[] { new Segment(4f, true), new Segment(2f, false) },
                0, 2, 5, RelatedType.NonPersistent, new[] { 2010, 2011 }, 3002,
                "1002", BedStateChange.None, false, "无",
                0, 0, "");

            // 2017 分享拌面
            CreateCard(folder, 2017, "分享拌面", "向宋明月分享拌面，触发对话。",
                "也许该给宋明月分享一些，报答她的薯片，她应该还没睡……", CardType.NonStackable, 1,
                new[] { new Segment(5f, true) },
                0, 3, 5, RelatedType.NonPersistent, new int[0], 3003,
                "1002", BedStateChange.None, false,
                "此卡牌使用后不改变备选区的卡牌（除卡牌ID2016的卡牌），在备选区生成卡牌ID为2018的新卡牌。卡牌ID2016的卡牌减少一层，卡牌ID2016的卡牌暂时暂停生成逻辑。",
                0, 0, "");

            // 2018 拿回拌面
            CreateCard(folder, 2018, "拿回拌面", "从宋明月那里拿回拌面",
                "该拿回来接着吃了，宋明月似乎很开心……", CardType.NonStackable, 1,
                new[] { new Segment(3f, true), new Segment(3f, false) },
                0, 5, 2, RelatedType.NonPersistent, new int[0], 0,
                "1002", BedStateChange.None, false,
                "此卡牌使用后不改变备选区的卡牌（除卡牌ID2016的卡牌），卡牌ID2016的卡牌恢复生成逻辑，保持卡牌层数与暂停生成前一致。",
                0, 0, "");

            AssetDatabase.Refresh();
            Debug.Log("[CardDataGenerator] 13 张第二夜卡牌生成完毕 → Assets/Resources/Card_Level2/");
        }

        [MenuItem("Tools/生成第三夜卡牌")]
        public static void GenerateThirdNightCards()
        {
            string folder = "Assets/Resources/Card_Level3/";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Resources", "Card_Level3");

            // 2001 掀开被子
            CreateCard(folder, 2001, "掀开被子", "查看被中零食/道具，离开被子",
                "被子中有什么东西来着？掀开看看……", CardType.NonStackable, 1,
                new[] { new Segment(1f, true), new Segment(1f, false) },
                0, 5, 0, RelatedType.Persistent, new[] { 2009, 2010, 2011 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE",
                1, 5002, "");

            // 2004 拿出手机
            CreateCard(folder, 2004, "拿出手机", "拿出枕头下的手机，变为会被发现的危险状态",
                "无", CardType.NonStackable, 1,
                new[] { new Segment(2f, true), new Segment(1f, false) },
                0, 3, 2, RelatedType.Persistent, new[] { 2010, 2019, 2029 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，拿出了手机，这个值就会变成FALSE",
                1, 0, "");

            // 2005 下床
            CreateCard(folder, 2005, "下床", "下床后可以去到别处，变为会被发现的危险状态",
                "下床有点危险，好害怕……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                0, 2, 0, RelatedType.Persistent, new[] { 2006, 2010, 2030 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE",
                1, 5005, "");

            // 2006 前往储物柜
            CreateCard(folder, 2006, "前往储物柜", "前往储物柜，从柜子中可以拿取物品。",
                "面包应该在柜子里面，去拿一下吧……", CardType.NonStackable, 1,
                new[] { new Segment(3f, true) },
                0, 3, 0, RelatedType.Persistent, new[] { 2010, 2024, 2030 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE",
                1, 5005, "");

            // 2009 喝水
            CreateCard(folder, 2009, "喝水", "喝水降低兴奋值和慌乱值",
                "一口水下去，感觉冷静下来了。", CardType.Stackable, 3,
                new[] { new Segment(1f, true), new Segment(1f, false), new Segment(1f, true) },
                0, -2, -5, RelatedType.NonPersistent, new int[0], 0,
                "all", BedStateChange.None, false, "无",
                0, 0, "");

            // 2010 深呼吸
            CreateCard(folder, 2010, "深呼吸", "降低慌乱值和兴奋值",
                "深呼吸，没什么好笑的，也没什么好怕的……", CardType.Stackable, 10,
                new[] { new Segment(3f, true) },
                0, -2, -3, RelatedType.NonPersistent, new int[0], 0,
                "all", BedStateChange.None, false,
                "本张卡使用后，备选取的其他卡牌的显示保持不变，不按照此卡牌的关联列表生成卡牌，此卡牌按照规则减少一层数。",
                0, 0, "");

            // 2011 盖回被子
            CreateCard(folder, 2011, "盖回被子", "盖回被子中，回到装睡状态",
                "身体融入被子中，还是盖着被子有安全感……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                0, -4, -2, RelatedType.Persistent, new[] { 2001, 2004, 2005, 2010 }, 0,
                "all", BedStateChange.EnterBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，回到被子中，这个值就会变成TRUE",
                1, 5001, "");

            // 2019 点亮手机屏幕
            CreateCard(folder, 2019, "点亮手机屏幕", "亮起手机屏幕，可以操作手机中的软件",
                "已经是最低亮度了，怎么还这么亮……", CardType.NonStackable, 1,
                new[] { new Segment(1f, true) },
                0, 2, 0, RelatedType.Persistent, new[] { 2010, 2020, 2021, 2022 }, 0,
                "1003", BedStateChange.None, false, "无",
                0, 0, "");

            // 2020 打游戏
            CreateCard(folder, 2020, "打游戏", "打游戏时中间段会有4秒不可打断时间",
                "这几个游戏好久没有玩了，看看有什么新东西……", CardType.Stackable, 3,
                new[] { new Segment(2f, true), new Segment(4f, false), new Segment(2f, true) },
                0, 3, 5, RelatedType.NonPersistent, new[] { 2010, 2020, 2021, 2022 }, 0,
                "1003", BedStateChange.None, false, "无",
                0, 0, "");

            // 2021 刷视频
            CreateCard(folder, 2021, "刷视频", "刷视频会入迷，后半段有4秒不可打断区",
                "刷刷视频放松一下吧……", CardType.Stackable, 5,
                new[] { new Segment(2f, true), new Segment(4f, false) },
                0, 2, 4, RelatedType.NonPersistent, new[] { 2010, 2020, 2021, 2022 }, 0,
                "1003", BedStateChange.None, false, "无",
                0, 0, "");

            // 2022 熄灭手机屏幕
            CreateCard(folder, 2022, "熄灭手机屏幕", "熄灭手机屏幕后可以隐藏手机。",
                "这下可以放心放回枕头下了……", CardType.NonStackable, 1,
                new[] { new Segment(1f, true) },
                0, 0, 2, RelatedType.Persistent, new[] { 2010, 2019, 2029 }, 0,
                "1003", BedStateChange.None, false, "无",
                0, 0, "");

            // 2024 打开储物柜
            CreateCard(folder, 2024, "打开储物柜", "柜子中有物品",
                "面包应该在里面，开柜子声音不会很大吧……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                0, 2, 1, RelatedType.Persistent, new[] { 2010, 2025, 2026, 2030 }, 0,
                "1003", BedStateChange.None, false, "无",
                0, 0, "");

            // 2025 吃面包
            CreateCard(folder, 2025, "吃面包", "吃掉面包所用时间12秒，使用时每2秒增加一点兴奋值，打断后不会重置使用时间",
                "里面是奶油馅的，很好吃……", CardType.NonStackable, 1,
                new[] { new Segment(12f, true) },
                0, 2, 6, RelatedType.NonPersistent, new int[0], 3006,
                "1003", BedStateChange.None, true,
                "当此卡牌被手动打断时，不会因为打断卡牌而重置读条进度，也就是说，当下一次将此卡牌拖动到卡牌框中时，会接续上一次进度。使用时每2秒增加一点兴奋值。慌乱值增加是使用后结算。本张卡使用后，备选取的其他卡牌的显示保持不变，不按照此卡牌的关联列表生成卡牌。使用卡牌后，将卡牌ID为2026的卡牌变为已使用直接隐藏，不再生成。",
                0, 0, "");

            // 2026 拿走面包
            CreateCard(folder, 2026, "拿走面包", "拿走面包，可以掀开被子后查看食用",
                "拿回被子里吃吧，在这里有点危险……", CardType.NonStackable, 1,
                new[] { new Segment(2f, false) },
                0, 1, 2, RelatedType.NonPersistent, new[] { 2010, 2030 }, 0,
                "1003", BedStateChange.None, false,
                "使用完成后检查，如果是卡牌ID为2025的卡牌没有被使用过，则将卡牌ID为2025的卡牌改为卡牌ID为2001的关联卡牌。",
                0, 0, "");

            // 2029 隐藏手机
            CreateCard(folder, 2029, "隐藏手机", "将手机放回枕头下，变为安全状态。",
                "塞到枕头下面好像很有安全感……", CardType.NonStackable, 1,
                new[] { new Segment(1f, true) },
                0, 0, 2, RelatedType.Persistent, new[] { 2001, 2004, 2005, 2010 }, 0,
                "1003", BedStateChange.EnterBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，回到被子中，这个值就会变成TRUE",
                1, 0, "");

            // 2030 返回床上
            CreateCard(folder, 2030, "返回床上", "从宿舍其他位置返回床上，变为安全状态。",
                "还是躺在被子里有安全感……", CardType.NonStackable, 1,
                new[] { new Segment(4f, true) },
                0, 2, 3, RelatedType.Persistent, new[] { 2001, 2004, 2005, 2010 }, 0,
                "1003", BedStateChange.EnterBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，回到被子中，这个值就会变成TRUE",
                1, 5001, "");

            AssetDatabase.Refresh();
            Debug.Log("[CardDataGenerator] 16 张第三夜卡牌生成完毕 → Assets/Resources/Card_Level3/");
        }

        [MenuItem("Tools/生成第五夜卡牌")]
        public static void GenerateFifthNightCards()
        {
            string folder = "Assets/Resources/Card_Level5/";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Resources", "Card_Level5");

            // 2001 掀开被子
            CreateCard(folder, 2001, "掀开被子", "查看被中零食/道具，离开被子",
                "被子中有什么东西来着？掀开看看……", CardType.NonStackable, 1,
                new[] { new Segment(1f, true), new Segment(1f, false) },
                0, 5, 0, RelatedType.Persistent, new[] { 2009, 2010, 2011 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE",
                1, 5002, "");

            // 2003 看向床对面
            CreateCard(folder, 2003, "看向床对面", "转为床对面视角，可以看到舍友，也有可能和舍友聊天。仍然为在被子中状态。",
                "想换个姿势睡觉……", CardType.NonStackable, 1,
                new[] { new Segment(3f, true) },
                0, 3, 2, RelatedType.Persistent, new[] { 2001, 2005, 2010 }, 0,
                "1005", BedStateChange.None, false, "无",
                1, 5006, "");

            // 2005 下床
            CreateCard(folder, 2005, "下床", "下床后可以去到别处，变为会被发现的危险状态",
                "下床有点危险，好害怕……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                0, 2, 0, RelatedType.Persistent, new[] { 2006, 2010, 2030 }, 0,
                "all", BedStateChange.LeaveBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，离开被子，这个值就会变成FALSE",
                1, 5007, "");

            // 2006 前往储物柜
            CreateCard(folder, 2006, "前往储物柜", "前往储物柜，从柜子中可以拿取物品。",
                "面包应该在柜子里面，去拿一下吧……", CardType.NonStackable, 1,
                new[] { new Segment(3f, true) },
                0, 3, 0, RelatedType.Persistent, new[] { 2010, 2024, 2030 }, 0,
                "all", BedStateChange.LeaveBed, false, "",
                1, 5008, "");

            // 2009 喝水
            CreateCard(folder, 2009, "喝水", "喝水降低兴奋值和慌乱值",
                "一口水下去，感觉冷静下来了。", CardType.Stackable, 3,
                new[] { new Segment(1f, true), new Segment(1f, false), new Segment(1f, true) },
                0, -2, -5, RelatedType.NonPersistent, new int[0], 0,
                "all", BedStateChange.None, false,
                "本张卡使用后，备选取的其他卡牌的显示保持不变，不按照此卡牌的关联列表生成卡牌，此卡牌按照规则减少一层数。",
                0, 0, "");

            // 2010 深呼吸
            CreateCard(folder, 2010, "深呼吸", "降低慌乱值和兴奋值",
                "深呼吸，没什么好笑的，也没什么好怕的……", CardType.Stackable, 10,
                new[] { new Segment(3f, true) },
                0, -2, -3, RelatedType.NonPersistent, new int[0], 0,
                "all", BedStateChange.None, false,
                "本张卡使用后，备选取的其他卡牌的显示保持不变，不按照此卡牌的关联列表生成卡牌，此卡牌按照规则减少一层数。",
                0, 0, "");

            // 2011 盖回被子
            CreateCard(folder, 2011, "盖回被子", "盖回被子中，回到装睡状态",
                "身体融入被子中，还是盖着被子有安全感……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                0, -4, -2, RelatedType.Persistent, new[] { 2001, 2003, 2005, 2010 }, 0,
                "all", BedStateChange.EnterBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，回到被子中，这个值就会变成TRUE",
                1, 5001, "");

            // 2024 打开储物柜
            CreateCard(folder, 2024, "打开储物柜", "柜子中有物品",
                "面包应该在里面，开柜子声音不会很大吧……", CardType.NonStackable, 1,
                new[] { new Segment(2f, true) },
                0, 2, 1, RelatedType.Persistent, new[] { 2010, 2025, 2030 }, 0,
                "1005", BedStateChange.None, false, "无",
                0, 5009, "");

            // 2025 拿走卫生纸
            CreateCard(folder, 2025, "拿走卫生纸", "拿走卫生纸，解锁去厕所的行动卡",
                "拿到卫生纸了，终于可以去厕所了。", CardType.NonStackable, 1,
                new[] { new Segment(4f, false) },
                0, 4, 1, RelatedType.NonPersistent, new[] { 2010, 2030, 2040 }, 0,
                "1005", BedStateChange.None, false,
                "将卡牌ID为2040的卡牌设置为卡牌ID为2005的关联卡牌。",
                0, 5009, "");

            // 2026 寻求宋明月帮助
            CreateCard(folder, 2026, "寻求宋明月帮助", "向宋明月寻求帮助，获得卫生纸",
                "宋明月那里应该有卫生纸,问问她吧……", CardType.NonStackable, 1,
                new[] { new Segment(6f, true) },
                0, 4, 3, RelatedType.Persistent, new[] { 2001, 2005, 2010 }, 0,
                "1005", BedStateChange.None, false,
                "将卡牌ID为2040的卡牌设置为卡牌ID为2005的关联卡牌。",
                0, 0, "");

            // 2030 返回床上
            CreateCard(folder, 2030, "返回床上", "从宿舍其他位置返回床上，变为安全状态。",
                "还是躺在被子里有安全感……", CardType.NonStackable, 1,
                new[] { new Segment(4f, true) },
                0, 2, 3, RelatedType.Persistent, new[] { 2001, 2003, 2005, 2010 }, 0,
                "1005", BedStateChange.EnterBed, false,
                "本张卡为老师查寝判定条件之一的回到被子中的值的改变卡，当使用这张卡时，回到被子中，这个值就会变成TRUE",
                1, 5001, "");

            // 2038 前往厕所
            CreateCard(folder, 2038, "前往厕所", "奔向厕所",
                "厕所我来了……", CardType.NonStackable, 1,
                new[] { new Segment(5f, true) },
                0, 3, 3, RelatedType.Persistent, new int[0], 3009,
                "1005", BedStateChange.None, false, "无",
                1, 5010, "");

            // 2040 前往走廊
            CreateCard(folder, 2040, "前往走廊", "前往宿舍门外",
                "终于要出宿舍了吗，不管了……", CardType.NonStackable, 1,
                new[] { new Segment(5f, true) },
                0, 0, 6, RelatedType.Persistent, new[] { 2038, 2010 }, 0,
                "1005", BedStateChange.None, false,
                "使用这张卡牌后将暂停老师的查寝逻辑。",
                1, 5010, "");

            AssetDatabase.Refresh();
            Debug.Log("[CardDataGenerator] 13 张第五夜卡牌生成完毕 → Assets/Resources/Card_Level5/");
        }

        private static void CreateCard(string folder, int id, string name, string effect,
            string desc, CardType type, int stack, Segment[] segments,
            int interruptPanic, int panicDelta, int exciteDelta,
            RelatedType relatedType, int[] relatedIds, int taskId,
            string levelIds, BedStateChange bed, bool saveProgress, string special,
            int cardBgType, int bgJumpId, string sfxName)
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
                specialEffect = special,
                cardBackgroundType = cardBgType,
                backgroundJumpId = bgJumpId,
                sfxName = sfxName
            };

            var container = ScriptableObject.CreateInstance<CardDataContainer>();
            container.cardData = data;

            string path = $"{folder}Card_{id}_{name}.asset";
            // 删除旧资产，确保数据更新
            var existing = AssetDatabase.LoadAssetAtPath<CardDataContainer>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(container, path);
        }
    }
}
