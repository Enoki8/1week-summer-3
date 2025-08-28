using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    // �V���O���g���C���X�^���X
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // BGM�Đ��p
    [SerializeField] private AudioSource seSource;  // SE�Đ��p

    [Header("Default Settings")]
    [SerializeField] private float crossfadeDuration = 1.0f; // BGM�؂�ւ����̃N���X�t�F�[�h����

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// BGM���N���X�t�F�[�h�ōĐ�����
    /// </summary>
    /// <param name="clipName">Resources/Audio/BGM/ �t�H���_���̃I�[�f�B�I�t�@�C����</param>
    public void PlayBGM(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/BGM/{clipName}");
        if (clip != null)
        {
            StartCoroutine(CrossfadeBGM(clip));
        }
        else
        {
            Debug.LogError($"BGM�̓ǂݍ��݂Ɏ��s���܂���: Resources/Audio/BGM/{clipName}");
        }
    }

    /// <summary>
    /// SE���Đ�����
    /// </summary>
    /// <param name="clipName">Resources/Audio/SE/ �t�H���_���̃I�[�f�B�I�t�@�C����</param>
    public void PlaySE(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/SE/{clipName}");
        if (clip != null)
        {
            seSource.PlayOneShot(clip); // �d�˂čĐ��\��SE
        }
        else
        {
            Debug.LogError($"SE�̓ǂݍ��݂Ɏ��s���܂���: Resources/Audio/SE/{clipName}");
        }
    }

    /// <summary>
    /// BGM���~����
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    private IEnumerator CrossfadeBGM(AudioClip nextClip)
    {
        // ���ݍĐ�����BGM�Ɠ����N���b�v�Ȃ牽�����Ȃ�
        if (bgmSource.clip == nextClip && bgmSource.isPlaying)
        {
            yield break;
        }

        // �t�F�[�h�A�E�g
        float startVolume = bgmSource.volume;
        for (float t = 0; t < crossfadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / crossfadeDuration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVolume; // �{�����[�������ɖ߂�

        // �t�F�[�h�C��
        bgmSource.clip = nextClip;
        bgmSource.Play();
        for (float t = 0; t < crossfadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, startVolume, t / crossfadeDuration);
            yield return null;
        }
        bgmSource.volume = startVolume;
    }
}