using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExperiencePanel : BasePanel
{
    public GameObject CG;
    public GameObject Achievement;
    public GameObject EndingCollection;
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
                UIMgr.Instance.HidePanel<ExperiencePanel>();
                UIMgr.Instance.ShowPanel<BeginPanel>();
                break;

            case "CGbtn":
                EndingCollection.SetActive(false);
                Achievement.SetActive(false);
                CG.SetActive(true);
                break;

            case  string BtnName when BtnName.StartsWith("CGContentBtn"):
                OnCGContentBtnClick(BtnName);
                break;                

            case "AchievementCollectionBtn":
                CG.SetActive(false);
                EndingCollection.SetActive(false);
                Achievement.SetActive(true);
                break;

            case string name when name.StartsWith("AchBtn"):
                int achIndex = int.Parse(name.Replace("AchBtn", ""));
                OnAchBtnClick(achIndex);
                break;

            case "EndingCollectionBtn":
                CG.SetActive(false);
                Achievement.SetActive(false);
                EndingCollection.SetActive(true);           
                break;

            case string name when name.StartsWith("EndingBtn"):
                int endingIndex = int.Parse(name.Replace("EndingBtn", ""));
                OnEndingBtnClick(endingIndex);
                break;
        }
    }
    private void OnCGContentBtnClick(string btnName)
    {
        // 获取最后一位数字 1~6
        string numStr = btnName.Replace("CGContentBtn", "");
        if (int.TryParse(numStr, out int index))
        {
            Debug.Log("点击了第 " + index + " 个CG按钮");
            UIMgr.Instance.ShowPanel<CGContentPanel>();
           
        }
    }
    // 成就按钮统一处理
    private void OnAchBtnClick(int index)
    {
        Debug.Log($"点击了第 {index} 个成就按钮");
        UIMgr.Instance.ShowPanel<AchievementContentPanel>();
        
    }

    // 结局按钮统一处理
    private void OnEndingBtnClick(int index)
    {
        Debug.Log($"点击了第 {index} 个结局按钮");
        UIMgr.Instance.ShowPanel<EndingContentPanel>();
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
