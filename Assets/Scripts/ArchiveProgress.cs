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
}