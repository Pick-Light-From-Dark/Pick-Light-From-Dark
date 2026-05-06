using System;

[AttributeUsage(AttributeTargets.Class)]
public class UIPathAttribute : Attribute
{
    public string path;

    public UIPathAttribute(string path)
    {
        this.path = path;
    }
}