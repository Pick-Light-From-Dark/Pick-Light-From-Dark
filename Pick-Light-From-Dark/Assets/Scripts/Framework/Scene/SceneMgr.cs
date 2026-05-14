using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换管理器 负责场景切换相关
/// </summary>
public class SceneMgr : BaseManager<SceneMgr>
{
    private SceneMgr() { }

    //同步切换场景的方法
    public void LoadScene(string name, UnityAction callBack = null)
    {
        SceneManager.LoadScene(name);
        callBack?.Invoke();
        callBack = null;
    }

    //异步切换场景的方法
    public void LoadSceneAsyn(string name, UnityAction callBack = null)
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadSceneAsyn(name, callBack));
    }

    private IEnumerator ReallyLoadSceneAsyn(string name, UnityAction callBack)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        while (!ao.isDone)
        {
            EventCenter.Instance.EventTrigger<float>(E_EventType.E_SceneLoadChange, ao.progress);
            yield return 0;
        }
        EventCenter.Instance.EventTrigger<float>(E_EventType.E_SceneLoadChange, 1);

        callBack?.Invoke();
        callBack = null;
    }
}
