using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ���� ��ʽ�滻ԭ�� װ�� ����ĸ���
/// </summary>
public abstract class EventInfoBase{ }

/// <summary>
/// �������� ��Ӧ�۲��� ����ί�е� ��
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventInfo<T>:EventInfoBase
{
    //�����۲��� ��Ӧ�� ������Ϣ ��¼������
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

/// <summary>
/// ��Ҫ������¼�޲��޷���ֵί��
/// </summary>
public class EventInfo: EventInfoBase
{
    public UnityAction actions;
     
    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}


/// <summary>
/// �¼�����ģ�� 
/// </summary>
public class EventCenter: BaseManager<EventCenter>
{
    //���ڼ�¼��Ӧ�¼� ������ ��Ӧ���߼�
    private Dictionary<E_EventType, EventInfoBase> eventDic = new Dictionary<E_EventType, EventInfoBase>();

    private EventCenter() { }

    /// <summary>
    /// �����¼� 
    /// </summary>
    /// <param name="eventName">�¼�����</param>
    public void EventTrigger<T>(E_EventType eventName, T info)
    {
        //���ڹ����ҵ��� ��֪ͨ����ȥ�����߼�
        if(eventDic.ContainsKey(eventName))
        {
            //ȥִ�ж�Ӧ���߼�
            (eventDic[eventName] as EventInfo<T>).actions?.Invoke(info);
        }
    }

    /// <summary>
    /// �����¼� �޲���
    /// </summary>
    /// <param name="eventName"></param>
    public void EventTrigger(E_EventType eventName)
    {
        //���ڹ����ҵ��� ��֪ͨ����ȥ�����߼�
        if (eventDic.ContainsKey(eventName))
        {
            //ȥִ�ж�Ӧ���߼�
            (eventDic[eventName] as EventInfo).actions?.Invoke();
        }
    }


    /// <summary>
    /// �����¼�������
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="func"></param>
    public void AddEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        //����Ѿ����ڹ����¼���ί�м�¼ ֱ�����Ӽ���
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo<T>(func));
        }
    }

    public void AddEventListener(E_EventType eventName, UnityAction func)
    {
        //����Ѿ����ڹ����¼���ί�м�¼ ֱ�����Ӽ���
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo(func));
        }
    }

    /// <summary>
    /// �Ƴ��¼�������
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="func"></param>
    public void RemoveEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo<T>).actions -= func;
    }

    public void RemoveEventListener(E_EventType eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo).actions -= func;
    }

    /// <summary>
    /// ��������¼��ļ���
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }

}
