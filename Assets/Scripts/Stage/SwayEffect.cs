using UnityEngine;

public class SwayEffect : MonoBehaviour
{
    // 물고기가 헤엄치는 속도 (시간에 곱해질 값)
    private float swimSpeed = -10f;
    // 좌우 이동 범위 (물고기의 몸체가 좌우로 얼마만큼 이동할지)
    public float lateralAmplitude = 0.1f;
    // 좌우 회전 범위 (물고기가 좌우로 기울어지는 최대 각도)
    public float rotationAmplitude = 15f;
    // 필요에 따라 z축(전후) 이동 범위 추가 (미세한 오프셋 효과)
    public float forwardAmplitude = 0.03f;
    // forwardAmplitude의 적용 속도 (옵션)
    public float forwardSpeedMultiplier = 0.5f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // 시작 시 로컬 위치와 회전을 저장
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        // 시간에 swimSpeed를 곱해서 t값 계산
        float t = Time.time * swimSpeed;
        // 좌우 이동: sine 함수로 x축 오프셋 계산
        float xOffset = Mathf.Sin(t) * lateralAmplitude;
        // 좌우 회전: cosine 함수로 회전 각도 계산 (sine와 90도 위상차)
        float rotationY = Mathf.Cos(t) * rotationAmplitude;
        // 선택적: 전후 이동(미세한 오프셋)
        float zOffset = Mathf.Sin(t * forwardSpeedMultiplier) * forwardAmplitude;

        // 초기 위치에서 x와 z축 오프셋을 적용하여 S자 모양의 이동 구현
        transform.localPosition = initialPosition + new Vector3(xOffset, 0, zOffset);
        // 초기 회전에서 y축 회전을 추가 (물고기가 좌우로 기울어짐)
        transform.localRotation = initialRotation * Quaternion.Euler(0, rotationY, 0);
    }
}