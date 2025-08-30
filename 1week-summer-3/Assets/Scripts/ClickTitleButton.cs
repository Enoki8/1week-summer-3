using UnityEngine;

public class ClickTitleButton : MonoBehaviour
{
    public void onclick()
    {
        FadeManager.Instance.FadeToSceneOnTitle("Introduction");
    }
}
