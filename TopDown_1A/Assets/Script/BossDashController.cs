using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BossDashController : MonoBehaviour
{
    [Header("보스 스텟 데이터 파일")]
    [SerializeField] private EnemyData bossData;

    [Header("🏃 돌진(Dash) 세부 설정")]
    [SerializeField] private float dashSpeed = 15f;         // 돌진할 때의 속도 (수치로 직접 입력)
    [SerializeField] private float dashCooldown = 4f;       // 돌진 주기 (4초마다)
    [SerializeField] private float chargeDuration = 1.2f;    // 돌진 전 경고/대기 시간
    [SerializeField] private float dashDuration = 0.5f;      // 실제 돌진하는 시간

    // 보스 상태 관리용 열거형
    private enum State { Chase, Charge, Dash }
    private State currentState = State.Chase;

    private float currentHp;
    private float stateTimer = 0f;
    private Vector2 dashDirection;

    // 내부 컴포넌트 변수
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform playerTransform;
    private Vector2 currentVelocity;

    [Header("👻 보스 전용 세이브 설정")]
    [SerializeField] private GameObject bossSoulPrefab; // 죽을 때 남길 영혼 프리팹

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // 물리 수면 방지

        if (bossData != null)
        {
            currentHp = bossData.maxHp;
            sr.sprite = bossData.enemySprite;
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (playerTransform == null || bossData == null) return;

        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            // 1. 일반 추격 상태
            case State.Chase:
                Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position);

                // 스크립터블 오브젝트에 적힌 플레이어 인식 범위(chaseRadius) 체크
                if (toPlayer.sqrMagnitude <= bossData.chaseRadius * bossData.chaseRadius)
                {
                    Vector2 dir = toPlayer.normalized;
                    HandleSpriteFlip(dir);
                    currentVelocity = dir * bossData.moveSpeed;

                    // 쿨타임이 차면 돌진 준비(Charge) 상태로 전환
                    if (stateTimer >= dashCooldown)
                    {
                        currentState = State.Charge;
                        stateTimer = 0f;
                        // 기 모으기 시작할 때 플레이어가 있던 방향을 돌진 방향으로 고정 조준!
                        dashDirection = dir;
                    }
                }
                else
                {
                    currentVelocity = Vector2.zero; // 플레이어가 범위 밖에 있으면 대기
                }
                break;

            // 2. 돌진 전 기 모으는 상태 (예고 타임)
            case State.Charge:
                currentVelocity = Vector2.zero; // 제자리에 정지

                // 빨간색으로 깜빡거리며 경고 신호 주기
                sr.color = (Mathf.FloorToInt(stateTimer * 12f) % 2 == 0) ? Color.red : Color.white;

                if (stateTimer >= chargeDuration)
                {
                    currentState = State.Dash;
                    stateTimer = 0f;
                    sr.color = Color.white; // 색상 원상 복구
                }
                break;

            // 3. 실제 돌진 상태
            case State.Dash:
                // 조준해 둔 방향으로 빠른 속도 주입
                currentVelocity = dashDirection * dashSpeed;

                if (stateTimer >= dashDuration)
                {
                    currentState = State.Chase;
                    stateTimer = 0f;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        // 물리 연산(rb.MovePosition)을 이용하여 벽을 뚫지 못하게 이동 처리
        Vector2 nextPosition = rb.position + currentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    private void HandleSpriteFlip(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        if (dir.x > 0.01f) sr.flipX = false;
        else if (dir.x < -0.01f) sr.flipX = true;
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        if (bossSoulPrefab != null)
        {
            Instantiate(bossSoulPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}