using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SurvivorAutoMoveTest : MonoBehaviour
{
    public PlayerController controller;
    public float moveDistance = 4f;
    public float pauseAtEnds = 0.3f;

    private Vector3 startPos;
    private int direction = 1;
    private float pauseTimer = 0f;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }
    }

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (controller == null) return;
        if (controller.enablePlayerInput) return; // 只在“非玩家控制”时自动移动

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        float speed = controller.GetBaseMoveSpeed() * controller.externalSpeedMultiplier;
        Vector3 moveDir = transform.right * direction;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.Move(moveDir * speed * Time.deltaTime);
        }

        float offset = transform.position.x - startPos.x;

        if (offset >= moveDistance)
        {
            direction = -1;
            pauseTimer = pauseAtEnds;
        }
        else if (offset <= -moveDistance)
        {
            direction = 1;
            pauseTimer = pauseAtEnds;
        }
    }
}