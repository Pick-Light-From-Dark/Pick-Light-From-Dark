using UnityEngine;
using System;
using System.IO;

namespace Game.Test
{
    /// <summary>
    /// 测试用玩家数据 — Phase 3
    /// 简单的 JSON 序列化数据结构，用于验证存档读档时的数据一致性。
    /// </summary>
    [Serializable]
    public class TestPlayerData
    {
        public int playerId;
        public string playerName;
        public int lives;
        public int emotion;
        public int currentLevel;
        public string currentSegment;
        public long lastSaveTime;
        public bool hasSeenOpening;

        public TestPlayerData()
        {
            playerId = 1;
            playerName = "TestPlayer";
            lives = 3;
            emotion = 50;
            currentLevel = 1;
            currentSegment = "day1-1";
            hasSeenOpening = false;
        }
    }

    /// <summary>
    /// 测试玩家数据管理器 — 负责 JSON 的读写和变化追踪
    /// </summary>
    public class TestPlayerDataManager : MonoBehaviour
    {
        public static TestPlayerDataManager Instance { get; private set; }

        const string DATA_FILE = "test_player_data.json";

        [Header("当前数据")]
        public TestPlayerData currentData = new TestPlayerData();

        [Header("调试")]
        public bool logChanges = true;

        string FilePath => Path.Combine(Application.persistentDataPath, DATA_FILE);

        void Awake()
        {
            Instance = this;
            LoadData();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>保存当前数据到 JSON</summary>
        public void SaveData()
        {
            currentData.lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string json = JsonUtility.ToJson(currentData, true);
            File.WriteAllText(FilePath, json);
            if (logChanges)
                Debug.Log($"[TestPlayerData] 数据已保存: {FilePath}\n{json}");
        }

        /// <summary>从 JSON 加载数据</summary>
        public void LoadData()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                var loaded = JsonUtility.FromJson<TestPlayerData>(json);
                if (loaded != null)
                {
                    currentData = loaded;
                    if (logChanges)
                        Debug.Log($"[TestPlayerData] 数据已加载:\n{json}");
                }
            }
            else
            {
                // 首次创建默认数据
                currentData = new TestPlayerData();
                SaveData();
            }
        }

        /// <summary>更新数据并自动保存</summary>
        public void UpdateData(Action<TestPlayerData> modifier)
        {
            var before = JsonUtility.ToJson(currentData);
            modifier(currentData);
            var after = JsonUtility.ToJson(currentData);

            if (before != after)
            {
                SaveData();
                Debug.Log("[TestPlayerData] 数据已变更并保存");
            }
        }

        /// <summary>清除数据</summary>
        public void ClearData()
        {
            currentData = new TestPlayerData();
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            Debug.Log("[TestPlayerData] 数据已清除");
        }

        /// <summary>在控制台显示当前数据</summary>
        public void DisplayData()
        {
            string json = JsonUtility.ToJson(currentData, true);
            Debug.Log($"[TestPlayerData] 当前数据:\n{json}");
        }
    }
}
