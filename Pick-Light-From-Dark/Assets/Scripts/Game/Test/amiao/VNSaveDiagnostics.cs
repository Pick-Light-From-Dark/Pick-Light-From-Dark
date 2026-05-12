using UnityEngine;
using Fungus;

namespace Game.Test
{
    /// <summary>
    /// VN 存档系统运行时诊断工具。
    /// 挂载到场景中即可在 Start 时自动检查存档系统配置完整性。
    /// </summary>
    public class VNSaveDiagnostics : MonoBehaviour
    {
        [Header("待检查的 VN 控制器（为空则自动查找）")]
        public FungusVNController vnController;

        void Start()
        {
            if (vnController == null)
                vnController = FindObjectOfType<FungusVNController>();

            RunDiagnostics();
        }

        public void RunDiagnostics()
        {
            bool allPassed = true;
            Debug.Log("========== VN 存档系统诊断开始 ==========");

            // 1. 检查 VN_SaveFlowchart
            var flowchartGo = GameObject.Find("VN_SaveFlowchart");
            if (flowchartGo == null)
            {
                Debug.LogError("[VNSaveDiagnostics] ❌ VN_SaveFlowchart 不存在");
                allPassed = false;
            }
            else
            {
                var flowchart = flowchartGo.GetComponent<Flowchart>();
                if (flowchart == null)
                {
                    Debug.LogError("[VNSaveDiagnostics] ❌ VN_SaveFlowchart 缺少 Flowchart 组件");
                    allPassed = false;
                }
                else
                {
                    Debug.Log("[VNSaveDiagnostics] ✅ VN_SaveFlowchart 存在");

                    // 检查变量
                    CheckVariable(flowchart, "VN_LineIndex", 0, ref allPassed);
                    CheckVariable(flowchart, "VN_StoryFile", "", ref allPassed);
                    CheckVariable(flowchart, "VN_IsOpeningDone", false, ref allPassed);
                }
            }

            // 2. 检查 SaveData
            var saveData = FindObjectOfType<SaveData>();
            if (saveData == null)
            {
                Debug.LogError("[VNSaveDiagnostics] ❌ SaveData 组件不存在");
                allPassed = false;
            }
            else
            {
                Debug.Log("[VNSaveDiagnostics] ✅ SaveData 组件存在");
            }

            // 3. 检查 FungusVNController.saveFlowchart 引用
            if (vnController == null)
            {
                Debug.LogWarning("[VNSaveDiagnostics] ⚠️ 未找到 FungusVNController");
            }
            else if (vnController.saveFlowchart == null)
            {
                Debug.LogWarning("[VNSaveDiagnostics] ⚠️ FungusVNController.saveFlowchart 未赋值（运行时将由 VN_SaveFlowchartSetup 自动配置）");
            }
            else
            {
                Debug.Log("[VNSaveDiagnostics] ✅ FungusVNController.saveFlowchart 已赋值");
            }

            // 4. 检查存档文件是否存在
            var saveManager = FungusManager.Instance?.SaveManager;
            if (saveManager != null && saveManager.SaveDataExists("vn_save"))
            {
                Debug.Log("[VNSaveDiagnostics] ✅ 存档文件 vn_save 存在");
            }
            else
            {
                Debug.Log("[VNSaveDiagnostics] ℹ️ 存档文件 vn_save 不存在（首次运行正常）");
            }

            if (allPassed)
                Debug.Log("========== VN 存档系统诊断通过 ✅ ==========");
            else
                Debug.Log("========== VN 存档系统诊断发现问题 ❌ ==========");
        }

        void CheckVariable<T>(Flowchart flowchart, string key, T expectedValue, ref bool allPassed)
        {
            var variable = flowchart.GetVariable(key);
            if (variable == null)
            {
                Debug.LogError($"[VNSaveDiagnostics] ❌ 变量 {key} 不存在");
                allPassed = false;
            }
            else
            {
                Debug.Log($"[VNSaveDiagnostics] ✅ 变量 {key} 存在 ({variable.GetType().Name})");
            }
        }
    }
}
