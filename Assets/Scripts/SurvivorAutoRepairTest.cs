using UnityEngine;

public class SurvivorAutoRepairTest : MonoBehaviour
{
    [Header("Auto Repair Settings")]
    public bool autoRepairEnabled = true;
    public CipherMachine targetCipher;
    public float startDelay = 1f;

    [Header("References")]
    public InteractionUI repairerUI;

    [Header("Debug")]
    public bool showDebugLog = true;

    private float timer = 0f;
    private bool isRepairing = false;

    void Awake()
    {
        if (repairerUI == null)
        {
            repairerUI = GetComponent<InteractionUI>();
        }
    }

    void OnEnable()
    {
        timer = 0f;
        isRepairing = false;
    }

    void Update()
    {
        if (!autoRepairEnabled)
        {
            StopRepair();
            return;
        }

        if (targetCipher == null)
        {
            StopRepair();
            return;
        }

        if (repairerUI == null)
        {
            StopRepair();
            return;
        }

        if (targetCipher.isCompleted)
        {
            StopRepair();
            return;
        }

        timer += Time.deltaTime;

        if (timer < startDelay)
        {
            return;
        }

        if (!isRepairing)
        {
            StartRepair();
        }

        targetCipher.BeginRepair(repairerUI);
    }

    void StartRepair()
    {
        isRepairing = true;

        if (showDebugLog && targetCipher != null)
        {
            Debug.Log(gameObject.name + " starts auto repairing " + targetCipher.name);
        }
    }

    void StopRepair()
    {
        if (targetCipher != null && repairerUI != null)
        {
            targetCipher.EndRepair(repairerUI);
        }

        if (isRepairing && showDebugLog)
        {
            Debug.Log(gameObject.name + " stops auto repairing.");
        }

        isRepairing = false;
    }

    void OnDisable()
    {
        StopRepair();
    }

    void OnDestroy()
    {
        StopRepair();
    }
}