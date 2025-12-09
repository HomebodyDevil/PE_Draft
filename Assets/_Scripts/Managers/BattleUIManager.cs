using UnityEngine;

public class BattleUIManager : Singleton<BattleUIManager>
{
    [SerializeField] private Canvas _buttonsUiCanvas;
    
    protected override void Awake()
    {
        base.Awake();

        VarSetup();
        Setup();
    }

    private void VarSetup()
    {
        if (_buttonsUiCanvas == null) transform.AssignChildVar<Canvas>("ButtonsUICanvas", ref _buttonsUiCanvas);
    }

    private void Setup()
    {
        if (_buttonsUiCanvas != null)
        {
            _buttonsUiCanvas.sortingOrder = ConstValue.UI_ORDER;
        }
    }
}
