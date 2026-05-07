using System.Text;
using TMPro;
using UnityEngine;

public class GameHUDManager : MonoBehaviour
{
    [Header("HUD Roots")]
    public GameObject survivorHUDRoot;
    public GameObject hunterHUDRoot;

    [Header("Core References")]
    public RoleSwitchController roleSwitchController;
    public MatchManager matchManager;

    [Header("Survivor References")]
    public CharacterStatus playerSurvivorStatus;
    public SurvivorDashSkill survivorDashSkill;
    public InteractionUI playerSurvivorInteractionUI;

    [Header("Hunter References")]
    public HunterBasicAttack hunterBasicAttack;
    public HunterSlowSkill hunterSlowSkill;
    public HunterDetectSkill hunterDetectSkill;

    [Header("Tracked Survivors")]
    public CharacterStatus[] trackedSurvivorStatuses;
    public InteractionUI[] trackedSurvivorInteractionUIs;

    [Header("Survivor HUD Texts")]
    public TMP_Text survivorObjectiveText;
    public TMP_Text survivorCipherText;
    public TMP_Text survivorSelfStatusText;
    public TMP_Text survivorTeamStatusText;
    public TMP_Text survivorDashText;
    public TMP_Text survivorStateHintText;
    public TMP_Text survivorEventFeedbackText;

    [Header("Hunter HUD Texts")]
    public TMP_Text hunterObjectiveText;
    public TMP_Text hunterCipherText;
    public TMP_Text hunterRepairInfoText;
    public TMP_Text hunterSurvivorSignalText;
    public TMP_Text hunterAttackText;
    public TMP_Text hunterSlowText;
    public TMP_Text hunterDetectText;
    public TMP_Text hunterHintText;
    public TMP_Text hunterEventFeedbackText;

    [Header("Settings")]
    public bool showHUD = false;
    public float refreshInterval = 0.1f;
    public float feedbackVisibleTime = 2f;

    private float refreshTimer = 0f;

    private bool feedbackStateInitialized = false;
    private int previousCompletedCipherCount = 0;
    private bool previousGateUnlocked = false;
    private int previousSelfHP = -1;
    private string previousSelfState = "";
    private string[] previousSurvivorStateKeys = new string[0];

    private float survivorFeedbackTimer = 0f;
    private float hunterFeedbackTimer = 0f;

    void Start()
    {
        AutoFindReferences();

        bool startVisible = showHUD;
        SetHUDVisible(startVisible);
    }

