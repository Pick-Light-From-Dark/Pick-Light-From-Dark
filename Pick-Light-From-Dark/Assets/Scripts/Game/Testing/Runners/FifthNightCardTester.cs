using UnityEngine;
using Game.Config;
using Game.Flow;

/// <summary>
/// 第五夜卡牌测试启动器。
/// 挂到场景任意 GameObject 上，Play 即自动初始化第五关并弹出 GamePanel。
/// </summary>
public class FifthNightCardTester : MonoBehaviour
{
    public bool autoStart = true;

    void Start()
    {
        if (!autoStart) return;

        var config = Resources.Load<LevelConfigSO>("Config/LevelConfig_5");
        if (config == null)
        {
            Debug.LogError("[FifthNightCardTester] 未找到 Resources/Config/LevelConfig_5.asset，请先在 Unity 中运行 Game > Generate Level Configs > Generate Level 5 Config");
            return;
        }

        GameFlowController.Instance.Initialize(config);
        UIMgr.Instance.ShowPanel<GamePanel>();
        UIMgr.Instance.HidePanel<BeginPanel>();

        Debug.Log($"[FifthNightCardTester] 第五夜已启动，初始卡牌: {string.Join(", ", config.initialCards)}");
    }
}
