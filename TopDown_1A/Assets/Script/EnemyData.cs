using UnityEngine;

// 에디터 우클릭 메뉴 [Create -> RogueLike -> Enemy Data]를 통해 데이터 파일을 만들 수 있게 합니다.
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "RogueLike/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보 입력")]
    public string enemyName;              // 적 유령의 이름

    [Header("비주얼 에셋 입력")]
    public Sprite enemySprite;            // 유령 도트 스프라이트 (2번/4번 이미지)
   

    [Header("전투 능력치 입력")]
    public int maxHp;                     // 최대 체력
    public float moveSpeed;               // 이동 속도
    public float attackDamage;            // 공격력
}