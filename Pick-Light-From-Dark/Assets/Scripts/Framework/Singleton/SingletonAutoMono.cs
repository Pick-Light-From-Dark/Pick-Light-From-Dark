using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 自动创建方式 继承Mono的单例模式封装
/// 推荐使用
/// 手动挂载 动态加载 不会出现当场景不存在时报错的情况
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonAutoMono<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                Debug.Log($"[SingletonAutoMono] 创建新实例 {typeof(T).Name}, 调用栈:");
                Debug.Log(UnityEngine.StackTraceUtility.ExtractStackTrace());

                //静态创建 动态加载
                //在场景上创建一个空物体
                GameObject obj = new GameObject();
                //得到T脚本的名字 为了可读性 编辑器中可看到准确的物体名
                //单例模式脚本挂载的GameObject
                obj.name = typeof(T).ToString();
                //静态得到对应组件 单例模式脚本
                instance = obj.AddComponent<T>();
                //过场景时不移除该物体 保证其只有一个存在游戏整个运行周期中
                DontDestroyOnLoad(obj);

                Debug.Log($"[SingletonAutoMono] 实例创建完成 InstanceID:{instance.GetInstanceID()}");
            }
            return instance;
        }
    }

}
