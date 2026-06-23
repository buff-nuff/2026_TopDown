using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class OnlyEnemyController : MonoBehaviour
{
    [Header("몬스터 스텟 데이터 파일")]
    [SerializeField] private EnemyData enemyData;

    [Header("👻 보스 전용 세이브 설정")]
    [SerializeField] private GameObject bossSoulPrefab;

    [Header("⚔️ 몸싸움 대미지 설정")]
    [SerializeField] private float contactDamage = 15f; // 플레이어와 부딪혔을 때 줄 대미지 수치

    private float currentHp;

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

        if (enemyData != null)
        {
            currentHp = enemyData.maxHp;
            // ⭐ 스크립터블 오브젝트에 있는 단 1장의 이미지를 스프라이트 렌더러에 주입합니다.
            sr.sprite = enemyData.enemySprite;
        }
        else
        {
            Debug.LogError($"{gameObject.name}에 EnemyData가 등록되지 않았습니다!");
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
        if (isDead || playerTransform == null || enemyData == null) return;

        float distanceSqr = (playerTransform.position - transform.position).sqrMagnitude;
        float chaseRadiusSqr = enemyData.chaseRadius * enemyData.chaseRadius;

        if (distanceSqr <= chaseRadiusSqr)
        {
            moveDirection = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

            // ⭐ 매 프레임 이동 방향을 보고 이미지를 뒤집을지 말지 결정합니다.
            HandleSpriteFlip();
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || enemyData == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection * enemyData.moveSpeed;
    }

    // ⭐ [핵심 기믹] 이동 방향의 X축을 보고 렌더러를 좌우 반전 시키는 함수
    private void HandleSpriteFlip()
    {
        if (moveDirection == Vector2.zero) return;

        // 원본 이미지 방향에 맞춰 세팅하셔야 합니다. (기본 이미지가 오른쪽을 보고 있다고 가정할 때)
        if (moveDirection.x > 0.01f)
        {
            sr.flipX = false; // 오른쪽을 볼 때는 정방향
        }
        else if (moveDirection.x < -0.01f)
        {
            sr.flipX = true;  // 왼쪽을 볼 때는 이미지 반전!
        }
        // 위/아래로만 움직일 때는 기존의 flipX 상태를 그대로 유지하므로 어색하지 않습니다.
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 대상이 'Player' 태그를 가졌는지 확인합니다.
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어 컨트롤러 컴포넌트를 가져옵니다.
            OnlyPlayerController player = collision.gameObject.GetComponent<OnlyPlayerController>();

            if (player != null)
            {
                player.TakeDamage(contactDamage);
                Debug.Log($"💥 [몸싸움 피격] {gameObject.name}과 부딪혀 플레이어가 {contactDamage} 대미지를 입었습니다.");
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHp -= amount;

        if (currentHp <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDestroyedEvent?.Invoke();
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        if (bossSoulPrefab != null)
        {
            Instantiate(bossSoulPrefab, transform.position, Quaternion.identity);
        }

        // ⭐ [이 줄을 추가합니다] 던전 매니저에게 보스가 죽었다고 알립니다.
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.BossDefeated();
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.chaseRadius);
    }
}