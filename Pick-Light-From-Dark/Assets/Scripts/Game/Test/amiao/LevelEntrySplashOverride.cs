using Game.Flow;
using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 挂在目标关卡场景（如 Level3）。若上一场景胜利剧情设置了 SuppressChapterSplashForLevelId，
    /// 则本关不再播「第 X 夜」章节图，避免接关时闪一下标题屏。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class LevelEntrySplashOverride : MonoBehaviour
    {
        /// <summary>下一关加载前由 Level2VictoryStoryRunner 等设置。</summary>
        public static int SuppressChapterSplashForLevelId;

        public int levelId = 3;

        void Awake()
        {
            // Level2_end 切场景时协程会被销毁，黑屏 Loading 须在本关 Awake 关掉
            LoadingScreenController.HideImmediate();

            if (SuppressChapterSplashForLevelId != levelId)
                return;

            SuppressChapterSplashForLevelId = 0;

            var coord = FindFirstObjectByType<LevelFlowCoordinator>();
            if (coord == null || coord.levelId != levelId)
                return;

            coord.showChapterSplash = false;
            Debug.Log($"[LevelEntrySplash] 跳过 Level{levelId} 章节图（接胜利剧情关）");
        }
    }
}
