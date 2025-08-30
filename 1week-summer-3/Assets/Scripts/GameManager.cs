using UnityEngine;
using System.Collections; // Coroutine���g�����߂ɒǉ�

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject image; // ���Ԑ؂�̎��ɕ\������摜

    private bool _timeIsRemain = true;

    public bool TimeIsRemain
    {
        get { return _timeIsRemain; }
        set
        {
            if (_timeIsRemain == true && value == false)
            {
                _timeIsRemain = value;
                // �ʏ�̃��\�b�h�Ăяo������R���[�`���̊J�n�ɕύX
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

    // ���Ԑ؂�̏������R���[�`���ɕύX
    private IEnumerator HandleTimeUpCoroutine()
    {
        Debug.Log("���Ԑ؂�ł��I�Q�[���I�[�o�[�V�[���Ɉړ����܂��B");

        // 1. �摜��\������
        if (image != null)
        {
            image.SetActive(true);
        }

        // 2. �v���C���[���摜�����邽�߂̒Z���ҋ@���ԁi�b���͒����\�j
        yield return new WaitForSeconds(1.5f);

        // 3. FadeManager���g���ăV�[���J�ڂ��s���A��������܂őҋ@����
        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.FadeToScene("GameOver"));
        }
        else
        {
            Debug.LogError("FadeManager�̃C���X�^���X��������܂���I");
            yield break; // FadeManager���Ȃ���Ώ����𒆒f
        }

        // 4. �V�[���J�ڂ�����������A�摜���\���ɂ���
        if (image != null)
        {
            image.SetActive(false);
        }
    }
}