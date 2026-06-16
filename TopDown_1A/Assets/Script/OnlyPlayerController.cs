using UnityEngine;
// ⚡ 새로운 인풋 시스템 시스템을 사용하기 위한 필수 선언
using UnityEngine.InputSystem; 

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

    // 내부 컴포넌트 및 변수
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private Vector2 velocity;
    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    // 다른 스크립트(PlayerPossession 등) 연동용 방향 변수
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
        // 🕹️ [New Input System 방식 수동 키 감지]
        // 에디터 세팅과 충돌 없이 현재 활성화된 키보드의 WASD 및 방향키 값을 추출합니다.
        MatrixKeyInputCheck();

        // 대각선 이동 속도 보정 및 속도 대입
        velocity = input.normalized * moveSpeed;

        // 1. 키보드 입력이 없으면 (멈춰 서 있으면) 발걸음을 첫 프레임으로 고정하고 리턴
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
        // 물리 엔진 기반 이동 처리
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    // New Input System 환경에서 안전하게 WASD/방향키 값을 받아오는 함수
    private void MatrixKeyInputCheck()
    {
        input = Vector2.zero;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // X축 입력 (A, D / 좌, 우 화살표)
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x = 1f;

        // Y축 입력 (W, S / 위, 아래 화살표)
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
                ChangeSprites(spriteRight);
                playerDirection = Vector2.right;
            }
            else
            {
                ChangeSprites(spriteLeft);
                playerDirection = Vector2.left;
            }
        }
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
}