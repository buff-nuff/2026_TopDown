using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    private bool playerInRange = false;
    private GameObject playerObj; // 범위 내에 들어온 플레이어를 기억할 변수

    [Header("순간이동할 목표 오브젝트")]
    // 유니티 에디터에서 아래쪽 방 입구에 배치한 빈 오브젝트를 여기에 드래그해 넣습니다.
    [SerializeField] private Transform teleportTarget; 

    void Update()
    {
        // ⭐ 몬스터 체크 없이, 플레이어가 범위 안에서 E키를 누르면 무조건 순간이동합니다!
        if (playerInRange && playerObj != null && Input.GetKeyDown(KeyCode.E))
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        if (teleportTarget == null)
        {
            Debug.LogError("이동할 목표 오브젝트(Teleport Target)가 지정되지 않았습니다! 인스펙터를 확인하세요.");
            return;
        }

        Debug.Log($"[공간 이동] 플레이어를 '{teleportTarget.name}' 위치로 즉시 이동시킵니다.");
        
        // 플레이어의 현재 위치 좌표를 목표 오브젝트의 위치 좌표로 강제 이동
        playerObj.transform.position = teleportTarget.position;
    }

    // --- 범위 감지 트리거 (플레이어가 문 앞에 왔는지 확인) ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            playerObj = collision.gameObject; // 플레이어 오브젝트 원본 기억
            Debug.Log("[문 감지] 문 앞에 도착했습니다. 'E'키를 누르면 이동합니다.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            playerObj = null;
            Debug.Log("[문 감지] 문에서 멀어졌습니다.");
        }
    }
}