using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerProjectile : MonoBehaviour
{
    [Header("구체 설정")]
    public float damage = 25f;       // 이 구체가 줄 대미지 양
    public float destroyTime = 3f;   // 아무것도 안 맞았을 때 자동으로 사라질 시간

    private void Start()
    {
        // 화면 밖으로 무한히 날아가는 것을 방지하기 위해 일정 시간 뒤 자동 삭제
        Destroy(gameObject, destroyTime);
    }

    // ⭐ [핵심 충돌 로직] 구체의 Collider2D가 무언가와 부딪혔을 때 호출됩니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 부딪힌 대상의 태그가 "Enemy"인지 확인합니다.
        if (collision.CompareTag("Enemy"))
        {
            // 2. 부딪힌 적 오브젝트에서 우리가 만든 '뇌/실행자' 스크립트를 찾아옵니다.
            OnlyEnemyController enemy = collision.GetComponent<OnlyEnemyController>();

            // 3. 스크립트가 안전하게 존재한다면 원격으로 대미지 함수를 찔러줍니다!
            if (enemy != null)
            {
                Debug.Log($"[구체 명중] {collision.name}에게 {damage}의 대미지를 입힙니다.");
                enemy.TakeDamage(damage);
            }

            // 4. 적을 맞췄으므로 구체 자신은 씬에서 소멸합니다.
            Destroy(gameObject);
        }
        // (선택 사항) 벽이나 장애물에 부딪혔을 때도 구체가 사라지게 하고 싶다면 아래 주석을 푸세요.
        /*
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        */
    }
}