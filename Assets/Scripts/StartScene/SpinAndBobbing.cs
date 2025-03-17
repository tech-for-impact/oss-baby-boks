using UnityEngine;

public class SpinAndBobbing : MonoBehaviour
{
    [Header("Spin 설정")]
    public bool enableSpin = true;              // 회전 효과 활성화 여부
    public float spinSpeed = 90f;               // 회전 속도 (초당 각도)
    public Vector3 spinAxis = Vector3.up;       // 회전 축

    [Header("Bobbing 설정")]
    public bool enableBobbing = true;           // 둥실거림 효과 활성화 여부
    public float verticalAmplitude = 0.2f;      // 상하 진폭
    public float verticalFrequency = 2f;        // 상하 주파수
    public float horizontalAmplitude = 0.1f;    // 좌우 진폭
    public float horizontalFrequency = 2f;      // 좌우 주파수

    private Vector3 initialLocalPosition;       // 자식의 초기 로컬 위치

    void Start()
    {
        // 부모의 이동에 영향을 받지 않도록 로컬 위치를 사용합니다.
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // 회전 효과 적용 (회전은 전역적으로 적용되어도 상관없음)
        if (enableSpin)
        {
            transform.Rotate(spinAxis, spinSpeed * Time.deltaTime);
        }

        // 둥실거림 효과 적용 (부모의 이동을 유지하기 위해 localPosition 사용)
        if (enableBobbing)
        {
            float offsetY = Mathf.Sin(Time.time * verticalFrequency) * verticalAmplitude;
            float offsetX = Mathf.Cos(Time.time * horizontalFrequency) * horizontalAmplitude;
            // 초기 localPosition을 기준으로 x, y 오프셋만 적용하고, z는 그대로 유지
            transform.localPosition = new Vector3(initialLocalPosition.x + offsetX,
                                                  initialLocalPosition.y + offsetY,
                                                  initialLocalPosition.z);
        }
    }
}
