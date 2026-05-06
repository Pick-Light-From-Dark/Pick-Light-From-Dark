using UnityEngine;
using Game.Emotion;
using Game.Config;
using Game.Data;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 情绪系统手动测试器
    /// 开发者在 Inspector 调参，运行后通过 OnGUI 按钮或快捷键触发
    /// 支持根据情绪值和姿态（床上/站立）自动切换角色动画
    /// </summary>
    public class EmotionSystemTester : MonoBehaviour
    {
        [Header("测试关卡配置")]
        [Tooltip("拖入 TestLevelConfig 或自定义 LevelConfigSO")]
        public LevelConfigSO testConfig;

        [Header("测试参数")]
        [Tooltip("ChangePanic 测试用 delta")]
        public int panicDelta = 10;
        [Tooltip("ChangeExcite 测试用 delta")]
        public int exciteDelta = 10;
        [Tooltip("DecreaseEmotionWhileEyeClose 测试用模拟时间(秒)")]
        public float simulateEyeCloseSeconds = 1f;
        [Tooltip("DecreaseEmotionWhileEyeClose 测试用降率")]
        public float decreaseRate = 5f;

        [Header("角色动画")]
        [Tooltip("是否显示角色精灵")]
        public bool showCharacterSprite = true;
        [Tooltip("床上姿态的 AnimatorController [0]=Blink [1]=LittleExcited [2]=Excited")]
        public RuntimeAnimatorController[] luYingControllers = new RuntimeAnimatorController[3];
        [Tooltip("站立姿态的 AnimatorController [0]=Blink [1]=LittleExcited [2]=Excited")]
        public RuntimeAnimatorController[] luYingStandControllers = new RuntimeAnimatorController[3];

        [Header("情绪阈值")]
        [Tooltip("达到小兴奋状态的兴奋值阈值")]
        public int littleExcitedThreshold = 50;
        [Tooltip("达到高兴奋状态的兴奋值阈值")]
        public int highExcitedThreshold = 70;

        [Header("UI 显示")]
        public bool showOnGUI = true;
        public KeyCode initializeKey = KeyCode.F1;
        public KeyCode applyPanicKey = KeyCode.F2;
        public KeyCode applyExciteKey = KeyCode.F3;
        public KeyCode runAllKey = KeyCode.F4;

        private EmotionSystem emotionSystem;
        private PlayerState playerState;
        private Animator animator;
        private SpriteRenderer spriteRenderer;

        private enum CharacterState { Blink, LittleExcited, Excited }
        private CharacterState currentState = CharacterState.Blink;
        private bool lastInBed = true;

        void Start()
        {
            emotionSystem = EmotionSystem.Instance;
            playerState = PlayerState.Instance;

            EnsureCharacterComponents();
            UpdateCharacterAnimation(true);

            Debug.Log("[EmotionSystemTester] 已就绪。F1=Initialize / F2=ChangePanic / F3=ChangeExcite / F4=RunAll");
        }

        void Update()
        {
            if (Input.GetKeyDown(initializeKey)) DoInitialize();
            if (Input.GetKeyDown(applyPanicKey)) DoChangePanic();
            if (Input.GetKeyDown(applyExciteKey)) DoChangeExcite();
            if (Input.GetKeyDown(runAllKey)) RunAllTests();

            UpdateCharacterAnimation(false);
        }

        // ========== 角色动画控制 ==========

        void EnsureCharacterComponents()
        {
            if (!showCharacterSprite) return;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }

            bool hasLuYing = luYingControllers != null && luYingControllers.Length >= 3
                             && luYingControllers[0] != null && luYingControllers[1] != null && luYingControllers[2] != null;
            bool hasStand = luYingStandControllers != null && luYingStandControllers.Length >= 3
                            && luYingStandControllers[0] != null && luYingStandControllers[1] != null && luYingStandControllers[2] != null;

            if (!hasLuYing || !hasStand)
            {
                Debug.LogWarning("[EmotionSystemTester] AnimatorController 未完全配置。请在 Inspector 中将 LuYing 和 LuYingStand 的 Controller 拖拽到对应字段：[0]=Blink [1]=LittleExcited [2]=Excited");
            }
        }

        void UpdateCharacterAnimation(bool forceUpdate)
        {
            if (!showCharacterSprite || animator == null || playerState == null || emotionSystem == null)
                return;

            bool inBed = playerState.IsInBed();
            var info = emotionSystem.GetEmotionInfo();

            CharacterState newState;
            if (info.exciteValue >= highExcitedThreshold)
                newState = CharacterState.Excited;
            else if (info.exciteValue >= littleExcitedThreshold)
                newState = CharacterState.LittleExcited;
            else
                newState = CharacterState.Blink;

            if (forceUpdate || newState != currentState || inBed != lastInBed)
            {
                currentState = newState;
                lastInBed = inBed;

                RuntimeAnimatorController[] controllers = inBed ? luYingControllers : luYingStandControllers;
                if (controllers != null && (int)newState < controllers.Length && controllers[(int)newState] != null)
                {
                    animator.runtimeAnimatorController = controllers[(int)newState];
                }
            }
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            const int w = 320, h = 24;
            int x = 10, y = 10;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 340), "EmotionSystem 测试器");
            y += 25;

            if (emotionSystem != null)
            {
                var info = emotionSystem.GetEmotionInfo();
                GUI.Label(new Rect(x, y, w, h), $"状态: {info}"); y += h;
                GUI.Label(new Rect(x, y, w, h), $"超临界: {emotionSystem.IsCaughtByCriticalValue()}"); y += h + 4;
            }
            else
            {
                GUI.Label(new Rect(x, y, w, h), "EmotionSystem 实例未就绪"); y += h + 4;
            }

            // 角色状态显示
            if (showCharacterSprite && playerState != null)
            {
                string pose = playerState.IsInBed() ? "床上" : "站立";
                GUI.Label(new Rect(x, y, w, h), $"姿态: {pose}  动画: {currentState}"); y += h + 4;
            }

            GUI.Label(new Rect(x, y, 100, h), "Panic Δ:");
            int.TryParse(GUI.TextField(new Rect(x + 100, y, 60, h), panicDelta.ToString()), out panicDelta); y += h;

            GUI.Label(new Rect(x, y, 100, h), "Excite Δ:");
            int.TryParse(GUI.TextField(new Rect(x + 100, y, 60, h), exciteDelta.ToString()), out exciteDelta); y += h + 4;

            if (GUI.Button(new Rect(x, y, w, h), "[F1] Initialize 关卡配置")) DoInitialize(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "[F2] ChangePanic")) DoChangePanic(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "[F3] ChangeExcite")) DoChangeExcite(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), $"模拟闭眼{simulateEyeCloseSeconds}秒(降率{decreaseRate})")) DoDecreaseWhileEyeClose(); y += h;

            // 上床/下床按钮
            if (playerState != null)
            {
                string bedBtn = playerState.IsInBed() ? "下床 (测试站立姿态)" : "上床 (测试床上姿态)";
                if (GUI.Button(new Rect(x, y, w, h), bedBtn))
                {
                    playerState.SetInBed(!playerState.IsInBed());
                    UpdateCharacterAnimation(true);
                }
                y += h;
            }

            if (GUI.Button(new Rect(x, y, w, h), "[F4] 一键跑全部用例")) RunAllTests(); y += h;
        }

        [ContextMenu("Initialize")]
        public void DoInitialize()
        {
            if (testConfig == null)
            {
                Debug.LogError("[EmotionSystemTester] testConfig 未指定");
                return;
            }
            emotionSystem.Initialize(testConfig);
            Debug.Log($"[EmotionSystemTester] Initialize 完成 → {emotionSystem.GetEmotionInfo()}");
        }

        [ContextMenu("ChangePanic")]
        public void DoChangePanic()
        {
            int before = emotionSystem.GetEmotionInfo().panicValue;
            emotionSystem.ChangePanic(panicDelta);
            int after = emotionSystem.GetEmotionInfo().panicValue;
            Debug.Log($"[EmotionSystemTester] ChangePanic({panicDelta}) {before}→{after}");
        }

        [ContextMenu("ChangeExcite")]
        public void DoChangeExcite()
        {
            int before = emotionSystem.GetEmotionInfo().exciteValue;
            emotionSystem.ChangeExcite(exciteDelta);
            int after = emotionSystem.GetEmotionInfo().exciteValue;
            Debug.Log($"[EmotionSystemTester] ChangeExcite({exciteDelta}) {before}→{after}");
        }

        [ContextMenu("DecreaseWhileEyeClose")]
        public void DoDecreaseWhileEyeClose()
        {
            int before = emotionSystem.GetTotalEmotion();
            emotionSystem.DecreaseEmotionWhileEyeClose(simulateEyeCloseSeconds, decreaseRate);
            int after = emotionSystem.GetTotalEmotion();
            Debug.Log($"[EmotionSystemTester] DecreaseEmotionWhileEyeClose({simulateEyeCloseSeconds}s @ {decreaseRate}/s) total {before}→{after}");
        }

        [ContextMenu("RunAllTests")]
        public void RunAllTests()
        {
            Debug.Log("=== EmotionSystem 一键测试 ===");
            DoInitialize();
            DoChangePanic();
            DoChangeExcite();
            // 边界测试
            emotionSystem.ChangePanic(999);
            int high = emotionSystem.GetEmotionInfo().panicValue;
            emotionSystem.ChangePanic(-999);
            int low = emotionSystem.GetEmotionInfo().panicValue;
            Debug.Log($"[EmotionSystemTester] Clamp 测试: 上限={high} 下限={low}");
            DoDecreaseWhileEyeClose();
            Debug.Log("=== 完成 ===");
        }
    }
}
