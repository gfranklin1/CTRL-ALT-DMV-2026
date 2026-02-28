using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinFailUI : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject failPanel;
    [SerializeField] Text winPayoutText;
    [SerializeField] Button winPlayAgainButton;
    [SerializeField] Button failPlayAgainButton;

    void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
        if (winPanel != null)  winPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);

        if (winPlayAgainButton != null)  winPlayAgainButton.onClick.AddListener(PlayAgain);
        if (failPlayAgainButton != null) failPlayAgainButton.onClick.AddListener(PlayAgain);
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        if (state == GameState.Win)
        {
            if (winPanel != null) winPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (state == GameState.Fail)
        {
            if (failPanel != null) failPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
