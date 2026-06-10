using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private float speed;
    private int damage;
    
    private Transform targetEnemy;
    private Vector2 lastDirection; 
    private bool hasTarget = false;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // ⚡ 물리 속도 간섭 버그를 막기 위해 Kinematic 설정 유지
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Setup(Transform target, Vector2 fallbackDirection, float projectileSpeed, int projectileDamage)
    {
        speed = projectileSpeed;
        damage = projectileDamage;
        
        if (target != null)
        {
            targetEnemy = target;
            hasTarget = true;
        }
        else
        {
            targetEnemy = null;
            hasTarget = false;
            
            // 플레이어가 채집해 준 상하좌우 순수 방향을 백업
            lastDirection = fallbackDirection.normalized;
            
            // 이미지 각도를 진행 방향에 맞게 즉시 회전
            float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        Destroy(gameObject, 5f);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 1. 적을 포착했을 때 ➔ 적의 위치를 향해 실시간 좌표 추적 이동
        if (hasTarget && targetEnemy != null && targetEnemy.gameObject.activeInHierarchy)
        {
            Vector2 currentPos = transform.position;
            Vector2 targetPos = targetEnemy.position;
            Vector2 direction = (targetPos - currentPos).normalized;
            
            // ⚡ [버그 해결 핵심 1] rb.linearVelocity를 쓰지 않고 MovePosition으로 직접 좌표 이동
            Vector2 nextPosition = currentPos + direction * (speed * Time.fixedDeltaTime);
            rb.MovePosition(nextPosition);

            // 구체가 적을 바라보도록 실시간 각도 업데이트
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            lastDirection = direction;
        }
        // 2. 적이 없을 때 ➔ 플레이어가 입력했던 상하좌우 순수 데이터 방향으로 직선 좌표 이동
        else
        {
            Vector2 currentPos = transform.position;
            
            // ⚡ [버그 해결 핵심 2] 오른쪽 고정 버그 유발 인자를 지우고, 오직 입력받은 방향 벡터(lastDirection)로만 좌표 이동
            Vector2 nextPosition = currentPos + lastDirection * (speed * Time.fixedDeltaTime);
            rb.MovePosition(nextPosition);
            
            // 회전 고정 보장
            float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<EnemyController>(out var enemyCtrl))
        {
            if (!enemyCtrl.isDead && collision.TryGetComponent<EnemyAI>(out var enemyAI))
            {
                enemyAI.TakeDamage(damage);
                Destroy(gameObject); 
            }
        }
    }
}