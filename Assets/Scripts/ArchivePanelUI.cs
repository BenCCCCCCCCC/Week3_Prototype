using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchivePanelUI : MonoBehaviour
{
    [Header("References")]
    public ArchiveProgress archiveProgress;
    public LobbyUIManager lobbyUIManager;

    [Header("Archive Texts")]
    public TMP_Text archiveLevelText;
    public TMP_Text archiveCostText;
    public TMP_Text archiveStatusText;
    public TMP_Text archiveDescText;

    [Header("Upgrade Button")]
    public Button upgradeButton;
    public TMP_Text upgradeButtonText;

    [Header("Debug")]
    public bool showDebugLog = true;

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
        int currentMaterial = 0;

        if (PlayerProfile.Instance != null)
        {
            currentMaterial = PlayerProfile.Instance.archiveMaterial;
        }

        bool canUpgrade = PlayerProfile.Instance != null && currentMaterial >= cost;

        if (archiveLevelText != null)
        {
            archiveLevelText.text = "Archive Level: " + archiveProgress.archiveLevel;
        }

        if (archiveCostText != null)
        {
            archiveCostText.text = "Material: " + currentMaterial + " / " + cost;
        }

        if (archiveStatusText != null)
        {
            if (canUpgrade)
            {
                archiveStatusText.text = "Status: Ready to upgrade";
            }
            else
            {
                int missing = Mathf.Max(0, cost - currentMaterial);
                archiveStatusText.text = "Status: Not enough Archive Material\nNeed " + missing + " more material";
            }
        }

        if (archiveDescText != null)
        {
            archiveDescText.text =
                "Archive collection is the light progression line of Week 9.\n" +
                "Spend archive materials to increase archive level.\n" +
                "Higher archive level can later unlock story entries, profile nodes, or small passive progression.";
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = canUpgrade;
        }

        if (upgradeButtonText != null)
        {
            if (canUpgrade)
            {
                upgradeButtonText.text = "Upgrade Archive";
            }
            else
            {
                upgradeButtonText.text = "Need Material";
            }
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

        if (showDebugLog)
        {
            if (success)
            {
                Debug.Log("Archive upgraded. Current level = " + archiveProgress.archiveLevel);
            }
            else
            {
                Debug.Log("Archive upgrade failed. Not enough material or missing profile.");
            }
        }

        Refresh();

        if (lobbyUIManager != null)
        {
            lobbyUIManager.RefreshAll();
        }
    }
}