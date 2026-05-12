using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Game.Config;
using Game.Data;

namespace Game.Editor
{
    /// <summary>
    /// 关卡配置资产生成器
    /// 在 Unity 菜单栏 Game > Generate Level Configs 中运行
    /// 生成的资产文件位于 Assets/Resources/Config/
    /// </summary>
    public static class LevelConfigGenerator
    {
        private const string ConfigDir = "Assets/Resources/Config";

        [MenuItem("Game/Generate Level Configs/Generate All")]
        public static void GenerateAll()
        {
            EnsureDirectory();
            GenerateLevel1Config();
            GenerateLevel2Config();
            GenerateLevel3Config();
            GenerateLevel5Config();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LevelConfigGenerator] 全部关卡配置资产已生成");
        }

        [MenuItem("Game/Generate Level Configs/Generate Level 1 Config")]
        public static void GenerateLevel1Config()
        {
            EnsureDirectory();
            var config = CreateOrGetAsset("LevelConfig_1.asset");

            config.levelId = 1001;
            config.levelName = "第一夜";
            config.timeLimit = 600;
            config.maxLives = 2;
            config.initialInBed = true;
            config.initialPanic = 15;
            config.initialExcite = 15;
            config.criticalValue = 80;
            config.eyeClosePanicDecreasePerSec = 1f;
            config.eyeCloseAccelerationThreshold = 20f;
            config.eyeCloseAccelerationMultiplier = 1.5f;
            config.patrolIntervals = new Vector2(15f, 25f);
            config.patrolTime = new Vector2(8f, 10f);
            config.eyeCheckDuration = new Vector2(3f, 3f);
            config.flashCheckDuration = new Vector2(3f, 3f);
            config.flashPanicPerSec = 2;
            config.cardDataPath = "Card";
            config.initialCards = new List<int> { 2001, 2010 };
            config.taskGoals = new List<TaskGoal>
            {
                new TaskGoal(2008, 1) { state = TaskState.InProgress },
            };

            config.preDialogueFile = "Dialogue1-1_pre";
            config.postDialogueFile = "Dialogue1-1_post_eat";
            config.isChoiceLevel = true;
            config.choice2EndingDialogueFile = "Dialogue1-1_post_noeat";

            EditorUtility.SetDirty(config);
            Debug.Log("[LevelConfigGenerator] LevelConfig_1.asset 已更新");
        }

        [MenuItem("Game/Generate Level Configs/Generate Level 2 Config")]
        public static void GenerateLevel2Config()
        {
            EnsureDirectory();
            var config = CreateOrGetAsset("LevelConfig_2.asset");

            config.levelId = 1002;
            config.levelName = "第二夜";
            config.timeLimit = 600;
            config.maxLives = 2;
            config.initialInBed = true;
            config.initialPanic = 15;
            config.initialExcite = 15;
            config.criticalValue = 80;
            config.eyeClosePanicDecreasePerSec = 1f;
            config.eyeCloseAccelerationThreshold = 20f;
            config.eyeCloseAccelerationMultiplier = 1.5f;
            config.patrolIntervals = new Vector2(15f, 25f);
            config.patrolTime = new Vector2(8f, 10f);
            config.eyeCheckDuration = new Vector2(3f, 3f);
            config.flashCheckDuration = new Vector2(3f, 3f);
            config.flashPanicPerSec = 2;
            config.cardDataPath = "Card_Level2";
            config.initialCards = new List<int> { 2001, 2002, 2003, 2010 };
            config.taskGoals = new List<TaskGoal>
            {
                new TaskGoal(2016, 1) { state = TaskState.InProgress },
                new TaskGoal(2017, 1) { state = TaskState.InProgress },
            };

            config.preDialogueFile = "Dialogue2_pre";
            config.postDialogueFile = "Dialogue2_post";

            EditorUtility.SetDirty(config);
            Debug.Log("[LevelConfigGenerator] LevelConfig_2.asset 已更新");
        }

        [MenuItem("Game/Generate Level Configs/Generate Level 3 Config")]
        public static void GenerateLevel3Config()
        {
            EnsureDirectory();
            var config = CreateOrGetAsset("LevelConfig_3.asset");

            config.levelId = 1003;
            config.levelName = "第三夜";
            config.timeLimit = 600;
            config.maxLives = 2;
            config.initialInBed = true;
            config.initialPanic = 15;
            config.initialExcite = 15;
            config.criticalValue = 80;
            config.eyeClosePanicDecreasePerSec = 1f;
            config.eyeCloseAccelerationThreshold = 20f;
            config.eyeCloseAccelerationMultiplier = 1.5f;
            config.patrolIntervals = new Vector2(15f, 25f);
            config.patrolTime = new Vector2(8f, 10f);
            config.eyeCheckDuration = new Vector2(3f, 3f);
            config.flashCheckDuration = new Vector2(3f, 3f);
            config.flashPanicPerSec = 2;
            config.cardDataPath = "Card_Level3";
            config.initialCards = new List<int> { 2001, 2004, 2005, 2006, 2010 };
            config.taskGoals = new List<TaskGoal>
            {
                new TaskGoal(2020, 1) { state = TaskState.InProgress },
                new TaskGoal(2021, 1) { state = TaskState.InProgress },
                new TaskGoal(2025, 1) { state = TaskState.InProgress },
            };

            EditorUtility.SetDirty(config);
            Debug.Log("[LevelConfigGenerator] LevelConfig_3.asset 已更新");
        }

        [MenuItem("Game/Generate Level Configs/Generate Level 5 Config")]
        public static void GenerateLevel5Config()
        {
            EnsureDirectory();
            var config = CreateOrGetAsset("LevelConfig_5.asset");

            config.levelId = 1005;
            config.levelName = "第五夜";
            config.timeLimit = 600;
            config.maxLives = 2;
            config.initialInBed = true;
            config.initialPanic = 20;
            config.initialExcite = 15;
            config.criticalValue = 80;
            config.eyeClosePanicDecreasePerSec = 1f;
            config.eyeCloseAccelerationThreshold = 20f;
            config.eyeCloseAccelerationMultiplier = 1.5f;
            config.patrolIntervals = new Vector2(15f, 25f);
            config.patrolTime = new Vector2(8f, 10f);
            config.eyeCheckDuration = new Vector2(3f, 3f);
            config.flashCheckDuration = new Vector2(3f, 3f);
            config.flashPanicPerSec = 2;
            config.cardDataPath = "Card_Level5";
            config.initialCards = new List<int> { 2001, 2003, 2005, 2010 };
            config.taskGoals = new List<TaskGoal>
            {
                new TaskGoal(2038, 1) { state = TaskState.InProgress },
            };

            EditorUtility.SetDirty(config);
            Debug.Log("[LevelConfigGenerator] LevelConfig_5.asset 已更新");
        }

        private static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder(ConfigDir))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Config");
            }
        }

        private static LevelConfigSO CreateOrGetAsset(string fileName)
        {
            string path = $"{ConfigDir}/{fileName}";
            var existing = AssetDatabase.LoadAssetAtPath<LevelConfigSO>(path);
            if (existing != null)
            {
                Debug.Log($"[LevelConfigGenerator] 更新已有资产: {path}");
                return existing;
            }

            var config = ScriptableObject.CreateInstance<LevelConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[LevelConfigGenerator] 创建新资产: {path}");
            return config;
        }
    }
}
