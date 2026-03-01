using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WinFailUI : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject failPanel;
    [SerializeField] Text winPayoutText;
    [SerializeField] Button winGoHomeButton;
    [SerializeField] Button failRetryButton;
    [SerializeField] Button failRetreatButton;

    void Start()
    {
        GameManager.OnStateChanged += OnStateChanged;
        if (winPanel != null)  winPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);

        if (winGoHomeButton != null)   winGoHomeButton.onClick.AddListener(GoHome);
        if (failRetryButton != null)   failRetryButton.onClick.AddListener(Retry);
        if (failRetreatButton != null) failRetreatButton.onClick.AddListener(Retreat);
    }

    void OnDestroy() => GameManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState state)
    {
        if (state == GameState.Win)
        {
            int payout = RunData.TotalSessionPayout;
            if (winPayoutText != null)
                winPayoutText.text = $"PAYOUT: ${payout}";
            if (winPanel != null) winPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EventSystem.current?.SetSelectedGameObject(winGoHomeButton?.gameObject);
        }
        else if (state == GameState.Fail)
        {
            RunData.ChangeReputation(-10);
            RunData.Save();
            RunData.ClearSessionPhotos();
            if (failPanel != null) failPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EventSystem.current?.SetSelectedGameObject(failRetryButton?.gameObject);
        }
    }

    void GoHome()
    {
        // Best grade per celebrity → rep delta
        var bestGrade = new Dictionary<string, string>();
        foreach (var r in RunData.SessionResults)
        {
            if (string.IsNullOrEmpty(r.celebName)) continue;
            if (!bestGrade.ContainsKey(r.celebName) ||
                GradeRank(r.gradeLabel) > GradeRank(bestGrade[r.celebName]))
                bestGrade[r.celebName] = r.gradeLabel;
        }
        foreach (var g in bestGrade.Values)
            RunData.ChangeReputation(RunData.RepDeltaForGrade(g));

        RunData.AddPayout(RunData.TotalSessionPayout);
        RunData.Save();
        SceneLoader.Instance?.LoadHome();
    }

    void Retreat()
    {
        RunData.ChangeReputation(-5);
        RunData.Save();
        SceneLoader.Instance?.LoadHome();
    }

    void Retry()
    {
        SceneLoader.Instance?.ReloadCurrent();
    }

    int GradeRank(string g) => g switch {
        "MONEY SHOT"  => 3,
        "PUBLISHABLE" => 2,
        "WEAK"        => 1,
        _             => 0
    };
}
