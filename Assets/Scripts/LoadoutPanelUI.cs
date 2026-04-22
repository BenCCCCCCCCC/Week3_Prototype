using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class LoadoutPanelUI : MonoBehaviour
{
    [Header("Loadout Targets")]
    public PlayerLoadout survivorLoadout;
    public PlayerLoadout hunterLoadout;

    [Header("Available Equipment")]
    public EquipmentModifier[] survivorOptions;
    public EquipmentModifier[] hunterOptions;

    [Header("Survivor UI")]
    public TMP_Dropdown survivorSlotADropdown;
    public TMP_Dropdown survivorSlotBDropdown;
    public TMP_Text survivorPreviewText;

    [Header("Hunter UI")]
    public TMP_Dropdown hunterSlotADropdown;
    public TMP_Dropdown hunterSlotBDropdown;
    public TMP_Text hunterPreviewText;

    private bool isRefreshing = false;

    private const string SurvivorSlotAKey = "NF_SurvivorSlotA";
    private const string SurvivorSlotBKey = "NF_SurvivorSlotB";
    private const string HunterSlotAKey = "NF_HunterSlotA";
    private const string HunterSlotBKey = "NF_HunterSlotB";

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        isRefreshing = true;

        SetupDropdown(survivorSlotADropdown, survivorOptions);
        SetupDropdown(survivorSlotBDropdown, survivorOptions);
        SetupDropdown(hunterSlotADropdown, hunterOptions);
        SetupDropdown(hunterSlotBDropdown, hunterOptions);

        LoadSavedSelection();
        RefreshPreviewTexts();

        isRefreshing = false;
    }

    void SetupDropdown(TMP_Dropdown dropdown, EquipmentModifier[] options)
    {
        if (dropdown == null) return;

        dropdown.ClearOptions();

        List<string> names = new List<string>();
        names.Add("Empty");

        if (options != null)
        {
            for (int i = 0; i < options.Length; i++)
            {
                names.Add(options[i] != null ? options[i].modifierName : "Empty");
            }
        }

        dropdown.AddOptions(names);
    }

    void LoadSavedSelection()
    {
        SetDropdownValueAndApply(
            survivorSlotADropdown,
            PlayerPrefs.GetInt(SurvivorSlotAKey, 0),
            true,
            true
        );

        SetDropdownValueAndApply(
            survivorSlotBDropdown,
            PlayerPrefs.GetInt(SurvivorSlotBKey, 0),
            true,
            false
        );

        SetDropdownValueAndApply(
            hunterSlotADropdown,
            PlayerPrefs.GetInt(HunterSlotAKey, 0),
            false,
            true
        );

        SetDropdownValueAndApply(
            hunterSlotBDropdown,
            PlayerPrefs.GetInt(HunterSlotBKey, 0),
            false,
            false
        );
    }

    void SetDropdownValueAndApply(TMP_Dropdown dropdown, int savedIndex, bool isSurvivor, bool isSlotA)
    {
        if (dropdown == null) return;

        if (savedIndex < 0) savedIndex = 0;
        if (savedIndex >= dropdown.options.Count) savedIndex = 0;

        dropdown.value = savedIndex;

        EquipmentModifier selected = null;
        if (savedIndex > 0)
        {
            EquipmentModifier[] source = isSurvivor ? survivorOptions : hunterOptions;
            int sourceIndex = savedIndex - 1;

            if (source != null && sourceIndex >= 0 && sourceIndex < source.Length)
            {
                selected = source[sourceIndex];
            }
        }

        PlayerLoadout loadout = isSurvivor ? survivorLoadout : hunterLoadout;
        if (loadout == null) return;

        if (isSlotA)
        {
            loadout.slotA = selected;
        }
        else
        {
            loadout.slotB = selected;
        }
    }

    public void OnSurvivorSlotAChanged(int index)
    {
        if (isRefreshing) return;
        ApplyDropdownSelection(index, survivorLoadout, survivorOptions, true, SurvivorSlotAKey);
    }

    public void OnSurvivorSlotBChanged(int index)
    {
        if (isRefreshing) return;
        ApplyDropdownSelection(index, survivorLoadout, survivorOptions, false, SurvivorSlotBKey);
    }

    public void OnHunterSlotAChanged(int index)
    {
        if (isRefreshing) return;
        ApplyDropdownSelection(index, hunterLoadout, hunterOptions, true, HunterSlotAKey);
    }

    public void OnHunterSlotBChanged(int index)
    {
        if (isRefreshing) return;
        ApplyDropdownSelection(index, hunterLoadout, hunterOptions, false, HunterSlotBKey);
    }

    void ApplyDropdownSelection(
        int index,
        PlayerLoadout loadout,
        EquipmentModifier[] options,
        bool isSlotA,
        string saveKey
    )
    {
        if (loadout == null) return;

        EquipmentModifier selected = null;

        if (index > 0 && options != null)
        {
            int optionIndex = index - 1;
            if (optionIndex >= 0 && optionIndex < options.Length)
            {
                selected = options[optionIndex];
            }
        }

        if (isSlotA)
        {
            loadout.slotA = selected;
        }
        else
        {
            loadout.slotB = selected;
        }

        PlayerPrefs.SetInt(saveKey, index);
        PlayerPrefs.Save();

        RefreshPreviewTexts();
    }

    void RefreshPreviewTexts()
    {
        if (survivorPreviewText != null && survivorLoadout != null)
        {
            survivorPreviewText.text = BuildPreviewText("Survivor", survivorLoadout);
        }

        if (hunterPreviewText != null && hunterLoadout != null)
        {
            hunterPreviewText.text = BuildPreviewText("Hunter", hunterLoadout);
        }
    }

    string BuildPreviewText(string title, PlayerLoadout loadout)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(title + " Loadout");
        sb.AppendLine(loadout.GetEquippedSummaryText());
        sb.AppendLine();

        if (loadout.slotA != null)
        {
            sb.AppendLine(GetModifierDescription(loadout.slotA));
        }

        if (loadout.slotB != null)
        {
            sb.AppendLine(GetModifierDescription(loadout.slotB));
        }

        return sb.ToString();
    }

    string GetModifierDescription(EquipmentModifier modifier)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("- " + modifier.modifierName);

        if (modifier.cipherRepairSpeedBonusPercent > 0f)
            sb.AppendLine("  Cipher Repair +" + modifier.cipherRepairSpeedBonusPercent + "%");

        if (modifier.rescueSpeedBonusPercent > 0f)
            sb.AppendLine("  Rescue Speed +" + modifier.rescueSpeedBonusPercent + "%");

        if (modifier.genericInteractSpeedBonusPercent > 0f)
            sb.AppendLine("  Generic Interact +" + modifier.genericInteractSpeedBonusPercent + "%");

        if (modifier.survivorDashCooldownBonusPercent > 0f)
            sb.AppendLine("  Dash Cooldown +" + modifier.survivorDashCooldownBonusPercent + "%");

        if (modifier.survivorDashPowerBonusPercent > 0f)
            sb.AppendLine("  Dash Power +" + modifier.survivorDashPowerBonusPercent + "%");

        if (modifier.hunterBasicAttackCooldownBonusPercent > 0f)
            sb.AppendLine("  Basic Attack Cooldown +" + modifier.hunterBasicAttackCooldownBonusPercent + "%");

        if (modifier.hunterSlowSkillCooldownBonusPercent > 0f)
            sb.AppendLine("  Slow Skill Cooldown +" + modifier.hunterSlowSkillCooldownBonusPercent + "%");

        if (modifier.hunterDetectSkillCooldownBonusPercent > 0f)
            sb.AppendLine("  Detect Skill Cooldown +" + modifier.hunterDetectSkillCooldownBonusPercent + "%");

        if (modifier.softCurrencyFlatBonus > 0)
            sb.AppendLine("  Settlement Soft +" + modifier.softCurrencyFlatBonus);

        if (modifier.materialFlatBonus > 0)
            sb.AppendLine("  Settlement Material +" + modifier.materialFlatBonus);

        return sb.ToString();
    }
}