using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    /// <summary>
    /// 卡牌放置区域 — 卡牌拖到这里松手后留在该位置
    /// </summary>
    public class CardDropZone : MonoBehaviour, IDropHandler
    {
        public static event Action<CardSlot> OnCardDropped;

        public void OnDrop(PointerEventData eventData)
        {
            var card = CardSlot.CurrentDragging;
            if (card == null) return;

            // 已有卡牌则拒绝
            if (transform.childCount > 0)
                return;

            card.AcceptDrop(transform);
            OnCardDropped?.Invoke(card);

            // 关闭已放入卡牌的射线
            var cg = card.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = false;
            var img = card.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = false;
        }
    }
}
