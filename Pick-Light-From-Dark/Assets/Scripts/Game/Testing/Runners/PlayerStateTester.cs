using UnityEngine;
using Game.Data;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 玩家状态手动测试器
    /// 测试 SetInBed/SetEyesClosed/ToggleEyesClosed 及对应事件
    /// </summary>
    public class PlayerStateTester : MonoBehaviour
    {
        [Header("UI 显示")]
        public bool showOnGUI = true;

        private PlayerState playerState;
        private int eyeCloseChangedCount;
        private int eyeCloseStartCount;
        private int eyeCloseEndCount;

        void Start()
        {
            playerState = PlayerState.Instance;
            EventCenter.Instance.AddEventListener<bool>(E_EventType.PlayerEyeCloseChanged, OnEyeCloseChanged);
            EventCenter.Instance.AddEventListener(E_EventType.EyeCloseStart, OnEyeCloseStart);
            EventCenter.Instance.AddEventListener(E_EventType.EyeCloseEnd, OnEyeCloseEnd);
            Debug.Log("[PlayerStateTester] 已就绪");
        }

        void OnDestroy()
        {
            if (EventCenter.Instance == null) return;
            EventCenter.Instance.RemoveEventListener<bool>(E_EventType.PlayerEyeCloseChanged, OnEyeCloseChanged);
            EventCenter.Instance.RemoveEventListener(E_EventType.EyeCloseStart, OnEyeCloseStart);
            EventCenter.Instance.RemoveEventListener(E_EventType.EyeCloseEnd, OnEyeCloseEnd);
        }

        private void OnEyeCloseChanged(bool closed)
        {
            eyeCloseChangedCount++;
            Debug.Log($"[PlayerStateTester] 收到 PlayerEyeCloseChanged({closed}) 第{eyeCloseChangedCount}次");
        }

        private void OnEyeCloseStart()
        {
            eyeCloseStartCount++;
            Debug.Log($"[PlayerStateTester] 收到 EyeCloseStart 第{eyeCloseStartCount}次");
        }

        private void OnEyeCloseEnd()
        {
            eyeCloseEndCount++;
            Debug.Log($"[PlayerStateTester] 收到 EyeCloseEnd 第{eyeCloseEndCount}次");
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            int x = 10, y = 10, w = 320, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 250), "PlayerState 测试器");
            y += 25;

            if (playerState != null)
            {
                GUI.Label(new Rect(x, y, w, h), $"床上: {playerState.IsInBed()}  闭眼: {playerState.IsEyesClosed()}"); y += h;
            }

            GUI.Label(new Rect(x, y, w, h), $"PlayerEyeCloseChanged 次数: {eyeCloseChangedCount}"); y += h;
            GUI.Label(new Rect(x, y, w, h), $"EyeCloseStart 次数: {eyeCloseStartCount}"); y += h;
            GUI.Label(new Rect(x, y, w, h), $"EyeCloseEnd 次数: {eyeCloseEndCount}"); y += h + 4;

            if (GUI.Button(new Rect(x, y, w, h), "SetInBed(true)")) playerState.SetInBed(true); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "SetInBed(false)")) playerState.SetInBed(false); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "SetEyesClosed(true)")) playerState.SetEyesClosed(true); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "SetEyesClosed(false)")) playerState.SetEyesClosed(false); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "ToggleEyesClosed (等同 C 键)")) playerState.ToggleEyesClosed(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "重置事件计数器"))
            {
                eyeCloseChangedCount = eyeCloseStartCount = eyeCloseEndCount = 0;
            }
        }
    }
}
