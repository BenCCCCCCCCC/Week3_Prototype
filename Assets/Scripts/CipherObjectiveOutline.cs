using UnityEngine;

public class CipherObjectiveOutline : MonoBehaviour
{
    [Header("References")]
    public CipherMachine cipherMachine;
    public GameObject outlineRoot;
    public RoleSwitchController roleSwitchController;

    [Header("Basic Settings")]
    public bool hideWhenCompleted = true;
    public bool hideWhenHunterActive = true;

    [Header("Survivor Objective X-Ray")]
    public Color survivorOutlineColor = new Color(0f, 1f, 1f, 0.28f);

    [Header("Hunter Repair Signal")]
    public bool showHunterRepairSignal = true;
    public Color hunterRepairSignalColor = new Color(1f, 0.35f, 0f, 0.75f);
    public float hunterSignalMinAlpha = 0.15f;
    public float hunterSignalMaxAlpha = 0.85f;
    public float hunterSignalFlashSpeed = 5f;

    private Renderer[] outlineRenderers;
    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        AutoFindReferences();
        CacheRenderers();
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

    void CacheRenderers()
    {
        if (outlineRoot != null)
        {
            outlineRenderers = outlineRoot.GetComponentsInChildren<Renderer>(true);
        }

        propertyBlock = new MaterialPropertyBlock();
    }

    void RefreshOutline()
    {
        if (outlineRoot == null)
        {
            return;
        }

        if (outlineRenderers == null || outlineRenderers.Length == 0)
        {
            CacheRenderers();
        }

        bool isCompleted = false;

        if (cipherMachine != null && cipherMachine.isCompleted)
        {
            isCompleted = true;
        }

        if (hideWhenCompleted && isCompleted)
        {
            SetOutlineVisible(false);
            return;
        }

        bool isSurvivorView = roleSwitchController == null || roleSwitchController.IsSurvivorActive();
        bool isHunterView = roleSwitchController != null && roleSwitchController.IsHunterActive();

        if (isSurvivorView)
        {
            SetOutlineVisible(true);
            SetOutlineColor(survivorOutlineColor);
            return;
        }

        if (isHunterView)
        {
            bool isBeingRepaired = false;

            if (cipherMachine != null && cipherMachine.ActiveRepairerCount > 0)
            {
                isBeingRepaired = true;
            }

            if (showHunterRepairSignal && isBeingRepaired)
            {
                float flash01 = (Mathf.Sin(Time.time * hunterSignalFlashSpeed) + 1f) * 0.5f;
                float alpha = Mathf.Lerp(hunterSignalMinAlpha, hunterSignalMaxAlpha, flash01);

                Color flashColor = hunterRepairSignalColor;
                flashColor.a = alpha;

                SetOutlineVisible(true);
                SetOutlineColor(flashColor);
                return;
            }

            if (hideWhenHunterActive)
            {
                SetOutlineVisible(false);
                return;
            }
        }

        SetOutlineVisible(false);
    }

    void SetOutlineVisible(bool visible)
    {
        if (outlineRoot != null && outlineRoot.activeSelf != visible)
        {
            outlineRoot.SetActive(visible);
        }
    }

    void SetOutlineColor(Color color)
    {
        if (outlineRenderers == null)
        {
            return;
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        for (int i = 0; i < outlineRenderers.Length; i++)
        {
            Renderer renderer = outlineRenderers[i];

            if (renderer == null)
            {
                continue;
            }

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}