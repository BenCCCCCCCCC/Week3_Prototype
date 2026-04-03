using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public Image progressFill;
    public GameObject progressBG;
    public GameObject hintTextRoot;
    public TMP_Text hintLabel;

    [Header("Interaction Stats (must assign)")]
    public InteractionStatsSO interactionStats;

    [Header("Debug")]
    public bool showDebugLog = true;

    private float progress = 0f;
    private bool inRange = false;

    private InputAction interactHoldAction;
    private InteractionTarget currentTarget;
    private Collider currentTargetCollider;

    private CipherMachine currentCipher;
    private GateController currentGate;
    private ChairController currentChair;

    public bool IsInteractingRescue =>
        inRange &&
        currentTarget != null &&
        currentTarget.interactionType == InteractionType.Rescue &&
        progress > 0f;

    void Awake()
    {
        interactHoldAction = new InputAction(
            name: "InteractHold",
            type: InputActionType.Button,
            binding: "<Keyboard>/e"
        );
    }

    void OnEnable()
    {
        interactHoldAction.Enable();
    }

    void OnDisable()
    {
        interactHoldAction.Disable();
        StopSpecialInteractions();
    }

    void Start()
    {
        SetUI(false);
        SetProgress(0f);
    }

    void Update()
    {
        if (interactionStats == null)
        {
            Debug.LogError("InteractionUI: interactionStats is not assigned.");
            return;
        }

        if (!inRange || currentTarget == null)
        {
            ResetInteractionState(false);
            return;
        }

        if (!currentTarget.CanBeInteractedBy(gameObject))
        {
            ResetInteractionState(false);
            return;
        }

        if (currentChair != null &&
            currentTarget.interactionType == InteractionType.Rescue &&
            !currentChair.CanRescue())
        {
            ResetInteractionState(false);
            return;
        }

        SetUI(true);
        UpdateHintText();

        bool eHeld = interactHoldAction.IsPressed();

        if (currentCipher != null)
        {
            HandleCipherInteraction(eHeld);
            return;
        }

        if (currentGate != null)
        {
            HandleGateInteraction(eHeld);
            return;
        }

        HandleDefaultInteraction(eHeld);
    }

    void HandleCipherInteraction(bool eHeld)
    {
        if (currentCipher == null) return;

        if (!currentTarget.CanBeInteractedBy(gameObject))
        {
            currentCipher.EndRepair(this);
            SetProgress(currentCipher.progress01);
            return;
        }

        if (currentCipher.isCompleted)
        {
            currentCipher.EndRepair(this);
            SetProgress(currentCipher.progress01);
            return;
        }

        if (eHeld)
        {
            currentCipher.BeginRepair(this);
        }
        else
        {
            currentCipher.EndRepair(this);
        }

        SetProgress(currentCipher.progress01);
    }

    void HandleGateInteraction(bool eHeld)
    {
        if (currentGate == null) return;

        if (!currentTarget.CanBeInteractedBy(gameObject))
        {
            currentGate.EndOpen(this);
            SetProgress(currentGate.progress01);
            return;
        }

        if (!currentGate.isUnlocked)
        {
            currentGate.EndOpen(this);
            SetProgress(0f);
            return;
        }

        if (currentGate.isOpened)
        {
            currentGate.EndOpen(this);
            SetProgress(1f);
            return;
        }

        if (eHeld)
        {
            currentGate.BeginOpen(this);
        }
        else
        {
            currentGate.EndOpen(this);
        }

        SetProgress(currentGate.progress01);
    }

    void HandleDefaultInteraction(bool eHeld)
    {
        float holdSeconds = Mathf.Max(0.01f, currentTarget.GetHoldSeconds(interactionStats));

        if (eHeld)
        {
            progress += Time.deltaTime / holdSeconds;
        }
        else
        {
            progress = 0f;
        }

        progress = Mathf.Clamp01(progress);
        SetProgress(progress);

        if (progress >= 1f)
        {
            OnDefaultInteractionCompleted();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("InteractPoint")) return;

        InteractionTarget target = other.GetComponent<InteractionTarget>();
        if (target == null)
        {
            if (showDebugLog)
            {
                Debug.LogWarning("InteractionUI: " + other.name + " has InteractPoint tag but no InteractionTarget.");
            }
            return;
        }

        if (!target.CanBeInteractedBy(gameObject))
        {
            if (showDebugLog)
            {
                Debug.Log("InteractionUI: " + gameObject.name + " is not allowed to interact with " + other.name);
            }
            return;
        }

        currentTarget = target;
        currentTargetCollider = other;
        currentCipher = other.GetComponent<CipherMachine>();
        currentGate = other.GetComponent<GateController>();
        currentChair = other.GetComponent<ChairController>();

        if (currentChair != null &&
            currentTarget.interactionType == InteractionType.Rescue &&
            !currentChair.CanRescue())
        {
            currentTarget = null;
            currentTargetCollider = null;
            currentCipher = null;
            currentGate = null;
            currentChair = null;
            return;
        }

        inRange = true;
        progress = 0f;

        if (currentCipher != null)
        {
            SetProgress(currentCipher.progress01);
        }
        else if (currentGate != null)
        {
            SetProgress(currentGate.progress01);
        }
        else
        {
            SetProgress(0f);
        }

        UpdateHintText();

        if (showDebugLog)
        {
            Debug.Log("Enter interact range: " + other.name + ", Type = " + target.interactionType);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other != currentTargetCollider) return;

        if (showDebugLog)
        {
            Debug.Log("Exit interact range: " + other.name);
        }

        ResetInteractionState(false);
    }

    void OnDefaultInteractionCompleted()
    {
        if (currentTarget == null) return;

        if (currentChair != null && currentTarget.interactionType == InteractionType.Rescue)
        {
            bool rescueSuccess = currentChair.RescueOccupant();

            if (showDebugLog)
            {
                Debug.Log("Rescue complete on chair: " + currentChair.name + ", success = " + rescueSuccess);
            }

            ResetInteractionState(false);
            return;
        }

        if (showDebugLog)
        {
            Debug.Log("Default interaction complete: " + currentTarget.name);
        }

        currentTarget.CompleteInteraction();

        if (currentTarget.oneShot || currentTarget.disableAfterComplete)
        {
            ResetInteractionState(false);
        }
        else
        {
            progress = 0f;
            SetProgress(0f);
            UpdateHintText();
        }
    }

    void UpdateHintText()
    {
        if (hintLabel == null || currentTarget == null) return;

        if (currentCipher != null)
        {
            if (currentCipher.isCompleted)
            {
                hintLabel.text = "Cipher already completed";
            }
            else
            {
                hintLabel.text = "Hold E to repair and decrypt the file";
            }
            return;
        }

        if (currentGate != null)
        {
            if (currentGate.isOpened)
            {
                hintLabel.text = "Gate already opened";
            }
            else if (!currentGate.isUnlocked)
            {
                hintLabel.text = "Door locked. Complete all required ciphers first";
            }
            else
            {
                hintLabel.text = "Hold E to open the evacuation door";
            }
            return;
        }

        if (currentChair != null && currentTarget.interactionType == InteractionType.Rescue)
        {
            if (currentChair.CanRescue())
            {
                hintLabel.text = "Hold E to rescue your teammate from the chair";
            }
            else
            {
                hintLabel.text = "No teammate on chair";
            }
            return;
        }

        switch (currentTarget.interactionType)
        {
            case InteractionType.Repair:
                hintLabel.text = "Hold E to repair and decrypt the file";
                break;

            case InteractionType.Gate:
                hintLabel.text = "Hold E to open the evacuation door";
                break;

            case InteractionType.Rescue:
                hintLabel.text = "Hold E to rescue your teammate from the chair";
                break;

            default:
                hintLabel.text = "Hold E to interact";
                break;
        }
    }

    public void ForceInterruptInteraction(string reason = "")
    {
        if (!inRange && currentTarget == null) return;

        if (showDebugLog)
        {
            if (string.IsNullOrEmpty(reason))
            {
                Debug.Log("InteractionUI: interaction interrupted.");
            }
            else
            {
                Debug.Log("InteractionUI: interaction interrupted. Reason = " + reason);
            }
        }

        ResetInteractionState(false);
    }

    void StopSpecialInteractions()
    {
        if (currentCipher != null)
        {
            currentCipher.EndRepair(this);
        }

        if (currentGate != null)
        {
            currentGate.EndOpen(this);
        }
    }

    void ResetInteractionState(bool keepTarget)
    {
        StopSpecialInteractions();

        SetUI(false);
        progress = 0f;
        SetProgress(0f);
        inRange = false;

        if (!keepTarget)
        {
            currentTarget = null;
            currentTargetCollider = null;
            currentCipher = null;
            currentGate = null;
            currentChair = null;
        }
    }

    void SetUI(bool show)
    {
        if (hintTextRoot != null) hintTextRoot.SetActive(show);
        if (progressBG != null) progressBG.SetActive(show);
        if (progressFill != null) progressFill.gameObject.SetActive(show);
    }

    void SetProgress(float value)
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = value;
        }
    }
}