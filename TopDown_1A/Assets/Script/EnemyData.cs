using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "RogueLike/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;

    [Header("비주얼 에셋")]
    public Sprite enemySprite;        // 살아있을 때 도트
    public Sprite deadSprite;         // 죽었을 때 (시체) 도트

    [Header("전투 능력치")]
    public int maxHp;
    public float moveSpeed;
    public float attackDamage;
}