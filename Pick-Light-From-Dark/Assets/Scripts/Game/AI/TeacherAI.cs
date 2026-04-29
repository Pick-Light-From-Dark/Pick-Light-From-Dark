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
        private float targetDuration;

        void Awake()
        {
            gameFlow = GameFlowController.Instance;
        }

        /// <summary>
        /// 初始化老师AI
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            levelConfig = config;
            patrolCount = 0;
            EnterState(TeacherState.Idle);
            Debug.Log("[TeacherAI] 初始化完成");
        }

        void Update()
        {
            if (gameFlow != null && (gameFlow.IsPaused() || gameFlow.IsGameOver()))
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
            stateTimer = Random.Range(levelConfig.patrolIntervals.x, levelConfig.patrolIntervals.y);
            targetDuration = stateTimer;
            isVisible = false;
            approachProgress = 0f;
            Debug.Log($"[TeacherAI] 进入空闲状态，等待 {stateTimer:F1}秒");
        }

        void EnterApproaching()
        {
            stateTimer = Random.Range(levelConfig.patrolTime.x, levelConfig.patrolTime.y);
            targetDuration = stateTimer;
            isVisible = false;
            approachProgress = 0f;

            // 根据巡逻次数增加手电筒概率
            float flashProb = Mathf.Min(0.6f, 0.3f + patrolCount * 0.05f);
            currentInspectType = Random.value < flashProb ? InspectType.Flash : InspectType.Eye;

            Debug.Log($"[TeacherAI] 开始接近 ({currentInspectType}检查)，预计 {stateTimer:F1}秒到达");

            // 触发脚步声事件
            EventCenter.Instance.EventTrigger(E_EventType.TeacherFootstepStart, currentInspectType);
        }

        void EnterInspecting()
        {
            if (currentInspectType == InspectType.Eye)
            {
                stateTimer = Random.Range(levelConfig.eyeCheckDuration.x, levelConfig.eyeCheckDuration.y);
            }
            else
            {
                stateTimer = Random.Range(levelConfig.flashCheckDuration.x, levelConfig.flashCheckDuration.y);
            }
            targetDuration = stateTimer;
            isVisible = true;
            approachProgress = 1f;

            Debug.Log($"[TeacherAI] 开始检查 ({currentInspectType})，持续 {stateTimer:F1}秒");

            // 触发检查开始事件
            EventCenter.Instance.EventTrigger(E_EventType.TeacherInspectStart, currentInspectType);
        }

        void EnterLeaving()
        {
            stateTimer = Random.Range(1.5f, 2.5f);
            targetDuration = stateTimer;
            isVisible = false;
            patrolCount++;

            Debug.Log($"[TeacherAI] 离开中，耗时 {stateTimer:F1}秒 (第{patrolCount}次巡逻)");

            // 触发检查结束事件
            EventCenter.Instance.EventTrigger(E_EventType.TeacherInspectEnd);
        }

        void OnStateTimerComplete()
        {
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
