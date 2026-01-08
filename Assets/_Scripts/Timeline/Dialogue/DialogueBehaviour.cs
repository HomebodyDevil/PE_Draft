using UnityEngine;
using UnityEngine.Playables;

public class DialogueBehaviour : PlayableBehaviour
{
    public string dialoguePath;
    private bool _done = false;
    
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        if (_done) return;
        
        _done = true;
        
        DialogueService.Instance.EnableDialogue(true, dialoguePath);

        // GetTime : 현재 Playable이 얼마나 진행됐는가(시간)
        // GetDuration : Playable의 (총 재생)시간
        double remainTime = playable.GetDuration() - playable.GetTime();
        
        PlayableDirector director = playable.GetGraph().GetResolver() as PlayableDirector;
        if (director == null)
        {
            Debug.Log("director is null");
            return;
        }
        
        director.time = director.time + remainTime;
        director.Pause();
    }
}
