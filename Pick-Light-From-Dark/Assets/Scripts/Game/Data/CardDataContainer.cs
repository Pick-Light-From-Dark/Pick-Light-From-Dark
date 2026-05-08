using UnityEngine;
using Game.Data;

namespace Game.Data
{
    /// <summary>
    /// 单张卡牌的 ScriptableObject 配置
    /// 在 Project 窗口右键 → Create → Game → CardConfig 创建
    /// </summary>
    [CreateAssetMenu(fileName = "Card_", menuName = "Game/CardConfig", order = 1)]
    public class CardDataContainer : ScriptableObject
    {
        public CardData cardData;
    }
}
