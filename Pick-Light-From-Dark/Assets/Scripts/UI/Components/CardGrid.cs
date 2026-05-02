using System;
using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Card;

namespace Game.UI
{
    /// <summary>
    /// 卡牌网格组件
    /// 显示和管理手牌
    /// </summary>
    public class CardGrid : MonoBehaviour
    {
        [Header("预制体")]
        [SerializeField] private GameObject cardSlotPrefab;

        [Header("配置")]
        [SerializeField] private int maxDisplayCount = 10;
        [SerializeField] private int columnCount = 2;

        private List<CardSlot> cardSlots = new List<CardSlot>();
        private CardManager cardManager;
        private Action<CardInstance> onCardSelected;

        void Start()
        {
            // 获取卡牌管理器引用
            cardManager = CardManager.Instance;

            // 初始加载手牌
            LoadHandCards();
        }

        void OnDestroy()
        {
            // 暂时不监听事件
        }

        /// <summary>
        /// 设置卡牌选中回调
        /// </summary>
        public void SetCardSelectedCallback(global::System.Action<CardInstance> callback)
        {
            onCardSelected = callback;
        }

        /// <summary>
        /// 加载手牌
        /// </summary>
        private void LoadHandCards()
        {
            if (cardManager == null) return;

            var handCards = cardManager.GetHandCards();
            RefreshCardGrid(handCards);
        }

        /// <summary>
        /// 刷新卡牌网格显示
        /// </summary>
        private void RefreshCardGrid(List<CardInstance> cards)
        {
            // 清空现有卡牌槽
            ClearCardSlots();

            // 创建新的卡牌槽
            for (int i = 0; i < cards.Count && i < maxDisplayCount; i++)
            {
                CreateCardSlot(cards[i]);
            }
        }

        /// <summary>
        /// 创建卡牌槽
        /// </summary>
        private void CreateCardSlot(CardInstance card)
        {
            if (cardSlotPrefab == null) return;

            GameObject slotObj = Instantiate(cardSlotPrefab, transform);
            CardSlot slot = slotObj.GetComponent<CardSlot>();

            if (slot != null)
            {
                slot.SetCard(card, onCardSelected);
                cardSlots.Add(slot);
            }
        }

        /// <summary>
        /// 清空卡牌槽
        /// </summary>
        private void ClearCardSlots()
        {
            foreach (var slot in cardSlots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    Destroy(slot.gameObject);
                }
            }

            cardSlots.Clear();
        }

        /// <summary>
        /// 设置卡牌槽预制体
        /// </summary>
        public void SetCardSlotPrefab(GameObject prefab)
        {
            cardSlotPrefab = prefab;
        }

        /// <summary>
        /// 获取当前显示的卡牌槽数量
        /// </summary>
        public int GetCardSlotCount()
        {
            return cardSlots.Count;
        }
    }
}
