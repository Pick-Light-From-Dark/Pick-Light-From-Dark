using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveGamePanel : BasePanel
{
    private Button SaveGameBtn1;
    private Button SaveGameBtn2;
    private Button SaveGameBtn3;
    private Button SaveGameBtn4;
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
    private void SetBtnImage(string btnName, string imagePath)
    {
        // 1. 获取按钮上的 Image 组件
        Image btnImage = GetControl<Image>(btnName);

        // 2. 从 Resources 文件夹加载图片
        Sprite sprite = Resources.Load<Sprite>(imagePath);

        // 3. 设置
        if (btnImage != null && sprite != null)
        {
            btnImage.sprite = sprite;
        }
    }
}
