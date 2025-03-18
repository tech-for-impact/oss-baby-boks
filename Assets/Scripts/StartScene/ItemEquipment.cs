using UnityEngine;

public class ItemEquipment : MonoBehaviour
{
    public GameObject[] equipmentObjects;

    private void Update()
    {
        int minLength = Mathf.Min(GameManager.Instance.Item.Length, equipmentObjects.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (GameManager.Instance.Item[i])
            {
                equipmentObjects[i].SetActive(true);
            }
            else
            {
                equipmentObjects[i].SetActive(false);
            }
        }
    }
}
