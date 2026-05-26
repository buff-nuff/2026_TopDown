using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("외형 데이터")]
    public Sprite characterSprite;                 
     

    [Header("능력치 데이터")]
    public string characterName;  
    public int maxHp;             
    public int currentHp;         
    public float moveSpeed;       
    public float attackDamage;    

    // ⭐️ [이 부분이 없어서 에러가 난 것입니다] ⭐️
    // 외부(플레이어 공격 등)에서 데미지를 줄 때 호출하는 메서드
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"{characterName}이(가) {damage}의 피해를 입었습니다. 남은 체력: {currentHp}");

        // 체력이 0 이하가 되면 사망 처리
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 사망 시 처리하는 메서드
    private void Die()
    {
        // 이 오브젝트에 EnemyController가 붙어있다면 (즉, 적이라면)
        if (TryGetComponent<EnemyController>(out var enemy))
        {
            enemy.BecomeCorpse(); // 적을 시체 상태로 만듭니다.
        }
        else
        {
            // 플레이어가 죽었을 때의 처리 (필요시 작성)
            Debug.Log("플레이어가 사망했습니다.");
        }
    }

    // 타겟의 스텟을 내 것으로 복사하는 메서드
    public void CopyFromTarget(CharacterStats enemyStats)
    {
        this.characterName = enemyStats.characterName;
        this.maxHp = enemyStats.maxHp;
        this.currentHp = enemyStats.maxHp; // 빙의할 땐 풀피로 회복되게 설정
        this.moveSpeed = enemyStats.moveSpeed;
        this.attackDamage = enemyStats.attackDamage;
        this.characterSprite = enemyStats.characterSprite;
       
    }
}