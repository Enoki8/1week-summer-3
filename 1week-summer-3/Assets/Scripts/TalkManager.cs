using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // �V�[���Ǘ��̂��߂ɒǉ�

// �V�i���I1�s���̃f�[�^���i�[����N���X
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
    [Header("UI�p�[�c")]
    [SerializeField] private TextMeshProUGUI textUI;
    [SerializeField] private TextMeshProUGUI characterNameTextUI;
    [SerializeField] private GameObject textBoxObject;
    [SerializeField] private GameObject nextIconObject;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fadeImage;

    [Header("�I�����p�[�c")]
    [SerializeField] private GameObject choiceButtonPrefab; // �I�����{�^���̃v���n�u
    [SerializeField] private Transform choiceContainer;      // �I�����{�^����z�u����e�I�u�W�F�N�g

    [Header("�e�L�X�g�ݒ�")]
    [SerializeField] private float waitCanClick = 0.5f;

    [Header("���o�ݒ�")]
    [SerializeField] private float fadeSpeed = 1.0f;

    [Header("�V�i���I�t�@�C��")]
    [SerializeField] private TextAsset scenarioFile;

    private List<ScenarioLine> _scenarioLines;
    private int _currentLineIndex = 0;

    // ���x�����ƍs�C���f�b�N�X��R�t���邽�߂̎���
    private Dictionary<string, int> _labelDictionary = new Dictionary<string, int>();
    // �v���C���[���I������I�Ԃ܂ŃV�i���I�i�s��ҋ@�����邽�߂̕ϐ�
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
        choiceContainer.gameObject.SetActive(false); // �I�����R���e�i���\����

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
        _labelDictionary.Clear(); // ���x���������N���A

        var lines = csvText.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

        // �w�b�_�[�s���X�L�b�v
        if (lines.Count > 0)
        {
            lines.RemoveAt(0);
        }

        // �܂��A�S�Ă�LABEL�R�}���h���X�L�������Ď����ɓo�^����
        for (int i = 0; i < lines.Count; i++)
        {
            var values = lines[i].Trim().Split(',');
            if (values.Length > 1 && !string.IsNullOrEmpty(values[1]))
            {
                if (values[1].StartsWith("LABEL:"))
                {
                    // "LABEL:" �̕������폜���ă��x�����������擾
                    string label = values[1].Substring("LABEL:".Length);
                    _labelDictionary[label] = i;
                }
            }
        }

        // �V�i���I�f�[�^�����X�g�Ɋi�[
        foreach (var line in lines)
        {
            var values = line.Trim().Split(',');
            var data = new ScenarioLine();

            // Command��CHOICE�Ŏn�܂�ꍇ�A���ʏ������s��
            if (values.Length > 1 && values[1].StartsWith("CHOICE:"))
            {
                // 2��ڈȍ~�̗v�f��S�ăJ���}�ŘA�����A�P�̃R�}���h�Ƃ��čč\�z����
                string combinedCommand = string.Join(",", values.Skip(1));

                data.ID = values[0];
                data.Command = combinedCommand;
                data.CharacterName = ""; // CHOICE�s�̓L�������ƃZ���t�͋�
                data.Sentence = "";
            }
            else // ����ȊO�̍s�́A����܂Œʂ�̏���
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
            // �I��҂��t���O�������Ă���Ԃ̓R���[�`�����ꎞ��~
            yield return new WaitUntil(() => !_isWaitingForChoice);

            ScenarioLine currentLine = _scenarioLines[_currentLineIndex];

            // --- �R�}���h�̎��s ---
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
                        _currentLineIndex++; // Jump��͎��̍s���������Ȃ��悤�ɃC���N�������g���Ă���
                        continue; // �����Ɏ��̃��[�v��
                    case "LABEL":
                        // LABEL��Load���ɏ����ς݂Ȃ̂ŉ������Ȃ�
                        break;
                    case "LOAD_SCENE":
                        yield return StartCoroutine(LoadSceneFade(arguments[0]));
                        // �V�[���J�ڌ�͂���TalkManager�͕s�v�ɂȂ�̂ŁA�R���[�`�����I��
                        yield break;
                    default:
                        ExecuteSimpleCommand(commandName, arguments.Length > 0 ? arguments[0] : "");
                        break;
                }

                _currentLineIndex++;
                continue;
            }

            // --- �e�L�X�g�Ɩ��O�̕\�� ---
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

    // �I�����R�}���h������
    private void HandleChoiceCommand(string[] arguments)
    {
        // �����̐�����̏ꍇ�́A�y�A���������Ȃ��̂ŃG���[
        if (arguments.Length == 0 || arguments.Length % 2 != 0)
        {
            Debug.LogError($"CHOICE�R�}���h�̈������s���ł��B�e�L�X�g�ƃ��x�����y�A�ɂȂ��Ă��Ȃ����A��������ł��BCSV���m�F���Ă��������B ����: {string.Join(",", arguments)}");
            // �ҋ@��ԂɂȂ炸�Ɏ��̍s�֐i�ނ悤�ɂ���
            _isWaitingForChoice = false;
            return;
        }

        _isWaitingForChoice = true;
        choiceContainer.gameObject.SetActive(true);

        // �Â��I�������c���Ă���΍폜
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        // CSV����ǂݎ�������őI�����𐶐�
        for (int i = 0; i < arguments.Length; i += 2)
        {
            string choiceText = arguments[i];
            string targetLabel = arguments[i + 1];

            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
            ChoiceButton choiceButton = buttonObj.GetComponent<ChoiceButton>();
            choiceButton.Setup(choiceText, targetLabel, OnChoiceSelected);
        }
    }

    // �I�����{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo�����
    public void OnChoiceSelected(string targetLabel)
    {
        // �I������S�č폜
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        choiceContainer.gameObject.SetActive(false);
        textBoxObject.SetActive(true); // ��b�E�B���h�E���ĕ\��

        // �V�i���I���w��̃��x���ɃW�����v������
        JumpToLabel(targetLabel);

        _isWaitingForChoice = false; // �ҋ@��Ԃ�����
    }



    private void JumpToLabel(string label)
    {
        if (_labelDictionary.TryGetValue(label, out int index))
        {
            _currentLineIndex = index;
        }
        else
        {
            Debug.LogError($"�w�肳�ꂽ���x����������܂���: {label}");
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

    // �ҋ@���s�v�ȃR�}���h�����s����
    private void ExecuteSimpleCommand(string command, string argument)
    {
        Debug.Log($"�R�}���h���s: {command}, ����: {argument}");
        switch (command)
        {
            case "SHOW_PORTRAIT":
                // TODO: �����G�\���̏���
                break;
            case "PLAY_BGM":
                // TODO: BGM�Đ��̏���
                break;
        }
    }

    // �t�F�[�h�t���Ŕw�i��ύX����R���[�`��
    private IEnumerator ChangeBackgroundFade(string backgroundName)
    {
        // �t�F�[�h�A�E�g
        yield return StartCoroutine(Fade(1.0f));

        // �w�i�摜�������ւ�
        if (backgroundImage != null)
        {
            Sprite newBackground = Resources.Load<Sprite>($"Backgrounds/{backgroundName}");
            if (newBackground != null)
            {
                backgroundImage.sprite = newBackground;
            }
            else
            {
                Debug.LogError($"�w�i�摜�̓ǂݍ��݂Ɏ��s���܂���: Resources/Backgrounds/{backgroundName}");
            }
        }

        // �t�F�[�h�C��
        yield return StartCoroutine(Fade(0.0f));
    }

    /// <summary>
    /// �t�F�[�h�t���Ŏw�肳�ꂽ�V�[�������[�h����R���[�`��
    /// </summary>
    /// <param name="sceneName">���[�h����V�[����</param>
    private IEnumerator LoadSceneFade(string sceneName)
    {
        // �܂���ʂ��t�F�[�h�A�E�g������
        yield return StartCoroutine(Fade(1.0f));

        // �񓯊��ŃV�[�������[�h����
        //AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        FadeManager.Instance.FadeToScene(sceneName);

        //// ���[�h����������܂őҋ@����
        //while (!asyncLoad.isDone)
        //{
        //    yield return null;
        //}
    }

    // �t�F�[�h�����̖{��
    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("�t�F�[�h�p��Image���ݒ肳��Ă��܂���B");
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
            yield return null; // 1�t���[���҂�
        }

        // �m���ɖڕW�̃A���t�@�l�ɂ���
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
        Debug.Log("��b�I��");
    }
}