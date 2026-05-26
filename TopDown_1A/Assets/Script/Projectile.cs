using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 moveDirection;
    private float speed;
    private int damage;
    private bool isInitialized = false;

    public void Setup(Vector2 direction, float projectileSpeed, int projectileDamage)
    {
        moveDirection = direction.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        isInitialized = true;

        // 맵 밖으로 영원히 날아가는 것을 방지하기 위해 5초 뒤 자동 파괴
        Destroy(gameObject, 5f); 
    }
    // Update is called once per frame
    void Update()
    {
        if (!isInitialized) return;

        // 매 프레임 지정된 방향으로 이동
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트가 "Enemy" 태그를 가지고 있다면
        if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // 적에게 데미지 전달
            }

            // 적과 부딪힌 이펙트는 즉시 파괴 (관통 효과를 원하면 이 줄을 지우세요)
            Destroy(gameObject); 
        }
    }
}
