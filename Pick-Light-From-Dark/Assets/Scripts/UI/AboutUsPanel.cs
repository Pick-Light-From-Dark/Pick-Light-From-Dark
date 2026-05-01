using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutUsPanel : BasePanel
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
                UIMgr.Instance.HidePanel<AboutUsPanel>();
                UIMgr.Instance.ShowPanel<BeginPanel>();
                break;

        }
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
