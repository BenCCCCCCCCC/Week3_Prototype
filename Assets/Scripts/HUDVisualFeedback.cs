using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDVisualFeedback : MonoBehaviour
{
    [Header("Core References")]
    public RoleSwitchController roleSwitchController;
    public CharacterStatus playerSurvivorStatus;

    [Header("Tracked Survivors")]
    public CharacterStatus[] trackedSurvivorStatuses;

    [Header("Survivor Downed Visual Feedback")]
    public Image survivorRedOverlay;
    public float downedOverlayAlpha = 0.45f;

    [Header("Survivor Injured Border Feedback")]
    public Image[] injuredBorderImages;
    public float injuredBorderMinAlpha = 0.12f;
    public float injuredBorderMaxAlpha = 0.28f;
    public float injuredBorderPulseSpeed = 3f;

    [Header("Survivor Hit Flash")]
    public float hitFlashAlpha = 0.28f;
    public float hitFlashDuration = 0.45f;

    [Header("Hunter Visual Feedback")]
    public TMP_Text crosshairText;
    public string normalCrosshairText = "+";
    public string hitMarkerText = "×";
    public Color normalCrosshairColor = Color.white;
    public Color hitMarkerColor = Color.red;
    public float hunterHitMarkerDuration = 0.25f;

    [Header("Debug")]
    public bool autoFindReferences = true;

    private int previousPlayerHP = -1;
    private int[] previousTrackedHP = new int[0];

    private float hitFlashTimer = 0f;
    private float hunterHitMarkerTimer = 0f;

    void Start()
    {
        AutoFindMissingReferences();
        SaveCurrentHPState();
        HideSurvivorOverlay();
        HideInjuredBorders();
        RestoreCrosshair();
    }

    void Update()
    {
        if (autoFindReferences)
        {
            AutoFindMissingReferences();
        }

        DetectHPChanges();
        UpdateSurvivorVisuals();
        UpdateHunterHitMarker();
    }

    void AutoFindMissingReferences()
    {
        if (roleSwitchController == null)
        {
            roleSwitchController = FindFirstObjectByType<RoleSwitchController>();
        }

        if (playerSurvivorStatus == null &&
            roleSwitchController != null &&
            roleSwitchController.survivorController != null)
        {
            playerSurvivorStatus = roleSwitchController.survivorController.GetComponent<CharacterStatus>();
        }
    }

    void DetectHPChanges()
    {
        if (playerSurvivorStatus != null)
        {
            if (previousPlayerHP >= 0 && playerSurvivorStatus.currentHP < previousPlayerHP)
            {
                TriggerSurvivorHitFlash();
            }
        }

        EnsureTrackedHPArray();

        if (trackedSurvivorStatuses != null)
        {
            for (int i = 0; i < trackedSurvivorStatuses.Length; i++)
            {
                CharacterStatus status = trackedSurvivorStatuses[i];

                if (status == null)
                {
                    continue;
                }

                if (previousTrackedHP[i] >= 0 && status.currentHP < previousTrackedHP[i])
                {
                    TriggerHunterHitMarker();
                }
            }
        }

        SaveCurrentHPState();
    }

    void SaveCurrentHPState()
    {
        if (playerSurvivorStatus != null)
        {
            previousPlayerHP = playerSurvivorStatus.currentHP;
        }

        EnsureTrackedHPArray();

        if (trackedSurvivorStatuses != null)
        {
            for (int i = 0; i < trackedSurvivorStatuses.Length; i++)
            {
                if (trackedSurvivorStatuses[i] != null)
                {
                    previousTrackedHP[i] = trackedSurvivorStatuses[i].currentHP;
                }
                else
                {
                    previousTrackedHP[i] = -1;
                }
            }
        }
    }

    void EnsureTrackedHPArray()
    {
        int length = 0;

        if (trackedSurvivorStatuses != null)
        {
            length = trackedSurvivorStatuses.Length;
        }

        if (previousTrackedHP == null || previousTrackedHP.Length != length)
        {
            previousTrackedHP = new int[length];

            for (int i = 0; i < previousTrackedHP.Length; i++)
            {
                previousTrackedHP[i] = -1;
            }
        }
    }

    void TriggerSurvivorHitFlash()
    {
        hitFlashTimer = hitFlashDuration;
    }

    public void TriggerHunterHitMarker()
    {
        hunterHitMarkerTimer = hunterHitMarkerDuration;

        if (crosshairText != null)
        {
            crosshairText.gameObject.SetActive(true);
            crosshairText.text = hitMarkerText;
            crosshairText.color = hitMarkerColor;
        }
    }

    void UpdateSurvivorVisuals()
    {
        bool survivorViewActive = roleSwitchController == null || roleSwitchController.IsSurvivorActive();

        if (!survivorViewActive)
        {
            HideSurvivorOverlay();
            HideInjuredBorders();
            return;
        }

        bool isDownedLikeState = false;
        bool isInjuredState = false;

        if (playerSurvivorStatus != null)
        {
            isDownedLikeState =
                playerSurvivorStatus.IsDowned ||
                playerSurvivorStatus.IsCarried ||
                playerSurvivorStatus.IsChaired;

            isInjuredState =
                playerSurvivorStatus.IsInjured &&
                !isDownedLikeState;
        }

        if (isDownedLikeState)
        {
            SetSurvivorOverlayAlpha(downedOverlayAlpha);
            HideInjuredBorders();
            return;
        }

        HideSurvivorOverlay();

        if (isInjuredState)
        {
            UpdateInjuredBorders();
        }
        else if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;

            float flashProgress = Mathf.Clamp01(hitFlashTimer / hitFlashDuration);
            float alpha = hitFlashAlpha * flashProgress;

            SetInjuredBorderAlpha(alpha);
        }
        else
        {
            HideInjuredBorders();
        }
    }

    void UpdateInjuredBorders()
    {
        float pulse01 = (Mathf.Sin(Time.time * injuredBorderPulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(injuredBorderMinAlpha, injuredBorderMaxAlpha, pulse01);

        SetInjuredBorderAlpha(alpha);
    }

    void SetInjuredBorderAlpha(float alpha)
    {
        if (injuredBorderImages == null)
        {
            return;
        }

        for (int i = 0; i < injuredBorderImages.Length; i++)
        {
            Image image = injuredBorderImages[i];

            if (image == null)
            {
                continue;
            }

            image.gameObject.SetActive(alpha > 0.01f);

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    void HideInjuredBorders()
    {
        SetInjuredBorderAlpha(0f);
    }

    void UpdateHunterHitMarker()
    {
        if (crosshairText == null)
        {
            return;
        }

        bool hunterViewActive = roleSwitchController == null || roleSwitchController.IsHunterActive();

        if (!hunterViewActive)
        {
            RestoreCrosshair();
            return;
        }

        if (hunterHitMarkerTimer > 0f)
        {
            hunterHitMarkerTimer -= Time.deltaTime;

            crosshairText.text = hitMarkerText;
            crosshairText.color = hitMarkerColor;

            if (hunterHitMarkerTimer <= 0f)
            {
                RestoreCrosshair();
            }
        }
    }

    void SetSurvivorOverlayAlpha(float alpha)
    {
        if (survivorRedOverlay == null)
        {
            return;
        }

        Color color = survivorRedOverlay.color;
        color.a = alpha;
        survivorRedOverlay.color = color;

        survivorRedOverlay.gameObject.SetActive(alpha > 0.01f);
    }

    void HideSurvivorOverlay()
    {
        if (survivorRedOverlay == null)
        {
            return;
        }

        Color color = survivorRedOverlay.color;
        color.a = 0f;
        survivorRedOverlay.color = color;
        survivorRedOverlay.gameObject.SetActive(false);
    }

    void RestoreCrosshair()
    {
        if (crosshairText == null)
        {
            return;
        }

        crosshairText.gameObject.SetActive(true);
        crosshairText.text = normalCrosshairText;
        crosshairText.color = normalCrosshairColor;
    }
}