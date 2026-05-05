using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    
    void Start()
    {
        UIMgr.Instance.ShowPanel<BeginPanel>();
        Debug.Log("Main Start");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
