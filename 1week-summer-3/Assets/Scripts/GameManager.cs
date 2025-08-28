using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool timeIsRemain = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
