using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class BossAreaCreatorController : MonoBehaviour
{
    [Header("보스 스텟 데이터 파일")]
    [SerializeField] private EnemyData bossData;

    [Header("🔥 장판(Floor Hazard) 설정")]
    [SerializeField] private GameObject hazardZonePrefab; // 스폰할 장판 프리팹
    [SerializeField] private float spawnCooldown = 4f;       // 장판 생성 주기 (4초마다)
    [SerializeField] private bool spawnAtPlayerLocation = true; // true: 플레이어 발밑에 생성, false: 보스 주변 무작위 생성

    private float currentHp;
    private float spawnTimer = 0f;

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

        float distanceSqr = (playerTransform.position - transform.position).sqrMagnitude;
        float chaseRadiusSqr = bossData.chaseRadius * bossData.chaseRadius;

        // 플레이어 인식 범위 안에 있을 때만 추격 및 장판 생성
        if (distanceSqr <= chaseRadiusSqr)
        {
            moveDirection = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            HandleSpriteFlip();

            // ⏱️ 장판 소환 타이머 가동
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnCooldown)
            {
                spawnTimer = 0f;
                SpawnHazardZone(); // 장판 소환!
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

    private void HandleSpriteFlip()
    {
        if (moveDirection == Vector2.zero) return;
        if (moveDirection.x > 0.01f) sr.flipX = false;
        else if (moveDirection.x < -0.01f) sr.flipX = true;
    }

    // 💥 [장판 소환 함수]
    private void SpawnHazardZone()
    {
        if (hazardZonePrefab == null)
        {
            Debug.LogWarning("장판 프리팹이 등록되지 않았습니다!");
            return;
        }

        Vector3 spawnPosition = transform.position;

        if (spawnAtPlayerLocation && playerTransform != null)
        {
            // 🎯 플레이어의 현재 위치에 장판 생성 (저격형)
            spawnPosition = playerTransform.position;
            Debug.Log($"🔥 [보스 스킬] 플레이어의 발밑에 위험 장판을 생성합니다!");
        }
        else
        {
            // 🎲 보스 주변 무작위 반경에 장판 생성 (뿌리기형)
            spawnPosition += (Vector3)Random.insideUnitCircle * 3f;
            Debug.Log($"🔥 [보스 스킬] 보스 주변에 무작위 위험 장판을 생성합니다!");
        }

        // 장판 오브젝트 생성
        Instantiate(hazardZonePrefab, spawnPosition, Quaternion.identity);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHp -= amount;
        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDestroyedEvent?.Invoke();
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        Destroy(gameObject);
    }
}