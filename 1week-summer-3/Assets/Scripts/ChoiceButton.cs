using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action���g�����߂ɕK�v

public class ChoiceButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    private Button _button;
    private string _targetLabel;
    private Action<string> _onClickAction;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// �I�����{�^���������ݒ肷��
    /// </summary>
    /// <param name="text">�\������e�L�X�g</param>
    /// <param name="label">��ѐ�̃��x��</param>
    /// <param name="action">�N���b�N���Ɏ��s���鏈��</param>
    public void Setup(string text, string label, Action<string> action)
    {
        buttonText.text = text;
        _targetLabel = label;
        _onClickAction = action;
    }

    private void OnClick()
    {
        // �o�^���ꂽ�A�N�V�����iTalkManager�̃��\�b�h�j�����s���A��ѐ惉�x����n��
        _onClickAction?.Invoke(_targetLabel);
    }
}