using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Interaction Stats", fileName = "InteractionStats")]
public class InteractionStatsSO : ScriptableObject
{
    [Header("Generic Hold Interaction")]
    public float interactHoldSeconds = 2f;
}
