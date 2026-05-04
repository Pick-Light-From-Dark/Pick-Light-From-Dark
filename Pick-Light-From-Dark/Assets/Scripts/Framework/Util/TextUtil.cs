using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ���ڴ����ַ�����һЩ�������ܵ�
/// </summary>
public class TextUtil
{
    private static StringBuilder resultStr = new StringBuilder("");

    #region �ַ���������
    /// <summary>
    /// ����ַ��� �����ַ�������
    /// </summary>
    /// <param name="str">��Ҫ����ֵ��ַ���</param>
    /// <param name="type">����ַ����ͣ� 1-; 2-, 3-% 4-: 5-�ո� 6-| 7-_ </param>
    /// <returns></returns>
    public static string[] SplitStr(string str, int type = 1)
    {
        if (str == "")
            return new string[0];
        string newStr = str;
        if (type == 1)
        {
            //Ϊ�˱���Ӣ�ķ�����������ķ��� �����Ƚ���һ���滻
            while (newStr.IndexOf("��") != -1)
                newStr = newStr.Replace("��", ";");
            return newStr.Split(';');
        }
        else if (type == 2)
        {
            //Ϊ�˱���Ӣ�ķ�����������ķ��� �����Ƚ���һ���滻
            while (newStr.IndexOf("��") != -1)
                newStr = newStr.Replace("��", ",");
            return newStr.Split(',');
        }
        else if (type == 3)
        {
            return newStr.Split('%');
        }
        else if (type == 4)
        {
            //Ϊ�˱���Ӣ�ķ�����������ķ��� �����Ƚ���һ���滻
            while (newStr.IndexOf("��") != -1)
                newStr = newStr.Replace("��", ":");
            return newStr.Split(':');
        }
        else if (type == 5)
        {
            return newStr.Split(' ');
        }
        else if (type == 6)
        {
            return newStr.Split('|');
        }
        else if (type == 7)
        {
            return newStr.Split('_');
        }

        return new string[0];
    }

    /// <summary>
    /// ����ַ��� ������������
    /// </summary>
    /// <param name="str">��Ҫ����ֵ��ַ���</param>
    /// <param name="type">����ַ����ͣ� 1-; 2-, 3-% 4-: 5-�ո� 6-| 7-_ </param>
    /// <returns></returns>
    public static int[] SplitStrToIntArr(string str, int type = 1)
    {
        //�õ���ֺ���ַ�������
        string[] strs = SplitStr(str, type);
        if (strs.Length == 0)
            return new int[0];
        //���ַ������� ת���� int���� 
        return Array.ConvertAll<string, int>(strs, (str) =>
        {
            return int.Parse(str);
        });
    }

    /// <summary>
    /// ר��������ֶ����ֵ����ʽ�����ݵ� ��int����
    /// </summary>
    /// <param name="str">����ֵ��ַ���</param>
    /// <param name="typeOne">���ָ���  1-; 2-, 3-% 4-: 5-�ո� 6-| 7-_ </param>
    /// <param name="typeTwo">��ֵ�Էָ��� 1-; 2-, 3-% 4-: 5-�ո� 6-| 7-_ </param>
    /// <param name="callBack">�ص�����</param>
    public static void SplitStrToIntArrTwice(string str, int typeOne, int typeTwo, UnityAction<int, int> callBack)
    {
        string[] strs = SplitStr(str, typeOne);
        if (strs.Length == 0)
            return;
        int[] ints;
        for (int i = 0; i < strs.Length; i++)
        {
            //��ֵ������ߵ�ID��������Ϣ
            ints = SplitStrToIntArr(strs[i], typeTwo);
            if (ints.Length < 2)
                continue;
            callBack?.Invoke(ints[0], ints[1]);
        }
    }

    /// <summary>
    /// ר��������ֶ����ֵ����ʽ�����ݵ� ��string����
    /// </summary>
    /// <param name="str">����ֵ��ַ���</param>
    /// <param name="typeOne">���ָ��� 1-; 2-, 3-% 4-: 5-�ո� 6-| 7-_ </param>
    /// <param name="typeTwo">��ֵ�Էָ���  1-; 2-, 3-% 4-: 5-�ո� 6-| 7-_ </param>
    /// <param name="callBack">�ص�����</param>
    public static void SplitStrTwice(string str, int typeOne, int typeTwo, UnityAction<string, string> callBack)
    {
        string[] strs = SplitStr(str, typeOne);
        if (strs.Length == 0)
            return;
        string[] strs2;
        for (int i = 0; i < strs.Length; i++)
        {
            //��ֵ������ߵ�ID��������Ϣ
            strs2 = SplitStr(strs[i], typeTwo);
            if (strs2.Length == 0)
                continue;
            callBack.Invoke(strs2[0], strs2[1]);
        }
    }


    #endregion

