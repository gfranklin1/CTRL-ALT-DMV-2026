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
        // singleton
        // If a duplicate already exists (e.g. from a scene that also has one), destroy this one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad needs a root-level GameObject, so unparent it
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        // Clear static reference so nothing calls into a destroyed object
        if (Instance == this) Instance = null;
    }

    public void LoadScene(string sceneName)
    {
        if (this == null) return; // Unity overloads == null for destroyed objects
        if (isLoading) return;    // Prevent overlapping scene loads
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
        // Block raycasts during fade to prevent UI clicks mid-transition
        fadeCanvasGroup.blocksRaycasts = true;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // unscaled so fade works even if timeScale is 0
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = to;
        // Only keep blocking raycasts if we faded TO opaque (screen is covered)
        fadeCanvasGroup.blocksRaycasts = to > 0.5f;
    }
}
