using System.Collections.Generic;
using UnityEngine;

public class ChairController : MonoBehaviour
{
    [Header("References")]
    public MatchManager matchManager;
    public Transform seatAnchor;
    public Transform rescuePoint;
    public InteractionTarget interactionTarget;

    [Header("Chair Rules")]
    public float chairDuration = 15f;
    public float rescueLockDuration = 0.5f;

    [Header("Debug")]
    public bool logChairEvents = true;

    private CharacterStatus occupant;
    private float remainingTime = 0f;
    private bool eliminationResolved = false;
    private float rescueLockTimer = 0f;

    public bool IsOccupied => occupant != null;
    public CharacterStatus Occupant => occupant;
    public float RemainingTime => remainingTime;

    void Awake()
    {
        if (interactionTarget == null)
        {
            interactionTarget = GetComponent<InteractionTarget>();
        }
    }

    void Update()
    {
        if (rescueLockTimer > 0f)
        {
            rescueLockTimer -= Time.deltaTime;
            if (rescueLockTimer < 0f)
            {
                rescueLockTimer = 0f;
            }
        }

        if (occupant == null) return;
        if (eliminationResolved) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EliminateOccupant();
        }
    }

    public bool CanPlace(CharacterStatus target)
    {
        if (target == null) return false;
        if (occupant != null) return false;
        if (!target.IsCarried) return false;
        if (target.IsEliminated) return false;

        return true;
    }

    public bool PlaceSurvivor(CharacterStatus target)
    {
        if (!CanPlace(target)) return false;
        if (seatAnchor == null)
        {
            Debug.LogWarning("ChairController: seatAnchor is not assigned on " + gameObject.name);
            return false;
        }

        bool success = target.PlaceOnChair(this);
        if (!success)
        {
            return false;
        }

        occupant = target;
        remainingTime = chairDuration;
        eliminationResolved = false;
        rescueLockTimer = 0f;

        if (interactionTarget != null)
        {
            interactionTarget.isCompleted = false;
        }

        if (logChairEvents)
        {
            Debug.Log("ChairController: survivor placed on chair = " + target.name + ", chair = " + gameObject.name);
        }

        TelemetryLogger.Emit("survivor_chaired", new Dictionary<string, object>
        {
            { "chair_id", TelemetryLogger.GetObjectId(gameObject) },
            { "survivor_id", TelemetryLogger.GetObjectId(target.gameObject) },
            { "hunter_id", GetHunterId() },
            { "time_since_match_start", GetTimeSinceMatchStart() }
        });

        return true;
    }

    public bool CanRescue()
    {
        if (occupant == null) return false;
        if (eliminationResolved) return false;
        if (rescueLockTimer > 0f) return false;

        return true;
    }

    public bool RescueOccupant(string rescuerId, float rescueDurationSeconds)
    {
        if (!CanRescue()) return false;

        CharacterStatus target = occupant;
        occupant = null;
        remainingTime = 0f;
        eliminationResolved = false;
        rescueLockTimer = rescueLockDuration;

        Vector3 releasePosition = transform.position + transform.right * 1.2f;

        if (rescuePoint != null)
        {
            releasePosition = rescuePoint.position;
        }

        target.RescueFromChair(releasePosition);

        if (interactionTarget != null)
        {
            interactionTarget.isCompleted = false;
        }

        if (logChairEvents)
        {
            Debug.Log("ChairController: occupant rescued from chair = " + target.name);
        }

        TelemetryLogger.Emit("rescue_attempt_complete", new Dictionary<string, object>
        {
            { "chair_id", TelemetryLogger.GetObjectId(gameObject) },
            { "rescuer_id", rescuerId },
            { "target_id", TelemetryLogger.GetObjectId(target.gameObject) },
            { "rescue_duration_seconds", rescueDurationSeconds }
        });

        return true;
    }

    public void ResetChairForNewMatch()
    {
        occupant = null;
        remainingTime = 0f;
        eliminationResolved = false;
        rescueLockTimer = 0f;

        if (interactionTarget != null)
        {
            interactionTarget.isCompleted = false;
            interactionTarget.gameObject.SetActive(true);
        }

        enabled = true;

        if (logChairEvents)
        {
            Debug.Log("ChairController: reset chair for new match = " + gameObject.name);
        }
    }

    void EliminateOccupant()
    {
        if (occupant == null) return;
        if (eliminationResolved) return;

        eliminationResolved = true;

        CharacterStatus target = occupant;
        occupant = null;

        target.Eliminate();

        TelemetryLogger.Emit("survivor_eliminated", new Dictionary<string, object>
        {
            { "chair_id", TelemetryLogger.GetObjectId(gameObject) },
            { "survivor_id", TelemetryLogger.GetObjectId(target.gameObject) },
            { "hunter_id", GetHunterId() },
            { "chair_duration_at_elim", Mathf.Max(0f, chairDuration - remainingTime) }
        });

        if (matchManager != null)
        {
            matchManager.OnSurvivorEliminated(target.gameObject);
        }

        if (logChairEvents)
        {
            Debug.Log("ChairController: occupant eliminated from chair = " + target.name);
        }
    }

    float GetTimeSinceMatchStart()
    {
        if (MatchStatsManager.Instance == null) return 0f;
        return MatchStatsManager.Instance.GetElapsedTime();
    }

    string GetHunterId()
    {
        if (matchManager == null) return "unknown";
        return TelemetryLogger.GetObjectId(matchManager.hunterController);
    }
}
