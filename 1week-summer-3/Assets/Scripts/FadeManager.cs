using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;
    private Animator anim;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        anim = this.GetComponent<Animator>();
    }

    public IEnumerator FadeToScene(string sceneName)
    {
        yield return StartCoroutine(FadeAndLoad(sceneName));
    }
    public void FadeToSceneOnTitle(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(1));
        SceneManager.LoadScene(sceneName);
        yield return null; // シーンロード直後の1フレーム待機
        yield return StartCoroutine(Fade(0));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        bool isFadeIn = targetAlpha == 1;
        string triggerName = isFadeIn ? "FadeIn" : "FadeOut";
        string stateName = isFadeIn ? "FadeIn" : "FadeOut";

        Debug.Log($"Fade coroutine started. Triggering: {triggerName}");

        // アニメーションをトリガー
        anim.SetTrigger(triggerName);

        // アニメーターが再生状態になるまで少し待つ
        yield return null;

        // 現在のステートが目的のステートになるまで待つ
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            // 目的のステートに遷移するまで待機
            yield return null;
        }

        Debug.Log($"State changed to: {stateName}. Waiting for animation to finish.");

        // アニメーションが終了するまで待つ ( normalizedTimeが1以上になるまで )
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        Debug.Log($"Animation {stateName} finished.");
    }

}
