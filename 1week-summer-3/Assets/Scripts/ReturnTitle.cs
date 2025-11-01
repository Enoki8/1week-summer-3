using UnityEngine;
using UnityEngine.InputSystem;

public class ReturnTitle : MonoBehaviour
{
    private PlayerInput playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInput.actions["Click"].WasPressedThisFrame())
        {
            StartCoroutine(FadeManager.Instance.FadeToScene("00_Title"));
        }
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    void OnEnable()
    {
        playerInput.actions.Enable();
    }
}
