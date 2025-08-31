using UnityEngine;

public class TimeSetForGameOver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TimeLimitManager.Instance.nowSwowingTimeNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
