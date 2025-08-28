using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // BGM再生用
    [SerializeField] private AudioSource seSource;  // SE再生用

    [Header("Default Settings")]
    [SerializeField] private float crossfadeDuration = 1.0f; // BGM切り替え時のクロスフェード時間

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// BGMをクロスフェードで再生する
    /// </summary>
    /// <param name="clipName">Resources/Audio/BGM/ フォルダ内のオーディオファイル名</param>
    public void PlayBGM(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/BGM/{clipName}");
        if (clip != null)
        {
            StartCoroutine(CrossfadeBGM(clip));
        }
        else
        {
            Debug.LogError($"BGMの読み込みに失敗しました: Resources/Audio/BGM/{clipName}");
        }
    }

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="clipName">Resources/Audio/SE/ フォルダ内のオーディオファイル名</param>
    public void PlaySE(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/SE/{clipName}");
        if (clip != null)
        {
            seSource.PlayOneShot(clip); // 重ねて再生可能なSE
        }
        else
        {
            Debug.LogError($"SEの読み込みに失敗しました: Resources/Audio/SE/{clipName}");
        }
    }

    /// <summary>
    /// BGMを停止する
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    private IEnumerator CrossfadeBGM(AudioClip nextClip)
    {
        // 現在再生中のBGMと同じクリップなら何もしない
        if (bgmSource.clip == nextClip && bgmSource.isPlaying)
        {
            yield break;
        }

        // フェードアウト
        float startVolume = bgmSource.volume;
        for (float t = 0; t < crossfadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / crossfadeDuration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVolume; // ボリュームを元に戻す

        // フェードイン
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