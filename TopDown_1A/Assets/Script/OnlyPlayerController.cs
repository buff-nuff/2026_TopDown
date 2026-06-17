using UnityEngine;
using UnityEngine.InputSystem; // ⚡ 새로운 인풋 시스템 필수 선언

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent] // 플레이어 전용 중복 차단
public class OnlyPlayerController : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("방향별 발걸음 스프라이트 배열")]
    public Sprite[] spriteUp;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;

    [Header("애니메이션 속도 (프레임 시간)")]
    public float frameTime = 0.15f;

    [Header("🔮 구체 발사 설정")]
    [SerializeField] private GameObject projectilePrefab; // 생성할 구체 프리팹
    [SerializeField] private float projectileSpeed = 10f;  // 구체가 날아갈 속도

    // 내부 컴포넌트 및 변수
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private Vector2 velocity;
    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    // 실시간으로 플레이어가 바라보는 방향 (기본값은 아래쪽)
    [HideInInspector] public Vector2 playerDirection = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // 2D 탑다운 기본 물리 세팅
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 시작 시 기본 아래 보기 세팅
        if (spriteDown != null && spriteDown.Length > 0)
        {
            currentSprites = spriteDown;
            sr.sprite = currentSprites[0];
        }
    }

    private void Update()
    {
        // 🕹️ 실시간 키보드 WASD 및 방향키 감지
        MatrixKeyInputCheck();

        // 🕹️ ⭐ [Space 키 입력 실시간 감지]
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            FireProjectile();
        }

        // 대각선 속도 보정
        velocity = input.normalized * moveSpeed;

        // 1. 키보드 입력이 없으면 발걸음 고정 후 리턴
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            if (currentSprites != null && currentSprites.Length > 0)
            {
                sr.sprite = currentSprites[frameIndex];
            }
            return;
        }

        // 2. 입력이 있을 때 실시간으로 바라보는 방향 및 스프라이트 변경
        UpdateDirectionAndSprites();

        // 3. 타이머를 통한 스프라이트 발걸음 애니메이션 재생
        if (currentSprites == null || currentSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex++;

            if (frameIndex >= currentSprites.Length)
            {
                frameIndex = 0;
            }

            sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    // ⭐ [구체 실제 발사 로직]
    private void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("PlayerProjectile 프리팹이 등록되지 않았습니다!");
            return;
        }

        // 1. 플레이어 위치에 구체 생성
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // 2. ⭐ [회전값 보정] 바라보는 방향(playerDirection)을 각도로 변환하여 구체를 회전시킵니다.
        // 이 처리를 해주면 구체 이미지가 날아가는 방향을 똑바로 바라보게 됩니다.
        float angle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 3. 물리 속도 대입 (중력이 0인 상태에서 이 속도로만 직선 비행합니다)
        if (projectile.TryGetComponent<Rigidbody2D>(out var projRb))
        {
            // 유니티 버전에 따라 linearVelocity 대신 velocity를 사용해야 할 수 있습니다.
            projRb.linearVelocity = playerDirection * projectileSpeed;
        }
    }

    // New Input System 환경 키보드 값 추출 함수
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

    // 방향 및 스프라이트 세트 변경 연산
    private void UpdateDirectionAndSprites()
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            if (input.x < 0)
            {
                ChangeSprites(spriteLeft);
                playerDirection = Vector2.left; // 오른쪽 조준
            }
            else
            {
                ChangeSprites(spriteRight);
                playerDirection = Vector2.right;  // 왼쪽 조준
            }
        }
        else
        {
            if (input.y > 0)
            {
                ChangeSprites(spriteUp);
                playerDirection = Vector2.up;    // 위쪽 조준
            }
            else
            {
                ChangeSprites(spriteDown);
                playerDirection = Vector2.down;  // 아래쪽 조준
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
}