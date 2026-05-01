using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public override void HideMe()
    {
        
    }

    public override void ShowMe()
    {
        
    }
    protected override void ClickBtn(string btnName)
    {
        switch (btnName)

        {
            case "BackBtn":
                UIMgr.Instance.HidePanel<SettingPanel>();
                UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
            
        }
    }
    protected override void SliderValueChange(string sliderName, float value)
    {
        switch (sliderName)
        {
            case "LightControl":
                break;
            case "GameTotalMusicVolumeControl":
                break;
            case "BkMusicControl":
                break;
            case "SoundControl":
                break;
        }
    }
    public void DropDownChange()
    {
        Dropdown DifficultyControl = this.GetControl<Dropdown>("DifficultyControl");

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
