using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// �㼶ö��
/// </summary>
public enum E_UILayer
{
    /// <summary>
    /// ��ײ�
    /// </summary>
    Bottom,
    /// <summary>
    /// �в�
    /// </summary>
    Middle,
    /// <summary>
    /// �߲�
    /// </summary>
    Top,
    /// <summary>
    /// ϵͳ�� ��߲�
    /// </summary>
    System,
}

/// <summary>
/// ��������UI���Ĺ�����
/// ע�⣺���Ԥ������Ҫ���������һ�£���������
/// </summary>
public class UIMgr : BaseManager<UIMgr>
{
    /// <summary>
    /// ��Ҫ������ʽ�滻ԭ�� ���ֵ��� �ø�������װ���������
    /// </summary>
    private abstract class BasePanelInfo { }

    /// <summary>
    /// ���ڴ洢�����Ϣ �ͼ�����ɵĻص�������
    /// </summary>
    /// <typeparam name="T">��������</typeparam>
    private class PanelInfo<T> : BasePanelInfo where T:BasePanel
    {
        public T panel;
        public UnityAction<T> callBack;
        public bool isHide;

        public PanelInfo(UnityAction<T> callBack)
        {
            this.callBack += callBack;
        }
    }


    private Camera uiCamera;
    private Canvas uiCanvas;
    private EventSystem uiEventSystem;

    //�㼶������
    private Transform bottomLayer;
    private Transform middleLayer;
    private Transform topLayer;
    private Transform systemLayer;

    /// <summary>
    /// ���ڴ洢���е�������
    /// </summary>
    private Dictionary<string, BasePanelInfo> panelDic = new Dictionary<string, BasePanelInfo>();

    private UIMgr()
    {
        //��̬����Ψһ��Canvas��EventSystem���������
        uiCamera = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/UICamera")).GetComponent<Camera>();
        //ui��������������Ƴ� ר��������ȾUI���
        GameObject.DontDestroyOnLoad(uiCamera.gameObject);

        //��̬����Canvas
        uiCanvas = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/Canvas")).GetComponent<Canvas>();
        //����ʹ�õ�UI�����
        uiCanvas.worldCamera = uiCamera;
        //���������Ƴ�
        GameObject.DontDestroyOnLoad(uiCanvas.gameObject);

        //�ҵ��㼶������
        bottomLayer = uiCanvas.transform.Find("Bottom");
        middleLayer = uiCanvas.transform.Find("Middle");
        topLayer = uiCanvas.transform.Find("Top");
        systemLayer = uiCanvas.transform.Find("System");

        //��̬����EventSystem
        uiEventSystem = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/EventSystem")).GetComponent<EventSystem>();
        GameObject.DontDestroyOnLoad(uiEventSystem.gameObject);
    }

