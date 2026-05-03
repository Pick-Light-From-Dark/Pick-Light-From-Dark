using UnityEngine;
using System.Collections.Generic;
using Game.Config;
using Game.Flow;
using Game.Emotion;
using Game.Data;
using Game.Card;
using Game.Task;
using Game.AI;
using Game.EyeClose;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 开发者模式控制器
    /// 提供运行时调参、时间跳跃、状态干预、快捷事件触发等开发工具
    /// </summary>
    public class DevModeController : MonoBehaviour
    {
        [Header("关卡配置")]
        public LevelConfigSO levelConfig;

        [Header("时间控制")]
        [Tooltip("时间跳跃增量（秒），负数为倒退")]
        public float timeJumpDelta = 30f;
        [Tooltip("直接设置剩余时间")]
        public float setRemainingTime = 300f;

        [Header("情绪值控制")]
        [Range(0, 100)] public int setPanic = 35;
        [Range(0, 100)] public int setExcite = 35;

        [Header("玩家状态")]
        public bool setInBed = true;
        public bool setEyesClosed = false;

        [Header("卡牌控制")]
        public int addCardId = 2008;
        public List<int> setInitialCards = new List<int> { 2001, 2010 };

        [Header("教师AI参数")]
        public Vector2 patrolIntervals = new Vector2(15f, 25f);
        public Vector2 patrolTime = new Vector2(8f, 10f);
        public float flashPanicPerSec = 2f;

        [Header("颜文字情绪显示")]
        public bool showEmotionEmoji = true;
        public float emojiDisplayX = 10f;
        public float emojiDisplayY = 300f;

        [Header("UI 显示")]
        public bool showOnGUI = true;
        public KeyCode toggleDevPanelKey = KeyCode.BackQuote; // ` 键

        private GameFlowController gameFlow;
        private EmotionSystem emotionSystem;
        private PlayerState playerState;
        private CardManager cardManager;
        private TaskManager taskManager;
        private EyeCloseSystem eyeCloseSystem;
        private TeacherAI teacherAI;

        private bool panelVisible = true;

        void Start()
        {
            gameFlow = GameFlowController.Instance;
            emotionSystem = EmotionSystem.Instance;
            playerState = PlayerState.Instance;
            cardManager = CardManager.Instance;
            taskManager = TaskManager.Instance;
            eyeCloseSystem = EyeCloseSystem.Instance;
            teacherAI = FindFirstObjectByType<TeacherAI>();

            Debug.Log("[DevMode] 开发者模式已就绪。按 ` 键切换面板。");
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleDevPanelKey))
            {
                panelVisible = !panelVisible;
            }
        }

        void OnGUI()
        {
            if (showEmotionEmoji)
            {
                DrawEmotionEmoji();
            }

            if (!showOnGUI || !panelVisible) return;

            int x = 10, y = 10, w = 380, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 520), "开发者模式 (DevMode)");
            y += 25;

            // 时间控制
            GUI.Label(new Rect(x, y, w, h), "=== 时间控制 ==="); y += h + 2;
            GUI.Label(new Rect(x, y, 140, h), $"剩余时间: {gameFlow?.GetRemainingTime():F1}s"); y += h;
            if (GUI.Button(new Rect(x, y, 120, h), "+30秒")) JumpTime(30f);
            if (GUI.Button(new Rect(x + 125, y, 120, h), "-30秒")) JumpTime(-30f);
            if (GUI.Button(new Rect(x + 250, y, 120, h), "直接设置")) SetTime(setRemainingTime);
            y += h + 4;

            // 情绪值控制
            GUI.Label(new Rect(x, y, w, h), "=== 情绪值 ==="); y += h + 2;
            var info = emotionSystem?.GetEmotionInfo();
            GUI.Label(new Rect(x, y, w, h), $"慌乱:{info?.panicValue} 兴奋:{info?.exciteValue} 总和:{info?.totalValue}"); y += h;
            GUI.Label(new Rect(x, y, 80, h), "Panic:");
            int.TryParse(GUI.TextField(new Rect(x + 80, y, 50, h), setPanic.ToString()), out setPanic);
            if (GUI.Button(new Rect(x + 135, y, 80, h), "设置")) SetEmotion(setPanic, setExcite);
            y += h;
            GUI.Label(new Rect(x, y, 80, h), "Excite:");
            int.TryParse(GUI.TextField(new Rect(x + 80, y, 50, h), setExcite.ToString()), out setExcite);
            y += h + 4;

            // 玩家状态
            GUI.Label(new Rect(x, y, w, h), "=== 玩家状态 ==="); y += h + 2;
            GUI.Label(new Rect(x, y, w, h), $"床上:{playerState?.IsInBed()} 闭眼:{playerState?.IsEyesClosed()}"); y += h;
            setInBed = GUI.Toggle(new Rect(x, y, 80, h), setInBed, "InBed");
            setEyesClosed = GUI.Toggle(new Rect(x + 90, y, 100, h), setEyesClosed, "EyesClosed");
            if (GUI.Button(new Rect(x + 200, y, 100, h), "应用")) ApplyPlayerState();
            y += h + 4;

            // 卡牌控制
            GUI.Label(new Rect(x, y, w, h), "=== 卡牌 ==="); y += h + 2;
            GUI.Label(new Rect(x, y, w, h), $"手牌数: {cardManager?.GetHandCardCount()}"); y += h;
            GUI.Label(new Rect(x, y, 60, h), "AddID:");
            int.TryParse(GUI.TextField(new Rect(x + 60, y, 50, h), addCardId.ToString()), out addCardId);
            if (GUI.Button(new Rect(x + 115, y, 80, h), "添加")) AddCardById(addCardId);
            if (GUI.Button(new Rect(x + 200, y, 100, h), "清空手牌")) ClearHandCards();
            y += h + 4;

            // 教师AI参数
            GUI.Label(new Rect(x, y, w, h), "=== 教师AI ==="); y += h + 2;
            GUI.Label(new Rect(x, y, w, h), $"状态: {teacherAI?.GetCurrentState()} 类型: {teacherAI?.GetCurrentInspectType()}"); y += h;
            if (GUI.Button(new Rect(x, y, 120, h), "跳到Approaching")) ForceTeacherState(TeacherState.Approaching);
            if (GUI.Button(new Rect(x + 125, y, 120, h), "跳到Inspecting")) ForceTeacherState(TeacherState.Inspecting);
            y += h;
            if (GUI.Button(new Rect(x, y, 180, h), "应用AI参数到Config")) ApplyAIParmsToConfig();
            y += h + 4;

            // 快捷事件
            GUI.Label(new Rect(x, y, w, h), "=== 快捷事件 ==="); y += h + 2;
            if (GUI.Button(new Rect(x, y, 90, h), "GameWin")) gameFlow?.GameWin();
            if (GUI.Button(new Rect(x + 95, y, 90, h), "GameLose")) gameFlow?.GameLose("开发者触发");
            if (GUI.Button(new Rect(x + 190, y, 100, h), "PlayerCaught")) gameFlow?.OnPlayerCaught();
            y += h;
            if (GUI.Button(new Rect(x, y, 120, h), "触发LevelComplete")) EventCenter.Instance.EventTrigger(E_EventType.LevelComplete);
            y += h + 4;

            // 第一关初始化
            GUI.Label(new Rect(x, y, w, h), "=== 第一关 ==="); y += h + 2;
            if (GUI.Button(new Rect(x, y, w, h), "初始化第一关（测试案v2参数）")) InitializeFirstLevel();
            y += h;
        }

        // ========== 时间控制 ==========

        [ContextMenu("JumpTime +30s")]
        public void JumpTimeForward30() => JumpTime(30f);

        [ContextMenu("JumpTime -30s")]
        public void JumpTimeBackward30() => JumpTime(-30f);

        public void JumpTime(float delta)
        {
            if (gameFlow == null) return;
            float before = gameFlow.GetRemainingTime();
            // 通过反射修改私有字段 remainingTime
            var field = typeof(GameFlowController).GetField("remainingTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                float newTime = Mathf.Max(0f, before + delta);
                field.SetValue(gameFlow, newTime);
                Debug.Log($"[DevMode] 时间跳跃 {delta:F1}s: {before:F1} → {newTime:F1}");
            }
        }

        public void SetTime(float time)
        {
            if (gameFlow == null) return;
            var field = typeof(GameFlowController).GetField("remainingTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(gameFlow, Mathf.Max(0f, time));
                Debug.Log($"[DevMode] 时间设置为 {time:F1}s");
            }
        }

        // ========== 情绪值控制 ==========

        [ContextMenu("SetEmotion")]
        public void SetEmotion(int panic, int excite)
        {
            if (emotionSystem == null) return;
            // 通过反射修改 panicValue 和 exciteValue
            var panicField = typeof(EmotionSystem).GetField("panicValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var exciteField = typeof(EmotionSystem).GetField("exciteValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (panicField != null) panicField.SetValue(emotionSystem, Mathf.Clamp(panic, 0, 100));
            if (exciteField != null) exciteField.SetValue(emotionSystem, Mathf.Clamp(excite, 0, 100));
            emotionSystem.GetType().GetMethod("NotifyEmotionChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(emotionSystem, null);
            Debug.Log($"[DevMode] 情绪值设置: 慌乱={panic} 兴奋={excite}");
        }

        // ========== 玩家状态 ==========

        [ContextMenu("ApplyPlayerState")]
        public void ApplyPlayerState()
        {
            if (playerState == null) return;
            playerState.SetInBed(setInBed);
            playerState.SetEyesClosed(setEyesClosed);
            Debug.Log($"[DevMode] 玩家状态: 床上={setInBed} 闭眼={setEyesClosed}");
        }

        // ========== 卡牌控制 ==========

        [ContextMenu("AddCardById")]
        public void AddCardById(int cardId)
        {
            if (cardManager == null) return;
            CardData data = FindCardDataById(cardId);
            if (data != null)
            {
                cardManager.AddCard(data);
                Debug.Log($"[DevMode] 添加卡牌: {data.cardName} (ID:{cardId})");
            }
            else
            {
                Debug.LogWarning($"[DevMode] 未找到卡牌 ID:{cardId}");
            }
        }

        [ContextMenu("ClearHandCards")]
        public void ClearHandCards()
        {
            if (cardManager == null) return;
            var cards = cardManager.GetHandCards();
            foreach (var c in cards)
            {
                cardManager.RemoveCard(c);
            }
            Debug.Log("[DevMode] 清空所有手牌");
        }

        // ========== 教师AI控制 ==========

        public void ForceTeacherState(TeacherState state)
        {
            if (teacherAI == null)
            {
                Debug.LogWarning("[DevMode] 场景中无 TeacherAI");
                return;
            }
            // 通过反射调用 EnterState
            var method = typeof(TeacherAI).GetMethod("EnterState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(teacherAI, new object[] { state });
                Debug.Log($"[DevMode] 强制 TeacherAI 进入 {state}");
            }
        }

        [ContextMenu("ApplyAIParmsToConfig")]
        public void ApplyAIParmsToConfig()
        {
            if (levelConfig == null)
            {
                Debug.LogError("[DevMode] levelConfig 未指定");
                return;
            }
            levelConfig.patrolIntervals = patrolIntervals;
            levelConfig.patrolTime = patrolTime;
            levelConfig.flashPanicPerSec = Mathf.RoundToInt(flashPanicPerSec);
            Debug.Log($"[DevMode] AI参数已写入 levelConfig");
        }

        // ========== 第一关初始化 ==========

        [ContextMenu("InitializeFirstLevel")]
        public void InitializeFirstLevel()
        {
            if (levelConfig == null)
            {
                Debug.LogError("[DevMode] levelConfig 未指定，无法初始化第一关");
                return;
            }

            // 使用测试案v2参数
            levelConfig.levelId = 1001;
            levelConfig.levelName = "第一夜";
            levelConfig.timeLimit = 600f;
            levelConfig.maxLives = 2;
            levelConfig.initialInBed = true;
            levelConfig.initialPanic = 15;
            levelConfig.initialExcite = 15;
            levelConfig.criticalValue = 80;
            levelConfig.eyeClosePanicDecreasePerSec = 1f;
            levelConfig.eyeCloseAccelerationThreshold = 20f;
            levelConfig.eyeCloseAccelerationMultiplier = 1.5f;
            levelConfig.patrolIntervals = new Vector2(15f, 25f);
            levelConfig.patrolTime = new Vector2(8f, 10f);
            levelConfig.eyeCheckDuration = new Vector2(3f, 3f);
            levelConfig.flashCheckDuration = new Vector2(3f, 3f);
            levelConfig.flashPanicPerSec = 2;
            levelConfig.initialCards = new List<int>(setInitialCards);

            // 任务配置
            var goal = new TaskGoal(2008, 1) { state = TaskState.InProgress };
            levelConfig.taskGoals = new List<TaskGoal> { goal };

            // 初始化各系统
            gameFlow?.Initialize(levelConfig);
            teacherAI?.Initialize(levelConfig);

            Debug.Log("[DevMode] 第一关已初始化（测试案v2参数）");
        }

        // ========== 颜文字情绪显示 ==========

        void DrawEmotionEmoji()
        {
            if (emotionSystem == null) return;
            var info = emotionSystem.GetEmotionInfo();
            string emoji = GetEmotionEmoji(info.totalValue, info.criticalValue);
            string label = $"{emoji} 慌乱:{info.panicValue} 兴奋:{info.exciteValue}";
            GUI.Label(new Rect(emojiDisplayX, emojiDisplayY, 300, 30), $"<size=24>{label}</size>");
        }

        string GetEmotionEmoji(int totalValue, int criticalValue)
        {
            if (criticalValue <= 0) return "(╯°□°)╯";
            float ratio = (float)totalValue / criticalValue;
            if (ratio >= 1.0f) return "(╯°□°)╯"; // 超临界
            if (ratio >= 0.8f) return "(；´Д｀)"; // 高
            if (ratio >= 0.6f) return "(；一_一)"; // 中高
            if (ratio >= 0.4f) return "(￣▽￣)"; // 中
            if (ratio >= 0.2f) return "(｡♥‿♥｡)"; // 低
            return "(✿◠‿◠)"; // 很低
        }

        // ========== 工具方法 ==========

        CardData FindCardDataById(int cardId)
        {
            CardDataContainer[] containers = Resources.LoadAll<CardDataContainer>("TestData");
            foreach (var container in containers)
            {
                if (container.cardData != null && container.cardData.id == cardId)
                    return container.cardData;
            }
            return null;
        }
    }
}
