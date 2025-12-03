using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CardViewSystem : Singleton<CardViewSystem>
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Camera cardViewCam;
    [SerializeField] private Transform cardViews;
    
    private List<CardView> _cardViews;
    
    protected override void Awake()
    {
        base.Awake();
        Setup();
    }

    private void Setup()
    {
        if (splineContainer == null) transform.AssignChildVar<SplineContainer>("CardViewCurve", ref splineContainer);
        if (cardViewCam == null) transform.AssignChildVar<Camera>("CardViewCamera", ref cardViewCam);
        if (cardViews == null) transform.AssignChildVar<Transform>("CardViews", ref cardViews);
    }
}