    void Update()
    {
        if (!showHUD)
        {
            return;
        }

        UpdateFeedbackTimers();

        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            RefreshHUD();
        }
    }

    public void SetHUDVisible(bool visible)
    {
        showHUD = visible;

        if (!visible)
        {
            if (survivorHUDRoot != null)
            {
                survivorHUDRoot.SetActive(false);
            }

            if (hunterHUDRoot != null)
            {
                hunterHUDRoot.SetActive(false);
            }

            ClearFeedbackTexts();
            return;
        }

        refreshTimer = 0f;
        RefreshHUD();
    }

    public void RefreshHUD()
    {
        AutoFindReferences();
        DetectImportantFeedbackEvents();

        if (roleSwitchController == null)
        {
            SetBothHUDInactive();
            return;
        }

        if (roleSwitchController.IsSurvivorActive())
        {
            ShowSurvivorHUD();
        }
        else
        {
            ShowHunterHUD();
        }
    }

    void ShowSurvivorHUD()
    {
        if (survivorHUDRoot != null)
        {
            survivorHUDRoot.SetActive(true);
        }

        if (hunterHUDRoot != null)
        {
            hunterHUDRoot.SetActive(false);
        }

        UpdateSurvivorHUD();
    }

    void ShowHunterHUD()
    {
        if (survivorHUDRoot != null)
        {
            survivorHUDRoot.SetActive(false);
        }

        if (hunterHUDRoot != null)
        {
            hunterHUDRoot.SetActive(true);
        }

        UpdateHunterHUD();
    }

    void SetBothHUDInactive()
    {
        if (survivorHUDRoot != null)
        {
            survivorHUDRoot.SetActive(false);
        }

        if (hunterHUDRoot != null)
        {
            hunterHUDRoot.SetActive(false);
        }
    }

    void UpdateSurvivorHUD()
    {
        if (survivorObjectiveText != null)
        {
            if (matchManager != null && matchManager.GatesUnlocked)
            {
                survivorObjectiveText.text = "L1 Objective: Open the gate and escape";
            }
            else
            {
                survivorObjectiveText.text = "L1 Objective: Repair ciphers";
            }
        }

        if (survivorCipherText != null)
        {
            survivorCipherText.text = "L1 Ciphers: " + GetCipherProgressText();
        }


        if (survivorSelfStatusText != null)
        {
            survivorSelfStatusText.text = "L1 Your Status: " + GetStatusText(playerSurvivorStatus);
        }

        if (survivorTeamStatusText != null)
        {
            survivorTeamStatusText.text = "L1 Team Status:\n" + GetTeamStatusText();
        }

        if (survivorDashText != null)
        {
            survivorDashText.text = "L2 Dash: " + GetDashText();
        }

        if (survivorStateHintText != null)
        {
            survivorStateHintText.text = GetSurvivorHintText();
        }
    }

    void UpdateHunterHUD()
    {
        if (hunterObjectiveText != null)
        {
            hunterObjectiveText.text = "L1 Objective: Stop survivors from escaping";
        }

        if (hunterCipherText != null)
        {
            hunterCipherText.text = "L1 Cipher Pressure: " + GetCipherProgressText();
        }
        if (hunterRepairInfoText != null)
        {
            hunterRepairInfoText.text = GetActiveRepairTextForHunter();
        }
        if (hunterSurvivorSignalText != null)
        {
            hunterSurvivorSignalText.text = "L1 Survivor Signals:\n" + GetTeamStatusText();
        }

        if (hunterAttackText != null)
        {
            hunterAttackText.text = "L1 Attack: " + GetAttackText();
        }

        if (hunterSlowText != null)
        {
            hunterSlowText.text = "L2 Slow Ray: " + GetSlowText();
        }

        if (hunterDetectText != null)
        {
            hunterDetectText.text = "L2 Detect: " + GetDetectText();
        }

        if (hunterHintText != null)
        {
            hunterHintText.text = GetHunterHintText();
        }
    }

    string GetCipherProgressText()
    {
        if (matchManager == null)
        {
            return "0 / ? | 0%";
        }

        int completed = matchManager.CompletedCipherCount;
        int required = Mathf.Max(1, matchManager.requiredCompletedCiphers);
        int percent = Mathf.RoundToInt(GetCipherOverallProgress01() * 100f);

        return completed + " / " + required + " | " + percent + "%";
    }

    float GetCipherOverallProgress01()
    {
        if (matchManager == null)
        {
            return 0f;
        }

        int required = Mathf.Max(1, matchManager.requiredCompletedCiphers);

        if (matchManager.ciphers == null || matchManager.ciphers.Length == 0)
        {
            return Mathf.Clamp01((float)matchManager.CompletedCipherCount / required);
        }

        float totalProgress = 0f;

        for (int i = 0; i < matchManager.ciphers.Length; i++)
        {
            CipherMachine cipher = matchManager.ciphers[i];

            if (cipher == null)
            {
                continue;
            }

            totalProgress += Mathf.Clamp01(cipher.progress01);
        }

        float progress01 = totalProgress / required;
        return Mathf.Clamp01(progress01);
    }

    string GetActiveRepairTextForHunter()
    {
        if (trackedSurvivorInteractionUIs == null || trackedSurvivorInteractionUIs.Length == 0)
        {
            return "L1 Repair Signal: No active repair";
        }

        StringBuilder builder = new StringBuilder();
        bool hasActiveRepair = false;

        builder.Append("L1 Repair Signal:");

        for (int i = 0; i < trackedSurvivorInteractionUIs.Length; i++)
        {
            InteractionUI ui = trackedSurvivorInteractionUIs[i];

            if (ui == null)
            {
                continue;
            }

            if (!ui.IsInteractingRepair)
            {
                continue;
            }

            CipherMachine cipher = ui.CurrentRepairCipher;

            if (cipher == null)
            {
                continue;
            }

            hasActiveRepair = true;

            builder.Append("\nS");
            builder.Append(i + 1);
            builder.Append(" repairing ");
            builder.Append(GetCleanCipherName(cipher.name));
            builder.Append(" | ");
            builder.Append(Mathf.RoundToInt(cipher.progress01 * 100f));
            builder.Append("%");
        }

        if (!hasActiveRepair)
        {
            return "L1 Repair Signal: No active repair";
        }

        return builder.ToString();
    }

    string GetCleanCipherName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
        {
            return "Unknown Cipher";
        }

        return rawName.Replace("(Clone)", "").Trim();
    }
    string GetTeamStatusText()
    {
        if (trackedSurvivorStatuses == null || trackedSurvivorStatuses.Length == 0)
        {
            return "No survivor data";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < trackedSurvivorStatuses.Length; i++)
        {
            CharacterStatus status = trackedSurvivorStatuses[i];

            builder.Append("S");
            builder.Append(i + 1);
            builder.Append(": ");

            if (status == null)
            {
                builder.Append("Unknown");
            }
            else
            {
                builder.Append(GetStatusText(status));
            }

            if (i < trackedSurvivorStatuses.Length - 1)
            {
                builder.Append("\n");
            }
        }

        return builder.ToString();
    }



    string GetStatusText(CharacterStatus status)
    {
        if (status == null)
        {
            return "Unknown";
        }

        string stateKey = GetStatusKey(status);

        if (stateKey == "Escaped")
        {
            return "Escaped";
        }

        if (stateKey == "Eliminated")
        {
            return "Eliminated";
        }

        if (stateKey == "BeingRescued")
        {
            return "Being Rescued";
        }

        if (stateKey == "OnChair")
        {
            return "On Chair";
        }

        if (stateKey == "Carried")
        {
            return "Carried";
        }

        if (stateKey == "Downed")
        {
            return "Downed";
        }

        if (stateKey == "HitStun")
        {
            return "Hit Stun";
        }

        if (stateKey == "Slowed")
        {
            return "Slowed";
        }

        if (stateKey == "Injured")
        {
            return "Injured HP " + status.currentHP;
        }

        return "Normal HP " + status.currentHP;
    }

    string GetStatusKey(CharacterStatus status)
    {
        if (status == null)
        {
            return "Unknown";
        }

        if (status.IsEscaped)
        {
            return "Escaped";
        }

        if (status.IsEliminated)
        {
            return "Eliminated";
        }

        if (status.IsChaired && AnyRescueInteractionInProgress())
        {
            return "BeingRescued";
        }

        if (status.IsChaired)
        {
            return "OnChair";
        }

        if (status.IsCarried)
        {
            return "Carried";
        }

        if (status.IsDowned)
        {
            return "Downed";
        }

        if (status.IsHitStunned)
        {
            return "HitStun";
        }

        if (status.IsSlowed)
        {
            return "Slowed";
        }

        if (status.IsInjured)
        {
            return "Injured";
        }

        return "Normal";
    }

    bool AnyRescueInteractionInProgress()
    {
        if (trackedSurvivorInteractionUIs == null || trackedSurvivorInteractionUIs.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < trackedSurvivorInteractionUIs.Length; i++)
        {
            InteractionUI ui = trackedSurvivorInteractionUIs[i];

            if (ui != null && ui.IsInteractingRescue)
            {
                return true;
            }
        }

        return false;
    }

    string GetDashText()
    {
        if (survivorDashSkill == null)
        {
            return "Not Available";
        }

        if (survivorDashSkill.IsDashing)
        {
            return "Dashing";
        }

        if (survivorDashSkill.CooldownTimer > 0f)
        {
            return survivorDashSkill.CooldownTimer.ToString("F1") + "s";
        }

        return "Ready";
    }

    string GetAttackText()
    {
        if (hunterBasicAttack == null)
        {
            return "Not Available";
        }

        if (hunterBasicAttack.IsAttacking)
        {
            return "Attacking";
        }

        if (hunterBasicAttack.CooldownTimer > 0f)
        {
            return hunterBasicAttack.CooldownTimer.ToString("F1") + "s";
        }

        return "Ready";
    }

    string GetSlowText()
    {
        if (hunterSlowSkill == null)
        {
            return "Not Available";
        }

        if (hunterSlowSkill.IsAiming)
        {
            return "Aiming";
        }

        if (hunterSlowSkill.CooldownTimer > 0f)
        {
            return hunterSlowSkill.CooldownTimer.ToString("F1") + "s";
        }

        return "Ready";
    }

    string GetDetectText()
    {
        if (hunterDetectSkill == null)
        {
            return "Not Available";
        }

        if (hunterDetectSkill.IsDetecting)
        {
            return "Scanning";
        }

        if (hunterDetectSkill.CooldownTimer > 0f)
        {
            return hunterDetectSkill.CooldownTimer.ToString("F1") + "s";
        }

        return "Ready";
    }

    string GetSurvivorHintText()
    {
        if (playerSurvivorStatus == null)
        {
            return "L2 Hint: Find a cipher and hold E to interact";
        }

        string stateKey = GetStatusKey(playerSurvivorStatus);

        if (stateKey == "Downed")
        {
            return "L1 Critical: You are downed. Wait for rescue.";
        }

        if (stateKey == "BeingRescued")
        {
            return "L1 Critical: Teammate is rescuing you.";
        }

        if (stateKey == "Carried")
        {
            return "L1 Critical: You are being carried by the Hunter.";
        }

        if (stateKey == "OnChair")
        {
            return "L1 Critical: You are on chair. Teammate rescue needed.";
        }

        if (matchManager != null && matchManager.GatesUnlocked)
        {
            return "L1 Hint: Gate is unlocked. Find the exit.";
        }

        return "L2 Hint: Repair ciphers, avoid Hunter, rescue teammates when safe.";
    }

    string GetHunterHintText()
    {
        if (matchManager != null && matchManager.GatesUnlocked)
        {
            return "L1 Alert: Gate unlocked. Defend the exit.";
        }

        if (hunterDetectSkill != null && hunterDetectSkill.IsDetecting)
        {
            return "L2 Hint: Follow the detection signal.";
        }

        if (hunterSlowSkill != null && hunterSlowSkill.IsAiming)
        {
            return "L2 Hint: Left click to fire Slow Ray.";
        }

        return "L2 Hint: Track survivors, interrupt repair, secure downed targets.";
    }

    void DetectImportantFeedbackEvents()
    {
        if (!feedbackStateInitialized)
        {
            SaveCurrentFeedbackState();
            feedbackStateInitialized = true;
            return;
        }

        int currentCompletedCipherCount = 0;
        bool currentGateUnlocked = false;

        if (matchManager != null)
        {
            currentCompletedCipherCount = matchManager.CompletedCipherCount;
            currentGateUnlocked = matchManager.GatesUnlocked;
        }

        if (currentCompletedCipherCount > previousCompletedCipherCount)
        {
            ShowFeedbackBoth("L1 Event: Cipher completed");
        }

        if (currentGateUnlocked && !previousGateUnlocked)
        {
            ShowFeedbackBoth("L1 Event: Gate unlocked");
        }

        if (playerSurvivorStatus != null)
        {
            string currentSelfState = GetStatusKey(playerSurvivorStatus);

            if (previousSelfHP >= 0 && playerSurvivorStatus.currentHP < previousSelfHP)
            {
                ShowSurvivorFeedback("L1 Critical: You were hit. HP " + playerSurvivorStatus.currentHP);
            }

            if (currentSelfState != previousSelfState)
            {
                if (currentSelfState == "Downed")
                {
                    ShowSurvivorFeedback("L1 Critical: You are downed");
                }
                else if (currentSelfState == "BeingRescued")
                {
                    ShowSurvivorFeedback("L1 Critical: You are being rescued");
                }
                else if (currentSelfState == "OnChair")
                {
                    ShowSurvivorFeedback("L1 Critical: You are on chair");
                }
            }
        }

        if (trackedSurvivorStatuses != null)
        {
            EnsurePreviousSurvivorStateArray();

            for (int i = 0; i < trackedSurvivorStatuses.Length; i++)
            {
                CharacterStatus status = trackedSurvivorStatuses[i];
                string currentState = GetStatusKey(status);
                string previousState = previousSurvivorStateKeys[i];

                if (currentState != previousState)
                {
                    string label = "S" + (i + 1);

                    if (currentState == "Downed")
                    {
                        ShowSurvivorFeedback("L1 Event: " + label + " downed");
                        ShowHunterFeedback("L1 Signal: " + label + " downed");
                    }
                    else if (currentState == "BeingRescued")
                    {
                        ShowSurvivorFeedback("L1 Event: " + label + " being rescued");
                        ShowHunterFeedback("L2 Signal: " + label + " rescue in progress");
                    }
                    else if (currentState == "OnChair")
                    {
                        ShowSurvivorFeedback("L1 Event: " + label + " on chair");
                        ShowHunterFeedback("L1 Signal: " + label + " secured on chair");
                    }
                    else if (currentState == "Eliminated")
                    {
                        ShowFeedbackBoth("L1 Event: " + label + " eliminated");
                    }
                    else if (currentState == "Escaped")
                    {
                        ShowFeedbackBoth("L1 Event: " + label + " escaped");
                    }
                }
            }
        }

        SaveCurrentFeedbackState();
    }

    void SaveCurrentFeedbackState()
    {
        if (matchManager != null)
        {
            previousCompletedCipherCount = matchManager.CompletedCipherCount;
            previousGateUnlocked = matchManager.GatesUnlocked;
        }

        if (playerSurvivorStatus != null)
        {
            previousSelfHP = playerSurvivorStatus.currentHP;
            previousSelfState = GetStatusKey(playerSurvivorStatus);
        }

        EnsurePreviousSurvivorStateArray();

        if (trackedSurvivorStatuses != null)
        {
            for (int i = 0; i < trackedSurvivorStatuses.Length; i++)
            {
                previousSurvivorStateKeys[i] = GetStatusKey(trackedSurvivorStatuses[i]);
            }
        }
    }

    void EnsurePreviousSurvivorStateArray()
    {
        int targetLength = 0;

        if (trackedSurvivorStatuses != null)
        {
            targetLength = trackedSurvivorStatuses.Length;
        }

        if (previousSurvivorStateKeys == null || previousSurvivorStateKeys.Length != targetLength)
        {
            previousSurvivorStateKeys = new string[targetLength];

            for (int i = 0; i < previousSurvivorStateKeys.Length; i++)
            {
                previousSurvivorStateKeys[i] = "";
            }
        }
    }

    void ShowFeedbackBoth(string message)
    {
        ShowSurvivorFeedback(message);
        ShowHunterFeedback(message);
    }

    void ShowSurvivorFeedback(string message)
    {
        if (survivorEventFeedbackText == null)
        {
            return;
        }

        survivorEventFeedbackText.gameObject.SetActive(true);
        survivorEventFeedbackText.text = message;
        survivorFeedbackTimer = feedbackVisibleTime;
    }

    void ShowHunterFeedback(string message)
    {
        if (hunterEventFeedbackText == null)
        {
            return;
        }

        hunterEventFeedbackText.gameObject.SetActive(true);
        hunterEventFeedbackText.text = message;
        hunterFeedbackTimer = feedbackVisibleTime;
    }

    void UpdateFeedbackTimers()
    {
        if (survivorFeedbackTimer > 0f)
        {
            survivorFeedbackTimer -= Time.deltaTime;

            if (survivorFeedbackTimer <= 0f && survivorEventFeedbackText != null)
            {
                survivorEventFeedbackText.text = "";
                survivorEventFeedbackText.gameObject.SetActive(false);
            }
        }

        if (hunterFeedbackTimer > 0f)
        {
            hunterFeedbackTimer -= Time.deltaTime;

            if (hunterFeedbackTimer <= 0f && hunterEventFeedbackText != null)
            {
                hunterEventFeedbackText.text = "";
                hunterEventFeedbackText.gameObject.SetActive(false);
            }
        }
    }

    void ClearFeedbackTexts()
    {
        survivorFeedbackTimer = 0f;
        hunterFeedbackTimer = 0f;

        if (survivorEventFeedbackText != null)
        {
            survivorEventFeedbackText.text = "";
            survivorEventFeedbackText.gameObject.SetActive(false);
        }

        if (hunterEventFeedbackText != null)
        {
            hunterEventFeedbackText.text = "";
            hunterEventFeedbackText.gameObject.SetActive(false);
        }
    }

    void AutoFindReferences()
    {
        if (roleSwitchController == null)
        {
            roleSwitchController = FindFirstObjectByType<RoleSwitchController>();
        }

        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        if (survivorDashSkill == null)
        {
            survivorDashSkill = FindFirstObjectByType<SurvivorDashSkill>();
        }

        if (hunterBasicAttack == null)
        {
            hunterBasicAttack = FindFirstObjectByType<HunterBasicAttack>();
        }

        if (hunterSlowSkill == null)
        {
            hunterSlowSkill = FindFirstObjectByType<HunterSlowSkill>();
        }

        if (hunterDetectSkill == null)
        {
            hunterDetectSkill = FindFirstObjectByType<HunterDetectSkill>();
        }

        if (playerSurvivorStatus == null && roleSwitchController != null && roleSwitchController.survivorController != null)
        {
            playerSurvivorStatus = roleSwitchController.survivorController.GetComponent<CharacterStatus>();
        }

        if (playerSurvivorInteractionUI == null && roleSwitchController != null && roleSwitchController.survivorController != null)
        {
            playerSurvivorInteractionUI = roleSwitchController.survivorController.GetComponent<InteractionUI>();
        }

        if (trackedSurvivorInteractionUIs == null || trackedSurvivorInteractionUIs.Length == 0)
        {
            trackedSurvivorInteractionUIs = FindObjectsByType<InteractionUI>(FindObjectsSortMode.None);
        }
    }
}