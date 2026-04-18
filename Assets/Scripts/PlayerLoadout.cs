using UnityEngine;

public class PlayerLoadout : MonoBehaviour
{
    [Header("Two Slots")]
    public EquipmentModifier slotA;
    public EquipmentModifier slotB;

    public float GetRescueSpeedMultiplier()
    {
        float totalPercent = 0f;

        if (slotA != null) totalPercent += slotA.rescueSpeedBonusPercent;
        if (slotB != null) totalPercent += slotB.rescueSpeedBonusPercent;

        return 1f + totalPercent / 100f;
    }

    public float GetGenericInteractSpeedMultiplier()
    {
        float totalPercent = 0f;

        if (slotA != null) totalPercent += slotA.genericInteractSpeedBonusPercent;
        if (slotB != null) totalPercent += slotB.genericInteractSpeedBonusPercent;

        return 1f + totalPercent / 100f;
    }

    public float GetCipherRepairSpeedMultiplier()
    {
        float totalPercent = 0f;

        if (slotA != null) totalPercent += slotA.cipherRepairSpeedBonusPercent;
        if (slotB != null) totalPercent += slotB.cipherRepairSpeedBonusPercent;

        return 1f + totalPercent / 100f;
    }

    public float GetSurvivorDashCooldownMultiplier()
    {
        float totalPercent = 0f;

        if (slotA != null) totalPercent += slotA.survivorDashCooldownBonusPercent;
        if (slotB != null) totalPercent += slotB.survivorDashCooldownBonusPercent;

        return 1f + totalPercent / 100f;
    }

    public float GetSurvivorDashPowerMultiplier()
    {
        float totalPercent = 0f;

        if (slotA != null) totalPercent += slotA.survivorDashPowerBonusPercent;
        if (slotB != null) totalPercent += slotB.survivorDashPowerBonusPercent;

        return 1f + totalPercent / 100f;
    }

    public float GetHunterBasicAttackCooldownMultiplier()
    {
        float totalPercent = 0f;

        if (slotA != null) totalPercent += slotA.hunterBasicAttackCooldownBonusPercent;
        if (slotB != null) totalPercent += slotB.hunterBasicAttackCooldownBonusPercent;

        return 1f + totalPercent / 100f;
    }

    public float GetHunterSlowSkillCooldownMultiplier()
    {
        float totalPercent = 0f;

        if (slotA != null) totalPercent += slotA.hunterSlowSkillCooldownBonusPercent;
        if (slotB != null) totalPercent += slotB.hunterSlowSkillCooldownBonusPercent;

        return 1f + totalPercent / 100f;
    }

    public float GetHunterDetectSkillCooldownMultiplier()
    {
        float totalPercent = 0f;

        if (slotA != null) totalPercent += slotA.hunterDetectSkillCooldownBonusPercent;
        if (slotB != null) totalPercent += slotB.hunterDetectSkillCooldownBonusPercent;

        return 1f + totalPercent / 100f;
    }

    public int GetSoftCurrencyBonus()
    {
        int total = 0;

        if (slotA != null) total += slotA.softCurrencyFlatBonus;
        if (slotB != null) total += slotB.softCurrencyFlatBonus;

        return total;
    }

    public int GetMaterialBonus()
    {
        int total = 0;

        if (slotA != null) total += slotA.materialFlatBonus;
        if (slotB != null) total += slotB.materialFlatBonus;

        return total;
    }

    public string GetEquippedSummaryText()
    {
        string slotAText = slotA != null ? "Slot A: " + slotA.modifierName : "Slot A: Empty";
        string slotBText = slotB != null ? "Slot B: " + slotB.modifierName : "Slot B: Empty";
        return slotAText + "\n" + slotBText;
    }
}