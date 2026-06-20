using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HazardZone : MonoBehaviour
{
    [Header("장판 상세 스텟")]
    [SerializeField] private float warnDuration = 1.5f;     // 돌진 전 예고 시간 (장판 켜지기 전 안전 타임)
    [SerializeField] private float activeDuration = 5f;    // 장판이 켜져서 유지되는 시간
    [SerializeField] private float dpsDamage = 15f;         // 초당 대미지 (틱 대미지)
    [SerializeField] private float damageInterval = 0.5f;   // 대미지가 들어가는 주기 (0.5초마다 대미지 계산)

    private SpriteRenderer sr;
    private Collider2D zoneCollider;
    private bool isActivated = false;     // 장판 활성화 여부
    private bool isPlayerInside = false;  // 플레이어가 현재 장판을 밟고 있는지 여부
    private OnlyPlayerController playerController;
    private Coroutine damageRoutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        zoneCollider = GetComponent<Collider2D>();

        // ⚠️ 시작할 때는 공격 판정을 꺼둡니다 (예고 시간 동안은 밟아도 안전해야 하므로)
        zoneCollider.enabled = false;
        zoneCollider.isTrigger = true; // 무조건 트리거 체크
    }

    private void Start()
    {
        // 장판 시퀀스 스타트!
        StartCoroutine(HazardZoneRoutine());
    }

    // 🔄 예고 ➔ 활성화(지속딜) ➔ 소멸 시퀀스
    private IEnumerator HazardZoneRoutine()
    {
        // [STEP 1] 예고 상태 (바닥에 주황색 경고 표시)
        float elapsed = 0f;
        Color warnColor = new Color(1f, 0.6f, 0f, 0.4f); // 주황색 반투명
        
        while (elapsed < warnDuration)
        {
            // 기 모으는 느낌으로 알파값(투명도)을 깜빡이게 만듦
            sr.color = (Mathf.FloorToInt(elapsed * 10f) % 2 == 0) ? warnColor : new Color(1f, 0.6f, 0f, 0.1f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // [STEP 2] 활성화 상태 (새빨갛게 불타오르며 공격 판정 ON)
        isActivated = true;
        zoneCollider.enabled = true; // 이제부터 충돌 체크 시작!
        sr.color = new Color(1f, 0f, 0f, 0.6f); // 짙은 빨간색 반투명

        yield return new WaitForSeconds(activeDuration);

        // [STEP 3] 소멸 상태 (스르륵 사라짐)
        if (damageRoutine != null) StopCoroutine(damageRoutine);
        
        elapsed = 0f;
        Color startColor = sr.color;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            sr.color = Color.Lerp(startColor, new Color(1f, 0f, 0f, 0f), elapsed / 0.5f);
            yield return null;
        }

        Destroy(gameObject); // 완전히 파괴
    }

    // 플레이어가 장판을 밟았을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActivated) return;

        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerController = collision.GetComponent<OnlyPlayerController>();

            // 틱 대미지 코루틴 가동
            if (damageRoutine == null && playerController != null)
            {
                damageRoutine = StartCoroutine(TickDamageRoutine());
            }
        }
    }

    // 플레이어가 장판에서 발을 뗐을 때 (안전지대로 탈출)
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (damageRoutine != null)
            {
                StopCoroutine(damageRoutine);
                damageRoutine = null;
            }
        }
    }

    // ⭐ [지속 틱 대미지 연산] 장판 위를 밟고 있는 동안 무한 반복
    private IEnumerator TickDamageRoutine()
    {
        while (isPlayerInside && playerController != null)
        {
            // 주기별 대미지 계산 (예: 초당 15 대미지인데 0.5초 주기면 한 번에 7.5씩 차감)
            float calculatedDamage = dpsDamage * damageInterval;
            playerController.TakeDamage(calculatedDamage);

            // 지정된 주기(0.5초)만큼 대기했다가 다음 딜 넣기
            yield return new WaitForSeconds(damageInterval);
        }
    }
}