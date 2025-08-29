using TMPro;
using UnityEngine;

public class TimeLimitViewer : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI rimitTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ShowTimeRemain();
    }

    private void ShowTimeRemain()
    {
        int num = TimeLimitManager.Instance.nowSwowingTimeNumber;
        rimitTime.text = "<mspace=100px>" + num.ToString() + "</mspace>";
    }
}
