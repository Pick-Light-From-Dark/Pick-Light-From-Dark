using UnityEngine;
using Game.Config;
using Game.Backend;
using Game.Test;

namespace Game.Flow
{
    /// <summary>
    /// 关卡流程协调器 — 统筹一关的完整三段式生命周期：
    /// 开场剧情 → 游玩(打牌) → 结尾剧情 → 结果面板
    /// </summary>
    public class LevelFlowCoordinator : MonoBehaviour
    {
        [Header("剧情文本")]
        public TextAsset openingStory;
        public TextAsset endingStory;

        [Header("关卡配置")]
        public LevelConfigSO levelConfig;
        public int levelId = 1;

        [Header("VN 控制器")]
        public FungusVNController vnController;

        private bool isGameOver = false;

        void Start()
        {
            if (vnController == null)
            {
                vnController = FindObjectOfType<FungusVNController>();
                if (vnController == null)
                {
                    Debug.LogError("[LevelFlowCoordinator] 未找到 FungusVNController，请在 Inspector 中赋值或确保场景中存在");
                    return;
                }
            }

            StartOpeningStory();
        }

        /// <summary>阶段1：开场剧情</summary>
        void StartOpeningStory()
        {
            if (openingStory == null)
            {
                Debug.LogWarning("[LevelFlowCoordinator] openingStory 未赋值，跳过开场剧情直接进入游玩");
                StartGameplay();
                return;
            }

            Debug.Log("[LevelFlowCoordinator] === 阶段1：开场剧情 ===");
            vnController.dialogueText = openingStory;
            vnController.OnDialogueComplete = OnOpeningStoryEnd;
            // 重置 VN 控制器状态
            vnController.ClearPlaceholder();
        }

        /// <summary>开场剧情结束 → 自动存档 → 进入游玩</summary>
        void OnOpeningStoryEnd()
        {
            Debug.Log("[LevelFlowCoordinator] 开场剧情结束，自动存档...");

            // 自动存档：记录已完成开场剧情
            var record = new StoryProgressRecord
            {
                levelId = levelId,
                storyFileName = openingStory != null ? openingStory.name : "",
                lineIndex = -1, // -1 表示开场剧情已全部看完
                isOpeningDone = true
            };
            PlayerDataStore.Instance.SaveStoryProgress(record);

            StartGameplay();
        }

        /// <summary>阶段2：游玩（打牌）</summary>
        void StartGameplay()
        {
            Debug.Log("[LevelFlowCoordinator] === 阶段2：游玩 ===");

            // 显示游戏面板
            UIMgr.Instance.ShowPanel<GamePanel>();

            // 初始化游戏流程控制器
            if (levelConfig != null)
            {
                GameFlowController.Instance.Initialize(levelConfig);
            }
            else
            {
                Debug.LogWarning("[LevelFlowCoordinator] levelConfig 未赋值，GameFlowController 未初始化");
            }

            // 监听游玩结束事件
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        /// <summary>阶段3a：游戏胜利 → 结尾剧情</summary>
        void OnGameWin()
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log("[LevelFlowCoordinator] 游戏胜利，进入结尾剧情...");
            UnsubscribeGameEvents();
            StartEndingStory();
        }

        /// <summary>阶段3b：游戏失败 → 显示失败提示</summary>
        void OnGameLose(string reason)
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log($"[LevelFlowCoordinator] 游戏失败: {reason}");
            UnsubscribeGameEvents();

            // 显示失败提示面板（或返回主菜单）
            UIMgr.Instance.ShowPanel<TipPanel>();
            Debug.Log("[LevelFlowCoordinator] 已显示 TipPanel（失败提示），可在此扩展失败 UI");
        }

        /// <summary>阶段3：结尾剧情</summary>
        void StartEndingStory()
        {
            if (endingStory == null)
            {
                Debug.LogWarning("[LevelFlowCoordinator] endingStory 未赋值，跳过结尾剧情");
                ShowVictoryPanel();
                return;
            }

            Debug.Log("[LevelFlowCoordinator] === 阶段3：结尾剧情 ===");
            vnController.dialogueText = endingStory;
            vnController.OnDialogueComplete = OnEndingStoryEnd;
            vnController.ClearPlaceholder();
        }

        /// <summary>结尾剧情结束 → 显示胜利/结算面板</summary>
        void OnEndingStoryEnd()
        {
            Debug.Log("[LevelFlowCoordinator] 结尾剧情结束，显示结算...");
            ShowVictoryPanel();
        }

        /// <summary>显示胜利结算面板</summary>
        void ShowVictoryPanel()
        {
            UIMgr.Instance.ShowPanel<EndingContentPanel>();
            Debug.Log("[LevelFlowCoordinator] 已显示 EndingContentPanel（胜利结算），可在此扩展结算 UI");
        }

        /// <summary>取消游戏事件订阅</summary>
        void UnsubscribeGameEvents()
        {
            EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        void OnDestroy()
        {
            UnsubscribeGameEvents();
        }
    }
}
