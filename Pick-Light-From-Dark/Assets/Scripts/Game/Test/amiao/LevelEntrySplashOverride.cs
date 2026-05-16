using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 挂在 Level3 等场景：接关 Loading 黑屏在上一场景打出，须在下一关 Awake 关掉；
    /// 不干预「第 X 夜」章节图（由 LevelFlowCoordinator.showChapterSplash 控制）。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class LevelEntrySplashOverride : MonoBehaviour
    {
        void Awake()
        {
            LoadingScreenController.HideImmediate();
        }
    }
}
