using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public Image progressFill;
    public GameObject progressBG;

    // 整个提示文字物体（用于显示/隐藏）
    public GameObject hintTextRoot;

    // 真正显示文字的 TextMeshPro 文本组件
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

        SetUI(true);
        UpdateHintText();

        float holdSeconds = Mathf.Max(0.01f, currentTarget.GetHoldSeconds(interactionStats));
        bool eHeld = interactHoldAction.IsPressed();

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
            OnInteractionCompleted();
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
                Debug.LogWarning($"InteractionUI: {other.name} has InteractPoint tag but no InteractionTarget.");
            }
            return;
        }

        currentTarget = target;
        currentTargetCollider = other;
        inRange = true;
        progress = 0f;
        SetProgress(0f);
        UpdateHintText();

        if (showDebugLog)
        {
            Debug.Log($"Enter interact range: {other.name}, Type = {target.interactionType}, Hold = {target.GetHoldSeconds(interactionStats)}s");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other != currentTargetCollider) return;

        if (showDebugLog)
        {
            Debug.Log($"Exit interact range: {other.name}");
        }

        ResetInteractionState(false);
    }

    void OnInteractionCompleted()
    {
        if (currentTarget == null) return;

        if (showDebugLog)
        {
            Debug.Log($"Interact Complete! Target = {currentTarget.name}, Type = {currentTarget.interactionType}");
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

        switch (currentTarget.interactionType)
        {
            case InteractionType.Repair:
                hintLabel.text = "Hold  E to repair and decrypt the file";
                break;

            case InteractionType.Gate:
                hintLabel.text = "Hold E to open the evacuation door";
                break;

            case InteractionType.Rescue:
                hintLabel.text = "Hold  E to rescue your teammate from the chair";
                break;

            default:
                hintLabel.text = "Hold E to interact";
                break;
        }
    }

    void ResetInteractionState(bool keepTarget)
    {
        SetUI(false);
        progress = 0f;
        SetProgress(0f);
        inRange = false;

        if (!keepTarget)
        {
            currentTarget = null;
            currentTargetCollider = null;
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