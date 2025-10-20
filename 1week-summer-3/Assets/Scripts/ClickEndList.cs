using UnityEngine;

public class ClickEndList : MonoBehaviour
{
    public void onclick()
    {
        FadeManager.Instance.FadeToSceneOnTitle("ENDLIST");
    }
}
