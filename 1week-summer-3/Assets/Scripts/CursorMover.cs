using UnityEngine;

public class TriangleMover : MonoBehaviour
{
    private float nowPos;
    private Transform tr;
    private float scare;
    [SerializeField] private int moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        tr = this.GetComponent<Transform>();
        scare = tr.localScale.x;
        nowPos = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //‰ñ“]‚³‚¹‚é
        AddTime();
        tr.localScale = new Vector3(scare * Mathf.Sin(nowPos), scare, 0);
    }

    void AddTime()
    {
        nowPos += Time.deltaTime * moveSpeed;
        if (nowPos >= Mathf.PI)
        {
            nowPos -= Mathf.PI;
        }
    }
}
