using System.Text;
using TMPro;
using UnityEngine;

public class TaskPanelUI : MonoBehaviour
{
    [Header("References")]
    public MatchSettlement matchSettlement;
    public TMP_Text activeTasksText;
    public TMP_Text lastCompletedTasksText;

    private void OnEnable()
    {
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
            return "No active tasks.";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Active Tasks");

        for (int i = 0; i < matchSettlement.activeTasks.Length; i++)
        {
            TaskDefinition task = matchSettlement.activeTasks[i];
            if (task == null) continue;

            sb.AppendLine();
            sb.AppendLine((i + 1) + ". " + task.taskName);
            sb.AppendLine("Type: " + task.taskType);
            sb.AppendLine("Target: " + task.targetValue);
            sb.AppendLine("Reward: +" + task.softCurrencyReward + " Soft, +" + task.materialReward + " Material");
        }

        return sb.ToString();
    }
}