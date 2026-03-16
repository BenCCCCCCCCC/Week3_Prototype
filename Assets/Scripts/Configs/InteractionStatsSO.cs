using UnityEngine;

[CreateAssetMenu(menuName = "NightFile/Stats/Interaction Stats")]
public class InteractionStatsSO : ScriptableObject
{
    [Header("Fallback")]
    [Min(0.1f)] public float interactHoldSeconds = 2f;   // 先留着吧

    [Header("Repair")]
    [Min(0.1f)] public float repairHoldSeconds = 45f;    
    [Range(0f, 1f)] public float repairInterruptLoss = 0.06f; 
    [Range(0.1f, 1f)] public float multiRepairEfficiency = 0.85f; // 如果要用两个人破解快点的话

    [Header("Gate")]
    [Min(0.1f)] public float gateOpenHoldSeconds = 18f;  

    [Header("Rescue")]
    [Min(0.1f)] public float rescueHoldSeconds = 2.5f;   

    [Header("Feel")]
    [Min(0f)] public float interactCancelLockSeconds = 0.4f; // 打断/取消等以外中断交互
}