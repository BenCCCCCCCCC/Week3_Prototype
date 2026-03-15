using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Player Stats", fileName = "PlayerStats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Move")]
    public float walkSpeed = 4f;
    public float runSpeed = 6.5f;
    public float crouchSpeed = 2.5f;

    [Header("Look")]
    public float mouseSensitivity = 0.08f;

    [Header("Life")]
    public int maxHP = 2;
    public float hitInvincibleDuration = 0.4f;
    public float hitStunDuration = 0.25f;
}
