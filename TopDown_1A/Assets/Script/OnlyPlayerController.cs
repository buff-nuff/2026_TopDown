using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; 
using UnityEngine.SceneManagement; 

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class OnlyPlayerController : MonoBehaviour
{
    [Header("플레이어 스텟 설정")]
    public float maxHp = 100f;
    private float currentHp;
    public float moveSpeed = 5f;

    [Header("💀 게임 오버 UI 설정")]
    [SerializeField] private GameObject gameOverPanel;

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

    [Header("✨ 변신 시스템 설정")]
    private float originalMoveSpeed;
    private Sprite humanBaseSprite;
    private bool isTransformed = false;
    private EnemyData currentBossData = null;
    private bool isDashing = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private Vector2 velocity;
    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    private Vector2 playerDirection = Vector2.down;
    [HideInInspector] public bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentHp = maxHp;

        if (spriteDown != null && spriteDown.Length > 0)
        {
            currentSprites = spriteDown;
            sr.sprite = currentSprites[0];
            humanBaseSprite = spriteDown[0];
        }
        originalMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleTransformation();
        }

        if (isDead) return;

        MatrixKeyInputCheck();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTransformed && currentBossData != null)
            {
                UseBossGimmick(currentBossData.name);
            }
            else
            {
                FireProjectile();
            }
        }

        velocity = input.normalized * moveSpeed;
        if (isTransformed) return;

        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            if (currentSprites != null && currentSprites.Length > 0) sr.sprite = currentSprites[frameIndex];
            return;
        }

        UpdateDirectionAndSprites();
        timer += Time.deltaTime;
        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex = (frameIndex + 1) % currentSprites.Length;
            sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isDashing) return;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null) return;
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        #if UNITY_2022_1_OR_NEWER
            projRb.linearVelocity = playerDirection * projectileSpeed;
        #else
            projRb.velocity = playerDirection * projectileSpeed;
        #endif
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHp -= amount;
        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        sr.color = new Color(1f, 1f, 1f, 0.5f);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    private void MatrixKeyInputCheck()
    {
        input = Vector2.zero;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x = 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y = -1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y = 1f;
    }

    private void UpdateDirectionAndSprites()
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            ChangeSprites(input.x > 0 ? spriteRight : spriteLeft);
            playerDirection = input.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            ChangeSprites(input.y > 0 ? spriteUp : spriteDown);
            playerDirection = input.y > 0 ? Vector2.up : Vector2.down;
        }
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        if (currentSprites == newSprites) return;
        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
    }

    private void UseBossGimmick(string bossName)
    {
        switch (bossName)
        {
            case "Boss1Data": Fire8Way(); break;
            case "Boss2Data": StartCoroutine(Boss2DashRoutine()); break;
        }
    }

    private void Fire8Way()
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody2D pRb = proj.GetComponent<Rigidbody2D>();
            #if UNITY_2022_1_OR_NEWER
                pRb.linearVelocity = dir * projectileSpeed;
            #else
                pRb.velocity = dir * projectileSpeed;
            #endif
        }
    }

    private IEnumerator Boss2DashRoutine()
    {
        isDashing = true;
        float dashSpeed = moveSpeed * 3f;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            #if UNITY_2022_1_OR_NEWER
                rb.linearVelocity = playerDirection * dashSpeed;
            #else
                rb.velocity = playerDirection * dashSpeed;
            #endif
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    private void ToggleTransformation()
    {
        if (isTransformed)
        {
            isTransformed = false;
            currentBossData = null;
            moveSpeed = originalMoveSpeed;
            sr.sprite = humanBaseSprite;
        }
        else
        {
            BossSoulData savedSoul = BossSaveManager.LoadBossSoul();
            if (savedSoul == null || string.IsNullOrEmpty(savedSoul.scriptableObjectName)) return;
            EnemyData loadedBossData = Resources.Load<EnemyData>($"BossData/{savedSoul.scriptableObjectName}");
            if (loadedBossData == null) return;
            isTransformed = true;
            currentBossData = loadedBossData;
            moveSpeed = loadedBossData.moveSpeed;
            sr.sprite = loadedBossData.enemySprite;
        }
    }
}