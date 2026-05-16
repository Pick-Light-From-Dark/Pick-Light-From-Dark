using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    private Image brightnessOverlay;

    public override void HideMe()
    {
        if (brightnessOverlay != null)
            brightnessOverlay.gameObject.SetActive(false);
    }

    public override void ShowMe()
    {
        var bkSlider = GetControl<Slider>("BkMusicControl");
        if (bkSlider != null) bkSlider.value = MusicMgr.Instance.BkMusicValue;

        var soundSlider = GetControl<Slider>("SoundControl");
        if (soundSlider != null) soundSlider.value = MusicMgr.Instance.SoundValue;

        // 总音量滑块取平均值
        var totalSlider = GetControl<Slider>("GameTotalMusicVolumeControl");
        if (totalSlider != null) totalSlider.value = (MusicMgr.Instance.BkMusicValue + MusicMgr.Instance.SoundValue) * 0.5f;

        // 亮度滑块从PlayerPrefs读取
        var lightSlider = GetControl<Slider>("LightControl");
        if (lightSlider != null) lightSlider.value = PlayerPrefs.GetFloat("ScreenBrightness", 1f);

        EnsureBrightnessOverlay();
    }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "BackBtn":
                UIMgr.Instance.HidePanel<SettingPanel>();
                if (GamePanel.Instance != null && GamePanel.Instance.gameObject.activeInHierarchy)
                    UIMgr.Instance.ShowPanel<StopGamePanel>();
                else
                    UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
        }
    }

    protected override void SliderValueChange(string sliderName, float value)
    {
        switch (sliderName)
        {
            case "LightControl":
                PlayerPrefs.SetFloat("ScreenBrightness", value);
                ApplyBrightness(value);
                break;
            case "GameTotalMusicVolumeControl":
                MusicMgr.Instance.ChangeBKMusicValue(value);
                MusicMgr.Instance.ChangeSoundValue(value);
                break;
            case "BkMusicControl":
                MusicMgr.Instance.ChangeBKMusicValue(value);
                break;
            case "SoundControl":
                MusicMgr.Instance.ChangeSoundValue(value);
                break;
        }
    }

    private void EnsureBrightnessOverlay()
    {
        if (brightnessOverlay != null) return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("BrightnessOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;

        brightnessOverlay = img;
        ApplyBrightness(PlayerPrefs.GetFloat("ScreenBrightness", 1f));
    }

    private void ApplyBrightness(float value)
    {
        if (brightnessOverlay == null) return;
        // value: 1=最亮(透明) → 0=最暗(不透明黑)
        float alpha = 1f - value;
        var c = brightnessOverlay.color;
        c.a = alpha;
        brightnessOverlay.color = c;
        brightnessOverlay.gameObject.SetActive(alpha > 0.01f);
    }

    public void DropDownChange()
    {
        Dropdown DifficultyControl = this.GetControl<Dropdown>("DifficultyControl");
    }

    void Start() { }
    void Update() { }
}
