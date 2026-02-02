using System;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour
{
    public MapNode MapNode { get; private set; }

    private Image _mapNodeImage;
    
    public MapNodeView(MapNode mapNode)
    {
        MapNode = mapNode;
    }

    public void SetMapNode(MapNode mapNode)
    {
        MapNode = mapNode;
    }

    private void Awake()
    {
        if (!_mapNodeImage) transform.AssignChildVar<Image>("MapNodeImage", ref _mapNodeImage);
    }

    private void Start()
    {
        Debug.Log("Test : MapNodeView 이미지 설정.");
        if (_mapNodeImage)
        {
            Color color = MapNode.NodeType switch
            {
                NodeType.Battle => Color.red,
                NodeType.None => Color.black,
                NodeType.Elite => Color.blue,
                NodeType.Rest => Color.yellow,
                _ => Color.white,
            };
            
            _mapNodeImage.color = color;
        }
    }

    public void OnClick()
    {
        Debug.Log("현재 MapNodeView에선, Click시, BattleScene으로만 전환.");
        SceneService.Instance.ChangeScene(SceneType.BattleScene);
        if (MapNode == null)
        {
            Debug.Log("MapNodeView: MapNode is null");
            return;
        }

        Debug.Log($"MapNode Type: {MapNode.NodeType.ToString()}");
        Debug.Log("PlayerStatusService의 MapNodeStatus Set.");
        PlayerStatusService.Instance.CurrentMapNodeStatus = MapNode.MapNodeStatus;
    }
}
