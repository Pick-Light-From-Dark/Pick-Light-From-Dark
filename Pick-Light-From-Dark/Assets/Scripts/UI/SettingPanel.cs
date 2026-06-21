using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    private const string DifficultyPrefsKey = "GameDifficulty";
    private const string MasterVolumePrefsKey = "MasterVolume";

    public override void HideMe() { }

    public override void ShowMe()
    {
        // BGM音量
        var bkSlider = GetControl<Slider>("BkMusicControl");
        if (bkSlider != null) bkSlider.value = MusicMgr.Instance.BkMusicValue;

        // 音效音量
        var soundSlider = GetControl<Slider>("SoundControl");
        if (soundSlider != null) soundSlider.value = MusicMgr.Instance.SoundValue;

        // 总音量
        var masterSlider = GetControl<Slider>("GameTotalMusicVolumeControl");
        if (masterSlider != null)
            masterSlider.value = MusicMgr.Instance.MasterVolume;

        // 难度
        var diffDropdown = GetControl<Dropdown>("DifficultyControl");
        if (diffDropdown != null)
        {
            diffDropdown.options.Clear();
            diffDropdown.options.Add(new Dropdown.OptionData("简单"));
            diffDropdown.options.Add(new Dropdown.OptionData("普通"));
            diffDropdown.options.Add(new Dropdown.OptionData("困难"));
            diffDropdown.value = PlayerPrefs.GetInt(DifficultyPrefsKey, 1);
            diffDropdown.onValueChanged.RemoveAllListeners();
            diffDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        }

        // 亮度（如有滑块则初始化为1）
        var lightSlider = GetControl<Slider>("LightControl");
        if (lightSlider != null)
        {
            lightSlider.value = PlayerPrefs.GetFloat("ScreenBrightness", 1f);
            lightSlider.onValueChanged.RemoveAllListeners();
            lightSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }
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
            case "BkMusicControl":
                MusicMgr.Instance.ChangeBKMusicValue(value);
                break;

            case "SoundControl":
                MusicMgr.Instance.ChangeSoundValue(value);
                break;

            case "GameTotalMusicVolumeControl":
                MusicMgr.Instance.ChangeMasterVolume(value);
                break;

            case "LightControl":
                PlayerPrefs.SetFloat("ScreenBrightness", value);
                Screen.brightness = value;
                break;
        }
    }

    void OnDifficultyChanged(int index)
    {
        PlayerPrefs.SetInt(DifficultyPrefsKey, index);
        PlayerPrefs.Save();
        Debug.Log($"[SettingPanel] 难度设置为: {(index == 0 ? "简单" : index == 1 ? "普通" : "困难")}");
    }

    void OnBrightnessChanged(float value)
    {
        PlayerPrefs.SetFloat("ScreenBrightness", value);
        PlayerPrefs.Save();
        Screen.brightness = value;
    }
}
