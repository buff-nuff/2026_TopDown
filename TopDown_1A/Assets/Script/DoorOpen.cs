using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    private bool isUnLocked = false;
    private bool playerInRange = false;

    [Header("시작적 연출용")]
    [SerializeField] private Sprite opneDoorSprite;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //문이 해금되었고, 플레이어가 범위 내에 있을때 F키를 누르면 문이 열리도록 상호작용
        if (isUnLocked && playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            OpenDoor();
        }
    }

    public void UnlockDoor()
    {
        isUnLocked = true;
        Debug.Log("[문 해금] 모든 몬스터가 처치되었습니다! 이제 문과 상호작용할 수 있습니다.");

        // 시각적 연출 : 문 색상을 바꾸거나 열린 스프라이트로 교체해 유저에게 알림
        if (spriteRenderer != null && opneDoorSprite != null)
        {
            spriteRenderer.sprite = opneDoorSprite;
        }
    }

    private void OpenDoor()
    {
        Debug.Log("[문 작동] 다음 방으로 이동합니다.");
    }

    // --- 트리거 범위 감지 ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (!isUnLocked)
            {
                Debug.Log("[문 잠김] 주변의 몬스터를 먼저 처치해야 합니다.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

}
