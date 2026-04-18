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
        int completedCipherCount,
        SettlementSummary settlementSummary
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
            string body = "";
            body += "\n\n\n";   // 给标题让出空间，避免和正文重叠

            body += "Match Duration: " + matchDuration.ToString("F1") + " s\n";
            body += "Escaped Survivors: " + escapedCount + "\n";
            body += "Eliminated Survivors: " + eliminatedCount + "\n";
            body += "Hunter Downs: " + downedCount + "\n";
            body += "Completed Ciphers: " + completedCipherCount;

            if (settlementSummary != null)
            {
                body += "\n\nRewards";
                body += "\nSoft Currency: +" + settlementSummary.totalSoft;
                body += "\nPremium Currency: +" + settlementSummary.totalPremium;
                body += "\nArchive Material: +" + settlementSummary.totalMaterial;

                if (settlementSummary.loadoutSoft > 0 || settlementSummary.loadoutMaterial > 0)
                {
                    body += "\nExtra From Loadout";
                    body += "\nSoft: +" + settlementSummary.loadoutSoft;
                    body += "\nMaterial: +" + settlementSummary.loadoutMaterial;
                }

                body += "\n\nCompleted Tasks: " + settlementSummary.completedTaskText;
                body += "\n\nEquipped Loadout";
                body += "\n" + settlementSummary.equippedLoadoutText;
            }

            resultBodyText.text = body;
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

        if (resultTitleText != null)
        {
            resultTitleText.text = "";
        }

        if (resultBodyText != null)
        {
            resultBodyText.text = "";
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