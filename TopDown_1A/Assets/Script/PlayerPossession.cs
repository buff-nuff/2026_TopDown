using UnityEngine;

public class PlayerPossession : MonoBehaviour
{
    // --- 싱글톤 (메모리 최적화) ---
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
    public float attackRadius = 1.5f;     

    [Header("순수 스프라이트 공격 이펙트")]
    public GameObject attackEffectPrefab; 
    public Transform attackPoint;         

    // 가비지 컬렉터(GC) 방지를 위해 가변 배열 저장용 버퍼 미리 할당 (최대 10마리 감지)
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

    // [최적화] F키 공격 및 이펙트 처리
    private void Attack()
    {
        Vector3 spawnPosition = attackPoint != null ? attackPoint.position : transform.position;
        
        // 이펙트 생성 및 방향 물리 반전
        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, spawnPosition, transform.rotation);
            if (playerSpriteRenderer.flipX)
            {
                Vector3 localScale = effect.transform.localScale;
                localScale.x *= -1;
                effect.transform.localScale = localScale;
            }
        }

        // [최적화] NonAlloc 방식을 사용하여 매 프레임 발생하는 가비지(GC) 낭비 제거
        int hitCount = Physics2D.OverlapCircleNonAlloc(spawnPosition, attackRadius, scanBuffer, enemyLayer);
        
        for (int i = 0; i < hitCount; i++)
        {
            // GetComponent 중복 호출 최소화를 위해 안전하게 검사
            if (scanBuffer[i].TryGetComponent<EnemyController>(out var enemyCtrl))
            {
                if (!enemyCtrl.isDead && scanBuffer[i].TryGetComponent<CharacterStats>(out var enemyStats))
                {
                    enemyStats.TakeDamage((int)playerStats.attackDamage);
                }
            }
        }
    }

    // [최적化] E키를 눌렀을 때만 주변의 죽은 시체를 탐색하도록 흐름 수정 (매 프레임 연산 제거)
    private void TryPossessNearestCorpse()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, possessRadius, scanBuffer, enemyLayer);
        if (hitCount == 0) return;

        GameObject targetEnemyObj = null;
        CharacterStats targetEnemyStats = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            if (scanBuffer[i].TryGetComponent<EnemyController>(out var enemyCtrl) && enemyCtrl.isDead)
            {
                float distance = Vector2.Distance(transform.position, scanBuffer[i].transform.position);
                if (distance < shortestDistance)
                {
                    if (scanBuffer[i].TryGetComponent<CharacterStats>(out var enemyStats))
                    {
                        shortestDistance = distance;
                        targetEnemyObj = scanBuffer[i].gameObject;
                        targetEnemyStats = enemyStats;
                    }
                }
            }
        }

        // 조건에 맞는 가장 가까운 시체가 있다면 빙의 실행
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