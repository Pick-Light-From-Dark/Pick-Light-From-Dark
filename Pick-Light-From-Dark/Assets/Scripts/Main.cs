using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIMgr.Instance.ShowPanel<BeginPanel>(E_UILayer.Middle, (panel) =>
        {
            print("初始面板显示成功");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
