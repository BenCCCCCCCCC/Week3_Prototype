using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettlementSummary
{
    public int baseSoft;
    public int winSoft;
    public int taskSoft;
    public int loadoutSoft;
    public int totalSoft;

    public int totalPremium;

    public int baseMaterial;
    public int taskMaterial;
    public int loadoutMaterial;
    public int totalMaterial;

    public string completedTaskText = "None";
    public string equippedLoadoutText = "No loadout equipped";
}

public class MatchSettlement : MonoBehaviour
{
    [Header("Active Tasks")]
    public TaskDefinition[] activeTasks;

    [Header("Loadout Source")]
    public PlayerLoadout settlementLoadout;

    [Header("Base Rewards")]
    public int baseSoftReward = 30;
    public int winSoftReward = 20;
    public int baseMaterialReward = 1;

    [Header("Debug")]
    public bool logSettlement = true;

    [Header("Week 10 Debug")]
    public bool enableWeek10DebugSettleKey = true;
    public KeyCode debugSettleKey = KeyCode.F9;

    private bool hasSettledThisMatch = false;

    public SettlementSummary lastSummary = new SettlementSummary();

    public SettlementSummary LastSummary
    {
        get
        {
            return lastSummary;
        }
    }

    private void Update()
    {
        if (enableWeek10DebugSettleKey && Input.GetKeyDown(debugSettleKey))
        {
            Debug.Log("[Week10Demo] Force settlement triggered.");
            SettleMatch("debug_f9");
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            PrintMatchMetrics();
        }
    }

    public void ResetSettlementLock()
    {
        hasSettledThisMatch = false;
    }

