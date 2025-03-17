using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;          // ������ �̸�
    public Sprite itemSprite;        // �����ۿ� ����� ��������Ʈ
    public int requiredStageIndex;   // �� �������� ����ϱ� ���� ���� �������� �ε���
    public int requiredScore;        // �ش� ������������ �޼��ؾ� �ϴ� ���� (��� ����)
}
