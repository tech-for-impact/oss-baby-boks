using UnityEngine;

public class SpinAndBobbing : MonoBehaviour
{
    [Header("Spin ����")]
    public bool enableSpin = true;              // ȸ�� ȿ�� Ȱ��ȭ ����
    public float spinSpeed = 90f;               // ȸ�� �ӵ� (�ʴ� ����)
    public Vector3 spinAxis = Vector3.up;       // ȸ�� ��

    [Header("Bobbing ����")]
    public bool enableBobbing = true;           // �սǰŸ� ȿ�� Ȱ��ȭ ����
    public float verticalAmplitude = 0.2f;      // ���� ����
    public float verticalFrequency = 2f;        // ���� ���ļ�
    public float horizontalAmplitude = 0.1f;    // �¿� ����
    public float horizontalFrequency = 2f;      // �¿� ���ļ�

    private Vector3 initialLocalPosition;       // �ڽ��� �ʱ� ���� ��ġ

    void Start()
    {
        // �θ��� �̵��� ������ ���� �ʵ��� ���� ��ġ�� ����մϴ�.
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // ȸ�� ȿ�� ���� (ȸ���� ���������� ����Ǿ �������)
        if (enableSpin)
        {
            transform.Rotate(spinAxis, spinSpeed * Time.deltaTime);
        }

        // �սǰŸ� ȿ�� ���� (�θ��� �̵��� �����ϱ� ���� localPosition ���)
        if (enableBobbing)
        {
            float offsetY = Mathf.Sin(Time.time * verticalFrequency) * verticalAmplitude;
            float offsetX = Mathf.Cos(Time.time * horizontalFrequency) * horizontalAmplitude;
            // �ʱ� localPosition�� �������� x, y �����¸� �����ϰ�, z�� �״�� ����
            transform.localPosition = new Vector3(initialLocalPosition.x + offsetX,
                                                  initialLocalPosition.y + offsetY,
                                                  initialLocalPosition.z);
        }
    }
}
