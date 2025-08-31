using UnityEngine;

public class TimeLimitReset : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TimeLimitManager.Instance.ResetTime();
    }
}
