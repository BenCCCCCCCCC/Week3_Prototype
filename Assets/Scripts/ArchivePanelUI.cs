using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchivePanelUI : MonoBehaviour
{
    [Header("References")]
    public ArchiveProgress archiveProgress;
    public LobbyUIManager lobbyUIManager;
    public TMP_Text archiveLevelText;
    public TMP_Text archiveCostText;
    public TMP_Text archiveDescText;
    public Button upgradeButton;

    private void OnEnable()
    {
        if (archiveProgress == null)
        {
            archiveProgress = ArchiveProgress.Instance;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (archiveProgress == null)
        {
            archiveProgress = ArchiveProgress.Instance;
        }

        if (archiveProgress == null)
        {
            return;
        }

        int cost = archiveProgress.GetUpgradeCost();

        if (archiveLevelText != null)
        {
            archiveLevelText.text = "Archive Level: " + archiveProgress.archiveLevel;
        }

        if (archiveCostText != null)
        {
            archiveCostText.text = "Upgrade Cost: " + cost + " Material";
        }

        if (archiveDescText != null)
        {
            archiveDescText.text =
                "Archive collection is the light progression line of Week 8.\n" +
                "Spend archive materials to increase archive level.\n" +
                "This can later unlock story entries, profile nodes, or small passive progression.";
        }

        if (upgradeButton != null && PlayerProfile.Instance != null)
        {
            upgradeButton.interactable = PlayerProfile.Instance.archiveMaterial >= cost;
        }
    }

    public void OnClickUpgrade()
    {
        if (archiveProgress == null)
        {
            archiveProgress = ArchiveProgress.Instance;
        }

        bool success = false;

        if (archiveProgress != null)
        {
            success = archiveProgress.TryUpgrade();
        }

        if (success)
        {
            Debug.Log("Archive upgraded. Current level = " + archiveProgress.archiveLevel);
        }
        else
        {
            Debug.Log("Archive upgrade failed. Not enough material or missing profile.");
        }

        Refresh();

        if (lobbyUIManager != null)
        {
            lobbyUIManager.RefreshAll();
        }
    }
}