using UnityEngine;

public class SceneMusicConfig : MonoBehaviour
{
    public string bgmName;

    void Start()
    {
        if (!string.IsNullOrEmpty(bgmName))
        {
            MusicMgr.Instance.PlayBKMusic(bgmName);
        }
    }
}