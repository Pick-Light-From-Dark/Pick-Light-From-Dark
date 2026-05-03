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

            // 订阅游戏流程事件，避免 PauseGame/ResumeGame 强制覆写 timeScale 后丢失加速状态
            EventCenter.Instance.AddEventListener(E_EventType.GameResume, OnGameResume);
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameEnd);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameEndWithReason);
        }

        void OnDestroy()
        {
            // EventCenter 是非 Mono 单例，应用周期常驻；RemoveEventListener 内部对 dict 做了存在性判断，安全可调
            EventCenter.Instance.RemoveEventListener(E_EventType.GameResume, OnGameResume);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameEnd);
            EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameEndWithReason);
        }

        /// <summary>
        /// 游戏恢复后，如果还在加速状态需要重新写入 timeScale
        /// （PauseGame 把 timeScale 设为 0，ResumeGame 强制写回 1，会丢失加速）
        /// </summary>
        private void OnGameResume()
        {
            if (isTimeAccelerated)
            {
                Time.timeScale = accelerationMultiplier;
                Debug.Log($"[EyeCloseSystem] 游戏恢复，重新应用闭眼加速 x{accelerationMultiplier}");
            }
        }

        /// <summary>
        /// 游戏结束（胜利）时复位加速状态
        /// </summary>
        private void OnGameEnd()
        {
            if (isTimeAccelerated)
            {
                isTimeAccelerated = false;
                eyeCloseTimer = 0f;
                Debug.Log("[EyeCloseSystem] 游戏结束，复位加速状态");
            }
        }

        /// <summary>
        /// 游戏结束（失败）时复位加速状态（GameLose 携带 string 参数）
        /// </summary>
        private void OnGameEndWithReason(string reason)
        {
            OnGameEnd();
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
