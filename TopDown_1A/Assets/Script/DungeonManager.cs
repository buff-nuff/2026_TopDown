using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    // 🌟 어디서나 쉽게 접근할 수 있도록 싱글톤 인스턴스를 만듭니다.
    public static DungeonManager Instance { get; private set; }

    [Header("🚪 최종 보스 방 문 오브젝트")]
    [SerializeField] private GameObject finalBossDoor;

    [Header("🎯 목표 보스 처치 수")]
    [SerializeField] private int totalBossesToDefeat = 3;

    private int defeatedBossCount = 0;

    private void Awake()
    {
        // 싱글톤 세팅 안전장치
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 🔒 게임이 시작되면 최종 보스 문을 자동으로 숨깁니다.
        if (finalBossDoor != null)
        {
            finalBossDoor.SetActive(false);
        }
        else
        {
            Debug.LogWarning("최종 보스 문(Final Boss Door)이 인스펙터에 등록되지 않았습니다!");
        }
    }

    // 💀 보스가 처치될 때마다 각각의 보스 스크립트에서 이 함수를 호출합니다.
    public void BossDefeated()
    {
        defeatedBossCount++;
        Debug.Log($"👹 보스 처치 확인! (현재 {defeatedBossCount} / {totalBossesToDefeat})");

        // 3마리의 보스를 모두 잡았다면 문을 드러냅니다.
        if (defeatedBossCount >= totalBossesToDefeat)
        {
            RevealFinalDoor();
        }
    }

    private void RevealFinalDoor()
    {
        if (finalBossDoor != null)
        {
            // 🔓 숨겨져 있던 문을 다시 활성화하여 나타나게 합니다.
            finalBossDoor.SetActive(true);
            Debug.Log("✨ 모든 보스가 처치되었습니다! 최종 보스 방 문이 나타났습니다!");

            // 여기에 카메라 흔들림이나 파티클 이펙트 코드를 추가하면 더욱 멋진 연출이 가능합니다.
        }
    }
}