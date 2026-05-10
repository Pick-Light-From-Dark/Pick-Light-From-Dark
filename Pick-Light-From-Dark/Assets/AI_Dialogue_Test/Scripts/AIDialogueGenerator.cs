using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Game.Test.AI
{
    /// <summary>
    /// AI 对话生成器。
    /// 启动 llama-server 子进程，通过 HTTP API 调用本地 Qwen 模型生成对话。
    /// </summary>
    public class AIDialogueGenerator : MonoBehaviour
    {
        [Header("LLM 服务器配置")]
        public string serverHost = "127.0.0.1";
        public int serverPort = 8080;
        public int maxTokens = 100;
        public float temperature = 0.7f;

        [Header("模型路径（相对项目根目录）")]
        public string modelPath = "Assets/AI_Dialogue_Test/models/qwen2.5-0.5b-instruct-q4_k_m.gguf";
        public string llamaServerPath = "Assets/AI_Dialogue_Test/llama/llama-server.exe";

        [Header("启动等待")]
        public float serverStartupTimeout = 30f;

        private Process serverProcess;
        private bool isServerReady;
        private float startupTimer;

        void Start()
        {
            StartServer();
        }

        void Update()
        {
            if (!isServerReady && serverProcess != null && !serverProcess.HasExited)
            {
                startupTimer += Time.unscaledDeltaTime;
                if (startupTimer > serverStartupTimeout)
                {
                    UnityEngine.Debug.LogError("[AI] llama-server 启动超时");
                    StopServer();
                }
            }
        }

        void OnDestroy()
        {
            StopServer();
        }

        /// <summary>启动 llama-server 子进程</summary>
        public void StartServer()
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                UnityEngine.Debug.Log("[AI] llama-server 已在运行");
                isServerReady = true;
                return;
            }

            // 检查可执行文件是否存在
            if (!System.IO.File.Exists(llamaServerPath))
            {
                UnityEngine.Debug.LogError($"[AI] 未找到 llama-server: {llamaServerPath}");
                return;
            }
            if (!System.IO.File.Exists(modelPath))
            {
                UnityEngine.Debug.LogError($"[AI] 未找到模型: {modelPath}");
                return;
            }

            var psi = new ProcessStartInfo();
            psi.FileName = System.IO.Path.GetFullPath(llamaServerPath);
            psi.Arguments = $"-m \"{System.IO.Path.GetFullPath(modelPath)}\" --host {serverHost} --port {serverPort} -c 2048 --no-webui";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WorkingDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(llamaServerPath));

            serverProcess = new Process();
            serverProcess.StartInfo = psi;
            serverProcess.OutputDataReceived += OnServerOutput;
            serverProcess.ErrorDataReceived += OnServerError;

            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();

            startupTimer = 0f;
            UnityEngine.Debug.Log("[AI] 正在启动 llama-server...");
        }

        public bool IsServerReady()
        {
            return isServerReady;
        }

        void OnServerOutput(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;

            // 检测服务器就绪
            if (e.Data.Contains("HTTP server listening") || e.Data.Contains("server is listening"))
            {
                isServerReady = true;
                UnityEngine.Debug.Log("[AI] llama-server 已就绪");
            }
        }

        void OnServerError(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.LogWarning($"[llama-server] {e.Data}");
        }

        /// <summary>关闭 llama-server</summary>
        public void StopServer()
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                try
                {
                    serverProcess.Kill();
                    serverProcess.WaitForExit(2000);
                }
                catch { }
                serverProcess.Dispose();
                serverProcess = null;
            }
            isServerReady = false;
        }

        /// <summary>生成对话文本</summary>
        /// <param name="systemPrompt">System prompt（风格定义）</param>
        /// <param name="userPrompt">User prompt（当前情境）</param>
        /// <param name="onResult">回调，参数为生成结果（null 表示失败）</param>
        public void Generate(string systemPrompt, string userPrompt, Action<string> onResult)
        {
            if (!isServerReady)
            {
                UnityEngine.Debug.LogError("[AI] 服务器未就绪，请等待 llama-server 启动");
                onResult?.Invoke(null);
                return;
            }

            StartCoroutine(GenerateCoroutine(systemPrompt, userPrompt, onResult));
        }

        IEnumerator GenerateCoroutine(string systemPrompt, string userPrompt, Action<string> onResult)
        {
            var url = $"http://{serverHost}:{serverPort}/v1/chat/completions";

            var json = $"{{\"messages\":[" +
                $"{{\"role\":\"system\",\"content\":\"{EscapeJson(systemPrompt)}\"}}," +
                $"{{\"role\":\"user\",\"content\":\"{EscapeJson(userPrompt)}\"}}" +
                $"],\"max_tokens\":{maxTokens},\"temperature\":{temperature}}}";

            var request = new UnityWebRequest(url, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = request.downloadHandler.text;
                var result = ParseChatResponse(response);
                onResult?.Invoke(result);
            }
            else
            {
                UnityEngine.Debug.LogError($"[AI] 请求失败: {request.error}\n{request.downloadHandler.text}");
                onResult?.Invoke(null);
            }

            request.Dispose();
        }

        /// <summary>解析 OpenAI 兼容格式的响应</summary>
        string ParseChatResponse(string json)
        {
            try
            {
                // 简单字符串解析，避免依赖 JsonUtility（它不支持嵌套对象反序列化）
                var choicesIdx = json.IndexOf("\"choices\"");
                if (choicesIdx < 0) return null;

                var messageIdx = json.IndexOf("\"message\"", choicesIdx);
                if (messageIdx < 0) return null;

                var contentIdx = json.IndexOf("\"content\"", messageIdx);
                if (contentIdx < 0) return null;

                var start = json.IndexOf('"', contentIdx + 10);
                if (start < 0) return null;
                start++; // 跳过开头引号

                // 找到结尾引号（需要处理转义）
                var end = start;
                while (end < json.Length)
                {
                    if (json[end] == '\\' && end + 1 < json.Length)
                    {
                        end += 2; // 跳过转义序列
                    }
                    else if (json[end] == '"')
                    {
                        break;
                    }
                    else
                    {
                        end++;
                    }
                }

                if (end <= start) return null;

                var raw = json.Substring(start, end - start);
                return UnescapeJson(raw);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AI] 解析响应失败: {e.Message}");
                return null;
            }
        }

        string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        string UnescapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }
    }
}
