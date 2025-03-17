using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;          // 아이템 이름
    public Sprite itemSprite;        // 아이템에 사용할 스프라이트
    public int requiredStageIndex;   // 이 아이템을 언락하기 위한 연결 스테이지 인덱스
    public int requiredScore;        // 해당 스테이지에서 달성해야 하는 점수 (언락 조건)
}
