using UnityEngine;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 音效加载测试脚本
    /// 挂载到场景中任意 GameObject 即可运行测试
    /// </summary>
    public class SoundTestRunner : MonoBehaviour
    {
        [Header("按键说明（运行时）")]
        [Tooltip("1 = 播放按钮点击音效")]
        public KeyCode testSound1 = KeyCode.Alpha1;

        [Tooltip("2 = 播放撕开薯片袋")]
        public KeyCode testSound2 = KeyCode.Alpha2;

        [Tooltip("3 = 通过 MusicMgr 播放")]
        public KeyCode testMusicMgr = KeyCode.Alpha3;

        [Tooltip("4 = 测试 FungusVNController 音效加载")]
        public KeyCode testVN = KeyCode.Alpha4;

        [Header("测试配置")]
        public bool autoTestOnStart = true;

        [Header("VN 控制器（可选）")]
        public FungusVNController vnController;

        void Start()
        {
            if (autoTestOnStart)
            {
                Invoke(nameof(RunAutoTests), 1f);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(testSound1))
                TestDirectPlay("按钮点击音效");

            if (Input.GetKeyDown(testSound2))
                TestDirectPlay("02.撕开薯片袋");

            if (Input.GetKeyDown(testMusicMgr))
                TestMusicMgr();

            if (Input.GetKeyDown(testVN))
                TestVNLoad();
        }

        void RunAutoTests()
        {
            Debug.Log("========== 音效自动测试开始 ==========");
            TestDirectPlay("按钮点击音效");
            TestDirectPlay("02.撕开薯片袋");
            TestMusicMgr();
            TestVNLoad();
            Debug.Log("========== 音效自动测试结束 ==========");
        }

        [ContextMenu("测试直接播放")]
        void TestDirectPlay(string soundName)
        {
            var clip = Resources.Load<AudioClip>("Sound/sound/" + soundName);
            if (clip != null)
            {
                Debug.Log($"[SoundTestRunner] Resources 加载成功: {soundName}");
                var go = new GameObject("TempAudio_" + soundName);
                var source = go.AddComponent<AudioSource>();
                source.clip = clip;
                source.Play();
                Destroy(go, clip.length + 0.5f);
            }
            else
            {
                // 尝试子目录
                clip = Resources.Load<AudioClip>("Sound/sound/DXH_SOUND/" + soundName);
                if (clip != null)
                {
                    Debug.Log($"[SoundTestRunner] Resources 子目录加载成功: {soundName}");
                    var go = new GameObject("TempAudio_" + soundName);
                    var source = go.AddComponent<AudioSource>();
                    source.clip = clip;
                    source.Play();
                    Destroy(go, clip.length + 0.5f);
                }
                else
                {
                    Debug.LogError($"[SoundTestRunner] Resources 加载失败: {soundName}");
                }
            }
        }

        [ContextMenu("测试 MusicMgr")]
        void TestMusicMgr()
        {
            if (MusicMgr.Instance == null)
            {
                Debug.LogError("[SoundTestRunner] MusicMgr 未初始化");
                return;
            }

            Debug.Log("[SoundTestRunner] 通过 MusicMgr 播放音效...");
            MusicMgr.Instance.PlaySound("按钮点击音效", false, source =>
            {
                if (source != null)
                    Debug.Log("[SoundTestRunner] MusicMgr 播放回调成功");
                else
                    Debug.LogError("[SoundTestRunner] MusicMgr 播放回调返回 null");
            });
        }

        [ContextMenu("测试 VN 音效加载")]
        void TestVNLoad()
        {
            if (vnController == null)
            {
                vnController = FindObjectOfType<FungusVNController>();
                if (vnController == null)
                {
                    Debug.LogWarning("[SoundTestRunner] 未找到 FungusVNController，跳过 VN 测试");
                    return;
                }
            }

            // 通过反射调用私有方法 LoadAudioClip 验证
            var method = typeof(FungusVNController).GetMethod("LoadAudioClip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                var clip1 = method.Invoke(vnController, new object[] { "按钮点击音效", false }) as AudioClip;
                var clip2 = method.Invoke(vnController, new object[] { "02.撕开薯片袋", false }) as AudioClip;

                Debug.Log($"[SoundTestRunner] VN LoadAudioClip 按钮点击音效: {(clip1 != null ? "成功" : "失败")}");
                Debug.Log($"[SoundTestRunner] VN LoadAudioClip 02.撕开薯片袋: {(clip2 != null ? "成功" : "失败")}");
            }
            else
            {
                Debug.LogWarning("[SoundTestRunner] 未找到 LoadAudioClip 方法");
            }
        }
    }
}
