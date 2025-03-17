using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InventoryItem : MonoBehaviour
{
    [Header("인벤토리 아이템 상태")]
    public bool isEquipped = false;
    public bool isUnlocked = false;

    [Header("연결 스테이지")]
    public int correspondingStage;

    [Header("잠김 안내 텍스트")]
    public TMP_Text lockedText; // 인스펙터에서 할당 (초기에는 비활성화 혹은 알파값 0)

    // 잠김 안내 텍스트 애니메이션 중인지 확인
    private bool isLockedTextAnimating = false;
    // 텍스트의 원래 위치 저장 (흔들림 효과를 위한 기준점)
    private Vector3 lockedTextOriginalPos;

    private void Start()
    {
        if (lockedText != null)
        {
            lockedTextOriginalPos = lockedText.transform.localPosition;
            // 초기 상태: 텍스트를 숨김
            lockedText.gameObject.SetActive(false);
        }

        // 만약 correspondingStage가 0이면 기본적으로 잠금 해제
        if (correspondingStage == 0)
        {
            UnlockItem();
        }
        else
        {
            // PlayerPrefs에서 해당 스테이지에서 10000점을 달성했는지 확인 (값이 1이면 달성)
            if (PlayerPrefs.GetInt($"StageAchieved10000_{correspondingStage}", 0) == 1)
            {
                // 자식 오브젝트들을 모두 비활성화하고 잠금 해제 처리
                foreach (Transform child in transform)
                {
                    UnlockItem();
                    child.gameObject.SetActive(false);
                }
                Debug.Log($"StageAchieved10000_{correspondingStage} 달성됨 - 자식 오브젝트 비활성화");
            }
        }
    }

    // 아이템의 잠금을 해제하는 메서드
    public void UnlockItem()
    {
        isUnlocked = true;
        UpdateUI();
    }

    // 아이템 장착/해제 토글
    public void ToggleEquip()
    {
        if (!isUnlocked)
        {
            Debug.Log("아이템이 잠겨있습니다");
            // 텍스트가 이미 애니메이션 중이면 무시
            if (!isLockedTextAnimating)
            {
                StartCoroutine(ShowLockedText());
            }
            return;
        }
        isEquipped = !isEquipped;
        UpdateUI();
    }

    private void UpdateUI()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            if (!isUnlocked)
            {
                img.color = Color.gray;
            }
            else
            {
                img.color = isEquipped ? Color.green : Color.white;
            }
        }
    }

    // 텍스트를 표시하면서 바로 흔들리고 서서히 사라지도록 하는 코루틴
    private IEnumerator ShowLockedText()
    {
        if (lockedText == null)
            yield break;

        isLockedTextAnimating = true;

        // 텍스트 설정: 내용, 활성화, 알파 1로 초기화
        lockedText.text = "아이템이 잠겨있습니다";
        lockedText.gameObject.SetActive(true);
        Color originalColor = lockedText.color;
        originalColor.a = 1f;
        lockedText.color = originalColor;
        lockedText.transform.localPosition = lockedTextOriginalPos;

        float fadeDuration = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            // 흔들림 효과: 원래 위치에서 약간의 랜덤 오프셋 적용 (x, y 각각 -2 ~ 2)
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);
            lockedText.transform.localPosition = lockedTextOriginalPos + randomOffset;

            // 알파값 서서히 감소
            float newAlpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            Color tempColor = lockedText.color;
            tempColor.a = newAlpha;
            lockedText.color = tempColor;

            yield return null;
        }

        // 최종적으로 텍스트 숨김 처리
        lockedText.gameObject.SetActive(false);
        lockedText.transform.localPosition = lockedTextOriginalPos;
        isLockedTextAnimating = false;
    }
}
