using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SurvivorDashSkill : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public CharacterStatus status;
    public Text cooldownText;

    [Header("Dash Settings")]
    public float cooldown = 10f;
    public float dashMultiplier = 2.5f;
    public float dashDuration = 1f;
    public float invincibleDuration = 0.5f;

    [Header("UI")]
    public float readyShowDuration = 1f;

    private InputAction dashAction;
    private float cooldownTimer = 0f;
    private bool isDashing = false;

    private bool hasBeenUsed = false;
    private bool showingReady = false;
    private bool hasShownReadyThisCycle = false;
    private float readyTimer = 0f;

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (status == null) status = GetComponent<CharacterStatus>();

        dashAction = new InputAction(
            name: "Dash",
            type: InputActionType.Button,
            binding: "<Keyboard>/f"
        );
    }

    void OnEnable()
    {
        dashAction.Enable();
        HideCooldownUI();
    }

    void OnDisable()
    {
        dashAction.Disable();
        CancelInvoke(nameof(EndDash));
        HideCooldownUI();
    }

    void Update()
    {
        if (controller == null || controller.tuning == null) return;

        HandleCooldownTimers();
        UpdateCooldownUI();

        if (!controller.enablePlayerInput) return;

        if (dashAction.WasPressedThisFrame() && cooldownTimer <= 0f && !isDashing)
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

        cooldownTimer = cooldown;

        Vector3 dashDirection = controller.GetCurrentForwardOrMoveDirection();
        float dashSpeed = controller.GetDefaultMoveSpeed() * dashMultiplier;

        controller.SetSpeed(dashSpeed);
        controller.StartForcedMove(dashDirection, dashSpeed);

        if (status != null)
        {
            status.StartInvincible(invincibleDuration);
        }

        Invoke(nameof(EndDash), dashDuration);
    }

    void EndDash()
    {
        if (controller != null)
        {
            controller.StopForcedMove();
            controller.RestoreDefaultSpeed();
        }

        isDashing = false;
    }

    void UpdateCooldownUI()
    {
        if (cooldownText == null) return;

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

        // 冷却结束后，只显示一次 Ready
        if (!showingReady && !hasShownReadyThisCycle)
        {
            showingReady = true;
            hasShownReadyThisCycle = true;
            readyTimer = readyShowDuration;

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