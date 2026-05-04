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

    [Header("Survivor Visual Feedback")]
    public Image survivorRedOverlay;
    public float hitFlashAlpha = 0.28f;
    public float hitFlashDuration = 0.45f;
    public float downedOverlayAlpha = 0.45f;

    [Header("Hunter Visual Feedback")]
    public TMP_Text hunterHitMarkerText;
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
        HideHunterHitMarker();
    }

    void Update()
    {
        if (autoFindReferences)
        {
            AutoFindMissingReferences();
        }

        DetectHPChanges();
        UpdateSurvivorOverlay();
        UpdateHunterHitMarker();
    }

    void AutoFindMissingReferences()
    {
        if (roleSwitchController == null)
        {
            roleSwitchController = FindFirstObjectByType<RoleSwitchController>();
        }

        if (playerSurvivorStatus == null && roleSwitchController != null && roleSwitchController.survivorController != null)
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

    void TriggerHunterHitMarker()
    {
        hunterHitMarkerTimer = hunterHitMarkerDuration;

        if (hunterHitMarkerText != null)
        {
            hunterHitMarkerText.gameObject.SetActive(true);
            hunterHitMarkerText.text = "X";
        }
    }

    void UpdateSurvivorOverlay()
    {
        if (survivorRedOverlay == null)
        {
            return;
        }

        bool survivorViewActive = roleSwitchController == null || roleSwitchController.IsSurvivorActive();

        if (!survivorViewActive)
        {
            HideSurvivorOverlay();
            return;
        }

        float targetAlpha = 0f;

        if (playerSurvivorStatus != null &&
            (playerSurvivorStatus.IsDowned ||
             playerSurvivorStatus.IsCarried ||
             playerSurvivorStatus.IsChaired))
        {
            targetAlpha = downedOverlayAlpha;
        }
        else if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;

            float flashProgress = Mathf.Clamp01(hitFlashTimer / hitFlashDuration);
            targetAlpha = hitFlashAlpha * flashProgress;
        }

        SetSurvivorOverlayAlpha(targetAlpha);
    }

    void UpdateHunterHitMarker()
    {
        if (hunterHitMarkerText == null)
        {
            return;
        }

        bool hunterViewActive = roleSwitchController == null || roleSwitchController.IsHunterActive();

        if (!hunterViewActive)
        {
            HideHunterHitMarker();
            return;
        }

        if (hunterHitMarkerTimer > 0f)
        {
            hunterHitMarkerTimer -= Time.deltaTime;

            hunterHitMarkerText.gameObject.SetActive(true);
            hunterHitMarkerText.text = "X";

            if (hunterHitMarkerTimer <= 0f)
            {
                HideHunterHitMarker();
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

    void HideHunterHitMarker()
    {
        if (hunterHitMarkerText == null)
        {
            return;
        }

        hunterHitMarkerText.text = "";
        hunterHitMarkerText.gameObject.SetActive(false);
    }
}