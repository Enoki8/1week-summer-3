using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // シーン管理のために追加

// シナリオ1行分のデータを格納するクラス
[System.Serializable]
public class ScenarioLine
{
    public string ID;
    public string Command;
    public string CharacterName;
    public string Sentence;
}


public class TalkManager : MonoBehaviour
{
    [Header("UIパーツ")]
    [SerializeField] private TextMeshProUGUI textUI;
    [SerializeField] private TextMeshProUGUI characterNameTextUI;
    [SerializeField] private GameObject textBoxObject;
    [SerializeField] private GameObject nextIconObject;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fadeImage;

    [Header("選択肢パーツ")]
    [SerializeField] private GameObject choiceButtonPrefab; // 選択肢ボタンのプレハブ
    [SerializeField] private Transform choiceContainer;      // 選択肢ボタンを配置する親オブジェクト

    [Header("テキスト設定")]
    [SerializeField] private float waitCanClick = 0.5f;

    [Header("演出設定")]
    [SerializeField] private float fadeSpeed = 1.0f;

    [Header("シナリオファイル")]
    [SerializeField] private TextAsset scenarioFile;

    private List<ScenarioLine> _scenarioLines;
    private int _currentLineIndex = 0;

    // ラベル名と行インデックスを紐付けるための辞書
    private Dictionary<string, int> _labelDictionary = new Dictionary<string, int>();
    // プレイヤーが選択肢を選ぶまでシナリオ進行を待機させるための変数
    private bool _isWaitingForChoice = false;

    private bool _isTalking = false;
    private Coroutine _talkCoroutine;

