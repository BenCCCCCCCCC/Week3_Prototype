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
    private bool isDowned = false;
    private bool isHitStunned = false;

    public bool IsDowned => isDowned;
    public bool IsHitStunned => isHitStunned;
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
    }

    public bool ApplySlow(float slowMultiplier, float duration)
    {
        if (controller == null) return false;
        if (isDowned) return false;
        if (controller.IsInvincible) return false;

        CancelInvoke(nameof(RecoverOriginalSpeed));

        float slowedSpeed = controller.GetDefaultMoveSpeed() * slowMultiplier;
        controller.SetSpeed(slowedSpeed);

        SetSlowWarning(true);

        pendingSlowDuration = duration;
        Invoke(nameof(RecoverOriginalSpeed), pendingSlowDuration);

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
        else if (logStateChanges)
        {
            Debug.Log(gameObject.name + " was hit. State = Injured. HP = " + currentHP);
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

        if (!isDowned)
        {
            controller.RestoreDefaultSpeed();
            controller.enablePlayerInput = previousInputState;
        }

        isHitStunned = false;

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
            controller.RestoreDefaultSpeed();
            controller.enablePlayerInput = true;
        }

        SetSlowWarning(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " revived to Injured.");
        }
    }

    public void ResetToHealthy()
    {
        isDowned = false;
        isHitStunned = false;
        currentHP = GetMaxHP();

        if (controller != null)
        {
            controller.RestoreDefaultSpeed();
            controller.enablePlayerInput = true;
            controller.IsInvincible = false;
            controller.StopForcedMove();
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
        if (controller != null && !isDowned && !isHitStunned)
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
