using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMoveEff : MonoBehaviour
{
    // Inspector���� ������ �ӵ� (�ʴ� �̵� �Ÿ�)
    public float moveSpeed = 5f;

    // ������Ʈ �̵� ���� (���⼭�� static���� ���)
    private static bool isMovingObj = false;

    // �ʱ� ��ġ ����� ���� (RectTransform�� anchoredPosition)
    private Vector2 initialAnchoredPosition;

    // RectTransform ������Ʈ ����
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // ���� �� ���� anchoredPosition�� �ʱ� ��ġ�� ����
            initialAnchoredPosition = rectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogWarning("RectTransform ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    void Update()
    {
        if (rectTransform == null)
            return;

        if (isMovingObj)
        {
            // isMovingObj�� Ȱ��ȭ�Ǹ� x������ moveSpeed ��ŭ �̵�
            rectTransform.anchoredPosition += new Vector2(moveSpeed * Time.deltaTime, 0);
        }
        else
        {
            // isMovingObj�� ��Ȱ��ȭ�Ǹ� �ʱ� ��ġ�� ����
            rectTransform.anchoredPosition = initialAnchoredPosition;
        }
    }

    // �ܺο��� �̵� ���θ� ������ �� �ֵ��� public �޼��� �߰�
    public static void SetIsMovingObj(bool value)
    {
        isMovingObj = value;
    }
}
