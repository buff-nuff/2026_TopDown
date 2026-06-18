using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossProjectile : MonoBehaviour
{
    [Header("보스 구체 설정")]
    public float damage = 10f;       // 플레이어에게 입힐 대미지 양
    public float destroyTime = 4f;   // 맞추지 못했을 때 자동 소멸 시간

    private void Start()
    {
        // 화면 밖으로 무한히 날아가는 것을 방지
        Destroy(gameObject, destroyTime);
    }

    // ⭐ [핵심 충돌 로직] 보스 구체가 플레이어와 부딪혔을 때 발동
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 부딪힌 대상의 태그가 "Player"인지 칼같이 확인합니다.
        if (collision.CompareTag("Player"))
        {
            // 2. 플레이어 오브젝트에서 대미지를 받을 수 있는 'OnlyPlayerController' 스크립트를 찾아옵니다.
            OnlyPlayerController player = collision.GetComponent<OnlyPlayerController>();

            // 3. 플레이어 스크립트가 성공적으로 존재한다면 대미지 함수를 실행합니다!
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            // 4. 플레이어를 맞췄으므로 보스 구체 자신은 씬에서 소멸합니다.
            Destroy(gameObject);
        }
    }
}