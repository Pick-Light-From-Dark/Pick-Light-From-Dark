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

    public Sprite GetRoleSprite(string name)
    {
        if (roleDict == null) Init();

        return roleDict.ContainsKey(name) ? roleDict[name] : null;
    }
}