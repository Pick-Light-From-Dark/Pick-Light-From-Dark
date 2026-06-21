using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 结局结果面板（原木门选项面板已废弃，天台选择已取消）
    /// 第五关根据卡牌使用+血量直接判定结局，不再弹出选择
    /// </summary>
    public class RooftopChoicePanel : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("[RooftopChoicePanel] 天台选择已取消，结局由卡牌+血量直接判定");
            var saveSystem = CrossLevelSaveSystem.Instance;
            if (saveSystem != null)
            {
                saveSystem.EvaluateEnding();
            }
            Destroy(gameObject);
            EventCenter.Instance?.EventTrigger(E_EventType.GameWin);
        }
    }
}
