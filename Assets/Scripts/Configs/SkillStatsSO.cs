using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Skill Stats", fileName = "SkillStats")]
public class SkillStatsSO : ScriptableObject
{
    [Header("Survivor Dash")]
    public float dashCooldown = 10f;
    public float dashMultiplier = 2.5f;
    public float dashDuration = 1f;
    public float dashInvincibleDuration = 0.5f;
    public float dashReadyShowDuration = 1f;

    [Header("Hunter Slow")]
    public float slowAimWindow = 3f;
    public float slowHitCooldown = 15f;
    public float slowMissCooldown = 10f;
    public float slowRange = 25f;
    public float slowCastRadius = 0.25f;
    public float slowMultiplier = 0.7f;
    public float slowDuration = 2f;
    public float slowReadyShowDuration = 1f;

    [Header("Hunter Detect")]
    public float detectCooldown = 15f;
    public float detectDuration = 5f;
    public float detectCloseRadius = 6f;
    public float detectMaxRadius = 15f;
    public float detectScanInterval = 0.1f;
    public float detectReadyShowDuration = 1f;

    [Header("Hunter Basic Attack")]
    public float attackRange = 2.2f;
    public float attackRadius = 0.9f;
    public float attackWindup = 0.12f;
    public float attackRecovery = 0.35f;
    public float attackCooldown = 0.55f;
    public float attackMinForwardDot = 0.15f;
}
