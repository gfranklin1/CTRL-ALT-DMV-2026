using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public const string HOME_SCENE = "HomeScene";

    [SerializeField] CanvasGroup fadeCanvasGroup;
    [SerializeField] float fadeDuration = 0.4f;

    bool isLoading;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null); // Must be a root object for DontDestroyOnLoad
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        // Clear static reference so nothing calls into a destroyed object
        if (Instance == this) Instance = null;
    }

    public void LoadScene(string sceneName)
    {
        if (this == null) return; // guard against destroyed-but-still-referenced singleton
        if (isLoading) return;
        isLoading = true;
        StartCoroutine(LoadWithFade(sceneName));
    }

    public void LoadHome()
    {
        LoadScene(HOME_SCENE);
    }

    public void ReloadCurrent()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator LoadWithFade(string sceneName)
    {
        if (fadeCanvasGroup != null)
            yield return StartCoroutine(Fade(0f, 1f));

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        isLoading = false; // allow future loads (e.g. Retry button)

        if (fadeCanvasGroup != null)
            yield return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        fadeCanvasGroup.blocksRaycasts = true;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = to;
        fadeCanvasGroup.blocksRaycasts = to > 0.5f;
    }
}
