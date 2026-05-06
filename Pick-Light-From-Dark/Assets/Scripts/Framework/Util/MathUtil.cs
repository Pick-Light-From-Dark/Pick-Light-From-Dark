using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MathUtil
{
    #region �ǶȺͻ���
    /// <summary>
    /// �Ƕ�ת���ȵķ���
    /// </summary>
    /// <param name="deg">�Ƕ�ֵ</param>
    /// <returns>����ֵ</returns>
    public static float Deg2Rad(float deg)
    {
        return deg * Mathf.Deg2Rad;
    }

    /// <summary>
    /// ����ת�Ƕȵķ���
    /// </summary>
    /// <param name="rad">����ֵ</param>
    /// <returns>�Ƕ�ֵ</returns>
    public static float Rad2Deg(float rad)
    {
        return rad * Mathf.Rad2Deg;
    }
    #endregion

    #region ���������ص�
    /// <summary>
    /// ��ȡXZƽ���� ����ľ���
    /// </summary>
    /// <param name="srcPos">��1</param>
    /// <param name="targetPos">��2</param>
    /// <returns></returns>
    public static float GetObjDistanceXZ(Vector3 srcPos, Vector3 targetPos)
    {
        srcPos.y = 0;
        targetPos.y = 0;
        return Vector3.Distance(srcPos, targetPos);
    }

    /// <summary>
    /// �ж�����֮����� �Ƿ�С�ڵ���Ŀ����� XZƽ��
    /// </summary>
    /// <param name="srcPos">��1</param>
    /// <param name="targetPos">��2</param>
    /// <param name="dis">����</param>
    /// <returns></returns>
    public static bool CheckObjDistanceXZ(Vector3 srcPos, Vector3 targetPos, float dis)
    {
        return GetObjDistanceXZ(srcPos, targetPos) <= dis;
    }

    /// <summary>
    /// ��ȡXYƽ���� ����ľ���
    /// </summary>
    /// <param name="srcPos">��1</param>
    /// <param name="targetPos">��2</param>
    /// <returns></returns>
    public static float GetObjDistanceXY(Vector3 srcPos, Vector3 targetPos)
    {
        srcPos.z = 0;
        targetPos.z = 0;
        return Vector3.Distance(srcPos, targetPos);
    }

    /// <summary>
    /// �ж�����֮����� �Ƿ�С�ڵ���Ŀ����� XYƽ��
    /// </summary>
    /// <param name="srcPos">��1</param>
    /// <param name="targetPos">��2</param>
    /// <param name="dis">����</param>
    /// <returns></returns>
    public static bool CheckObjDistanceXY(Vector3 srcPos, Vector3 targetPos, float dis)
    {
        return GetObjDistanceXY(srcPos, targetPos) <= dis;
    }

    #endregion

    #region λ���ж����
    /// <summary>
    /// �ж���������ϵ�µ�ĳһ���� �Ƿ�����Ļ�ɼ���Χ��
    /// </summary>
    /// <param name="pos">��������ϵ�µ�һ�����λ��</param>
    /// <returns>����ڿɼ���Χ�ⷵ��true�����򷵻�false</returns>
    public static bool IsWorldPosOutScreen(Vector3 pos)
    {
        //����������תΪ��Ļ����
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        //�ж��Ƿ�����Ļ��Χ��
        if (screenPos.x >= 0 && screenPos.x <= Screen.width &&
            screenPos.y >= 0 && screenPos.y <= Screen.height)
            return false;
        return true;
    }

    /// <summary>
    /// �ж�ĳһ��λ�� �Ƿ���ָ�����η�Χ�ڣ�ע�⣺��������������������ǻ���ͬһ������ϵ�µģ�
    /// </summary>
    /// <param name="pos">�������ĵ�λ��</param>
    /// <param name="forward">�Լ����泯��</param>
    /// <param name="targetPos">Ŀ�����</param>
    /// <param name="radius">�뾶</param>
    /// <param name="angle">���εĽǶ�</param>
    /// <returns></returns>
    public static bool IsInSectorRangeXZ(Vector3 pos, Vector3 forward, Vector3 targetPos, float radius, float angle)
    {
        pos.y = 0;
        forward.y = 0;
        targetPos.y = 0;
        //���� + �Ƕ�
        return Vector3.Distance(pos, targetPos) <= radius && Vector3.Angle(forward, targetPos - pos) <= angle / 2f;
    }
    #endregion

    #region ���߼�����

    /// <summary>
    /// ���߼�� ��ȡһ������ ָ������ ָ���㼶��
    /// </summary>
    /// <param name="ray">����</param>
    /// <param name="callBack">�ص����������������RayCastHit��Ϣ���ݳ�ȥ��</param>
    /// <param name="maxDistance">������</param>
    /// <param name="layerMask">�㼶ɸѡ</param>
    public static void RayCast(Ray ray, UnityAction<RaycastHit> callBack, float maxDistance, int layerMask)
    {
        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
            callBack?.Invoke(hitInfo);
    }

    /// <summary>
    /// ���߼�� ��ȡһ������ ָ������ ָ���㼶��
    /// </summary>
    /// <param name="ray">����</param>
    /// <param name="callBack">�ص����������������GameObject��Ϣ���ݳ�ȥ��</param>
    /// <param name="maxDistance">������</param>
    /// <param name="layerMask">�㼶ɸѡ</param>
    public static void RayCast(Ray ray, UnityAction<GameObject> callBack, float maxDistance, int layerMask)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
            callBack?.Invoke(hitInfo.collider.gameObject);
    }

    /// <summary>
    /// ���߼�� ��ȡһ������ ָ������ ָ���㼶��
    /// </summary>
    /// <param name="ray">����</param>
    /// <param name="callBack">�ص���������������Ķ�����Ϣ�Ϲ��ڵ�ָ���ű����ݳ�ȥ��</param>
    /// <param name="maxDistance">������</param>
    /// <param name="layerMask">�㼶ɸѡ</param>
    public static void RayCast<T>(Ray ray, UnityAction<T> callBack, float maxDistance, int layerMask)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
            callBack?.Invoke(hitInfo.collider.gameObject.GetComponent<T>());
    }

    /// <summary>
    /// ���߼�� ��ȡ��������� ָ������ ָ���㼶
    /// </summary>
    /// <param name="ray">����</param>
    /// <param name="callBack">�ص����������������RayCastHit��Ϣ���ݳ�ȥ�� ÿһ�����󶼻����һ��</param>
    /// <param name="maxDistance">������</param>
    /// <param name="layerMask">�㼶ɸѡ</param>
    public static void RayCastAll(Ray ray, UnityAction<RaycastHit> callBack, float maxDistance, int layerMask)
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        for (int i = 0; i < hitInfos.Length; i++)
            callBack?.Invoke(hitInfos[i]);
    }

    /// <summary>
    /// ���߼�� ��ȡ��������� ָ������ ָ���㼶
    /// </summary>
    /// <param name="ray">����</param>
    /// <param name="callBack">�ص����������������GameObject��Ϣ���ݳ�ȥ�� ÿһ�����󶼻����һ��</param>
    /// <param name="maxDistance">������</param>
    /// <param name="layerMask">�㼶ɸѡ</param>
    public static void RayCastAll(Ray ray, UnityAction<GameObject> callBack, float maxDistance, int layerMask)
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        for (int i = 0; i < hitInfos.Length; i++)
            callBack?.Invoke(hitInfos[i].collider.gameObject);
    }

    /// <summary>
    /// ���߼�� ��ȡ��������� ָ������ ָ���㼶
    /// </summary>
    /// <param name="ray">����</param>
    /// <param name="callBack">�ص���������������Ķ�����Ϣ�������Ľű����ݳ�ȥ�� ÿһ�����󶼻����һ��</param>
    /// <param name="maxDistance">������</param>
    /// <param name="layerMask">�㼶ɸѡ</param>
    public static void RayCastAll<T>(Ray ray, UnityAction<T> callBack, float maxDistance, int layerMask)
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        for (int i = 0; i < hitInfos.Length; i++)
            callBack?.Invoke(hitInfos[i].collider.gameObject.GetComponent<T>());
    }
    #endregion

    #region ��Χ������
    /// <summary>
    /// ���к�װ��Χ���
    /// </summary>
    /// <typeparam name="T">��Ҫ��ȡ����Ϣ���� ������д Collider GameObject �Լ��������������������</typeparam>
    /// <param name="center">��װ���ĵ�</param>
    /// <param name="rotation">���ӵĽǶ�</param>
    /// <param name="halfExtents">�����ߵ�һ��</param>
    /// <param name="layerMask">�㼶ɸѡ</param>
    /// <param name="callBack">�ص����� </param>
    public static void OverlapBox<T>(Vector3 center, Quaternion rotation, Vector3 halfExtents, int layerMask, UnityAction<T> callBack) where T : class
    {
        Type type = typeof(T);
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, rotation, layerMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (type == typeof(Collider))
                callBack?.Invoke(colliders[i] as T);
            else if (type == typeof(GameObject))
                callBack?.Invoke(colliders[i].gameObject as T);
            else
                callBack?.Invoke(colliders[i].gameObject.GetComponent<T>());
        }
    }

    /// <summary>
    /// �������巶Χ���
    /// </summary>
    /// <typeparam name="T">��Ҫ��ȡ����Ϣ���� ������д Collider GameObject �Լ��������������������</typeparam>
    /// <param name="center">��������ĵ�</param>
    /// <param name="radius">����İ뾶</param>
    /// <param name="layerMask">�㼶ɸѡ</param>
    /// <param name="callBack">�ص�����</param>
    public static void OverlapSphere<T>(Vector3 center, float radius, int layerMask, UnityAction<T> callBack) where T:class
    {
        Type type = typeof(T);
        Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (type == typeof(Collider))
                callBack?.Invoke(colliders[i] as T);
            else if (type == typeof(GameObject))
                callBack?.Invoke(colliders[i].gameObject as T);
            else
                callBack?.Invoke(colliders[i].gameObject.GetComponent<T>());
        }
    }
    #endregion
}
