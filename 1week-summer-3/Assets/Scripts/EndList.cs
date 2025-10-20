using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndList : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text[] texts;
    [SerializeField] private string[] EndText;
    private PlayerInput playerInput;
    private bool set = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool[] openData = GameDataHandler.Instance.OpenData;
        for (int i = 0; i < texts.Length; i++)
        {
            if (openData[i] == true)
            {
                texts[i].SetText(EndText[i]);
            }
        }

    }

    void Update()
    {
        if (set) return;
        if (playerInput.actions["Click"].WasPressedThisFrame())
        {
            set = true;
            Debug.Log("TEST");
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