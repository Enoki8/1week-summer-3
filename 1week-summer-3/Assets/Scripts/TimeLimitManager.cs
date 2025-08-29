using TMPro;
using UnityEngine;

public class TimeLimitManager : MonoBehaviour
{
    public static TimeLimitManager Instance;
    public bool isStart = false;
    [SerializeField] private float timeLimit = 60f;

    //[SerializeField] private TextMeshProUGUI rimitTime;

    public  int nowSwowingTimeNumber = 60;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStart) return;
        if (nowSwowingTimeNumber == 0) return;
        DecreaseTime();
        ShowTimeRemain();
        if (nowSwowingTimeNumber == 0)
        {
            GameManager.Instance.timeIsRemain = false;
        }
    }

    public void DecreaseTime()
    {
        timeLimit -= Time.deltaTime;
    }

    private void ShowTimeRemain()
    {
        if (nowSwowingTimeNumber == (int)timeLimit) return;
        Debug.Log(timeLimit);

        nowSwowingTimeNumber = (int)timeLimit;
    }
}
