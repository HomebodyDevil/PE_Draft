using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestGAContext
{
    public string Text { get; private set; }
    public Transform TextObject { get; private set; }

    public TestGAContext(string text, Transform textObject)
    {
        Text = text;
        TextObject = textObject;
    }
}

public class TestGA : GameAbility
{
    public TestGAContext TestGaContext { get; private set; }

    public TestGA() { }
    
    public TestGA(TestGAContext ctx)
    {
        TestGaContext = ctx;
    }
}

public class DebugHelperService : PersistantSingleton<DebugHelperService>
{
    private TextMeshProUGUI _text;

    protected override void Awake()
    {
        base.Awake();
        
        if (_text == null) transform.AssignChildVar<TextMeshProUGUI>("Text", ref _text);
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneLoaded;
    }

    private void Start()
    {
        AddTestGAPerformer();
    }

    private void OnSceneLoaded(Scene oldScene, Scene newScene)
    {
        if (newScene.name.EndsWith("BattleScene"))
        {
            Debug.Log("BattleScene Loaded");
            AddTestGAPerformer();
        }
    }

    private void AddTestGAPerformer()
    {
        if (GameAbilitySystem.Instance != null)
        {
            Debug.Log("AddTestGAPerformer");
            GameAbilitySystem.Instance.AddPerformer<TestGA>(TestGAPerformer);
        }
    }

    private IEnumerator TestGAPerformer(TestGA testGA)
    {
        Debug.Log("Perform Test GA");
        
        _text.text = testGA.TestGaContext.Text;
        testGA.TestGaContext.TextObject.gameObject.SetActive(true);
            
        yield return new WaitForSeconds(0.7f);
        
        testGA.TestGaContext.TextObject.gameObject.SetActive(false);
        
        _text.text = "Debug TEXT";
    }
}
