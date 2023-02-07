using UnityEngine;

public class Condition_True : EDS_DialogueCondition
{
    public int 整数;
    public float valueFloat;
    public string valueString;
    public bool 布尔;
    public Object valueObject;

    public override bool Check()
    {
        return true;
    }
}
