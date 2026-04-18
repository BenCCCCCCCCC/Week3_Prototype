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

        switch (task.taskType)
        {
            case TaskType.EscapeOnce:
                return stats.escaped;

            case TaskType.RescueCountReach:
                return stats.rescueCount >= task.targetValue;

            case TaskType.RepairProgressReach:
                return stats.totalRepairProgress >= task.targetValue;

            case TaskType.HunterHitCountReach:
                return stats.hunterHitCount >= task.targetValue;

            case TaskType.DownCountReach:
                return stats.downCount >= task.targetValue;

            case TaskType.GateOpenCountReach:
                return stats.gateOpenCount >= task.targetValue;

            case TaskType.EnvironmentInteractReach:
                return stats.environmentInteractCount >= task.targetValue;

            default:
                return false;
        }
    }
}