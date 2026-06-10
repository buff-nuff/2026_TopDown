using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("구체 프리팹 직접 등록")]
    [SerializeField] private GameObject projectilePrefab; 

    [Header("발사 스펙")]
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private Transform firePoint;

    [Header("자동 타겟팅 최적화 설정")]
    [SerializeField] private float targetSearchRadius = 10f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int maxTargetDetection = 20; // 한 번에 탐색할 최대 적의 수

    // ⚡ [최적화 1] 가비지 컬렉션(GC) 렉 방지를 위한 메모리 고정 고간
    private Collider2D[] targetResults; 
    private SpriteRenderer playerSpriteRenderer;
    private PlayerController pC;

    void Awake()
    {
        // 배열을 미리 한 번만 할당해 두고 평생 재사용합니다. (메모리 쓰레기 0%)
        targetResults = new Collider2D[maxTargetDetection];
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        pC = GetComponent<PlayerController>();

        if (firePoint == null)
        {
            Transform foundChild = transform.Find("FirePoint");
            firePoint = foundChild != null ? foundChild : transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootTargeted();
        }
    }

    private void ShootTargeted()
    {
        if (projectilePrefab == null) return;

        // ⚡ [최적화 2] Physics2D.OverlapCircleNonAlloc 사용
        // OverlapCircleAll은 호출할 때마다 새로운 배열을 힙 메모리에 생성하여 순간적인 끊김(렉)을 유발합니다.
        // NonAlloc 방식을 쓰면 기존에 만들어 둔 targetResults 배열에 알맹이만 채워 넣으므로 렉이 전혀 없습니다.
        Collider2D[] allEnemyTargets = Physics2D.OverlapCircleAll(transform.position, targetSearchRadius, enemyLayer);
        
        Transform closestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity; // 제곱 거리를 비교할 것이므로 초기값은 무한대

        // ⚡ [최적화 3] Vector2.Distance 대치 ➔ sqrMagnitude 사용
        // 컴퓨터는 루트(√) 연산을 할 때 엄청난 연산 부하를 느낍니다. Distance 함수 내부에는 루트 연산이 들어있습니다.
        // 거리의 제곱 비중을 비교하는 sqrMagnitude를 사용하면 루트 연산이 완전히 생략되어 연산 속도가 수십 배 빨라집니다.
        foreach (Collider2D col in allEnemyTargets)
        {
            // 가끔 배열 구멍에 비어있는 값이 들어오는 것을 방지
            if (col == null) continue;

            // GetComponent 횟수를 줄이기 위해 Collider가 살아있는지 기본 검사 후 거리 비교
            float distanceSqr = (col.transform.position - transform.position).sqrMagnitude;
            
            if (distanceSqr < closestDistanceSqr)
            {
                // 진짜 적인지 컴포넌트 최종 확인
                if (col.TryGetComponent<EnemyAI>(out var enemy))
                {
                    closestDistanceSqr = distanceSqr;
                    closestEnemy = col.transform;
                }
            }
        }


        // 구체 생성 및 데이터 주입
        GameObject projGo = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        if (projGo.TryGetComponent<Projectile>(out var proj))
        {
            proj.Setup(closestEnemy, pC.playerDirection, projectileSpeed, attackDamage);
        }
    }

    // 개발 중 에디터 뷰에서 노란색 원으로 타겟팅 범위를 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetSearchRadius);
    }
}