 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneUpDown : MonoBehaviour
{
    public float speed = 1.0f;         // 파도 속도
    public float amplitude = 0.1f;     // 움직임 범위 (±0.1)
    public float phaseOffset = 0.1f;   // 자식 간 위상 차이

    private Vector3[] originalLocalPositions;  // 각 자식의 초기 localPosition 저장

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
            // 각 자식마다 phaseOffset을 곱해 위상 차이를 준다.
            float phase = i * phaseOffset;
            // sin 함수를 사용해 y축 이동량 계산
            float offsetY = Mathf.Sin(Time.time * speed + phase) * amplitude;
            // 초기 위치에 offsetY를 더해 새로운 localPosition 설정
            Vector3 newLocalPos = originalLocalPositions[i] + new Vector3(0, offsetY, 0);
            transform.GetChild(i).localPosition = newLocalPos;
        }
    }
}
