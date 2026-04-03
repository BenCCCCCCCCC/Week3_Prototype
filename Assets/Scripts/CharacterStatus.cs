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
    private bool isCarried = false;
    private bool isChaired = false;
    private bool isEliminated = false;
    private bool isEscaped = false;
    private bool downCountReported = false;

    private Transform carryAnchor;
    private ChairController currentChair;

    private CharacterController characterController;
    private CapsuleCollider capsuleCollider;
    private MatchManager matchManager;
    private InteractionUI interactionUI;
    private RescueAutoTest rescueAutoTest;

    public bool IsDowned => isDowned;
    public bool IsHitStunned => isHitStunned;
    public bool IsSlowed => isSlowed;
    public bool IsCarried => isCarried;
    public bool IsChaired => isChaired;
    public bool IsEliminated => isEliminated;
    public bool IsEscaped => isEscaped;
    public ChairController CurrentChair => currentChair;

    public bool IsInjured
    {
        get
        {
            return !isDowned &&
                   !isCarried &&
                   !isChaired &&
                   !isEliminated &&
                   !isEscaped &&
                   currentHP < GetMaxHP();
        }
    }

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

        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        matchManager = FindFirstObjectByType<MatchManager>();
        interactionUI = GetComponent<InteractionUI>();
        rescueAutoTest = GetComponent<RescueAutoTest>();
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
        SetCarryCollisionEnabled(true);

        RefreshMoveSpeedFromState();
    }

    void LateUpdate()
    {
        if (isCarried && carryAnchor != null)
        {
            transform.position = carryAnchor.position;
            transform.rotation = carryAnchor.rotation;
        }

        if (isChaired && currentChair != null && currentChair.seatAnchor != null)
        {
            transform.position = currentChair.seatAnchor.position;
            transform.rotation = currentChair.seatAnchor.rotation;
        }
    }

    public bool ApplySlow(float slowMultiplier, float duration)
    {
        if (controller == null) return false;
        if (playerStats == null) return false;
        if (isDowned) return false;
        if (isCarried) return false;
        if (isChaired) return false;
        if (isEliminated) return false;
        if (isEscaped) return false;
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
        if (isCarried) return false;
        if (isChaired) return false;
        if (isEliminated) return false;
        if (isEscaped) return false;
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
        if (interactionUI != null)
        {
            interactionUI.ForceInterruptInteraction("Character was hit or interrupted");
        }

        if (rescueAutoTest != null)
        {
            rescueAutoTest.ForceInterruptAutoRescue("Character was hit or interrupted");
        }

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
        if (isCarried) yield break;
        if (isChaired) yield break;

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

        if (!isDowned && !isCarried && !isChaired && !isEliminated && !isEscaped)
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
        isCarried = false;
        isChaired = false;
        isEliminated = false;
        isEscaped = false;

        currentChair = null;
        carryAnchor = null;
        currentHP = 0;

        CancelInvoke(nameof(RecoverOriginalSpeed));

        if (controller != null)
        {
            controller.StopForcedMove();
            controller.SetSpeed(0f);
            controller.enablePlayerInput = false;
        }

        SetCarryCollisionEnabled(true);
        SetSlowWarning(false);

        if (!downCountReported && matchManager != null)
        {
            matchManager.OnSurvivorDowned(gameObject);
            downCountReported = true;
        }

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " is Downed.");
        }
    }

    public bool CanBePickedUp()
    {
        return isDowned &&
               !isCarried &&
               !isChaired &&
               !isEliminated &&
               !isEscaped;
    }

    public bool StartCarry(Transform newCarryAnchor)
    {
        if (!CanBePickedUp()) return false;
        if (newCarryAnchor == null) return false;

        isDowned = false;
        isCarried = true;
        isChaired = false;
        isEliminated = false;
        isEscaped = false;

        currentChair = null;
        carryAnchor = newCarryAnchor;

        if (controller != null)
        {
            controller.StopForcedMove();
            controller.SetSpeed(0f);
            controller.enablePlayerInput = false;
        }

        SetCarryCollisionEnabled(false);
        SetSlowWarning(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " is now Carried.");
        }

        return true;
    }

    public void DropFromCarryToGround()
    {
        if (!isCarried) return;

        isCarried = false;
        isDowned = true;
        isChaired = false;
        isEliminated = false;
        isEscaped = false;

        carryAnchor = null;
        currentChair = null;

        if (controller != null)
        {
            controller.StopForcedMove();
            controller.SetSpeed(0f);
            controller.enablePlayerInput = false;
        }

        SetCarryCollisionEnabled(true);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " was dropped and returned to Downed state.");
        }
    }

    public bool PlaceOnChair(ChairController chair)
    {
        if (!isCarried) return false;
        if (chair == null) return false;

        isCarried = false;
        isDowned = false;
        isChaired = true;
        isEliminated = false;
        isEscaped = false;

        carryAnchor = null;
        currentChair = chair;

        if (controller != null)
        {
            controller.StopForcedMove();
            controller.SetSpeed(0f);
            controller.enablePlayerInput = false;
        }

        SetCarryCollisionEnabled(false);
        SetSlowWarning(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " is now on Chair.");
        }

        return true;
    }

    public void RescueFromChair(Vector3 releasePosition)
    {
        if (!isChaired) return;

        isChaired = false;
        isDowned = false;
        isCarried = false;
        isEliminated = false;
        isEscaped = false;

        currentChair = null;
        carryAnchor = null;
        isHitStunned = false;

        currentHP = 1;
        downCountReported = false;
        transform.position = releasePosition;

        SetCarryCollisionEnabled(true);

        if (controller != null)
        {
            controller.StopForcedMove();
            controller.SetSpeed(0f);
            controller.enablePlayerInput = false;
            RefreshMoveSpeedFromState();
        }

        SetSlowWarning(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " was rescued from Chair and returned to Injured state.");
        }
    }

    public void Eliminate()
    {
        if (isEliminated) return;

        isEliminated = true;
        isDowned = false;
        isCarried = false;
        isChaired = false;

        currentChair = null;
        carryAnchor = null;

        if (controller != null)
        {
            controller.StopForcedMove();
            controller.SetSpeed(0f);
            controller.enablePlayerInput = false;
        }

        SetCarryCollisionEnabled(false);
        SetSlowWarning(false);
        SetCloseDetectMarker(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " is Eliminated.");
        }
    }

    public void MarkEscaped()
    {
        if (isEscaped) return;

        isEscaped = true;
        isDowned = false;
        isCarried = false;
        isChaired = false;

        currentChair = null;
        carryAnchor = null;

        if (controller != null)
        {
            controller.StopForcedMove();
            controller.SetSpeed(0f);
            controller.enablePlayerInput = false;
        }

        SetCarryCollisionEnabled(false);
        SetSlowWarning(false);
        SetCloseDetectMarker(false);

        if (logStateChanges)
        {
            Debug.Log(gameObject.name + " is Escaped.");
        }
    }

    public void ReviveToInjured()
    {
        isDowned = false;
        isHitStunned = false;
        isCarried = false;
        isChaired = false;
        isEliminated = false;
        isEscaped = false;

        currentChair = null;
        carryAnchor = null;
        currentHP = 1;
        downCountReported = false;

        SetCarryCollisionEnabled(true);

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
        isCarried = false;
        isChaired = false;
        isEliminated = false;
        isEscaped = false;

        currentChair = null;
        carryAnchor = null;
        currentSlowMultiplier = 1f;
        currentHP = GetMaxHP();
        downCountReported = false;

        SetCarryCollisionEnabled(true);

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

        if (controller != null && !isDowned && !isHitStunned && !isCarried && !isChaired && !isEliminated && !isEscaped)
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
        if (isCarried) return;
        if (isChaired) return;
        if (isEliminated) return;
        if (isEscaped) return;

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

    void SetCarryCollisionEnabled(bool enabled)
    {
        if (characterController != null)
        {
            characterController.enabled = enabled;
        }

        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = enabled;
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