using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HunterBasicAttack : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public SkillStatsSO skillStats;
    public HunterSlowSkill slowSkill;
    public Transform attackPoint;

    [Header("Other")]
    public string targetTag = "Survivor";

    [Header("Debug")]
    public bool drawGizmos = true;
    public bool logAttackResult = true;

    private bool isAttacking = false;
    private float cooldownTimer = 0f;
    private PlayerLoadout localLoadout;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        localLoadout = GetComponent<PlayerLoadout>();
    }

    void Update()
    {
        if (controller == null || skillStats == null) return;
        if (!controller.enablePlayerInput) return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f)
            {
                cooldownTimer = 0f;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryStartAttack();
        }
    }

    void TryStartAttack()
    {
        if (isAttacking) return;
        if (cooldownTimer > 0f) return;
        if (slowSkill != null && slowSkill.IsAiming) return;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(skillStats.attackWindup);

        DoHitCheck();

        yield return new WaitForSeconds(skillStats.attackRecovery);

        isAttacking = false;
        cooldownTimer = GetModifiedAttackCooldown();
    }

    float GetModifiedAttackCooldown()
    {
        float baseCooldown = skillStats.attackCooldown;

        if (localLoadout == null)
        {
            return baseCooldown;
        }

        float multiplier = localLoadout.GetHunterBasicAttackCooldownMultiplier();
        if (multiplier < 0.01f)
        {
            multiplier = 1f;
        }

        return baseCooldown / multiplier;
    }

    void DoHitCheck()
    {
        Vector3 origin = GetAttackOrigin();

        Collider[] hits = Physics.OverlapSphere(
            origin,
            skillStats.attackRadius,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        CharacterStatus bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (Collider col in hits)
        {
            Transform root = col.transform.root;

            if (!root.CompareTag(targetTag))
            {
                continue;
            }

            CharacterStatus status = root.GetComponent<CharacterStatus>();
            if (status == null)
            {
                continue;
            }

            Vector3 toTarget = root.position - transform.position;
            Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
            float distance = flatToTarget.magnitude;

            if (distance > skillStats.attackRange)
            {
                continue;
            }

            float dot = Vector3.Dot(transform.forward, flatToTarget.normalized);
            if (dot < skillStats.attackMinForwardDot)
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = status;
            }
        }

        if (bestTarget != null)
        {
            bool hitSuccess = bestTarget.TakeHit(transform.position);

            if (hitSuccess)
            {
                if (MatchStatsManager.Instance != null)
                {
                    MatchStatsManager.Instance.AddHunterHit();
                    MatchStatsManager.Instance.AddSurvivorHitTaken();
                }
            }

            if (logAttackResult)
            {
                Debug.Log(hitSuccess
                    ? "Hunter Basic Attack: Hit survivor."
                    : "Hunter Basic Attack: Hit target but blocked.");
            }
        }
        else if (logAttackResult)
        {
            Debug.Log("Hunter Basic Attack: Miss.");
        }
    }

    Vector3 GetAttackOrigin()
    {
        if (attackPoint != null)
        {
            return attackPoint.position;
        }

        return transform.position + transform.forward * 1.2f + Vector3.up * 1.0f;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || skillStats == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackOrigin(), skillStats.attackRadius);
    }
}