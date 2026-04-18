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

        float baseTeamRateMultiplier = 1f;
        if (repairerCount > 1)
        {
            baseTeamRateMultiplier = 1f + (repairerCount - 1) * multiRepairEfficiency;
        }

        float averageLoadoutMultiplier = GetAverageRepairSpeedMultiplier();
        float finalRateMultiplier = baseTeamRateMultiplier * averageLoadoutMultiplier;

        float beforeProgress = progress01;

        float delta = (Time.deltaTime / repairSeconds) * finalRateMultiplier;
        progress01 = Mathf.Clamp01(progress01 + delta);

        float actualDelta = progress01 - beforeProgress;

        if (actualDelta > 0f && MatchStatsManager.Instance != null)
        {
            int objectiveCipherCount = 1;

            if (matchManager != null)
            {
                objectiveCipherCount = Mathf.Max(1, matchManager.requiredCompletedCiphers);
            }

            // 主目标：修满本局要求的全部密码机 100
            float normalizedProgress = (actualDelta * 100f) / objectiveCipherCount;
            MatchStatsManager.Instance.AddRepairProgress(normalizedProgress);
        }

        if (progress01 >= 1f)
        {
            CompleteCipher();
        }
    }

    float GetAverageRepairSpeedMultiplier()
    {
        if (activeRepairers.Count <= 0)
        {
            return 1f;
        }

        float total = 0f;
        int count = 0;

        foreach (InteractionUI ui in activeRepairers)
        {
            if (ui == null) continue;

            total += ui.GetCipherRepairSpeedMultiplier();
            count++;
        }

        if (count <= 0)
        {
            return 1f;
        }

        return total / count;
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