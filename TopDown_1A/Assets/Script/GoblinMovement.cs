using UnityEngine;

public class GoblinMovement : MonoBehaviour
{
    [Header("데이터 연결")]
    public EnemyData enemyData; // 스크립터블 오브젝트 할당

    [Header("추적 설정")]
    public float detectionRadius = 5f;

    private int currentHp;
    private Transform playerTransform;
    private bool isChasing = false;
    private bool isDead = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    private Animator anim;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        // 데이터가 연결되어 있다면 초기 능력치 세팅
        if (enemyData != null)
        {
            currentHp = enemyData.maxHp;
            if (spriteRenderer != null) spriteRenderer.sprite = enemyData.enemySprite;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // 죽은 상태거나 플레이어가 없으면 AI 작동 중지
        if (isDead || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            isChasing = true;
            anim.SetBool("isRunning", true);
            Debug.Log("위치가 활성화 되었습니다.");
        }

        if (isChasing)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, enemyData.moveSpeed * Time.deltaTime);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (direction.x < 0);
        }
    }

    // [신규] 투사체에 맞았을 때 호출될 데미지 함수
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHp -= damageAmount;
        Debug.Log($"{enemyData.enemyName}이(가) {damageAmount}의 피해를 입음. 남은 HP: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // [신규] 죽었을 때 처리 함수
    void Die()
    {
        isDead = true;
        isChasing = false;

        // 1. 더 이상 투사체나 플레이어와 부딪히지 않게 컬라이더 끄기
        if (enemyCollider != null) enemyCollider.enabled = false;

        // 2. 이미지를 죽은 시체 이미지로 교체
        if (spriteRenderer != null && enemyData.deadSprite != null)
        {
            spriteRenderer.sprite = enemyData.deadSprite;
        }

        Debug.Log($"{enemyData.enemyName} 사망. 5초 뒤 시체가 사라집니다.");

        // 3. 5초 뒤에 이 오브젝트를 완전히 삭제
        Destroy(gameObject, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}