using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FinalBossController : MonoBehaviour
{
    [Header("🎯 타겟 설정")]
    [SerializeField] private Transform playerTransform; // 플레이어 위치 (기본값 null이면 자동으로 찾음)

    [Header("📊 보스 기본 능력치")]
    [SerializeField] private float moveSpeed = 1.5f;     // 플레이어를 졸졸 따라가는 속도

    [Header("🌀 패턴 2: 이동형 장판 설정")]
    [SerializeField] private GameObject dangerZonePrefab; // 3번 보스 스타일의 장판 프리팹
    [SerializeField] private float zoneScale = 3f;        // 장판의 크기 (범위)

    [Header("🎡 패턴 2: 회전 탄막 설정")]
    [SerializeField] private GameObject projectilePrefab; // 1번 보스 스타일의 구체 프리팹
    [SerializeField] private float projectileSpeed = 5f;  // 구체 날아가는 속도
    [SerializeField] private float fireInterval = 0.2f;    // 탄막이 뿜어져 나오는 시간 간격
    [SerializeField] private float rotationSpeed = 45f;   // 탄막 바람개비가 회전하는 속도 (도/초)

    private Rigidbody2D rb;
    private GameObject activeDangerZone;
    private bool isPatternActive = false;
    private float currentAngle = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        // 씬에서 플레이어를 자동으로 찾습니다. (태그가 "Player"여야 합니다)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        // 보스가 생성되자마자 2번 복합 패턴 가동!
        StartBossPattern();
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        // 3번 보스 기믹의 진화: 플레이어 방향으로 느리게 슬금슬금 따라갑니다 (이동형 장판 효과)
        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    public void StartBossPattern()
    {
        if (isPatternActive) return;
        StartCoroutine(Pattern2Routine());
    }

    private IEnumerator Pattern2Routine()
    {
        isPatternActive = true;

        // 1. 내 발밑에 3번 보스 스타일의 장판 프리팹을 자식으로 생성
        if (dangerZonePrefab != null)
        {
            activeDangerZone = Instantiate(dangerZonePrefab, transform.position, Quaternion.identity);
            activeDangerZone.transform.SetParent(transform); // 보스를 따라 움직이도록 자식으로 설정
            activeDangerZone.transform.localScale = new Vector3(zoneScale, zoneScale, 1f);
        }

        Debug.Log("🌋 [최종 보스 2번 패턴 발동] 이동형 장판 + 회전 회오리 탄막!");

        // 2. 장판이 켜져 있는 동안 무한 루프로 1번 보스 스타일의 회전 탄막 유출
        while (isPatternActive)
        {
            // 한 번 발사할 때 양방향(마주 보는 방향)으로 발사하여 바람개비 형태로 연출
            FireCrossProjectile(currentAngle);
            FireCrossProjectile(currentAngle + 180f);
            
            // 더 촘촘하게 만들고 싶다면 90도, 270도 십자 모양도 추가 가능
            // FireCrossProjectile(currentAngle + 90f);
            // FireCrossProjectile(currentAngle + 270f);

            // 매 프레임 발사 각도를 누적하여 바람개비처럼 회전시킵니다.
            currentAngle += rotationSpeed * fireInterval;
            if (currentAngle >= 360f) currentAngle -= 360f;

            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void FireCrossProjectile(float angle)
    {
        if (projectilePrefab == null) return;

        // 각도를 방향 벡터로 변환 (삼각함수 사용)
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        // 투사체 생성 및 회전 세팅
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 투사체 날리기 (이전 버전 호환성 코드 적용)
        if (proj.TryGetComponent<Rigidbody2D>(out var projRb))
        {
#if UNITY_2022_1_OR_NEWER
            projRb.linearVelocity = dir * projectileSpeed;
#else
            projRb.velocity = dir * projectileSpeed;
#endif
        }
    }

    // 보스가 죽거나 페이즈가 넘어갈 때 패턴을 끄는 함수
    public void StopBossPattern()
    {
        isPatternActive = false;
        if (activeDangerZone != null)
        {
            Destroy(activeDangerZone);
        }
    }
}