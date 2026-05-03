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

    [Header("Hunter References")]
    public HunterBasicAttack hunterBasicAttack;
    public HunterSlowSkill hunterSlowSkill;
    public HunterDetectSkill hunterDetectSkill;

    [Header("Tracked Survivors")]
    public CharacterStatus[] trackedSurvivorStatuses;

    [Header("Survivor HUD Texts")]
    public TMP_Text survivorObjectiveText;
    public TMP_Text survivorCipherText;
    public TMP_Text survivorSelfStatusText;
    public TMP_Text survivorTeamStatusText;
    public TMP_Text survivorDashText;
    public TMP_Text survivorStateHintText;

    [Header("Hunter HUD Texts")]
    public TMP_Text hunterObjectiveText;
    public TMP_Text hunterCipherText;
    public TMP_Text hunterSurvivorSignalText;
    public TMP_Text hunterAttackText;
    public TMP_Text hunterSlowText;
    public TMP_Text hunterDetectText;
    public TMP_Text hunterHintText;

    [Header("Settings")]
    public bool showHUD = false;
    public float refreshInterval = 0.1f;

    private float refreshTimer = 0f;

    void Start()
    {
        SetHUDVisible(false);
    }

    void Update()
    {
        if (!showHUD)
        {
            return;
        }

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

            return;
        }

        RefreshHUD();
    }

    public void RefreshHUD()
    {
        if (roleSwitchController == null)
        {
            AutoFindReferences();
        }

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
            return "0 / ?";
        }

        return matchManager.CompletedCipherCount + " / " + matchManager.requiredCompletedCiphers;
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

        if (status.IsEscaped)
        {
            return "Escaped";
        }

        if (status.IsEliminated)
        {
            return "Eliminated";
        }

        if (status.IsChaired)
        {
            return "On Chair";
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
            return "Hit Stun";
        }

        if (status.IsSlowed)
        {
            return "Slowed";
        }

        if (status.IsInjured)
        {
            return "Injured HP " + status.currentHP;
        }

        return "Normal HP " + status.currentHP;
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

        if (playerSurvivorStatus.IsDowned)
        {
            return "L1 Critical: You are downed. Wait for rescue.";
        }

        if (playerSurvivorStatus.IsCarried)
        {
            return "L1 Critical: You are being carried by the Hunter.";
        }

        if (playerSurvivorStatus.IsChaired)
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
    }
}