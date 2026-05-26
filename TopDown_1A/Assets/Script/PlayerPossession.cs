using UnityEngine;

public class PlayerPossession : MonoBehaviour
{
    // --- 싱글톤 (Singleton) 구현부 ---
    private static PlayerPossession _instance;
    public static PlayerPossession Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PlayerPossession>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }

        playerStats = GetComponent<CharacterStats>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
    }
    // ----------------------------------

    [HideInInspector] public CharacterStats playerStats;
    private SpriteRenderer playerSpriteRenderer;
    private Animator playerAnimator;

    [Header("빙의 범위 설정")]
    public LayerMask enemyLayer;          // 적 레이어 (Enemy)
    public float possessRadius = 2f;      // F키로 감지할 주변 반경 (반지름)

    void Update()
    {
        // 1. 매 프레임마다 주변에 빙의 가능한 적이 있는지 체크
        Collider2D nearestEnemy = GetNearestEnemy();

        if (nearestEnemy != null)
        {
            // [팁] 여기에 "F를 눌러 빙의" 같은 UI 텍스트를 띄우는 코드를 넣으면 좋습니다.
            // UIManager.Instance.ShowInteractionPrompt(true);

            // 2. 적이 범위 내에 있고, F 키를 눌렀을 때 빙의 실행
            if (Input.GetKeyDown(KeyCode.F))
            {
                CharacterStats enemyStats = nearestEnemy.GetComponent<CharacterStats>();
                if (enemyStats != null)
                {
                    ExecutePossession(enemyStats, nearestEnemy.gameObject);
                }
            }
        }
        else
        {
            // 주변에 적이 없으면 안내 UI를 끕니다.
            // UIManager.Instance.ShowInteractionPrompt(false);
        }
    }

    // 플레이어 주변에서 가장 가까운 적을 찾아 반환하는 메서드
    private Collider2D GetNearestEnemy()
    {
        // 플레이어 위치 중심으로 무형의 원을 그려 범위 내의 모든 적 컬라이더를 잡습니다.
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, possessRadius, enemyLayer);

        if (enemiesInRange.Length == 0) return null;

        Collider2D nearest = null;
        float shortestDistance = Mathf.Infinity;

        // 범위 내의 적들 중 가장 거리가 가까운 적을 선별합니다.
        foreach (Collider2D enemy in enemiesInRange)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void ExecutePossession(CharacterStats enemyStats, GameObject enemyObject)
    {
        Debug.Log($"[빙의 성공] F키를 눌러 {enemyStats.characterName}의 육체를 강탈했습니다.");

        // 스텟 및 외형 정보 복사
        playerStats.CopyFromTarget(enemyStats);

        // 비주얼 업데이트
        playerSpriteRenderer.sprite = playerStats.characterSprite;
       

        // 껍데기가 된 적 제거
        Destroy(enemyObject);
    }

    // 에디터 뷰에서 빙의 감지 범위를 시각적으로 확인하기 위한 기즈모 그리기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, possessRadius);
    }
}