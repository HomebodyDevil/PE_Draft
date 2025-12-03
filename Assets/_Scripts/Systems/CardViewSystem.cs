using UnityEngine;
using UnityEngine.Splines;

public class CardViewSystem : Singleton<CardViewSystem>
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Camera cardViewCam;

    protected override void Awake()
    {
        base.Awake();
        Setup();
    }

    private void Setup()
    {
        if (splineContainer == null) transform.AssignChildVar<SplineContainer>("CardViewCurve", ref splineContainer);
        if (cardViewCam == null) transform.AssignChildVar<Camera>("CardViewCamera", ref cardViewCam);
    }
}
