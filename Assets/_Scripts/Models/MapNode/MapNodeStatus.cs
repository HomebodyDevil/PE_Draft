using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Timeline;

[Serializable]
public class MapNodeStatus
{
    [field: SerializeField] public AssetReferenceT<TimelineAsset> StartTimeline { get; set; } = null;
    [field: SerializeField] public AssetReferenceT<BattleEnemiesData> BattleEnemiesData { get; set; } = null;
    [field: SerializeField] public AssetReferenceT<EventData> EventData { get; set; } = null;
    
    public MapNodeStatus() { }
    
    public MapNodeStatus(MapNodeData mapNodeData)
    {
        StartTimeline = mapNodeData.StartTimeline;
        BattleEnemiesData = mapNodeData.BattleEnemiesData;
        EventData = mapNodeData.EventData;
    }

    public bool CheckIsValid(out bool timelineCheck, out bool enemiesDataCheck, out bool eventDataCheck)
    {
        timelineCheck = !string.IsNullOrEmpty(StartTimeline.AssetGUID) && StartTimeline.RuntimeKeyIsValid();
        enemiesDataCheck = !string.IsNullOrEmpty(BattleEnemiesData.AssetGUID) && BattleEnemiesData.RuntimeKeyIsValid();
        eventDataCheck = !string.IsNullOrEmpty(EventData.AssetGUID) && EventData.RuntimeKeyIsValid();
        
        Debug.Log($"Check Result\ntimeline : {timelineCheck}\nenemies : {enemiesDataCheck}\nevents : {eventDataCheck}\n");
        
        return  timelineCheck && enemiesDataCheck && eventDataCheck;
    }
}
