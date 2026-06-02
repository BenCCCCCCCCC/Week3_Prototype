using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchStatsManager : MonoBehaviour
{
    public static MatchStatsManager Instance;

    [Header("Runtime")]
    public MatchStats currentStats = new MatchStats();

    private float matchStartTime;
    private bool matchStarted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartMatch(MatchManager matchManager, bool emitTelemetry = true)
    {
        currentStats = new MatchStats();
        matchStartTime = Time.time;
        matchStarted = true;

        TelemetryLogger.BeginMatch(Guid.NewGuid().ToString("N"));

        if (emitTelemetry)
        {
            TelemetryLogger.Emit("match_start", new Dictionary<string, object>
            {
                { "map_name", SceneManager.GetActiveScene().name },
                { "hunter_player_id", GetHunterId(matchManager) },
                { "survivor_player_ids", GetSurvivorIds(matchManager) }
            });
        }
    }

    public void EndMatch(int escapedCount, int eliminatedCount)
    {
        currentStats.escaped = escapedCount > 0;
        currentStats.eliminated = eliminatedCount > 0;

        if (matchStarted)
        {
            currentStats.surviveTime = Time.time - matchStartTime;
        }

        TelemetryLogger.Emit("match_end", new Dictionary<string, object>
        {
            { "duration_seconds", currentStats.surviveTime },
            { "escaped_count", escapedCount },
            { "eliminated_count", eliminatedCount }
        });

        matchStarted = false;
    }

    public void SetCompletedCipherCount(int value)
    {
        currentStats.completedCipherCount = Mathf.Max(0, value);
    }

    public void AddRepairProgress(float value)
    {
        currentStats.totalRepairProgress += value;

        if (currentStats.totalRepairProgress < 0f)
        {
            currentStats.totalRepairProgress = 0f;
        }

        if (currentStats.totalRepairProgress > 100f)
        {
            currentStats.totalRepairProgress = 100f;
        }
    }

    public void AddGateOpen()
    {
        currentStats.gateOpenCount++;
    }

    public void AddRescue()
    {
        currentStats.rescueCount++;
    }

    public void AddRescueAttempt()
    {
        currentStats.rescueAttemptCount++;
    }

    public void AddHunterHit()
    {
        currentStats.hunterHitCount++;
    }

    public void AddSurvivorHitTaken()
    {
        currentStats.survivorHitTakenCount++;
    }

    public bool AddDown()
    {
        bool isFirstDown = currentStats.downCount == 0;

        if (isFirstDown)
        {
            currentStats.firstDownTime = GetElapsedTime();
        }

        currentStats.downCount++;
        return isFirstDown;
    }

    public void AddEnvironmentInteract()
    {
        currentStats.environmentInteractCount++;
    }

    public float GetElapsedTime()
    {
        if (!matchStarted) return currentStats.surviveTime;
        return Mathf.Max(0f, Time.time - matchStartTime);
    }

    string GetHunterId(MatchManager matchManager)
    {
        if (matchManager == null) return "unknown";
        return TelemetryLogger.GetObjectId(matchManager.hunterController);
    }

    string[] GetSurvivorIds(MatchManager matchManager)
    {
        if (matchManager == null || matchManager.trackedSurvivors == null)
        {
            return new string[0];
        }

        string[] ids = new string[matchManager.trackedSurvivors.Length];

        for (int i = 0; i < matchManager.trackedSurvivors.Length; i++)
        {
            ids[i] = TelemetryLogger.GetObjectId(matchManager.trackedSurvivors[i]);
        }

        return ids;
    }
}