    #region ����ת�ַ������
    /// <summary>
    /// �õ�ָ�����ȵ�����ת�ַ������ݣ�������Ȳ�������ǰ�油0��������ȳ������ᱣ��ԭʼ��ֵ
    /// </summary>
    /// <param name="value">��ֵ</param>
    /// <param name="len">����</param>
    /// <returns></returns>
    public static string GetNumStr(int value, int len)
    {
        //tostring�д���һ�� Dn ���ַ���
        //������Ҫ������ת��Ϊ����λn���ַ���
        //������Ȳ��� ����ǰ�油0
        return value.ToString($"D{len}");
    }
    /// <summary>
    /// ��ָ������������С�����nλ
    /// </summary>
    /// <param name="value">����ĸ�����</param>
    /// <param name="len">����С�����nλ</param>
    /// <returns></returns>
    public static string GetDecimalStr(float value, int len)
    {
        //tostring�д���һ�� Fn ���ַ���
        //������Ҫ����С�����λС��
        return value.ToString($"F{len}");
    }

    /// <summary>
    /// ���ϴ�ϳ����� ת��Ϊ�ַ���
    /// </summary>
    /// <param name="num">������ֵ</param>
    /// <returns>n��nǧ�� �� n��nǧ �� 1000 3434 234</returns>
    public static string GetBigDataToString(int num)
    {
        //�������1�� ��ô����ʾ n��nǧ��
        if (num >= 100000000)
        {
            return BigDataChange(num, 100000000, "��", "ǧ��");
        }
        //�������1�� ��ô����ʾ n��nǧ
        else if (num >= 10000)
        {
            return BigDataChange(num, 10000, "��", "ǧ");
        }
        //�������� ��ֱ����ʾ��ֵ����
        else
            return num.ToString();
    }

    /// <summary>
    /// �Ѵ�����ת���ɶ�Ӧ���ַ���ƴ��
    /// </summary>
    /// <param name="num">��ֵ</param>
    /// <param name="company">�ָλ ������ 100000000��10000</param>
    /// <param name="bigCompany">��λ �ڡ���</param>
    /// <param name="littltCompany">С��λ ��ǧ</param>
    /// <returns></returns>
    private static string BigDataChange(int num, int company, string bigCompany, string littltCompany)
    {
        resultStr.Clear();
        //�м��ڡ�����
        resultStr.Append(num / company);
        resultStr.Append(bigCompany);
        //�м�ǧ�򡢼�ǧ
        int tmpNum = num % company;
        //���м�ǧ�򡢼�ǧ
        tmpNum /= (company / 10);
        //�������Ϊ0
        if(tmpNum != 0)
        {
            resultStr.Append(tmpNum);
            resultStr.Append(littltCompany);
        }
        return resultStr.ToString();
    }

    #endregion

    #region ʱ��ת�����
    /// <summary>
    /// ��תʱ�����ʽ ����ʱ��������Լ���
    /// </summary>
    /// <param name="s">����</param>
    /// <param name="egZero">�Ƿ����0</param>
    /// <param name="isKeepLen">�Ƿ�������2λ</param>
    /// <param name="hourStr">Сʱ��ƴ���ַ�</param>
    /// <param name="minuteStr">���ӵ�ƴ���ַ�</param>
    /// <param name="secondStr">���ƴ���ַ�</param>
    /// <returns></returns>
    public static string SecondToHMS(int s, bool egZero = false, bool isKeepLen = false, string hourStr = "ʱ", string minuteStr = "��", string secondStr = "��")
    {
        //ʱ�䲻���и��� ����������������Ǹ���ֱ�ӹ�0
        if (s < 0)
            s = 0;
        //����Сʱ
        int hour = s / 3600;
        //�������
        //��ȥСʱ���ʣ����
        int second = s % 3600;
        //ʣ����תΪ������
        int minute = second / 60;
        //������
        second = s % 60;
        //ƴ��
        resultStr.Clear();
        //���Сʱ��Ϊ0 ���� ������0 
        if (hour != 0 || !egZero)
        {
            resultStr.Append(isKeepLen?GetNumStr(hour, 2):hour);//���弸��Сʱ
            resultStr.Append(hourStr);
        }
        //������Ӳ�Ϊ0 ���� ������0 ���� Сʱ��Ϊ0
        if(minute != 0 || !egZero || hour != 0)
        {
            resultStr.Append(isKeepLen?GetNumStr(minute,2): minute);//���弸����
            resultStr.Append(minuteStr);
        }
        //����벻Ϊ0 ���� ������0 ���� Сʱ�ͷ��Ӳ�Ϊ0
        if(second != 0 || !egZero || hour != 0 || minute != 0)
        {
            resultStr.Append(isKeepLen?GetNumStr(second,2): second);//���������
            resultStr.Append(secondStr);
        }

        //�������Ĳ�����0��ʱ
        if(resultStr.Length == 0)
        {
            resultStr.Append(0);
            resultStr.Append(secondStr);
        }

        return resultStr.ToString();
    }
    
    /// <summary>
    /// ��ת00:00:00��ʽ
    /// </summary>
    /// <param name="s"></param>
    /// <param name="egZero"></param>
    /// <returns></returns>
    public static string SecondToHMS2(int s, bool egZero = false)
    {
        return SecondToHMS(s, egZero, true, ":", ":", "");
    }
    #endregion

}
