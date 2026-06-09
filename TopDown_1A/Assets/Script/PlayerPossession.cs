using UnityEngine;

public class PlayerPossession : MonoBehaviour
{
    // --- 싱글톤 ---
    private static PlayerPossession _instance;
    public static PlayerPossession Instance 
    { 
        get 
        { 
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PlayerPossession>();
            }
            return _instance; 
        } 
    }

    [HideInInspector] public CharacterStats playerStats;
    private SpriteRenderer playerSpriteRenderer;

    [Header("전투 및 빙의 설정")]
    public LayerMask enemyLayer;          
    public float possessRadius = 2f;      
    public float attackRadius = 10f;     

    [Header("순수 스프라이트 공격 이펙트 (구체 프리팹)")]
    public GameObject attackEffectPrefab; 
    public Transform attackPoint;         

    [Header("구체 발사 스펙")]
    public float projectileSpeed = 8f;

    // ⭐ [추가] 캐릭터가 마지막으로 바라본 상하좌우 방향을 기억하는 변수 (기본값: 오른쪽)
    private Vector2 lastLookDirection = Vector2.right;

    private readonly Collider2D[] scanBuffer = new Collider2D[10];

    private void Awake() 
    { 
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }

        playerStats = GetComponent<CharacterStats>(); 
        playerSpriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    void Update()
    {
        // ⭐ [방향 기록 핵심] 매 프레임 플레이어의 이동 입력을 감지해 '바라보는 정면 방향'을 실시간 업데이트합니다.
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        
        // 아무 키나 누르고 있을 때만 그 방향을 정면으로 기억 (멈췄을 때 0으로 초기화되는 것 방지)
        if (moveX != 0 || moveY != 0)
        {
            lastLookDirection = new Vector2(moveX, moveY).normalized;
        }

        // 1. 키 입력 처리 (공격)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Attack();
        }

        // 2. 키 입력 처리 (빙의)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPossessNearestCorpse();
        }
    }

    private void Attack()
    {
        if (attackEffectPrefab == null) return;

        Vector3 spawnPosition = attackPoint != null ? attackPoint.position : transform.position;

        // 1. 범위 내 적 수집 및 타겟팅
        int hitCount = Physics2D.OverlapCircleNonAlloc(spawnPosition, attackRadius, scanBuffer, enemyLayer);
        
        Transform closestEnemy = null;
        float shortestDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            if (scanBuffer[i] == null) continue;

            if (scanBuffer[i].TryGetComponent<EnemyController>(out var enemyCtrl))
            {
                if (enemyCtrl.isDead) continue;

                float distanceSqr = (scanBuffer[i].transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < shortestDistanceSqr)
                {
                    shortestDistanceSqr = distanceSqr;
                    closestEnemy = scanBuffer[i].transform;
                }
            }
        }

        // ⭐ [버그 해결] 타겟이 없을 때, 실시간으로 기억해 둔 마지막 정면 방향(lastLookDirection)을 그대로 사용합니다.
        Vector2 defaultDirection = lastLookDirection;

        // 2. 구체 생성
        GameObject effect = Instantiate(attackEffectPrefab, spawnPosition, Quaternion.identity);
        
        // 이펙트 비주얼 좌우 반전 처리
        if (playerSpriteRenderer != null && playerSpriteRenderer.flipX)
        {
            Vector3 localScale = effect.transform.localScale;
            localScale.x *= -1;
            effect.transform.localScale = localScale;
        }

        // 3. 구체에 정보 주입 (적 발견 시 유도탄, 없을 시 상하좌우 정면 비행)
        if (effect.TryGetComponent<Projectile>(out var proj))
        {
            int damage = playerStats != null ? (int)playerStats.attackDamage : 15;
            proj.Setup(closestEnemy, defaultDirection, projectileSpeed, damage);
        }
    }

    private void TryPossessNearestCorpse()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, possessRadius, scanBuffer, enemyLayer);
        if (hitCount == 0) return;

        GameObject targetEnemyObj = null;
        CharacterStats targetEnemyStats = null;
        float shortestDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            if (scanBuffer[i] == null) continue;

            if (scanBuffer[i].TryGetComponent<EnemyController>(out var enemyCtrl) && enemyCtrl.isDead)
            {
                float distanceSqr = (scanBuffer[i].transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < shortestDistanceSqr)
                {
                    if (scanBuffer[i].transform.TryGetComponent<CharacterStats>(out var enemyStats))
                    {
                        shortestDistanceSqr = distanceSqr;
                        targetEnemyObj = scanBuffer[i].gameObject;
                        targetEnemyStats = enemyStats;
                    }
                }
            }
        }

        if (targetEnemyObj != null && targetEnemyStats != null)
        {
            ExecutePossession(targetEnemyStats, targetEnemyObj);
        }
    }

    private void ExecutePossession(CharacterStats enemyStats, GameObject enemyObject)
    {
        playerStats.CopyFromTarget(enemyStats);
        playerSpriteRenderer.sprite = playerStats.characterSprite;
        playerSpriteRenderer.color = Color.white; 

        // 빙의 성공 시 바라보는 방향 초기화 변수 방지 차원 유지
        lastLookDirection = Vector2.right;

        Destroy(enemyObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, possessRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}