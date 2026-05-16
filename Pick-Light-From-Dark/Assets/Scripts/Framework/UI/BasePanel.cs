using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    /// <summary>
    /// 用于存储面板上需要用的UI控件 以便将来替换原有 拖拽修改等
    /// </summary>
    protected Dictionary<string, UIBehaviour> controlDic = new Dictionary<string, UIBehaviour>();

    /// <summary>
    /// 控件默认名称 如果获取到的控件名称存在于这个列表中 则意味着我们不能通过名称去使用它 它只属于外观显示的控件
    /// </summary>
    private static List<string> defaultNameList = new List<string>() { "Image",
                                                                   "Text (TMP)",
                                                                   "RawImage",
                                                                   "Background",
                                                                   "Checkmark",
                                                                   "Label",
                                                                   "Text (Legacy)",
                                                                   "Arrow",
                                                                   "Placeholder",
                                                                   "Fill",
                                                                   "Handle",
                                                                   "Viewport",
                                                                   "Scrollbar Horizontal",
                                                                   "Scrollbar Vertical"};


    protected virtual void Awake()
    {
        //为了避免 某一个子对象上挂载多个同种控件
        //应该先查找不需要的控件
        FindChildrenControl<Button>();
        FindChildrenControl<Toggle>();
        FindChildrenControl<Slider>();
        FindChildrenControl<InputField>();
        FindChildrenControl<ScrollRect>();
        FindChildrenControl<Dropdown>();
        //即使手上挂了多个脚本 只要我找到了我需要的
        //之后也能通过需要获取的脚本来获取对应的控件
        FindChildrenControl<Text>();
        FindChildrenControl<TextMeshPro>();
        FindChildrenControl<Image>();
    }

    /// <summary>
    /// 面板显示时调用的逻辑
    /// </summary>
    public abstract void ShowMe();

    /// <summary>
    /// 面板隐藏时调用的逻辑
    /// </summary>
    public abstract void HideMe();

    /// <summary>
    /// 获取指定名称以及指定类型的控件
    /// </summary>
    /// <typeparam name="T">控件类型</typeparam>
    /// <param name="name">控件名称</param>
    /// <returns></returns>
    public T GetControl<T>(string name) where T:UIBehaviour
    {
        if(controlDic.ContainsKey(name))
        {
            T control = controlDic[name] as T;
            if (control == null)
                Debug.LogError($"存在对应名称{name}但类型不为{typeof(T)}的控件");
            return control;
        }
        else
        {
            Debug.LogError($"不存在对应名称{name}的控件");
            return null;
        }
    }

    /// <summary>
    /// 手动注册动态生成的控件（如运行时实例化的卡牌、列表项等）
    /// </summary>
    protected void RegisterControl<T>(string name, T control) where T : UIBehaviour
    {
        if (controlDic.ContainsKey(name))
        {
            Debug.LogWarning($"控件名称{name}已存在，将被覆盖");
            controlDic[name] = control;
        }
        else
        {
            controlDic.Add(name, control);
        }
    }

    /// <summary>
    /// 移除动态注册的控件 用于动态生成物销毁时清理
    /// </summary>
    protected void RemoveControl(string name)
    {
        if (controlDic.ContainsKey(name))
            controlDic.Remove(name);
    }

    /// <summary>
    /// 从指定父节点查找并注册子控件 适用于动态实例化的子面板
    /// </summary>
    protected void FindChildrenControl<T>(Transform parent) where T : UIBehaviour
    {
        T[] controls = parent.GetComponentsInChildren<T>(true);
        for (int i = 0; i < controls.Length; i++)
        {
            string controlName = controls[i].gameObject.name;
            if (!controlDic.ContainsKey(controlName) && !defaultNameList.Contains(controlName))
            {
                controlDic.Add(controlName, controls[i]);
                BindControlEvent(controls[i], controlName);
            }
        }
    }

    /// <summary>
    /// 格式化情绪值变化文本 慌+5 兴-3
    /// </summary>
    protected static string FormatEmoDelta(int panicDelta, int exciteDelta)
    {
        string s = "";
        if (panicDelta != 0) s += $"慌{panicDelta:+0;-0}";
        if (exciteDelta != 0) s += $" 兴{exciteDelta:+0;-0}";
        return s.Trim();
    }

    /// <summary>
    /// 根据恐慌值获取当前情绪档位 0=安全 1=警告 2=危险
    /// </summary>
    protected static int GetPanicLevel(int panicValue, int lowThreshold = 50, int highThreshold = 75)
    {
        if (panicValue <= lowThreshold) return 0;
        if (panicValue <= highThreshold) return 1;
        return 2;
    }

    protected virtual void ClickBtn(string btnName)
    {

    }

    protected virtual void SliderValueChange(string sliderName, float value)
    {

    }

    protected virtual void ToggleValueChange(string sliderName, bool value)
    {

    }

    /// <summary>子类可重写为false来禁用按钮悬停/点击音效</summary>
    protected virtual bool EnableButtonSounds => true;

    private void BindControlEvent<T>(T control, string controlName) where T : UIBehaviour
    {
        if (control is Button btn)
        {
            if (EnableButtonSounds)
            {
                btn.onClick.AddListener(() =>
                {
                    MusicMgr.Instance.PlaySound("DXH_SOUND/SOUND3/33.钢笔敲击纸的声音");
                    ClickBtn(controlName);
                });

                // 悬停音效
                var trigger = btn.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
                var enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((_) =>
                {
                    MusicMgr.Instance.PlaySound("DXH_SOUND/SOUND3/32.钢笔写字声");
                });
                trigger.triggers.Add(enterEntry);
            }
            else
            {
                btn.onClick.AddListener(() => ClickBtn(controlName));
            }
        }
        else if (control is Slider slider)
        {
            slider.onValueChanged.AddListener((value) => SliderValueChange(controlName, value));
        }
        else if (control is Toggle toggle)
        {
            toggle.onValueChanged.AddListener((value) => ToggleValueChange(controlName, value));
        }
    }

    private void FindChildrenControl<T>() where T:UIBehaviour
    {
        T[] controls = this.GetComponentsInChildren<T>(true);
        for (int i = 0; i < controls.Length; i++)
        {
            //获取当前控件名称
            string controlName = controls[i].gameObject.name;
            //通过这种方式 避免重复添加控件
            if (!controlDic.ContainsKey(controlName))
            {
                if(!defaultNameList.Contains(controlName))
                {
                    controlDic.Add(controlName, controls[i]);
                    //判断控件类型 决定是否添加事件监听
                    BindControlEvent(controls[i], controlName);
                }

            }
        }
    }
}
