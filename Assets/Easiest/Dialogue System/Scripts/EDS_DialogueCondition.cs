using System;
using System.Collections.Generic;

public abstract class EDS_DialogueCondition : UnityEngine.Object
{
    public abstract bool Check();

    public static List<string> GetAllKeys()
    {
        return new List<string>(conditionInfos.Keys);
    }

    public static Dictionary<string, Type> conditionInfos = new()
    {
        ["永远达成条件"] = typeof(Condition_True),
        ["如果大于"] = typeof(Condition_IfBigger),
        ["如果所有选项都选过"] = typeof(Condition_IfAllChoicesSelect),
    };
}
