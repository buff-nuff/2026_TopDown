using UnityEngine;
using UnityEngine.Pool;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("프리팹 등록")]
    [SerializeField] private Projectile projectilePrefab;
    
    [Header("풀 사이즈 설정")]
    [SerializeField] private int defaultCapacity = 30;
    [SerializeField] private int maxPoolSize = 100;

    private IObjectPool<Projectile> projectilePool;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 유니티 2021 버전 이상 공식 내장 풀링 엔진 시스템 빌드
        projectilePool = new ObjectPool<Projectile>(
            CreateProjectile, 
            OnGetProjectile, 
            OnReleaseProjectile, 
            OnDestroyProjectile,
            collectionCheck: false, // 릴리즈 중복 체크 생략으로 속도 최적화
            defaultCapacity, 
            maxPoolSize
        );
    }

    private Projectile CreateProjectile()
    {
        Projectile proj = Instantiate(projectilePrefab, transform);
        proj.SetPool(projectilePool);
        return proj;
    }

    private void OnGetProjectile(Projectile proj) => proj.gameObject.SetActive(true);
    private void OnReleaseProjectile(Projectile proj) => proj.gameObject.SetActive(false);
    private void OnDestroyProjectile(Projectile proj) => Destroy(proj.gameObject);

    // 외부 발사 연산 인터페이스
    public void SpawnProjectile(Vector2 spawnPosition, Vector2 direction, float speed, int damage)
    {
        if (projectilePrefab == null) return;
        Projectile proj = projectilePool.Get();
        proj.transform.position = spawnPosition;
        proj.Setup(direction, speed, damage);
    }
}