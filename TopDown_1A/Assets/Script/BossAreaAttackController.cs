using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAreaAttackController : MonoBehaviour
{
    [Header("보스 설정 데이터 파일")]
    [SerializeField] private EnemyData bossData;

    [Header("🔮 8방향 사격 설정")]
    [SerializeField] private GameObject projectilePrefab; // 위에서 만든 Boss1Projectile이 붙은 구체 프리팹
    [SerializeField] private float projectileSpeed = 6f;   // 구체 비행 속도
    [SerializeField] private float attackCooldown = 3f;     // 사격 주기 (3초마다)

    private float cooldownTimer = 0f;
    private Transform playerTransform;
    private Rigidbody2D rb;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (playerTransform == null || bossData == null) return;

        // 1. 플레이어와 보스 사이의 거리를 계산합니다.
        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;

        // 2. 플레이어가 보스의 인식 범위(chaseRadius) 안에 들어왔을 때만 작동합니다.
        if (toPlayer.sqrMagnitude <= bossData.chaseRadius * bossData.chaseRadius)
        {
            // 범위 안에 들어왔을 때만 공격 쿨타임을 계산합니다.
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0f;
                Fire8Directions(); // 8방향 발사 실행
            }
        }
        else
        {
            // 플레이어가 범위 밖으로 나가면 쿨타임을 초기화하거나 멈춥니다.
            // (선택 사항: 보스를 보자마자 바로 쏘게 하고 싶다면 0이 아닌 attackCooldown 값으로 세팅할 수도 있습니다.)
            cooldownTimer = 0f;
        }
    }

    // 🎯 [8방향 구체 생성 함수]
    private void Fire8Directions()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("보스 1의 구체 프리팹(Projectile Prefab)이 등록되지 않았습니다!");
            return;
        }

        Debug.Log("🔮 [보스 1] 8방향 광역 사격을 개시합니다!");

        // 0도부터 360도까지 45도씩 더해가며 총 8개의 구체를 생성합니다.
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;

            // 삼각함수 계산을 위해 도(Degree)를 라디안(Radian)으로 변환하여 방향 벡터를 구합니다.
            float radian = angle * Mathf.Deg2Rad;
            Vector2 launchDirection = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;

            // 보스 위치에 구체를 생성합니다.
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // 구체의 회전값을 계산된 각도로 정렬합니다.
            proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // 구체의 Rigidbody2D 속도에 방향과 속력을 대입합니다.
            if (proj.TryGetComponent<Rigidbody2D>(out var projRb))
            {
                projRb.gravityScale = 0f;
#if UNITY_2022_1_OR_NEWER
                projRb.linearVelocity = launchDirection * projectileSpeed;
#else
                projRb.velocity = launchDirection * projectileSpeed;
#endif
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (bossData == null) return;

        // 보스 체력 차감 로직 (플레이어 공격을 받을 때 호출)
        // 본 예시에서는 축약형으로 작성되었으며 필요 시 currentHp 변수를 추가하여 제어 가능합니다.
    }

   
}