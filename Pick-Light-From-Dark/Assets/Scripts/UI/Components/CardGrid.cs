using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.Data;

namespace Game.UI
{
    public class CardGrid : MonoBehaviour
    {
        [SerializeField] private GameObject cardSlotPrefab;

        private List<CardSlot> slots = new List<CardSlot>();

        /// <summary>从 ScriptableObject 数据生成卡牌</summary>
        public CardSlot AddCard(CardData data, Transform parentSlot)
        {
            var slot = CreateSlot(parentSlot);
            if (slot != null)
            {
                slot.SetCardData(data);
                slots.Add(slot);
            }
            return slot;
        }

        /// <summary>前端测试用（无数据绑定）</summary>
        public CardSlot AddCard(string cardName, string time, string emo, Transform parentSlot)
        {
            var slot = CreateSlot(parentSlot);
            if (slot != null)
            {
                slot.SetDisplay(cardName, time, emo);
                slots.Add(slot);
            }
            return slot;
        }

        private CardSlot CreateSlot(Transform parentSlot)
        {
            if (cardSlotPrefab == null)
            {
                Debug.LogError("[CardGrid] cardSlotPrefab 未赋值！");
                return null;
            }

            GameObject obj = Instantiate(cardSlotPrefab, parentSlot);
            obj.transform.localPosition = Vector3.zero;

            CardSlot slot = obj.GetComponent<CardSlot>();
            if (slot == null)
                slot = obj.AddComponent<CardSlot>();

            AutoWireCardSlot(slot, obj);
            return slot;
        }

        private void AutoWireCardSlot(CardSlot slot, GameObject root)
        {
            var t = typeof(CardSlot);
            TrySetField(slot, t, "nameText",  FindTMP(root, "CardNameText"));
            TrySetField(slot, t, "timeText",  FindTMP(root, "TotalTimeText"));
            TrySetField(slot, t, "emoText",   FindTMP(root, "EmoCountText"));
            TrySetField(slot, t, "cardImage", root.transform.Find("CardImg")?.GetComponent<Image>());
        }

        private TextMeshProUGUI FindTMP(GameObject root, string childName)
        {
            var child = root.transform.Find(childName);
            return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        }

        private void TrySetField(object target, Type type, string fieldName, object value)
        {
            if (value == null) return;
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        public void RemoveCard(CardSlot slot)
        {
            if (slots.Remove(slot))
                Destroy(slot.gameObject);
        }

        public void Clear()
        {
            foreach (var s in slots)
            {
                if (s != null) Destroy(s.gameObject);
            }
            slots.Clear();
        }

        public int Count => slots.Count;

        public void SetCardSelectedCallback(Action<Game.Data.CardInstance> callback) { }
    }
}
