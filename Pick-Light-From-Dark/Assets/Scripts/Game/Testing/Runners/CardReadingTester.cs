using System.Collections.Generic;
using UnityEngine;
using Game.Card;
using Game.Data;
using Game.Config;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 卡牌读条系统手动测试器
    /// 测试 StartReading / InterruptReading / 进度保存（吃薯片卡）
    /// </summary>
    public class CardReadingTester : MonoBehaviour
    {
        [Header("测试关卡配置")]
        public LevelConfigSO testConfig;

        [Header("自定义测试卡牌")]
        [Tooltip("卡牌名（仅用于日志）")]
        public string cardName = "TestCard";
        [Tooltip("打断时增加的慌乱值")]
        public int interruptPanicAdd = 5;
        [Tooltip("打断时是否保存进度（吃薯片卡=true）")]
        public bool saveProgressOnInterrupt = false;
        [Tooltip("片段时长列表（秒）")]
        public List<float> segmentDurations = new List<float> { 2f, 1f, 2f };
        [Tooltip("片段是否可打断（与 segmentDurations 等长）")]
        public List<bool> segmentInterruptible = new List<bool> { true, false, true };
        [Tooltip("完成后情绪变化")]
        public int panicDelta = 0;
        public int exciteDelta = 0;
        public BedStateChange bedStateChange = BedStateChange.None;

        [Header("UI 显示")]
        public bool showOnGUI = true;

        private CardReadingSystem cardReading;
        private CardInstance testInstance;
        private int instanceCounter = 1;

        void Awake()
        {
            cardReading = GetComponent<CardReadingSystem>();
            if (cardReading == null)
                cardReading = gameObject.AddComponent<CardReadingSystem>();
        }

        void Start()
        {
            if (testConfig != null) cardReading.Initialize(testConfig);
            Debug.Log("[CardReadingTester] 已就绪");
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            int x = 10, y = 10, w = 360, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 280), "CardReading 测试器");
            y += 25;

            GUI.Label(new Rect(x, y, w, h), $"读条中: {cardReading.IsReading()}  进度: {cardReading.GetProgress():P1}"); y += h;
            var seg = cardReading.GetCurrentSegment();
            GUI.Label(new Rect(x, y, w, h), $"当前片段: {(seg == null ? "无" : $"{seg.duration}s 可打断={seg.isInterruptible}")}"); y += h;
            GUI.Label(new Rect(x, y, w, h), $"可打断: {cardReading.CanInterrupt()}"); y += h;
            if (testInstance != null)
            {
                GUI.Label(new Rect(x, y, w, h), $"卡牌进度保存: time={testInstance.currentReadTime:F2}s seg={testInstance.currentSegmentIndex}"); y += h;
            }
            y += 4;

            if (GUI.Button(new Rect(x, y, w, h), "构造测试卡并 StartReading")) DoStartReading(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "InterruptReading 打断")) DoInterruptReading(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "再次 StartReading（测试进度恢复）")) DoStartReadingResume(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "重置测试卡")) ResetInstance(); y += h;
        }

        [ContextMenu("StartReading")]
        public void DoStartReading()
        {
            if (testInstance == null || testInstance.isUsed)
                BuildInstance();
            cardReading.StartReading(testInstance);
        }

        [ContextMenu("InterruptReading")]
        public void DoInterruptReading()
        {
            bool ok = cardReading.InterruptReading();
            Debug.Log($"[CardReadingTester] InterruptReading 返回 {ok}");
        }

        [ContextMenu("StartReadingResume")]
        public void DoStartReadingResume()
        {
            if (testInstance == null)
            {
                Debug.LogWarning("[CardReadingTester] 还没有测试卡");
                return;
            }
            cardReading.StartReading(testInstance);
        }

        [ContextMenu("ResetInstance")]
        public void ResetInstance()
        {
            testInstance = null;
            Debug.Log("[CardReadingTester] 测试卡已清除");
        }

        private void BuildInstance()
        {
            var data = new CardData
            {
                id = 9000 + instanceCounter,
                cardName = cardName,
                interruptPanicAdd = interruptPanicAdd,
                saveProgressOnInterrupt = saveProgressOnInterrupt,
                panicDelta = panicDelta,
                exciteDelta = exciteDelta,
                bedStateChange = bedStateChange,
                segments = new List<Segment>()
            };
            int cnt = Mathf.Min(segmentDurations.Count, segmentInterruptible.Count);
            for (int i = 0; i < cnt; i++)
            {
                data.segments.Add(new Segment(segmentDurations[i], segmentInterruptible[i]));
            }
            testInstance = new CardInstance(data, instanceCounter++);
            Debug.Log($"[CardReadingTester] 构造测试卡 {data.cardName} 段数={data.segments.Count} 可保存进度={data.saveProgressOnInterrupt}");
        }
    }
}
