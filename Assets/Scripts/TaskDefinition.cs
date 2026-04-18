using UnityEngine;

public enum TaskType
{
    EscapeOnce,
    RescueCountReach,
    RepairProgressReach,
    HunterHitCountReach,
    DownCountReach,
    GateOpenCountReach,
    EnvironmentInteractReach
}

[CreateAssetMenu(fileName = "TaskDefinition", menuName = "NightFile/Task Definition")]
public class TaskDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string taskId = "task_001";
    public string taskName = "Escape once";
    [TextArea] public string description = "Complete the objective once.";

    [Header("Condition")]
    public TaskType taskType = TaskType.EscapeOnce;
    public int targetValue = 1;

    [Header("Rewards")]
    public int softCurrencyReward = 50;
    public int premiumCurrencyReward = 0;
    public int materialReward = 1;
}