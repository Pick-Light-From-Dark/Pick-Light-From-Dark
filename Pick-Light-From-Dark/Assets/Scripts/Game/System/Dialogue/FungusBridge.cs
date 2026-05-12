using UnityEngine;
using UnityEngine.UI;
using Fungus;

/// <summary>
/// Fungus 桥接层：在现有对话系统运行时，同步驱动 Fungus 的 SayDialog 与 Stage。
/// 完全自包含，运行时自动从 Fungus Resources 实例化所需组件，无需手动场景配置。
/// 剧情存档已迁移至 Fungus Save System（SaveManager + SaveData），关卡数据继续走 PlayerDataStore。
/// </summary>
public class FungusBridge : MonoBehaviour
{
    private static FungusBridge _instance;
    public static FungusBridge Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<FungusBridge>();
            return _instance;
        }
    }

    [Header("Fungus 组件（为空则运行时自动创建）")]
    public SayDialog sayDialog;
    public Stage stage;
    public Image stageBackgroundImage;

    private void Awake()
    {
        _instance = this;
        EnsureComponents();
    }

    private void EnsureComponents()
    {
        // 先触发 UIMgr 初始化，确保 EventSystem 由项目自身创建，避免 Fungus 重复创建
        var _ = UIMgr.Instance;

        // 1. SayDialog：优先使用场景中的，没有则自动创建
        if (sayDialog == null)
        {
            sayDialog = SayDialog.GetSayDialog();
            if (sayDialog == null)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/SayDialog");
                if (prefab != null)
                {
                    var go = Instantiate(prefab);
                    go.name = "SayDialog";
                    go.SetActive(false);
                    sayDialog = go.GetComponent<SayDialog>();
                }
            }
        }

        // 2. Stage：优先使用场景中的，没有则自动创建
        if (stage == null)
        {
            if (Stage.ActiveStages.Count > 0)
            {
                stage = Stage.ActiveStages[0];
            }
            else
            {
                var prefab = Resources.Load<GameObject>("Prefabs/Stage");
                if (prefab != null)
                {
                    var go = Instantiate(prefab);
                    go.name = "Stage";
                    stage = go.GetComponent<Stage>();
                }
            }
        }

        // 3. 背景 Image：优先使用 Stage 下的，没有则自动创建
        if (stageBackgroundImage == null && stage != null)
        {
            stageBackgroundImage = stage.GetComponentInChildren<Image>(true);
            if (stageBackgroundImage == null)
            {
                var canvas = stage.GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                {
                    var go = new GameObject("BackgroundImage", typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(canvas.transform, false);
                    go.transform.SetAsFirstSibling();

                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    stageBackgroundImage = go.GetComponent<Image>();
                }
            }
        }
    }

    public void ShowSay(string speakerName, string text)
    {
        if (sayDialog == null) return;

        sayDialog.NameText = speakerName;
        sayDialog.StoryText = text;
        sayDialog.gameObject.SetActive(true);
    }

    public void SetBackground(Sprite sprite)
    {
        if (sprite == null) return;

        if (stageBackgroundImage != null)
        {
            stageBackgroundImage.sprite = sprite;
            stageBackgroundImage.gameObject.SetActive(true);
        }
    }

    public void HideSayDialog()
    {
        if (sayDialog != null)
            sayDialog.gameObject.SetActive(false);
    }

    public void PlaySound(string sfxName)
    {
        if (!string.IsNullOrEmpty(sfxName))
            MusicMgr.Instance?.PlaySound(sfxName);
    }

    public void PlayBGM(string bgmName)
    {
        if (!string.IsNullOrEmpty(bgmName))
            MusicMgr.Instance?.PlayBKMusic(bgmName);
    }
}
