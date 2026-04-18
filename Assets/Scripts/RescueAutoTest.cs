using UnityEngine;

public class RescueAutoTest : MonoBehaviour
{
    [Header("References")]
    public CharacterStatus selfStatus;
    public InteractionStatsSO interactionStats;

    [Header("Auto Rescue Test")]
    public bool autoRescueEnabled = false;
    public float detectRange = 3f;
    public float restartDelayAfterInterrupt = 0.5f;

    [Header("Debug")]
    public bool showDebugLog = true;

    private ChairController currentChair;
    private float progress01 = 0f;
    private float restartLockTimer = 0f;

    public bool IsAutoRescuing => currentChair != null && progress01 > 0f;

    void Awake()
    {
        if (selfStatus == null)
        {
            selfStatus = GetComponent<CharacterStatus>();
        }
    }

    void Update()
    {
        if (!autoRescueEnabled)
        {
            ResetRescue(false);
            return;
        }

        if (restartLockTimer > 0f)
        {
            restartLockTimer -= Time.deltaTime;
            if (restartLockTimer < 0f)
            {
                restartLockTimer = 0f;
            }
        }

        if (!CanAttemptRescue())
        {
            ResetRescue(false);
            return;
        }

        ChairController targetChair = FindNearestRescuableChair();

        if (targetChair == null || restartLockTimer > 0f)
        {
            ResetRescue(false);
            return;
        }

        if (currentChair != targetChair)
        {
            currentChair = targetChair;
            progress01 = 0f;

            if (showDebugLog)
            {
                Debug.Log("RescueAutoTest: started auto rescue on chair = " + currentChair.name);
            }
        }

        float rescueSeconds = 2.5f;
        if (interactionStats != null)
        {
            rescueSeconds = Mathf.Max(0.1f, interactionStats.rescueHoldSeconds);
        }

        progress01 += Time.deltaTime / rescueSeconds;

        if (progress01 >= 1f)
        {
            bool success = currentChair.RescueOccupant();

            if (success && MatchStatsManager.Instance != null)
            {
                MatchStatsManager.Instance.AddRescue();
            }

            if (showDebugLog)
            {
                Debug.Log("RescueAutoTest: auto rescue complete. success = " + success);
            }

            ResetRescue(false);
        }
    }

    bool CanAttemptRescue()
    {
        if (selfStatus == null) return true;
        if (selfStatus.IsDowned) return false;
        if (selfStatus.IsCarried) return false;
        if (selfStatus.IsChaired) return false;
        if (selfStatus.IsEliminated) return false;
        if (selfStatus.IsEscaped) return false;
        if (selfStatus.IsHitStunned) return false;
        return true;
    }

    ChairController FindNearestRescuableChair()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectRange,
            ~0,
            QueryTriggerInteraction.Collide
        );

        ChairController nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            ChairController chair = hit.GetComponent<ChairController>();
            if (chair == null)
            {
                chair = hit.GetComponentInParent<ChairController>();
            }

            if (chair == null) continue;
            if (!chair.CanRescue()) continue;

            float distance = Vector3.Distance(transform.position, chair.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = chair;
            }
        }

        return nearest;
    }

    public void ForceInterruptAutoRescue(string reason = "")
    {
        if (currentChair == null && progress01 <= 0f) return;

        if (showDebugLog)
        {
            if (string.IsNullOrEmpty(reason))
            {
                Debug.Log("RescueAutoTest: auto rescue interrupted.");
            }
            else
            {
                Debug.Log("RescueAutoTest: auto rescue interrupted. Reason = " + reason);
            }
        }

        ResetRescue(true);
    }

    void ResetRescue(bool applyRestartLock)
    {
        currentChair = null;
        progress01 = 0f;

        if (applyRestartLock)
        {
            restartLockTimer = restartDelayAfterInterrupt;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}