using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("References")]
    public MatchManager matchManager;
    public InteractionTarget interactionTarget;
    public InteractionStatsSO interactionStats;
    public EscapeZone escapeZone;

    [Header("Runtime")]
    public bool isUnlocked = false;
    public bool isOpened = false;
    [Range(0f, 1f)] public float progress01 = 0f;

    [Header("Debug")]
    public bool logGateEvents = true;

    private HashSet<InteractionUI> activeOpeners = new HashSet<InteractionUI>();

    void Awake()
    {
        if (interactionTarget == null)
        {
            interactionTarget = GetComponent<InteractionTarget>();
        }
    }

    void Start()
    {
        if (escapeZone != null)
        {
            escapeZone.SetEscapeEnabled(false);
        }
    }

    void Update()
    {
        if (!isUnlocked) return;
        if (isOpened) return;

        CleanupNullOpeners();

        int openerCount = activeOpeners.Count;
        if (openerCount <= 0) return;

        float gateSeconds = 18f;
        if (interactionStats != null)
        {
            gateSeconds = Mathf.Max(0.1f, interactionStats.gateOpenHoldSeconds);
        }

        float delta = Time.deltaTime / gateSeconds;
        progress01 = Mathf.Clamp01(progress01 + delta);

        if (progress01 >= 1f)
        {
            OpenGate();
        }
    }

    void CleanupNullOpeners()
    {
        activeOpeners.RemoveWhere(ui => ui == null);
    }

    public void BeginOpen(InteractionUI ui)
    {
        if (ui == null) return;
        if (!CanOpen()) return;

        activeOpeners.Add(ui);
    }

    public void EndOpen(InteractionUI ui)
    {
        if (ui == null) return;

        if (activeOpeners.Contains(ui))
        {
            activeOpeners.Remove(ui);
        }
    }

    public bool CanOpen()
    {
        return isUnlocked && !isOpened;
    }

    public void UnlockGate()
    {
        if (isUnlocked) return;

        isUnlocked = true;

        if (logGateEvents)
        {
            Debug.Log("GateController: Gate unlocked = " + gameObject.name);
        }
    }

    void OpenGate()
    {
        if (isOpened) return;

        isOpened = true;
        progress01 = 1f;
        activeOpeners.Clear();

        if (interactionTarget != null)
        {
            interactionTarget.isCompleted = true;
        }

        if (escapeZone != null)
        {
            escapeZone.SetEscapeEnabled(true);
        }

        if (matchManager != null)
        {
            matchManager.OnGateOpened(this);
        }

        if (logGateEvents)
        {
            Debug.Log("GateController: Gate opened = " + gameObject.name);
        }
    }
}