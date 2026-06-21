using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Game.Test
{
    /// <summary>
    /// 新手引导系统 — 单例控制器
    /// 监听关卡启动事件，对未完成引导的关卡按步骤执行引导流程
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TutorialManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[TutorialManager]");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<TutorialManager>();
                    }
                }
                return _instance;
            }
        }
        private static TutorialManager _instance;

        [Header("配置")]
        [Tooltip("按关卡ID索引的引导配置")]
        public List<TutorialConfigSO> tutorialConfigs = new List<TutorialConfigSO>();

        [Header("调试")]
        public bool logOperations = true;

        /// <summary>当前是否正在执行引导</summary>
        public static bool IsTutorialActive => _instance != null && _instance._state != TutorialState.Idle;

        // 内部状态
        private enum TutorialState { Idle, Active, StepTransition }
        private TutorialState _state = TutorialState.Idle;
        private TutorialConfigSO _currentConfig;
        private int _currentStepIndex;
        private TutorialPanel _tutorialPanel;
        private GameObject _currentTarget;
        private UnityAction _targetClickHook;
        private float _prevTimeScale;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            HookEvents();
        }

        void OnDestroy()
        {
            UnhookEvents();
            if (_instance == this) _instance = null;
        }

        // ==================== 事件接入 ====================

        void HookEvents()
        {
            EventCenter.Instance.AddEventListener(E_EventType.GameStart, OnGameStart);
        }

        void UnhookEvents()
        {
            if (EventCenter.Instance != null)
                EventCenter.Instance.RemoveEventListener(E_EventType.GameStart, OnGameStart);
        }

        /// <summary>关卡游玩阶段开始时检测是否需要引导</summary>
        void OnGameStart()
        {
            // 获取当前关卡ID
            int levelId = GetCurrentLevelId();
            Debug.Log($"[Tutorial] OnGameStart: levelId={levelId}, SaveSystem={(CrossLevelSaveSystem.Instance != null ? "就绪" : "null")}");

            if (levelId <= 0)
            {
                Log($"[Tutorial] 无法获取关卡ID，跳过引导检测");
                return;
            }

            // 检查是否已完成
            bool alreadyDone = CrossLevelSaveSystem.Instance?.HasCompletedTutorial(levelId) ?? false;
            Debug.Log($"[Tutorial] 关卡{levelId}引导已检查: alreadyDone={alreadyDone}");
            if (alreadyDone)
            {
                Log($"[Tutorial] 关卡{levelId}引导已完成，跳过");
                return;
            }

            // 查找配置
            var config = FindConfig(levelId);
            if (config == null || !config.HasSteps)
            {
                Log($"[Tutorial] 关卡{levelId}无引导配置");
                return;
            }

            Log($"[Tutorial] 关卡{levelId}引导开始（共{config.steps.Count}步）");
            StartTutorial(config);
        }

        int GetCurrentLevelId()
        {
            // 从LevelFlowCoordinator或场景名推断
            var coord = FindObjectOfType<Game.Flow.LevelFlowCoordinator>();
            if (coord != null) return coord.levelId;

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Level") && int.TryParse(sceneName.Substring(5), out int id))
                return id;

            return 0;
        }

        TutorialConfigSO FindConfig(int levelId)
        {
            foreach (var cfg in tutorialConfigs)
            {
                if (cfg != null && cfg.levelId == levelId) return cfg;
            }
            // 回退：从 Resources 加载
            var resCfg = Resources.Load<TutorialConfigSO>($"Tutorial/TutorialConfig_Level{levelId}");
            if (resCfg != null && resCfg.HasSteps)
            {
                tutorialConfigs.Add(resCfg);
                return resCfg;
            }
            return null;
        }

        // ==================== 引导生命周期 ====================

        /// <summary>开始引导</summary>
        public void StartTutorial(TutorialConfigSO config)
        {
            if (_state != TutorialState.Idle) return;
            _currentConfig = config;
            _currentStepIndex = 0;
            _state = TutorialState.Active;

            // 暂停游戏
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // 暂停老师巡逻
            Game.AI.TeacherAI.IsPatrolPaused = true;

            // 创建引导面板
            if (_tutorialPanel == null)
            {
                var panelGo = new GameObject("TutorialPanel");
                DontDestroyOnLoad(panelGo);
                _tutorialPanel = panelGo.AddComponent<TutorialPanel>();
            }

            // 延迟一帧等待UI初始化完毕后显示第一步
            StartCoroutine(ShowFirstStepNextFrame());
        }

        IEnumerator ShowFirstStepNextFrame()
        {
            yield return null; // 等一帧确保GamePanel等UI已就绪
            ShowStep(0);
        }

        /// <summary>完成引导</summary>
        void CompleteTutorial()
        {
            Log($"[Tutorial] 关卡{_currentConfig?.levelId}引导完成");

            // 标记完成
            int levelId = _currentConfig?.levelId ?? 0;
            if (levelId > 0)
                CrossLevelSaveSystem.Instance?.MarkTutorialCompleted(levelId);

            // 恢复游戏
            Time.timeScale = _prevTimeScale;
            Game.AI.TeacherAI.IsPatrolPaused = false;

            // 清理
            if (_tutorialPanel != null)
            {
                _tutorialPanel.HideAll();
                Destroy(_tutorialPanel.gameObject);
                _tutorialPanel = null;
            }
            RemoveTargetHook();
            _currentConfig = null;
            _currentStepIndex = 0;
            _state = TutorialState.Idle;
        }

        // ==================== 步骤控制 ====================

        void ShowStep(int index)
        {
            if (_currentConfig == null || index >= _currentConfig.steps.Count)
            {
                CompleteTutorial();
                return;
            }

            var step = _currentConfig.steps[index];
            Log($"[Tutorial] 步骤{index + 1}/{_currentConfig.steps.Count}: {step.stepId} — {step.instructionText}");

            // 查找目标（空targetName表示纯文字步骤，无目标）
            _currentTarget = FindTarget(step.targetObjectName);
            if (_currentTarget == null && !string.IsNullOrEmpty(step.targetObjectName))
            {
                Debug.LogWarning($"[Tutorial] 未找到目标物体: {step.targetObjectName}");
            }

            // 获取目标屏幕矩形
            Rect targetRect = GetTargetScreenRect(_currentTarget, step);

            // 显示引导UI
            if (_tutorialPanel != null)
                _tutorialPanel.ShowStep(targetRect, step.instructionText, step.indicator, step.showMask);

            // 挂载完成监听
            _currentStepIndex = index;
            HookTargetInteraction(step);
        }

        void AdvanceToNextStep()
        {
            RemoveTargetHook();
            _currentStepIndex++;
            if (_currentStepIndex < _currentConfig.steps.Count)
            {
                ShowStep(_currentStepIndex);
            }
            else
            {
                CompleteTutorial();
            }
        }

        GameObject FindTarget(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return null;

            // 先按全名查找
            var go = GameObject.Find(objectName);
            if (go != null) return go;

            // 在所有 Canvas 中递归查找
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                var found = FindInChildren(canvas.transform, objectName);
                if (found != null) return found;
            }

            return null;
        }

        GameObject FindInChildren(Transform parent, string name)
        {
            if (parent.name == name) return parent.gameObject;
            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindInChildren(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        Rect GetTargetScreenRect(GameObject target, TutorialStep step)
        {
            float sw = Screen.width;
            float sh = Screen.height;
            float fw = step.cutoutSize.x;
            float fh = step.cutoutSize.y;
            float ox = step.cutoutOffset.x;
            float oy = step.cutoutOffset.y;

            // 非Target锚点：直接按屏幕边角定位
            switch (step.cutoutAnchor)
            {
                case CutoutAnchor.TopRight:
                    return new Rect(sw - fw - ox, sh - fh - oy, fw, fh);
                case CutoutAnchor.BottomLeft:
                    return new Rect(ox, oy, fw, fh);
                case CutoutAnchor.Center:
                    return new Rect((sw - fw) / 2f + ox, (sh - fh) / 2f + oy, fw, fh);
            }

            // Target模式：跟随目标元素
            if (target == null)
            {
                return new Rect((sw - fw) / 2f + ox, (sh - fh) / 2f + oy, fw, fh);
            }

            var rt = target.GetComponent<RectTransform>();
            if (rt == null)
            {
                var screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(target.transform.position) : Vector3.zero;
                return new Rect(screenPos.x - fw / 2f + ox, screenPos.y - fh / 2f + oy, fw, fh);
            }

            // 用Canvas判定坐标系
            var canvas = rt.GetComponentInParent<Canvas>();
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector3 bl, tr;
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                bl = corners[0]; tr = corners[2];
            }
            else
            {
                Camera cam = canvas?.worldCamera ?? Camera.main;
                bl = cam != null ? cam.WorldToScreenPoint(corners[0]) : corners[0];
                tr = cam != null ? cam.WorldToScreenPoint(corners[2]) : corners[2];
            }

            float x = bl.x;
            float y = bl.y;
            float w = tr.x - bl.x;
            float h = tr.y - bl.y;

            Debug.Log($"[Tutorial] 目标'{target.name}': BL({x:F0},{y:F0}) TR({tr.x:F0},{tr.y:F0}), 大小({w:F0}x{h:F0}), 镂空({fw:F0}x{fh:F0}), 屏幕({sw}x{sh})");

            return new Rect(x + (w - fw) / 2f + ox, y + (h - fh) / 2f + oy, fw, fh);
        }

        // ==================== 目标交互监听 ====================

        void HookTargetInteraction(TutorialStep step)
        {
            RemoveTargetHook();

            if (step.trigger == TutorialTrigger.ClickTarget && _currentTarget != null)
            {
                // ClickTarget模式：遮罩阻挡非镂空区域，只监听目标点击
                _tutorialPanel?.SetInteractionMode(true);
                var btn = _currentTarget.GetComponent<Button>();
                if (btn != null)
                {
                    _targetClickHook = () =>
                    {
                        Log($"[Tutorial] 目标点击: {step.stepId}");
                        AdvanceToNextStep();
                    };
                    btn.onClick.AddListener(_targetClickHook);
                }
                else
                {
                    var et = _currentTarget.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                    if (et == null) et = _currentTarget.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                    var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                    entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
                    UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> triggerCb = (_) => AdvanceToNextStep();
                    entry.callback.AddListener(triggerCb);
                    et.triggers.Add(entry);
                }
            }
            else if (step.trigger == TutorialTrigger.AnyClick)
            {
                // AnyClick模式：遮罩仅视觉装饰，全屏任意点击即继续
                _tutorialPanel?.SetInteractionMode(false);
                if (_tutorialPanel?.ClickCatcherButton != null)
                {
                    _targetClickHook = () =>
                    {
                        Log($"[Tutorial] 确认点击: {step.stepId}");
                        AdvanceToNextStep();
                    };
                    _tutorialPanel.ClickCatcherButton.onClick.AddListener(_targetClickHook);
                }
            }
            // DragToTarget 留待后续实现
        }

        void RemoveTargetHook()
        {
            if (_currentTarget != null)
            {
                // 清理Button监听
                if (_targetClickHook != null)
                {
                    var btn = _currentTarget.GetComponent<Button>();
                    if (btn != null)
                        btn.onClick.RemoveListener(_targetClickHook);
                }
                // 清理EventTrigger条目
                var et = _currentTarget.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (et != null)
                    et.triggers.RemoveAll(e => e.callback != null);
            }

            // 清理click catcher
            if (_tutorialPanel?.ClickCatcherButton != null && _targetClickHook != null)
                _tutorialPanel.ClickCatcherButton.onClick.RemoveListener(_targetClickHook);

            _targetClickHook = null;
        }

        // ==================== 公开API ====================

        /// <summary>强制终止当前引导（调试用）</summary>
        public void ForceStopTutorial()
        {
            if (_state == TutorialState.Idle) return;
            Log("[Tutorial] 强制终止引导");
            CompleteTutorial();
        }

        // ==================== 调试 ====================

        void Log(string msg)
        {
            if (logOperations)
                Debug.Log(msg);
        }

#if UNITY_EDITOR
        void Update()
        {
            // 调试：按F12跳过引导
            if (_state != TutorialState.Idle && Input.GetKeyDown(KeyCode.F12))
            {
                Log("[Tutorial] 调试：F12跳过引导");
                ForceStopTutorial();
            }
        }
#endif
    }
}
