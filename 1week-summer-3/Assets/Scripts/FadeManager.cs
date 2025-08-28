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

    public void FadeToScene(string sceneName)
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
        anim.SetTrigger(targetAlpha == 1 ? "FadeIn" : "FadeOut");
        string stateName = targetAlpha == 1 ? "FadeIn" : "FadeOut";
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
    }

}
