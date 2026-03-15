using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HunterDetectSkill : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public SkillStatsSO skillStats;
    public Camera hunterCamera;
    public DetectionArrowUI arrowUI;
    public Text cooldownText;

    [Header("Other")]
    public string targetTag = "Survivor";

    private InputAction detectAction;
    private float cooldownTimer = 0f;
    private bool isDetecting = false;
    private CharacterStatus currentTarget;

    private bool hasBeenUsed = false;
    private float readyTimer = 0f;
    private bool readyShownThisCycle = false;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        detectAction = new InputAction(
            name: "HunterDetect",
            type: InputActionType.Button,
            binding: "<Keyboard>/f"
        );
    }

    void OnEnable()
    {
        detectAction.Enable();
        HideCooldownText();
    }

    void OnDisable()
    {
        detectAction.Disable();
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

        if (detectAction.WasPressedThisFrame() && cooldownTimer <= 0f && !isDetecting)
        {
            hasBeenUsed = true;
            readyShownThisCycle = false;
            readyTimer = 0f;
            StartCoroutine(DetectRoutine());
        }
    }

    IEnumerator DetectRoutine()
    {
        isDetecting = true;
        cooldownTimer = skillStats.detectCooldown;

        float timer = 0f;

        while (timer < skillStats.detectDuration)
        {
            CharacterStatus nearestTarget = FindNearestTarget();
            ApplyDetect(nearestTarget);

            timer += skillStats.detectScanInterval;
            yield return new WaitForSeconds(skillStats.detectScanInterval);
        }

        ClearDetect();
        isDetecting = false;
    }

    CharacterStatus FindNearestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, skillStats.detectMaxRadius, ~0, QueryTriggerInteraction.Ignore);

        CharacterStatus nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            Transform root = hit.transform.root;

            if (!root.CompareTag(targetTag))
            {
                continue;
            }

            CharacterStatus status = root.GetComponent<CharacterStatus>();
            if (status == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, root.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = status;
            }
        }

        return nearest;
    }

    void ApplyDetect(CharacterStatus target)
    {
        if (currentTarget != null && currentTarget != target)
        {
            currentTarget.SetCloseDetectMarker(false);
        }

        currentTarget = target;

        if (currentTarget == null)
        {
            if (arrowUI != null)
            {
                arrowUI.ClearTarget();
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distance <= skillStats.detectCloseRadius)
        {
            currentTarget.SetCloseDetectMarker(true);

            if (arrowUI != null)
            {
                arrowUI.ClearTarget();
            }
        }
        else if (distance <= skillStats.detectMaxRadius)
        {
            currentTarget.SetCloseDetectMarker(false);

            if (arrowUI != null)
            {
                arrowUI.SetTarget(currentTarget.transform, hunterCamera);
            }
        }
        else
        {
            currentTarget.SetCloseDetectMarker(false);

            if (arrowUI != null)
            {
                arrowUI.ClearTarget();
            }
        }
    }

    void ClearDetect()
    {
        if (currentTarget != null)
        {
            currentTarget.SetCloseDetectMarker(false);
        }

        if (arrowUI != null)
        {
            arrowUI.ClearTarget();
        }

        currentTarget = null;
    }

    void UpdateUI()
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
            cooldownText.text = "Detect F: " + cooldownTimer.ToString("F1");
            return;
        }

        if (!readyShownThisCycle && !isDetecting)
        {
            readyShownThisCycle = true;
            readyTimer = skillStats.detectReadyShowDuration;
            ShowCooldownText();
            cooldownText.text = "Detect F: Ready";
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
}
