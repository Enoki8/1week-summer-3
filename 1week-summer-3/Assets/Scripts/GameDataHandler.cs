using UnityEngine;
using UnityEngine.WSA;

public class GameDataHandler : MonoBehaviour
{
    public static GameDataHandler Instance;
    private Animator anim;
    public bool[] OpenData = new bool[8];

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    public void SetData(int loc)
    {
        OpenData[loc] = true;
    }
    public void ResetData()
    {
        OpenData = new bool[8];
    }

}
