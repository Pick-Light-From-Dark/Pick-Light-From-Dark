using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGamePanel : BasePanel
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
                UIMgr.Instance.HidePanel<SaveGamePanel>();
                UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
            case "SaveGameBtn1":
                break;
            case "SaveGameBtn2":
                break;
            case "SaveGameBtn3":
                break;
            case "SaveGameBtn4":
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
