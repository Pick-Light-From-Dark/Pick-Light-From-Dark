using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���루�����е����ݣ�����
/// </summary>
public class PoolData
{
    //�����洢�����еĶ��� ��¼����û��ʹ�õĶ���
    private Stack<GameObject> dataStack = new Stack<GameObject>();

    //������¼ʹ���еĶ���� 
    private List<GameObject> usedList = new List<GameObject>();

    //�������� ������ͬʱ���ڵĶ�������޸���
    private int maxNum;

    //��������� �������в��ֹ����Ķ���
    private GameObject rootObj;

    //��ȡ�������Ƿ��ж���
    public int Count => dataStack.Count;

    public int UsedCount => usedList.Count;

    /// <summary>
    /// ����ʹ���ж�������������������бȽ� С�ڷ���true ��Ҫʵ����
    /// </summary>
    public bool NeedCreate => usedList.Count < maxNum;

    /// <summary>
    /// ��ʼ�����캯��
    /// </summary>
    /// <param name="root">���ӣ�����أ�������</param>
    /// <param name="name">���븸���������</param>
    public PoolData(GameObject root, string name, GameObject usedObj)
    {
        //��������ʱ �Żᶯ̬���� �������ӹ�ϵ
        if(PoolMgr.isOpenLayout)
        {
            //�������븸����
            rootObj = new GameObject(name);
            //�͹��Ӹ����������ӹ�ϵ
            rootObj.transform.SetParent(root.transform);
        }

        //��������ʱ �ⲿ�϶��ǻᶯ̬����һ�������
        //����Ӧ�ý����¼�� ʹ���еĶ���������
        PushUsedList(usedObj);

        PoolObj poolObj = usedObj.GetComponent<PoolObj>();
        if (poolObj == null)
        {
            Debug.LogError("��Ϊʹ�û���ع��ܵ�Ԥ����������PoolObj�ű� ����������������");
            return;
        }
        //��¼��������ֵ
        maxNum = poolObj.maxNum;
    }

    /// <summary>
    /// �ӳ����е������ݶ���
    /// </summary>
    /// <returns>��Ҫ�Ķ�������</returns>
    public GameObject Pop()
    {
        //ȡ������
        GameObject obj;

        if (Count > 0)
        {
            //��û�е���������ȡ��ʹ��
            obj = dataStack.Pop();
            //����Ҫʹ���� Ӧ��Ҫ��ʹ���е�������¼��
            usedList.Add(obj);
        }
        else
        {
            //ȡ0�����Ķ��� �����ľ���ʹ��ʱ����Ķ���
            obj = usedList[0];
            //���Ұ�����ʹ���ŵĶ������Ƴ�
            usedList.RemoveAt(0);
            //��������Ҫ�ó�ȥ�ã���������Ӧ�ð����ּ�¼�� ʹ���е�������ȥ 
            //�������ӵ�β�� ��ʾ �Ƚ��µĿ�ʼ
            usedList.Add(obj);
        }

        //�������
        obj.SetActive(true);
        //�Ͽ����ӹ�ϵ
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(null);

        return obj;
    }

    /// <summary>
    /// ��������뵽���������
    /// </summary>
    /// <param name="obj"></param>
    public void Push(GameObject obj)
    {
        //ʧ��������Ķ���
        obj.SetActive(false);
        //�����Ӧ����ĸ������� �������ӹ�ϵ
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(rootObj.transform);
        //ͨ��ջ��¼��Ӧ�Ķ�������
        dataStack.Push(obj);
        //��������Ѿ�����ʹ���� Ӧ�ð����Ӽ�¼�������Ƴ�
        usedList.Remove(obj);
    }


    /// <summary>
    /// ������ѹ�뵽ʹ���е������м�¼
    /// </summary>
    /// <param name="obj"></param>
    public void PushUsedList(GameObject obj)
    {
        usedList.Add(obj);
    }
}

/// <summary>
/// �������ֵ䵱������ʽ�滻ԭ�� �洢�������
/// </summary>
public abstract class PoolObjectBase { }

