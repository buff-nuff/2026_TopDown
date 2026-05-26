using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private int damage;
    
    private IObjectPool<Projectile> managedPool;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 물리 투사체 연산 최적화를 위한 설정 자동화
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void SetPool(IObjectPool<Projectile> pool)
    {
        managedPool = pool;
    }

    public void Setup(Vector2 direction, float projectileSpeed, int projectileDamage)
    {
        moveDirection = (Vector3)direction.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;

        // [최적화] 날아가는 방향으로 1회만 각도 계산 (Update 연산 제거)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnEnable()
    {
        // 켜지면 5초 뒤 자동 비활성화 타이머 작동
        Invoke(nameof(Deactivate), 5f);
    }

    private void OnDisable()
    {
        // 꺼질 때 타이머 및 물리 속도 완전 리셋 (잔상/가속 버그 방지)
        CancelInvoke(nameof(Deactivate));
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void FixedUpdate()
    {
        // Translate 대신 가볍고 정확한 물리 MovePosition 연산 사용
        if (rb != null)
        {
            rb.MovePosition(transform.position + moveDirection * (speed * Time.fixedDeltaTime));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // TryGetComponent로 태그 검사와 컴포넌트 추출을 동시에 처리 (연산량 절반 감소)
        if (collision.TryGetComponent<EnemyAI>(out var enemy))
        {
            enemy.TakeDamage(damage);
            Deactivate();
        }
    }

    private void Deactivate()
    {
        if (managedPool != null) managedPool.Release(this);
        else gameObject.SetActive(false);
    }
}