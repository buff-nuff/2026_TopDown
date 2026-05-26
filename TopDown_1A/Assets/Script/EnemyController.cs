using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData; 

    private CharacterStats myStats;
    private SpriteRenderer mySpriteRenderer;
    //private Animator myAnimator;
    
    // ⭐️ [이 부분이 없어서 에러가 난 것입니다] ⭐️
    // 이 적이 죽었는지 살았는지 체크하는 플래그 변수입니다.
    [HideInInspector] public bool isDead = false; 

    void Awake()
    {
        myStats = GetComponent<CharacterStats>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        //myAnimator = GetComponent<Animator>();
        InitializeEnemy();
    }

    public void InitializeEnemy()
    {
        if (enemyData == null) return;
        myStats.characterName = enemyData.enemyName;
        myStats.maxHp = enemyData.maxHp;
        myStats.currentHp = enemyData.maxHp; 
        myStats.moveSpeed = enemyData.moveSpeed;
        myStats.attackDamage = enemyData.attackDamage;
        myStats.characterSprite = enemyData.enemySprite;
        //myStats.animatorCtrl = enemyData.animatorCtrl;
        mySpriteRenderer.sprite = enemyData.enemySprite;
    }

    // ⭐️ [이 메서드도 PlayerPossession과 연동을 위해 꼭 필요합니다] ⭐️
    // 체력이 0이 되었을 때 시체 상태로 만드는 함수
    public void BecomeCorpse()
    {
        if (isDead) return;
        isDead = true; // 여기서 true로 바뀌어야 Player가 E키로 빙의할 수 있습니다.

        Debug.Log($"{myStats.characterName}이(가) 쓰러졌습니다. 빙의 가능!");

        // 시체 표현 (색상을 어둡고 약간 투명하게)
        mySpriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    }
}