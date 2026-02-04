using System;
using System.Collections.Generic;
using SerializeReferenceEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum NodeType
{
    None,
    Rest,
    Battle,
    Elite,
    Event,
}

public class MapNode
{
    public NodeType NodeType { get; private set; }
    public List<MapNode> ParentNode { get; private set; } = new();
    public List<MapNode> ChildNode { get; private set; } = new();
    public int NodeLevel { get; private set; } = 0;
    public MapNodeStatus MapNodeStatus { get; private set; } = new();
    
    public MapNode(int nodeLevel, NodeType nodeType = NodeType.None)
    {
        NodeLevel = nodeLevel;
        NodeType = nodeType;
    }
    
    public void SetMapNodeData(MapNodeData mapNodeData)
    {
        MapNodeStatus = new(mapNodeData);
        // if (!MapNodeStatus.CheckIsValid(out var time, out var enemies, out var events))
        // {
        //     if (!time) Debug.LogError("Timeline is Invalid");
        //     if (!enemies) Debug.LogError("Enemies is Invalid");
        //     if (!events) Debug.LogError("Events is Invalid");
        // }
        // else
        //     Debug.Log("Success to Set Map Node Status");
    }

    // public void SetMapNodeDataByNodeType()
    // {
    //     MapNodeStatus ??= new();
    // }
    
    // 보류
    // public void AddNode(bool isChildNode, MapNode node)
    // {
    //     if (node == null)
    //     {
    //         Debug.Log("Node is null");
    //         return;
    //     }
    //     
    //     var list = isChildNode ? ChildNode : ParentNode;
    //     list.Add(node);
    // }
    //
    // public void RemoveNode(bool isChildNode, MapNode node)
    // {
    //     if (node == null)
    //     {
    //         Debug.Log("Node is null");
    //         return;
    //     }
    //     
    //     var list = isChildNode ? ChildNode : ParentNode;
    //     list.Remove(node);
    // }
}
