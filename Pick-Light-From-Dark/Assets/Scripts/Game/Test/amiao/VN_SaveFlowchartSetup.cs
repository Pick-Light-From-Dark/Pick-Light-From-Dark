using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Fungus;

namespace Game.Test
{
    /// <summary>
    /// VN 存档 Flowchart 自动配置工具。
    /// 运行时自动创建 VN_SaveFlowchart、配置存档变量、关联 SaveData。
    /// 挂载到场景中任意 GameObject（建议挂在 VN 预制体根节点）即可。
    /// </summary>
    public class VN_SaveFlowchartSetup : MonoBehaviour
    {
        [Header("自动创建配置")]
        public string flowchartName = "VN_SaveFlowchart";

        void Awake()
        {
            EnsureSaveFlowchart();
            EnsureSaveData();
        }

        /// <summary>查找或创建存档专用 Flowchart，并配置三个变量</summary>
        void EnsureSaveFlowchart()
        {
            var flowchartGo = GameObject.Find(flowchartName);
            Flowchart flowchart;

            if (flowchartGo == null)
            {
                flowchartGo = new GameObject(flowchartName);
                flowchart = flowchartGo.AddComponent<Flowchart>();
                Debug.Log($"[VN_SaveFlowchartSetup] 自动创建 {flowchartName}");
            }
            else
            {
                flowchart = flowchartGo.GetComponent<Flowchart>();
                if (flowchart == null)
                {
                    flowchart = flowchartGo.AddComponent<Flowchart>();
                }
            }

            // 获取或创建变量列表（反射访问 protected variables 字段）
            var variablesField = typeof(Flowchart).GetField("variables",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (variablesField == null)
            {
                Debug.LogError("[VN_SaveFlowchartSetup] 无法访问 Flowchart.variables 字段");
                return;
            }

            var variables = variablesField.GetValue(flowchart) as List<Variable>;
            if (variables == null)
            {
                variables = new List<Variable>();
                variablesField.SetValue(flowchart, variables);
            }

            // 确保三个存档变量存在
            EnsureIntegerVariable(flowchart, variables, "VN_LineIndex", 0);
            EnsureStringVariable(flowchart, variables, "VN_StoryFile", "");
            EnsureBooleanVariable(flowchart, variables, "VN_IsOpeningDone", false);

            Debug.Log("[VN_SaveFlowchartSetup] Flowchart 变量配置完成");
        }

        void EnsureIntegerVariable(Flowchart flowchart, List<Variable> variables, string key, int defaultValue)
        {
            foreach (var v in variables)
                if (v != null && v.Key == key) return;

            var varGo = new GameObject(key);
            varGo.transform.SetParent(flowchart.transform, false);
            var variable = varGo.AddComponent<IntegerVariable>();
            variable.Key = key;
            variable.Value = defaultValue;
            variables.Add(variable);
            Debug.Log($"[VN_SaveFlowchartSetup] 创建变量: {key} = {defaultValue}");
        }

        void EnsureStringVariable(Flowchart flowchart, List<Variable> variables, string key, string defaultValue)
        {
            foreach (var v in variables)
                if (v != null && v.Key == key) return;

            var varGo = new GameObject(key);
            varGo.transform.SetParent(flowchart.transform, false);
            var variable = varGo.AddComponent<StringVariable>();
            variable.Key = key;
            variable.Value = defaultValue;
            variables.Add(variable);
            Debug.Log($"[VN_SaveFlowchartSetup] 创建变量: {key} = {defaultValue}");
        }

        void EnsureBooleanVariable(Flowchart flowchart, List<Variable> variables, string key, bool defaultValue)
        {
            foreach (var v in variables)
                if (v != null && v.Key == key) return;

            var varGo = new GameObject(key);
            varGo.transform.SetParent(flowchart.transform, false);
            var variable = varGo.AddComponent<BooleanVariable>();
            variable.Key = key;
            variable.Value = defaultValue;
            variables.Add(variable);
            Debug.Log($"[VN_SaveFlowchartSetup] 创建变量: {key} = {defaultValue}");
        }

        /// <summary>查找或创建 SaveData 组件，并将 VN_SaveFlowchart 加入其列表</summary>
        void EnsureSaveData()
        {
            var saveData = FindObjectOfType<SaveData>();
            if (saveData == null)
            {
                var sdGo = new GameObject("FungusSaveData");
                saveData = sdGo.AddComponent<SaveData>();
                Debug.Log("[VN_SaveFlowchartSetup] 自动创建 SaveData");
            }

            // 获取 flowcharts 列表（反射访问 protected flowcharts 字段）
            var flowchartsField = typeof(SaveData).GetField("flowcharts",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (flowchartsField == null)
            {
                Debug.LogError("[VN_SaveFlowchartSetup] 无法访问 SaveData.flowcharts 字段");
                return;
            }

            var flowcharts = flowchartsField.GetValue(saveData) as List<Flowchart>;
            if (flowcharts == null)
            {
                flowcharts = new List<Flowchart>();
                flowchartsField.SetValue(saveData, flowcharts);
            }

            var vnFlowchart = GameObject.Find(flowchartName)?.GetComponent<Flowchart>();
            if (vnFlowchart == null)
            {
                Debug.LogWarning("[VN_SaveFlowchartSetup] 未找到 VN_SaveFlowchart");
                return;
            }

            if (!flowcharts.Contains(vnFlowchart))
            {
                flowcharts.Add(vnFlowchart);
                Debug.Log("[VN_SaveFlowchartSetup] 已将 VN_SaveFlowchart 加入 SaveData");
            }
        }
    }
}
