using System;
using UnityEngine;
using Game.Data;
using Game.Card;

namespace Game.UI
{
    /// <summary>
    /// UI管理器
    /// 统筹所有UI组件，处理UI交互逻辑
    /// </summary>
    public class UIManager : SingletonAutoMono<UIManager>
    {
        [Header("UI组件引用")]
        [SerializeField] private TimerDisplay timerDisplay;
        [SerializeField] private EmotionDisplay emotionDisplay;
        [SerializeField] private CardGrid cardGrid;

        private CardManager cardManager;
        private CardInstance selectedCard;

        void Start()
        {
            // 获取卡牌管理器引用
            cardManager = CardManager.Instance;

            // 设置卡牌网格的选中回调
            if (cardGrid != null)
            {
                cardGrid.SetCardSelectedCallback(OnCardSelected);
            }

            // 注册游戏流程事件
            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.AddEventListener(E_EventType.GameStart, OnGameStart);
                EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
                EventCenter.Instance.AddEventListener(E_EventType.GameLose, OnGameLose);
            }
        }

        void OnDestroy()
        {
            // 移除事件监听
            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.RemoveEventListener(E_EventType.GameStart, OnGameStart);
                EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
                EventCenter.Instance.RemoveEventListener(E_EventType.GameLose, OnGameLose);
            }
        }

        /// <summary>
        /// 游戏开始事件
        /// </summary>
        private void OnGameStart()
        {
            Debug.Log("[UIManager] 游戏开始");
        }

        /// <summary>
        /// 游戏胜利事件
        /// </summary>
        private void OnGameWin()
        {
            Debug.Log("[UIManager] 游戏胜利");
            // TODO: 显示胜利弹窗
        }

        /// <summary>
        /// 游戏失败事件
        /// </summary>
        private void OnGameLose()
        {
            Debug.Log("[UIManager] 游戏失败");
            // TODO: 显示失败弹窗
        }

        /// <summary>
        /// 卡牌被选中
        /// </summary>
        private void OnCardSelected(CardInstance card)
        {
            if (card == null || card.data == null) return;

            Debug.Log($"[UIManager] 选中卡牌: {card.data.cardName}");

            // 阶段1.1简化版：直接执行卡牌
            selectedCard = card;
            ExecuteSelectedCard();
        }

        /// <summary>
        /// 执行选中的卡牌
        /// </summary>
        private void ExecuteSelectedCard()
        {
            if (selectedCard == null || cardManager == null) return;

            Debug.Log($"[UIManager] 执行卡牌: {selectedCard.data.cardName}");

            // 调用卡牌管理器执行卡牌
            // TODO: 这里需要根据实际的CardManager API调整
            // cardManager.ExecuteCard(selectedCard);

            // 清空选中状态
            selectedCard = null;
        }

        /// <summary>
        /// 设置UI组件引用
        /// </summary>
        public void SetTimerDisplay(TimerDisplay display)
        {
            timerDisplay = display;
        }

        public void SetEmotionDisplay(EmotionDisplay display)
        {
            emotionDisplay = display;
        }

        public void SetCardGrid(CardGrid grid)
        {
            cardGrid = grid;
        }
    }
}
