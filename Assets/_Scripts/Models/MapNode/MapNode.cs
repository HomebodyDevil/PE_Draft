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
    public List<MapNode> ParentNodes { get; private set; } = new();
    public List<MapNode> ChildNodes { get; private set; } = new();
    public int NodeLevel { get; private set; } = 0;
    public int NodeSlot { get; private set; } = 0;
    public MapNodeStatus MapNodeStatus { get; private set; } = new();
    
    public MapNode(int nodeLevel, int nodeSlot, NodeType nodeType = NodeType.None)
    {
        NodeLevel = nodeLevel;
        NodeSlot = nodeSlot;
        NodeType = nodeType;
    }
    
    public void SetMapNodeData(MapNodeData mapNodeData)
    {
        MapNodeStatus = new(mapNodeData);
    }

    public static void Connect(MapNode parent, MapNode child)
    {
        if (parent == null || child == null) return;
        if (!parent.ChildNodes.Contains(child)) parent.ChildNodes.Add(child);
        if (!child.ParentNodes.Contains(parent)) child.ParentNodes.Add(parent);
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
