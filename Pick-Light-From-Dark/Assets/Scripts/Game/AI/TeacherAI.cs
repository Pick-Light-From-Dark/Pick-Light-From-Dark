using UnityEngine;
using Game.Config;
using Game.Flow;

namespace Game.AI
{
    /// <summary>
    /// 老师AI状态机
    /// 状态: Idle -> Approaching -> Inspecting -> Leaving -> Idle
    /// </summary>
    public class TeacherAI : MonoBehaviour
    {
        public static TeacherAI Instance { get; private set; }
        [Header("当前状态")]
        [SerializeField] private TeacherState currentState;
        [SerializeField] private InspectType currentInspectType;
        [SerializeField] private float stateTimer;
        [SerializeField] private int patrolCount;

        [Header("接近进度 (0-1)")]
        [SerializeField] private float approachProgress;

        [Header("是否可见")]
        [SerializeField] private bool isVisible;

        private LevelConfigSO levelConfig;
        private GameFlowController gameFlow;
        private Game.Emotion.EmotionSystem emotionSystem;
        private Game.Card.CardReadingSystem cardReadingSystem;
        private Game.Data.PlayerState playerState;
        private float targetDuration;
        private float flashPanicAccumulated;
        private AudioSource footstepSource;
        private bool hasUsedCardThisPatrol;

        // 死循环安全阀
        private static int safetyUpdateFrame = -1;
        private static int safetyUpdateCount = 0;
        private const int SAFETY_UPDATE_MAX = 100;

        void Awake()
        {
            Instance = this;
            gameFlow = GameFlowController.Instance;
            EventCenter.Instance.AddEventListener(E_EventType.GamePause, OnGamePause);
            EventCenter.Instance.AddEventListener(E_EventType.GameResume, OnGameResume);
        }

        void OnDestroy()
        {
            StopFootstep();
            EventCenter.Instance.RemoveEventListener<int>(E_EventType.CardReadComplete, OnCardUsed);
            EventCenter.Instance.RemoveEventListener(E_EventType.GamePause, OnGamePause);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameResume, OnGameResume);
            if (Instance == this) Instance = null;
        }

        private void OnGamePause()
        {
            StopFootstep();
        }

        private void OnGameResume()
        {
            if (currentState == TeacherState.Approaching || currentState == TeacherState.Leaving)
            {
                MusicMgr.Instance.PlaySound("DXH_SOUND/08.脚步声", true, (source) =>
                {
                    footstepSource = source;
                    footstepSource.volume = currentState == TeacherState.Approaching
                        ? 0.65f * MusicMgr.Instance.SoundValue
                        : MusicMgr.Instance.SoundValue;
                });
            }
        }

        private void OnCardUsed(int cardId)
        {
            hasUsedCardThisPatrol = true;
        }

        /// <summary>
        /// 初始化老师AI
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            if (config == null)
            {
                Debug.LogError("[TeacherAI] Initialize 失败：config 为 null");
                return;
            }

            levelConfig = config;
            patrolCount = 0;
            flashPanicAccumulated = 0f;
            hasUsedCardThisPatrol = false;
            IsPatrolPaused = false;

            // 获取系统引用
            emotionSystem = Game.Emotion.EmotionSystem.Instance;
            cardReadingSystem = FindFirstObjectByType<Game.Card.CardReadingSystem>();
            playerState = Game.Data.PlayerState.Instance;

            EventCenter.Instance.AddEventListener<int>(E_EventType.CardReadComplete, OnCardUsed);