    private PlayerInput playerInput;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        playerInput.actions.Enable();
    }

    void Start()
    {
        textBoxObject.SetActive(false);
        nextIconObject.SetActive(false);
        characterNameTextUI.gameObject.SetActive(false);
        choiceContainer.gameObject.SetActive(false); // 選択肢コンテナを非表示に

        if (fadeImage != null)
        {
            //fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;
        }

        _scenarioLines = new List<ScenarioLine>();

        if (scenarioFile != null)
        {
            StartTalk(scenarioFile);
        }
    }

    public void StartTalk(TextAsset scenario)
    {
        if (_isTalking) return;

        LoadScenario(scenario.text);
        _currentLineIndex = 0;
        _talkCoroutine = StartCoroutine(TalkCoroutine());
    }

    private void LoadScenario(string csvText)
    {
        _scenarioLines.Clear();
        _labelDictionary.Clear(); // ラベル辞書をクリア

        var lines = csvText.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

        // ヘッダー行をスキップ
        if (lines.Count > 0)
        {
            lines.RemoveAt(0);
        }

        // まず、全てのLABELコマンドをスキャンして辞書に登録する
        for (int i = 0; i < lines.Count; i++)
        {
            var values = lines[i].Trim().Split(',');
            if (values.Length > 1 && !string.IsNullOrEmpty(values[1]))
            {
                if (values[1].StartsWith("LABEL:"))
                {
                    // "LABEL:" の部分を削除してラベル名だけを取得
                    string label = values[1].Substring("LABEL:".Length);
                    _labelDictionary[label] = i;
                }
            }
        }

        // シナリオデータをリストに格納
        foreach (var line in lines)
        {
            var values = line.Trim().Split(',');
            var data = new ScenarioLine();

            // Command列がCHOICEで始まる場合、特別処理を行う
            if (values.Length > 1 && values[1].StartsWith("CHOICE:"))
            {
                // 2列目以降の要素を全てカンマで連結し、１つのコマンドとして再構築する
                string combinedCommand = string.Join(",", values.Skip(1));

                data.ID = values[0];
                data.Command = combinedCommand;
                data.CharacterName = ""; // CHOICE行はキャラ名とセリフは空
                data.Sentence = "";
            }
            else // それ以外の行は、これまで通りの処理
            {
                if (values.Length < 4) continue;
                data.ID = values[0];
                data.Command = values[1];
                data.CharacterName = values[2];
                data.Sentence = values[3];
            }

            _scenarioLines.Add(data);
        }
    }

    private IEnumerator TalkCoroutine()
    {
        _isTalking = true;
        textBoxObject.SetActive(true);

        while (_currentLineIndex < _scenarioLines.Count)
        {
            // 選択待ちフラグが立っている間はコルーチンを一時停止
            yield return new WaitUntil(() => !_isWaitingForChoice);

            ScenarioLine currentLine = _scenarioLines[_currentLineIndex];

            // --- コマンドの実行 ---
            if (!string.IsNullOrEmpty(currentLine.Command))
            {
                string[] parts = currentLine.Command.Split(':');
                string commandName = parts[0];
                string[] arguments = parts.Length > 1 ? parts[1].Split(',') : new string[0];

                switch (commandName)
                {
                    case "CHANGE_BG":
                        yield return StartCoroutine(ChangeBackgroundFade(arguments[0]));
                        break;
                    case "CHOICE":
                        HandleChoiceCommand(arguments);
                        break;
                    case "JUMP":
                        JumpToLabel(arguments[0]);
                        _currentLineIndex++; // Jump後は次の行を処理しないようにインクリメントしておく
                        continue; // 即座に次のループへ
                    case "LABEL":
                        // LABELはLoad時に処理済みなので何もしない
                        break;
                    case "LOAD_SCENE":
                        yield return StartCoroutine(LoadSceneFade(arguments[0]));
                        // シーン遷移後はこのTalkManagerは不要になるので、コルーチンを終了
                        yield break;
                    default:
                        ExecuteSimpleCommand(commandName, arguments.Length > 0 ? arguments[0] : "");
                        break;
                }

                _currentLineIndex++;
                continue;
            }

            // --- テキストと名前の表示 ---
            if (!string.IsNullOrEmpty(currentLine.CharacterName))
            {
                characterNameTextUI.gameObject.SetActive(true);
                characterNameTextUI.text = currentLine.CharacterName;
            }
            else
            {
                characterNameTextUI.gameObject.SetActive(false);
            }

            yield return StartCoroutine(TypeSentenceCoroutine(currentLine.Sentence));

            _currentLineIndex++;
        }

        EndTalk();
    }

    // 選択肢コマンドを処理
    private void HandleChoiceCommand(string[] arguments)
    {
        // 引数の数が奇数の場合は、ペアが成立しないのでエラー
        if (arguments.Length == 0 || arguments.Length % 2 != 0)
        {
            Debug.LogError($"CHOICEコマンドの引数が不正です。テキストとラベルがペアになっていないか、引数が空です。CSVを確認してください。 引数: {string.Join(",", arguments)}");
            // 待機状態にならずに次の行へ進むようにする
            _isWaitingForChoice = false;
            return;
        }

        _isWaitingForChoice = true;
        choiceContainer.gameObject.SetActive(true);

        // 古い選択肢が残っていれば削除
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        // CSVから読み取った情報で選択肢を生成
        for (int i = 0; i < arguments.Length; i += 2)
        {
            string choiceText = arguments[i];
            string targetLabel = arguments[i + 1];

            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
            ChoiceButton choiceButton = buttonObj.GetComponent<ChoiceButton>();
            choiceButton.Setup(choiceText, targetLabel, OnChoiceSelected);
        }
    }

    // 選択肢ボタンがクリックされたときに呼び出される
    public void OnChoiceSelected(string targetLabel)
    {
        // 選択肢を全て削除
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        choiceContainer.gameObject.SetActive(false);
        textBoxObject.SetActive(true); // 会話ウィンドウを再表示

        // シナリオを指定のラベルにジャンプさせる
        JumpToLabel(targetLabel);

        _isWaitingForChoice = false; // 待機状態を解除
    }



    private void JumpToLabel(string label)
    {
        if (_labelDictionary.TryGetValue(label, out int index))
        {
            _currentLineIndex = index;
        }
        else
        {
            Debug.LogError($"指定されたラベルが見つかりません: {label}");
        }
    }

    private IEnumerator TypeSentenceCoroutine(string sentence)
    {
        textUI.text = sentence;
        nextIconObject.SetActive(false);
        yield return new WaitForSeconds(waitCanClick);

        nextIconObject.SetActive(true);
        yield return new WaitUntil(() => playerInput.actions["Click"].WasPressedThisFrame());
        nextIconObject.SetActive(false);
    }

    // 待機が不要なコマンドを実行する
    private void ExecuteSimpleCommand(string command, string argument)
    {
        Debug.Log($"コマンド実行: {command}, 引数: {argument}");
        switch (command)
        {
            case "SHOW_PORTRAIT":
                // TODO: 立ち絵表示の処理
                break;
            case "PLAY_BGM":
                // TODO: BGM再生の処理
                break;
        }
    }

    // フェード付きで背景を変更するコルーチン
    private IEnumerator ChangeBackgroundFade(string backgroundName)
    {
        // フェードアウト
        yield return StartCoroutine(Fade(1.0f));

        // 背景画像を差し替え
        if (backgroundImage != null)
        {
            Sprite newBackground = Resources.Load<Sprite>($"Backgrounds/{backgroundName}");
            if (newBackground != null)
            {
                backgroundImage.sprite = newBackground;
            }
            else
            {
                Debug.LogError($"背景画像の読み込みに失敗しました: Resources/Backgrounds/{backgroundName}");
            }
        }

        // フェードイン
        yield return StartCoroutine(Fade(0.0f));
    }

    /// <summary>
    /// フェード付きで指定されたシーンをロードするコルーチン
    /// </summary>
    /// <param name="sceneName">ロードするシーン名</param>
    private IEnumerator LoadSceneFade(string sceneName)
    {
        // まず画面をフェードアウトさせる
        yield return StartCoroutine(Fade(1.0f));

        // 非同期でシーンをロードする
        //AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        FadeManager.Instance.FadeToScene(sceneName);

        //// ロードが完了するまで待機する
        //while (!asyncLoad.isDone)
        //{
        //    yield return null;
        //}
    }

    // フェード処理の本体
    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("フェード用のImageが設定されていません。");
            yield break;
        }

        Color currentColor = fadeImage.color;
        float startAlpha = currentColor.a;
        float timer = 0.0f;

        while (timer < 1.0f)
        {
            timer += Time.deltaTime * fadeSpeed;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer);
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null; // 1フレーム待つ
        }

        // 確実に目標のアルファ値にする
        fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }

    private void EndTalk()
    {
        _isTalking = false;
        textBoxObject.SetActive(false);
        textUI.text = "";
        characterNameTextUI.text = "";

        if (_talkCoroutine != null)
        {
            StopCoroutine(_talkCoroutine);
            _talkCoroutine = null;
        }
        Debug.Log("会話終了");
    }
}