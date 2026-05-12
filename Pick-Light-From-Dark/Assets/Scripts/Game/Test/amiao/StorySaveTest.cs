using UnityEngine;
using Fungus;

namespace Game.Test
{
    /// <summary>
    /// Fungus 存档系统测试工具 — 挂载到任意 FungusVN 实例上即可测试存档/读档
    /// 快捷键：
    ///   F5 = 手动存档当前剧情进度
    ///   F8 = 模拟"开场剧情结束自动存档"
    ///   F9 = 读档验证（从 Fungus 存档恢复）
    ///   F12 = 清空 Fungus 存档数据
    /// </summary>
    public class StorySaveTest : MonoBehaviour
    {
        [Header("测试配置")]
        public FungusVNController vnController;

        void Start()
        {
            if (vnController == null)
                vnController = GetComponent<FungusVNController>();

            Debug.Log("[StorySaveTest] Fungus 存档测试工具已启动");
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
                if (vnController != null && vnController.saveFlowchart != null)
                {
                    vnController.saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", true);
                }

                var saveManager = FungusManager.Instance.SaveManager;
                saveManager.AddSavePoint("OpeningComplete", "开场剧情结束自动存档");
                saveManager.Save("vn_save");
                Debug.Log("[StorySaveTest] 模拟自动存档完成: isOpeningDone=true");
            }

            // F9 — 读档验证（从 Fungus 存档恢复）
            if (Input.GetKeyDown(KeyCode.F9))
            {
                var saveManager = FungusManager.Instance.SaveManager;
                if (saveManager.SaveDataExists("vn_save"))
                {
                    Debug.Log("[StorySaveTest] 读档中... Fungus 将重新加载场景并恢复 Flowchart 变量");
                    saveManager.Load("vn_save");
                }
                else
                {
                    Debug.Log("[StorySaveTest] 无存档数据");
                }
            }

            // F12 — 清空 Fungus 存档
            if (Input.GetKeyDown(KeyCode.F12))
            {
                string savePath = System.IO.Path.Combine(Application.persistentDataPath, "FungusSaves", "vn_save.json");
                try
                {
                    if (System.IO.File.Exists(savePath))
                    {
                        System.IO.File.Delete(savePath);
                        Debug.Log($"[StorySaveTest] 已删除 Fungus 存档: {savePath}");
                    }
                    else
                    {
                        Debug.Log("[StorySaveTest] 存档文件不存在，无需删除");
                    }

                    // 同时清空 Flowchart 变量
                    if (vnController != null && vnController.saveFlowchart != null)
                    {
                        vnController.saveFlowchart.SetIntegerVariable("VN_LineIndex", 0);
                        vnController.saveFlowchart.SetStringVariable("VN_StoryFile", "");
                        vnController.saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", false);
                        Debug.Log("[StorySaveTest] 已重置 Flowchart 存档变量");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[StorySaveTest] 清空存档失败: {ex.Message}");
                }
            }
        }
    }
}
