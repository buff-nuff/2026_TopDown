using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("몬스터 정보 및 스텟")]
    public string enemyName = "몬스터";
    public float maxHp = 100f;
    public float moveSpeed = 3f;
    public float chaseRadius = 6f;
    public int damage = 10;

    [Header("몬스터 원본 이미지 (단 1장)")]
    // 💡 좌우로 움직일 때 이 이미지를 기반으로 렌더러가 반전시킵니다.
    public Sprite enemySprite;
}