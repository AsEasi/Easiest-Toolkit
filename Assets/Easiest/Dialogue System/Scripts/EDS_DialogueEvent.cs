using System.Collections.Generic;
using System;

public abstract class EDS_DialogueEvent
{
    public abstract object Execute();

    public static Dictionary<string, Type> eventInfos
         = new()
         {
             ["нч"] = null,
         };

    public static List<string> GetAllKeys()
    {
        return new List<string>(eventInfos.Keys);
    }
}
