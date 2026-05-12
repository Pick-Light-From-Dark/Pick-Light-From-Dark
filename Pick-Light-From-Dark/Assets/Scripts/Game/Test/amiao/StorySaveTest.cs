using UnityEngine;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// 存档系统测试工具 — 挂载到任意 FungusVN 实例上即可测试存档/读档
    /// 快捷键：
    ///   F5 = 手动存档当前剧情进度
    ///   F8 = 模拟"开场剧情结束自动存档"
    ///   F9 = 读档验证（打印存档内容到 Console）
    ///   F12 = 清空所有存档数据
    /// </summary>
    public class StorySaveTest : MonoBehaviour
    {
        [Header("测试配置")]
        public FungusVNController vnController;
        public int testLevelId = 1;

        void Start()
        {
            if (vnController == null)
                vnController = GetComponent<FungusVNController>();

            Debug.Log("[StorySaveTest] 存档测试工具已启动");
            Debug.Log("[StorySaveTest] F5=手动存档 | F8=模拟自动存档 | F9=读档验证 | F12=清空存档");
        }

        void Update()
        {
            // F5 — 手动存档当前 VN 进度
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (vnController != null)
                {
                    vnController.SaveProgress();
                }
                else
                {
                    Debug.LogWarning("[StorySaveTest] vnController 未赋值");
                }
            }

            // F8 — 模拟"开场剧情结束自动存档"
            if (Input.GetKeyDown(KeyCode.F8))
            {
                var record = new StoryProgressRecord
                {
                    levelId = testLevelId,
                    storyFileName = vnController != null && vnController.dialogueText != null
                        ? vnController.dialogueText.name
                        : "TestStory",
                    lineIndex = -1, // -1 表示开场剧情已全部看完
                    isOpeningDone = true
                };
                PlayerDataStore.Instance.SaveStoryProgress(record);
                Debug.Log($"[StorySaveTest] 模拟自动存档完成: level={testLevelId}, isOpeningDone=true");
            }

            // F9 — 读档验证
            if (Input.GetKeyDown(KeyCode.F9))
            {
                var progress = PlayerDataStore.Instance.LoadStoryProgress();
                if (progress != null)
                {
                    Debug.Log($"[StorySaveTest] 读档成功 ===");
                    Debug.Log($"  关卡ID: {progress.levelId}");
                    Debug.Log($"  剧情文件: {progress.storyFileName}");
                    Debug.Log($"  当前行号: {progress.lineIndex} (-1=已看完)");
                    Debug.Log($"  开场完成: {progress.isOpeningDone}");
                    Debug.Log($"  存档时间: {System.DateTimeOffset.FromUnixTimeMilliseconds(progress.timestamp).LocalDateTime}");
                }
                else
                {
                    Debug.Log("[StorySaveTest] 无存档数据");
                }
            }

            // F12 — 清空所有存档
            if (Input.GetKeyDown(KeyCode.F12))
            {
                PlayerDataStore.Instance.ClearAllRecords();
                Debug.Log("[StorySaveTest] 已清空所有存档数据");
            }
        }
    }
}
