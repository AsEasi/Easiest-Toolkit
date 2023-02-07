using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ETK/WindowSetting", fileName = "New EDS_Window Setting")]
public class EDS_WindowSetting : ScriptableObject
{
    // Node color

    [Header("Node Color")]
    public Color normalNodeColor = new Color();
    public Color staightNodeColor;
    public Color branchNodeColor;
    public Color eventsNodeColor;
}
