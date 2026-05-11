using UnityEngine;
using Game.Config;

namespace Game.Flow
{
    public class LevelFlowManager : SingletonAutoMono<LevelFlowManager>
    {
        private enum FlowState { Idle, PreDialogue, Gameplay, PostDialogue, EndingDialogue }

        [SerializeField] private string[] levelSequence = {
            "Config/LevelConfig_1",
            "Config/LevelConfig_2"
        };

        private FlowState currentState = FlowState.Idle;
        private int currentLevelIndex;
        private LevelConfigSO currentLevelConfig;

        void Start()
        {
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        void OnDestroy()
        {
            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
                EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameLose);
            }
        }

        public void StartGame()
        {
            currentLevelIndex = 0;
            StartLevel(0);
        }

        private void StartLevel(int index)
        {
            if (index >= levelSequence.Length)
            {
                BackToMenu();
                return;
            }

            currentLevelConfig = Resources.Load<LevelConfigSO>(levelSequence[index]);
            if (currentLevelConfig == null)
            {
                Debug.LogError($"[LevelFlow] 找不到关卡配置: {levelSequence[index]}");
                BackToMenu();
                return;
            }

            currentLevelIndex = index;
            Debug.Log($"[LevelFlow] 开始关卡 {index + 1}: {currentLevelConfig.levelName}");

            if (!string.IsNullOrEmpty(currentLevelConfig.preDialogueFile))
                BeginPreDialogue();
            else
                BeginGameplay();
        }

        private void BeginPreDialogue()
        {
            currentState = FlowState.PreDialogue;
            var txt = Resources.Load<TextAsset>("Dialogue/" + currentLevelConfig.preDialogueFile);
            if (txt == null)
            {
                Debug.LogError($"[LevelFlow] 找不到前置对话: {currentLevelConfig.preDialogueFile}");
                BeginGameplay();
                return;
            }

            UIMgr.Instance.ShowPanel<GalDialoguePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    DialogueSystem.Instance.BindPanel(panel);
                    DialogueSystem.Instance.SetOnComplete(OnPreDialogueComplete);
                    DialogueSystem.Instance.StartDialogue(txt, DialogueSystem.DialogueMode.Gal);
                }
            );
        }

        private void OnPreDialogueComplete()
        {
            if (currentLevelConfig.isChoiceLevel && DialogueSystem.Instance.LastChoiceIndex == 2)
            {
                BeginEndingDialogue();
                return;
            }

            UIMgr.Instance.HidePanel<GalDialoguePanel>();
            BeginGameplay();
        }

        private void BeginEndingDialogue()
        {
            if (string.IsNullOrEmpty(currentLevelConfig.choice2EndingDialogueFile))
            {
                BackToMenu();
                return;
            }

            currentState = FlowState.EndingDialogue;
            var txt = Resources.Load<TextAsset>("Dialogue/" + currentLevelConfig.choice2EndingDialogueFile);
            if (txt == null)
            {
                BackToMenu();
                return;
            }

            UIMgr.Instance.ShowPanel<GalDialoguePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    DialogueSystem.Instance.BindPanel(panel);
                    DialogueSystem.Instance.SetOnComplete(OnEndingDialogueComplete);
                    DialogueSystem.Instance.StartDialogue(txt, DialogueSystem.DialogueMode.Gal);
                }
            );
        }

        private void OnEndingDialogueComplete()
        {
            UIMgr.Instance.HidePanel<GalDialoguePanel>();
            BackToMenu();
        }

        private void BeginGameplay()
        {
            currentState = FlowState.Gameplay;
            UIMgr.Instance.ShowPanel<GamePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    panel.InitializeWithConfig(currentLevelConfig);
                }
            );
        }

        private void OnGameWin()
        {
            if (currentState != FlowState.Gameplay) return;

            Debug.Log($"[LevelFlow] 关卡 {currentLevelIndex + 1} 胜利");

            if (!string.IsNullOrEmpty(currentLevelConfig.postDialogueFile))
            {
                UIMgr.Instance.HidePanel<GamePanel>();
                BeginPostDialogue();
            }
            else
            {
                UIMgr.Instance.HidePanel<GamePanel>();
                AdvanceToNextLevel();
            }
        }

        private void OnGameLose(string reason)
        {
            if (currentState != FlowState.Gameplay) return;

            Debug.Log($"[LevelFlow] 关卡 {currentLevelIndex + 1} 失败: {reason}");
            UIMgr.Instance.HidePanel<GamePanel>();
            BackToMenu();
        }

        private void BeginPostDialogue()
        {
            currentState = FlowState.PostDialogue;
            var txt = Resources.Load<TextAsset>("Dialogue/" + currentLevelConfig.postDialogueFile);
            if (txt == null)
            {
                Debug.LogError($"[LevelFlow] 找不到后置对话: {currentLevelConfig.postDialogueFile}");
                AdvanceToNextLevel();
                return;
            }

            UIMgr.Instance.ShowPanel<GalDialoguePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    DialogueSystem.Instance.BindPanel(panel);
                    DialogueSystem.Instance.SetOnComplete(OnPostDialogueComplete);
                    DialogueSystem.Instance.StartDialogue(txt, DialogueSystem.DialogueMode.Gal);
                }
            );
        }

        private void OnPostDialogueComplete()
        {
            UIMgr.Instance.HidePanel<GalDialoguePanel>();
            AdvanceToNextLevel();
        }

        private void AdvanceToNextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= levelSequence.Length)
            {
                Debug.Log("[LevelFlow] 所有关卡完成，返回主菜单");
                BackToMenu();
            }
            else
            {
                StartLevel(currentLevelIndex);
            }
        }

        private void BackToMenu()
        {
            currentState = FlowState.Idle;
            UIMgr.Instance.ShowPanel<BeginPanel>();
        }
    }
}
