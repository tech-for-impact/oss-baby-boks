using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPrefs : MonoBehaviour
{
    public int stageCount = 5;

    public void InitializePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();

        for (int i = 0; i < stageCount; i++)
        {
            PlayerPrefs.SetInt($"StageUnlocked_{i}", i == 0 ? 1 : 0);
            PlayerPrefs.SetInt($"BestScore_{i}", 0);
            PlayerPrefs.SetInt($"TotalScore_{i}", 0);
        }

        PlayerPrefs.Save();

        Debug.Log("PlayerPrefs 초기화 완료");
    }
}