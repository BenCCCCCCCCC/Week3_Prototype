using UnityEngine;
using UnityEngine.Events;

public enum InteractionType
{
    Default,
    Repair,
    Gate,
    Rescue
}

public class InteractionTarget : MonoBehaviour
{
    [Header("Type")]
    public InteractionType interactionType = InteractionType.Default;

    [Header("Display")]
    public string displayName = "Interact";
    public bool disableAfterComplete = false;
    public bool oneShot = false;

    [Header("Who Can Interact")]
    public bool allowSurvivor = true;
    public bool allowHunter = false;

    [Header("Runtime")]
    public bool isCompleted = false;

    [Header("Callbacks")]
    public UnityEvent onInteractComplete;

    public float GetHoldSeconds(InteractionStatsSO stats)
    {
        if (stats == null) return 1f;

        switch (interactionType)
        {
            case InteractionType.Repair:
                return stats.repairHoldSeconds;

            case InteractionType.Gate:
                return stats.gateOpenHoldSeconds;

            case InteractionType.Rescue:
                return stats.rescueHoldSeconds;

            default:
                return stats.interactHoldSeconds;
        }
    }

    public bool CanBeInteractedBy(GameObject actor)
    {
        if (actor == null) return false;

        Transform root = actor.transform.root;

        if (root.CompareTag("Survivor"))
        {
            return allowSurvivor;
        }

        if (root.CompareTag("Hunter"))
        {
            return allowHunter;
        }

        return false;
    }

    public void CompleteInteraction()
    {
        if (oneShot && isCompleted) return;

        isCompleted = true;
        onInteractComplete?.Invoke();

        if (disableAfterComplete)
        {
            gameObject.SetActive(false);
        }
    }
}