/// <summary>
/// ���ڴ洢 ���ݽṹ�� �� �߼��� �����̳�mono�ģ�������
/// </summary>
/// <typeparam name="T"></typeparam>
public class PoolObject<T> : PoolObjectBase where T:class
{
    public Queue<T> poolObjs = new Queue<T>();
}

/// <summary>
/// ��Ҫ�����õ� ���ݽṹ�ࡢ�߼��� ������Ҫ�̳иýӿ�
/// </summary>
public interface IPoolObject
{
    /// <summary>
    /// �������ݵķ���
    /// </summary>
    void ResetInfo();
}

/// <summary>
/// �����(�����)ģ�� ������
/// </summary>
public class PoolMgr : BaseManager<PoolMgr>
{
    //�������������г��������
    //ֵ ��ʵ�����ľ���һ�� �������
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();

    /// <summary>
    /// ���ڴ洢���ݽṹ�ࡢ�߼������� ���ӵ��ֵ�����
    /// </summary>
    private Dictionary<string, PoolObjectBase> poolObjectDic = new Dictionary<string, PoolObjectBase>();

    //���Ӹ�����
    private GameObject poolObj;

    //�Ƿ������ֹ���
    public static bool isOpenLayout = true;

    private PoolMgr() {

        //���������Ϊ�� �ʹ���
        if (poolObj == null && isOpenLayout)
            poolObj = new GameObject("Pool");

    }

    /// <summary>
    /// �ö����ķ���
    /// </summary>
    /// <param name="name">��������������</param>
    /// <returns>�ӻ������ȡ���Ķ���</returns>
    public GameObject GetObj(string name)
    {
        //���������Ϊ�� �ʹ���
        if (poolObj == null && isOpenLayout)
            poolObj = new GameObject("Pool");

        GameObject obj;

        #region �������������޺���߼��ж�
        if(!poolDic.ContainsKey(name) ||
            (poolDic[name].Count == 0 && poolDic[name].NeedCreate))
        {
            //��̬��������
            //û�е�ʱ�� ͨ����Դ���� ȥʵ������һ��GameObject
            GameObject prefab = Resources.Load<GameObject>(name);
            if (prefab == null)
            {
                Debug.LogError($"[PoolMgr] 未找到资源 {name}");
                return null;
            }
            obj = GameObject.Instantiate(prefab);
            //����ʵ���������Ķ��� Ĭ�ϻ������ֺ����һ��(Clone)
            //�������������� �����������
            obj.name = name;

            //��������
            if(!poolDic.ContainsKey(name))
                poolDic.Add(name, new PoolData(poolObj, name, obj));
            else//ʵ���������Ķ��� ��Ҫ��¼��ʹ���еĶ���������
                poolDic[name].PushUsedList(obj);
        }
        //���������ж��� ���� ʹ���еĶ��������� ֱ��ȥȡ������
        else
        {
            obj = poolDic[name].Pop();
        }

        #endregion


        #region û�м��� ����ʱ���߼�
        ////�г��� ���� ������ �ж��� ��ȥֱ����
        //if (poolDic.ContainsKey(name) && poolDic[name].Count > 0)
        //{
        //    //����ջ�еĶ��� ֱ�ӷ��ظ��ⲿʹ��
        //    obj = poolDic[name].Pop();
        //}
        ////���򣬾�Ӧ��ȥ����
        //else
        //{
        //    //û�е�ʱ�� ͨ����Դ���� ȥʵ������һ��GameObject
        //    obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
        //    //����ʵ���������Ķ��� Ĭ�ϻ������ֺ����һ��(Clone)
        //    //�������������� �����������
        //    obj.name = name;
        //}
        #endregion
        return obj;
    }

