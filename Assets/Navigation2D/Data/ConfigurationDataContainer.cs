using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Navigation2D/Create config")]
public class ConfigurationDataContainer : ScriptableObject
{
    public List<string> agentNames;
    public List<string> areaNames;
    public List<float> agentRadius;
}
