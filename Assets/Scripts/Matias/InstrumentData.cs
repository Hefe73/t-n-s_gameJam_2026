using UnityEngine;

[CreateAssetMenu(fileName = "InstrumentData", menuName = "Scriptable Objects/InstrumentData")]
public class InstrumentData : ScriptableObject
{
    public string instrumentName;
    public Sprite icon;
}
