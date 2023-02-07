using System;
using System.Collections.Generic;

public abstract class EDS_DialogueEvent : UnityEngine.Object
{
    public abstract object Execute();

    public static List<string> GetAllKeys()
    {
        return new List<string>(eventInfos.Keys);
    }

    public static Dictionary<string, Type> eventInfos = new()
    {
        ["掷骰子"] = typeof(Event_Dice),
    };
}
