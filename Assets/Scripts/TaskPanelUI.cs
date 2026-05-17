using System.Text;
using TMPro;
using UnityEngine;

public class TaskPanelUI : MonoBehaviour
{
    [Header("References")]
    public MatchSettlement matchSettlement;
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
        if (matchSettlement == null || matchSettlement.activeTasks == null || matchSettlement.activeTasks.Length == 0)
        {
            return "Task Layer: " + GetCurrentLayerTitle() + "\n" +
                   "--------------------------------\n" +
                   "No active tasks.";
        }

        MatchStats stats = null;

        if (MatchStatsManager.Instance != null)
        {
            stats = MatchStatsManager.Instance.currentStats;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Task Layer: " + GetCurrentLayerTitle());
        sb.AppendLine("--------------------------------");

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
            sb.AppendLine("Perspective: " + task.GetPerspectiveText());
            sb.AppendLine("Counterpart: " + task.counterpartTaskId);
            sb.AppendLine("Condition: " + task.completionRule);

            if (TaskChecker.Instance != null && stats != null)
            {
                sb.AppendLine("Progress: " + TaskChecker.Instance.GetProgressText(task, stats));
            }
            else
            {
                sb.AppendLine("Progress: 0 / " + task.targetValue);
            }

            sb.AppendLine("Reward: " + task.GetRewardText());
            sb.AppendLine("Report: " + task.progressReportFrequency);

            if (!task.rewardEnabledInPrototype)
            {
                sb.AppendLine("Prototype Status: Display only, no runtime reward.");
            }
            else
            {
                sb.AppendLine("Prototype Status: Runtime reward enabled.");
            }
        }

        if (shownCount == 0)
        {
            sb.AppendLine();
            sb.AppendLine("No " + GetCurrentLayerTitle() + " tasks assigned in activeTasks.");
        }

        return sb.ToString();
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