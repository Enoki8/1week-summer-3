using UnityEngine;

public class TimeLimitStop : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TimeLimitManager.Instance.isStop = true;
    }
}
