using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BeginPanel : BasePanel
{
    private Button continueBtn;
    private CanvasGroup continueBtnCanvasGroup;

    public override void HideMe() { }

    public override void ShowMe()
    {
        SetupAllButtonHover();
        UpdateContinueButton();
        Time.timeScale = 1f;
        MusicMgr.Instance.ResumeBKMusic();
    }

    private void UpdateContinueButton()
    {
        if (continueBtn == null)
        {
            var t = transform.Find("ContinueBtn");
            if (t != null) continueBtn = t.GetComponent<Button>();
        }
        if (continueBtn != null)
        {
            bool hasSave = Game.Test.CrossLevelSaveSystem.Instance?.HasSave() == true;
            continueBtn.interactable = hasSave;
            if (continueBtnCanvasGroup == null)
                continueBtnCanvasGroup = continueBtn.GetComponent<CanvasGroup>();
            if (continueBtnCanvasGroup == null)
                continueBtnCanvasGroup = continueBtn.gameObject.AddComponent<CanvasGroup>();
            continueBtnCanvasGroup.alpha = hasSave ? 1f : 0.35f;
            continueBtnCanvasGroup.interactable = hasSave;
        }
    }

    private void SetupAllButtonHover()
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            var hoverImage = btn.transform.Find("Image");
            if (hoverImage == null) continue;
            hoverImage.gameObject.SetActive(false);

            var trigger = btn.gameObject.GetComponent<HoverImageTrigger>();
            if (trigger == null)
                trigger = btn.gameObject.AddComponent<HoverImageTrigger>();
            trigger.hoverImage = hoverImage.gameObject;
        }
    }

    private class HoverImageTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject hoverImage;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverImage != null) hoverImage.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverImage != null) hoverImage.SetActive(false);
        }
    }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "StartBtn":
                MusicMgr.Instance.PauseBKMusic();
                Game.Test.CrossLevelSaveSystem.Instance?.MarkGameCompleted();
                Game.Flow.GameFlowController.Instance?.ResetForNewGame();
                Time.timeScale = 1f;
                UIMgr.Instance.HidePanel<BeginPanel>(true);
                SceneMgr.Instance.LoadScene("Level1");
                break;

            case "ContinueBtn":
                Time.timeScale = 1f;
                ContinueGame();
                break;

            case "SaveBtn":
                UIMgr.Instance.ShowPanel<SaveGamePanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "SettingBtn":
                UIMgr.Instance.ShowPanel<SettingPanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "ExperienceBtn":
                UIMgr.Instance.ShowPanel<ExperiencePanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "AboutUsBtn":
                UIMgr.Instance.ShowPanel<AboutUsPanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "QuitBtn":
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    private void ContinueGame()
    {
        var save = Game.Test.CrossLevelSaveSystem.Instance;
        if (save == null || !save.HasSave()) return;

        var cp = save.LoadCheckpoint();
        string sceneName = cp.currentLevelId > 0 ? $"Level{cp.currentLevelId}" : "Level1";
        MusicMgr.Instance.PauseBKMusic();
        UIMgr.Instance.HidePanel<BeginPanel>(true);
        SceneMgr.Instance.LoadScene(sceneName);
    }
}
