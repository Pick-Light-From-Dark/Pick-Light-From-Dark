using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RoleConfig", menuName = "Dialogue/RoleConfig")]
public class RoleConfig : ScriptableObject
{
    [System.Serializable]
    public class RoleData
    {
        public string name;
        public Sprite sprite;
    }

    public List<RoleData> roles;

    private Dictionary<string, Sprite> roleDict;

    public void Init()
    {
        roleDict = new Dictionary<string, Sprite>();
        foreach (var r in roles)
        {
            if (!roleDict.ContainsKey(r.name))
                roleDict.Add(r.name, r.sprite);
        }
       
    }

    public Sprite GetRoleSprite(string roleName)
    {
        if (roleDict.ContainsKey(roleName))
        {
            return roleDict[roleName];  // 返回找到的角色图
        }
        else
        {
            Debug.LogWarning($"角色立绘未找到：{roleName}");  // 输出未找到的角色名称
            return null;  // 如果找不到，返回 null
        }
    }
}