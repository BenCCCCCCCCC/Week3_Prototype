using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanelUI : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject panelRoot;
    public TMP_Text resultTitleText;
    public TMP_Text resultBodyText;

    [Header("Optional Result Text")]
    public TMP_Text resultOutcomeText;

    [Header("Role Reference")]
    public RoleSwitchController roleSwitchController;

    [Header("Fade")]
    public GameObject fadePanel;
    public Image fadeImage;
    public float fadeDuration = 0.25f;
    public bool useFadeBeforeShow = true;

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
        AutoFindReferences();

        string title = GetResultTitle(result);
        string outcome = GetPlayerOutcomeText(result);
        string body = BuildResultBody(
            result,
            outcome,
            matchDuration,
            escapedCount,
            eliminatedCount,
            downedCount,
            completedCipherCount,
            settlementSummary
        );

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }

        if (resultTitleText != null)
        {
            resultTitleText.text = "";
        }

        if (resultOutcomeText != null)
        {
            resultOutcomeText.text = "";
        }

        if (resultBodyText != null)
        {
            resultBodyText.text = "";
        }

        if (useFadeBeforeShow && fadePanel != null && fadeImage != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(ShowResultsWithFade(title, outcome, body));
        }
        else
        {
            ShowResultsInstant(title, outcome, body);
        }

        if (logResultUI)
        {
            Debug.Log("ResultPanelUI: ShowResults called. Result = " + result + ", Outcome = " + outcome);
        }
    }

    void AutoFindReferences()
    {
        if (roleSwitchController == null)
        {
            roleSwitchController = FindFirstObjectByType<RoleSwitchController>();
        }
    }

    string BuildResultBody(
        MatchResult result,
        string outcome,
        float matchDuration,
        int escapedCount,
        int eliminatedCount,
        int downedCount,
        int completedCipherCount,
        SettlementSummary settlementSummary
    )
    {
        string body = "";

        // Keep space for title and optional outcome text.
        body += "\n\n\n";

        if (resultOutcomeText == null)
        {
            body += "Outcome: " + outcome + "\n\n";
        }

        body += "Match Summary\n";
        body += "Match Duration: " + matchDuration.ToString("F1") + " s\n";
        body += "Escaped Survivors: " + escapedCount + "\n";
        body += "Eliminated Survivors: " + eliminatedCount + "\n";
        body += "Hunter Downs: " + downedCount + "\n";
        body += "Completed Ciphers: " + completedCipherCount + "\n";

        if (settlementSummary != null)
        {
            body += "\nRewards\n";
            body += "Soft Currency: +" + settlementSummary.totalSoft + "\n";
            body += "Premium Currency: +" + settlementSummary.totalPremium + "\n";
            body += "Archive Material: +" + settlementSummary.totalMaterial + "\n";

            if (settlementSummary.loadoutSoft > 0 || settlementSummary.loadoutMaterial > 0)
            {
                body += "\nLoadout Bonus\n";
                body += "Bonus Soft: +" + settlementSummary.loadoutSoft + "\n";
                body += "Bonus Material: +" + settlementSummary.loadoutMaterial + "\n";
            }

            body += "\nCompleted Tasks\n";
            body += settlementSummary.completedTaskText + "\n";

            body += "\nEquipped Loadout\n";
            body += settlementSummary.equippedLoadoutText;
        }
        else
        {
            body += "\nRewards\n";
            body += "No reward data available.";
        }

        return body;
    }

    IEnumerator ShowResultsWithFade(string title, string outcome, string body)
    {
        yield return FadeToBlack();

        ShowResultsInstant(title, outcome, body);

        yield return FadeFromBlack();
    }

    void ShowResultsInstant(string title, string outcome, string body)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }

        if (resultTitleText != null)
        {
            resultTitleText.text = title;
        }

        if (resultOutcomeText != null)
        {
            resultOutcomeText.text = outcome;
        }

        if (resultBodyText != null)
        {
            resultBodyText.text = body;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePanel()
    {
        if (resultTitleText != null)
        {
            resultTitleText.text = "";
        }

        if (resultOutcomeText != null)
        {
            resultOutcomeText.text = "";
        }

        if (resultBodyText != null)
        {
            resultBodyText.text = "";
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    IEnumerator FadeToBlack()
    {
        if (fadePanel == null || fadeImage == null)
        {
            yield break;
        }

        fadePanel.SetActive(true);
        fadePanel.transform.SetAsLastSibling();

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(1f);
    }

    IEnumerator FadeFromBlack()
    {
        if (fadePanel == null || fadeImage == null)
        {
            yield break;
        }

        fadePanel.SetActive(true);
        fadePanel.transform.SetAsLastSibling();

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(0f);
        fadePanel.SetActive(false);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
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

    string GetPlayerOutcomeText(MatchResult result)
    {
        if (result == MatchResult.Draw)
        {
            return "Draw";
        }

        if (roleSwitchController == null)
        {
            return "Match Finished";
        }

        if (roleSwitchController.currentRole == PlayableRole.Hunter)
        {
            if (result == MatchResult.HunterWin)
            {
                return "You Win";
            }

            return "You Lose";
        }

        if (roleSwitchController.currentRole == PlayableRole.Survivor)
        {
            if (result == MatchResult.SurvivorWin)
            {
                return "You Win";
            }

            return "You Lose";
        }

        return "Match Finished";
    }
}