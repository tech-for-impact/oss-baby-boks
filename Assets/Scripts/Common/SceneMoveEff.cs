using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMoveEff : MonoBehaviour
{
    // Inspector에서 지정할 속도 (초당 이동 거리)
    public float moveSpeed = 5f;

    // 오브젝트 이동 여부 (여기서는 static으로 사용)
    private static bool isMovingObj = false;

    // 초기 위치 저장용 변수 (RectTransform의 anchoredPosition)
    private Vector2 initialAnchoredPosition;

    // RectTransform 컴포넌트 참조
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 시작 시 현재 anchoredPosition을 초기 위치로 저장
            initialAnchoredPosition = rectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogWarning("RectTransform 컴포넌트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if (rectTransform == null)
            return;

        if (isMovingObj)
        {
            // isMovingObj가 활성화되면 x축으로 moveSpeed 만큼 이동
            rectTransform.anchoredPosition += new Vector2(moveSpeed * Time.deltaTime, 0);
        }
        else
        {
            // isMovingObj가 비활성화되면 초기 위치로 복원
            rectTransform.anchoredPosition = initialAnchoredPosition;
        }
    }

    // 외부에서 이동 여부를 제어할 수 있도록 public 메서드 추가
    public static void SetIsMovingObj(bool value)
    {
        isMovingObj = value;
    }
}
