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

    public SettlementSummary lastSummary = new SettlementSummary();
    public SettlementSummary LastSummary => lastSummary;

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

        lastSummary = new SettlementSummary();
        lastSummary.baseSoft = baseSoftReward;
        lastSummary.baseMaterial = baseMaterialReward;

        if (stats.escaped)
        {
            lastSummary.winSoft = winSoftReward;
        }

        List<string> completedTaskNames = new List<string>();

        if (activeTasks != null && TaskChecker.Instance != null)
        {
            for (int i = 0; i < activeTasks.Length; i++)
            {
                TaskDefinition task = activeTasks[i];
                if (task == null) continue;

                bool completed = TaskChecker.Instance.IsTaskCompleted(task, stats);
                if (!completed) continue;

                completedTaskNames.Add(task.taskName);
                lastSummary.taskSoft += task.softCurrencyReward;
                lastSummary.totalPremium += task.premiumCurrencyReward;
                lastSummary.taskMaterial += task.materialReward;

                if (logSettlement)
                {
                    Debug.Log("Task completed: " + task.taskName);
                }
            }
        }

        lastSummary.completedTaskText = completedTaskNames.Count > 0
            ? string.Join(", ", completedTaskNames)
            : "None";

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
    }
}