            EnterState(TeacherState.Idle);
            Debug.Log("[TeacherAI] 初始化完成");
        }

        /// <summary>卡牌2040(前往走廊)触发后暂停巡逻</summary>
        public static bool IsPatrolPaused { get; set; }

        void Update()
        {
            // 死循环安全阀
            if (Time.frameCount != safetyUpdateFrame)
            {
                safetyUpdateFrame = Time.frameCount;
                safetyUpdateCount = 0;
            }
            safetyUpdateCount++;
            if (safetyUpdateCount > SAFETY_UPDATE_MAX)
            {
                Debug.LogError($"[TeacherAI] Update 同一帧内被调用超过 {SAFETY_UPDATE_MAX} 次，强制返回防止死循环");
                return;
            }

            if (gameFlow != null && (gameFlow.IsPaused() || gameFlow.IsGameOver() || IsPatrolPaused))
                return;

            if (stateTimer > 0)
            {
                stateTimer -= Time.deltaTime;

                // 更新接近进度
                if (currentState == TeacherState.Approaching)
                {
                    if (targetDuration > 0)
                    {
                        approachProgress = Mathf.Min(1f, 1f - (stateTimer / targetDuration));
                    }
                }
                else if (currentState == TeacherState.Leaving)
                {
                    if (targetDuration > 0)
                    {
                        approachProgress = Mathf.Max(0f, stateTimer / targetDuration);
                    }
                }

                // 查寝判定：在检查期间持续判定
                if (currentState == TeacherState.Inspecting)
                {
                    PerformInspectionCheck();
                }

                // 脚步声根据接近进度调整音量
                if (footstepSource != null)
                {
                    float sv = MusicMgr.Instance.SoundValue;
                    float targetVol;
                    if (currentState == TeacherState.Approaching)
                        targetVol = Mathf.Lerp(0.65f * sv, sv, approachProgress);
                    else // Leaving
                        targetVol = approachProgress * sv;
                    footstepSource.volume = Mathf.Lerp(footstepSource.volume, targetVol, Time.deltaTime * 4f);
                }

                if (stateTimer <= 0)
                {
                    OnStateTimerComplete();
                }
            }
        }

        void EnterState(TeacherState newState)
        {
            ExitState(currentState);
            currentState = newState;

            switch (newState)
            {
                case TeacherState.Idle:
                    EnterIdle();
                    break;

                case TeacherState.Approaching:
                    EnterApproaching();
                    break;

                case TeacherState.Inspecting:
                    EnterInspecting();
                    break;

                case TeacherState.Leaving:
                    EnterLeaving();
                    break;
            }

            // 触发状态变化事件
            EventCenter.Instance.EventTrigger(E_EventType.TeacherStateChanged, newState);
        }

        void EnterIdle()
        {
            StopFootstep();
            stateTimer = Mathf.Max(0.5f, Random.Range(levelConfig.patrolIntervals.x, levelConfig.patrolIntervals.y));
            targetDuration = stateTimer;
            isVisible = false;
            approachProgress = 0f;
            Debug.Log($"[TeacherAI] 进入空闲状态，等待 {stateTimer:F1}秒");
        }

        void EnterApproaching()
        {
            stateTimer = Mathf.Max(0.5f, Random.Range(levelConfig.patrolTime.x, levelConfig.patrolTime.y));
            targetDuration = stateTimer;
            isVisible = false;
            approachProgress = 0f;

            // 未使用卡牌 → 目光查寝（无情绪判定，无手电动画）
            // 使用过卡牌 → 手电筒查寝（判断情绪值 + 手电动画）
            currentInspectType = hasUsedCardThisPatrol ? InspectType.Flash : InspectType.Eye;

            Debug.Log($"[TeacherAI] 开始接近 ({currentInspectType}检查)，预计 {stateTimer:F1}秒到达");

            // 触发脚步声事件
            EventCenter.Instance.EventTrigger(E_EventType.TeacherFootstepStart, currentInspectType);

            // 播放脚步声（循环），音量从25%渐增至100%
            MusicMgr.Instance.PlaySound("DXH_SOUND/08.脚步声", true, (source) =>
            {
                footstepSource = source;
                footstepSource.volume = 0.65f * MusicMgr.Instance.SoundValue;
            });
        }

        void EnterInspecting()
        {
            if (currentInspectType == InspectType.Eye)
            {
                stateTimer = Mathf.Max(0.5f, Random.Range(levelConfig.eyeCheckDuration.x, levelConfig.eyeCheckDuration.y));
            }
            else
            {
                stateTimer = Mathf.Max(0.5f, Random.Range(levelConfig.flashCheckDuration.x, levelConfig.flashCheckDuration.y));
            }
            targetDuration = stateTimer;
            isVisible = true;
            approachProgress = 1f;

            Debug.Log($"[TeacherAI] 开始检查 ({currentInspectType})，持续 {stateTimer:F1}秒");

            // 停止脚步声
            StopFootstep();

            // 手电筒检查时显示老师画面 + 手电动画
            if (currentInspectType == InspectType.Flash)
            {
                GamePanel.Instance?.ShowTeacherImage();
                GamePanel.Instance?.ShowFlashlightOverlay(2f);
            }

            // 触发检查开始事件
            EventCenter.Instance.EventTrigger(E_EventType.TeacherInspectStart, currentInspectType);
        }

        void EnterLeaving()
        {
            stateTimer = Mathf.Max(0.5f, Random.Range(levelConfig.patrolTime.x, levelConfig.patrolTime.y));
            targetDuration = stateTimer;
            isVisible = false;
            patrolCount++;
            hasUsedCardThisPatrol = false;

            // 隐藏老师画面
            GamePanel.Instance?.HideTeacherImage();

            // 重新播放脚步声（循环），音量从满渐减，由Update根据approachProgress调整
            MusicMgr.Instance.PlaySound("DXH_SOUND/08.脚步声", true, (source) =>
            {
                footstepSource = source;
                footstepSource.volume = MusicMgr.Instance.SoundValue;
            });

            Debug.Log($"[TeacherAI] 离开中，耗时 {stateTimer:F1}秒 (第{patrolCount}次巡逻)");

            // 触发检查结束事件
            EventCenter.Instance.EventTrigger(E_EventType.TeacherInspectEnd);
        }

        /// <summary>
        /// 执行查寝判定
        /// </summary>
        void PerformInspectionCheck()
        {
            // 如果游戏已结束，不再检查
            if (gameFlow != null && gameFlow.IsGameOver())
                return;

            // 检查条件是否满足
            bool isCaught = CheckPlayerCaught();

            if (isCaught)
            {
                Debug.Log($"[TeacherAI] 玩家被抓！检查类型: {currentInspectType}");

                // 播放敲门声
                MusicMgr.Instance.PlaySound("DXH_SOUND/SOUND2/26.敲门声");

                // 触发被抓音效事件
                EventCenter.Instance.EventTrigger(E_EventType.PlayCaughtSound);

                // 触发被抓事件
                EventCenter.Instance.EventTrigger(E_EventType.PlayerCaught);

                // 通知游戏流程
                if (gameFlow != null)
                {
                    gameFlow.OnPlayerCaught();
                }

                // 立即离开
                EnterState(TeacherState.Leaving);
            }
        }

        /// <summary>
        /// 检查玩家是否被抓
        /// </summary>
        bool CheckPlayerCaught()
        {
            // 获取玩家状态
            bool playerInBed = playerState != null && playerState.IsInBed();
            bool playerEyesClosed = playerState != null && playerState.IsEyesClosed();
            bool isReading = cardReadingSystem != null ? cardReadingSystem.IsReading() : GamePanel.IsCardReading;

            // 获取当前读条片段
            Game.Data.Segment currentSegment = null;
            bool isInUninterruptibleSegment = false;

            if (cardReadingSystem != null && isReading)
            {
                currentSegment = cardReadingSystem.GetCurrentSegment();
                isInUninterruptibleSegment = (currentSegment != null && !currentSegment.isInterruptible);
            }
            else if (isReading)
            {
                isInUninterruptibleSegment = GamePanel.IsInUninterruptibleSegment;
            }

            // 获取情绪值
            int totalEmotion = 0;
            bool isEmotionCritical = false;

            if (emotionSystem != null)
            {
                totalEmotion = emotionSystem.GetTotalEmotion();
                isEmotionCritical = emotionSystem.IsCaughtByCriticalValue();
            }

            // 公共基础检查（眼神和手电筒都需要）
            if (isInUninterruptibleSegment)
            {
                Debug.Log($"[TeacherAI] {currentInspectType}检查发现：不可打断片段期间，闭眼无效！");
                return true;
            }

            if (!playerInBed)
            {
                Debug.Log($"[TeacherAI] {currentInspectType}检查发现：玩家未卧床");
                return true;
            }

            if (!playerEyesClosed)
            {
                Debug.Log($"[TeacherAI] {currentInspectType}检查发现：玩家未闭眼");
                return true;
            }

            // 根据检查类型判定额外条件
            if (currentInspectType == InspectType.Eye)
            {
                // 眼神检查无额外条件
            }
            else if (currentInspectType == InspectType.Flash)
            {
                // 手电筒额外检查情绪值
                if (isEmotionCritical)
                {
                    Debug.Log($"[TeacherAI] 手电筒检查发现：情绪值超标 {totalEmotion} >= {levelConfig.criticalValue}");
                    return true;
                }

                // 手电筒持续增加慌乱值（累积小数部分避免 RoundToInt 截断）
                if (emotionSystem != null)
                {
                    flashPanicAccumulated += levelConfig.flashPanicPerSec * Time.deltaTime;
                    int increase = Mathf.FloorToInt(flashPanicAccumulated);
                    if (increase > 0)
                    {
                        emotionSystem.ChangePanic(increase);
                        flashPanicAccumulated -= increase;
                    }
                }
            }

            return false;
        }

        void OnStateTimerComplete()
        {
            // 游戏结束时不再转换状态
            if (gameFlow != null && gameFlow.IsGameOver())
                return;

            switch (currentState)
            {
                case TeacherState.Idle:
                    EnterState(TeacherState.Approaching);
                    break;

                case TeacherState.Approaching:
                    EnterState(TeacherState.Inspecting);
                    break;

                case TeacherState.Inspecting:
                    EnterState(TeacherState.Leaving);
                    break;

                case TeacherState.Leaving:
                    EnterState(TeacherState.Idle);
                    break;
            }
        }

        void StopFootstep()
        {
            if (footstepSource != null)
            {
                MusicMgr.Instance.StopSound(footstepSource);
                footstepSource = null;
            }
        }

        void ExitState(TeacherState state)
        {
            // 状态退出时的清理工作
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public TeacherState GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// 获取当前检查类型
        /// </summary>
        public InspectType GetCurrentInspectType()
        {
            return currentInspectType;
        }

        /// <summary>
        /// 是否正在检查
        /// </summary>
        public bool IsInspecting()
        {
            return currentState == TeacherState.Inspecting;
        }

        /// <summary>
        /// 是否正在接近
        /// </summary>
        public bool IsApproaching()
        {
            return currentState == TeacherState.Approaching;
        }

        /// <summary>
        /// 获取接近进度 (0-1)
        /// </summary>
        public float GetApproachProgress()
        {
            return approachProgress;
        }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible()
        {
            return isVisible;
        }
    }
}
