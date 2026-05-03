using UnityEngine;

public enum PlayableRole
{
    Hunter,
    Survivor
}

public class RoleSwitchController : MonoBehaviour
{
    [Header("Current Role")]
    public PlayableRole currentRole = PlayableRole.Hunter;

    [Header("Hunter")]
    public PlayerController hunterController;
    public Camera hunterCamera;
    public AudioListener hunterAudioListener;
    public InteractionUI hunterInteractionUI;
    public HunterSlowSkill hunterSlowSkill;
    public HunterDetectSkill hunterDetectSkill;
    public HunterBasicAttack hunterBasicAttack;
    public HunterCarryController hunterCarryController;

    [Header("Survivor Player")]
    public PlayerController survivorController;
    public Camera survivorCamera;
    public AudioListener survivorAudioListener;
    public InteractionUI survivorInteractionUI;
    public SurvivorDashSkill survivorDashSkill;

    [Header("Dummy Survivor")]
    public PlayerController dummyController;
    public Camera dummyCamera;
    public AudioListener dummyAudioListener;
    public InteractionUI dummyInteractionUI;
    public SurvivorAutoMoveTest dummyAutoMoveTest;

    [Header("Debug")]
    public bool allowKeyboardSwitch = true;
    public bool logRoleSwitch = true;

    void Start()
    {
        AutoFindMissingReferences();
        ValidateReferences();

        if (currentRole == PlayableRole.Hunter)
        {
            SelectHunter();
        }
        else
        {
            SelectSurvivor();
        }
    }

