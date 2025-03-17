 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneUpDown : MonoBehaviour
{
    public float speed = 1.0f;         // �ĵ� �ӵ�
    public float amplitude = 0.1f;     // ������ ���� (��0.1)
    public float phaseOffset = 0.1f;   // �ڽ� �� ���� ����

    private Vector3[] originalLocalPositions;  // �� �ڽ��� �ʱ� localPosition ����

    void Start()
    {
        int childCount = transform.childCount;
        originalLocalPositions = new Vector3[childCount];
        for (int i = 0; i < childCount; i++)
        {
            originalLocalPositions[i] = transform.GetChild(i).localPosition;
        }
    }

    void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            // �� �ڽĸ��� phaseOffset�� ���� ���� ���̸� �ش�.
            float phase = i * phaseOffset;
            // sin �Լ��� ����� y�� �̵��� ���
            float offsetY = Mathf.Sin(Time.time * speed + phase) * amplitude;
            // �ʱ� ��ġ�� offsetY�� ���� ���ο� localPosition ����
            Vector3 newLocalPos = originalLocalPositions[i] + new Vector3(0, offsetY, 0);
            transform.GetChild(i).localPosition = newLocalPos;
        }
    }
}
