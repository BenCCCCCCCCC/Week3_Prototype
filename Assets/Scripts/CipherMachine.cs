using System.Collections.Generic;
using UnityEngine;

public class CipherMachine : MonoBehaviour
{
    [Header("References")]
    public MatchManager matchManager;
    public InteractionTarget interactionTarget;
    public InteractionStatsSO interactionStats;

    [Header("Runtime")]
    [Range(0f, 1f)] public float progress01 = 0f;
    public bool isCompleted = false;

    [Header("Debug")]
    public bool logCipherEvents = true;

    private HashSet<InteractionUI> activeRepairers = new HashSet<InteractionUI>();

    public int ActiveRepairerCount => activeRepairers.Count;

    void Awake()
    {
        if (interactionTarget == null)
        {
            interactionTarget = GetComponent<InteractionTarget>();
        }
    }

    void Update()
    {
        if (isCompleted) return;

        CleanupNullRepairers();

        int repairerCount = activeRepairers.Count;
        if (repairerCount <= 0) return;

        float repairSeconds = 45f;
        float multiRepairEfficiency = 0.85f;

        if (interactionStats != null)
        {
            repairSeconds = Mathf.Max(0.1f, interactionStats.repairHoldSeconds);
            multiRepairEfficiency = interactionStats.multiRepairEfficiency;
        }

        float totalRateMultiplier = 1f;

        if (repairerCount > 1)
        {
            totalRateMultiplier = 1f + (repairerCount - 1) * multiRepairEfficiency;
        }

        float delta = (Time.deltaTime / repairSeconds) * totalRateMultiplier;
        progress01 = Mathf.Clamp01(progress01 + delta);

        if (progress01 >= 1f)
        {
            CompleteCipher();
        }
    }

    void CleanupNullRepairers()
    {
        activeRepairers.RemoveWhere(ui => ui == null);
    }

    public void BeginRepair(InteractionUI ui)
    {
        if (ui == null) return;
        if (isCompleted) return;

        activeRepairers.Add(ui);
    }

    public void EndRepair(InteractionUI ui)
    {
        if (ui == null) return;

        if (activeRepairers.Contains(ui))
        {
            activeRepairers.Remove(ui);
        }
    }

    public bool CanRepair()
    {
        return !isCompleted;
    }

    void CompleteCipher()
    {
        if (isCompleted) return;

        isCompleted = true;
        progress01 = 1f;
        activeRepairers.Clear();

        if (interactionTarget != null)
        {
            interactionTarget.isCompleted = true;
        }

        if (matchManager != null)
        {
            matchManager.OnCipherCompleted(this);
        }

        if (logCipherEvents)
        {
            Debug.Log("CipherMachine: Completed = " + gameObject.name);
        }
    }
}