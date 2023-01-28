using UnityEngine;

public class Condition_True : EDS_DialogueCondition
{
    public int 整数;
    public float valueFloat;
    public string valueString;
    public Object valueObject;

    public override bool Check()
    {
        return true;
    }
}