    /// <summary>
    /// ��ȡ�Զ�������ݽṹ����߼������ �����̳�Mono�ģ�
    /// </summary>
    /// <typeparam name="T">��������</typeparam>
    /// <returns></returns>
    public T GetObj<T>(string nameSpace = "") where T:class,IPoolObject,new()
    {
        //���ӵ����� �Ǹ������������������ ������������
        string poolName = nameSpace + "_" + typeof(T).Name;
        //�г���
        if(poolObjectDic.ContainsKey(poolName))
        {
            PoolObject<T> pool = poolObjectDic[poolName] as PoolObject<T>;
            //���ӵ����Ƿ��п��Ը��õ�����
            if(pool.poolObjs.Count > 0)
            {
                //�Ӷ�����ȡ������ ���и���
                T obj = pool.poolObjs.Dequeue() as T;
                return obj;
            }
            //���ӵ����ǿյ�
            else
            {
                //���뱣֤�����޲ι��캯��
                T obj = new T();
                return obj;
            }
        }
        else//û�г���
        {
            T obj = new T();
            return obj;
        }
        
    }

    /// <summary>
    /// ��������з������
    /// </summary>
    /// <param name="name">���루���󣩵�����</param>
    /// <param name="obj">ϣ������Ķ���</param>
    public void PushObj(GameObject obj)
    {
        #region ��Ϊʧ�� ���ӹ�ϵ�������� ��������д��� ���Բ���Ҫ�ٴ�����Щ������
        ////��֮��Ŀ�ľ���Ҫ�Ѷ�����������
        ////������ֱ���Ƴ����� ���ǽ�����ʧ�� һ������� �õ�ʱ���ټ�����
        ////�������ַ�ʽ�������԰Ѷ���ŵ���Ļ�⿴�����ĵط�
        //obj.SetActive(false);

        ////��ʧ��Ķ���Ҫ��������еĶ��� ������������Ϊ ���ӣ�����أ�������
        //obj.transform.SetParent(poolObj.transform);
        #endregion

        //û�г��� ��������
        //if (!poolDic.ContainsKey(obj.name))
        //    poolDic.Add(obj.name, new PoolData(poolObj, obj.name));

        //�����뵱�зŶ���
        if (obj == null) return;
        if (!poolDic.ContainsKey(obj.name))
        {
            Debug.LogWarning($"[PoolMgr] 对象 {obj.name} 未在池中，直接销毁");
            GameObject.Destroy(obj);
            return;
        }
        poolDic[obj.name].Push(obj);

        ////������ڶ�Ӧ�ĳ������� ֱ�ӷ�
        //if(poolDic.ContainsKey(name))
        //{
        //    //��ջ�����룩�з������
        //    poolDic[name].Push(obj);
        //}
        ////���� ��Ҫ�ȴ������� �ٷ�
        //else
        //{
        //    //�ȴ�������
        //    poolDic.Add(name, new Stack<GameObject>());
        //    //�������������
        //    poolDic[name].Push(obj);
        //}
    }

    /// <summary>
    /// ���Զ������ݽṹ����߼��� ���������
    /// </summary>
    /// <typeparam name="T">��Ӧ����</typeparam>
    public void PushObj<T>(T obj, string nameSpace = "") where T:class,IPoolObject
    {
        //�����Ҫѹ��null���� �ǲ���������
        if (obj == null)
            return;
        //���ӵ����� �Ǹ������������������ ������������
        string poolName = nameSpace + "_" + typeof(T).Name;
        //�г���
        PoolObject<T> pool;
        if (poolObjectDic.ContainsKey(poolName))
            //ȡ������ ѹ�����
            pool = poolObjectDic[poolName] as PoolObject<T>;
        else//û�г���
        {
            pool = new PoolObject<T>();
            poolObjectDic.Add(poolName, pool);
        }
        //�ڷ��������֮ǰ �����ö��������
        obj.ResetInfo();
        pool.poolObjs.Enqueue(obj);
    }

    /// <summary>
    /// ��������������ӵ��е����� 
    /// ʹ�ó��� ��Ҫ�� �г���ʱ
    /// </summary>
    public void ClearPool()
    {
        poolDic.Clear();
        poolObj = null;
        poolObjectDic.Clear();
    }
}
