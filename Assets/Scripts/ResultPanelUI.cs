using TMPro;
using UnityEngine;

public class ResultPanelUI : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject panelRoot;
    public TMP_Text resultTitleText;
    public TMP_Text resultBodyText;

    [Header("Debug")]
    public bool logResultUI = true;

    void Start()
    {
        HidePanel();
    }

    public void ShowResults(
        MatchResult result,
        float matchDuration,
        int escapedCount,
        int eliminatedCount,
        int downedCount,
        int completedCipherCount
    )
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (resultTitleText != null)
        {
            resultTitleText.text = GetResultTitle(result);
        }

        if (resultBodyText != null)
        {
            resultBodyText.text =
                "Match Duration: " + matchDuration.ToString("F1") + " s\n" +
                "Escaped Survivors: " + escapedCount + "\n" +
                "Eliminated Survivors: " + eliminatedCount + "\n" +
                "Hunter Downs: " + downedCount + "\n" +
                "Completed Ciphers: " + completedCipherCount;
        }

        if (logResultUI)
        {
            Debug.Log("ResultPanelUI: ShowResults called. Result = " + result);
        }
    }

    public void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    string GetResultTitle(MatchResult result)
    {
        switch (result)
        {
            case MatchResult.SurvivorWin:
                return "Survivor Victory";
            case MatchResult.HunterWin:
                return "Hunter Victory";
            case MatchResult.Draw:
                return "Draw";
            default:
                return "Match Result";
        }
    }
}