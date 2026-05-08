using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ��Դ��Ϣ���� ��Ҫ������ʽ�滻ԭ�� ��������װ�������
/// </summary>
public abstract class ResInfoBase {
    //���ü���
    public int refCount;
}

/// <summary>
/// ��Դ��Ϣ���� ��Ҫ���ڴ洢��Դ��Ϣ �첽����ί����Ϣ �첽���� Э����Ϣ
/// </summary>
/// <typeparam name="T">��Դ����</typeparam>
public class ResInfo<T> : ResInfoBase
{
    //��Դ
    public T asset;
    //��Ҫ�����첽���ؽ����� ������Դ���ⲿ��ί��
    public UnityAction<T> callBack;
    //���ڴ洢�첽����ʱ ������Эͬ����
    public Coroutine coroutine;
    //�������ü���Ϊ0ʱ �Ƿ�������Ҫ�Ƴ�
    public bool isDel;
    

    public void AddRefCount()
    {
        ++refCount;
    }

    public void SubRefCount()
    {
        --refCount;
        if (refCount < 0)
            Debug.LogError("���ü���С��0�ˣ�����ʹ�ú�ж���Ƿ����ִ��");
    }
}


/// <summary>
/// Resources ��Դ����ģ�������
/// </summary>
public class ResMgr : BaseManager<ResMgr>
{
    //���ڴ洢���ع�����Դ���߼����е���Դ������
    private Dictionary<string, ResInfoBase> resDic = new Dictionary<string, ResInfoBase>();

    private ResMgr() { }

