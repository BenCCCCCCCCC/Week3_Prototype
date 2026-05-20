using System.Text;
using TMPro;
using UnityEngine;

public class TaskPanelUI : MonoBehaviour
{
    [Header("References")]
    public MatchSettlement matchSettlement;
    public ArchiveProgress archiveProgress;
    public TMP_Text activeTasksText;
    public TMP_Text lastCompletedTasksText;

    [Header("Current Tab")]
    public TaskLayer currentLayer = TaskLayer.Daily;

    private void OnEnable()
    {
        Refresh();
    }

    public void ShowDailyTasks()
    {
        currentLayer = TaskLayer.Daily;
        Refresh();
    }

    public void ShowWeeklyTasks()
    {
        currentLayer = TaskLayer.Weekly;
        Refresh();
    }

    public void ShowSeasonTasks()
    {
        currentLayer = TaskLayer.Season;
        Refresh();
    }

    public void Refresh()
    {
        if (activeTasksText != null)
        {
            activeTasksText.text = BuildTaskListText();
        }

        if (lastCompletedTasksText != null)
        {
            string completed = "None";

            if (matchSettlement != null && matchSettlement.LastSummary != null)
            {
                completed = matchSettlement.LastSummary.completedTaskText;
            }

            lastCompletedTasksText.text = "Last Completed Tasks: " + completed;
        }
    }

    string BuildTaskListText()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("TASK LAYER: " + GetCurrentLayerTitle().ToUpper());

        if (currentLayer == TaskLayer.Daily)
        {
            sb.AppendLine("Refresh: Daily 04:00 | Active: 3 per day | Pool shown: 6");
        }
        else if (currentLayer == TaskLayer.Weekly)
        {
            sb.AppendLine("Refresh: Monday 04:00 | Active: 5 per week | Pool shown: 6");
        }
        else
        {
            int level = GetArchiveLevel();
            sb.AppendLine("Season Progress Source: ArchiveProgress.cs");
            sb.AppendLine("Archive Level: " + level + " | Entry Rule: Archive Lv. >= 2 | Pool shown: 10");
        }

        sb.AppendLine("────────────────────────────────────────────");

        if (matchSettlement == null || matchSettlement.activeTasks == null || matchSettlement.activeTasks.Length == 0)
        {
            sb.AppendLine("No active tasks assigned.");
            return sb.ToString();
        }

        MatchStats stats = null;

        if (MatchStatsManager.Instance != null)
        {
            stats = MatchStatsManager.Instance.currentStats;
        }

        int shownCount = 0;

        for (int i = 0; i < matchSettlement.activeTasks.Length; i++)
        {
            TaskDefinition task = matchSettlement.activeTasks[i];

            if (task == null)
            {
                continue;
            }

            if (task.taskLayer != currentLayer)
            {
                continue;
            }

            shownCount++;

            sb.AppendLine();
            sb.AppendLine(shownCount + ". [" + task.taskId + "] " + task.taskName);
            sb.AppendLine("Role: " + task.GetPerspectiveText() + "    Pair: " + task.counterpartTaskId);
            sb.AppendLine("Target: " + BuildCompactTargetText(task));

            if (TaskChecker.Instance != null && stats != null)
            {
                sb.AppendLine("Progress: " + TaskChecker.Instance.GetProgressText(task, stats));
            }
            else
            {
                sb.AppendLine("Progress: 0 / " + task.targetValue);
            }

            sb.AppendLine("Reward: " + task.GetRewardText() + "    Claim: " + task.rewardTiming);
            sb.AppendLine("Reset: " + task.canReset + "    Status: " + BuildRuntimeStatusText(task));

            if (!string.IsNullOrWhiteSpace(task.implementationNote))
            {
                sb.AppendLine("Note: " + BuildShortNote(task.implementationNote));
            }

            sb.AppendLine("────────────────────────────────────────────");
        }

        if (shownCount == 0)
        {
            sb.AppendLine();
            sb.AppendLine("No " + GetCurrentLayerTitle() + " tasks assigned in activeTasks.");
        }

        return sb.ToString();
    }

    string BuildCompactTargetText(TaskDefinition task)
    {
        if (task == null)
        {
            return "-";
        }

        switch (task.taskType)
        {
            case TaskType.RepairProgressReach:
                return "Repair contribution >= " + task.targetValue + "%";

            case TaskType.HunterHitCountReach:
                return "Hunter hits >= " + task.targetValue;

            case TaskType.DownCountReach:
                return "Hunter downs >= " + task.targetValue;

            case TaskType.RescueCountReach:
                return "Rescues >= " + task.targetValue;

            case TaskType.PreventRescueReach:
                return "Prevent rescues >= " + task.targetValue;

            case TaskType.MatchCompleteCountReach:
                return "Valid matches >= " + task.targetValue;

            case TaskType.SkillUseCountReach:
                return "Skill uses >= " + task.targetValue;

            case TaskType.PatrolSignalReach:
                return "Patrol signals >= " + task.targetValue;

            case TaskType.GateOpenCountReach:
                return "Gate opens >= " + task.targetValue;

            case TaskType.EscapeOnce:
                return "Escapes >= " + task.targetValue;

            case TaskType.SeasonWinCountReach:
                return "Season wins >= " + task.targetValue;

            case TaskType.CustomDesignOnly:
                return "Custom objective >= " + task.targetValue;

            default:
                return task.completionRule;
        }
    }

    string BuildRuntimeStatusText(TaskDefinition task)
    {
        if (task == null)
        {
            return "Unknown";
        }

        if (task.rewardEnabledInPrototype)
        {
            return "Runtime reward enabled";
        }

        return "Displayed only";
    }

    string BuildShortNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return "";
        }

        if (note.Length <= 90)
        {
            return note;
        }

        return note.Substring(0, 90) + "...";
    }

    int GetArchiveLevel()
    {
        if (archiveProgress != null)
        {
            return archiveProgress.archiveLevel;
        }

        if (ArchiveProgress.Instance != null)
        {
            return ArchiveProgress.Instance.archiveLevel;
        }

        return 0;
    }

    string GetCurrentLayerTitle()
    {
        if (currentLayer == TaskLayer.Daily)
        {
            return "Daily";
        }

        if (currentLayer == TaskLayer.Weekly)
        {
            return "Weekly";
        }

        return "Season";
    }
}