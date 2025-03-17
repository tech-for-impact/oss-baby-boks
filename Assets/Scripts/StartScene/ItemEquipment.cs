using UnityEngine;

public class ItemEquipment : MonoBehaviour
{
    public GameObject[] equipmentObjects;

    private void Update()
    {

        for (int i = 0; i < GameManager.Instance.Item.Length; i++)
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
