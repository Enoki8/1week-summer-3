using UnityEngine;

public class TimeLimitStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
           TimeLimitManager.Instance.isStart = true;
    }
}
