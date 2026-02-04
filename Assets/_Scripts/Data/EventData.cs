using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName="EventData", menuName="Data/EventData")]
public class EventData : ScriptableObject
{
    [SerializeField, TextArea] public string EventText;
    [SerializeField] public List<AssetReferenceT<EventButtonData>> EventButtonDatas;
    [SerializeField] public AssetReferenceT<Sprite> EventSprite;
}
