using UnityEngine;

public class SurvivorAutoMoveTest : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public CharacterStatus status;

    [Header("Auto Move")]
    public float moveDistance = 4f;
    public float pauseAtEnds = 0.3f;

    [Header("Debug")]
    public bool showDebugLog = false;

    private Vector3 startPosition;
    private Vector3 moveAxis;
    private bool moveToPositive = true;
    private float pauseTimer = 0f;
    private bool initialized = false;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        if (status == null)
        {
            status = GetComponent<CharacterStatus>();
        }
    }

    void OnEnable()
    {
        if (!initialized)
        {
            startPosition = transform.position;
            moveAxis = transform.right.normalized;

            if (moveAxis.sqrMagnitude < 0.001f)
            {
                moveAxis = Vector3.right;
            }

            initialized = true;
        }

        pauseTimer = 0f;
    }

    void OnDisable()
    {
        StopAutoMove();
    }

    void Update()
    {
        if (controller == null)
        {
            return;
        }

        if (!CanAutoMove())
        {
            StopAutoMove();
            return;
        }

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            StopAutoMove();
            return;
        }

        Vector3 targetOffset = moveToPositive ? moveAxis * moveDistance : -moveAxis * moveDistance;
        Vector3 targetPosition = startPosition + targetOffset;

        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= 0.15f)
        {
            moveToPositive = !moveToPositive;
            pauseTimer = pauseAtEnds;
            StopAutoMove();

            if (showDebugLog)
            {
                Debug.Log("SurvivorAutoMoveTest: reached end point, turning around.");
            }

            return;
        }

        float autoSpeed = controller.GetDefaultMoveSpeed();
        controller.StartForcedMove(toTarget.normalized, autoSpeed);
    }

    bool CanAutoMove()
    {
        if (!enabled)
        {
            return false;
        }

        if (controller == null)
        {
            return false;
        }

        CharacterController cc = controller.GetComponent<CharacterController>();
        if (cc == null)
        {
            return false;
        }

        if (!cc.enabled)
        {
            return false;
        }

        if (status != null)
        {
            if (status.IsDowned) return false;
            if (status.IsCarried) return false;
            if (status.IsChaired) return false;
            if (status.IsEliminated) return false;
            if (status.IsEscaped) return false;
            if (status.IsHitStunned) return false;
        }

        return true;
    }

    void StopAutoMove()
    {
        if (controller != null)
        {
            controller.StopForcedMove();
        }
    }
}