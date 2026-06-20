using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float destroyTime = 4f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    // ⭐ [변경] Trigger 대신 일반 Collision 충돌 함수를 사용합니다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 일반 충돌은 collision.gameObject로 접근해야 합니다.
        if (collision.gameObject.CompareTag("Player"))
        {
            OnlyPlayerController player = collision.gameObject.GetComponent<OnlyPlayerController>();
            
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}