using UnityEngine;

// 이 스크립트가 작동하려면 CharacterStats와 SpriteRenderer 컴포넌트가 필수적으로 필요함을 명시합니다.
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    [Header("이 적에게 적용할 기획 데이터")]
    public EnemyData enemyData; // 위에서 만든 데이터 에셋을 여기에 드래그 앤 드롭합니다.

    private CharacterStats myStats;
    private SpriteRenderer mySpriteRenderer;
    private Animator myAnimator;

    void Awake()
    {
        myStats = GetComponent<CharacterStats>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myAnimator = GetComponent<Animator>();

        // 게임이 시작될 때 입력한 데이터를 바탕으로 적의 정보를 초기화합니다.
        InitializeEnemy();
    }

    // [핵심] 입력된 데이터를 몬스터 컴포넌트에 주입하는 메서드
    public void InitializeEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError($"{gameObject.name}에 EnemyData가 입력되지 않았습니다!");
            return;
        }

        // 1. CharacterStats 컴포넌트에 스텟 입력
        myStats.characterName = enemyData.enemyName;
        myStats.maxHp = enemyData.maxHp;
        myStats.currentHp = enemyData.maxHp; // 처음 생성 시에는 풀피로 설정
        myStats.moveSpeed = enemyData.moveSpeed;
        myStats.attackDamage = enemyData.attackDamage;

        // 2. 비주얼(스프라이트 및 애니메이션) 주입
        myStats.characterSprite = enemyData.enemySprite;
        

        // 3. 현재 렌더러에 도트 이미지 반영
        mySpriteRenderer.sprite = enemyData.enemySprite;

       
    }

    // 런타임(게임 도중)에 코드로 직접 적의 스텟을 바꾸고 싶을 때 사용하는 메서드
    public void SetEnemyInfoDirectly(string name, int hp, float speed, float damage, Sprite sprite)
    {
        myStats.characterName = name;
        myStats.maxHp = hp;
        myStats.currentHp = hp;
        myStats.moveSpeed = speed;
        myStats.attackDamage = damage;

        myStats.characterSprite = sprite;
        mySpriteRenderer.sprite = sprite;
    }
}