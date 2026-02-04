using UnityEngine;

[CreateAssetMenu(fileName="EventButtonData", menuName="Data/EventButtonData")]
public class EventButtonData : ScriptableObject
{
    [SerializeField, TextArea] public string EventButtonText;
}
