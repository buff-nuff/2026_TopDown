using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class OnlyPlayerController : MonoBehaviour
{
    [Header("플레이어 스텟 설정")]
    public float maxHp = 100f;          // 최대 체력
    private float currentHp;            // 현재 체력
    public float moveSpeed = 5f;        // 이동 속도

    [Header("방향별 발걸음 스프라이트 배열")]
    public Sprite[] spriteUp;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;

    [Header("애니메이션 속도")]
    public float frameTime = 0.15f;

    [Header("🔮 구체 발사 설정")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;

    // ⭐ [추가됨] 변신 시스템을 위한 변수들
    [Header("✨ 변신 시스템 설정")]
    private float originalMoveSpeed;    // 인간일 때의 원래 이동 속도
    private Sprite humanBaseSprite;     // 인간일 때의 기본 스프라이트
    private bool isTransformed = false; // 현재 변신 중인지 확인하는 플래그

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private Vector2 velocity;
    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    private Vector2 playerDirection = Vector2.down;

    [HideInInspector] public bool isDead = false; // 플레이어 사망 여부

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // ⭐ 게임 시작 시 체력을 가득 채워줍니다.
        currentHp = maxHp;

        if (spriteDown != null && spriteDown.Length > 0)
        {
            currentSprites = spriteDown;
            sr.sprite = currentSprites[0];

            // ⭐ [추가됨] 게임 시작 시 인간 형태의 기본값 백업
            humanBaseSprite = spriteDown[0];
        }
        // ⭐ [추가됨] 원래 속도 백업
        originalMoveSpeed = moveSpeed;

    }

    private void Update()
    {
        // ⭐ [추가됨] F키를 누르면 변신 토글 실행
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleTransformation();
        }
        // 사망 상태라면 키 입력이나 행동을 모두 차단합니다.
        if (isDead) return;

        MatrixKeyInputCheck();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            FireProjectile();
        }


        velocity = input.normalized * moveSpeed;

        // ⭐ [추가됨] 보스로 변신 중이라면 인간 형태의 발걸음 애니메이션을 계산하지 않고 마칩니다.
        if (isTransformed) return;

        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            if (currentSprites != null && currentSprites.Length > 0)
            {
                sr.sprite = currentSprites[frameIndex];
            }
            return;
        }

        UpdateDirectionAndSprites();

        if (currentSprites == null || currentSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex++;
            if (frameIndex >= currentSprites.Length) frameIndex = 0;
            sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        float angle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (projectile.TryGetComponent<Rigidbody2D>(out var projRb))
        {
#if UNITY_2022_1_OR_NEWER
            projRb.linearVelocity = playerDirection * projectileSpeed;
#else
            projRb.velocity = playerDirection * projectileSpeed;
#endif
        }
    }

    // ⭐ [핵심 추가] 보스의 공격이나 적에게 부딪혔을 때 플레이어의 체력을 깎는 함수
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHp -= amount;
        // 콘솔창에 플레이어가 대미지를 입었다고 명확하게 띄워줍니다.
        Debug.Log($"💥 [플레이어 피격] 대미지 {amount} 탑승! 현재 HP: {currentHp} / {maxHp}");

        // 체력이 0 이하가 되면 사망 처리
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // ⭐ [핵심 추가] 플레이어 사망 함수
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.LogError("💀 [게임 오버] 플레이어가 사망했습니다!");

        // 사망 시 리지드바디와 콜라이더를 멈추거나 꺼서 유령 상태로 만듭니다.
        rb.linearVelocity = Vector2.zero;
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;

        // 우선은 파괴하지 않고 투명도를 살짝 주거나 멈추게만 처리합니다. (필요시 Destroy(gameObject) 가능)
        sr.color = new Color(1f, 1f, 1f, 0.5f);
    }

    private void MatrixKeyInputCheck()
    {
        input = Vector2.zero;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y = -1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y = 1f;
    }

    private void UpdateDirectionAndSprites()
    {
        // 좌우 이동 비중이 클 때
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            if (input.x > 0)
            {
                ChangeSprites(spriteRight);
                playerDirection = Vector2.right;
                // ⭐ 오른쪽을 누르고 있지만 '왼쪽'으로 쏘도록 보정!
            }
            else
            {
                ChangeSprites(spriteLeft);
                playerDirection = Vector2.left;// ⭐ 왼쪽을 누르고 있지만 '오른쪽'으로 쏘도록 보정!
            }
        }
        // 상하 이동 비중이 클 때
        else
        {
            if (input.y > 0)
            {
                ChangeSprites(spriteUp);
                playerDirection = Vector2.up;
            }
            else
            {
                ChangeSprites(spriteDown);
                playerDirection = Vector2.down;
            }
        }
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        if (currentSprites == newSprites) return;
        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
        if (currentSprites != null && currentSprites.Length > 0)
        {
            sr.sprite = currentSprites[frameIndex];
        }
    }
    // ⭐ [추가됨] JSON 파일과 연동하여 보스로 변신하거나 해제하는 함수
    private void ToggleTransformation()
    {
        if (isTransformed)
        {
            // 1. 이미 변신 중이었다면 원래 인간으로 복구
            isTransformed = false;
            moveSpeed = originalMoveSpeed;
            sr.sprite = humanBaseSprite;
            Debug.Log("🧍 보스 변신을 해제하고 인간 형태로 돌아왔습니다.");
        }
        else
        {
            // 2. 인간 상태라면 저장된 JSON 보스 데이터를 로드
            BossSoulData savedSoul = BossSaveManager.LoadBossSoul();

            if (savedSoul == null || string.IsNullOrEmpty(savedSoul.scriptableObjectName))
            {
                Debug.LogWarning("❌ 변신 불가: 저장된 보스 영혼이 없습니다. (보스 시체에서 Q를 눌러 흡수하세요)");
                return;
            }

            // Resources/BossData 경로에서 스크립터블 오브젝트 로드
            EnemyData loadedBossData = Resources.Load<EnemyData>($"BossData/{savedSoul.scriptableObjectName}");

            if (loadedBossData == null)
            {
                Debug.LogError($"❌ 에셋 로드 실패: 'Assets/Resources/BossData/{savedSoul.scriptableObjectName}' 파일을 찾을 수 없습니다.");
                return;
            }

            // 보스의 능력치와 외형 덮어씌우기
            isTransformed = true;
            moveSpeed = loadedBossData.moveSpeed;
            sr.sprite = loadedBossData.enemySprite;

            Debug.Log($"👹 [변신 성공] 현재 보스: {loadedBossData.enemyName} (속도: {moveSpeed})");
        }
    }
}