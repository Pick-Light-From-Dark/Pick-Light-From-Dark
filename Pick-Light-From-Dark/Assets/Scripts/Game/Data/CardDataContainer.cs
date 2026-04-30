using UnityEngine;
using Game.Data;

namespace Game.Data
{
    /// <summary>
    /// 卡牌数据容器（用于在Unity中序列化CardData）
    /// </summary>
    public class CardDataContainer : ScriptableObject
    {
        public CardData cardData;
    }
}