    void Update()
    {
        if (!allowKeyboardSwitch)
        {
            return;
        }

        // Keep 1 / 2 switch for testing.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectHunter();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSurvivor();
        }
    }

    public void SelectHunter()
    {
        currentRole = PlayableRole.Hunter;
        DisableAllRoles();

        if (hunterController != null)
        {
            hunterController.SetPlayerInputEnabled(true);
        }

        SetCameraState(hunterCamera, hunterAudioListener, true);

        if (hunterInteractionUI != null)
        {
            hunterInteractionUI.enabled = true;
        }

        if (hunterSlowSkill != null)
        {
            hunterSlowSkill.enabled = true;
        }

        if (hunterDetectSkill != null)
        {
            hunterDetectSkill.enabled = true;
        }

        if (hunterBasicAttack != null)
        {
            hunterBasicAttack.enabled = true;
        }

        if (hunterCarryController != null)
        {
            hunterCarryController.enabled = true;
        }

        if (dummyAutoMoveTest != null)
        {
            dummyAutoMoveTest.enabled = true;
        }

        if (logRoleSwitch)
        {
            Debug.Log("Role Switch: Hunter Active");
            DebugCurrentCameraState();
        }
    }

    public void SelectSurvivor()
    {
        currentRole = PlayableRole.Survivor;
        DisableAllRoles();

        if (survivorController != null)
        {
            survivorController.SetPlayerInputEnabled(true);
        }

        SetCameraState(survivorCamera, survivorAudioListener, true);

        if (survivorInteractionUI != null)
        {
            survivorInteractionUI.enabled = true;
        }

        if (survivorDashSkill != null)
        {
            survivorDashSkill.enabled = true;
        }

        if (dummyAutoMoveTest != null)
        {
            dummyAutoMoveTest.enabled = true;
        }

        if (logRoleSwitch)
        {
            Debug.Log("Role Switch: Survivor Active");
            DebugCurrentCameraState();
        }
    }

    public bool IsHunterActive()
    {
        return currentRole == PlayableRole.Hunter;
    }

    public bool IsSurvivorActive()
    {
        return currentRole == PlayableRole.Survivor;
    }

    void DisableAllRoles()
    {
        if (hunterController != null)
        {
            hunterController.SetPlayerInputEnabled(false);
        }

        if (survivorController != null)
        {
            survivorController.SetPlayerInputEnabled(false);
        }

        if (dummyController != null)
        {
            dummyController.SetPlayerInputEnabled(false);
        }

        SetCameraState(hunterCamera, hunterAudioListener, false);
        SetCameraState(survivorCamera, survivorAudioListener, false);
        SetCameraState(dummyCamera, dummyAudioListener, false);

        if (hunterInteractionUI != null)
        {
            hunterInteractionUI.enabled = false;
        }

        if (survivorInteractionUI != null)
        {
            survivorInteractionUI.enabled = false;
        }

        if (dummyInteractionUI != null)
        {
            dummyInteractionUI.enabled = false;
        }

        if (hunterSlowSkill != null)
        {
            hunterSlowSkill.enabled = false;
        }

        if (hunterDetectSkill != null)
        {
            hunterDetectSkill.enabled = false;
        }

        if (hunterBasicAttack != null)
        {
            hunterBasicAttack.enabled = false;
        }

        if (hunterCarryController != null)
        {
            hunterCarryController.enabled = false;
        }

        if (survivorDashSkill != null)
        {
            survivorDashSkill.enabled = false;
        }

        if (dummyAutoMoveTest != null)
        {
            dummyAutoMoveTest.enabled = false;
        }
    }

    void SetCameraState(Camera cam, AudioListener listener, bool active)
    {
        if (cam != null)
        {
            cam.enabled = active;
        }

        if (listener != null)
        {
            listener.enabled = active;
        }
    }

    void AutoFindMissingReferences()
    {
        if (hunterCarryController == null)
        {
            if (hunterController != null)
            {
                hunterCarryController = hunterController.GetComponent<HunterCarryController>();
            }
        }

        if (hunterCarryController == null && hunterController != null)
        {
            hunterCarryController = hunterController.GetComponentInChildren<HunterCarryController>();
        }

        if (hunterCarryController == null)
        {
            hunterCarryController = FindFirstObjectByType<HunterCarryController>();
        }
    }
    void ValidateReferences()
    {
        if (!logRoleSwitch)
        {
            return;
        }

        Debug.Log("=== RoleSwitchController Reference Check ===");

        Debug.Log("Hunter Controller: " + (hunterController != null ? "OK" : "Missing"));
        Debug.Log("Hunter Camera: " + (hunterCamera != null ? hunterCamera.name : "Missing"));
        Debug.Log("Hunter AudioListener: " + (hunterAudioListener != null ? "OK" : "Missing"));
        Debug.Log("Hunter Slow Skill: " + (hunterSlowSkill != null ? "OK" : "Missing"));
        Debug.Log("Hunter Detect Skill: " + (hunterDetectSkill != null ? "OK" : "Missing"));
        Debug.Log("Hunter Basic Attack: " + (hunterBasicAttack != null ? "OK" : "Missing"));
        Debug.Log("Hunter Carry Controller: " + (hunterCarryController != null ? "OK" : "Missing"));

        Debug.Log("Survivor Controller: " + (survivorController != null ? "OK" : "Missing"));
        Debug.Log("Survivor Camera: " + (survivorCamera != null ? survivorCamera.name : "Missing"));
        Debug.Log("Survivor AudioListener: " + (survivorAudioListener != null ? "OK" : "Missing"));
        Debug.Log("Survivor Dash Skill: " + (survivorDashSkill != null ? "OK" : "Missing"));

        Debug.Log("Dummy Controller: " + (dummyController != null ? "OK" : "Missing"));
        Debug.Log("Dummy Camera: " + (dummyCamera != null ? dummyCamera.name : "Missing"));
        Debug.Log("Dummy AudioListener: " + (dummyAudioListener != null ? "OK" : "Missing"));
        Debug.Log("Dummy Auto Move Test: " + (dummyAutoMoveTest != null ? "OK" : "Missing"));
    }

    void DebugCurrentCameraState()
    {
        Debug.Log("Current Role: " + currentRole);
        Debug.Log("Hunter Camera Enabled: " + (hunterCamera != null && hunterCamera.enabled));
        Debug.Log("Survivor Camera Enabled: " + (survivorCamera != null && survivorCamera.enabled));
        Debug.Log("Dummy Camera Enabled: " + (dummyCamera != null && dummyCamera.enabled));
        Debug.Log("Dummy Auto Move Enabled: " + (dummyAutoMoveTest != null && dummyAutoMoveTest.enabled));
    }
}