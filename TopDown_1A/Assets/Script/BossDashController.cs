using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class BossDashController : MonoBehaviour
{
    [Header("보스 스텟 데이터 파일")]
    [SerializeField] private EnemyData bossData;

    [Header("🏃 돌진(Dash) 세부 설정")]
    [SerializeField] private float dashSpeedMultiplier = 4f; // 평소 속도의 몇 배로 돌진할 것인가?
    [SerializeField] private float dashCooldown = 5f;        // 돌진 주기 (5초마다)
    [SerializeField] private float chargeDuration = 1.5f;     // 돌진 전 기 모으는 시간 (예고 타임)
    [SerializeField] private float dashDuration = 0.8f;       // 실제 돌진하는 시간
    [SerializeField] private float groggyDuration = 1.2f;     // 돌진 후 지쳐서 멈추는 시간

    // 보스 상태 관리를 위한 열거형(State)
    private enum BossState { Chase, Charge, Dash, Groggy }
    private BossState currentState = BossState.Chase;

    private float currentHp;
    private float dashTimer = 0f;
    private Vector2 dashDirection;

    // 내부 컴포넌트 변수
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform playerTransform;
    private Vector2 moveDirection;

    [HideInInspector] public bool isDead = false;
    public System.Action OnDestroyedEvent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // 물리 연산이 수면 상태에 빠져 충돌이 씹히는 것을 방지
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        if (bossData != null)
        {
            currentHp = bossData.maxHp;
            sr.sprite = bossData.enemySprite;
        }
        else
        {
            Debug.LogError($"{gameObject.name}에 Boss용 EnemyData가 등록되지 않았습니다!");
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (isDead || playerTransform == null || bossData == null) return;

        // 1. 상태별 타이머 및 로직 처리
        switch (currentState)
        {
            case BossState.Chase:
                // 평소에는 플레이어를 추격하며 돌진 쿨타임을 채웁니다.
                CalculateChaseDirection();
                dashTimer += Time.deltaTime;
                if (dashTimer >= dashCooldown)
                {
                    dashTimer = 0f;
                    StartCoroutine(DashPatternRoutine()); // 돌진 시퀀스 스타트!
                }
                break;

            case BossState.Charge:
                // 기 모으는 중에는 이동 방향 없음 (제자리 정지)
                moveDirection = Vector2.zero;
                break;

            case BossState.Dash:
                // 이미 정해진 돌진 방향으로만 직진 (Update에서는 아무것도 안 함)
                break;

            case BossState.Groggy:
                // 지쳐서 헉헉대는 중 (정지)
                moveDirection = Vector2.zero;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || bossData == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 2. 상태별 실제 물리 이동 처리
        if (currentState == BossState.Chase)
        {
            rb.linearVelocity = moveDirection * bossData.moveSpeed;
        }
        else if (currentState == BossState.Dash)
        {
            // 평소 속도 * 배율(예: 4배)로 돌진 방향을 향해 강력하게 밀어붙임
            rb.linearVelocity = dashDirection * (bossData.moveSpeed * dashSpeedMultiplier);
        }
        else
        {
            // 기 모으기 및 그로기 상태일 때는 속도 0
            rb.linearVelocity = Vector2.zero;
        }
    }

    // 플레이어 추격 방향 계산 및 이미지 뒤집기
    private void CalculateChaseDirection()
    {
        float distanceSqr = (playerTransform.position - transform.position).sqrMagnitude;
        float chaseRadiusSqr = bossData.chaseRadius * bossData.chaseRadius;

        if (distanceSqr <= chaseRadiusSqr)
        {
            moveDirection = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            HandleSpriteFlip(moveDirection);
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    // 좌우 반전 처리
    private void HandleSpriteFlip(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        if (dir.x > 0.01f) sr.flipX = false;
        else if (dir.x < -0.01f) sr.flipX = true;
    }

    // ⭐ [핵심 패턴 코루틴] 기 모으기 ➔ 돌진 ➔ 지침 시퀀스를 제어합니다.
    private IEnumerator DashPatternRoutine()
    {
        // -------------------------------------------------------------
        // [STEP 1] 기 모으기 (Charge)
        // -------------------------------------------------------------
        currentState = BossState.Charge;
        Debug.Log($"🔥 [보스 패턴] {bossData.enemyName}이 돌진을 위해 기를 모읍니다!");
        
        // 돌진 직전, 플레이어가 서 있는 방향을 타겟으로 고정 조준합니다.
        dashDirection = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        HandleSpriteFlip(dashDirection); // 조준한 방향 쳐다보기

        // 단 1장의 이미지를 코드 유니티 연산으로 부르르 떨게 만들어 경고 신호를 줍니다.
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            // 빨간색으로 깜빡이면서 미세하게 좌우로 진동
            sr.color = (Mathf.FloorToInt(elapsed * 15f) % 2 == 0) ? Color.red : Color.white;
            transform.position = originalPosition + (Vector3)Random.insideUnitCircle * 0.08f;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = Color.white; // 색상 원상 복구

        // -------------------------------------------------------------
        // [STEP 2] 초고속 돌진 (Dash)
        // -------------------------------------------------------------
        currentState = BossState.Dash;
        Debug.Log($"⚡ [보스 패턴] {bossData.enemyName} 초고속 돌진!!");
        
        // 보스 몸에 이펙트를 켜거나 잔상을 남기고 싶다면 여기에 코드를 넣으면 됩니다.
        yield return new WaitForSeconds(dashDuration);

        // -------------------------------------------------------------
        // [STEP 3] 지침 / 그로기 (Groggy)
        // -------------------------------------------------------------
        currentState = BossState.Groggy;
        Debug.Log($"🤢 [보스 패턴] {bossData.enemyName}이 돌진 후 지쳤습니다. (딜타임!)");
        
        // 지쳐서 맛이 간 느낌을 주기 위해 파란색이나 어두운색으로 변경
        sr.color = new Color(0.6f, 0.6f, 1f, 1f); 
        yield return new WaitForSeconds(groggyDuration);
        
        // 패턴 종료 후 추격 상태로 복귀
        sr.color = Color.white;
        currentState = BossState.Chase;
    }

    // 벽이나 플레이어와 부딪혔을 때 처리 (예: 돌진 중 벽에 박으면 즉시 그로기 유도 가능)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 만약 돌진 중에 "Wall" 레이어나 장애물에 쿵 박았다면?
        if (currentState == BossState.Dash && collision.gameObject.CompareTag("Wall"))
        {
            // 코루틴을 강제로 끊고 즉시 그로기 상태로 전환시켜 난이도를 완화할 수도 있습니다.
            Debug.Log("💥 보스가 벽에 꼬꾸라졌습니다!");
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // 💡 만약 그로기(지침) 상태일 때 보스가 대미지를 2倍로 받게 하고 싶다면?
        if (currentState == BossState.Groggy)
        {
            amount *= 2f;
            Debug.Log("🎯 그로기 상태의 보스에게 치명타 대미지가 들어갑니다!");
        }

        currentHp -= amount;
        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines(); // 작동 중인 돌진 루틴 강제 종료
        OnDestroyedEvent?.Invoke();
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        Destroy(gameObject);
    }
}