using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public float speed = 2.0f; // 기본 이동 속도
    public float rotationSpeed = 2.0f; // 회전 속도
    public float changeDirectionInterval = 3.0f; // 방향 전환 간격
    public float swimAreaRadius = 5.0f; // 물고기가 움직이는 영역 반경

    // S자 헤엄 효과 관련 변수
    public float swayFrequency = 2.0f; // S자 효과의 주파수
    public float swayAmplitude = 0.1f; // S자 효과의 진폭

    private Vector3 targetDirection; // 목표 이동 방향
    private Vector3 originPosition;  // 시작(원점) 위치
    private Vector3 basePosition;    // 기본 이동 경로를 누적할 변수 (sway 효과를 적용하기 전의 위치)
    private float timeSinceLastDirectionChange = 0.0f;

    void Start()
    {
        originPosition = transform.position;
        basePosition = transform.position;
        SetRandomDirection();
    }

    void Update()
    {
        // 일정 시간마다 방향 전환
        timeSinceLastDirectionChange += Time.deltaTime;
        if (timeSinceLastDirectionChange > changeDirectionInterval)
        {
            SetRandomDirection();
            timeSinceLastDirectionChange = 0.0f;
        }

        // 목표 방향으로 부드럽게 회전
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 기본 이동: 현재 전방 방향으로 이동한 후, basePosition에 누적
        basePosition += transform.forward * speed * Time.deltaTime;

        // S자 효과: sine 함수를 이용해 lateral(좌우) offset 계산
        float swayOffset = Mathf.Sin(Time.time * swayFrequency) * swayAmplitude;

        // 누적된 basePosition에 transform.right 방향으로 offset을 더해 S자 움직임 구현
        transform.position = basePosition + transform.right * swayOffset;

        // 기본 이동 위치를 기준으로 지정된 영역을 벗어나면 원점으로 되돌아가도록 방향 수정
        if (Vector3.Distance(basePosition, originPosition) > swimAreaRadius)
        {
            SetDirectionTowardsOrigin();
        }
    }

    // 랜덤한 방향 설정
    private void SetRandomDirection()
    {
        targetDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }

    // 원래 위치를 향하는 방향 설정 (basePosition 기준)
    private void SetDirectionTowardsOrigin()
    {
        targetDirection = (originPosition - basePosition).normalized;
    }

    // 영역 시각화를 위한 기즈모
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(originPosition, swimAreaRadius);
    }
}
