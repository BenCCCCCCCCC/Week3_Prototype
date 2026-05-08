using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchFlowUIManager : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject lobbyRoot;
    public GameObject characterSelectPanel;
    public GameObject matchmakingPanel;

    [Header("Fade")]
    public GameObject fadePanel;
    public Image fadeImage;
    public float fadeDuration = 0.25f;

    [Header("Matchmaking UI")]
    public Slider matchmakingProgressSlider;
    public TMP_Text matchmakingStatusText;
    public TMP_Text matchmakingFailedText;
    public Button retryMatchmakingButton;
    public Button cancelMatchmakingButton;
    public float matchmakingDuration = 3f;
    public bool simulateMatchFailure = false;

    [Header("Character Select UI")]
    public TMP_Text selectedRoleText;
    public Button hunterSelectButton;
    public Button survivorSelectButton;
    public Button confirmRoleButton;
    public Button backToLobbyButton;

    [Header("Core References")]
    public LobbyUIManager lobbyUIManager;
    public RoleSwitchController roleSwitchController;
    public GameHUDManager gameHUDManager;

    [Header("Debug")]
    public bool logFlow = true;

    private PlayableRole selectedRole = PlayableRole.Hunter;
    private bool isFlowRunning = false;

    void Start()
    {
        AutoFindReferences();
        HideFlowPanels();
        SetSelectedRole(PlayableRole.Hunter);
        SetFadeAlpha(0f);

        if (fadePanel != null)
        {
            fadePanel.SetActive(false);
        }

        if (gameHUDManager != null)
        {
            gameHUDManager.SetHUDVisible(false);
        }
    }

    void AutoFindReferences()
    {
        if (lobbyUIManager == null)
        {
            lobbyUIManager = FindFirstObjectByType<LobbyUIManager>();
        }

        if (roleSwitchController == null)
        {
            roleSwitchController = FindFirstObjectByType<RoleSwitchController>();
        }

        if (gameHUDManager == null)
        {
            gameHUDManager = FindFirstObjectByType<GameHUDManager>();
        }
    }

    public void OpenCharacterSelect()
    {
        if (isFlowRunning)
        {
            return;
        }

        StartCoroutine(OpenCharacterSelectRoutine());
    }

    IEnumerator OpenCharacterSelectRoutine()
    {
        isFlowRunning = true;

        AutoFindReferences();

        if (logFlow)
        {
            Debug.Log("MatchFlowUIManager: Open Character Select with fade");
        }

        yield return FadeToBlack();

        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(false);
        }

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(true);
        }

        if (matchmakingPanel != null)
        {
            matchmakingPanel.SetActive(false);
        }

        if (gameHUDManager != null)
        {
            gameHUDManager.SetHUDVisible(false);
        }

        SetSelectedRole(selectedRole);
        UnlockCursorForUI();

        yield return FadeFromBlack();

        isFlowRunning = false;
    }

    public void BackToLobbyFromCharacterSelect()
    {
        if (isFlowRunning)
        {
            return;
        }

        StartCoroutine(BackToLobbyFromCharacterSelectRoutine());
    }

    IEnumerator BackToLobbyFromCharacterSelectRoutine()
    {
        isFlowRunning = true;

        if (logFlow)
        {
            Debug.Log("MatchFlowUIManager: Back To Lobby with fade");
        }

        yield return FadeToBlack();

        HideFlowPanels();

        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(true);
        }

        if (lobbyUIManager != null)
        {
            lobbyUIManager.ShowHome();
        }

        if (gameHUDManager != null)
        {
            gameHUDManager.SetHUDVisible(false);
        }

        UnlockCursorForUI();

        yield return FadeFromBlack();

        isFlowRunning = false;
    }

    public void SelectHunterRole()
    {
        SetSelectedRole(PlayableRole.Hunter);
    }

    public void SelectSurvivorRole()
    {
        SetSelectedRole(PlayableRole.Survivor);
    }

    void SetSelectedRole(PlayableRole role)
    {
        selectedRole = role;

        if (selectedRoleText != null)
        {
            selectedRoleText.text = "Selected Role: " + selectedRole.ToString();
        }

        if (hunterSelectButton != null)
        {
            hunterSelectButton.interactable = selectedRole != PlayableRole.Hunter;
        }

        if (survivorSelectButton != null)
        {
            survivorSelectButton.interactable = selectedRole != PlayableRole.Survivor;
        }
    }

    public void ConfirmRoleAndStartMatchmaking()
    {
        if (isFlowRunning)
        {
            return;
        }

        StartCoroutine(MatchmakingRoutine());
    }

    public void RetryMatchmaking()
    {
        if (isFlowRunning)
        {
            return;
        }

        simulateMatchFailure = false;
        StartCoroutine(MatchmakingRoutine());
    }

    public void CancelMatchmaking()
    {
        if (isFlowRunning)
        {
            return;
        }

        if (matchmakingPanel != null)
        {
            matchmakingPanel.SetActive(false);
        }

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(true);
        }

        UnlockCursorForUI();
    }

    IEnumerator MatchmakingRoutine()
    {
        isFlowRunning = true;

        if (logFlow)
        {
            Debug.Log("MatchFlowUIManager: Start Matchmaking. Role = " + selectedRole);
        }

        yield return FadeToBlack();

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }

        if (matchmakingPanel != null)
        {
            matchmakingPanel.SetActive(true);
        }

        SetMatchmakingFailedUI(false);

        if (matchmakingProgressSlider != null)
        {
            matchmakingProgressSlider.value = 0f;
        }

        if (matchmakingStatusText != null)
        {
            matchmakingStatusText.text = "Searching for match...";
        }

        yield return FadeFromBlack();

        float timer = 0f;

        while (timer < matchmakingDuration)
        {
            timer += Time.deltaTime;
            float progress01 = Mathf.Clamp01(timer / matchmakingDuration);

            if (matchmakingProgressSlider != null)
            {
                matchmakingProgressSlider.value = progress01;
            }

            if (matchmakingStatusText != null)
            {
                int percent = Mathf.RoundToInt(progress01 * 100f);
                matchmakingStatusText.text = "Matching... " + percent + "%";
            }

            yield return null;
        }

        if (simulateMatchFailure)
        {
            ShowMatchmakingFailed();
            isFlowRunning = false;
            yield break;
        }

        if (matchmakingStatusText != null)
        {
            matchmakingStatusText.text = "Match found.";
        }

        yield return new WaitForSeconds(0.4f);

        yield return FadeToBlack();

        BeginGame();

        yield return FadeFromBlack();

        isFlowRunning = false;
    }

    void ShowMatchmakingFailed()
    {
        if (matchmakingStatusText != null)
        {
            matchmakingStatusText.text = "Matchmaking failed.";
        }

        SetMatchmakingFailedUI(true);
    }

    void SetMatchmakingFailedUI(bool failed)
    {
        if (matchmakingFailedText != null)
        {
            matchmakingFailedText.gameObject.SetActive(failed);
            matchmakingFailedText.text = failed ? "Match failed. Please retry or return to role selection." : "";
        }

        if (retryMatchmakingButton != null)
        {
            retryMatchmakingButton.gameObject.SetActive(failed);
        }

        if (cancelMatchmakingButton != null)
        {
            cancelMatchmakingButton.gameObject.SetActive(failed);
        }
    }

    void BeginGame()
    {
        if (logFlow)
        {
            Debug.Log("MatchFlowUIManager: Begin Game as " + selectedRole);
        }

        HideFlowPanels();

        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(false);
        }

        if (roleSwitchController != null)
        {
            if (selectedRole == PlayableRole.Hunter)
            {
                roleSwitchController.SelectHunter();
            }
            else
            {
                roleSwitchController.SelectSurvivor();
            }
        }

        if (lobbyUIManager != null)
        {
            lobbyUIManager.BeginGameplayWithoutReload();
        }

        if (gameHUDManager != null)
        {
            gameHUDManager.SetHUDVisible(true);
        }

        LockCursorForGameplay();
    }

    void HideFlowPanels()
    {
        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }

        if (matchmakingPanel != null)
        {
            matchmakingPanel.SetActive(false);
        }
    }

    IEnumerator FadeToBlack()
    {
        if (fadePanel == null || fadeImage == null)
        {
            yield break;
        }

        fadePanel.SetActive(true);

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

    void UnlockCursorForUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LockCursorForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}