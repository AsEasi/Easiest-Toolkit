using System.Collections.Generic;
using System;

public abstract class EDS_DialogueCondition : UnityEngine.Object
{
    public abstract bool Check();
    
    public static Dictionary<string, Type> conditionInfos = new()
         {
             ["永远达成条件"] = typeof(Condition_True),
         };

    public static List<string> GetAllKeys()
    {
        return new List<String>(conditionInfos.Keys);
    }
}
