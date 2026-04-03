using UnityEngine;

public class EscapeZone : MonoBehaviour
{
    [Header("References")]
    public MatchManager matchManager;
    public GateController gateController;

    [Header("Other")]
    public string survivorTag = "Survivor";

    [Header("Runtime")]
    public bool escapeEnabled = false;

    [Header("Debug")]
    public bool logEscapeEvents = true;

    public void SetEscapeEnabled(bool enabled)
    {
        escapeEnabled = enabled;

        if (logEscapeEvents)
        {
            Debug.Log("EscapeZone: escapeEnabled = " + escapeEnabled);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!escapeEnabled) return;

        Transform root = other.transform.root;
        if (root == null) return;

        if (!root.CompareTag(survivorTag))
        {
            return;
        }

        if (matchManager != null)
        {
            matchManager.OnSurvivorEscaped(root.gameObject);
        }

        if (logEscapeEvents)
        {
            Debug.Log("EscapeZone: Survivor entered escape zone = " + root.name);
        }
    }
}