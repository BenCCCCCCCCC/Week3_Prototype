using UnityEngine;

public class CipherObjectiveOutline : MonoBehaviour
{
    [Header("References")]
    public CipherMachine cipherMachine;
    public GameObject outlineRoot;
    public RoleSwitchController roleSwitchController;

    [Header("Settings")]
    public bool hideWhenCompleted = true;
    public bool hideWhenHunterActive = true;

    void Start()
    {
        AutoFindReferences();
        RefreshOutline();
    }

    void Update()
    {
        RefreshOutline();
    }

    void AutoFindReferences()
    {
        if (cipherMachine == null)
        {
            cipherMachine = GetComponent<CipherMachine>();
        }

        if (roleSwitchController == null)
        {
            roleSwitchController = FindFirstObjectByType<RoleSwitchController>();
        }
    }

    void RefreshOutline()
    {
        if (outlineRoot == null)
        {
            return;
        }

        bool shouldShow = true;

        if (hideWhenHunterActive && roleSwitchController != null && !roleSwitchController.IsSurvivorActive())
        {
            shouldShow = false;
        }

        if (hideWhenCompleted && cipherMachine != null && cipherMachine.progress01 >= 1f)
        {
            shouldShow = false;
        }

        if (outlineRoot.activeSelf != shouldShow)
        {
            outlineRoot.SetActive(shouldShow);
        }
    }
}