    public void SettleMatch(string triggerSource = "natural")
    {
        if (hasSettledThisMatch)
        {
            Debug.LogWarning("MatchSettlement: This match has already been settled.");
            return;
        }

        hasSettledThisMatch = true;

        if (MatchStatsManager.Instance == null)
        {
            Debug.LogWarning("MatchSettlement: MatchStatsManager not found.");
            return;
        }

        if (PlayerProfile.Instance == null)
        {
            Debug.LogWarning("MatchSettlement: PlayerProfile not found.");
            return;
        }

        MatchStats stats = MatchStatsManager.Instance.currentStats;

        lastSummary = new SettlementSummary();

        lastSummary.baseSoft = baseSoftReward;
        lastSummary.baseMaterial = baseMaterialReward;

        if (stats.escaped)
        {
            lastSummary.winSoft = winSoftReward;
        }

        bool abnormalMatch = IsAbnormalMatch();

        if (abnormalMatch)
        {
            TelemetryLogger.SetAbnormal(true);
            triggerSource = "abnormal_match";

            TelemetryLogger.Emit("match_abnormal_flagged", new Dictionary<string, object>
            {
                { "abnormal_reason", "risk_control" }
            });

            if (logSettlement)
            {
                Debug.Log("[RiskControl] Abnormal match detected. Task progress and task rewards are ignored.");
            }
        }

        List<string> completedTaskNames = new List<string>();

        if (!abnormalMatch && activeTasks != null && TaskChecker.Instance != null)
        {
            for (int i = 0; i < activeTasks.Length; i++)
            {
                TaskDefinition task = activeTasks[i];

                if (task == null)
                {
                    continue;
                }

                if (!task.rewardEnabledInPrototype)
                {
                    continue;
                }

                int progress = TaskChecker.Instance.GetCurrentProgress(task, stats);

                if (logSettlement)
                {
                    Debug.Log(
                        "[TaskCheck] task_id=" + task.taskId +
                        ", type=" + task.taskType +
                        ", progress=" + progress +
                        ", target=" + task.targetValue +
                        ", rewardEnabled=" + task.rewardEnabledInPrototype
                    );
                }

                bool completed = TaskChecker.Instance.IsTaskCompleted(task, stats);

                if (!completed)
                {
                    continue;
                }

                completedTaskNames.Add(task.taskName);

                lastSummary.taskSoft += task.softCurrencyReward;
                lastSummary.totalPremium += task.premiumCurrencyReward;
                lastSummary.taskMaterial += task.materialReward;

                if (logSettlement)
                {
                    Debug.Log(
                        "[TaskReward] task_id=" + task.taskId +
                        ", BP=+" + task.softCurrencyReward +
                        ", Ticket=+" + task.premiumCurrencyReward +
                        ", Material=+" + task.materialReward
                    );
                }
            }
        }
        else
        {
            if (TaskChecker.Instance == null)
            {
                Debug.LogWarning("MatchSettlement: TaskChecker.Instance is null. Task rewards will not be checked.");
            }
        }

        if (completedTaskNames.Count > 0)
        {
            lastSummary.completedTaskText = string.Join(", ", completedTaskNames);
        }
        else
        {
            lastSummary.completedTaskText = "None";
        }

        if (settlementLoadout != null)
        {
            lastSummary.loadoutSoft = settlementLoadout.GetSoftCurrencyBonus();
            lastSummary.loadoutMaterial = settlementLoadout.GetMaterialBonus();
            lastSummary.equippedLoadoutText = settlementLoadout.GetEquippedSummaryText();

            if (logSettlement)
            {
                Debug.Log(
                    "Loadout bonus applied. " +
                    "Soft bonus = " + lastSummary.loadoutSoft +
                    ", Material bonus = " + lastSummary.loadoutMaterial
                );
            }
        }
        else
        {
            lastSummary.equippedLoadoutText = "No loadout equipped";
        }

        lastSummary.totalSoft =
            lastSummary.baseSoft +
            lastSummary.winSoft +
            lastSummary.taskSoft +
            lastSummary.loadoutSoft;

        lastSummary.totalMaterial =
            lastSummary.baseMaterial +
            lastSummary.taskMaterial +
            lastSummary.loadoutMaterial;

        PlayerProfile.Instance.AddRewards(
            lastSummary.totalSoft,
            lastSummary.totalPremium,
            lastSummary.totalMaterial
        );

        if (logSettlement)
        {
            Debug.Log(
                "Settlement complete. " +
                "Soft = " + lastSummary.totalSoft +
                ", Premium = " + lastSummary.totalPremium +
                ", Material = " + lastSummary.totalMaterial
            );
        }

        TelemetryLogger.Emit("settlement_complete", new Dictionary<string, object>
        {
            { "total_soft", lastSummary.totalSoft },
            { "total_material", lastSummary.totalMaterial },
            { "total_premium", lastSummary.totalPremium },
            { "task_count_completed", completedTaskNames.Count },
            { "trigger_source", triggerSource }
        });
    }

    void PrintMatchMetrics()
    {
        if (MatchStatsManager.Instance == null)
        {
            Debug.LogWarning("[Metrics] MatchStatsManager not found.");
            return;
        }

        MatchStats stats = MatchStatsManager.Instance.currentStats;
        float rescueSuccessRate = stats.rescueAttemptCount > 0
            ? (float)stats.rescueCount / stats.rescueAttemptCount
            : 0f;
        float firstDownTime = stats.firstDownTime < 0f ? -1f : stats.firstDownTime;

        Debug.Log("[Metrics] match_duration_seconds=" + MatchStatsManager.Instance.GetElapsedTime().ToString("F1"));
        Debug.Log("[Metrics] escape_status=" + stats.escaped);
        Debug.Log("[Metrics] first_down_time_seconds=" + firstDownTime.ToString("F1"));
        Debug.Log("[Metrics] rescue_success_rate=" + rescueSuccessRate.ToString("P1"));
        Debug.Log("[Metrics] repair_completion_rate=" + (stats.totalRepairProgress / 100f).ToString("P1"));
    }

    public bool IsAbnormalMatch()
    {
        // Week 10 prototype stub.
        // This week only uses a client-side placeholder.
        // The final version should validate abnormal matches on the server.
        return false;
    }
}
