using UnityEngine;

public class ArchiveProgress : MonoBehaviour
{
    public static ArchiveProgress Instance;

    [Header("Archive Data")]
    public int archiveLevel = 1;
    public int baseUpgradeCost = 2;

    private const string ArchiveLevelKey = "NF_ArchiveLevel";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetUpgradeCost()
    {
        return baseUpgradeCost + Mathf.Max(0, archiveLevel - 1);
    }

    public bool TryUpgrade()
    {
        if (PlayerProfile.Instance == null)
        {
            return false;
        }

        int cost = GetUpgradeCost();

        if (PlayerProfile.Instance.archiveMaterial < cost)
        {
            return false;
        }

        PlayerProfile.Instance.AddRewards(0, 0, -cost);
        archiveLevel++;
        Save();
        return true;
    }

    public void Save()
    {
        PlayerPrefs.SetInt(ArchiveLevelKey, archiveLevel);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        archiveLevel = PlayerPrefs.GetInt(ArchiveLevelKey, 1);
    }

    [ContextMenu("Reset Archive To Level 1")]
    public void ResetArchiveToLevel1()
    {
        archiveLevel = 1;
        Save();
        Debug.Log("Archive level reset to 1.");
    }

    [ContextMenu("Delete Archive Save Key")]
    public void DeleteArchiveSaveKey()
    {
        PlayerPrefs.DeleteKey(ArchiveLevelKey);
        PlayerPrefs.Save();
        Load();
        Debug.Log("Archive save key deleted. Current archive level = " + archiveLevel);
    }
}