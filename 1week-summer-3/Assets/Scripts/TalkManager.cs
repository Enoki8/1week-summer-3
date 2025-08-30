using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private Image characterImage;

    [Header("選択肢パーツ")]
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;

    [Header("テキスト設定")]
    [SerializeField] private float clickDebounceTime = 0.2f; // クリック連打による意図しないスキップを防ぐための待機時間

    [Header("演出設定")]
    [SerializeField] private float fadeSpeed = 1.0f;

    [Header("シナリオファイル")]
    [SerializeField] private TextAsset scenarioFile;

    private List<ScenarioLine> _scenarioLines;
    private int _currentLineIndex = 0;

    private Dictionary<string, int> _labelDictionary = new Dictionary<string, int>();
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
        choiceContainer.gameObject.SetActive(false);

        if (characterImage != null)
        {
            characterImage.gameObject.SetActive(false);
            characterImage.color = new Color(1, 1, 1, 0);
        }
        if (fadeImage != null)
        {
            //fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;
        }

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

        if (_talkCoroutine != null)
        {
            StopCoroutine(_talkCoroutine);
        }
        _talkCoroutine = StartCoroutine(TalkCoroutine());
    }

    private void LoadScenario(string csvText)
    {
        _scenarioLines = new List<ScenarioLine>();
        _labelDictionary.Clear();

        var lines = csvText.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

        if (lines.Count > 0)
        {
            lines.RemoveAt(0); // ヘッダー行をスキップ
        }

        // シナリオデータをリストに格納しつつ、同時にラベルも登録する（ループを1回にまとめる）
        foreach (var line in lines)
        {
            var values = line.Trim().Split(',');
            var data = new ScenarioLine();

            // Commandの解析
            if (values.Length > 1 && values[1].StartsWith("CHOICE:"))
            {
                data.Command = string.Join(",", values.Skip(1));
            }
            else if (values.Length > 1)
            {
                data.Command = values[1];
            }

            // ラベルの登録
            if (!string.IsNullOrEmpty(data.Command) && data.Command.StartsWith("LABEL:"))
            {
                string label = data.Command.Substring("LABEL:".Length);
                _labelDictionary[label] = _scenarioLines.Count; // 現在の行のインデックスを登録
            }

            // 各列のデータを格納
            data.ID = values[0];
            if (values.Length > 2) data.CharacterName = values[2];
            if (values.Length > 3) data.Sentence = values[3];

            _scenarioLines.Add(data);
        }
    }

    // TalkManager.cs 内

    private IEnumerator TalkCoroutine()
    {
        _isTalking = true;
        textBoxObject.SetActive(true);

        while (_currentLineIndex < _scenarioLines.Count)
        {
            // 選択肢が表示されている場合は、解除されるまで待機
            yield return new WaitUntil(() => !_isWaitingForChoice);

            ScenarioLine currentLine = _scenarioLines[_currentLineIndex];

            // これから処理する行がコマンドかどうかを先に判定しておく
            bool isCommand = !string.IsNullOrEmpty(currentLine.Command);

            // 現在の行を処理（コマンド実行 or テキスト表示）
            yield return StartCoroutine(ProcessLine(currentLine));

            // 選択肢が表示されている場合、クリック待ちはHandleChoiceCommandに任せる
            if (_isWaitingForChoice)
            {
                continue;
            }

            // ★処理した行がコマンドでなかった（＝セリフだった）場合のみ、クリックを待つ
            if (!isCommand)
            {
                // 次の行に進むためにクリックを待つ
                nextIconObject.SetActive(true);
                yield return new WaitUntil(() => playerInput.actions["Click"].WasPressedThisFrame());
                nextIconObject.SetActive(false);

                // 連打によるスキップ防止
                if (clickDebounceTime > 0)
                    yield return new WaitForSeconds(clickDebounceTime);
            }

            _currentLineIndex++;
        }

        EndTalk();
    }

    // 1行分のシナリオを解釈して実行する
    private IEnumerator ProcessLine(ScenarioLine line)
    {
        if (!string.IsNullOrEmpty(line.Command))
        {
            string[] parts = line.Command.Split(':');
            string commandName = parts[0];
            string[] arguments = parts.Length > 1 ? parts[1].Split(',') : new string[0];

            switch (commandName)
            {
                case "CHANGE_BG":
                    yield return StartCoroutine(ChangeBackgroundFade(arguments[0]));
                    break;
                case "SHOW_PORTRAIT":
                    yield return StartCoroutine(ShowCharacterFade(arguments[0]));
                    break;
                case "HIDE_PORTRAIT":
                    yield return StartCoroutine(HideCharacterFade());
                    break;
                case "CHOICE":
                    HandleChoiceCommand(arguments);
                    break; // HandleChoiceCommand内でisWaitingForChoiceがtrueになる
                case "JUMP":
                    JumpToLabel(arguments[0]);
                    break;
                case "LABEL":
                    // LABELはLoad時に処理済みなので何もしない
                    break;
                case "LOAD_SCENE":
                    yield return StartCoroutine(FadeManager.Instance.FadeToScene(arguments[0]));
                    yield break;
                default:
                    ExecuteImmediateCommand(commandName, arguments.Length > 0 ? arguments[0] : "");
                    break;
            }
        }
        else // コマンドがない場合はテキスト表示
        {
            UpdateCharacterName(line.CharacterName);
            yield return StartCoroutine(TypeSentenceCoroutine(line.Sentence));
        }
    }

    // キャラクター名表示を更新する
    private void UpdateCharacterName(string characterName)
    {
        if (!string.IsNullOrEmpty(characterName))
        {
            characterNameTextUI.gameObject.SetActive(true);
            characterNameTextUI.text = characterName;
        }
        else
        {
            characterNameTextUI.gameObject.SetActive(false);
        }
    }

    // 選択肢コマンドを処理
    private void HandleChoiceCommand(string[] arguments)
    {
        if (arguments.Length == 0 || arguments.Length % 2 != 0)
        {
            Debug.LogError($"CHOICEコマンドの引数が不正です。CSVを確認してください。 引数: {string.Join(",", arguments)}");
            return;
        }

        _isWaitingForChoice = true;
        choiceContainer.gameObject.SetActive(true);

        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

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
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        choiceContainer.gameObject.SetActive(false);

        JumpToLabel(targetLabel);

        // 待機状態を解除する前に、連打防止のウェイトを入れる
        StartCoroutine(ResumeTalkAfterChoice());
    }

    private IEnumerator ResumeTalkAfterChoice()
    {
        if (clickDebounceTime > 0)
            yield return new WaitForSeconds(clickDebounceTime);
        _isWaitingForChoice = false;
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

    // このコルーチンはテキストをタイプアップ表示することだけに専念する
    private IEnumerator TypeSentenceCoroutine(string sentence)
    {
        textUI.text = sentence;
        // 今後、一文字ずつ表示する演出を追加する場合はここに記述
        yield return null; // 1フレーム待機（テキストがUIに反映されるのを保証）
    }

    // 待機が不要なコマンドを即時実行する
    private void ExecuteImmediateCommand(string command, string argument)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManagerが見つかりません！");
        }

        switch (command)
        {
            case "PLAY_BGM": AudioManager.Instance.PlayBGM(argument); break;
            case "STOP_BGM": AudioManager.Instance.StopBGM(); break;
            case "PLAY_SE": AudioManager.Instance.PlaySE(argument); break;
            default:
                Debug.LogWarning($"未定義のコマンドが実行されました: {command}");
                break;
        }
    }

    private IEnumerator ShowCharacterFade(string characterSpriteName)
    {
        if (characterImage == null) yield break;

        // 画像を読み込み
        Sprite characterSprite = Resources.Load<Sprite>($"Characters/{characterSpriteName}");
        if (characterSprite == null)
        {
            Debug.LogError($"キャラクター画像の読み込みに失敗しました: Resources/Characters/{characterSpriteName}");
            yield break;
        }

        characterImage.sprite = characterSprite;
        characterImage.gameObject.SetActive(true);

        // フェードイン処理
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime * fadeSpeed;
            characterImage.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, timer * 2));
            yield return null;
        }
        characterImage.color = new Color(1, 1, 1, 1);
    }

    /// <summary>
    /// キャラクターをフェードアウトで非表示にするコルーチン
    /// </summary>
    private IEnumerator HideCharacterFade()
    {
        if (characterImage == null) yield break;

        // フェードアウト処理
        float timer = 0f;
        while (timer < 1.0f)
        {
            timer += Time.deltaTime * fadeSpeed;
            characterImage.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, timer));
            yield return null;
        }
        characterImage.color = new Color(1, 1, 1, 0);
        characterImage.gameObject.SetActive(false);
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
        textUI.text = "";
        characterNameTextUI.text = "";

        if (_talkCoroutine != null)
        {
            StopCoroutine(_talkCoroutine);
            _talkCoroutine = null;
        }
        StartCoroutine(FadeManager.Instance.FadeToScene("00_Title"));
        Debug.Log("会話終了");

    }
}