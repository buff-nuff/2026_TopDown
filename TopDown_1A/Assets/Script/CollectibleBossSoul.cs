using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class CollectibleBossSoul : MonoBehaviour
{
    [Header("이 시체의 주인인 보스 데이터(스크립터블 오브젝트)")]
    public EnemyData bossData; // 인스펙터에서 보스 데이터를 드래그해서 넣어주세요!

    private bool isPlayerNearby = false;

    private void Start()
    {
        // 충돌체가 트리거(통과 가능) 상태인지 확인
        if (TryGetComponent<Collider2D>(out var col)) col.isTrigger = true;
    }

    private void Update()
    {
        // 플레이어가 근처에 있고 Q키를 누르면 저장 실행!
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            SaveBossData();
        }
    }

    private void SaveBossData()
    {
        if (bossData == null)
        {
            Debug.LogError("⚠️ 보스 데이터가 연결되지 않았습니다! 인스펙터를 확인하세요.");
            return;
        }

        // 제이슨에 보스 데이터의 '이름'을 저장합니다.
        BossSoulData data = new BossSoulData();
        data.scriptableObjectName = bossData.name; 
        BossSaveManager.SaveBossSoul(data);

        Debug.Log($"✨ [{bossData.enemyName}] 보스의 영혼을 흡수했습니다!");
        Destroy(gameObject); // 흡수 후 시체 삭제
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = false;
    }
}