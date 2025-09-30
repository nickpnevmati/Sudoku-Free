using UnityEngine;
using deVoid.UIFramework;

public class UIController : MonoBehaviour
{
    [SerializeField] UISettings settings;

    private UIFrame uiFrame;

    void Awake()
    {
        uiFrame = settings.CreateUIInstance();
        uiFrame.ShowScreen("WindowInGame");
    }
}
