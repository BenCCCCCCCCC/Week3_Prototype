using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatus : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public PlayerStatsSO playerStats;
    public GameObject closeDetectMarker;
    public Image slowWarningImage;

    [Header("Runtime State")]
    public int currentHP = 2;

    [Header("Debug")]
    public bool logStateChanges = true;

    private float pendingSlowDuration = 2f;
    private float currentSlowMultiplier = 1f;

    private bool isDowned = false;
    private bool isHitStunned = false;
    private bool isSlowed = false;

    public bool IsDowned => isDowned;
    public bool IsHitStunned => isHitStunned;
    public bool IsSlowed => isSlowed;
    public bool IsInjured => !isDowned && currentHP < GetMaxHP();

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        if (playerStats == null && controller != null)
        {
            playerStats = controller.playerStats;
        }
    }

    void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError("CharacterStatus: playerStats is not assigned.");
            return;
        }

        currentHP = Mathf.Clamp(currentHP, 1, GetMaxHP());

        SetCloseDetectMarker(false);
        SetSlowWarning(false);

        RefreshMoveSpeedFromState();
    }

    public bool ApplySlow(float slowMultiplier, float duration)
    {
        if (controller == null) return false;
        if (playerStats == null) return false;
        if (isDowned) return false;
        if (controller.IsInvincible) return false;

        CancelInvoke(nameof(RecoverOriginalSpeed));

        isSlowed = true;
        currentSlowMultiplier = slowMultiplier;
        pendingSlowDuration = duration;

        SetSlowWarning(true);
        RefreshMoveSpeedFromState();

        Invoke(nameof(RecoverOriginalSpeed), pendingSlowDuration);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " is slowed. Multiplier = " + slowMultiplier + ", Duration = " + duration);
        }

        return true;
    }

    public bool TakeHit(Vector3 attackerPosition)
    {
        if (controller == null) return false;
        if (playerStats == null) return false;
        if (isDowned) return false;
        if (controller.IsInvincible) return false;

        InterruptCurrentAction();

        controller.StopForcedMove();

        currentHP = Mathf.Max(0, currentHP - 1);

        StartInvincible(playerStats.hitInvincibleDuration);
        StartCoroutine(HitStunRoutine());

        if (currentHP <= 0)
        {
            Down();
        }
        else
        {
            if (logStateChanges)
            {
                Debug.Log(gameObject.name + " was hit. State = Injured. HP = " + currentHP);
            }
        }

        return true;
    }

    public void InterruptCurrentAction()
    {
        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " current interaction was interrupted.");
        }
    }

    IEnumerator HitStunRoutine()
    {
        if (controller == null) yield break;
        if (playerStats == null) yield break;
        if (isDowned) yield break;

        isHitStunned = true;

        bool previousInputState = controller.enablePlayerInput;

        controller.StopForcedMove();
        controller.SetSpeed(0f);
        controller.enablePlayerInput = false;

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " entered HitStun.");
        }

        yield return new WaitForSeconds(playerStats.hitStunDuration);

        isHitStunned = false;

        if (!isDowned)
        {
            controller.enablePlayerInput = previousInputState;
            RefreshMoveSpeedFromState();
        }

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " exited HitStun.");
        }
    }

    void Down()
    {
        isDowned = true;
        currentHP = 0;

        CancelInvoke(nameof(RecoverOriginalSpeed));

        if (controller != null)
        {
            controller.StopForcedMove();
            controller.SetSpeed(0f);
            controller.enablePlayerInput = false;
        }

        SetSlowWarning(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " is Downed.");
        }
    }

    public void ReviveToInjured()
    {
        isDowned = false;
        isHitStunned = false;
        currentHP = 1;

        if (controller != null)
        {
            controller.enablePlayerInput = true;
            RefreshMoveSpeedFromState();
        }

        SetSlowWarning(isSlowed);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " revived to Injured.");
        }
    }

    public void ResetToHealthy()
    {
        isDowned = false;
        isHitStunned = false;
        isSlowed = false;
        currentSlowMultiplier = 1f;

        currentHP = GetMaxHP();

        if (controller != null)
        {
            controller.enablePlayerInput = true;
            controller.IsInvincible = false;
            controller.StopForcedMove();
            RefreshMoveSpeedFromState();
        }

        SetSlowWarning(false);
        SetCloseDetectMarker(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " reset to Healthy.");
        }
    }

    void RecoverOriginalSpeed()
    {
        isSlowed = false;
        currentSlowMultiplier = 1f;

        if (controller != null && !isDowned && !isHitStunned)
        {
            RefreshMoveSpeedFromState();
        }

        SetSlowWarning(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " slow expired.");
        }
    }

    public void RefreshMoveSpeedFromState()
    {
        if (controller == null) return;
        if (playerStats == null) return;
        if (isDowned) return;
        if (isHitStunned) return;

        float finalSpeed = controller.GetDefaultMoveSpeed();

        if (IsInjured)
        {
            finalSpeed *= playerStats.injuredMoveMultiplier;
        }

        if (isSlowed)
        {
            finalSpeed *= currentSlowMultiplier;
        }

        controller.SetSpeed(finalSpeed);
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

    int GetMaxHP()
    {
        if (playerStats == null) return 1;
        return playerStats.maxHP;
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