    /// <summary>
    /// ��ȡ��Ӧ�㼶�ĸ�����
    /// </summary>
    /// <param name="layer">�㼶ö��ֵ</param>
    /// <returns></returns>
    public Transform GetLayerFather(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.Bottom:
                return bottomLayer;
            case E_UILayer.Middle:
                return middleLayer;
            case E_UILayer.Top:
                return topLayer;
            case E_UILayer.System:
                return systemLayer;
            default:
                return null;
        }
    }

    /// <summary>
    /// ��ʾ���
    /// </summary>
    /// <typeparam name="T">��������</typeparam>
    /// <param name="layer">�����ʾ�Ĳ㼶</param>
    /// <param name="callBack">���ڿ������첽���� ���ͨ��ί�лص�����ʽ ��������ɵ���崫�ݳ�ȥ����ʹ��</param>
    /// <param name="isSync">�Ƿ����ͬ������ Ĭ��Ϊfalse</param>
    public void ShowPanel<T>(E_UILayer layer = E_UILayer.Middle, UnityAction<T> callBack = null, bool isSync = false) where T:BasePanel
    {
        //��ȡ����� Ԥ������������������һ�� 
        string panelName = typeof(T).Name;
        //�������
        if(panelDic.ContainsKey(panelName))
        {
            //ȡ���ֵ����Ѿ�ռ��λ�õ�����
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            //�����첽������
            if(panelInfo.panel == null)
            {
                //���֮ǰ��ʾ�������� ����������ʾ ��ôֱ����Ϊfalse
                panelInfo.isHide = false;

                //��������첽���� Ӧ�õȴ���������� ֻ��Ҫ��¼�ص����� �������ȥ���ü���
                if (callBack != null)
                    panelInfo.callBack += callBack;
            }
            else//�Ѿ����ؽ���
            {
                //�����ʧ��״̬ ֱ�Ӽ������ �Ϳ�����ʾ��
                if (!panelInfo.panel.gameObject.activeSelf)
                    panelInfo.panel.gameObject.SetActive(true);

                //���Ҫ��ʾ��� ��ִ��һ������Ĭ����ʾ�߼�
                panelInfo.panel.ShowMe();
                //������ڻص� ֱ�ӷ��س�ȥ����
                callBack?.Invoke(panelInfo.panel);
            }
            return;
        }

        //��������� �ȴ����ֵ䵱�� ռ��λ�� ֮���������ʾ �Ҳ��ܵõ��ֵ��е���Ϣ�����ж�
        panelDic.Add(panelName, new PanelInfo<T>(callBack));

        //��������� �������
        ResMgr.Instance.LoadAsync<GameObject>("UI/" + panelName, (res) =>
        {
            //ȡ���ֵ����Ѿ�ռ��λ�õ�����
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            //��ʾ�첽���ؽ���ǰ ����Ҫ���ظ������ 
            if(panelInfo.isHide)
            {
                panelDic.Remove(panelName);
                return;
            }

            //�㼶�Ĵ���
            Transform father = GetLayerFather(layer);
            //����û�а�ָ�����򴫵ݲ㼶���� ����Ϊ��
            if (father == null)
                father = middleLayer;
            //�����Ԥ���崴������Ӧ�������� ���ұ���ԭ�������Ŵ�С
            GameObject panelObj = GameObject.Instantiate(res, father, false);

            //��ȡ��ӦUI������س�ȥ
            T panel = panelObj.GetComponent<T>();
            //��ʾ���ʱִ�е�Ĭ�Ϸ���
            panel.ShowMe();
            //����ȥʹ��
            panelInfo.callBack?.Invoke(panel);
            //�ص�ִ���� ������� �����ڴ�й©
            panelInfo.callBack = null;
            //�洢panel
            panelInfo.panel = panel;

        });
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <typeparam name="T">�������</typeparam>
    public void HidePanel<T>(bool isDestory = false) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            //ȡ���ֵ����Ѿ�ռ��λ�õ�����
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            //�������ڼ�����
            if(panelInfo.panel == null)
            {
                //�޸����ر�ʾ ��ʾ �����弴��Ҫ����
                panelInfo.isHide = true;
                //��ȻҪ������ �ص���������������� ֱ���ÿ�
                panelInfo.callBack = null;
            }
            else//�Ѿ����ؽ���
            {
                //ִ��Ĭ�ϵ����������Ҫ��������
                panelInfo.panel.HideMe();
                //���Ҫ����  ��ֱ�ӽ�������ٴ��ֵ����Ƴ���¼
                if (isDestory)
                {
                    //�������
                    GameObject.Destroy(panelInfo.panel.gameObject);
                    //���������Ƴ�
                    panelDic.Remove(panelName);
                }
                //��������� ��ô��ֻ��ʧ�� �´�����ʾ��ʱ�� ֱ�Ӹ��ü���
                else
                    panelInfo.panel.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ��ȡ���
    /// </summary>
    /// <typeparam name="T">��������</typeparam>
    public void GetPanel<T>( UnityAction<T> callBack ) where T:BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            //ȡ���ֵ����Ѿ�ռ��λ�õ�����
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            //���ڼ�����
            if(panelInfo.panel == null)
            {
                //������ Ӧ�õȴ����ؽ��� ��ͨ���ص����ݸ��ⲿȥʹ��
                panelInfo.callBack += callBack;
            }
            else if(!panelInfo.isHide)//���ؽ��� ����û������
            {
                callBack?.Invoke(panelInfo.panel);
            }
        }
    }


    /// <summary>
    /// Ϊ�ؼ������Զ����¼�
    /// </summary>
    /// <param name="control">��Ӧ�Ŀؼ�</param>
    /// <param name="type">�¼�������</param>
    /// <param name="callBack">��Ӧ�ĺ���</param>
    public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callBack)
    {
        //�����߼���Ҫ�����ڱ�֤ �ؼ���ֻ�����һ��EventTrigger
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callBack);

        trigger.triggers.Add(entry);
    }
}
