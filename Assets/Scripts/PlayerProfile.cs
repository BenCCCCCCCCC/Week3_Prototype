using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance;

    [Header("Currencies")]
    public int softCurrency;
    public int premiumCurrency;
    public int archiveMaterial;

    private const string SoftCurrencyKey = "NF_SoftCurrency";
    private const string PremiumCurrencyKey = "NF_PremiumCurrency";
    private const string ArchiveMaterialKey = "NF_ArchiveMaterial";

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

    public void AddRewards(int soft, int premium, int material)
    {
        softCurrency += soft;
        premiumCurrency += premium;
        archiveMaterial += material;

        if (softCurrency < 0) softCurrency = 0;
        if (premiumCurrency < 0) premiumCurrency = 0;
        if (archiveMaterial < 0) archiveMaterial = 0;

        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetInt(SoftCurrencyKey, softCurrency);
        PlayerPrefs.SetInt(PremiumCurrencyKey, premiumCurrency);
        PlayerPrefs.SetInt(ArchiveMaterialKey, archiveMaterial);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        softCurrency = PlayerPrefs.GetInt(SoftCurrencyKey, 0);
        premiumCurrency = PlayerPrefs.GetInt(PremiumCurrencyKey, 0);
        archiveMaterial = PlayerPrefs.GetInt(ArchiveMaterialKey, 0);
    }

    public void ResetProfileData()
    {
        softCurrency = 0;
        premiumCurrency = 0;
        archiveMaterial = 0;
        Save();
    }
}