using System;
using System.Collections.Generic;

public abstract class EDS_DialogueEvent : UnityEngine.Object
{
    public abstract object Execute();

    public static Dictionary<string, Type> eventInfos = new()
    {
        ["无"] = null,
    };

    public static List<string> GetAllKeys()
    {
        return new List<string>(eventInfos.Keys);
    }
}
