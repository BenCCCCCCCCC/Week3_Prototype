using UnityEngine;

public class MatchSettlement : MonoBehaviour
{
    [Header("Active Tasks")]
    public TaskDefinition[] activeTasks;

    [Header("Base Rewards")]
    public int baseSoftReward = 30;
    public int winSoftReward = 20;
    public int baseMaterialReward = 1;

    [Header("Debug")]
    public bool logSettlement = true;

    public void SettleMatch()
    {
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

        int totalSoft = baseSoftReward;
        int totalPremium = 0;
        int totalMaterial = baseMaterialReward;

        if (stats.escaped)
        {
            totalSoft += winSoftReward;
        }

        if (activeTasks != null && TaskChecker.Instance != null)
        {
            for (int i = 0; i < activeTasks.Length; i++)
            {
                TaskDefinition task = activeTasks[i];
                if (task == null) continue;

                bool completed = TaskChecker.Instance.IsTaskCompleted(task, stats);
                if (!completed) continue;

                totalSoft += task.softCurrencyReward;
                totalPremium += task.premiumCurrencyReward;
                totalMaterial += task.materialReward;

                if (logSettlement)
                {
                    Debug.Log("Task completed: " + task.taskName);
                }
            }
        }

        PlayerProfile.Instance.AddRewards(totalSoft, totalPremium, totalMaterial);

        if (logSettlement)
        {
            Debug.Log(
                "Settlement complete. " +
                "Soft = " + totalSoft +
                ", Premium = " + totalPremium +
                ", Material = " + totalMaterial
            );
        }
    }
}