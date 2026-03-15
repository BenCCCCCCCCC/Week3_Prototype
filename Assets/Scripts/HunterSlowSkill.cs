using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HunterSlowSkill : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public SkillStatsSO skillStats;
    public Camera hunterCamera;
    public Text cooldownText;
    public Text aimHintText;

    [Header("Other")]
    public string targetTag = "Survivor";

    private InputAction qAction;
    private InputAction fireAction;

    private float cooldownTimer = 0f;
    private bool isAiming = false;
    public bool IsAiming => isAiming;
    private float aimTimer = 0f;

    private bool hasBeenUsed = false;
    private float readyTimer = 0f;
    private bool readyShownThisCycle = false;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        qAction = new InputAction(
            name: "HunterSlowAim",
            type: InputActionType.Button,
            binding: "<Keyboard>/q"
        );

        fireAction = new InputAction(
            name: "HunterSlowFire",
            type: InputActionType.Button,
            binding: "<Mouse>/leftButton"
        );
    }

    void OnEnable()
    {
        qAction.Enable();
        fireAction.Enable();
        HideCooldownText();
        HideAimHintText();
    }

    void OnDisable()
    {
        qAction.Disable();
        fireAction.Disable();
    }

    void Update()
    {
        if (skillStats == null) return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f)
            {
                cooldownTimer = 0f;
            }
        }

        if (readyTimer > 0f)
        {
            readyTimer -= Time.deltaTime;
            if (readyTimer <= 0f)
            {
                HideCooldownText();
            }
        }

        UpdateUI();

        if (controller == null) return;
        if (!controller.enablePlayerInput) return;

        if (qAction.WasPressedThisFrame() && cooldownTimer <= 0f && !isAiming)
        {
            isAiming = true;
            aimTimer = skillStats.slowAimWindow;
            ShowAimHintText();
            Debug.Log("Hunter Slow Skill: Aim mode started.");
        }

        if (isAiming)
        {
            aimTimer -= Time.deltaTime;

            if (fireAction.WasPressedThisFrame())
            {
                FireSlowRay();
            }

            if (aimTimer <= 0f)
            {
                isAiming = false;
                HideAimHintText();

                hasBeenUsed = true;
                readyShownThisCycle = false;
                readyTimer = 0f;
                cooldownTimer = skillStats.slowMissCooldown;

                Debug.Log("Hunter Slow Skill: Missed window, skill on miss cooldown.");
            }
        }
    }

    void FireSlowRay()
    {
        isAiming = false;
        HideAimHintText();

        if (hunterCamera == null)
        {
            Debug.LogWarning("HunterSlowSkill: hunterCamera is not assigned.");
            hasBeenUsed = true;
            readyShownThisCycle = false;
            readyTimer = 0f;
            cooldownTimer = skillStats.slowMissCooldown;
            return;
        }

        Ray ray = hunterCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool hitSuccess = false;

        Debug.DrawRay(ray.origin, ray.direction * skillStats.slowRange, Color.red, 1.5f);

        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            skillStats.slowCastRadius,
            skillStats.slowRange,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0)
        {
            Debug.Log("Hunter Slow Skill: Ray hit nothing.");
            hasBeenUsed = true;
            readyShownThisCycle = false;
            readyTimer = 0f;
            cooldownTimer = skillStats.slowMissCooldown;
            return;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Transform root = hit.collider.transform.root;
            CharacterStatus targetStatus = root.GetComponent<CharacterStatus>();

            Debug.Log("Hit collider: " + hit.collider.name);
            Debug.Log("Hit root object: " + root.name);
            Debug.Log("Hit root tag: " + root.tag);

            if (targetStatus != null && root.CompareTag(targetTag))
            {
                hitSuccess = targetStatus.ApplySlow(skillStats.slowMultiplier, skillStats.slowDuration);

                if (hitSuccess)
                {
                    Debug.Log("Hunter Slow Skill: Survivor slowed successfully.");
                }
                else
                {
                    Debug.Log("Hunter Slow Skill: Hit Survivor but slow was blocked.");
                }

                break;
            }

            if (!hit.collider.isTrigger)
            {
                Debug.Log("Hunter Slow Skill: Shot was blocked by " + root.name);
                break;
            }
        }

        hasBeenUsed = true;
        readyShownThisCycle = false;
        readyTimer = 0f;
        cooldownTimer = hitSuccess ? skillStats.slowHitCooldown : skillStats.slowMissCooldown;
    }

    void UpdateUI()
    {
        UpdateCooldownText();
        UpdateAimHintText();
    }

    void UpdateCooldownText()
    {
        if (cooldownText == null) return;

        if (!hasBeenUsed)
        {
            HideCooldownText();
            return;
        }

        if (cooldownTimer > 0f)
        {
            ShowCooldownText();
            cooldownText.text = "Slow Q: " + cooldownTimer.ToString("F1");
            return;
        }

        if (!readyShownThisCycle && !isAiming)
        {
            readyShownThisCycle = true;
            readyTimer = skillStats.slowReadyShowDuration;
            ShowCooldownText();
            cooldownText.text = "Slow Q: Ready";
        }
    }

    void UpdateAimHintText()
    {
        if (aimHintText == null) return;

        if (isAiming)
        {
            ShowAimHintText();
            aimHintText.text = "Aim Mode: Left Click to Fire";
        }
        else
        {
            HideAimHintText();
        }
    }

    void ShowCooldownText()
    {
        if (cooldownText != null)
        {
            cooldownText.enabled = true;
        }
    }

    void HideCooldownText()
    {
        if (cooldownText != null)
        {
            cooldownText.text = "";
            cooldownText.enabled = false;
        }
    }

    void ShowAimHintText()
    {
        if (aimHintText != null)
        {
            aimHintText.enabled = true;
        }
    }

    void HideAimHintText()
    {
        if (aimHintText != null)
        {
            aimHintText.text = "";
            aimHintText.enabled = false;
        }
    }
}
