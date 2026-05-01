using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Backend
{
    /// <summary>
    /// JSON 数据访问层 — 负责读写本地 JSON 文件
    /// 未来可无缝替换为 Unity Cloud Save 的后端实现
    /// </summary>
    public class PlayerDataStore : SingletonAutoMono<PlayerDataStore>
    {
        private string _filePath;

        private void Awake()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "player_data.json");
            Debug.Log($"[PlayerDataStore] 数据文件路径: {_filePath}");
        }

        /// <summary>
        /// 保存本关记录（追加到已有记录列表）
        /// </summary>
        public void SaveLevelRecord(JsonLevelRecord record)
        {
            var data = LoadOrCreate();
            data.records.Add(record);
            WriteFile(data);
            Debug.Log($"[PlayerDataStore] 已保存关卡 {record.levelId} 记录（总计 {data.records.Count} 条）");
        }

        /// <summary>
        /// 获取某关卡的所有记录（可能多次游玩）
        /// </summary>
        public List<JsonLevelRecord> GetRecordsByLevel(int levelId)
        {
            var data = LoadOrCreate();
            var result = new List<JsonLevelRecord>();
            foreach (var r in data.records)
            {
                if (r.levelId == levelId) result.Add(r);
            }
            return result;
        }

        /// <summary>
        /// 获取最近一次通关记录（用于读取历史状态）
        /// </summary>
        public JsonLevelRecord GetLastRecord(int levelId)
        {
            var all = GetRecordsByLevel(levelId);
            if (all.Count == 0) return null;
            return all[all.Count - 1];
        }

        /// <summary>
        /// 获取所有记录
        /// </summary>
        public List<JsonLevelRecord> GetAllRecords()
        {
            return LoadOrCreate().records;
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void ClearAllRecords()
        {
            WriteFile(new PlayerDataFile());
            Debug.Log("[PlayerDataStore] 已清除所有玩家数据");
        }

        private PlayerDataFile LoadOrCreate()
        {
            if (!File.Exists(_filePath))
            {
                return new PlayerDataFile();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                var data = JsonUtility.FromJson<PlayerDataFile>(json);
                return data ?? new PlayerDataFile();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayerDataStore] 读取文件失败，使用空数据: {ex.Message}");
                return new PlayerDataFile();
            }
        }

        private void WriteFile(PlayerDataFile data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerDataStore] 写入文件失败: {ex.Message}");
            }
        }
    }
}