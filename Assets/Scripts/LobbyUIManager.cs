using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Panel Roots")]
    public GameObject lobbyRoot;
    public GameObject lobbyHomePanel;
    public GameObject taskPanel;
    public GameObject loadoutPanel;
    public GameObject archivePanel;

    [Header("Header Texts")]
    public TMP_Text softCurrencyText;
    public TMP_Text premiumCurrencyText;
    public TMP_Text materialText;
    public TMP_Text archiveLevelText;

    [Header("Home Info")]
    public TMP_Text recentMatchText;

    [Header("Fade")]
    public GameObject fadePanel;
    public Image fadeImage;
    public float fadeDuration = 0.25f;

    private bool isReturningToLobby = false;

    [Header("Other References")]
    public MatchSettlement matchSettlement;
    public ResultPanelUI resultPanelUI;
    public TaskPanelUI taskPanelUI;
    public LoadoutPanelUI loadoutPanelUI;
    public ArchivePanelUI archivePanelUI;
    public GameHUDManager gameHUDManager;
    public MatchManager matchManager;

    [Header("Gameplay References")]
    public RoleSwitchController roleSwitchController;
    public PlayerController hunterController;
    public PlayerController survivorPlayerController;
    public InteractionUI hunterInteractionUI;
    public InteractionUI survivorInteractionUI;
    public HunterSlowSkill hunterSlowSkill;
    public HunterDetectSkill hunterDetectSkill;
    public HunterBasicAttack hunterBasicAttack;
    public HunterCarryController hunterCarryController;
    public SurvivorDashSkill survivorDashSkill;

    private static bool startMatchImmediately = false;

    private void Start()
    {
        if (startMatchImmediately)
        {
            startMatchImmediately = false;
            StartGameplayMode();
            return;
        }

        ShowHome();
        RefreshAll();
        EnterLobbyMode();
    }

    public void ShowHome()
    {
        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(true);
        }

        SetOnlyActive(lobbyHomePanel);
        RefreshAll();
    }

    public void ShowTasks()
    {
        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(true);
        }

        SetOnlyActive(taskPanel);
        RefreshAll();
    }

    public void ShowLoadout()
    {
        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(true);
        }

        SetOnlyActive(loadoutPanel);
        RefreshAll();
    }

    public void ShowArchive()
    {
        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(true);
        }

        SetOnlyActive(archivePanel);
        RefreshAll();
    }

    public void OpenLobbyAfterMatch()
    {
        ReturnToLobbyWithoutReload();
    }

    public void StartMatchFromLobby()
    {
        startMatchImmediately = true;
        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void BeginGameplayWithoutReload()
    {
        StartGameplayMode();
    }

    public void ReturnToLobbyWithoutReload()
    {
        if (isReturningToLobby)
        {
            return;
        }

        StartCoroutine(ReturnToLobbyRoutine());
    }

    IEnumerator ReturnToLobbyRoutine()
    {
        isReturningToLobby = true;

        yield return FadeToBlack();

        if (resultPanelUI != null)
        {
            resultPanelUI.HidePanel();
        }

        ShowHome();
        EnterLobbyMode();

        yield return FadeFromBlack();

        isReturningToLobby = false;
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

    void StartGameplayMode()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        if (matchManager != null)
        {
            matchManager.ResetMatchForNewRun();
        }

        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(false);
        }

        SetGameplayEnabled(true);

        if (roleSwitchController != null)
        {
            roleSwitchController.enabled = true;

            if (roleSwitchController.currentRole == PlayableRole.Hunter)
            {
                roleSwitchController.SelectHunter();
            }
            else
            {
                roleSwitchController.SelectSurvivor();
            }
        }

        LockCursorForGameplay();

        if (resultPanelUI != null)
        {
            resultPanelUI.HidePanel();
        }

        if (gameHUDManager != null)
        {
            gameHUDManager.SetHUDVisible(true);
        }
    }

    void EnterLobbyMode()
    {
        if (lobbyRoot != null)
        {
            lobbyRoot.SetActive(true);
        }

        SetGameplayEnabled(false);
        UnlockCursorForUI();

        if (gameHUDManager != null)
        {
            gameHUDManager.SetHUDVisible(false);
        }
    }

    void SetGameplayEnabled(bool enabled)
    {
        if (roleSwitchController != null)
        {
            roleSwitchController.enabled = enabled;
        }

        if (hunterController != null)
        {
            hunterController.SetPlayerInputEnabled(enabled);
        }

        if (survivorPlayerController != null)
        {
            survivorPlayerController.SetPlayerInputEnabled(enabled);
        }

        if (hunterInteractionUI != null)
        {
            hunterInteractionUI.enabled = enabled;
        }

        if (survivorInteractionUI != null)
        {
            survivorInteractionUI.enabled = enabled;
        }

        if (hunterSlowSkill != null)
        {
            hunterSlowSkill.enabled = enabled;
        }

        if (hunterDetectSkill != null)
        {
            hunterDetectSkill.enabled = enabled;
        }

        if (hunterBasicAttack != null)
        {
            hunterBasicAttack.enabled = enabled;
        }

        if (hunterCarryController != null)
        {
            hunterCarryController.enabled = enabled;
        }

        if (survivorDashSkill != null)
        {
            survivorDashSkill.enabled = enabled;
        }
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

    public void RefreshAll()
    {
        RefreshHeaderTexts();
        RefreshRecentMatchText();

        if (taskPanelUI != null)
        {
            taskPanelUI.Refresh();
        }

        if (loadoutPanelUI != null)
        {
            loadoutPanelUI.Refresh();
        }

        if (archivePanelUI != null)
        {
            archivePanelUI.Refresh();
        }
    }

    void RefreshHeaderTexts()
    {
        if (PlayerProfile.Instance != null)
        {
            if (softCurrencyText != null)
            {
                softCurrencyText.text = "Soft: " + PlayerProfile.Instance.softCurrency;
            }

            if (premiumCurrencyText != null)
            {
                premiumCurrencyText.text = "Ticket: " + PlayerProfile.Instance.premiumCurrency;
            }

            if (materialText != null)
            {
                materialText.text = "Material: " + PlayerProfile.Instance.archiveMaterial;
            }
        }

        if (ArchiveProgress.Instance != null && archiveLevelText != null)
        {
            archiveLevelText.text = "Archive Lv." + ArchiveProgress.Instance.archiveLevel;
        }
    }

    void RefreshRecentMatchText()
    {
        if (recentMatchText == null)
        {
            return;
        }

        if (matchSettlement == null || matchSettlement.LastSummary == null)
        {
            recentMatchText.text = "No recent match summary.";
            return;
        }

        SettlementSummary s = matchSettlement.LastSummary;

        recentMatchText.text =
            "Recent Match\n" +
            "Soft +" + s.totalSoft +
            " | Material +" + s.totalMaterial + "\n" +
            "Tasks: " + s.completedTaskText + "\n" +
            "Loadout Extra: Soft +" + s.loadoutSoft + ", Material +" + s.loadoutMaterial;
    }

    void SetOnlyActive(GameObject target)
    {
        if (lobbyHomePanel != null) lobbyHomePanel.SetActive(lobbyHomePanel == target);
        if (taskPanel != null) taskPanel.SetActive(taskPanel == target);
        if (loadoutPanel != null) loadoutPanel.SetActive(loadoutPanel == target);
        if (archivePanel != null) archivePanel.SetActive(archivePanel == target);
    }
}