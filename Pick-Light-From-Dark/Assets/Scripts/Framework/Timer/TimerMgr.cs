using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ��ʱ�������� ��Ҫ���ڿ�����ֹͣ�����õȵȲ�����������ʱ��
/// </summary>
public class TimerMgr : BaseManager<TimerMgr>
{
    /// <summary>
    /// ���ڼ�¼��ǰ��Ҫ������ΨһID��
    /// </summary>
    private int TIMER_KEY = 0;
    /// <summary>
    /// ���ڴ洢�������м�ʱ�����ֵ�����
    /// </summary>
    private Dictionary<int, TimerItem> timerDic = new Dictionary<int, TimerItem>();
    /// <summary>
    /// ���ڴ洢�������м�ʱ�����ֵ�����������Time.timeScaleӰ��ļ�ʱ����
    /// </summary>
    private Dictionary<int, TimerItem> realTimerDic = new Dictionary<int, TimerItem>();
    /// <summary>
    /// ���Ƴ��б�
    /// </summary>
    private List<TimerItem> delList = new List<TimerItem>();

    //Ϊ�˱����ڴ���˷� ÿ��while�������� 
    //����ֱ�ӽ�������Ϊ��Ա����
    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(intervalTime);
    private WaitForSeconds waitForSeconds = new WaitForSeconds(intervalTime);

    private Coroutine timer;
    private Coroutine realTimer;

    /// <summary>
    /// ��ʱ���������е�Ψһ��ʱ�õ�Эͬ���� �ļ��ʱ��
    /// </summary>
    private const float intervalTime = 0.1f;

    private TimerMgr() 
    {
        //Ĭ�ϼ�ʱ�����ǿ�����
        Start();
    }

    //������ʱ���������ķ���
    public void Start()
    {
        timer = MonoMgr.Instance.StartCoroutine(StartTiming(false, timerDic));
        realTimer = MonoMgr.Instance.StartCoroutine(StartTiming(true, realTimerDic));
    }

    //�رռ�ʱ���������ķ���
    public void Stop()
    {
        MonoMgr.Instance.StopCoroutine(timer);
        MonoMgr.Instance.StopCoroutine(realTimer);
    }


    IEnumerator StartTiming(bool isRealTime, Dictionary<int, TimerItem> timerDic)
    {
        while (true)
        {
            //100�������һ�μ�ʱ
            if (isRealTime)
                yield return waitForSecondsRealtime;
            else
                yield return waitForSeconds;
            //�������еļ�ʱ�� �������ݸ���
            foreach (TimerItem item in timerDic.Values)
            {
                if (!item.isRuning)
                    continue;
                //�жϼ�ʱ���Ƿ��м��ʱ��ִ�е�����
                if(item.callBack != null)
                {
                    //��ȥ100����
                    item.intervalTime -= (int)(intervalTime*1000);
                    //����һ�μ��ʱ��ִ��
                    if(item.intervalTime <= 0)
                    {
                        //���һ��ʱ�� ִ��һ�λص�
                        item.callBack.Invoke();
                        //���ü��ʱ��
                        item.intervalTime = item.maxIntervalTime;
                    }
                }
                //�ܵ�ʱ�����
                item.allTime -= (int)(intervalTime * 1000);
                //��ʱʱ�䵽 ��Ҫִ����ɻص�����
                if(item.allTime <= 0)
                {
                    item.overCallBack?.Invoke();
                    delList.Add(item);
                }
            }

            //�Ƴ����Ƴ��б��е�����
            for (int i = 0; i < delList.Count; i++)
            {
                //���ֵ����Ƴ�
                timerDic.Remove(delList[i].keyID);
                //���뻺�����
                PoolMgr.Instance.PushObj(delList[i]);
            }
            //�Ƴ������� ����б�
            delList.Clear();
        }
    }

    /// <summary>
    /// ����������ʱ��
    /// </summary>
    /// <param name="isRealTime">�����true����Time.timeScaleӰ��</param>
    /// <param name="allTime">�ܵ�ʱ�� ���� 1s=1000ms</param>
    /// <param name="overCallBack">��ʱ������ص�</param>
    /// <param name="intervalTime">�����ʱʱ�� ���� 1s=1000ms</param>
    /// <param name="callBack">�����ʱʱ����� �ص�</param>
    /// <returns>����ΨһID �����ⲿ���ƶ�Ӧ��ʱ��</returns>
    public int CreateTimer(bool isRealTime, int allTime, UnityAction overCallBack, int intervalTime = 0, UnityAction callBack = null)
    {
        //����ΨһID
        int keyID = ++TIMER_KEY;
        //�ӻ����ȡ����Ӧ�ļ�ʱ��
        TimerItem timerItem = PoolMgr.Instance.GetObj<TimerItem>();
        //��ʼ������
        timerItem.InitInfo(keyID, allTime, overCallBack, intervalTime, callBack);
        //��¼���ֵ��� �������ݸ���
        if (isRealTime)
            realTimerDic.Add(keyID, timerItem);
        else
            timerDic.Add(keyID, timerItem);
        return keyID;
    }

    //�Ƴ�������ʱ��
    public void RemoveTimer(int keyID)
    {
        if(timerDic.ContainsKey(keyID))
        {
            //�Ƴ���Ӧid��ʱ�� ���뻺���
            PoolMgr.Instance.PushObj(timerDic[keyID]);
            //���ֵ����Ƴ�
            timerDic.Remove(keyID);
        }
        else if (realTimerDic.ContainsKey(keyID))
        {
            //�Ƴ���Ӧid��ʱ�� ���뻺���
            PoolMgr.Instance.PushObj(realTimerDic[keyID]);
            //���ֵ����Ƴ�
            realTimerDic.Remove(keyID);
        }
    }

    /// <summary>
    /// ���õ�����ʱ��
    /// </summary>
    /// <param name="keyID">��ʱ��ΨһID</param>
    public void ResetTimer(int keyID)
    {
        if (timerDic.ContainsKey(keyID))
        {
            timerDic[keyID].ResetTimer();
        }
        else if (realTimerDic.ContainsKey(keyID))
        {
            realTimerDic[keyID].ResetTimer();
        }
    }

    /// <summary>
    /// ����������ʱ�� ��Ҫ������ͣ�����¿�ʼ
    /// </summary>
    /// <param name="keyID">��ʱ��ΨһID</param>
    public void StartTimer(int keyID)
    {
        if (timerDic.ContainsKey(keyID))
        {
            timerDic[keyID].isRuning = true;
        }
        else if (realTimerDic.ContainsKey(keyID))
        {
            realTimerDic[keyID].isRuning = true;
        }
    }

    /// <summary>
    /// ֹͣ������ʱ�� ��Ҫ������ͣ
    /// </summary>
    /// <param name="keyID">��ʱ��ΨһID</param>
    public void StopTimer(int keyID)
    {
        if (timerDic.ContainsKey(keyID))
        {
            timerDic[keyID].isRuning = false;
        }
        else if (realTimerDic.ContainsKey(keyID))
        {
            realTimerDic[keyID].isRuning = false;
        }
    }
}

