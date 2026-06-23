using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossProjectile : MonoBehaviour
{
    [Header("구체 공격 설정")]
    [SerializeField] private float damage = 10f;     // 플레이어에게 줄 대미지 수치
    [SerializeField] private float lifeTime = 5f;   // 구체가 화면에 머무는 시간

    private void Start()
    {
        // 벽이나 플레이어에 부딪히지 않아도 일정 시간 뒤에는 자동으로 소멸하도록 설정합니다.
        Destroy(gameObject, lifeTime);

        // 구체가 물리적으로 플레이어를 밀어내지 않고 통과하면서 대미지만 주도록 트리거를 활성화합니다.
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.isTrigger = true;
        }
    }

    // ⭐ [핵심 충돌 로직] 구체가 무언가와 겹쳤을 때 발동합니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 대상의 태그가 'Player'인지 검사합니다.
        if (collision.CompareTag("Player"))
        {
            // 수정된 플레이어 컨트롤러 컴포넌트를 가져옵니다.
            OnlyPlayerController player = collision.GetComponent<OnlyPlayerController>();

            if (player != null)
            {
                // 플레이어의 TakeDamage 함수를 호출하여 대미지를 입힙니다.
                player.TakeDamage(damage);
                Debug.Log($"🔮 [보스 1 피격 성공] 플레이어에게 {damage} 대미지를 입혔습니다.");
            }

            // 대미지를 준 직후 구체는 즉시 파괴됩니다.
            Destroy(gameObject);
        }
        // 만약 맵의 벽 태그가 'Wall'이라면 벽에 부딪혔을 때도 사라지게 만듭니다.
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}