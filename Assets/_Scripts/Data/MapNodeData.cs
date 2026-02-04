using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName="MapNodeData", menuName="Data/MapNodeData")]
public class MapNodeData : ScriptableObject
{
    [field: SerializeField] public AssetReferenceT<TimelineAsset> StartTimeline { get; set; }
    [field: SerializeField] public AssetReferenceT<BattleEnemiesData> BattleEnemiesData { get; set; }
    [field: SerializeField] public AssetReferenceT<EventData> EventData { get; set; }
    // (전투)배경화면도 지정할 수 있도록 나중에 추가해주자.
}
