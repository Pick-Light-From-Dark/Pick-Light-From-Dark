using UnityEngine;
using Game.Config;

namespace Game.EyeClose
{
    /// <summary>
    /// 闭眼系统
    /// 管理闭眼期间的情绪值降低和时间加速
    /// </summary>
    public class EyeCloseSystem : SingletonAutoMono<EyeCloseSystem>
    {
        [Header("闭眼状态")]
        [SerializeField] private bool isEyesClosed;

        [Header("闭眼时长")]
        [SerializeField] private float eyeCloseTimer;
        [SerializeField] private float timeAccelerationThreshold = 10f;

        [Header("时间加速")]
        [SerializeField] private float accelerationMultiplier = 2f;

        private Game.Data.PlayerState playerState;
        private Game.Emotion.EmotionSystem emotionSystem;
        private Game.Flow.GameFlowController gameFlow;
        private LevelConfigSO levelConfig;
        private bool isTimeAccelerated;

        void Start()
        {
            // 获取系统引用
            playerState = Game.Data.PlayerState.Instance;
            emotionSystem = Game.Emotion.EmotionSystem.Instance;
            gameFlow = Game.Flow.GameFlowController.Instance;
        }

        /// <summary>
        /// 从关卡配置初始化参数
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            levelConfig = config;
            if (levelConfig != null)
            {
                timeAccelerationThreshold = levelConfig.eyeCloseAccelerationThreshold;
                accelerationMultiplier = levelConfig.eyeCloseAccelerationMultiplier;
                Debug.Log($"[EyeCloseSystem] 初始化 阈值={timeAccelerationThreshold}s 倍率={accelerationMultiplier}");
            }
        }

        void Update()
        {
            if (gameFlow != null && (gameFlow.IsPaused() || gameFlow.IsGameOver()))
                return;

            // 同步闭眼状态
            isEyesClosed = playerState != null && playerState.IsEyesClosed();

            if (isEyesClosed)
            {
                eyeCloseTimer += Time.deltaTime;

                // 闭眼降低情绪值
                if (emotionSystem != null)
                {
                    float decreaseRate = levelConfig != null ? levelConfig.eyeClosePanicDecreasePerSec : 1f;
                    emotionSystem.DecreaseEmotionWhileEyeClose(Time.deltaTime, decreaseRate);
                }

                // 闭眼过久加速时间
                if (eyeCloseTimer >= timeAccelerationThreshold && !isTimeAccelerated)
                {
                    EnableTimeAcceleration();
                }
            }
            else
            {
                // 睁眼后重置
                if (eyeCloseTimer > 0)
                {
                    eyeCloseTimer = 0f;
                    DisableTimeAcceleration();
                }
            }
        }

        /// <summary>
        /// 启用时间加速
        /// </summary>
        void EnableTimeAcceleration()
        {
            if (isTimeAccelerated) return;

            isTimeAccelerated = true;
            Time.timeScale = accelerationMultiplier;

            Debug.Log($"[EyeCloseSystem] 闭眼过久({eyeCloseTimer:F1}秒)，时间加速 x{accelerationMultiplier}");
            EventCenter.Instance.EventTrigger(E_EventType.EyeCloseTimeAccelerated);
        }

        /// <summary>
        /// 禁用时间加速
        /// </summary>
        void DisableTimeAcceleration()
        {
            if (!isTimeAccelerated) return;

            isTimeAccelerated = false;
            Time.timeScale = 1f;

            Debug.Log($"[EyeCloseSystem] 睁眼，恢复正常时间流速");
            EventCenter.Instance.EventTrigger(E_EventType.EyeCloseTimeNormal);
        }

        /// <summary>
        /// 获取闭眼时长
        /// </summary>
        public float GetEyeCloseDuration()
        {
            return eyeCloseTimer;
        }

        /// <summary>
        /// 是否时间加速中
        /// </summary>
        public bool IsTimeAccelerated()
        {
            return isTimeAccelerated;
        }

        /// <summary>
        /// 重置闭眼系统
        /// </summary>
        public void Reset()
        {
            eyeCloseTimer = 0f;
            DisableTimeAcceleration();
        }
    }
}
