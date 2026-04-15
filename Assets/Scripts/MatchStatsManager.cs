using UnityEngine;

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

    public void StartMatch()
    {
        currentStats = new MatchStats();
        matchStartTime = Time.time;
        matchStarted = true;
    }

    public void EndMatch(bool escaped, bool eliminated)
    {
        currentStats.escaped = escaped;
        currentStats.eliminated = eliminated;

        if (matchStarted)
        {
            currentStats.surviveTime = Time.time - matchStartTime;
        }
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
    }

    public void AddGateOpen()
    {
        currentStats.gateOpenCount++;
    }

    public void AddRescue()
    {
        currentStats.rescueCount++;
    }

    public void AddHunterHit()
    {
        currentStats.hunterHitCount++;
    }

    public void AddSurvivorHitTaken()
    {
        currentStats.survivorHitTakenCount++;
    }

    public void AddDown()
    {
        currentStats.downCount++;
    }

    public void AddEnvironmentInteract()
    {
        currentStats.environmentInteractCount++;
    }
}