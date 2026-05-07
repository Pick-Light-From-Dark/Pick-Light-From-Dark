using UnityEngine;
using System.Collections.Generic;
using Game.Config;
using Game.Flow;
using Game.Emotion;
using Game.Data;
using Game.Card;
using Game.AI;
using Game.Task;
using Game.EyeClose;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 文字游戏控制器
    /// 用 OnGUI 实现一个可游玩的文字界面，核心玩法：做任务、睁眼闭眼、躲老师
    /// </summary>
    public class TextGameController : MonoBehaviour
    {
        [Header("游戏配置")]
        public LevelConfigSO levelConfig;
        public bool autoInitOnStart = true;

        [Header("UI 设置")]
        public bool showOnGUI = true;
        public KeyCode toggleGameKey = KeyCode.G;
        public KeyCode closeGameKey = KeyCode.Escape;

        private GameFlowController gameFlow;
        private EmotionSystem emotionSystem;
        private PlayerState playerState;
        private CardManager cardManager;
        private CardReadingSystem cardReadingSystem;
        private EyeCloseSystem eyeCloseSystem;
        private TeacherAI teacherAI;
        private TaskManager taskManager;

        private enum GameUIState { Menu, Playing, GameOver }
        private GameUIState uiState = GameUIState.Menu;
        private string gameOverReason = "";
        private bool isWin = false;

        private bool panelVisible = true;
        private float warningFlashTimer = 0f;
        private bool showWarning = false;
        private TeacherState lastTeacherState = TeacherState.Idle;
        private List<string> eventLog = new List<string>();
        private const int maxLogLines = 5;

        void Awake()
        {
            EnsureComponents();
        }

        void Start()
        {
            if (autoInitOnStart && levelConfig != null)
            {
                InitializeGame();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleGameKey))
            {
                panelVisible = !panelVisible;
            }
            if (Input.GetKeyDown(closeGameKey) && uiState == GameUIState.Playing)
            {
                panelVisible = false;
            }
            if (Input.GetKeyDown(KeyCode.C) && uiState == GameUIState.Playing)
            {
                playerState?.ToggleEyesClosed();
            }

            // 教师接近警告闪烁
            if (teacherAI != null && teacherAI.GetCurrentState() == TeacherState.Approaching)
            {
                warningFlashTimer += Time.deltaTime;
                if (warningFlashTimer >= 0.5f)
                {
                    warningFlashTimer = 0f;
                    showWarning = !showWarning;
                }
            }
            else
            {
                showWarning = false;
                warningFlashTimer = 0f;
            }

            // 检测教师状态变化，输出日志
            if (teacherAI != null && teacherAI.GetCurrentState() != lastTeacherState)
            {
                lastTeacherState = teacherAI.GetCurrentState();
                string msg = $"[教师] {GetTeacherStateName(lastTeacherState)}";
                if (lastTeacherState == TeacherState.Inspecting)
                    msg += $" ({teacherAI.GetCurrentInspectType()})";
                AddEventLog(msg);
            }

            // 检测游戏结束
            if (uiState == GameUIState.Playing && gameFlow != null && gameFlow.IsGameOver())
            {
                // 由事件系统处理，这里只检测
            }
        }

        void OnGUI()
        {
            if (!showOnGUI || !panelVisible) return;

            switch (uiState)
            {
                case GameUIState.Menu:
                    DrawMenu();
                    break;
                case GameUIState.Playing:
                    DrawGame();
                    break;
                case GameUIState.GameOver:
                    DrawGameOver();
                    break;
            }
        }

        // ========== 菜单界面 ==========

        void DrawMenu()
        {
            int w = 400, h = 300;
            int x = (Screen.width - w) / 2;
            int y = (Screen.height - h) / 2;

            GUI.Box(new Rect(x, y, w, h), "《灯下黑》第一夜");
            y += 40;

            GUI.Label(new Rect(x + 20, y, w - 40, 30), "你是 LuYing，深夜在床上躲避老师查寝。");
            y += 35;
            GUI.Label(new Rect(x + 20, y, w - 40, 30), "使用卡牌完成任务，闭眼躲避检查。");
            y += 35;
            GUI.Label(new Rect(x + 20, y, w - 40, 30), "不要让老师发现你！");
            y += 50;

            GUI.Label(new Rect(x + 20, y, w - 40, 30), "操作：C键闭眼 | G键开关面板 | ESC关闭面板");
            y += 40;

            if (GUI.Button(new Rect(x + 100, y, 200, 40), "开始游戏"))
            {
                InitializeGame();
            }
        }

        // ========== 游戏界面 ==========

        void DrawGame()
        {
            // 闭眼状态：全屏黑屏，仅显示睁眼按钮
            if (playerState?.IsEyesClosed() == true)
            {
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;

                int btnW = 200, btnH = 50;
                int bx = (Screen.width - btnW) / 2;
                int by = (Screen.height - btnH) / 2;

                GUI.Label(new Rect(bx, by - 40, btnW, 30), "闭眼中...", GUI.skin.box);
                if (GUI.Button(new Rect(bx, by, btnW, btnH), "👁 睁眼 (C)"))
                {
                    playerState?.ToggleEyesClosed();
                }
                return;
            }

            int panelW = 500;
            int panelH = 580;
            int x = (Screen.width - panelW) / 2;
            int y = 20;
            int lineH = 26;

            GUI.Box(new Rect(x, y, panelW, panelH), "");
            int cx = x + 15;
            int cy = y + 10;

            // === 顶部状态栏 ===
            GUI.Label(new Rect(cx, cy, panelW - 30, lineH), $"=== 状态 ===", GUI.skin.box);
            cy += lineH + 5;

            var info = emotionSystem?.GetEmotionInfo();
            string emoji = GetEmotionEmoji(info?.totalValue ?? 0, info?.criticalValue ?? 80);
            GUI.Label(new Rect(cx, cy, panelW - 30, lineH),
                $"时间: {gameFlow?.GetRemainingTime():F0}s  生命: {gameFlow?.GetCurrentLives()}  {emoji} 情绪: {info?.totalValue}/{info?.criticalValue}");
            cy += lineH + 5;

            GUI.Label(new Rect(cx, cy, panelW - 30, lineH),
                $"闭眼: {(playerState?.IsEyesClosed() == true ? "是" : "否")}  床上: {(playerState?.IsInBed() == true ? "是" : "否")}");
            cy += lineH + 10;

            // === 教师状态 ===
            GUI.Label(new Rect(cx, cy, panelW - 30, lineH), "=== 教师 ===", GUI.skin.box);
            cy += lineH + 5;

            TeacherState tState = teacherAI?.GetCurrentState() ?? TeacherState.Idle;
            Color tColor = GetTeacherStateColor(tState);
            Color prevColor = GUI.color;
            GUI.color = tColor;

            string tText = GetTeacherStateName(tState);
            if (tState == TeacherState.Approaching)
                tText += $" (进度: {teacherAI?.GetApproachProgress() * 100:F0}%)";
            if (tState == TeacherState.Inspecting)
                tText += $" ({teacherAI?.GetCurrentInspectType()})";

            GUI.Label(new Rect(cx, cy, panelW - 30, lineH), tText, GUI.skin.box);
            GUI.color = prevColor;
            cy += lineH + 5;

            // 警告
            if (showWarning)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(cx, cy, panelW - 30, lineH), "⚠️ 老师正在接近！准备闭眼！⚠️");
                GUI.color = prevColor;
            }
            cy += lineH + 10;

            // === 读条状态 ===
            GUI.Label(new Rect(cx, cy, panelW - 30, lineH), "=== 当前动作 ===", GUI.skin.box);
            cy += lineH + 5;

            if (cardReadingSystem != null && cardReadingSystem.IsReading())
            {
                var card = cardReadingSystem.GetCurrentCard();
                float progress = cardReadingSystem.GetProgress();
                GUI.Label(new Rect(cx, cy, panelW - 30, lineH),
                    $"读条中: {card?.data?.cardName} [{progress * 100:F0}%]");
                cy += lineH + 5;

                // 进度条
                GUI.Box(new Rect(cx, cy, panelW - 30, 20), "");
                GUI.Button(new Rect(cx, cy, (panelW - 30) * progress, 20), "", GUI.skin.box);
                cy += 25 + 5;

                if (cardReadingSystem.CanInterrupt())
                {
                    if (GUI.Button(new Rect(cx, cy, 120, 28), "打断读条"))
                    {
                        cardReadingSystem.InterruptReading();
                        AddEventLog("读条被打断");
                    }
                }
                cy += 30 + 5;
            }
            else
            {
                GUI.Label(new Rect(cx, cy, panelW - 30, lineH), "空闲中 - 选择一张卡牌使用");
                cy += lineH + 5;
            }
            cy += 10;

            // === 手牌 ===
            GUI.Label(new Rect(cx, cy, panelW - 30, lineH), "=== 手牌 ===", GUI.skin.box);
            cy += lineH + 5;

            var handCards = cardManager?.GetHandCards();
            if (handCards != null && handCards.Count > 0)
            {
                int btnW = 110, btnH = 50;
                int perRow = 4;
                int idx = 0;
                foreach (var card in handCards)
                {
                    if (card == null || card.data == null) continue;
                    int bx = cx + (idx % perRow) * (btnW + 8);
                    int by = cy + (idx / perRow) * (btnH + 5);

                    bool canUse = !card.isUsed && !cardReadingSystem.IsReading();
                    GUI.enabled = canUse;

                    string btnText = $"{card.data.cardName}\n({card.data.CalculateTotalDuration():F1}s)";
                    if (GUI.Button(new Rect(bx, by, btnW, btnH), btnText))
                    {
                        if (canUse)
                        {
                            cardReadingSystem.StartReading(card);
                            AddEventLog($"开始使用: {card.data.cardName}");
                        }
                    }
                    GUI.enabled = true;
                    idx++;
                }
                cy += ((idx + perRow - 1) / perRow) * (btnH + 5) + 10;
            }
            else
            {
                GUI.Label(new Rect(cx, cy, panelW - 30, lineH), "(无手牌)");
                cy += lineH + 10;
            }

            // === 控制按钮 ===
            GUI.Label(new Rect(cx, cy, panelW - 30, lineH), "=== 控制 ===", GUI.skin.box);
            cy += lineH + 5;

            bool isClosed = playerState?.IsEyesClosed() ?? false;
            string eyeBtn = isClosed ? "👁 睁眼 (C)" : "👁‍🗨 闭眼 (C)";
            if (GUI.Button(new Rect(cx, cy, 140, 32), eyeBtn))
            {
                playerState?.ToggleEyesClosed();
            }

            if (GUI.Button(new Rect(cx + 150, cy, 140, 32), "上床/下床"))
            {
                bool inBed = !(playerState?.IsInBed() ?? true);
                playerState?.SetInBed(inBed);
                AddEventLog(inBed ? "你爬上了床" : "你下了床");
            }

            if (GUI.Button(new Rect(cx + 300, cy, 100, 32), "暂停游戏"))
            {
                gameFlow?.PauseGame();
                AddEventLog("游戏已暂停");
            }
            cy += 40 + 10;

            // === 事件日志 ===
            GUI.Label(new Rect(cx, cy, panelW - 30, lineH), "=== 事件 ===", GUI.skin.box);
            cy += lineH + 5;
            foreach (var log in eventLog)
            {
                GUI.Label(new Rect(cx, cy, panelW - 30, 20), log);
                cy += 20;
            }
        }

        // ========== 结束界面 ==========

        void DrawGameOver()
        {
            int w = 400, h = 250;
            int x = (Screen.width - w) / 2;
            int y = (Screen.height - h) / 2;

            GUI.color = isWin ? Color.green : Color.red;
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.color = Color.white;

            y += 30;
            string title = isWin ? "🎉 胜利！" : "💀 失败";
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(x, y, w, 40), $"<size=24>{title}</size>");
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            y += 50;

            GUI.Label(new Rect(x + 20, y, w - 40, 30), gameOverReason);
            y += 40;

            if (GUI.Button(new Rect(x + 100, y, 200, 40), "重新开始"))
            {
                uiState = GameUIState.Menu;
            }
        }

        // ========== 游戏逻辑 ==========

        void InitializeGame()
        {
            if (levelConfig == null)
            {
                levelConfig = ScriptableObject.CreateInstance<LevelConfigSO>();
                SetupDefaultConfig();
            }

            // 初始化各系统
            gameFlow?.Initialize(levelConfig);
            teacherAI?.Initialize(levelConfig);
            cardReadingSystem?.Initialize(levelConfig);
            eyeCloseSystem?.Initialize(levelConfig);
            playerState?.Initialize(levelConfig);
            emotionSystem?.Initialize(levelConfig);
            cardManager?.Initialize(levelConfig);
            taskManager?.Initialize(levelConfig);

            // 订阅事件
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);
            EventCenter.Instance.AddEventListener(E_EventType.PlayerCaught, OnPlayerCaught);
            EventCenter.Instance.AddEventListener<int>(E_EventType.CardReadComplete, OnCardReadComplete);

            uiState = GameUIState.Playing;
            eventLog.Clear();
            AddEventLog("游戏开始！第一夜...");
            AddEventLog("提示：C键闭眼，点击手牌使用");
        }

        void SetupDefaultConfig()
        {
            levelConfig.levelId = 1001;
            levelConfig.levelName = "第一夜";
            levelConfig.timeLimit = 600;
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
            levelConfig.flashPanicPerSec = 2f;
            levelConfig.initialCards = new List<int> { 2001, 2010, 2008 };

            var goal = new TaskGoal(2008, 1) { state = TaskState.InProgress };
            levelConfig.taskGoals = new List<TaskGoal> { goal };
        }

        void EnsureComponents()
        {
            gameFlow = GameFlowController.Instance;
            emotionSystem = EmotionSystem.Instance;
            playerState = PlayerState.Instance;
            cardManager = CardManager.Instance;
            eyeCloseSystem = EyeCloseSystem.Instance;
            taskManager = TaskManager.Instance;

            teacherAI = FindFirstObjectByType<TeacherAI>();
            if (teacherAI == null)
            {
                GameObject go = new GameObject("TeacherAI");
                teacherAI = go.AddComponent<TeacherAI>();
            }

            cardReadingSystem = FindFirstObjectByType<CardReadingSystem>();
            if (cardReadingSystem == null)
            {
                GameObject go = new GameObject("CardReadingSystem");
                cardReadingSystem = go.AddComponent<CardReadingSystem>();
            }
        }

        void OnDestroy()
        {
            EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameLose);
            EventCenter.Instance.RemoveEventListener(E_EventType.PlayerCaught, OnPlayerCaught);
            EventCenter.Instance.RemoveEventListener<int>(E_EventType.CardReadComplete, OnCardReadComplete);
        }

        void OnGameWin()
        {
            isWin = true;
            gameOverReason = "你成功完成了所有任务，逃过了老师的检查！";
            uiState = GameUIState.GameOver;
        }

        void OnGameLose(string reason)
        {
            isWin = false;
            gameOverReason = $"游戏结束: {reason}";
            uiState = GameUIState.GameOver;
        }

        void OnPlayerCaught()
        {
            AddEventLog("⚠️ 被老师发现了！生命值 -1");
        }

        void OnCardReadComplete(int cardId)
        {
            AddEventLog($"✅ 卡牌使用完成！");
        }

        void AddEventLog(string msg)
        {
            eventLog.Add(msg);
            if (eventLog.Count > maxLogLines)
                eventLog.RemoveAt(0);
        }

        // ========== 工具方法 ==========

        string GetTeacherStateName(TeacherState state)
        {
            switch (state)
            {
                case TeacherState.Idle: return "空闲中...";
                case TeacherState.Approaching: return "正在接近！";
                case TeacherState.Inspecting: return "正在检查！";
                case TeacherState.Leaving: return "正在离开";
                default: return state.ToString();
            }
        }

        Color GetTeacherStateColor(TeacherState state)
        {
            switch (state)
            {
                case TeacherState.Idle: return Color.green;
                case TeacherState.Approaching: return Color.yellow;
                case TeacherState.Inspecting: return Color.red;
                case TeacherState.Leaving: return Color.cyan;
                default: return Color.white;
            }
        }

        string GetEmotionEmoji(int totalValue, int criticalValue)
        {
            if (criticalValue <= 0) return "(╯°□°)╯";
            float ratio = (float)totalValue / criticalValue;
            if (ratio >= 1.0f) return "(╯°□°)╯";
            if (ratio >= 0.8f) return "(；´Д｀)";
            if (ratio >= 0.6f) return "(；一_一)";
            if (ratio >= 0.4f) return "(￣▽￣)";
            if (ratio >= 0.2f) return "(｡♥‿♥｡)";
            return "(✿◠‿◠)";
        }
    }
}
