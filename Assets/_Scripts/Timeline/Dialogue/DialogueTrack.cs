using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(DialogueClip))]
public class DialogueTrack : PlayableTrack
{
    public Type BindingType = typeof(DialogueSystem);
    
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<DialogueMixer>.Create(graph, inputCount);
    }
}
