using UnityEngine;

// This ScriptableObject is a tweakable config asset (edit values in Inspector).
[CreateAssetMenu(menuName = "Configs/Player Tuning")]
public class PlayerTuningSO : ScriptableObject
{
    [Header("Move Speeds")]
    public float walkSpeed = 4f;
    public float runSpeed = 6.5f;
    public float crouchSpeed = 2.5f;

    [Header("Look")]
    public float mouseSensitivity = 0.08f;
}