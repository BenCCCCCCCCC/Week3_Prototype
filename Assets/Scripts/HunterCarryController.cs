using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HunterCarryController : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Transform carryAnchor;
    public TMP_Text carryHintText;

    [Header("Range")]
    public float pickupRange = 2.2f;
    public float chairRange = 2.5f;

    [Header("Other")]
    public string survivorTag = "Survivor";

    [Header("Debug")]
    public bool showDebugLog = true;

    private InputAction carryAction;
    private CharacterStatus carriedTarget;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        carryAction = new InputAction(
            name: "CarryAction",
            type: InputActionType.Button,
            binding: "<Keyboard>/r"
        );
    }

    void OnEnable()
    {
        carryAction.Enable();
        HideCarryHint();
    }

    void OnDisable()
    {
        carryAction.Disable();
        HideCarryHint();
    }

    void Update()
    {
        UpdateCarryHint();

        if (controller == null) return;
        if (!controller.enablePlayerInput) return;

        if (carryAction.WasPressedThisFrame())
        {
            HandleCarryAction();
        }
    }

    void HandleCarryAction()
    {
        if (carriedTarget == null)
        {
            TryPickUpNearestDownedSurvivor();
            return;
        }

        ChairController nearbyChair = FindNearestAvailableChair();

        if (nearbyChair != null)
        {
            bool success = nearbyChair.PlaceSurvivor(carriedTarget);

            if (success)
            {
                if (showDebugLog)
                {
                    Debug.Log("HunterCarryController: placed survivor on chair = " + nearbyChair.name);
                }

                carriedTarget = null;
                return;
            }
        }

        DropCarriedTarget();
    }

    void TryPickUpNearestDownedSurvivor()
    {
        CharacterStatus target = FindNearestDownedSurvivor();

        if (target == null)
        {
            if (showDebugLog)
            {
                Debug.Log("HunterCarryController: no downed survivor nearby.");
            }
            return;
        }

        if (carryAnchor == null)
        {
            Debug.LogWarning("HunterCarryController: carryAnchor is not assigned.");
            return;
        }

        bool success = target.StartCarry(carryAnchor);

        if (success)
        {
            carriedTarget = target;

            if (showDebugLog)
            {
                Debug.Log("HunterCarryController: picked up survivor = " + target.name);
            }
        }
    }

    CharacterStatus FindNearestDownedSurvivor()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            pickupRange,
            ~0,
            QueryTriggerInteraction.Collide
        );

        CharacterStatus nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            Transform root = hit.transform.root;

            if (!root.CompareTag(survivorTag))
            {
                continue;
            }

            CharacterStatus status = root.GetComponent<CharacterStatus>();
            if (status == null) continue;
            if (!status.CanBePickedUp()) continue;

            float distance = Vector3.Distance(transform.position, root.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = status;
            }
        }

        return nearest;
    }

    ChairController FindNearestAvailableChair()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            chairRange,
            ~0,
            QueryTriggerInteraction.Collide
        );

        ChairController nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            ChairController chair = hit.GetComponent<ChairController>();
            if (chair == null)
            {
                chair = hit.GetComponentInParent<ChairController>();
            }

            if (chair == null) continue;
            if (!chair.CanPlace(carriedTarget)) continue;

            float distance = Vector3.Distance(transform.position, chair.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = chair;
            }
        }

        return nearest;
    }

    void DropCarriedTarget()
    {
        if (carriedTarget == null) return;

        carriedTarget.DropFromCarryToGround();

        if (showDebugLog)
        {
            Debug.Log("HunterCarryController: dropped carried survivor = " + carriedTarget.name);
        }

        carriedTarget = null;
    }

    void UpdateCarryHint()
    {
        if (carryHintText == null) return;
        if (controller == null || !controller.enablePlayerInput)
        {
            HideCarryHint();
            return;
        }

        if (carriedTarget == null)
        {
            CharacterStatus nearbyTarget = FindNearestDownedSurvivor();

            if (nearbyTarget != null)
            {
                ShowCarryHint("Press R to pick up");
                return;
            }

            HideCarryHint();
            return;
        }

        ChairController nearbyChair = FindNearestAvailableChair();

        if (nearbyChair != null)
        {
            ShowCarryHint("Press R to place on chair");
            return;
        }

        ShowCarryHint("Press R to drop");
    }

    void ShowCarryHint(string text)
    {
        if (carryHintText == null) return;

        carryHintText.gameObject.SetActive(true);
        carryHintText.text = text;
    }

    void HideCarryHint()
    {
        if (carryHintText == null) return;

        carryHintText.text = "";
        carryHintText.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chairRange);
    }
}