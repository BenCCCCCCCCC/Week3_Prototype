using UnityEngine;

public class TaskChecker : MonoBehaviour
{
    public static TaskChecker Instance;

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

    public bool IsTaskCompleted(TaskDefinition task, MatchStats stats)
    {
        if (task == null || stats == null)
        {
            return false;
        }

        int currentProgress = GetCurrentProgress(task, stats);
        return currentProgress >= task.targetValue;
    }

    public int GetCurrentProgress(TaskDefinition task, MatchStats stats)
    {
        if (task == null || stats == null)
        {
            return 0;
        }

        switch (task.taskType)
        {
            case TaskType.EscapeOnce:
                if (stats.escaped)
                {
                    return 1;
                }
                return 0;

            case TaskType.RescueCountReach:
                return stats.rescueCount;

            case TaskType.RepairProgressReach:
                return Mathf.RoundToInt(stats.totalRepairProgress);

            case TaskType.HunterHitCountReach:
                return stats.hunterHitCount;

            case TaskType.DownCountReach:
                return stats.downCount;

            case TaskType.GateOpenCountReach:
                return stats.gateOpenCount;

            case TaskType.EnvironmentInteractReach:
                return stats.environmentInteractCount;

            // Week 10 design-only task types.
            // They can be shown in TaskPanelUI now.
            // Their real counters should be connected later.
            case TaskType.MatchCompleteCountReach:
                return 0;

            case TaskType.SkillUseCountReach:
                return 0;

            case TaskType.PatrolSignalReach:
                return 0;

            case TaskType.PreventRescueReach:
                return 0;

            case TaskType.FullHealthEscapeReach:
                return 0;

            case TaskType.FullEliminationReach:
                return 0;

            case TaskType.TeamSurviveMatchReach:
                return 0;

            case TaskType.QuickDoubleDownReach:
                return 0;

            case TaskType.SeasonWinCountReach:
                return 0;

            case TaskType.CustomDesignOnly:
                return 0;

            default:
                return 0;
        }
    }

    public string GetProgressText(TaskDefinition task, MatchStats stats)
    {
        if (task == null)
        {
            return "0 / 0";
        }

        int currentProgress = GetCurrentProgress(task, stats);
        return currentProgress + " / " + task.targetValue;
    }

    public bool IsWeek10RuntimeTask(TaskDefinition task)
    {
        if (task == null)
        {
            return false;
        }

        if (task.taskId == "D-S-01")
        {
            return true;
        }

        if (task.taskId == "D-H-01")
        {
            return true;
        }

        return false;
    }
}