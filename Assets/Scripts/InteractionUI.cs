using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public Image progressFill;
    public GameObject progressBG;
    public GameObject hintText;

    [Header("Interaction Stats (must assign)")]
    public InteractionStatsSO interactionStats;

    private float progress = 0f;
    private bool inRange = false;

    private InputAction interactHoldAction;

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

        if (!inRange)
        {
            SetUI(false);
            progress = 0f;
            SetProgress(0f);
            return;
        }

        SetUI(true);

        bool eHeld = interactHoldAction.IsPressed();

        if (eHeld)
            progress += Time.deltaTime / interactionStats.interactHoldSeconds;
        else
            progress = 0f;

        progress = Mathf.Clamp01(progress);
        SetProgress(progress);

        if (progress >= 1f)
        {
            Debug.Log("Interact Complete!");
            progress = 0f;
            SetProgress(0f);
        }
    }

    void SetUI(bool show)
    {
        if (hintText != null) hintText.SetActive(show);
        if (progressBG != null) progressBG.SetActive(show);
        if (progressFill != null) progressFill.gameObject.SetActive(show);
    }

    void SetProgress(float v)
    {
        if (progressFill != null) progressFill.fillAmount = v;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractPoint"))
        {
            inRange = true;
            progress = 0f;
            SetProgress(0f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractPoint"))
        {
            inRange = false;
            progress = 0f;
            SetProgress(0f);
        }
    }
}
