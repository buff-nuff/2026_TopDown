using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("외형 설정")]
    public Sprite characterSprite;

    [Header("능력치 설정")]
    public string characterName;
    public float moveSpeed;
    public int maxHp;
    public int currentHp;
    public float attackDamage;




    public void CopyFromTarget(CharacterStats enemyStats)
    {
        this.characterName = enemyStats.characterName;
        this.characterSprite = enemyStats.characterSprite;
        this.moveSpeed = enemyStats.moveSpeed;
        this.maxHp = enemyStats.maxHp;
        this.currentHp = enemyStats.currentHp;
        this.attackDamage = enemyStats.attackDamage;
    }
}
