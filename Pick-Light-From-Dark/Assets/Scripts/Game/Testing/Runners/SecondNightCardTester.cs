using UnityEngine;
using Game.Config;
using Game.Flow;

/// <summary>
/// 第二夜卡牌测试启动器。
/// 挂到场景任意 GameObject 上，Play 即自动初始化第二关并弹出 GamePanel。
/// </summary>
public class SecondNightCardTester : MonoBehaviour
{
    public bool autoStart = true;

    void Start()
    {
        if (!autoStart) return;

        var config = Resources.Load<LevelConfigSO>("Config/LevelConfig_2");
        if (config == null)
        {
            Debug.LogError("[SecondNightCardTester] 未找到 Resources/Config/LevelConfig_2.asset，请先在 Unity 中运行 Game > Generate Level Configs > Generate Level 2 Config");
            return;
        }

        GameFlowController.Instance.Initialize(config);
        UIMgr.Instance.ShowPanel<GamePanel>();
        UIMgr.Instance.HidePanel<BeginPanel>();

        Debug.Log($"[SecondNightCardTester] 第二夜已启动，初始卡牌: {string.Join(", ", config.initialCards)}");
    }
}
