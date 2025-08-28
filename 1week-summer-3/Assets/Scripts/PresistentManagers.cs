using UnityEngine;
public class PersistentManagers : MonoBehaviour
{
    private static bool exists = false;
    void Awake()
    {
        if (exists) { Destroy(gameObject); return; }
        exists = true;
        DontDestroyOnLoad(gameObject);
    }
}
