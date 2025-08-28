using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Actionを使うために必要

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
    /// 選択肢ボタンを初期設定する
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    /// <param name="label">飛び先のラベル</param>
    /// <param name="action">クリック時に実行する処理</param>
    public void Setup(string text, string label, Action<string> action)
    {
        buttonText.text = text;
        _targetLabel = label;
        _onClickAction = action;
    }

    private void OnClick()
    {
        // 登録されたアクション（TalkManagerのメソッド）を実行し、飛び先ラベルを渡す
        _onClickAction?.Invoke(_targetLabel);
    }
}