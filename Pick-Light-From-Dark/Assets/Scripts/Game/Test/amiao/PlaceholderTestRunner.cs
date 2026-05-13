using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 占位文字功能测试脚本
    /// 挂载到场景任意 GameObject，运行时按按键测试占位文字显示
    /// </summary>
    public class PlaceholderTestRunner : MonoBehaviour
    {
        [Header("目标占位显示器")]
        public PlaceholderDisplay placeholderDisplay;

        [Header("测试按键")]
        public KeyCode testImageKey = KeyCode.I;
        public KeyCode testSfxKey = KeyCode.S;
        public KeyCode testBgmKey = KeyCode.B;
        public KeyCode clearKey = KeyCode.C;

        void Start()
        {
            if (placeholderDisplay == null)
            {
                placeholderDisplay = FindObjectOfType<PlaceholderDisplay>();
                if (placeholderDisplay == null)
                {
                    var go = new GameObject("PlaceholderDisplay");
                    placeholderDisplay = go.AddComponent<PlaceholderDisplay>();
                    Debug.Log("[PlaceholderTestRunner] 自动创建 PlaceholderDisplay");
                }
            }

            Debug.Log($"[PlaceholderTestRunner] 测试按键: I=图片缺失, S=音效缺失, B=BGM缺失, C=清除");
        }

        void Update()
        {
            if (Input.GetKeyDown(testImageKey))
            {
                placeholderDisplay.Show("Image", "test_sprite_001");
                Debug.Log("[PlaceholderTestRunner] 测试: Image missing: test_sprite_001");
            }
            if (Input.GetKeyDown(testSfxKey))
            {
                placeholderDisplay.Show("SFX", "test_sound_002");
                Debug.Log("[PlaceholderTestRunner] 测试: SFX missing: test_sound_002");
            }
            if (Input.GetKeyDown(testBgmKey))
            {
                placeholderDisplay.Show("BGM", "test_music_003");
                Debug.Log("[PlaceholderTestRunner] 测试: BGM missing: test_music_003");
            }
            if (Input.GetKeyDown(clearKey))
            {
                placeholderDisplay.Clear();
                Debug.Log("[PlaceholderTestRunner] 已清除所有占位文字");
            }
        }

        [ContextMenu("测试图片缺失")]
        void TestImageMissing() { placeholderDisplay?.Show("Image", "test_sprite_001"); }

        [ContextMenu("测试音效缺失")]
        void TestSfxMissing() { placeholderDisplay?.Show("SFX", "test_sound_002"); }

        [ContextMenu("测试BGM缺失")]
        void TestBgmMissing() { placeholderDisplay?.Show("BGM", "test_music_003"); }

        [ContextMenu("清除占位文字")]
        void ClearPlaceholder() { placeholderDisplay?.Clear(); }
    }
}
