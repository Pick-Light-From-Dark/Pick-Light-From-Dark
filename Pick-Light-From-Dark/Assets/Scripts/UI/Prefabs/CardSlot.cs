using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Data;

namespace Game.UI
{
    /// <summary>
    /// 卡牌槽组件
    /// 用于显示单张卡牌或堆叠卡牌
    /// </summary>
    public class CardSlot : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI stackCountText;
        [SerializeField] private Button cardButton;

        [Header("占位美术配置")]
        [SerializeField] private Color normalCardColor = new Color(0.7f, 0.7f, 0.7f); // 灰色
        [SerializeField] private Color stackCardColor = new Color(0.5f, 0.7f, 0.9f); // 蓝灰色

        private Game.Data.CardInstance cardInstance;
        private Action<Game.Data.CardInstance> onCardClicked;

        void Awake()
        {
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(OnClick);
            }
        }

        void OnDestroy()
        {
            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(OnClick);
            }
        }

        /// <summary>
        /// 设置卡牌数据
        /// </summary>
        public void SetCard(Game.Data.CardInstance card, Action<Game.Data.CardInstance> clickCallback)
        {
            cardInstance = card;
            onCardClicked = clickCallback;

            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (cardInstance == null || cardInstance.data == null)
            {
                ShowEmpty();
                return;
            }

            var cardData = cardInstance.data;

            // 显示卡牌名称
            if (cardNameText != null)
            {
                cardNameText.text = cardData.cardName;
            }

            // 暂时隐藏堆叠层数（待实现堆叠功能）
            if (stackCountText != null)
            {
                stackCountText.gameObject.SetActive(false);
            }

            // 设置背景颜色（暂时统一使用普通颜色）
            if (background != null)
            {
                background.color = normalCardColor;
            }
        }

        /// <summary>
        /// 显示空状态
        /// </summary>
        private void ShowEmpty()
        {
            if (cardNameText != null)
            {
                cardNameText.text = "";
            }

            if (stackCountText != null)
            {
                stackCountText.gameObject.SetActive(false);
            }

            if (background != null)
            {
                background.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // 半透明灰色
            }
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        private void OnClick()
        {
            if (cardInstance != null && onCardClicked != null)
            {
                onCardClicked.Invoke(cardInstance);
            }
        }

        /// <summary>
        /// 清空卡牌
        /// </summary>
        public void ClearCard()
        {
            cardInstance = null;
            onCardClicked = null;
            ShowEmpty();
        }

        /// <summary>
        /// 获取当前卡牌
        /// </summary>
        public Game.Data.CardInstance GetCard()
        {
            return cardInstance;
        }
    }
}
