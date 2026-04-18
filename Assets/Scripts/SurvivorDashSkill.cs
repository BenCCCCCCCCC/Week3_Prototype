using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SurvivorDashSkill : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public CharacterStatus status;
    public SkillStatsSO skillStats;
    public Text cooldownText;

    private float cooldownTimer = 0f;
    private bool isDashing = false;
    private bool hasBeenUsed = false;
    private bool showingReady = false;
    private bool hasShownReadyThisCycle = false;
    private float readyTimer = 0f;

    private PlayerLoadout localLoadout;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        if (status == null)
        {
            status = GetComponent<CharacterStatus>();
        }

        localLoadout = GetComponent<PlayerLoadout>();
    }

    void OnEnable()
    {
        HideCooldownUI();
    }

    void OnDisable()
    {
        CancelInvoke(nameof(EndDash));
        HideCooldownUI();
    }

    void Update()
    {
        if (controller == null || controller.playerStats == null || skillStats == null)
        {
            return;
        }

        HandleCooldownTimers();
        UpdateCooldownUI();

        if (!controller.enablePlayerInput)
        {
            return;
        }

        bool fPressed = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;

        if (fPressed && cooldownTimer <= 0f && !isDashing)
        {
            StartDash();
        }
    }

    void HandleCooldownTimers()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f)
            {
                cooldownTimer = 0f;
            }
        }

        if (showingReady)
        {
            readyTimer -= Time.deltaTime;
            if (readyTimer <= 0f)
            {
                showingReady = false;
                HideCooldownUI();
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        hasBeenUsed = true;
        showingReady = false;
        hasShownReadyThisCycle = false;
        readyTimer = 0f;

        cooldownTimer = GetModifiedDashCooldown();

        Vector3 dashDirection = controller.GetCurrentForwardOrMoveDirection();
        float dashSpeed = controller.GetDefaultMoveSpeed() * skillStats.dashMultiplier * GetDashPowerMultiplier();

        controller.SetSpeed(dashSpeed);
        controller.StartForcedMove(dashDirection, dashSpeed);

        if (status != null)
        {
            status.StartInvincible(skillStats.dashInvincibleDuration);
        }

        Invoke(nameof(EndDash), skillStats.dashDuration);
    }

    float GetModifiedDashCooldown()
    {
        float baseCooldown = skillStats.dashCooldown;

        if (localLoadout == null)
        {
            return baseCooldown;
        }

        float multiplier = localLoadout.GetSurvivorDashCooldownMultiplier();
        if (multiplier < 0.01f)
        {
            multiplier = 1f;
        }

        return baseCooldown / multiplier;
    }

    float GetDashPowerMultiplier()
    {
        if (localLoadout == null)
        {
            return 1f;
        }

        float multiplier = localLoadout.GetSurvivorDashPowerMultiplier();
        if (multiplier < 0.01f)
        {
            multiplier = 1f;
        }

        return multiplier;
    }

    void EndDash()
    {
        if (controller != null)
        {
            controller.StopForcedMove();
        }

        if (status != null)
        {
            status.RefreshMoveSpeedFromState();
        }
        else if (controller != null)
        {
            controller.RestoreDefaultSpeed();
        }

        isDashing = false;
    }

    void UpdateCooldownUI()
    {
        if (cooldownText == null)
        {
            return;
        }

        if (!hasBeenUsed)
        {
            HideCooldownUI();
            return;
        }

        if (cooldownTimer > 0f)
        {
            ShowCooldownUI();
            cooldownText.text = "Dash F: " + cooldownTimer.ToString("F1");
            return;
        }

        if (!hasShownReadyThisCycle && !isDashing)
        {
            hasShownReadyThisCycle = true;
            showingReady = true;
            readyTimer = skillStats.dashReadyShowDuration;
            ShowCooldownUI();
            cooldownText.text = "Dash F: Ready";
        }
    }

    void ShowCooldownUI()
    {
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
        }
    }

    void HideCooldownUI()
    {
        if (cooldownText != null)
        {
            cooldownText.text = "";
            cooldownText.gameObject.SetActive(false);
        }
    }
}