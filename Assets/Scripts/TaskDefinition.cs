using UnityEngine;

public enum TaskType
{
    EscapeOnce,
    RescueCountReach,
    RepairProgressReach,
    HunterHitCountReach,
    DownCountReach,
    GateOpenCountReach,
    EnvironmentInteractReach,

    // Week 10 design-only / future-hook task types.
    MatchCompleteCountReach,
    SkillUseCountReach,
    PatrolSignalReach,
    PreventRescueReach,
    FullHealthEscapeReach,
    FullEliminationReach,
    TeamSurviveMatchReach,
    QuickDoubleDownReach,
    SeasonWinCountReach,
    CustomDesignOnly
}

public enum TaskLayer
{
    Daily,
    Weekly,
    Season
}

public enum TaskPerspective
{
    Shared,
    Survivor,
    Hunter
}

public enum TaskRewardTiming
{
    OnMatchSettlement,
    OnDailyClaim,
    OnWeeklyClaim,
    OnActivityClaim,
    OnSeasonClaim
}

[CreateAssetMenu(fileName = "TaskDefinition", menuName = "NightFile/Task Definition")]
public class TaskDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string taskId = "D-X-01";
    public string taskName = "Complete one task";

    [TextArea]
    public string description = "Complete the listed objective.";

    [Header("Week 10 Layer And Perspective")]
    public TaskLayer taskLayer = TaskLayer.Daily;
    public TaskPerspective perspective = TaskPerspective.Shared;

    [Tooltip("Shared tasks use '-'. Survivor/Hunter paired tasks must fill the opposite task id.")]
    public string counterpartTaskId = "-";

    [Header("Condition")]
    public TaskType taskType = TaskType.EscapeOnce;
    public int targetValue = 1;

    [TextArea]
    public string triggerCondition = "Triggered by match statistics.";

    [TextArea]
    public string completionRule = "Completed when current progress reaches the target value.";

    [Header("Progress Rule")]
    public string progressReportFrequency = "On match settlement";

    [Header("Rewards")]
    public int softCurrencyReward = 30;
    public int premiumCurrencyReward = 0;
    public int materialReward = 0;

    [Tooltip("Text only. This does not create a new currency or material.")]
    public string extraRewardText = "";

    [Header("Reward Rule")]
    public TaskRewardTiming rewardTiming = TaskRewardTiming.OnMatchSettlement;
    public bool canReset = true;

    [Header("Prototype Control")]
    [Tooltip("Only enabled tasks can actually give rewards in this Week 10 prototype.")]
    public bool rewardEnabledInPrototype = false;

    [TextArea]
    public string implementationNote = "Displayed in Unity. Runtime progress logic is not fully connected in Week 10.";

    public string GetLayerText()
    {
        if (taskLayer == TaskLayer.Daily)
        {
            return "Daily";
        }

        if (taskLayer == TaskLayer.Weekly)
        {
            return "Weekly";
        }

        return "Season";
    }

    public string GetPerspectiveText()
    {
        if (perspective == TaskPerspective.Survivor)
        {
            return "Survivor";
        }

        if (perspective == TaskPerspective.Hunter)
        {
            return "Hunter";
        }

        return "Shared";
    }

    public string GetRewardText()
    {
        string text = "+" + softCurrencyReward + " BP";

        if (premiumCurrencyReward > 0)
        {
            text += ", +" + premiumCurrencyReward + " Ticket";
        }

        if (materialReward > 0)
        {
            text += ", +" + materialReward + " Material";
        }

        if (!string.IsNullOrWhiteSpace(extraRewardText))
        {
            text += ", " + extraRewardText;
        }

        return text;
    }
}