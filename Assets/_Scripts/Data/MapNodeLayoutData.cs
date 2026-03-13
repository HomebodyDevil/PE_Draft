using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapNodeLayoutData", menuName = "Data/MapNodeLayoutData")]
public class MapNodeLayoutData : ScriptableObject
{
    [Serializable]
    public class NodeEntry
    {
        [Tooltip("몇 번째 레벨인지(0-based)")]
        public int level;
        
        [Tooltip("해당 레벨에서 몇 번째 슬롯인지(0-based")]
        public int slot;
        
        [Tooltip("노드의 타입")]
        public NodeType nodeType;

        [Tooltip("부모 노드의 (level, slot)쌍. 비워져있다면, 연결 없음")]
        public List<Vector2Int> parentEntries = new();
    }
    
    [Tooltip("레이아웃에 포함될 모든 노드들")]
    public List<NodeEntry> nodeEntries = new();
    
    [Tooltip("총 Level 수")]
    public int totalLevels = 15;
    
    [Tooltip("레벨당 최대 노드 수")]
    public int maxNodesPerLevel = 5;
}
