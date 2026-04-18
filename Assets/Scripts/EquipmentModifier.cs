using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentModifier", menuName = "NightFile/Equipment Modifier")]
public class EquipmentModifier : ScriptableObject
{
    [Header("Basic Info")]
    public string modifierId = "equip_001";
    public string modifierName = "Equipment";
    [TextArea] public string description = "A small loadout modifier.";

    [Header("Survivor Bonuses")]
    [Range(0f, 100f)]
    public float rescueSpeedBonusPercent = 0f;

    [Range(0f, 100f)]
    public float genericInteractSpeedBonusPercent = 0f;

    [Range(0f, 100f)]
    public float cipherRepairSpeedBonusPercent = 0f;

    [Range(0f, 100f)]
    public float survivorDashCooldownBonusPercent = 0f;

    [Range(0f, 100f)]
    public float survivorDashPowerBonusPercent = 0f;

    [Header("Hunter Bonuses")]
    [Range(0f, 100f)]
    public float hunterBasicAttackCooldownBonusPercent = 0f;

    [Range(0f, 100f)]
    public float hunterSlowSkillCooldownBonusPercent = 0f;

    [Range(0f, 100f)]
    public float hunterDetectSkillCooldownBonusPercent = 0f;

    [Header("Settlement Bonuses")]
    public int softCurrencyFlatBonus = 0;
    public int materialFlatBonus = 0;
}