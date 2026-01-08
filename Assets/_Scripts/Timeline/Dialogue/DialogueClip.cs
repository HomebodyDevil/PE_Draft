using UnityEngine;
using UnityEngine.Playables;

public class DialogueClip : PlayableAsset
{
    public string dialoguePath;
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.dialoguePath = dialoguePath;
        
        return playable;
    }
}
