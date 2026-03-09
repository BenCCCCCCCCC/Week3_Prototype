using UnityEngine;
using UnityEngine.UI;

public class CharacterStatus : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public GameObject closeDetectMarker;
    public Image slowWarningImage;

    private float pendingSlowDuration = 2f;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }
    }

    void Start()
    {
        SetCloseDetectMarker(false);
        SetSlowWarning(false);
    }

    // Keep the same public name so your other scripts still work
    public bool ApplySlow(float slowMultiplier, float duration)
    {
        if (controller == null) return false;
        if (controller.IsInvincible) return false;

        CancelInvoke(nameof(RecoverOriginalSpeed));

        float slowedSpeed = controller.GetDefaultMoveSpeed() * slowMultiplier;
        controller.SetSpeed(slowedSpeed);

        SetSlowWarning(true);

        pendingSlowDuration = duration;
        Invoke(nameof(RecoverOriginalSpeed), pendingSlowDuration);

        return true;
    }

    // Match teacher wording: 恢复原速
    void RecoverOriginalSpeed()
    {
        if (controller != null)
        {
            controller.RestoreDefaultSpeed();
        }

        SetSlowWarning(false);
    }

    public void StartInvincible(float duration)
    {
        if (controller == null) return;

        CancelInvoke(nameof(EndInvincible));
        controller.IsInvincible = true;
        Invoke(nameof(EndInvincible), duration);
    }

    void EndInvincible()
    {
        if (controller != null)
        {
            controller.IsInvincible = false;
        }
    }

    public void SetCloseDetectMarker(bool show)
    {
        if (closeDetectMarker != null)
        {
            closeDetectMarker.SetActive(show);
        }
    }

    void SetSlowWarning(bool show)
    {
        if (slowWarningImage != null)
        {
            slowWarningImage.gameObject.SetActive(show);
        }
    }
}