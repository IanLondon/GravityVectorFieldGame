using UnityEngine;
using UnityEngine.UIElements;

public class HUDCrosshair : MonoBehaviour
{
    VisualElement reticle;

    void Awake()
    {
        UIDocument baseContainerDocument = GetComponent<UIDocument>();
        reticle = baseContainerDocument.rootVisualElement.Q<VisualElement>("Reticle");
    }

    void Start()
    {
        ResetClasses();
        
        // assume we start with this state
        reticle.AddToClassList("outOfRangeOfLookPoint");

        GameEventsSingleton.instance.OnPushOffStateChange += OnPushOffStateChange;
    }

    void ResetClasses()
    {
        reticle.ClearClassList();
        reticle.AddToClassList("reticle");
    }

    void OnPushOffStateChange(PushOffState state)
    {
        ResetClasses();

        switch (state)
        {
            case PushOffState.OutOfRangeOfLookPoint:
                reticle.AddToClassList("outOfRangeOfLookPoint");
                break;
            case PushOffState.InRangeOfLookPoint:
                reticle.AddToClassList("inRangeOfLookPoint");
                break;
            case PushOffState.TractorEngagedCannotPushOff:
                reticle.AddToClassList("tractorEngagedCannotPushOff");
                break;
            case PushOffState.TractorEngagedCanPushOff:
                reticle.AddToClassList("tractorEngagedCanPushOff");
                break;
            default:
                Debug.LogWarning("Got unhandled enum value");
                break;
        }
        
    }

    
}