    /// <summary>
    /// ͬ������Resources����Դ�ķ���
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> info;
        //�ֵ��в�������Դʱ
        if (!resDic.ContainsKey(resName))
        {
            //ֱ��ͬ������ ���Ҽ�¼��Դ��Ϣ ���ֵ��� �����´�ֱ��ȡ������
            T res = Resources.Load<T>(path);
            info = new ResInfo<T>();
            info.asset = res;
            //���ü�������
            info.AddRefCount();
            resDic.Add(resName, info);
            return res;
        }
        else
        {
            //ȡ���ֵ��еļ�¼
            info = resDic[resName] as ResInfo<T>;
            //���ü�������
            info.AddRefCount();
            //�����첽���� ���ڼ�����
            if (info.asset == null)
            {
                //ֹͣ�첽���� 
                MonoMgr.Instance.StopCoroutine(info.coroutine);
                //ֱ�Ӳ���ͬ���ķ�ʽ���سɹ�
                T res = Resources.Load<T>(path);
                //��¼ 
                info.asset = res;
                //��Ӧ�ð���Щ�ȴ����첽���ؽ�����ί��ȥִ����
                info.callBack?.Invoke(res);
                //�ص����� �첽����Ҳͣ�� ����������õ�����
                info.callBack = null;
                info.coroutine = null;
                // ��ʹ��
                return res;
            }
            else
            {
                //����Ѿ����ؽ��� ֱ����
                return info.asset;
            }
        }
    }

    /// <summary>
    /// �첽������Դ�ķ���
    /// </summary>
    /// <typeparam name="T">��Դ����</typeparam>
    /// <param name="path">��Դ·����Resources�µģ�</param>
    /// <param name="callBack">���ؽ�����Ļص����� ���첽������Դ������Ż����</param>
    public void LoadAsync<T>(string path, UnityAction<T> callBack) where T: UnityEngine.Object
    {
        //��Դ��ΨһID����ͨ�� ·����_��Դ���� ƴ�Ӷ��ɵ�
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> info;
        if (!resDic.ContainsKey(resName))
        {
            //����һ�� ��Դ��Ϣ����
            info = new ResInfo<T>();
            //���ü�������
            info.AddRefCount();
            //����Դ��¼���ӵ��ֵ��У���Դ��û�м��سɹ���
            resDic.Add(resName, info);
            //��¼�����ί�к��� һ������������ ��ʹ��
            info.callBack += callBack;
            //����Э��ȥ���� �첽���� ���Ҽ�¼Эͬ���� ������֮����ܵ� ֹͣ��
            info.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync<T>(path));
        }
        else
        {
            //���ֵ���ȡ����Դ��Ϣ
            info = resDic[resName] as ResInfo<T>;
            //���ü�������
            info.AddRefCount();
            //�����Դ��û�м����� 
            //��ζ�� ���ڽ����첽����
            if (info.asset == null)
                info.callBack += callBack;
            else
                callBack?.Invoke(info.asset);
        }

        //Ҫͨ��Эͬ����ȥ�첽������Դ
        //MonoMgr.Instance.StartCoroutine(ReallyLoadAsync<T>(path, callBack));
    }

    private IEnumerator ReallyLoadAsync<T>(string path) where T : UnityEngine.Object
    {
        //�첽������Դ
        ResourceRequest rq = Resources.LoadAsync<T>(path);
        //�ȴ���Դ���ؽ����� �Ż����ִ��yield return����Ĵ���
        yield return rq;

        string resName = path + "_" + typeof(T).Name;
        //��Դ���ؽ��� ����Դ�����ⲿ��ί�к���ȥ����ʹ��
        if (resDic.ContainsKey(resName))
        {
            ResInfo<T> resInfo = resDic[resName] as ResInfo<T>;
            //ȡ����Դ��Ϣ ���Ҽ�¼������ɵ���Դ
            resInfo.asset = rq.asset as T;

            //���������Ҫɾ�� ��ȥ�Ƴ���Դ
            //���ü���Ϊ0 ������ȥ�Ƴ�
            if (resInfo.refCount == 0)
                UnloadAsset<T>(path, resInfo.isDel, null, false);
            else
            {
                //��������ɵ���Դ���ݳ�ȥ
                resInfo.callBack?.Invoke(resInfo.asset);
                //������Ϻ� ��Щ���þͿ������ �������õ�ռ�� ���ܴ�����Ǳ�ڵ��ڴ�й©����
                resInfo.callBack = null;
                resInfo.coroutine = null;
            }
        }
        
    }

    /// <summary>
    /// �첽������Դ�ķ���
    /// </summary>
    /// <param name="path">��Դ·����Resources�µģ�</param>
    /// <param name="callBack">���ؽ�����Ļص����� ���첽������Դ������Ż����</param>
    [Obsolete("ע�⣺����ʹ�÷��ͼ��ط�ʽ�����ʵ��Ҫ��Type���أ�һ�����ܺͷ��ͼ��ػ���ȥ����ͬ����ͬ����Դ")]
    public void LoadAsync(string path, Type type, UnityAction<UnityEngine.Object> callBack) 
    {
        //��Դ��ΨһID����ͨ�� ·����_��Դ���� ƴ�Ӷ��ɵ�
        string resName = path + "_" + type.Name;
        ResInfo<UnityEngine.Object> info;
        if (!resDic.ContainsKey(resName))
        {
            //����һ�� ��Դ��Ϣ����
            info = new ResInfo<UnityEngine.Object>();
            //���ü�������
            info.AddRefCount();
            //����Դ��¼���ӵ��ֵ��У���Դ��û�м��سɹ���
            resDic.Add(resName, info);
            //��¼�����ί�к��� һ������������ ��ʹ��
            info.callBack += callBack;
            //����Э��ȥ���� �첽���� ���Ҽ�¼Эͬ���� ������֮����ܵ� ֹͣ��
            info.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync(path, type));
        }
        else
        {
            //���ֵ���ȡ����Դ��Ϣ
            info = resDic[resName] as ResInfo<UnityEngine.Object>;
            //���ü�������
            info.AddRefCount();
            //�����Դ��û�м����� 
            //��ζ�� ���ڽ����첽����
            if (info.asset == null)
                info.callBack += callBack;
            else
                callBack?.Invoke(info.asset);
        }
    }

    private IEnumerator ReallyLoadAsync(string path, Type type)
    {
        //�첽������Դ
        ResourceRequest rq = Resources.LoadAsync(path, type);
        //�ȴ���Դ���ؽ����� �Ż����ִ��yield return����Ĵ���
        yield return rq;

        string resName = path + "_" + type.Name;
        //��Դ���ؽ��� ����Դ�����ⲿ��ί�к���ȥ����ʹ��
        if (resDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> resInfo = resDic[resName] as ResInfo<UnityEngine.Object>;
            //ȡ����Դ��Ϣ ���Ҽ�¼������ɵ���Դ
            resInfo.asset = rq.asset;
            //���������Ҫɾ�� ��ȥ�Ƴ���Դ
            //���ü���Ϊ0 ������ȥ�Ƴ�
            if (resInfo.refCount == 0)
                UnloadAsset(path, type, resInfo.isDel, null, false);
            else
            {
                //��������ɵ���Դ���ݳ�ȥ
                resInfo.callBack?.Invoke(resInfo.asset);
                //������Ϻ� ��Щ���þͿ������ �������õ�ռ�� ���ܴ�����Ǳ�ڵ��ڴ�й©����
                resInfo.callBack = null;
                resInfo.coroutine = null;
            }
        }
    }

    /// <summary>
    /// ָ��ж��һ����Դ
    /// </summary>
    /// <param name="assetToUnload"></param>
    public void UnloadAsset<T>(string path, bool isDel = false, UnityAction<T> callBack = null, bool isSub = true)
    {
        string resName = path + "_" + typeof(T).Name;
        //�ж��Ƿ���ڶ�Ӧ��Դ
        if(resDic.ContainsKey(resName))
        {
            ResInfo<T> resInfo = resDic[resName] as ResInfo<T>;
            //���ü���-1
            if(isSub)
                resInfo.SubRefCount();
            //��¼ ���ü���Ϊ0ʱ  �Ƿ������Ƴ���ǩ
            resInfo.isDel = isDel;
            //��Դ�Ѿ����ؽ��� 
            if(resInfo.asset != null && resInfo.refCount == 0 && resInfo.isDel)
            {
                //���ֵ��Ƴ�
                resDic.Remove(resName);
                //ͨ��api ж����Դ
                Resources.UnloadAsset(resInfo.asset as UnityEngine.Object);
            }
            else if(resInfo.asset == null)//��Դ�����첽������
            {
                //MonoMgr.Instance.StopCoroutine(resInfo.coroutine);
                //resDic.Remove(resName);
                //Ϊ�˱������ һ��Ҫ����Դ�Ƴ���
                //�ı��ʾ ��ɾ��
                //resInfo.isDel = true;
                //���첽���ز���ʹ��ʱ ����Ӧ���Ƴ����Ļص���¼ ������ֱ��ȥж����Դ
                if (callBack != null)
                    resInfo.callBack -= callBack;

            }
        }
    }

    public void UnloadAsset(string path, Type type, bool isDel = false, UnityAction<UnityEngine.Object> callBack = null, bool isSub = true)
    {
        string resName = path + "_" + type.Name;
        //�ж��Ƿ���ڶ�Ӧ��Դ
        if (resDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> resInfo = resDic[resName] as ResInfo<UnityEngine.Object>;
            //���ü���-1
            if(isSub)
                resInfo.SubRefCount();
            //��¼ ���ü���Ϊ0ʱ  �Ƿ������Ƴ���ǩ
            resInfo.isDel = isDel;
            //��Դ�Ѿ����ؽ��� 
            if (resInfo.asset != null && resInfo.refCount == 0 && resInfo.isDel)
            {
                //���ֵ��Ƴ�
                resDic.Remove(resName);
                //ͨ��api ж����Դ
                Resources.UnloadAsset(resInfo.asset);
            }
            else if (resInfo.asset == null)//��Դ�����첽������
            {
                //MonoMgr.Instance.StopCoroutine(resInfo.coroutine);
                //resDic.Remove(resName);
                //Ϊ�˱������ һ��Ҫ����Դ�Ƴ���
                //�ı��ʾ ��ɾ��
                //resInfo.isDel = true;
                //���첽���ز���ʹ��ʱ ����Ӧ���Ƴ����Ļص���¼ ������ֱ��ȥж����Դ
                if (callBack != null)
                    resInfo.callBack -= callBack;
            }
        }
    }

    /// <summary>
    /// �첽ж�ض�Ӧû��ʹ�õ�Resources��ص���Դ
    /// </summary>
    /// <param name="callBack">�ص�����</param>
    public void UnloadUnusedAssets(UnityAction callBack)
    {
        MonoMgr.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callBack));
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callBack)
    {
        //�����������Ƴ���ʹ�õ���Դ֮ǰ Ӧ�ð������Լ���¼����Щ���ü���Ϊ0 ����û�б��Ƴ���¼����Դ
        //�Ƴ���
        List<string> list = new List<string>();
        foreach (string path in resDic.Keys)
        {
            if (resDic[path].refCount == 0)
                list.Add(path);
        }
        foreach (string path in list)
        {
            resDic.Remove(path);
        }

        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        //ж����Ϻ� ֪ͨ�ⲿ
        callBack?.Invoke();
    }

    /// <summary>
    /// ��ȡ��ǰĳ����Դ�����ü���
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public int GetRefCount<T>(string path)
    {
        string resName = path + "_" + typeof(T).Name;
        if(resDic.ContainsKey(resName))
        {
            return (resDic[resName] as ResInfo<T>).refCount;
        }
        return 0;
    }


    /// <summary>
    /// ����ֵ�
    /// </summary>
    /// <param name="callBack"></param>
    public void ClearDic(UnityAction callBack)
    {
        MonoMgr.Instance.StartCoroutine(ReallyClearDic(callBack));
    }

    private IEnumerator ReallyClearDic(UnityAction callBack)
    {
        resDic.Clear();
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        //ж����Ϻ� ֪ͨ�ⲿ
        callBack?.Invoke();
    }
}
