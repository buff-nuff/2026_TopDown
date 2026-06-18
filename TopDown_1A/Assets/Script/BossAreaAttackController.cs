using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class BossAreaAttackController : MonoBehaviour
{
    [Header("보스 스텟 데이터 파일")]
    [SerializeField] private EnemyData bossData;

    [Header("⚔️ 광역 공격 설정")]
    [SerializeField] private GameObject bossProjectilePrefab; // 보스가 발사할 투사체 프리팹
    [SerializeField] private int projectileCount = 8;          // 한 번에 사방으로 발사할 구체 개수 (8방향, 12방향 등)
    [SerializeField] private float projectileSpeed = 5f;       // 보스 구체 속도
    [SerializeField] private float attackCooldown = 3f;        // 공격 주기 (3초마다 광역기)

    private float currentHp;
    private float attackTimer = 0f;

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
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (isDead || playerTransform == null || bossData == null) return;

        float distanceSqr = (playerTransform.position - transform.position).sqrMagnitude;
        float chaseRadiusSqr = bossData.chaseRadius * bossData.chaseRadius;

        // 1. 플레이어가 인식 범위 안에 들어왔을 때
        if (distanceSqr <= chaseRadiusSqr)
        {
            // 플레이어를 향해 이동 방향 계산
            moveDirection = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            HandleSpriteFlip();

            // ⭐ [광역 공격 타이머 가동]
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                ExecuteAreaAttack(); // 광역 공격 실행!
            }
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || bossData == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection * bossData.moveSpeed;
    }

    // 🔄 플레이어를 바라보는 좌우 반전 함수
    private void HandleSpriteFlip()
    {
        if (moveDirection == Vector2.zero) return;

        if (moveDirection.x > 0.01f) sr.flipX = false;
        else if (moveDirection.x < -0.01f) sr.flipX = true;
    }

    // 💥 [핵심 기믹: 360도 사방 광역 공격]
    private void ExecuteAreaAttack()
    {
        if (bossProjectilePrefab == null)
        {
            Debug.LogWarning("보스 투사체 프리팹이 등록되지 않았습니다!");
            return;
        }

        Debug.Log($"[보스 스킬] {bossData.enemyName}이 주변으로 광역 공격을 시전합니다!");

        // 지정된 개수(projectileCount)만큼 원형으로 각도를 쪼개서 투사체를 생성합니다.
        float angleStep = 360f / projectileCount;
        float angle = 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            // 1. 각도를 수학적 방향 벡터(X, Y)로 변환
            float projectileDirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180f);
            float projectileDirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180f);

            Vector2 spawnMoveDirection = new Vector2(projectileDirX, projectileDirY);
            Vector2 attackDirection = (spawnMoveDirection - (Vector2)transform.position).normalized;

            // 2. 보스 위치에 투사체 생성
            GameObject proj = Instantiate(bossProjectilePrefab, transform.position, Quaternion.identity);

            // 3. 투사체가 날아가는 방향에 맞춰 대가리(회전) 돌려주기
            float rotAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.AngleAxis(rotAngle, Vector3.forward);

            // 4. 투사체에 물리 속도 주입
            if (proj.TryGetComponent<Rigidbody2D>(out var projRb))
            {
                projRb.gravityScale = 0f; // 혹시 모를 중력 초기화
#if UNITY_2022_1_OR_NEWER
                projRb.linearVelocity = attackDirection * projectileSpeed;
#else
                projRb.velocity = attackDirection * projectileSpeed;
#endif
            }

            // 다음 구체가 날아갈 각도 계산 (예: 8방향이면 45도씩 더함)
            angle += angleStep;
        }
    }

    // 플레이어가 때렸을 때 대미지 받는 함수
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHp -= amount;
        Debug.Log($"[보스 피격] {bossData.enemyName} HP: {currentHp} / {bossData.maxHp}");

        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[보스 클리어] {bossData.enemyName}를 처치했습니다!");
        OnDestroyedEvent?.Invoke();

        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (bossData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, bossData.chaseRadius);
    }
}