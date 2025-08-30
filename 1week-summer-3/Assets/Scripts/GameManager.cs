using UnityEngine;
using System.Collections; // Coroutineを使うために追加

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject image; // 時間切れの時に表示する画像

    private bool _timeIsRemain = true;

    public bool TimeIsRemain
    {
        get { return _timeIsRemain; }
        set
        {
            if (_timeIsRemain == true && value == false)
            {
                _timeIsRemain = value;
                // 通常のメソッド呼び出しからコルーチンの開始に変更
                StartCoroutine(HandleTimeUpCoroutine());
            }
            else
            {
                _timeIsRemain = value;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 時間切れの処理をコルーチンに変更
    private IEnumerator HandleTimeUpCoroutine()
    {
        Debug.Log("時間切れです！ゲームオーバーシーンに移動します。");

        // 1. 画像を表示する
        if (image != null)
        {
            image.SetActive(true);
        }

        // 2. プレイヤーが画像を見るための短い待機時間（秒数は調整可能）
        yield return new WaitForSeconds(1.5f);

        // 3. FadeManagerを使ってシーン遷移を行い、完了するまで待機する
        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.FadeToScene("GameOver"));
        }
        else
        {
            Debug.LogError("FadeManagerのインスタンスが見つかりません！");
            yield break; // FadeManagerがなければ処理を中断
        }

        // 4. シーン遷移が完了した後、画像を非表示にする
        if (image != null)
        {
            image.SetActive(false);
        }
    }
}